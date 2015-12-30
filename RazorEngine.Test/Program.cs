using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Postman;
using System.Net.Mail;
using System.IO;
using dks.Templating;

namespace RazorEngine.Test
{

	public class Address
	{
		public string Street;
		public string Suburb;
		public string City;
		public int PostCode;
	}

	public class WelcomeModel
	{
		public string From;
		public string To;
		public string Name;
		public string Password;
		public string LogOnUrl;
		public Address Address;
	}


	class Program
	{



		static void Main(string[] args)
		{

			var model1 = new WelcomeModel() { From = "admin@localhost.com", To = "customer@localhost.com", Name = "Valued Customer", Password = "lkjasdf*(334", LogOnUrl = "http://www.website.com/logon/", Address = new Address() { Street = "Bla Street", Suburb = "Bl Blaburb", City = "Metroplis", PostCode = 90210 } };
			var model2 = new { From = "admin@localhost.com", To = "customer@localhost.com", Name = "Goober Shoes", Password = "lkjasdf*(334", LogOnUrl = "http://www.website.com/logon/", Address = new { Street = "Boogers Grove", Suburb = "Bl Blaburb", City = "Metroplis", PostCode = 90210 } };



			var service = new Postman.PostmanService();


			var m1 = service.Render("WelcomeMail.cshtml", model1);


			var m2 = service.Render("WelcomeMail.cshtml", model2);


			var smtpSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

			if (!Directory.Exists(smtpSavePath))
			{
				Directory.CreateDirectory(smtpSavePath);
			}


			using (var smtp = new SmtpClient())
			{

				smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				smtp.PickupDirectoryLocation = smtpSavePath;


				smtp.Send(m1);	// template with typed object for model

				smtp.Send(m2);	// template with anonymous object for model

			}

			Console.WriteLine();
			Console.WriteLine("Messages saved in {0}", smtpSavePath);
			Console.WriteLine();

			Console.WriteLine("Done, enter to exit");
			Console.ReadLine();




		}
	}
}
