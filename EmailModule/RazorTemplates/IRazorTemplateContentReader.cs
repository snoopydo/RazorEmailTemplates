namespace RazorTemplates
{
    public interface IRazorTemplateContentReader
    {
        string Read(string templateName);
    }
}