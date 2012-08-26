namespace EmailModule.Specs
{
    using Machine.Specifications;
    using Moq;

    using arg = Moq.It;
    using it = Machine.Specifications.It;

    [Subject(typeof(EmailSubsystem))]
    public class When_sending_welcome_mail
    {
        static Mock<IEmailTemplateEngine> templateEngine;
        static Mock<IEmailSender> sender;
        static EmailSubsystem subSystem;

        Establish context = () =>
        {
            templateEngine = new Mock<IEmailTemplateEngine>();
            sender = new Mock<IEmailSender>();

            subSystem = new EmailSubsystem("me@myself.com", templateEngine.Object, sender.Object);
        };

        Because of = () =>
        {
            var email = new Email();

            templateEngine.Setup(e => e.Execute(EmailSubsystem.SendWelcomeMailTemplateName, arg.IsAny<object>())).Returns(email).Verifiable();
            sender.Setup(s => s.Send(email)).Verifiable();

            subSystem.SendWelcomeMail("Jon Smith", "@abcd1234", "jon@smit.com");
        };

        it should_use_correct_template = () => templateEngine.Verify();

        it should_send_mail = () => sender.Verify();
    }
}