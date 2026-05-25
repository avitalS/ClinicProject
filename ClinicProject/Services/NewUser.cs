using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Services;


namespace Services
{
    public class NewUser: INewUser
    {
        public bool SendEmail(string email, string subject, string body)
        {
            try
            {
                string senderEmail = "avital2163@gmail.com";
                string password = "0548551680";
                var loginInfo = new NetworkCredential(senderEmail, password);
                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(senderEmail);
                    msg.To.Add(new MailAddress(email));
                    msg.Subject = subject;
                    msg.Body = body;
                    using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = loginInfo;
                        smtpClient.Send(msg);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
        public void SendWelcome(ClientDTO client)
        {
            SendEmail(client.Email, "Welcome to Clinic","you joined sucssefuly to our clinic");
        }
         public void SendPass(ClientDTO client)
        {
            SendEmail(client.Email, "Your password ", $"your pass: {client.Pass} ");
        }

        public void CreatePass(ClientDTO client)
        {
            Random rand = new Random();
            client.Pass = rand.Next(100000, 1000000).ToString();
        }
    }
}