using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dks.Templating
{

	public abstract class TemplateBase
	{
		public delegate void SectionWriter();

		private readonly StringBuilder buffer;
		private Dictionary<string, SectionWriter> SectionWriters = new Dictionary<string, SectionWriter>();

		protected TemplateBase()
		{
			buffer = new StringBuilder();
		}

		protected dynamic Model { get; private set; }

		public void SetModel(object model)
		{
			Model = model;
		}

		
		// Razor will override and implement this method
		public virtual  void Execute() {}
				
		protected string Result { get; private set; }
		protected Dictionary<string, string> Sections { get; private set; }

		public void Render()
		{
			Execute();
			Result = buffer.ToString();

			if (SectionWriters.Count > 0)
			{
				Sections = new Dictionary<string, string>();
				foreach (var s in SectionWriters.Keys)
				{
					buffer.Clear();
					SectionWriters[s].Invoke();
					Sections[s] = buffer.ToString();
				}
			}

		}

		protected virtual void Write(object value)
		{
			WriteLiteral(value);
		}

		protected virtual void WriteLiteral(object value)
		{
			buffer.Append(value);
		}

		protected virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
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

		protected void DefineSection(string name, SectionWriter action)
		{
			if (SectionWriters.ContainsKey(name))
			{
				throw new ApplicationException(string.Format("Section {0}, already defined.", name));
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
