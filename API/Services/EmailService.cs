using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace API.Services
{
    public class EmailService : IEmailService
    {
        public AccountRecovery SendEmail(string email)
        {
            Guid guid = Guid.NewGuid();
            AccountRecovery acc = new AccountRecovery()
            {
                Id = guid,
                email = email,
                used = false,
                TimeStamp = DateTime.Now
            };
            try
            {

                string link = "https://fsmorg.com/accountrecovery/" + guid.ToString();
                string body = "";
                body += @"
<html lang=""en"">
    <head>    
        <meta content=""text/html; charset=utf-8"" http-equiv=""Content-Type"">
        <title>
            Upcoming topics
        </title>
        <style type=""text/css"">
            body {
                text-align: center;
                font-family: Arial, Helvetica, sans-serif;
            }
        </style>
    </head>
    <body>
        <h1>FSM account recovery</h1>
        <p>Use the link below to change your password, if this was not you please contact fsmorg.ow@gmail.com ASAP.</p>";
                body += @$"
        <a href=""{link}"">{link}</a>
    </body>
</html>
";
                var message = new MailMessage("noreply@fsm.com.au", email, "FSM account recovery", body);
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("FSMorg.ow@gmail.com", "FSMadmin1"),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
            catch (Exception e)
            {
                throw new ApplicationException();
            }
            return acc;
        }
    }
}
