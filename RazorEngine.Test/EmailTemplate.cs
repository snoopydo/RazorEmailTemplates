using RazorTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailModule
{
	public class EmailTemplate : TemplateBase
	{

		// Standard Email Properties
		public string From { get; set; }
		public string Sender { get; set; }

		public ICollection<string> To { get; private set; }

		public ICollection<string> ReplyTo { get; private set; }

		public ICollection<string> CC { get; private set; }

		public ICollection<string> Bcc { get; private set; }

		public IDictionary<string, string> Headers { get; private set; }

		public string Subject { get; set; }

		public string Html { get; private set; }
		public string Text { get; private set; }
		public string Body { get; private set; }

		
		public string EmbedResource(string file)
		{
			// todo: locate and read resource, use linked resource for html alternative part
			return file;
		}
	

		public EmailTemplate()
			: base()
		{
			To = new List<string>();
			ReplyTo = new List<string>();
			CC = new List<string>();
			Bcc = new List<string>();
			Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		}

		


		// gotta have empty sub to keep things happy, actual Razor engine implements this function.
		public override void Execute() { }

		protected override void PopulateTemplate()
		{

			if (base.Sections.ContainsKey("Html")) { Html = base.Sections["Html"]; }
			if (base.Sections.ContainsKey("Text")) { Text = base.Sections["Text"]; }
			Body = base.Result;


			// use PreMailer.Net to fix up Html.
			// parse ? images and embed them>

		}
	}
}
