namespace EmailModule.Specs
{
    using System;
    using System.Net.Mail;

    using Machine.Specifications;
    using Moq;

    using arg = Moq.It;
    using it = Machine.Specifications.It;

    [Subject(typeof(EmailSender))]
    public class When_trying_to_send_null_email
    {
        static Exception exception;

        Because of = () =>
        {
            exception = Catch.Exception(() => new EmailSender().Send(null));
        };

        it should_throw_exception = () => exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Subject(typeof(EmailSender))]
    public class When_trying_to_send_valid_email
    {
        static Mock<ISmtpClient> client;
        static EmailSender sender;

        Establish context = () =>
        {
            client = new Mock<ISmtpClient>();
            client.Setup(c => c.Send(arg.Is<MailMessage>(m => m != null))).Verifiable();

            sender = new EmailSender { CreateClientFactory = () => client.Object };
        };

        Because of = () => sender.Send(new Email { From = "me@myself.com" });

        it should_send_mail = () => client.Verify();
    }

    [Subject(typeof(EmailSender))]
    public class When_message_is_created_from_email
    {
        static Email multiViewMail;
        static Email htmlMail;
        static Email textMail;

        static MailMessage multiViewMessage;
        static MailMessage htmlMessage;
        static MailMessage textMessage;

        Because of = () =>
        {
            multiViewMail = CreateEmail();
            htmlMail = CreateEmail();
            textMail = CreateEmail();

            multiViewMail.HtmlBody = htmlMail.HtmlBody = "<p>This is the html body<p>";
            multiViewMail.TextBody = textMail.TextBody = "This is the html body";

            var emailSender = new EmailSender();

            multiViewMessage = emailSender.CreateMailMessageFactory(multiViewMail);
            htmlMessage = emailSender.CreateMailMessageFactory(htmlMail);
            textMessage = emailSender.CreateMailMessageFactory(textMail);
        };

        it should_set_subject = () => multiViewMessage.Subject.ShouldMatch(multiViewMail.Subject);

        it should_set_from = () => multiViewMessage.From.Address.ShouldMatch(multiViewMail.From);

        it should_set_sender = () => multiViewMessage.Sender.Address.ShouldMatch(multiViewMail.Sender);

        it should_set_to = () =>
        {
            multiViewMessage.To.Count.ShouldEqual(multiViewMail.To.Count);

            foreach (var item in multiViewMessage.To)
            {
                multiViewMail.To.ShouldContain(item.Address);
            }
        };

        it should_set_reply_to = () =>
        {
            multiViewMessage.ReplyToList.Count.ShouldEqual(multiViewMail.ReplyTo.Count);

            foreach (var item in multiViewMessage.ReplyToList)
            {
                multiViewMail.ReplyTo.ShouldContain(item.Address);
            }
        };

        it should_set_cc = () =>
        {
            multiViewMessage.CC.Count.ShouldEqual(multiViewMail.CC.Count);

            foreach (var item in multiViewMessage.CC)
            {
                multiViewMail.CC.ShouldContain(item.Address);
            }
        };

        it should_set_bcc = () =>
        {
            multiViewMessage.Bcc.Count.ShouldEqual(multiViewMail.Bcc.Count);

            foreach (var item in multiViewMessage.Bcc)
            {
                multiViewMail.Bcc.ShouldContain(item.Address);
            }
        };

        it should_set_header = () =>
        {
            multiViewMessage.Headers.Count.ShouldEqual(multiViewMail.Headers.Count);

            foreach (var pair in multiViewMail.Headers)
            {
                multiViewMessage.Headers[pair.Key].ShouldMatch(multiViewMail.Headers[pair.Key]);
            }
        };

        it should_set_both_body_of_multiview_mail = () =>
        {
            multiViewMessage.AlternateViews.Count.ShouldEqual(2);
            multiViewMessage.AlternateViews[0].ContentType.MediaType.ShouldMatch(ContentTypes.Html);
            multiViewMessage.AlternateViews[1].ContentType.MediaType.ShouldMatch(ContentTypes.Text);
        };

        it should_not_set_body_of_multiview_mail = () => multiViewMessage.Body.ShouldBeEmpty();

        it should_not_set_alternate_view_of_html_mail = () => htmlMessage.AlternateViews.Count.ShouldEqual(0);

        it should_set_body_of_html_mail = () => htmlMessage.Body.ShouldNotBeEmpty();

        it should_mark_body_as_html_of_html_mail = () => htmlMessage.IsBodyHtml.ShouldBeTrue();

        it should_not_set_alternate_view_of_text_mail = () => textMessage.AlternateViews.Count.ShouldEqual(0);

        it should_set_body_of_text_mail = () => textMessage.Body.ShouldNotBeEmpty();

        it should_not_mark_body_as_html_of_text_mail = () => textMessage.IsBodyHtml.ShouldBeFalse();

        static Email CreateEmail()
        {
            var email = new Email
                       {
                           From = "me@myself.com",
                           Sender = "me@myself.com",
                           Subject = "A sample mail"
                       };

            email.To.Add("aaa@aaa.com");
            email.To.Add("bbb@bbb.com");

            email.ReplyTo.Add("ccc@ccc.com");
            email.ReplyTo.Add("ddd@ddd.com");

            email.CC.Add("eee@eee.com");
            email.CC.Add("fff@fff.com");

            email.Bcc.Add("ggg@ggg.com");
            email.Bcc.Add("hhh@hhh.com");

            email.Headers.Add("k1", "v1");
            email.Headers.Add("k2", "v2");

            return email;
        }
    }
}