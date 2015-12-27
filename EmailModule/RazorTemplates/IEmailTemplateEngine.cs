namespace EmailModule
{
    public interface IEmailTemplateEngine
    {
        Email Execute(string templateName, object model = null);
    }
}