using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace BiskaUtil
{
    public class eMail
    {
        public eMail()
        {
            this.IsHTML = true;            
            this.TO = new List<System.Net.Mail.MailAddress>();
            this.BCC = new List<System.Net.Mail.MailAddress>();
            this.CC = new List<System.Net.Mail.MailAddress>();            
            this.Attachments = new List<System.Net.Mail.Attachment>();
        }

        public System.Net.Mail.MailAddress AddTO(string mailAddress, string displayName)
        {
            System.Net.Mail.MailAddress item = new System.Net.Mail.MailAddress(mailAddress, displayName);
            this.TO.Add(item);
            return item;
        }

        public List<System.Net.Mail.Attachment> Attachments { get; set; }

        public List<System.Net.Mail.MailAddress> BCC { get; set; }

        public List<System.Net.Mail.MailAddress> CC { get; set; }

        public string Content { get; set; }    
        public string Host { get; set; }
        public bool IsHTML { get; set; }         
        public int Port { get; set; }

        public string SenderAccountName { get; set; }
        public string SenderAccountPassword { get; set; }
        public string SenderDisplayName { get; set; }
        public string SenderEmail { get; set; }
        public string Subject { get; set; }
        public List<System.Net.Mail.MailAddress> TO { get; set; }
        public bool UseSSL { get; set; }
        public static Exception Send(eMail msg)
        {                                   
            try
            {                   
                    #region Send Mail View .NET
                    MailMessage message = new MailMessage {
                        From = new System.Net.Mail.MailAddress(msg.SenderEmail, msg.SenderDisplayName, Encoding.UTF8)
                    };
                    SmtpClient client = new SmtpClient();
                    foreach (System.Net.Mail.MailAddress address2 in msg.TO)
                    {
                        message.To.Add(address2);
                    }
                    if (msg.BCC != null)
                    {
                        foreach (System.Net.Mail.MailAddress address3 in msg.BCC)
                        {
                            message.Bcc.Add(address3);
                        }
                    }
                    message.Subject = msg.Subject;
                    message.IsBodyHtml = msg.IsHTML;
                    if (msg.Attachments != null)
                    {
                        foreach (Attachment attachment in msg.Attachments)
                        {
                            message.Attachments.Add(attachment);
                        }
                    }
                    message.BodyEncoding = Encoding.UTF8;
                    message.Body = msg.Content;
                    message.Priority = MailPriority.High;
                    client.Credentials = new NetworkCredential(msg.SenderAccountName, msg.SenderAccountPassword);                
                    client.Port = msg.Port;
                    client.Host = msg.Host;
                    client.EnableSsl = msg.UseSSL;
                    client.Send(message);                
                    #endregion
                     
            }
            catch (Exception exception)
            {
                return exception;
            }
            
            return null;
        
        }
    }
}