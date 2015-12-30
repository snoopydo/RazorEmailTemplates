using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Postman
{
	public class EmailTemplate : dks.Templating.TemplateBase
	{
		public DeliveryNotificationOptions DeliveryNotificationOptions;
		public MailPriority Priority;

		public MailAddress From { get; set; }
		public MailAddress Sender { get; set; }

		public MailAddressCollection To { get; private set; }

		public MailAddressCollection ReplyToList { get; private set; }
		public MailAddressCollection CC { get; private set; }
		public MailAddressCollection Bcc { get; private set; }

		public IDictionary<string, string> Headers { get; private set; }

		public string Subject { get; set; }

		public List<Attachment> Attachments { get; private set; }



		public EmailTemplate()
			: base()
		{
			
			ReplyToList = new MailAddressCollection();
			To = new MailAddressCollection();
			CC = new MailAddressCollection();
			Bcc = new MailAddressCollection();

			Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Attachments = new List<Attachment>();
			DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.None;
			Priority = MailPriority.Normal;
			LinkedResources = new Dictionary<string, LinkedResource>();
		}

		internal Dictionary<string, LinkedResource> LinkedResources { get; private set; }
		public string EmbedResource(string file)
		{
			var key = file;
			LinkedResource item = null;
			if (LinkedResources.TryGetValue(key, out item))
			{
				return "cid:" + item.ContentId;
			}


			if (!Path.IsPathRooted(file))
			{
				file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"templates", file);
			}

			ContentType contentType = new ContentType();
			contentType.MediaType = System.Web.MimeMapping.GetMimeMapping(key);
			
			item = new LinkedResource(file);
			item.ContentId = "embedded-" + LinkedResources.Count.ToString();
			item.ContentType = contentType;

			LinkedResources.Add(key, item);

			return "cid:" + item.ContentId;
		}


		//todo: should these be internal?
		public string Html { get; private set; }
		public string Text { get; private set; }
		public string Body { get; private set; }


		internal void AssembleParts()
		{
			if (base.Sections.ContainsKey("Html")) { Html = base.Sections["Html"]; }
			if (base.Sections.ContainsKey("Text")) { Text = base.Sections["Text"]; }
			Body = base.Result;
		}
	}
}
