using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace OpenSupplicant
{
    public partial class FormOptions : Form
    {
        private FormLogin m_formLogin;
        private List<string> m_ethernetIds = new List<string>();
        //private bool m_isLoaded = false;

        public FormOptions(FormLogin owner)
        {
            m_formLogin = owner;
            InitializeComponent();
        }

        private void FormOptions_Load(object sender, EventArgs e)
        {
            /* 认证服务器 */
            textBoxServer.Text = m_formLogin.Server;

            /* 接入服务 */
            if (m_formLogin.Service == null) m_formLogin.Service = m_formLogin.Services[0];
            comboBoxServices.Items.AddRange(m_formLogin.Services);
            comboBoxServices.Text = m_formLogin.Service;

            /* 网卡 */
            foreach (KeyValuePair<string, EthernetInformation> ethernet in m_formLogin.Ethernets)
            {
                m_ethernetIds.Add(ethernet.Key);
                //comboBoxEthernets.Items.Add(ethernet.Value.Ip + " (" + formatMac(ethernet.Value.Mac) + (ethernet.Value.Dhcp ? ", DHCP" : "") + ")");
                comboBoxEthernets.Items.Add(ethernet.Value.Name);
            }

            string ethernetId = m_formLogin.EthernetId;

            // 如果是首次启动，尝试自动匹配最可能的网卡
            if (m_formLogin.IsFirstLaunch)
            {
                string[] serverIpSplit = m_formLogin.Server.Split('.');
                foreach (KeyValuePair<string, EthernetInformation> ethernet in m_formLogin.Ethernets)
                {
                    string[] clientIpSplit = ethernet.Value.Ip.Split('.');
                    if (serverIpSplit[0] == clientIpSplit[0] && serverIpSplit[1] == clientIpSplit[1])
                    {
                        ethernetId = ethernet.Key;
                        break;
                    }
                }
            }

            comboBoxEthernets.SelectedIndex = m_ethernetIds.IndexOf(ethernetId);

            /* IP */
            if (String.IsNullOrEmpty(m_formLogin.ManualIp))
            {
                checkBoxIsIpAuto.Checked = true;
                textBoxManualIp.Text = m_formLogin.Ethernets[ethernetId].Ip;
                textBoxManualIp.Enabled = false;
            }
            else
            {
                checkBoxIsIpAuto.Checked = false;
                textBoxManualIp.Text = m_formLogin.ManualIp;
                textBoxManualIp.Enabled = true;
            }

            /* 是否自动更新 */
            //checkBoxAutoUpdate.Checked = checkBoxIsInDevChannel.Enabled = m_formLogin.AutoUpdate;

            /* 是否更新到开发版 */
            //checkBoxIsInDevChannel.Checked = m_formLogin.IsInDevChannel;

            //m_isLoaded = true;
        }

        private void comboBoxEthernets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkBoxIsIpAuto.Checked)
            {
                textBoxManualIp.Text = m_formLogin.Ethernets[m_ethernetIds[comboBoxEthernets.SelectedIndex]].Ip;
            }
        }

        private void checkBoxIsIpAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxIsIpAuto.Checked)
            {
                textBoxManualIp.Text = m_formLogin.Ethernets[m_ethernetIds[comboBoxEthernets.SelectedIndex]].Ip;
                textBoxManualIp.Enabled = false;
            }
            else
            {
                textBoxManualIp.Text = m_formLogin.ManualIp;
                textBoxManualIp.Enabled = true;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            m_formLogin.Server = textBoxServer.Text;
            m_formLogin.Service = comboBoxServices.Text;
            m_formLogin.EthernetId = m_ethernetIds[comboBoxEthernets.SelectedIndex];
            m_formLogin.ManualIp = checkBoxIsIpAuto.Checked ? null : textBoxManualIp.Text;
            //m_formLogin.AutoUpdate = checkBoxAutoUpdate.Checked;
            //m_formLogin.IsInDevChannel = checkBoxIsInDevChannel.Checked;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否清除所有设置？", "清除设置", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ConfigManager.Clear();
                m_formLogin.clean = true;
                MessageBox.Show("清除完毕，请重新运行本程序", "清除设置", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private string formatMac(string mac)
        {
            string ret = "";
            for (int i = 0; i < mac.Length; i += 2)
            {
                ret += mac.Substring(i, 2) + "-";
            }
            return ret.Substring(0, ret.Length - 1);
        }

        /*private void checkBoxAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxIsInDevChannel.Enabled = checkBoxAutoUpdate.Checked;
            if (!checkBoxAutoUpdate.Checked) checkBoxIsInDevChannel.Checked = false;
        }

        private void checkBoxIsInDevChannel_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxIsInDevChannel.Checked && m_isLoaded)
            {
                if (MessageBox.Show("使用开发版通常能抢先体验新增的功能，但也可能出现不稳定的现象。\n您确定更新到开发版吗？", "更新到开发版", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    checkBoxIsInDevChannel.Checked = false;
                }
            }
        }*/
    }
}
