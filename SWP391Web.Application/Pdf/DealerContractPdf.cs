using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SWP391Web.Application.DTO.EContract;
using UglyToad.PdfPig;

namespace SWP391Web.Application.Pdf
{
    public class DealerContractPdf
    {
        public static MemoryStream RenderDealerEContract(string companyName, string dealerName, string dealerAddress, string contact, string taxNo, DateTime date)
        {
            var ms = new MemoryStream();
            Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(40);
                    p.Size(PageSizes.A4);
                    p.Header().Text($"CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").SemiBold().FontSize(18).AlignCenter();
                    p.Header().Text($"Độc lập - Tự do - Hạnh phúc").SemiBold().FontSize(18).AlignCenter();
                    p.Header().Text($"HỢP ĐỒNG ĐẠI LÝ – {companyName}").SemiBold().FontSize(18).AlignCenter();
                    p.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Bên A (Hãng): {companyName}");
                        col.Item().Text($"Bên B (Đại lý): {dealerName}");
                        col.Item().Text($"Địa chỉ ĐL: {dealerAddress}");
                        col.Item().Text($"Mã số thuế: {taxNo}");
                        col.Item().Text($"Liên hệ: {contact}");
                        col.Item().Text($"Ngày lập: {date:hh:mm dd/MM/yyyy}");
                        col.Item().PaddingTop(10).Text("Điều 1: Phạm vi…");
                        col.Item().Text("Điều 2: Giá cả, chiết khấu…");
                        col.Item().Text("Điều 3: Thanh toán, giao nhận…");
                        col.Item().Text("Điều 4: Cam kết…");
                        col.Item().PaddingTop(10).Text("Điều 1: Phạm vi…");
                        col.Item().Text("Điều 2: Giá cả, chiết khấu…");
                        col.Item().Text("Điều 3: Thanh toán, giao nhận…");
                        col.Item().Text("Điều 4: Cam kết…");
                        col.Item().PaddingTop(10).Text("Điều 1: Phạm vi…");
                        col.Item().Text("Điều 2: Giá cả, chiết khấu…");
                        col.Item().Text("Điều 3: Thanh toán, giao nhận…");
                        col.Item().Text("Điều 4: Cam kết…");
                        col.Item().PaddingTop(10).Text("Điều 1: Phạm vi…");
                        col.Item().Text("Điều 2: Giá cả, chiết khấu…");
                        col.Item().Text("Điều 3: Thanh toán, giao nhận…");
                        col.Item().Text("Điều 4: Cam kết…");

                        //Sign position
                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.Spacing(30); // khoảng cách giữa 2 bên

                            // BÊN A (trái)
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingLeft(20).Text("ĐẠI_DIỆN_BÊN_A").FontSize(12); // anchor
                            });

                            // BÊN B (phải)
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().PaddingRight(20).Text("ĐẠI_DIỆN_BÊN_B").FontSize(12); // anchor
                            });
                        });
                    });
                    p.Footer().AlignRight().Text(t => { t.Span("Trang "); t.CurrentPageNumber(); t.Span(" / "); t.TotalPages(); });
                });
            }).GeneratePdf(ms);
            ms.Position = 0; return ms;
        }

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
    }
}
