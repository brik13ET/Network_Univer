using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Network_2
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}

		List<string> files = new List<string>();
		List<string> dirs = new List<string>();
		string host, login, pass;
		private void doReq()
		{
			FtpWebRequest req = (FtpWebRequest)WebRequest.Create("ftp://" + host);
			req.Credentials = new NetworkCredential(login, pass);
			req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

			var root = new TreeNode(host);

			var c = (new StreamReader(req.GetResponse().GetResponseStream())).ReadToEnd().Replace("\r", "").Split('\n');
			Match m;
			foreach (var i in c)
			{
				m = Regex.Match(i, @"^d[rwx-]{3,9}([\s\t]+\d+){4}\s\w{3}\s\d\d\s+[\d:]+\s(.*)$");
				if (m.Success)
				{
					var _ = new TreeNode(m.Groups[2].Value);
					_.Tag = m.Groups[2].Value;
					root.Nodes.Add(_);
					dirs.Add("/" + m.Groups[2].Value);
				}
				m = Regex.Match(i, @"^[rwx-]{3,9}([\s\t]+\d+){4}\s\w{3}\s\d\d\s+[\d:]+\s(.*)$");
				if (m.Success)
					files.Add(m.Groups[2].Value);
			}
			string pwd;

			for (int i = 0; i  < dirs.Count - 1; i ++)
			{
				if (dirs[i].Length == 0)
				{
					dirs.Remove(dirs[i]);
					i--;
					continue;
				}

				pwd = dirs[i];
				req = (FtpWebRequest)WebRequest.Create("ftp://" + host + pwd);
				req.Credentials = new NetworkCredential(login, pass);
				req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				var tmp =
					(
						new StreamReader(
							req
								.GetResponse()
								.GetResponseStream()
						)
					)
						.ReadToEnd()
						.Replace("\r", "")
						.Split('\n');
				foreach (var item in tmp)
				{
					m = Regex.Match(item, @"^d[rwx-]{3,9}([\s\t]+\d+){4}\s\w{3}\s\d\d\s+[\d:]+\s(.*)$");
					if (m.Success)
					{
						dirs.Add(pwd + "/" + m.Groups[2].Value);
						TreeNode n = root;
						var s_pwd = pwd.Substring(1).Split('/');
						for (int j = 0; n != null && j < s_pwd.Length; j++)
						{
							n = n.FirstNode;
							while (n != null && n.Text != s_pwd[j])
								n = n.NextNode;
						}
						var _ = new TreeNode(m.Groups[2].Value);
						_.Tag = m.Groups[2].Value;
						n.Nodes.Add(_);
					}
					m = Regex.Match(item, @"^-[rwx-]{3,9}([\s\t]+\d+){4}\s\w{3}\s\d\d\s+[\d:]+\s(.*)$");
					if (m.Success)
						files.Add(pwd + "/" + m.Groups[2].Value);
				}
			}

			this.Invoke((MethodInvoker)delegate
			{
				Dictionary<string, int> cnt = new Dictionary<string, int>();
				dataGridView1.Rows.Clear();
				int sum = 0;
				foreach (string f in files)
				{
					req = (FtpWebRequest)WebRequest.Create("ftp://" + host + f);
					req.Credentials = new NetworkCredential(login, pass);
					req.Method = WebRequestMethods.Ftp.GetFileSize;
					var f_siz = req.GetResponse().ContentLength;
					m = Regex.Match(f, @"^/(.+/)*[\w\s.-]+(?<extt>(\.[a-z]+){1,2})$");
					if (!m.Success)
						break;
					var ext = m.Groups["extt"].Value;
					if (cnt.ContainsKey(ext))
						cnt[ext]+=(int)f_siz;
					else
						cnt.Add(ext, (int)f_siz);
					sum += (int)f_siz;
					dataGridView1.Rows.Add(f, ext, f_siz);
				}
				dataGridView1.Rows.Add("", "", "");
				foreach (KeyValuePair<string,int> pa in cnt)
				{
					dataGridView1.Rows.Add("*", pa.Key, pa.Value);
				}
				dataGridView1.Rows.Add("*", "*", sum);

				foreach (var item in root.Nodes)
					treeView2.Nodes.Add(item as TreeNode);
				listBox1.Items.AddRange(files.ToArray());
				files.Clear();
				dirs.Clear();
				button1.Enabled = true;
			});
		}

		private void button1_Click(object sender, EventArgs e)
		{
			host = textBox1.Text;
			login = textBox2.Text;
			pass = textBox3.Text;
			Task t = new Task(doReq);
			t.Start();
			if (sender is Button)
				(sender as Button).Enabled = false;
		}
	}
}
