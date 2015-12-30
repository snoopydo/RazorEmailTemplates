using System;
using System.Globalization;
using System.IO;

namespace dks.Templating
{
	internal class FileSystemTemplateContentReader : ITemplateContentReader
	{
		public FileSystemTemplateContentReader() : this("templates") { }

		public FileSystemTemplateContentReader(string templateDirectory)
		{
			if (String.IsNullOrWhiteSpace(templateDirectory))
			{
				throw new ArgumentException(string.Format("\"{0}\" cannot be null or blank.", "templateDirectory"));
			}

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
			if (String.IsNullOrWhiteSpace(templateName))
			{
				throw new ArgumentException(string.Format("\"{0}\" cannot be null or blank.", "templateName"));
			}

			var content = string.Empty;
			var path = BuildPath(templateName);

			if (File.Exists(path))
			{
				content = File.ReadAllText(path);
			}

			return content;
		}

		private string BuildPath(string templateName)
		{
			var fileName = templateName;

			var path = Path.Combine(TemplateDirectory, fileName);

			return path;
		}
	}
}