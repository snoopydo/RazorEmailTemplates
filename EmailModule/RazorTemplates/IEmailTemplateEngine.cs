namespace RazorTemplates
{
    public interface IEmailTemplateEngine
    {
		// todo: fix this, should be generic base template.
        Postman.Email Execute(string templateName, object model = null);
    }
}