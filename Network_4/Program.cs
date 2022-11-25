using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Network_4
{
	internal class Program
	{
		static SmtpClient client;

		static bool SendMail()
		{
			var account = ((NetworkCredential)client.Credentials).UserName;
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

				client.Send(mail);
			}
			catch (SmtpException e)
			{
				if (e.StatusCode == SmtpStatusCode.BadCommandSequence)
					Console.WriteLine("Войти не удалось, попробуйте ещё раз");
				else
					Console.WriteLine(e.StatusCode.ToString());
				return true;
			}
			return false;
		}
		static void Main(string[] args)
		{
			string smpt_host;
			int smtp_port = 25;
			string pop3_host;
			int pop3_port = 110;
			string account;
			var pass = "";

			{ 
				Console.Write("SMTP Host: ");

				var m = Regex.Match(Console.ReadLine(), @"^(http(s?)://)?(?<host>[\w.-]+)(:(?<port>[0-9]*))?$");
				if (!m.Success)
				{
					Console.WriteLine("Bad Host. Press any key to exit");
					Console.ReadKey();
					return;
				}
				smpt_host = m.Groups["host"].Value;
				if (m.Groups["port"].Success)
					int.TryParse(m.Groups["port"].Value, out smtp_port);
			}
			{
				Console.Write("POP3 Host: ");
				var m = Regex.Match(Console.ReadLine(), @"^(http(s?)://)?(?<host>[\w.-]+)(:(?<port>[0-9]*))?$");
				if (!m.Success)
				{
					Console.WriteLine("Bad Host. Press any key to exit");
					Console.ReadKey();
					return;
				}
				pop3_host = m.Groups["host"].Value;
				if (m.Groups["port"].Success)
					int.TryParse(m.Groups["port"].Value, out pop3_port);
			}

			Console.Write("Account: ");
			account = Console.ReadLine();

			Console.Write("Password: ");
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
			var cred = new NetworkCredential(account, pass, smpt_host);

			POP3Client pop3 = new POP3Client(pop3_host, pop3_port, cred);

			client = new SmtpClient(smpt_host, smtp_port);
			client.Credentials = cred;
			client.EnableSsl = true;

			var lmsg = pop3.GetMessages(0, 10);
			Console.WriteLine(
				"Press `Insert` to write message.\r\n" +
				"To move between pages press `Left` & `Right`\r\n" +
				"To open message press number of message\r\n" +
				"To update press `U`\r\n" +
				"To quit press `Q`\r\n"
			);
			int page = 0;
			bool quit = false;
			while (!quit)
			{
				var key = Console.ReadKey(true);
				Console.Clear();
				switch (key.Key)
				{
					case ConsoleKey.LeftArrow:
						if (page > 10)
							page--;
						lmsg = pop3.GetMessages(page, 10);
						break;
					case ConsoleKey.RightArrow:
						if (page < lmsg.Length - 10)
							page++;
						lmsg = pop3.GetMessages(page, 10);
						break;
					case ConsoleKey.U:
						lmsg = pop3.GetMessages(page, 10);
						break;
					case ConsoleKey.Insert:
						if (SendMail())
							continue;
						break;
					case ConsoleKey.Q:
						{
							quit = true ;
							break;
						}
					case ConsoleKey.D0:
					case ConsoleKey.D1:
					case ConsoleKey.D2:
					case ConsoleKey.D3:
					case ConsoleKey.D4:
					case ConsoleKey.D5:
					case ConsoleKey.D6:
					case ConsoleKey.D7:
					case ConsoleKey.D8:
					case ConsoleKey.D9:
						break;
					case ConsoleKey.F1:
						Console.Clear();
						for (int i = page * 10; i < lmsg.Length && i < (page + 1) * 10; i++)
						{
							Console.WriteLine(
								string.Format(
									"{0:0} {1}\t{2}\t\n\r",
									i % 10, lmsg[i].Subject, lmsg[i].From
								)
							);
						}
						continue;
					default:
						break;
				}

			}
			client.Dispose();
			Console.WriteLine("Bye!");

		}
	}
}
