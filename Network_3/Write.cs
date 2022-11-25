using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_3
{
	public partial class Write : Form
	{
		string from;
		TcpClient c;
		StreamReader rd;

		public bool wanna_close = false;

		public Write(string from, TcpClient c)
		{
			this.c = c;
			rd = new StreamReader(c.GetStream());

			this.from = from;
			InitializeComponent();
			this.textBox1.Text = from;
			if (!c.Connected)
			{
				throw new ArgumentException("Client not connected");
			}
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}

		private void Write_Load(object sender, EventArgs e)
		{

		}

		private void button1_Click(object sender, EventArgs e)
		{
			string msg = string.Format(
				"MAIL FROM: <{0}>\r\n" + 
				"RCPT TO: <{1}>\r\n" +
				"DATA\r\n" +
				"From: Student <{0}>\r\n" +
				"To: Recv <{1}>\r\n" +
				"Subject: {2}\r\n" +
				"Content-type: text/plain\r\n\r\n" +
				"{3}\r\n.",
				from, textBox2.Text, textBox3.Text, richTextBox1.Text
			);
			var buf = ASCIIEncoding.ASCII.GetBytes(msg);
			c.GetStream().Write(buf, 0, buf.Length);
			MessageBox.Show("SND OK" + rd.ReadLine());
		}

		private void Write_FormClosing(object sender, FormClosingEventArgs e)
		{
			wanna_close = true;
		}
	}
}
