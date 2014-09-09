namespace EmailModule
{
    using System;
    using System.IO;
    using System.Net.Mail;
    using System.Linq;
    using System.Collections.Generic;

    internal class Program
    {
        public static void Main()
        {
            IEmailTemplateContentReader templateReader = new FileSystemEmailTemplateContentReader();
            IEmailTemplateEngine templateEngine = new EmailTemplateEngine(templateReader);

            IEmailSender sender = new EmailSender
                                      {
                                          CreateClientFactory = () => new SmtpClientWrapper(CreateSmtpClientWhichDropsInLocalFileSystem())
                                      };

            EmailSubsystem subsystem = new EmailSubsystem("me@myself.com", templateEngine, sender);

            subsystem.SendWelcomeMail("Jon Smith", "~!Agc2d#7", "jon@smith.com", new List<string> (new string[]  {"Felipe","Felipe1","Felipe2"}));

            Console.WriteLine("Mail delivered, check the outbox folder.");
            Console.Read();
        }

        private static SmtpClient CreateSmtpClientWhichDropsInLocalFileSystem()
        {
            var outbox = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "outbox");

            if (!Directory.Exists(outbox))
            {
                Directory.CreateDirectory(outbox);
            }

            return new SmtpClient
                       {
                           DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                           PickupDirectoryLocation = outbox,
                           Host = "localhost",
                           UseDefaultCredentials = true
                       };
        }
    }
}