namespace RazorTemplates
{
    public interface IEmailTemplateContentReader
    {
        string Read(string templateName, string suffix);
    }
}