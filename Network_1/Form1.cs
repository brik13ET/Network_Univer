using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Network_1
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		List<string> links = new List<string>();
		int links_delta;
		HttpRequestMessage message;
		System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

		private async void responce_handler()
		{
			string doc;
			string resp_headers = "";
			HttpResponseMessage resp;
			List<string> images = new List<string>();
			while (links_delta != 0)
			{
				links_delta = 0;
				if (links[0] == "/")
					continue;

				if (links[0][0] == '/' && links[0] != "/")
				{ 
					links[0] = "http:/" + links[0];
					System.Uri uri = new Uri(links[0]);
					message = new HttpRequestMessage(HttpMethod.Get, uri);
					message.Headers.Add("Host", uri.Host);
					message.Method = HttpMethod.Get;
					message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:106.0) Gecko/20100101 Firefox/106.0");
					message.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
					message.Headers.Add("Accept-Language", "ru");
					message.Headers.Add("Accept-Encoding", "");
					links.Remove(links[0]);
				}

				try
				{
					resp = await client.SendAsync(message);
				}
				catch (SocketException e)
				{
					MessageBox.Show(e.Message);
					return;
				}
				catch (WebException e)
				{
					MessageBox.Show(e.Message);
					return;
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Undefined Exception");
					return;
				}
				doc = await resp.Content.ReadAsStringAsync();
				resp_headers = resp.Headers.ToString();

				foreach (String s in doc.Replace("><", ">\n<").Replace("\r", "").Split('\n'))
				{
					Match match;
					if (s.Contains("<img"))
						match = null;
					match = Regex.Match(s, @"(<a[\s]+.*href=[""']([a-zA-Z.-/:]+\.(bmp|jpeg|png|jpg|svg|webp))[""'].*>)");
					if (match.Success)
					{
						images.Add(match.Groups[2].Value);
					}
					match = Regex.Match(s, @"(<a[\s]+.*href=[""']([a-zA-Z.-/:]+)[""'].*>)");
					if (match.Success)
					{

						if (!links.Contains(match.Groups[2].Value))
						{
							links.Add(match.Groups[2].Value);
							links_delta++;
						}
					}
					match = Regex.Match(s, @"(<img[\s]+.*src=[""'](.+)[""']\s?.*>)");
					if (match.Success)
					{
						images.Add(match.Groups[2].Value);
					}
				}
			}
			this.Invoke
			((MethodInvoker)delegate {
				this.listBox1.Items.Clear();
				this.listBox2.Items.Clear();
				this.listBox3.Items.Clear();
				foreach (string h in resp_headers.Split('\n') )
					this.listBox3.Items.Add(h);
				foreach (string img in images)
					this.listBox2.Items.Add(img);
				foreach (var val in links)
					this.listBox1.Items.Add(val);
			});
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			links.Add(textBox1.Text);
			links_delta = 1;
			await Task.Run(responce_handler);
		}
	}
}
