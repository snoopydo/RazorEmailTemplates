namespace EmailModule
{
	using System;
	using System.CodeDom.Compiler;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using System.Web.Razor;
	using Microsoft.CSharp;

	public class EmailTemplateEngine : IEmailTemplateEngine
	{
		private const string NamespaceName = "_TemplateEngine";

		private static readonly Dictionary<string, Type> typeMapping = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		private static readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();

		private static readonly string[] referencedAssemblies = BuildReferenceList().ToArray();
		private static readonly RazorTemplateEngine razorEngine = CreateRazorEngine();

		public EmailTemplateEngine(IEmailTemplateContentReader contentReader)
		{
			ContentReader = contentReader;
		}

		protected IEmailTemplateContentReader ContentReader { get; private set; }

		public virtual Email Execute(string templateName, object model = null)
		{
			Invariant.IsNotBlank(templateName, "templateName");

			var template = CreateTemplateInstances(templateName);

			template.SetModel(WrapModel(model));
			template.Render();

			var mail = new Email();

			template.To.Each(email => mail.To.Add(email));

			template.ReplyTo.Each(email => mail.ReplyTo.Add(email));

			template.Bcc.Each(email => mail.Bcc.Add(email));

			template.CC.Each(email => mail.CC.Add(email));


			if (!string.IsNullOrWhiteSpace(template.From))
			{
				mail.From = template.From;
			}

			if (!string.IsNullOrWhiteSpace(template.Sender))
			{
				mail.Sender = template.Sender;
			}

			if (!string.IsNullOrWhiteSpace(template.Subject))
			{
				mail.Subject = template.Subject;
			}

			mail.HtmlBody = template.HtmlBody;
			mail.TextBody = template.TextBody;

			template.Headers.Each(pair => mail.Headers[pair.Key] = pair.Value);

			return mail;
		}

		protected virtual Assembly GenerateAssembly(string templateName, string source)
		{
			var assemblyName = NamespaceName + "." + Guid.NewGuid().ToString("N") + ".dll";

			var templateResults = razorEngine.GenerateCode(new StringReader(source), templateName, NamespaceName, templateName + ".cs");

			if (templateResults.ParserErrors.Any())
			{
				var parseExceptionMessage = string.Join(Environment.NewLine + Environment.NewLine, templateResults.ParserErrors.Select(e => e.Location + ":" + Environment.NewLine + e.Message).ToArray());

				throw new InvalidOperationException(parseExceptionMessage);
			}

			using (var codeProvider = new CSharpCodeProvider())
			{
				var compilerParameter = new CompilerParameters(referencedAssemblies, assemblyName, false)
				{
					GenerateInMemory = true,
					CompilerOptions = "/optimize"
				};

				compilerParameter.TempFiles.KeepFiles = true;

				var compilerResults = codeProvider.CompileAssemblyFromDom(compilerParameter, templateResults.GeneratedCode);

				if (compilerResults.Errors.HasErrors)
				{
					var compileExceptionMessage = string.Join(Environment.NewLine + Environment.NewLine, compilerResults.Errors.OfType<CompilerError>().Where(ce => !ce.IsWarning).Select(e => e.FileName + ":" + Environment.NewLine + e.ErrorText).ToArray());

					throw new InvalidOperationException(compileExceptionMessage);
				}

				return compilerResults.CompiledAssembly;
			}
		}

		protected virtual dynamic WrapModel(object model)
		{
			if (model == null)
			{
				return null;
			}

			if (model is IDynamicMetaObjectProvider)
			{
				return model;
			}

			return new RazorDynamicObject() { Model = model };
		}

		private static RazorTemplateEngine CreateRazorEngine()
		{
			var host = new TemplateRazorEngineHost(new CSharpRazorCodeLanguage())
			{
				DefaultBaseClass = typeof(EmailTemplate).FullName,
				DefaultNamespace = NamespaceName
			};


			host.NamespaceImports.Add("System");
			host.NamespaceImports.Add("System.Collections");
			host.NamespaceImports.Add("System.Collections.Generic");
			host.NamespaceImports.Add("System.Dynamic");
			host.NamespaceImports.Add("System.Linq");

			return new RazorTemplateEngine(host);
		}

		private static IEnumerable<string> BuildReferenceList()
		{
			string currentAssemblyLocation = typeof(EmailTemplateEngine).Assembly.CodeBase.Replace("file:///", string.Empty).Replace("/", "\\");

			return new List<string>
                       {
                           "mscorlib.dll",
                           "system.dll",
                           "system.core.dll",
                           "microsoft.csharp.dll",
                           currentAssemblyLocation
                       };
		}

		private IEmailTemplate CreateTemplateInstances(string templateName)
		{
			return (IEmailTemplate)Activator.CreateInstance(GetTemplateTypes(templateName));
		}

		private Type GetTemplateTypes(string templateName)
		{
			Type templateTypes;

			syncLock.EnterUpgradeableReadLock();

			try
			{
				if (!typeMapping.TryGetValue(templateName, out templateTypes))
				{
					syncLock.EnterWriteLock();

					try
					{
						templateTypes = GenerateTemplateTypes(templateName);
						typeMapping.Add(templateName, templateTypes);
					}
					finally
					{
						syncLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				syncLock.ExitUpgradeableReadLock();
			}

			return templateTypes;
		}

		private Type GenerateTemplateTypes(string templateName)
		{
			var source = ContentReader.Read(templateName, "");

			var assembly = GenerateAssembly(templateName, source);

			return assembly.GetType(NamespaceName + "." + templateName, true, false);
		}
	}
}