using System;
using System.Reflection;
using System.Windows.Forms;

namespace OpenSupplicant
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        #region 程序集特性访问器

        public string AssemblyTitle
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        #endregion

        private void FormAbout_Load(object sender, EventArgs e)
        {
            textBoxAbout.Text = AssemblyTitle + " " + AssemblyVersion + "\r\n"
                + "网络管理协会\r\n"
                + "© 2012 NA All rights reserved.\r\n"
                + "\r\n"
                + "本软件仅限华软软件学院内学习、交流之用，禁止任何未经许可的以任何方式的发布于校外网络。";
        }

        private void buttonWebsite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://na.sise.cn/");
        }

    }
}
