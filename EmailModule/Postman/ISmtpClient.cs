using System;
using System.Net.Mail;

namespace Postman
{
	public interface ISmtpClient : IDisposable
	{
		void Send(MailMessage message);
	}
}