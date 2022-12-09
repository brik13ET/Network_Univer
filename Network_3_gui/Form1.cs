﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Network_3_gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> list = new List<string>();

        private void button1_Click(object sender, EventArgs e)
        {
            var m = Regex.Match(hostBox.Text, @"^(?<host>[\w.-]+)(:(?<port>[0-9]*))?$");
            if (!m.Success)
            {
                MessageBox.Show("Bad Host. Press any key to exit");
                return;
            }
            string host = m.Groups["host"].Value;
            int port = 25;
            if (m.Groups["port"].Success)
                int.TryParse(m.Groups["port"].Value, out port);

            SmtpClient c = new SmtpClient(host, port);
            c.Credentials = new NetworkCredential(accountBox.Text, passwordBox.Text, host);
            c.EnableSsl = true;
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(accountBox.Text);
            mail.CC.Add(recvBox.Text);
            mail.Subject = subjBox.Text;
            mail.Body = msgBox.Text;

            foreach(var item in list)
            {
                Attachment at = new Attachment(item);
                mail.Attachments.Add(at);
            }
            try
            {
                c.Send(mail);
            }
            catch (SmtpException ee)
            {
                if (ee.StatusCode == SmtpStatusCode.BadCommandSequence )
                    MessageBox.Show("Войти не удалось, попробуйте ещё раз");
                else
                    MessageBox.Show(ee.StatusCode.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                list.Add(openFileDialog1.FileName);
        }
    }
}