using System.IO;

namespace dks.Templating
{
    internal interface ITemplateContentReader
    {
        string ReadTemplate(string templateName);
    }
}