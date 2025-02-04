using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace Service.Implement
{
    public class SendMailWithrawMoneyService
    {
        private readonly string _smtpServer = "smtp.gmail.com"; // Ví dụ: smtp.gmail.com
        private readonly int _smtpPort = 587; // Thường dùng 587 cho TLS
        private readonly string _emailSender = "minhthongpham9a2@gmail.com"; // Email của bạn
        private readonly string _emailPassword = "opbw bxye pymi osah"; // Mật khẩu ứng dụng hoặc email

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your Company Name", _emailSender));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, false); // false: dùng TLS
                await client.AuthenticateAsync(_emailSender, _emailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
