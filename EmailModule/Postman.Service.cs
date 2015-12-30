using dks.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Postman
{
	public class PostmanService
	{

		TemplateEngine e;

		public PostmanService()
		{
			e = new TemplateEngine(new FileSystemTemplateContentReader());
		}


		public MailMessage Render(string templateName)
		{
			return Render(templateName, null);
		}

		public MailMessage Render(string templateName, object model)
		{
			MailMessage message;

			var executionResult = e.Execute<EmailTemplate>(templateName, model);

			executionResult.AssembleParts();

			message = new MailMessage();

			message.Subject = executionResult.Subject;

			// Sender info
			if (executionResult.From != null) { message.From = executionResult.From; }
			if (executionResult.Sender != null) { message.Sender = executionResult.Sender; }
			foreach (var addr in executionResult.ReplyToList) { message.ReplyToList.Add(addr); }


			// Recepients
			foreach (var addr in executionResult.To) { message.To.Add(addr); }
			foreach (var addr in executionResult.CC) { message.CC.Add(addr); }
			foreach (var addr in executionResult.Bcc) { message.Bcc.Add(addr); }

			// Options
			message.DeliveryNotificationOptions = executionResult.DeliveryNotificationOptions;
			message.Priority = executionResult.Priority;

			foreach (var hdr in executionResult.Headers) { message.Headers.Add(hdr.Key, hdr.Value); }
			//Public property HeadersEncoding Gets or sets the encoding used for the user-defined custom headers for this e-mail message. 


			message.Subject = executionResult.Subject;
			//Public property SubjectEncoding Gets or sets the encoding used for the subject content for this e-mail message. 

			foreach (var att in executionResult.Attachments) { message.Attachments.Add(att); }



			string TextBody = executionResult.Text;	// @section Text { ... }
			string HtmlBody = executionResult.Html;	// @section Html { ... }
			string PlainBody = executionResult.Body; // content not in section(s).


			if (string.IsNullOrWhiteSpace(TextBody) && string.IsNullOrWhiteSpace(HtmlBody))
			{
				// no sections defined
				message.Body = PlainBody;
				message.IsBodyHtml = false;
				return message;
			}

			// no html part
			if (string.IsNullOrWhiteSpace(executionResult.Html))
			{
				message.Body = TextBody;
				message.IsBodyHtml = false;
				return message;
			}
			
			// clean up and move css inline to maximise compatibility with mail clients
			// stripping style block, class attributes and comments
			var preMailerResult = PreMailer.Net.PreMailer.MoveCssInline(HtmlBody, removeStyleElements: true, stripIdAndClassAttributes: true, removeComments: true);
			HtmlBody = preMailerResult.Html;
						

			// html, no text
			if (string.IsNullOrWhiteSpace(executionResult.Text))
			{
				// are there any embedded images?
				// no
				message.Body = HtmlBody;
				message.IsBodyHtml = true;
				return message;

			}

			// both text and html parts

			// create text alternate part

			// create html alternate part
			// add embeded (linked) resources


			var TextPart = AlternateView.CreateAlternateViewFromString(TextBody, new System.Net.Mime.ContentType("text/plain"));
			var HtmlPart = AlternateView.CreateAlternateViewFromString(HtmlBody, new System.Net.Mime.ContentType("text/html"));

			foreach (var i in executionResult.LinkedResources.Values)
			{
				HtmlPart.LinkedResources.Add(i);
			}

			message.AlternateViews.Add(TextPart);
			message.AlternateViews.Add(HtmlPart);

			//AlternateViews Gets the attachment collection used to store alternate forms of the message body. 
			//Public property Body Gets or sets the message body. 
			//Public property IsBodyHtml Gets or sets a value indicating whether the mail message body is in Html. 

			//Public property BodyEncoding Gets or sets the encoding used to encode the message body. 
			//Public property BodyTransferEncoding Gets or sets the transfer encoding used to encode the message body. 






			return message;

		}

	}
}
