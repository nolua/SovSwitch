using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace SovSwitch
{
    class SendMail
    {
        public SendMail(Hashtable listeMail, string pathFileLog,string fileLogTemp, string smtpServeur,string senderFrom)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(senderFrom);
            Attachment attachment = new Attachment(pathFileLog + "/" + fileLogTemp);
            msg.Attachments.Add(attachment);
            foreach (DictionaryEntry dict in listeMail)
            {
                msg.To.Add(new MailAddress((string)dict.Value));
            }
            msg.Subject = "Sauvegarde Cisco";
            msg.Body = "test";

            // Creat SMTP.
            //using (SmtpClient client = new SmtpClient())
            //{
            //    client.Send(Message);
            //}
            //DisposeAttachments();

            using (SmtpClient smtp = new SmtpClient(smtpServeur))
            {
                smtp.Send(msg);
             }
            attachment.Dispose();
        }
    }
}
