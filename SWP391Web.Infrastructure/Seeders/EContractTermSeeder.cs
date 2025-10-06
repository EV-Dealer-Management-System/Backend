using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Seeders
{
    public static class EContractTermSeeder
    {
        // GUID cố định để HasData (tránh thay đổi qua các migration)
        private static readonly Guid L1Id = Guid.Parse("11111111-1111-4111-8111-111111111111");
        private static readonly Guid L2Id = Guid.Parse("22222222-2222-4222-8222-222222222222");
        private static readonly Guid L3Id = Guid.Parse("33333333-3333-4333-8333-333333333333");

        // Thời điểm tạo cố định (UTC) để không bị thay đổi mỗi lần chạy
        private static readonly DateTime SeedCreatedAtUtc = new DateTime(2025, 10, 06, 00, 00, 00, DateTimeKind.Utc);

        public static void SeedTerm(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EContractTerm>().HasData(
                // Level 1: Standard
                new
                {
                    Id = L1Id,
                    Name = "Dealer Standard v1",
                    DealerLevel = 1,
                    RoleRepresentative = "Trần Văn A",
                    RoleTitle = "Giám đốc Kinh doanh",
                    ExpiryYear = 1,
                    Scope = "Bên B là đại lý phân phối chính thức sản phẩm xe máy điện thương hiệu ABX tại TP.HCM.",
                    Pricing = "Giá sỉ theo Phụ lục 1; chiết khấu bậc theo doanh số quý; thay đổi giá báo trước 7 ngày.",
                    Payment = "Thanh toán chuyển khoản T+7 kể từ ngày nhận hàng; công nợ tối đa 500.000.000đ; quá hạn áp dụng lãi phạt.",
                    Commitment = "Bên A đảm bảo chất lượng, cung cấp tài liệu & đào tạo; Bên B bán đúng giá, bảo quản đúng quy cách, không tiết lộ thông tin mật.",
                    Region = "TP.HCM",
                    NoticeDay = "7",
                    OrderConfirmDays = "2",
                    DeliveryLocation = "Kho Bên B",
                    PaymentMethod = "Chuyển khoản",
                    PaymentDueDays = "7",
                    PenaltyRate = "1.5", // %/tháng
                    ClaimDays = "7",
                    TerminationNoticeDays = "15",
                    DisputeLocation = "TP.HCM",
                    IsActive = true,
                    CreatedAt = SeedCreatedAtUtc,
                    CreatedBy = "system"
                },

                // Level 2: Premium
                new
                {
                    Id = L2Id,
                    Name = "Dealer Premium v1",
                    DealerLevel = 2,
                    RoleRepresentative = "Trần Văn A",
                    RoleTitle = "Giám đốc Kinh doanh",
                    ExpiryYear = 2,
                    Scope = "Bên B là đại lý phân phối chính thức sản phẩm ABC tại TP.HCM và các tỉnh lân cận theo danh mục khu vực kèm theo.",
                    Pricing = "Giá sỉ theo Phụ lục 1; chiết khấu cao hơn khi đạt chỉ tiêu; thưởng vượt doanh số; thay đổi giá báo trước 10 ngày.",
                    Payment = "Thanh toán chuyển khoản T+10; hạn mức công nợ 1.000.000.000đ; quá hạn áp dụng lãi phạt.",
                    Commitment = "Bên A hỗ trợ POSM, digital marketing, training định kỳ; Bên B cam kết trưng bày theo chuẩn thương hiệu và chia sẻ báo cáo bán hàng/ tồn kho hàng tuần.",
                    Region = "TP.HCM + lân cận",
                    NoticeDay = "10",
                    OrderConfirmDays = "2",
                    DeliveryLocation = "Kho Bên A (FOB) hoặc thỏa thuận từng đợt",
                    PaymentMethod = "Chuyển khoản",
                    PaymentDueDays = "10",
                    PenaltyRate = "1.2", // %/tháng
                    ClaimDays = "10",
                    TerminationNoticeDays = "15",
                    DisputeLocation = "TP.HCM",
                    IsActive = true,
                    CreatedAt = SeedCreatedAtUtc,
                    CreatedBy = "system"
                },

                // Level 3: Exclusive
                new
                {
                    Id = L3Id,
                    Name = "Dealer Exclusive v1",
                    DealerLevel = 3,
                    RoleRepresentative = "Trần Văn A",
                    RoleTitle = "Giám đốc Kinh doanh",
                    ExpiryYear = 3,
                    Scope = "Bên B là đại lý độc quyền khu vực theo Phụ lục khu vực; có chỉ tiêu doanh số tối thiểu theo quý.",
                    Pricing = "Giá sỉ ưu đãi; chiết khấu/bonus theo bậc độc quyền; chương trình marketing đồng tài trợ; thay đổi giá báo trước 15 ngày.",
                    Payment = "Thanh toán chuyển khoản T+15; hạn mức công nợ 2.000.000.000đ; quá hạn áp dụng lãi phạt.",
                    Commitment = "Bên A hỗ trợ kỹ thuật nâng cao, phụ tùng bảo hành dự trữ; Bên B tuân thủ độc quyền, không kinh doanh sản phẩm cạnh tranh.",
                    Region = "Khu vực độc quyền (xác định tại Phụ lục)",
                    NoticeDay = "15",
                    OrderConfirmDays = "3",
                    DeliveryLocation = "Theo lịch giao nhận & tuyến vận chuyển thỏa thuận",
                    PaymentMethod = "Chuyển khoản",
                    PaymentDueDays = "15",
                    PenaltyRate = "1.0", // %/tháng
                    ClaimDays = "15",
                    TerminationNoticeDays = "20",
                    DisputeLocation = "TP.HCM",
                    IsActive = true,
                    CreatedAt = SeedCreatedAtUtc,
                    CreatedBy = "system"
                }
            );
        }
    }
}
