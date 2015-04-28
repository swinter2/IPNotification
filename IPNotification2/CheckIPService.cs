using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;

namespace IPNotification2
{
    public partial class CheckIPService
    {
        private const string _checkURI = "CheckURI";
        private const string _emailFrom = "EmailFrom";
        private const string _emailTo = "EmailTo";
        private const string _emailSubject = "EmailSubject";

        private Regex _ipPattern = new Regex(@"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}", RegexOptions.Multiline);

        protected Uri CheckUri { get; set; }

        protected MailAddress EmailFrom { get; set; }
        protected MailAddress EmailTo { get; set; }
        protected string EmailSubject { get; set; }

        public CheckIPService()
        {
            CheckUri = new Uri(ConfigurationManager.AppSettings[_checkURI]);
            EmailFrom = new MailAddress(ConfigurationManager.AppSettings[_emailFrom]);
            EmailTo = new MailAddress(ConfigurationManager.AppSettings[_emailTo]);
            EmailSubject = ConfigurationManager.AppSettings[_emailSubject];
        }

        public void Check()
        {
            try
            {
                var req = WebRequest.Create(CheckUri);
                var res = req.GetResponse();
                var stream = res.GetResponseStream();
                var sr = new StreamReader(stream);
                var body = sr.ReadToEnd();
                sr.Close();
                stream.Close();

                var matches = _ipPattern.Matches(body);

                // Send these IP address matches in an email to me.
                var msgBody = new StringBuilder();
                msgBody.AppendLine("IP Addresses:");

                foreach (var match in matches)
                {
                    msgBody.AppendLine(match.ToString());
                }

                var msg = new MailMessage(EmailFrom.ToString(), EmailTo.ToString(), EmailSubject, msgBody.ToString());
                var smtp = new SmtpClient();
                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(Assembly.GetExecutingAssembly().FullName, string.Format("{0}\n\n{1}", ex.Message, ex.StackTrace), EventLogEntryType.Error);
            }
        }

    }
}
