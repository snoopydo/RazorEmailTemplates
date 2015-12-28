using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorTemplates;
using EmailModule;

namespace RazorEngine.Test
{

	class Address
	{
		public string Street;
		public string Suburb;
		public string City;
		public int PostCode;
	}

	class WelcomeModel
	{
		public string From;
		public string To;
		public string Name;
		public string Password;
		public string LogOnUrl;
		public Address Address;
	}


	

	//	public void Execute()
	//	{
	//	}
	//}

	class Program
	{
		static void Main(string[] args)
		{

			var model = new WelcomeModel()
			{
				From = "admin@localhost.com",
				To = "customer@localhost.com",
				Name = "Valued Customer",
				Password = "lkjasdf*(334",
				LogOnUrl = "http://www.website.com/logon/",
				Address = new Address()
				{
					Street = "Bla Street",
					Suburb = "Bl Blaburb",
					City = "Metroplis",
					PostCode = 90210
				}

			};

			var e = new TemplateEngine(new FileSystemTemplateContentReader());

			var r = e.Execute<EmailTemplate>("WelcomeMail.cshtml", model);

			Console.WriteLine(r.Html);

			model.Name = "Goober Shoes";
			model.Address.Street = "Boogers Grove";
			var r2 = e.Execute<EmailTemplate>("WelcomeMail.cshtml", model);

			Console.WriteLine(r2.Html);


			Console.ReadLine();

		}
	}
}
