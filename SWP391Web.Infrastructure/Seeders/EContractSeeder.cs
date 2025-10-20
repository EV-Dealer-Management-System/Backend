using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Seeders
{
    public class EContractSeeder
    {
        public static class EContractTemplateSeeder
        {
            // GUID cố định để HasData
            private static readonly Guid DealerTemplateId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            private static readonly Guid BookingTemplateId = Guid.Parse("2e932187-140c-4ccf-807f-5e7cc1061663");

            // HTML v1 (có token + anchor)
            private const string DealerContractHtmlV1 = @"
<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8"" />
<title>HỢP ĐỒNG ĐẠI LÝ – {{ company.name }}</title>
<style>
  @page { size: A4; margin: 10mm 10mm 12mm 10mm; }
  body { font-family: 'Noto Sans', DejaVu Sans, Arial, sans-serif; font-size: 12pt; }
  h1, h2 { text-align: center; margin: 4px 0; }
  .meta { margin-top: 8px; }
  .section-title { margin-top: 10px; font-weight: bold; }
  .sign { margin-top: 30px; display: flex; justify-content: space-between; }
  .sign .box { width: 45%; }
  .muted { color: #777; font-size: 10pt; }
</style>
</head>
<body>
  <h2>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h2>
  <h2>Độc lập - Tự do - Hạnh phúc</h2>
  <h1>HỢP ĐỒNG ĐẠI LÝ – {{ company.name }}</h1>

  <div class=""meta"">
    <div>Bên A (Hãng): {{ company.name }}</div>
    <div>Bên B (Đại lý): {{ dealer.name }}</div>
    <div>Địa chỉ ĐL: {{ dealer.address }}</div>
    <div>Mã số thuế: {{ dealer.taxNo }}</div>
    <div>Liên hệ: {{ dealer.contact }}</div>
    <div>Ngày lập: {{ contract.date | date.format '%d/%m/%Y %H:%M' }}</div>
  </div>

  <div class=""section-title"">Điều 1: Phạm vi…</div>
  <p>{{ terms.scope }}</p>
  <div class=""section-title"">Điều 2: Giá cả, chiết khấu…</div>
  <p>{{ terms.pricing }}</p>
  <div class=""section-title"">Điều 3: Thanh toán, giao nhận…</div>
  <p>{{ terms.payment }}</p>
  <div class=""section-title"">Điều 4: Cam kết…</div>
  <p>{{ terms.commitments }}</p>

  <div class=""sign"">
    <div class=""box"">
      <div class=""muted"">Ký, ghi rõ họ tên</div>
      <div>ĐẠI_DIỆN_BÊN_A</div>
      <div>{{ roles.A.representative }} – {{ roles.A.title }}</div>
    </div>
    <div class=""box"" style=""text-align:right"">
      <div class=""muted"">Ký, ghi rõ họ tên</div>
      <div>ĐẠI_DIỆN_BÊN_B</div>
      <div>{{ roles.B.representative }} – {{ roles.B.title }}</div>
    </div>
  </div>

  <div class=""muted"">Trang {{ page }} / {{ pages }}</div>
</body>
</html>";

            private const string BookingContractHtml = @"<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8"" />
<title>XÁC NHẬN ĐẶT XE – ĐIỀU XE VỀ ĐẠI LÝ</title>
<style>
  @page { size: A4; margin: 10mm 10mm 12mm 10mm; }
  body { background:#fff; font-family: 'Noto Sans', DejaVu Sans, Arial, sans-serif; font-size: 12pt; line-height: 1.45; }
  h1, h2, h3 { text-align: center; margin: 6px 0; }
  .meta { margin-top: 8px; }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 16px; }
  .section-title { margin-top: 12px; font-weight: bold; text-transform: uppercase; }
  table { width: 100%; border-collapse: collapse; margin-top: 8px; }
  th, td { border: 1px solid #444; padding: 6px 8px; vertical-align: top; }
  .right { text-align: right; }
  .muted { color: #777; font-size: 10pt; }
  .note { white-space: pre-line; }
  thead { display: table-header-group; }

  /* Chữ ký: 1 hàng – 2 cột (không khung) */
  .sign-table { width:100%; table-layout:fixed; border-collapse:collapse; margin-top:24px; break-inside:avoid; page-break-inside:avoid; }
  .sign-table tr, .sign-table td { break-inside:avoid; page-break-inside:avoid; }
  .sign-table td { width:50%; padding:0 6px; vertical-align:bottom; }

  /* Ô chứa chữ ký (để neo anchor tuyệt đối) */
  .sign-slot { position:relative; padding:10px 10px 10px 10px; }

  /* Ẩn anchor: chữ trắng, opacity rất nhỏ, cỡ chữ nhỏ — vẫn giữ trong text layer để tool dò */
  .anchor {
    position:absolute; bottom:10px; left:10px;
    font-size:1pt; line-height:1;
    color:#ffffff;          /* sửa đúng mã màu */
    opacity:0.01;           /* tránh 0 để không bị loại khỏi text layer */
    letter-spacing:-0.2pt;
    user-select:none;
  }
</style>
</head>
<body>
  <h2>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h2>
  <h3>Độc lập - Tự do - Hạnh phúc</h3><br>
  <h1>XÁC NHẬN ĐẶT XE – ĐIỀU XE VỀ ĐẠI LÝ</h1><br><br>

  <div class=""meta grid"">
    <div><b>Ngày lập:</b> {{ booking.date }}</div>
    <div><b>Đại lý (Bên đề nghị):</b> {{ dealer.name }}</div>
    <div><b>Địa chỉ Đại lý:</b> {{ dealer.address }}</div>
    <div><b>Liên hệ Đại lý:</b> {{ dealer.contact }}</div>
    <div><b>Hãng/Doanh nghiệp (Bên phê duyệt):</b> {{ company.name }} | MST: {{ company.taxNo }}</div>
    <div><b>Tổng số lượng:</b> {{ booking.total }}</div>
  </div>

  <div class=""section-title"">CHI TIẾT ĐỀ NGHỊ ĐIỀU XE</div>
  <table>
    <thead>
    <tbody>
      {{ booking.rows }}
    </tbody>
  </table>

  <div class=""section-title"">GHI CHÚ</div>
  <div class=""note"">{{ booking.note }}</div>

  <div class=""section-title"">ĐIỀU KHOẢN ÁP DỤNG</div>
  <div class=""note"">
    1) Mục đích: Đại lý đề nghị Hãng phân bổ/điều xe về kho nhận nêu trên để phục vụ bán hàng. <br><br>
    2) Thời hạn & lịch điều xe: Hãng sắp xếp nguồn hàng và lịch vận chuyển theo khả năng cung ứng; thời gian dự kiến có thể thay đổi do tồn kho/ logistics.<br><br>
    3) Trách nhiệm phối hợp: Hai bên phối hợp xác nhận lịch xuất – nhận xe; Đại lý chuẩn bị mặt bằng/kho bãi, nhân sự tiếp nhận và hồ sơ cần thiết theo hướng dẫn của Hãng.
    <br><br>
  </div>

  <!-- CHỮ KÝ: 1 HÀNG - 2 CỘT (không khung) -->
  <table class=""sign-table"">
    <tr>
      <td>
        <div class=""sign-slot"">
          <div><b>ĐẠI DIỆN ĐẠI LÝ (Bên đề nghị)</b></div>
          <div class=""muted"">Ký, ghi rõ họ tên (đóng dấu nếu có)</div>
          <div class=""anchor"">ĐẠI_DIỆN_BÊN_A</div>
        </div>
      </td>
      <td>
        <div class=""sign-slot"">
          <div><b>ĐẠI DIỆN HÃNG (Bên phê duyệt)</b></div>
          <div class=""muted"">Ký, ghi rõ họ tên (đóng dấu)</div>
          <div class=""anchor"">ĐẠI_DIỆN_BÊN_B</div>
        </div>
      </td>
    </tr>
  </table>
</body>
</html>
";
            public static void SeedDealerEContract(ModelBuilder modelBuilder)
            {
                // Template gốc
                modelBuilder.Entity<EContractTemplate>().HasData(new
                {
                    Id = DealerTemplateId,
                    Code = "REGISTERDEALER",
                    Name = "Hợp đồng Đại lý – Chuẩn",
                    ContentHtml = DealerContractHtmlV1,
                    CreatedAt = new DateTime(2025, 10, 06, 00, 00, 00, DateTimeKind.Utc),
                    IsDeleted = false
                });
                modelBuilder.Entity<EContractTemplate>().HasData(new
                {
                    Id = BookingTemplateId,
                    Code = "BOOKINGECONTRACT",
                    Name = "Xác nhận đặt xe",
                    ContentHtml = BookingContractHtml,
                    CreatedAt = new DateTime(2025, 10, 06, 00, 00, 00, DateTimeKind.Utc),
                    IsDeleted = false
                });
            }
        }
    }
}
