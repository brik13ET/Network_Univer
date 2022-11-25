using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Network_4
{
	internal class POP3Client
	{
		TcpClient client;
		StreamReader reader;
		StreamWriter writer;

		char[] waiter = { '|', '/', '-', '\\'};

		public POP3Client(string host, int port, NetworkCredential cred)
		{
			this.client = new TcpClient();
			var t = client.ConnectAsync(Dns.GetHostAddresses(host), port);
			//t.RunSynchronously();
			int i = 0;
			while (!t.IsCompleted)
			{
				Console.Write(waiter[i % waiter.Length]);
				Console.Write('\r');
				i++;
			}
			if (t.Status == TaskStatus.Faulted || t.Status == TaskStatus.Canceled)
				throw new ArgumentException("Unable connect ot host");
			var stream = client.GetStream();
			var ssl_stream = new SslStream(stream);
			ssl_stream.AuthenticateAsClient(host, null, System.Security.Authentication.SslProtocols.Tls12, true);
			reader = new StreamReader(ssl_stream, Encoding.ASCII);
			writer = new StreamWriter(ssl_stream, Encoding.ASCII, 65536, true);
			writer.NewLine = "\r\n";
			reader.ReadLine();
			auth(cred);

		}

		void auth(NetworkCredential cred)
		{
			writer.WriteLine("USER " + cred.UserName);
			writer.Flush();
			string resp = reader.ReadLine();
			if (resp[0] == '-')
			{
				throw new NetworkInformationException(-1);
			}
			writer.WriteLine("PASS " + cred.Password);
			writer.Flush();
			string resp2 = reader.ReadLine();
			if (resp2[0] == '-')
			{
				throw new WebException(resp2);
			}
		}

		public MailMessage[] GetMessages(int from, int count = -1)
		{

			writer.Write("STAT");
			//writer.Flush();
			int cnt = -1;

			string rl = "";
			var s = client.GetStream();
			while (s.DataAvailable)
			{
				int r = s.ReadByte();
				if (r == -1 || r == '\n')
					return null;
				char c = Convert.ToChar(r);
				rl += c;
			}

			Match mcnt = Regex.Match(rl, @"^\+OK (?<cnt>\d+) \d+$");
			if (!mcnt.Success)
			{
				int.TryParse(mcnt.Groups["cnt"].Value, out cnt);
			}

			if (from >= count)
				return null;

			MailMessage[] ret = new MailMessage[count];

			int local_i = 0;

			writer.Write("LIST");
			//writer.Flush();


			string rl2 = "";
			while(!s.DataAvailable);
			while (s.DataAvailable)
			{
				int r = s.ReadByte();
				if (r == -1 || r == '\n')
					return null;
				char c = Convert.ToChar(r);
				rl2 += c;
			}

			string resp = rl2;
			if (resp[0] == '-') { return null; }
			var l = new List<string>();
			while (l.Count - 1 < from)
			{
				string rl3 = "";
				while (s.DataAvailable)
				{
					int r = s.ReadByte();
					if (r == -1 || r == '\n')
						return null;
					char c = Convert.ToChar(r);
					rl3 += c;
				}

				l.Add(rl3);
			}
			return ret;
		}

		public void Dispose()
		{
			writer.WriteLine("QUIT");
			writer.Flush();
			writer.Dispose();
			reader.Dispose();
			client.Dispose();
		}

	}
}
