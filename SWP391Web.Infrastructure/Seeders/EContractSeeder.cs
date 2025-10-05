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
            private static readonly Guid DealerV1Id = Guid.Parse("22222222-2222-4222-8222-222222222222");

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

            public static void SeedDealerEContract(ModelBuilder modelBuilder)
            {
                // Template gốc
                modelBuilder.Entity<EContractTemplate>().HasData(new
                {
                    Id = DealerTemplateId,
                    Code = "DL-STD",
                    Name = "Hợp đồng Đại lý – Chuẩn",
                    CreatedAt = new DateTime(2025, 10, 06, 00, 00, 00, DateTimeKind.Utc),
                    IsDeleted = false
                });

                // Version v1 (Published + Active) — dùng shadow FK ContractTemplateId
                modelBuilder.Entity<EContractTemplateVersion>().HasData(new
                {
                    Id = DealerV1Id,
                    ContractTemplateId = DealerTemplateId,      // shadow FK như mapping ở trên
                    VersionNo = 1,
                    ContentHtml = DealerContractHtmlV1,
                    StyleCss = (string?)null,
                    IsActive = true,
                    CreatedBy = "System",
                    Notes = "Seed v1",
                    CreatedAt = new DateTime(2025, 10, 06, 00, 00, 00, DateTimeKind.Utc),
                    Status = TemplateVersionStatus.Published   // enum lưu int
                });
            }
        }
    }
}
