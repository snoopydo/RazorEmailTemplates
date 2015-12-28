using System.IO;
namespace RazorTemplates
{
    public interface ITemplateContentReader
    {
        string ReadTemplate(string templateName);
		Stream ReadResource(string resourceName);
    }
}