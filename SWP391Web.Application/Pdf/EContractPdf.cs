using Microsoft.Playwright;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.ValueObjects;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace SWP391Web.Application.Pdf
{
    public class EContractPdf
    {
        private static readonly Regex Placeholder = new(@"\{\{\s*([A-Za-z0-9\._]+)\s*(?:(?::\s*([^}]+))|\|\s*date\.format\s*['""]([^'""]+)['""])?\s*\}\}", RegexOptions.Compiled);
        public static AnchorBox FindAnchorBox(byte[] pdfBytes, string anchorText)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);
            int lastPgae = doc.NumberOfPages;
            var page = doc.GetPage(lastPgae);

            foreach (var word in page.GetWords())
            {
                if (word.Text.Contains(anchorText, StringComparison.Ordinal))
                {
                    var bbox = word.BoundingBox;
                    return new AnchorBox
                    {
                        Page = lastPgae,
                        Top = bbox.Top,
                        Bottom = bbox.Bottom,
                        Left = bbox.Left,
                        Right = bbox.Right
                    };
                }
            }

            throw new InvalidOperationException($"Cannot find anchor text '{anchorText}' in pdf.");
        }

        public static async Task<byte[]> RenderAsync(string html)
        {
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    Locale = "vi-VN",
                    ViewportSize = new ViewportSize { Width = 1240, Height = 1754 },
                    DeviceScaleFactor = 1.25f
                });

                var page = await context.NewPageAsync();

                await page.SetContentAsync(html, new PageSetContentOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

                var pdf = await page.PdfAsync(new PagePdfOptions
                {
                    Format = "A4",
                    PrintBackground = true,
                    PreferCSSPageSize = true,
                    Margin = new Margin
                    {
                        Top = "20mm",
                        Bottom = "15mm",
                        Left = "15mm",
                        Right = "15mm"
                    }
                });

                return pdf;
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist"))
            {
                await Task.Run(() => Program.Main(new[] { "install", "chromium" }));
                return await RenderAsync(html);
            }
        }

        public static string ReplacePlaceholders(string html, Object values, bool htmlEncode = false)
        {
            return Placeholder.Replace(html, m =>
            {
                var path = m.Groups[1].Value.Trim();
                var format = m.Groups[2].Success ? m.Groups[2].Value.Trim()
                           : m.Groups[3].Success ? m.Groups[3].Value.Trim()
                           : null;

                var value = ResolvePath(values, path);
                if (value == null)
                {
                    return string.Empty;
                }

                string text;
                if (value is IFormattable fo && !string.IsNullOrWhiteSpace(format))
                {
                    text = fo.ToString(format, CultureInfo.InvariantCulture) ?? string.Empty;
                }
                else
                {
                    text = value?.ToString() ?? string.Empty;
                }

                return htmlEncode ? WebUtility.HtmlEncode(text) : text;
            });
        }

        private static object? ResolvePath(object root, string path)
        {
            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            object? current = root;

            foreach (var segment in segments)
            {
                if (current == null) return null;
                if (current is IDictionary<string, object?> dict)
                {
                    if (!dict.TryGetValue(segment, out current))
                    {
                        var joined = string.Join(".", segments);
                        return dict.TryGetValue(joined, out var val) ? val : null;
                    }
                    continue;
                }
                var type = current?.GetType();

                var prop = type?.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    current = prop.GetValue(current);
                    continue;
                }

                var field = type?.GetField(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }

                return null;
            }

            return current;
        }

        public static Dictionary<string, AnchorBox> FindAnchors(byte[] pdfBytes, params string[] anchors)
        {
            var result = new Dictionary<string, AnchorBox>(StringComparer.Ordinal);
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);

            var remaining = new HashSet<string>(anchors, StringComparer.Ordinal);

            for (int pageNumber = 1; pageNumber <= doc.NumberOfPages && remaining.Count > 0; pageNumber++)
            {
                var page = doc.GetPage(pageNumber);
                foreach (var word in page.GetWords())
                {
                    foreach (var token in remaining.ToArray())
                    {
                        if (word.Text.Contains(token, StringComparison.Ordinal))
                        {
                            var bound = word.BoundingBox;
                            result[token] = new AnchorBox
                            {
                                Page = pageNumber,
                                Top = bound.Top,
                                Bottom = bound.Bottom,
                                Left = bound.Left,
                                Right = bound.Right
                            };
                            remaining.Remove(token);
                            if (remaining.Count == 0) break;
                        }
                    }
                    if (remaining.Count == 0) break;
                }
            }

            if (remaining.Count > 0)
            {
                var missing = string.Join(", ", remaining);
                throw new InvalidOperationException($"Cannot find anchor text(s): {missing} in pdf.");
            }

            return result;
        }

        public static string InjectStyle(string html, string css)
        {
            const string marker = "</head>";
            var style = $"<style>\n{css}\n</style>\n";
            var idx = html.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            return idx >= 0 ? html.Insert(idx, style) : style + html;
        }

        public static async Task<string> ConvertPdfToHtmlAsync(string pdfPath)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("PDF file not found", pdfPath);

            var workDir = Path.GetDirectoryName(pdfPath)!;
            var outputFile = Path.GetFileNameWithoutExtension(pdfPath) + ".html";

            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"run --rm -v \"{workDir}:/pdf\" bwits/pdf2htmlex \"{Path.GetFileName(pdfPath)}\" \"{outputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string stdOut = await process.StandardOutput.ReadToEndAsync();
            string stdErr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"pdf2htmlEX docker conversion failed: {stdErr}");
            }

            return Path.Combine(workDir, outputFile);
        }
    }
}