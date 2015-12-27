using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;

namespace EmailModule
{
	public class TemplateRazorEngineHost : RazorEngineHost
	{
		internal const string DefineSectionMethodName = "DefineSection";
		internal const string WriteToMethodName = "WriteTo";
		internal const string WriteLiteralToMethodName = "WriteLiteralTo";
		internal const string BeginContextMethodName = "BeginContext";
		internal const string EndContextMethodName = "EndContext";


		private const string TemplateTypeName = "goober";	// used when @helper <func> {} is defined.

		private TemplateRazorEngineHost()
		{
			GeneratedClassContext = new GeneratedClassContext(GeneratedClassContext.DefaultExecuteMethodName,
															  GeneratedClassContext.DefaultWriteMethodName,
															  GeneratedClassContext.DefaultWriteLiteralMethodName,
															  WriteToMethodName,
															  WriteLiteralToMethodName,
															  TemplateTypeName,
															  DefineSectionMethodName,
															  BeginContextMethodName,
															  EndContextMethodName);

		}

		public TemplateRazorEngineHost(RazorCodeLanguage codeLanguage)
			: this()
		{
			CodeLanguage = codeLanguage;	
		}

		 public override ParserBase CreateMarkupParser() 
         { 
             return new HtmlMarkupParser(); 
         } 

	}
}
