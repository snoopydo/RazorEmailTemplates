using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Postman
{
	public class PostalService
	{

		public MailMessage Render(string template)
		{
			return Render(template, null);
		}

		public MailMessage Render(string template, object model)
		{
			MailMessage result=null;




			return result;
		}

	}
}
