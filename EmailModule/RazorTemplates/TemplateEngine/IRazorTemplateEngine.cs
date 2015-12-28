namespace RazorTemplates
{
    public interface ITemplateEnginex
    {
		// todo: fix this, should be generic base template.
        Postman.Email Execute(string templateName, object model = null);
    }
}