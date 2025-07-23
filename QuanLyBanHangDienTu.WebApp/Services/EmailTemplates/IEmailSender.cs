namespace QuanLyBanHangDienTu.WebApp.Services.EmailTemplates
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
