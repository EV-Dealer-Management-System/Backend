using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace SWP391Web.Application.Pdf
{
    public class DealerContractPdf
    {
        public static MemoryStream Render(string companyName, string dealerName, string dealerAddress, string contact, DateTime date)
        {
            var ms = new MemoryStream();
            Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(40);
                    p.Size(PageSizes.A4);
                    p.Header().Text($"HỢP ĐỒNG ĐẠI LÝ – {companyName}").SemiBold().FontSize(18).AlignCenter();
                    p.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Bên A (Hãng): {companyName}");
                        col.Item().Text($"Bên B (Đại lý): {dealerName}");
                        col.Item().Text($"Địa chỉ ĐL: {dealerAddress}");
                        col.Item().Text($"Liên hệ: {contact}");
                        col.Item().Text($"Ngày lập: {date:dd/MM/yyyy}");
                        col.Item().PaddingTop(10).Text("Điều 1: Phạm vi…");
                        col.Item().Text("Điều 2: Giá cả, chiết khấu…");
                        col.Item().Text("Điều 3: Thanh toán, giao nhận…");
                        col.Item().Text("Điều 4: Cam kết…");
                    });
                    p.Footer().AlignRight().Text(t => { t.Span("Trang "); t.CurrentPageNumber(); t.Span(" / "); t.TotalPages(); });
                });
            }).GeneratePdf(ms);
            ms.Position = 0; return ms;
        }
    }
}
