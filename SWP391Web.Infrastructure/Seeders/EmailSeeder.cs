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
    public static class EmailSeeder
    {
        public static void SeedEmailTemplate(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailTemplate>().HasData(
               new
               {
                   Id = Guid.Parse("34835833-11aa-4760-a6bb-a1034f8ec9dc"),
                   TemplateName = "SendVerifyEmail",
                   SenderName = "SWP391",
                   SenderEmail = "hoangtuzami@gmail.com",
                   Category = "Verify",
                   SubjectLine = "Xác minh địa chỉ Email của bạn",
                   PreHeaderText = "Tài khoản của bạn đang chờ xác minh",
                   PersonalizationTags = "{Login}",
                   BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Xác minh Email</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #007bff; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>SWP391</h2>
                            <p style='margin: 0;'>Xác minh tài khoản của bạn</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Cảm ơn bạn đã đăng ký tài khoản tại <strong>SWP391</strong>.</p>
                            <p style='font-size: 16px;'>Để hoàn tất quá trình đăng ký, vui lòng nhấn vào nút bên dưới để xác minh địa chỉ email của bạn:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{Login}' style='background-color: #007bff; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Xác minh ngay</a>
                            </div>
                            <p style='font-size: 14px; color: #666;'>Nếu bạn không yêu cầu đăng ký tài khoản này, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi nếu có bất kỳ thắc mắc nào.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'>Đội ngũ <strong>SWP391</strong></p>
                            <p style='margin: 0;'>Mọi thắc mắc xin liên hệ: <a href='https://SWP391.vn' style='color: #007bff; text-decoration: none;'>SWP391.vn</a></p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                   FooterContent = "SWP391 Support Team",
                   CallToAction = "<a href=\"{Login}\">Xác minh tài khoản</a>",
                   Language = "Vietnamese",
                   RecipientType = "Customer",
                   CreatedBy = "System",
                   CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                   UpdatedBy = "System",
                   UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                   Status = EmailStatus.Active
               });

            modelBuilder.Entity<EmailTemplate>().HasData(
   new
   {
       Id = Guid.Parse("8f6d9d5d-2f91-4df8-9f70-ec5a09c4f111"),
       TemplateName = "ResetPassword",
       SenderName = "SWP391",
       SenderEmail = "hoangtuzami@gmail.com",
       Category = "Authentication",
       SubjectLine = "Yêu cầu đặt lại mật khẩu",
       PreHeaderText = "Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản SWP391",
       PersonalizationTags = "{ResetLink}",
       BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Đặt lại mật khẩu</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #dc3545; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>SWP391</h2>
                            <p style='margin: 0;'>Đặt lại mật khẩu</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Xin chào,</p>
                            <p style='font-size: 16px;'>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại <strong>SWP391</strong>.</p>
                            <p style='font-size: 16px;'>Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{ResetLink}' style='background-color: #dc3545; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Đặt lại mật khẩu</a>
                            </div>
                            <p style='font-size: 14px; color: #666;'>Nếu bạn không yêu cầu thao tác này, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'>Đội ngũ <strong>SWP391</strong></p>
                            <p style='margin: 0;'>Mọi thắc mắc xin liên hệ: <a href='https://SWP391.vn' style='color: #dc3545; text-decoration: none;'>SWP391.vn</a></p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
       FooterContent = "SWP391 Support Team",
       CallToAction = "<a href=\"{ResetLink}\">Đặt lại mật khẩu</a>",
       Language = "Vietnamese",
       RecipientType = "Customer",
       CreatedBy = "System",
       CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
       UpdatedBy = "System",
       UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
       Status = EmailStatus.Active
   });

            modelBuilder.Entity<EmailTemplate>().HasData(
              new
              {
                  Id = Guid.Parse("c1b4c5a2-0e9b-4f11-aa2f-3cf19b0f9e0b"),
                  TemplateName = "NotifyContractPdf",
                  SenderName = "SWP391",
                  SenderEmail = "hoangtuzami@gmail.com",
                  Category = "Contract",
                  SubjectLine = "Thông báo hợp đồng: {ContractSubject}",
                  PreHeaderText = "Hợp đồng của bạn đã sẵn sàng để xem/tải (PDF).",
                  // Liệt kê các placeholder bạn dự định thay thế
                  PersonalizationTags = "{FullName},{ContractSubject},{DownloadLink},{Company},{SupportEmail}",
                  BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Thông báo hợp đồng</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #0ea5e9; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>SWP391</h2>
                            <p style='margin: 0;'>Thông báo hợp đồng</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Xin chào <strong>{FullName}</strong>,</p>
                            <p style='font-size: 16px;'>Hợp đồng <strong>{ContractSubject}</strong> của bạn đã sẵn sàng.</p>
                            <p style='font-size: 16px;'>Bạn có thể xem hoặc tải về bản PDF bằng cách nhấn nút bên dưới:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{DownloadLink}' style='background-color: #0ea5e9; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Xem/Tải hợp đồng (PDF)</a>
                            </div>
                            <p style='font-size: 14px; color: #666;'>Nếu nút không hiển thị, bạn có thể mở liên kết: <a href='{DownloadLink}' style='color: #0ea5e9; text-decoration: none;'>{DownloadLink}</a></p>
                            <p style='font-size: 14px; color: #666;'>Nếu cần hỗ trợ, vui lòng liên hệ: <a href='mailto:{SupportEmail}' style='color: #0ea5e9; text-decoration: none;'>{SupportEmail}</a>.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'><strong>{Company}</strong> - Đội ngũ hỗ trợ SWP391</p>
                            <p style='margin: 0;'>Đây là email tự động, vui lòng không phản hồi.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                  FooterContent = "SWP391 Support Team",
                  CallToAction = "<a href=\"{DownloadLink}\">Xem/Tải hợp đồng (PDF)</a>",
                  Language = "Vietnamese",
                  RecipientType = "Customer",
                  CreatedBy = "System",
                  CreatedAt = new DateTime(2025, 10, 09, 0, 0, 0, DateTimeKind.Utc),
                  UpdatedBy = "System",
                  UpdatedAt = new DateTime(2025, 10, 09, 0, 0, 0, DateTimeKind.Utc),
                  Status = EmailStatus.Active
              });
            modelBuilder.Entity<EmailTemplate>().HasData(
    new
    {
        Id = Guid.Parse("b7f0a7cb-0d7c-4f1e-9c8e-9c1d4a2b7f11"),
        TemplateName = "DealerWelcome",
        SenderName = "SWP391",
        SenderEmail = "hoangtuzami@gmail.com",
        Category = "Dealer",
        SubjectLine = "Chúc mừng! Tài khoản đại lý của bạn đã được kích hoạt",
        PreHeaderText = "Thông tin đăng nhập và liên kết truy cập hệ thống",
        PersonalizationTags = "{FullName},{DealerName},{UserName},{Password},{LoginUrl},{Company},{SupportEmail}",
        BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Chào mừng Đại lý</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #16a34a; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>{Company}</h2>
                            <p style='margin: 0;'>Chào mừng Đại lý mới</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Xin chào <strong>{FullName}</strong>,</p>
                            <p style='font-size: 16px;'>Chúc mừng bạn đã trở thành đại lý của <strong>{Company}</strong> cho đơn vị <strong>{DealerName}</strong>.</p>
                            <p style='font-size: 16px;'>Dưới đây là thông tin đăng nhập của bạn:</p>
                            <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:6px; margin: 14px 0;'>
                                <tr>
                                    <td style='padding: 12px 16px; font-size: 14px;'>Tên đăng nhập:</td>
                                    <td style='padding: 12px 16px; font-size: 14px;'><strong>{UserName}</strong></td>
                                </tr>
                                <tr style='background-color:#ffffff;'>
                                    <td style='padding: 12px 16px; font-size: 14px;'>Mật khẩu:</td>
                                    <td style='padding: 12px 16px; font-size: 14px;'><strong>{Password}</strong></td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 16px; font-size: 14px;'>Đường dẫn đăng nhập:</td>
                                    <td style='padding: 12px 16px; font-size: 14px;'><a href='{LoginUrl}' style='color:#16a34a; text-decoration:none;'>{LoginUrl}</a></td>
                                </tr>
                            </table>

                            <div style='text-align: center; margin: 26px 0;'>
                                <a href='{LoginUrl}' style='background-color: #16a34a; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Đăng nhập ngay</a>
                            </div>

                            <p style='font-size: 14px; color: #666;'>
                                Vì lý do bảo mật, vui lòng <strong>đổi mật khẩu ngay</strong> sau lần đăng nhập đầu tiên (Mục: Hồ sơ/Tài khoản &rarr; Đổi mật khẩu).
                            </p>
                            <p style='font-size: 14px; color: #666;'>
                                Nếu cần hỗ trợ, vui lòng liên hệ: <a href='mailto:{SupportEmail}' style='color:#16a34a; text-decoration:none;'>{SupportEmail}</a>.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'><strong>{Company}</strong> - Đội ngũ hỗ trợ SWP391</p>
                            <p style='margin: 0;'>Đây là email tự động, vui lòng không phản hồi.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
        FooterContent = "SWP391 Support Team",
        CallToAction = "<a href=\"{LoginUrl}\">Đăng nhập ngay</a>",
        Language = "Vietnamese",
        RecipientType = "Customer",
        CreatedBy = "System",
        CreatedAt = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc),
        UpdatedBy = "System",
        UpdatedAt = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc),
        Status = EmailStatus.Active
    });

            modelBuilder.Entity<EmailTemplate>().HasData(
new
{
Id = Guid.Parse("A89C8D8B-BA15-4E60-A74E-8A3B9C2B7A77"),
TemplateName = "SendEmployeeCredentials",
SenderName = "EVSystem Support",
SenderEmail = "hoangtuzami@gmail.com",
Category = "Onboarding",
SubjectLine = "Chào mừng bạn đến với Hệ thống xe điện ABX – Thông tin tài khoản đăng nhập",
PreHeaderText = "Tài khoản nhân viên đã được tạo. Đăng nhập ngay để kích hoạt và thay đổi mật khẩu.",
PersonalizationTags = "{EmployeeName},{Username},{TempPassword},{LoginLink}",
BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Tài khoản nhân viên - EVSystem</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f6fa; color: #333; }
        .container { width: 100%; background-color: #f5f6fa; padding: 40px 0; }
        .email-card { width: 600px; background: #fff; border-radius: 12px; margin: auto; overflow: hidden; box-shadow: 0 6px 18px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(90deg, #dc3545, #b02a37); color: #fff; text-align: center; padding: 24px 16px; }
        .header h1 { margin: 0; font-size: 24px; letter-spacing: 0.5px; }
        .content { padding: 36px 40px; line-height: 1.7; }
        .content p { font-size: 15px; margin-bottom: 12px; }
        .credentials { background-color: #f8f9fa; border: 1px solid #eee; border-radius: 8px; padding: 16px 20px; margin: 20px 0; }
        .credentials td { padding: 8px 0; font-size: 15px; }
        .btn { display: inline-block; padding: 14px 28px; background: #dc3545; color: #fff !important; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: 600; transition: all 0.3s ease; }
        .btn:hover { background: #b02a37; }
        .note { font-size: 13px; color: #777; margin-top: 24px; line-height: 1.6; }
        .footer { background-color: #f1f1f1; text-align: center; padding: 18px; font-size: 13px; color: #888; }
        .footer a { color: #dc3545; text-decoration: none; }
        @media (max-width: 620px) {
            .email-card { width: 95%; }
            .content { padding: 24px; }
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='email-card'>
            <div class='header'>
                <h1>Electric Vehicle Management System</h1>
                <p>Chào mừng bạn gia nhập đội ngũ EVSystem</p>
            </div>
            <div class='content'>
                <p>Xin chào <strong>{EmployeeName}</strong>,</p>
                <p>Tài khoản nhân viên của bạn tại <strong>Hệ thống xe điện ABX</strong> đã được khởi tạo thành công.</p>
                <p>Vui lòng sử dụng thông tin dưới đây để đăng nhập lần đầu:</p>
                <table class='credentials' width='100%'>
                    <tr>
                        <td><strong>Tên đăng nhập:</strong></td>
                        <td>{Username}</td>
                    </tr>
                    <tr>
                        <td><strong>Mật khẩu tạm thời:</strong></td>
                        <td>{TempPassword}</td>
                    </tr>
                </table>
                <p style='text-align: center; margin: 30px 0;'>
                    <a href='{LoginLink}' class='btn'>Đăng nhập ngay</a>
                </p>
                <p class='note'>
                    🔒 <strong>Lưu ý bảo mật:</strong><br>
                    - Vui lòng đổi mật khẩu ngay trong lần đăng nhập đầu tiên.<br>
                    - Không chia sẻ thông tin tài khoản với người khác.<br>
                    - Nếu bạn không yêu cầu tạo tài khoản này, vui lòng liên hệ bộ phận quản trị hệ thống.
                </p>
            </div>
            <div class='footer'>
                <p>Trân trọng,<br><strong>EVSystem Team</strong></p>
                <p>Website: <a href='https://electricvehiclesystem.click'>electricvehiclesystem.click</a></p>
            </div>
        </div>
    </div>
</body>
</html>",
FooterContent = "EVSystem Team – Electric Vehicle Management System",
CallToAction = "<a href=\"{LoginLink}\" class=\"btn\">Đăng nhập ngay</a>",
Language = "Vietnamese",
RecipientType = "Employee",
CreatedBy = "System",
CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
UpdatedBy = "System",
UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
Status = EmailStatus.Active
});
        }
    }
}
