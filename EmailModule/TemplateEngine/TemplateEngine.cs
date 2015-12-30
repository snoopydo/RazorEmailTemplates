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

namespace dks.Templating
{
	public class TemplateEngine
	{
		private const string NamespaceName = "_TemplateEngine";

		// cache of 'compiled' templates
		private static readonly Dictionary<string, Type> typeMapping = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		private static readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();

		internal TemplateEngine(ITemplateContentReader contentReader)
		{
			ContentReader = contentReader;
		}

		private ITemplateContentReader ContentReader { get; set; }

		protected Assembly GenerateAssembly<T>(RazorTemplateEngine razorEngine, string className, string source)
		{
			string[] referencedAssemblies = BuildReferenceList<T>().ToArray();
			//var assemblyName = NamespaceName + "." + Guid.NewGuid().ToString("N") + ".dll";

			var templateResults = razorEngine.GenerateCode(new StringReader(source), className, NamespaceName, className + ".cs");

			if (templateResults.ParserErrors.Any())
			{
				var parseExceptionMessage = string.Join(Environment.NewLine + Environment.NewLine, templateResults.ParserErrors.Select(e => e.Location + ":" + Environment.NewLine + e.Message).ToArray());

				throw new InvalidOperationException(parseExceptionMessage);
			}

			using (var codeProvider = new CSharpCodeProvider())
			{
				var compilerParameter = new CompilerParameters(referencedAssemblies)
				{
					GenerateInMemory = true,
					CompilerOptions = "/optimize"
				};

				// aid to debugging, keeps source files
				//compilerParameter.TempFiles.KeepFiles = true;

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

		private static RazorTemplateEngine CreateRazorEngine<T>(RazorCodeLanguage codeLanguage)
		{

			var host = new TemplateRazorEngineHost(codeLanguage)
			{
				// todo: should be generic template type that is passed in.
				DefaultBaseClass = typeof(T).FullName,
				DefaultNamespace = NamespaceName
			};

			host.NamespaceImports.Add("System");
			host.NamespaceImports.Add("System.Collections");
			host.NamespaceImports.Add("System.Collections.Generic");
			host.NamespaceImports.Add("System.Dynamic");
			host.NamespaceImports.Add("System.Linq");

			return new RazorTemplateEngine(host);
		}

		private static IEnumerable<string> BuildReferenceList<T>()
		{
			string currentAssemblyLocation = typeof(TemplateEngine).Assembly.CodeBase.Replace("file:///", string.Empty).Replace("/", "\\");
			string currentTemplateLocation = typeof(T).Assembly.CodeBase.Replace("file:///", string.Empty).Replace("/", "\\");

			return new List<string>
                       {
                           "mscorlib.dll",
                           "system.dll",
                           "system.core.dll",
                           "microsoft.csharp.dll",
                           currentAssemblyLocation,
						   currentTemplateLocation
                       };
		}

		private T CreateTemplateInstances<T>(string templateName)
		{
			return (T)Activator.CreateInstance(GetTemplateTypes<T>(templateName));
		}

		private Type GetTemplateTypes<T>(string templateName)
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
						templateTypes = GenerateTemplateTypes<T>(templateName);
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

		private Type GenerateTemplateTypes<T>(string templateName)
		{
			var source = ContentReader.ReadTemplate(templateName);

			var className = templateName.Replace(".", "_");

			// determine language, pass into generateAssembly
			RazorCodeLanguage codeLanguage;

			switch (templateName.Substring(templateName.LastIndexOf(".")))
			{
				case ".cshtml":
					codeLanguage = new CSharpRazorCodeLanguage();
					break;

				case ".vbhtml":
					codeLanguage = new VBRazorCodeLanguage();
					break;

				default:
					throw new ApplicationException("Unsupported Template language. Either .cshtml or .vbhtml");
			}

			var razorEngine = CreateRazorEngine<T>(codeLanguage);

			var assembly = GenerateAssembly<T>(razorEngine, className, source);

			return assembly.GetType(NamespaceName + "." + className, true, false);
		}




		// ****************************************************************************************


		public T Execute<T>(string templateName, object model) where T : TemplateBase
		{
			var template = CreateTemplateInstances<T>(templateName);

			template.SetModel(WrapModel(model));
			template.Render();

			return template;
		}

	}
}