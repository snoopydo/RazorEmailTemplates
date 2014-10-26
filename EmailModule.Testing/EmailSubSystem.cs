namespace EmailModule
{
    using System.Collections.Generic;

    public class EmailSubsystem
    {
        public const string SendWelcomeMailTemplateName = "SendWelcomeMail";

        public EmailSubsystem(string fromAddress, IEmailTemplateEngine templateEngine, IEmailSender sender)
        {
            Invariant.IsNotBlank(fromAddress, "fromAddress");
            Invariant.IsNotNull(templateEngine, "templateEngine");
            Invariant.IsNotNull(sender, "sender");

            FromAddress = fromAddress;
            TemplateEngine = templateEngine;
            Sender = sender;
        }

        protected IEmailTemplateEngine TemplateEngine { get; private set; }

        protected IEmailSender Sender { get; private set; }

        protected string FromAddress { get; private set; }

        public virtual void SendWelcomeMail(string name, string password, string email, IList<string> names)
        {
            var model = new
                        {
                            From = FromAddress,
                            To = email,
                            Name = name,
                            Password = password,
                            LogOnUrl = "http://mycompany.com/logon",
                            Address = new
                            {
                                Street = "Bla Street",
                                Suburb = "Bl Blaburb",
                                City = "Metroplis",
                                PostCode = 90210
                            },
                            Names = names
                        };

            var mail = TemplateEngine.Execute(SendWelcomeMailTemplateName, model);

            Sender.Send(mail);
        }
    }
}