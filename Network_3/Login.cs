using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_3
{
	public partial class Login : Form
	{
		public Login()
		{
			InitializeComponent();
		}

		string host;
		int port = 25;
		string user;
		string pass;
		int timeout = 3000;
		private void MailLogin()
		{
			TcpClient c = new TcpClient();
			c.ReceiveTimeout = timeout;
			Stopwatch w = new Stopwatch();
			w.Start();

			var t = c.ConnectAsync(Dns.GetHostAddresses(host), port);
			while (!t.IsCompleted && w.ElapsedMilliseconds < timeout) ;
			if (w.ElapsedMilliseconds > timeout)
			{
				MessageBox.Show("Reached timeout. Check connection to server");
				return;
			}
			w.Stop();
			if (!c.Connected)
			{

				this.Invoke((MethodInvoker)delegate
				{
					button1.Enabled = true;
					toolStripStatusLabel1.Text = "Connection failed " + string.Format("{0} {1}ms", host, w.ElapsedMilliseconds);
				});
				return;
			}

			var helo = ASCIIEncoding.ASCII.GetBytes("HELO\r\n");

			c.GetStream().Write(helo, 0, helo.Length);


			Write write_from = null;
			try
			{
				write_from = new Write(user, c);
			}catch (ArgumentException e)
			{
				MessageBox.Show(e.Message);
			}
			finally
			{

				this.Invoke((MethodInvoker)delegate {
					this.Hide();
				});
				write_from.Show();
				while (!write_from.wanna_close) Application.DoEvents();
			}

			this.Invoke((MethodInvoker)delegate {
				this.Show();
				button1.Enabled = true;
			});
		}

		private void button1_Click(object sender, EventArgs e)
		{ 
			var m = Regex.Match(serverBox.Text, @"^(http(s?)://)?(?<host>[\w.-]+)(:(?<port>[0-9]*))$");
			if (!m.Success)
			{
				MessageBox.Show("Bad server. Check format");
			}
			host = m.Groups["host"].Value;
			if (m.Groups["port"].Success)
				int.TryParse(m.Groups["port"].Value, out port);

			user = loginBox.Text;
			pass = passBox.Text;

			var t = new Task(MailLogin);
			t.Start();
			(sender as Button).Enabled = false;
		}
	}
}
