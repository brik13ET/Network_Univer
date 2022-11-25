using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Network_3
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string host;
			string account;
			int port = 25;
			Console.Write("Host: ");

			var m = Regex.Match(Console.ReadLine(), @"^(http(s?)://)?(?<host>[\w.-]+)(:(?<port>[0-9]*))?$");
			if (!m.Success)
			{
				Console.WriteLine("Bad Host. Press any key to exit");
				Console.ReadKey();
				return;
			}
			host = m.Groups["host"].Value;
			if (m.Groups["port"].Success)
				int.TryParse(m.Groups["port"].Value, out port);

			Console.Write("Account: ");
			account = Console.ReadLine();

			Console.Write("Password: ");
			var pass = "";
			while (true)
			{
				var key = System.Console.ReadKey(true);
				if (key.Key == ConsoleKey.Enter)
					break;
				else if (key.Key == ConsoleKey.Delete)
					pass = "";
				else if (key.Key == ConsoleKey.Backspace)
					pass = pass.Substring(0, pass.Length - 1);
				pass += key.KeyChar;
			}
			Console.WriteLine();


			SmtpClient c = new SmtpClient(host, port);
			c.Credentials = new NetworkCredential(account, pass, host);
			c.EnableSsl = true;

			while (true)
			{
				Console.WriteLine("From: " + account);
				var from = account;

				Console.Write("To: ");
				var to = Console.ReadLine();

				Console.Write("Subject: ");
				var subj = Console.ReadLine();

				Console.WriteLine("msg: ");

				var msg = "";
				var d = "";
				while (!d.Equals("."))
				{
					d = Console.ReadLine();
					msg += d + "\r\n";
				}
				List<string> list = new List<string>();
				while (true)
				{
					Console.WriteLine("Add Files ? [Enter - Yes; Other - No]: ");
					var key = System.Console.ReadKey(true);
					if (key.Key == ConsoleKey.Enter)
					{
						var path = Console.ReadLine();
						if (System.IO.File.Exists(path))
							list.Add(path);
						else
							Console.WriteLine("File did not exists");
					}
					else
						break;
				}

				MailMessage mail = new MailMessage();
				mail.From = new MailAddress(from);
				mail.CC.Add(to);
				mail.Subject = subj;
				mail.Body = msg;
				foreach (var item in list)
				{
					Attachment at = new Attachment(item);
					mail.Attachments.Add(at);
				}
				try
				{

					c.Send(mail);
				}
				catch (SmtpException e)
				{
					if (e.StatusCode == SmtpStatusCode.BadCommandSequence)
						Console.WriteLine("Войти не удалось, попробуйте ещё раз");
					else
						Console.WriteLine(e.StatusCode.ToString());
					continue;
				}

				if (Console.ReadKey().Key != ConsoleKey.Enter)
					break;

			}
			Console.WriteLine("Bye!");

		}
	}
}
