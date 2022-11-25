using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_3
{
	public partial class Mail : Form
	{
		TcpClient c;
		public Mail(TcpClient c)
		{
			this.c = c;
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{

		}
	}
}
