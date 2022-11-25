using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
		Uri baseUri;
		List<Uri> localPages = new List<Uri>();
		List<Uri> deltaPages = new List<Uri>();
        List<Uri> outsidePages = new List<Uri>();
        List<Uri> outsideImg = new List<Uri>();
        List<Uri> imageLinks = new List<Uri>();
		int max_init = 500;
		int max;
		int errors = 0;

		public Form1()
		{
			InitializeComponent();
		}
		private string Get(Uri uri)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			try
			{
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			} catch (WebException ee)
			{
				return null;
			}
		}
		private void recursion()
		{
			imageLinks.Clear();
			localPages.Clear();
			outsidePages.Clear();
			int localPage_i = 0;
			max = max_init;
			errors = 0;
			while (max > 0 && ((localPage_i == 0 && localPages.Count == 0 || localPage_i < localPages.Count) && deltaPages.Count > 0) )
			{
				if (deltaPages.Count == 0)
				{
					deltaPages.Add(localPages[localPage_i]);
					localPage_i++;
				}
				for (int i = 0; i < deltaPages.Count && max > 0; i++)
				{
					var item = deltaPages[i];
					max--;
					update_progress((int)Math.Floor(100 - max * 100f / max_init));
					var doc_raw = Get(item);
					if (doc_raw == null)
					{
						errors++;
						//MessageBox.Show(item.ToString());
						continue;
					}
					var doc_prep = doc_raw.Replace("\n", "").Replace("\r", "").Replace("\t"," ");
					var doc = Regex.Replace(doc_prep, @"\s\s+", " ").Replace("><", ">\n<").Replace("> <", ">\n<").Split('\n');
					foreach (var div in doc)
					{
						Match m = Regex.Match(div, @"<img[\s:;/.a-zA-Z0-9?""=-]*src=[""']([:/.a-zA-Zа-яА-Я0-9?=-]+)[""'][\s:;/.a-zA-Z0-9?""=-]*/?>");
						if (m.Success)
						{
							var extracted_img = m.Groups[1].Value;
							if (extracted_img[0] == '/')
								extracted_img = baseUri + extracted_img.Substring(1);
							if (imageLinks.Contains(new Uri(extracted_img)))
								continue;
							if (extracted_img.Contains(baseUri.AbsoluteUri))
								imageLinks.Add(new Uri(extracted_img));
							else
								if (!outsideImg.Contains(new Uri(extracted_img)))
									outsideImg.Add(new Uri(extracted_img));
                            continue;
						}

						m = Regex.Match(div, @"<a[\s:;/.a-zA-Z0-9?""=-]*href=[""']([:/.a-zA-Z0-9?=-]+)[""'][\s:;/.a-zA-Z0-9?""=-]*/?>");
						if (!m.Success)
							continue;
						var extracted = m.Groups[1].Value;
						if (extracted.Contains(baseUri.Host) || extracted[0] == '/' || extracted[0] == '?' || !extracted.Contains("/"))
						{
							if (extracted[0] == '?' || !extracted.Contains("/"))
								extracted = baseUri + extracted;
							if (extracted[0] == '/')
								extracted = baseUri + extracted.Substring(1);

							if (localPages.Contains(new Uri(extracted)))
								continue;
							localPages.Add(new Uri(extracted));
							deltaPages.Add(new Uri(extracted));
							continue;
						}
						if (!outsidePages.Contains(new Uri(m.Groups[1].Value)))
							outsidePages.Add(new Uri(m.Groups[1].Value));
					}
					deltaPages.Remove(item);
				}
			}
			if (max != 0)
				update_progress(100);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!(sender as Button).Enabled)
				return;
			int.TryParse(maskedTextBox1.Text, out max_init);
			max = max_init;
			try
			{
				baseUri = new Uri(this.textBox1.Text);
			}
			catch (UriFormatException ee)
			{
				//MessageBox.Show(ee.Message, "BadURI");
				throw;
			}
			(sender as Button).Enabled = false;
			this.listBox1.Items.Clear();
			this.listBox2.Items.Clear();
			this.listBox3.Items.Clear();
			deltaPages.Clear();
			deltaPages.Add(baseUri);
			var t = new Task(() => recursion());
			t.Start();
		}
		private void update_progress(int percentage)
		{
			this.Invoke((MethodInvoker)delegate {
				this.Text = "delta: " + deltaPages.Count;
				this.progressBar1.Value = percentage;
				if (percentage >= 100)
					complete();
			});
		}

		private void complete()
		{
			this.Invoke((MethodInvoker)delegate {
				this.listBox1.Items.AddRange(localPages.ToArray());
				this.listBox2.Items.AddRange(imageLinks.ToArray());
                this.listBox3.Items.AddRange(outsideImg.ToArray());
				label1.Text = String.Format("Изображений: {0}   Внешних: {2}   Локальных: {1}   Err: {3}", imageLinks.Count, localPages.Count, outsideImg.Count, errors);
				this.button1.Enabled = true;
				this.Invalidate();
			});
		}
	}
}
