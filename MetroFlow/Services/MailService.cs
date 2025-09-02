using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace MetroFlow.Services
{
    public class MailService
    {
        private readonly IConfiguration _config;
        public MailService(IConfiguration config) => _config = config;

        public void SendOtp(string toEmail, string otp)
        {
            var s = _config.GetSection("SmtpSettings");
            using var client = new SmtpClient(s["Host"], int.Parse(s["Port"]))
            {
                Credentials = new NetworkCredential(s["User"], s["Password"]),
                EnableSsl = bool.Parse(s["EnableSsl"]!)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(s["User"]!, "MetroFlow Support"),
                Subject = "Your MetroFlow OTP",
                Body = $"Your OTP is: {otp}\nIt expires in 5 minutes.",
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);

            client.Send(mail);
        }
    }
}
