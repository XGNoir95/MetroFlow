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
                Subject = "🔐 Your MetroFlow OTP Verification Code",
                IsBodyHtml = true,
                Body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>
                    <div style='max-width: 500px; margin: auto; background: white; border-radius: 8px; 
                                box-shadow: 0 4px 8px rgba(0,0,0,0.1); padding: 20px; text-align: center;'>
                        
                        <h2 style='color: #4CAF50;'>MetroFlow OTP Verification</h2>
                        <p style='font-size: 16px; color: #555;'>Use the following OTP to verify your account:</p>
                        
                        <div style='font-size: 24px; font-weight: bold; color: #333; 
                                    background: #f0f0f0; padding: 10px; border-radius: 6px; display: inline-block;'>
                            {otp}
                        </div>
                        
                        <p style='margin-top: 20px; font-size: 14px; color: #888;'>
                            ⚠️ This OTP is valid for <b>5 minutes</b>. Do not share it with anyone.
                        </p>
                        
                        <hr style='margin: 20px 0;'/>
                        <p style='font-size: 12px; color: #aaa;'>If you didn’t request this OTP, please ignore this email.</p>
                    </div>
                </div>"
            };

            mail.To.Add(toEmail);
            client.Send(mail);
        }


        public void SendConfirmation(string toEmail, string name)
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
                Subject = "🎉 Your MetroFlow Account is Verified!",
                IsBodyHtml = true,
                Body = $@"
        <div style='font-family: Arial, sans-serif; padding: 40px; background: #f9f9f9;'>
            <div style='max-width: 500px; margin: auto; background: #fff; border-radius: 8px; 
                        box-shadow: 0 4px 8px rgba(0,0,0,0.1); padding: 20px; text-align: center;'>
                <h2 style='color:#4CAF50;'>Congratulations, {name}!</h2>
                <p style='font-size:16px; color:#555;'>Your MetroFlow account has been successfully verified.</p>
                <hr style='margin:20px 0;'/>
                <p style='font-size:12px; color:#aaa;'>Welcome aboard!</p>
            </div>
        </div>"
            };

            mail.To.Add(toEmail);
            client.Send(mail);
        }

    }
}
