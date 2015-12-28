using System;
using System.Globalization;
using System.IO;
using Utils;

namespace RazorTemplates
{
	public class FileSystemTemplateContentReader : ITemplateContentReader
	{
		public FileSystemTemplateContentReader() : this("templates") { }

		public FileSystemTemplateContentReader(string templateDirectory)
		{
			Invariant.IsNotBlank(templateDirectory, "templateDirectory");

			if (!Path.IsPathRooted(templateDirectory))
			{
				templateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templateDirectory);
			}

			if (!Directory.Exists(templateDirectory))
			{
				throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, "\"{0}\" does not exist.", templateDirectory));
			}

			TemplateDirectory = templateDirectory;
		}

		protected string TemplateDirectory { get; private set; }

		public string ReadTemplate(string templateName)
		{
			Invariant.IsNotBlank(templateName, "templateName");

			var content = string.Empty;
			var path = BuildPath(templateName);

			if (File.Exists(path))
			{
				content = File.ReadAllText(path);
			}

			return content;
		}

		public Stream ReadResource(string resourceName)
		{
			return new MemoryStream();
		}

		private string BuildPath(string templateName)
		{
			var fileName = templateName;

			var path = Path.Combine(TemplateDirectory, fileName);

			return path;
		}
	}
}