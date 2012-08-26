namespace EmailModule.Specs
{
    using System;

    using Machine.Specifications;
    using Moq;

    using arg = Moq.It;
    using it = Machine.Specifications.It;

    [Subject(typeof(EmailTemplateEngine))]
    public class When_trying_to_execute_with_blank_template_name
    {
        static Exception exception;

        Because of = () =>
        {
            exception = Catch.Exception(() => new EmailTemplateEngine(new Mock<IEmailTemplateContentReader>().Object).Execute(string.Empty));
        };

        it should_throw_exception = () => exception.ShouldBeOfType<ArgumentException>();
    }

    public abstract class EmailTemplateEngineWhenExecutingSpec
    {
        protected static EmailTemplateEngine templateEngine;
        protected static Email email;

        Because of = () =>
        {
            var model = new
                            {
                                EmailTemplateExecutingBehavior.From,
                                EmailTemplateExecutingBehavior.To,
                                EmailTemplateExecutingBehavior.CC,
                                EmailTemplateExecutingBehavior.Bcc,
                                EmailTemplateExecutingBehavior.Subject,
                                EmailTemplateExecutingBehavior.Name,
                                EmailTemplateExecutingBehavior.Password,
                                EmailTemplateExecutingBehavior.LogOnUrl
                            };

            email = templateEngine.Execute("Test" + Guid.NewGuid().ToString("N"), model);
        };
    }

    [Behaviors]
    public class EmailTemplateExecutingBehavior
    {
        public const string From = "me@myself.com";
        public const string To = "you@yourself.com";
        public const string CC = "he@himself.com";
        public const string Bcc = "she@herself.com";
        public const string Subject = "Welcome to myself.com";

        public const string Name = "Jon Smith";
        public const string Password = "ABcd12#$";
        public const string LogOnUrl = "http://myself.com/logon";

        protected static Email email;

        it should_set_form_address = () => email.From.ShouldMatch(From);

        it should_set_to_address = () => email.To.ShouldContain(To);

        it should_set_cc_address = () => email.CC.ShouldContain(CC);

        it should_set_bcc_address = () => email.Bcc.ShouldContain(Bcc);

        it should_set_subject = () => email.Subject.ShouldMatch(Subject);
    }

    [Subject(typeof(EmailTemplateEngine))]
    public class When_executing_multi_view_template : EmailTemplateEngineWhenExecutingSpec
    {
        Establish context = () =>
        {
            var contentReader = new Mock<IEmailTemplateContentReader>();

            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultSharedTemplateSuffix)).Returns(MailTemplates.SharedTemplate);
            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultHtmlTemplateSuffix)).Returns(MailTemplates.HtmlTemplate);
            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultTextTemplateSuffix)).Returns(MailTemplates.TextTemplate);

            templateEngine = new EmailTemplateEngine(contentReader.Object);
        };

        Behaves_like<EmailTemplateExecutingBehavior> template_execution;

        it should_set_html_body_variables = () =>
        {
            email.HtmlBody.ShouldContain(EmailTemplateExecutingBehavior.Name);
            email.HtmlBody.ShouldContain(EmailTemplateExecutingBehavior.Password);
            email.HtmlBody.ShouldContain(EmailTemplateExecutingBehavior.LogOnUrl);
        };

        it should_set_text_body_variables = () =>
        {
            email.TextBody.ShouldContain(EmailTemplateExecutingBehavior.Name);
            email.TextBody.ShouldContain(EmailTemplateExecutingBehavior.Password);
            email.TextBody.ShouldContain(EmailTemplateExecutingBehavior.LogOnUrl);
        };
    }

    [Subject(typeof(EmailTemplateEngine))]
    public class When_executing_single_html_view_template : EmailTemplateEngineWhenExecutingSpec
    {
        Establish context = () =>
        {
            var contentReader = new Mock<IEmailTemplateContentReader>();

            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultSharedTemplateSuffix)).Returns((string)null);
            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultHtmlTemplateSuffix)).Returns(MailTemplates.SharedTemplate + MailTemplates.HtmlTemplate);
            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultTextTemplateSuffix)).Returns((string)null);

            templateEngine = new EmailTemplateEngine(contentReader.Object);
        };

        Behaves_like<EmailTemplateExecutingBehavior> template_execution;

        it should_set_html_body_variables = () =>
        {
            email.HtmlBody.ShouldContain(EmailTemplateExecutingBehavior.Name);
            email.HtmlBody.ShouldContain(EmailTemplateExecutingBehavior.Password);
            email.HtmlBody.ShouldContain(EmailTemplateExecutingBehavior.LogOnUrl);
        };

        it should_not_set_text_body = () => email.TextBody.ShouldBeNull();
    }

    [Subject(typeof(EmailTemplateEngine))]
    public class When_executing_single_text_view_template : EmailTemplateEngineWhenExecutingSpec
    {
        Establish context = () =>
        {
            var contentReader = new Mock<IEmailTemplateContentReader>();

            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultSharedTemplateSuffix)).Returns((string)null);
            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultHtmlTemplateSuffix)).Returns((string)null);
            contentReader.Setup(r => r.Read(arg.IsAny<string>(), EmailTemplateEngine.DefaultTextTemplateSuffix)).Returns(MailTemplates.SharedTemplate + MailTemplates.TextTemplate);

            templateEngine = new EmailTemplateEngine(contentReader.Object);
        };

        Behaves_like<EmailTemplateExecutingBehavior> template_execution;

        it should_set_text_body_variables = () =>
        {
            email.TextBody.ShouldContain(EmailTemplateExecutingBehavior.Name);
            email.TextBody.ShouldContain(EmailTemplateExecutingBehavior.Password);
            email.TextBody.ShouldContain(EmailTemplateExecutingBehavior.LogOnUrl);
        };

        it should_not_set_html_body = () => email.HtmlBody.ShouldBeNull();
    }
}