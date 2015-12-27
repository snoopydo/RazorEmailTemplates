using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web.Razor;
using RazorTemplates;

	
// todo: fix this should be a generic template base in RazorTemplates
namespace Postman
{

	public delegate void SectionWriter();

	public abstract class EmailTemplate : IEmailTemplate
	{
		private readonly StringBuilder buffer;
		private Dictionary<string, SectionWriter> SectionWriters = new Dictionary<string, SectionWriter>();

		[DebuggerStepThrough]
		protected EmailTemplate()
		{
			To = new List<string>();
			ReplyTo = new List<string>();
			CC = new List<string>();
			Bcc = new List<string>();
			Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			buffer = new StringBuilder();
		}

		public string From { get; set; }

		public string Sender { get; set; }

		public ICollection<string> To { get; private set; }

		public ICollection<string> ReplyTo { get; private set; }

		public ICollection<string> CC { get; private set; }

		public ICollection<string> Bcc { get; private set; }

		public IDictionary<string, string> Headers { get; private set; }

		public string Subject { get; set; }

		public string HtmlBody { get; private set; }
		public string TextBody { get; private set; }
		
		protected dynamic Model { get; private set; }

		public void SetModel(dynamic model)
		{
			Model = model;
		}

		// Razor will override and implement this method
		public abstract void Execute();

		public void Render()
		{
			Execute();
			TextBody = buffer.ToString();

			if (SectionWriters.ContainsKey("Html"))
			{
				buffer.Clear();
				SectionWriters["Html"].Invoke();
				HtmlBody = buffer.ToString();
			}

			if (SectionWriters.ContainsKey("Text"))
			{
				buffer.Clear();
				SectionWriters["Text"].Invoke();
				TextBody = buffer.ToString();
			}


		}

		public virtual void Write(object value)
		{
			WriteLiteral(value);
		}

		public virtual void WriteLiteral(object value)
		{
			buffer.Append(value);
		}

		public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
		{
			bool first = true;
			bool wroteSomething = false;
			if (values.Length == 0)
			{
				// Explicitly empty attribute, so write the prefix and suffix 
				WritePositionTaggedLiteral(prefix);
				WritePositionTaggedLiteral(suffix);
			}
			else
			{
				for (int i = 0; i < values.Length; i++)
				{
					AttributeValue attrVal = values[i];
					PositionTagged<object> val = attrVal.Value;
					PositionTagged<string> next = i == values.Length - 1 ?
						suffix : // End of the list, grab the suffix 
						values[i + 1].Prefix; // Still in the list, grab the next prefix 


					bool? boolVal = null;
					if (val.Value is bool)
					{
						boolVal = (bool)val.Value;
					}


					if (val.Value != null && (boolVal == null || boolVal.Value))
					{
						string valStr = val.Value as string;
						if (valStr == null)
						{
							valStr = val.Value.ToString();
						}
						if (boolVal != null)
						{
							Debug.Assert(boolVal.Value);
							valStr = name;
						}


						if (first)
						{
							WritePositionTaggedLiteral(prefix);
							first = false;
						}
						else
						{
							WritePositionTaggedLiteral(attrVal.Prefix);
						}


						// Calculate length of the source span by the position of the next value (or suffix) 
						int sourceLength = next.Position - attrVal.Value.Position;


						if (attrVal.Literal)
						{
							WriteLiteral(valStr);
						}
						else
						{
							Write(valStr); // Write value 
						}
						wroteSomething = true;
					}
				}
				if (wroteSomething)
					WritePositionTaggedLiteral(suffix);
			}
		}

		public void DefineSection(string name, SectionWriter action)
		{
			if (SectionWriters.ContainsKey(name))
			{
				throw new ApplicationException(string.Format("Section {0}, already defined.",name)); 
			}
			SectionWriters[name] = action;
		}

		private void WritePositionTaggedLiteral(string value, int position)
		{
			if (value == null)
				return;


			WriteLiteral(value);
		}

		private void WritePositionTaggedLiteral(PositionTagged<string> value)
		{
			WritePositionTaggedLiteral(value.Value, value.Position);
		}

		
	}
}