using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.IO;

namespace OpenSupplicant
{
    public partial class FormLogin : Form
    {
        #region 私有变量
        private EthernetsManager m_ethernets = null;    // 网卡管理器
        private ConfigManager m_config = null;          // 配置管理器
        private string m_website = null;                // 自助服务网址
        private bool m_isFirstLaunch = false;           // 是否首次启动
        private bool m_isClosing = false;
        #endregion

        #region 公有变量
        public Amtium amtium;
        public bool clean = false;
        #endregion

        #region 公有属性
        /// <summary>
        /// 接入服务列表
        /// </summary>
        public string[] Services
        {
            get { return m_config.Services; }
        }

        /// <summary>
        /// 网卡管理器
        /// </summary>
        public EthernetsManager Ethernets
        {
            get { return m_ethernets; }
        }

        /// <summary>
        /// 接入服务
        /// </summary>
        public string Service
        {
            get { return m_config.Service; }
            set { m_config.Service = value; }
        }

        /// <summary>
        /// 认证服务器
        /// </summary>
        public string Server
        {
            get { return m_config.Server; }
            set
            {
                m_config.Server = value;
                if (amtium != null) amtium.Server = value;
            }
        }

        /// <summary>
        /// 客户IP
        /// </summary>
        public string Ip
        {
            get { return String.IsNullOrEmpty(m_config.ManualIp) ? m_ethernets[m_config.EthernetId].Ip : m_config.ManualIp; }
        }

        /// <summary>
        /// 客户MAC
        /// </summary>
        public string Mac
        {
            get { return m_ethernets[m_config.EthernetId].Mac; }
        }

        /// <summary>
        /// 客户DHCP是否已启用
        /// </summary>
        public bool Dhcp
        {
            get { return String.IsNullOrEmpty(m_config.ManualIp) ? m_ethernets[m_config.EthernetId].Dhcp : false; }
        }

        /// <summary>
        /// 网卡ID
        /// </summary>
        public string EthernetId
        {
            get { return m_config.EthernetId; }
            set
            {
                if (m_ethernets.ContainsKey(value))
                {
                    m_config.EthernetId = value;
                    if (amtium != null)
                    {
                        amtium.Ip = Ip;
                        amtium.Mac = Mac;
                        amtium.Dhcp = Dhcp;
                    }
                }
            }
        }

        public string ManualIp
        {
            get { return m_config.ManualIp; }
            set
            {
                m_config.ManualIp = value;
                if (amtium != null)
                {
                    amtium.Ip = Ip;
                }
            }
        }

        /// <summary>
        /// 是否首次启动
        /// </summary>
        public bool IsFirstLaunch
        {
            get { return m_isFirstLaunch; }
        }

        /// <summary>
        /// 是否开启自动更新
        /// </summary>
        public bool AutoUpdate
        {
            get { return m_config.AutoUpdate; }
            set { m_config.AutoUpdate = value; }
        }

        /// <summary>
        /// 是否允许更新到开发版
        /// </summary>
        public bool IsInDevChannel
        {
            get { return m_config.IsInDevChannel; }
            set { m_config.IsInDevChannel = value; }
        }
        #endregion

        #region 窗口构造函数
        public FormLogin()
        {
            /* 读取配置 */
            m_config = new ConfigManager();

            /* 初始化网卡设置 */
            m_ethernets = new EthernetsManager();
            if (EthernetId == null || !Ethernets.ContainsKey(EthernetId))
            {
                // 如果没有网卡设置或设置不存在，改为第一块网卡
                EthernetId = Ethernets.First().Key;

                // 需要显示选项窗口
                m_isFirstLaunch = true;
            }

            InitializeComponent();
        }
        #endregion

        #region 窗口控件事件
        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormLogin_Load(object sender, EventArgs e)
        {
            /* 初始化 */
            try
            {
                this.amtium = new Amtium(Ip, Mac, Server, Dhcp);
            }
            catch (AmtiumException)
            {
                MessageBox.Show("通讯端口被占用，可能是已经运行了一个蝴蝶了…", "初始化错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            amtium.LoginSuccess += amtium_LoginSuccess;
            amtium.LoginFailure += amtium_LoginFailure;
            amtium.Disconnected += amtium_Disconnected;
            amtium.GotServerIp += amtium_GotServerIp;
            amtium.GotServices += amtium_GotServices;
            amtium.Logouted += amtium_Logouted;

            /* 初始化控件状态 */
            textBoxUser.Text = m_config.Username;
            textBoxPswd.Text = m_config.Password;
            checkBoxRemind.Checked = (m_config.Password.Length > 0);
            checkBoxAutologin.Checked = m_config.AutoLogin;

            if (String.IsNullOrEmpty(textBoxUser.Text))
            {
                textBoxUser.Text = "请输入您的账号...";
                textBoxUser.ForeColor = SystemColors.GrayText;
            }

            if (String.IsNullOrEmpty(textBoxPswd.Text))
            {
                textBoxPswd.Text = "请输入您的密码...";
                textBoxPswd.ForeColor = SystemColors.GrayText;
                textBoxPswd.PasswordChar = Char.MinValue;
            }

            /* 获取认证服务器和认证服务 */
            if (m_config.Server == null)        // 如果未获取服务器
            {
                this.Show();
                DisableControls();
                GetServer();
                return;
            }
            else if (m_config.Services.Length == 0)    // 如果未获取接入服务
            {
                this.Show();
                DisableControls();
                GetServices();
                return;
            }

            /* 显示选项窗口 */
            if (m_isFirstLaunch)
            {
                new FormOptions(this).ShowDialog();
                return;
            }

            /* 自动登录 */
            if (checkBoxAutologin.Checked)
            {
                DisableControls();
                Login();
            }
        }

        /// <summary>
        /// 窗口即将关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存配置
            if (!clean) SaveConfig();

            if (m_isClosing) return;

            // 如果在线
            if (this.amtium != null && this.amtium.IsOnline)
            {
                // 先取消，完成了再关闭
                e.Cancel = true;

                // 如果是用户关闭
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    // 则询问是否退出，如果否则取消操作
                    if (MessageBox.Show("您确定要注销并退出吗？", "操作确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                // 消除托盘图标
                notifyIcon1.Dispose();

                // 注销
                m_isClosing = true;
                Logout();
            }
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormLogin_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && amtium.IsOnline)
            {
                HideForm();
            }
        }

        /// <summary>
        /// 按钮「登录」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (amtium.IsOnline) return;

            if (textBoxUser.Text.Length == 0)
            {
                MessageBox.Show("请输入账号", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBoxUser.Focus();
                return;
            }

            if (textBoxPswd.Text.Length == 0)
            {
                MessageBox.Show("请输入密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBoxPswd.Focus();
                return;
            }

            DisableControls();
            SaveConfig();
            Login();
        }

        /// <summary>
        /// 按钮「注销」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLogout_Click(object sender, EventArgs e)
        {
            AskLogout();
        }

        /// <summary>
        /// 按钮「选项」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOptions_Click(object sender, EventArgs e)
        {
            new FormOptions(this).ShowDialog();
        }

        /// <summary>
        /// 按钮「关于」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAbout_Click(object sender, EventArgs e)
        {
            new FormAbout().ShowDialog();
        }

        private void textBoxUser_Enter(object sender, EventArgs e)
        {
            if (textBoxUser.ForeColor == SystemColors.GrayText)
            {
                textBoxUser.Text = String.Empty;
                textBoxUser.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBoxUser_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxUser.Text))
            {
                textBoxUser.Text = "请输入您的账号...";
                textBoxUser.ForeColor = SystemColors.GrayText;
            }
        }

        private void textBoxPswd_Enter(object sender, EventArgs e)
        {
            if (textBoxPswd.ForeColor == SystemColors.GrayText)
            {
                textBoxPswd.Text = String.Empty;
                textBoxPswd.ForeColor = SystemColors.WindowText;
                textBoxPswd.PasswordChar = '*';
            }
        }

        private void textBoxPswd_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxPswd.Text))
            {
                textBoxPswd.Text = "请输入您的密码...";
                textBoxPswd.ForeColor = SystemColors.GrayText;
                textBoxPswd.PasswordChar = Char.MinValue;
            }
        }

        /// <summary>
        /// 左键单击托盘图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowForm();
            }
        }

        /// <summary>
        /// 菜单「注销并退出」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 菜单「注销」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemLogout_Click(object sender, EventArgs e)
        {
            AskLogout();
        }

        /// <summary>
        /// 菜单「显示主窗口」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemShow_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        /// <summary>
        /// 菜单「自助服务」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemWebsite_Click(object sender, EventArgs e)
        {
            if (this.m_website != null) System.Diagnostics.Process.Start(this.m_website);
        }

        /// <summary>
        /// 菜单「关于」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            new FormAbout().ShowDialog();
        }

        /// <summary>
        /// 复选框「自动登录」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutologin_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutologin.Checked)
            {
                checkBoxRemind.Checked = true;
            }
        }

        /// <summary>
        /// 复选框「记住密码」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxRemind_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxRemind.Checked)
            {
                checkBoxAutologin.Checked = false;
            }
        }

        /// <summary>
        /// 操作超时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            labelStatus.Text = "";
            timer1.Stop();

            switch (amtium.Status)
            {
                case AmtiumStatus.LoggingIn:
                    if (MessageBox.Show("登录超时，可能是网络连接出问题了…", "操作超时", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                    {
                        Login();
                    }
                    else
                    {
                        buttonLogin.Enabled = true;
                        EnableControls();
                    }
                    break;
                case AmtiumStatus.LoggingOut:
                    if (m_isClosing)
                    {
                        amtium.Dispose();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("注销超时", "操作超时", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        buttonLogout.Enabled = true;
                    }
                    break;
                case AmtiumStatus.Polling:
                    // 这个不用管。。。
                    break;
                case AmtiumStatus.RecivingServerIp:
                    if (MessageBox.Show("获取服务器IP超时了，可能是网络连接出了问题…", "操作超时", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                    {
                        GetServer();
                    }
                    else
                    {
                        Application.Exit();
                    }
                    break;
                case AmtiumStatus.RecivingServices:
                    if (MessageBox.Show("获取接入服务超时了，可能是网络连接出了问题…", "操作超时", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                    {
                        GetServices();
                    }
                    else
                    {
                        Application.Exit();
                    }
                    break;
                default:
                    // 没事
                    break;
            }
        }
        #endregion

        #region Amtium事件
        /// <summary>
        /// 登录成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amtium_LoginSuccess(object sender, AmtiumLoginSuccessEventArgs e)
        {
            labelStatus.Text = "";
            timer1.Stop();
            buttonLogin.Enabled = true;
            buttonLogin.Visible = false;
            buttonLogout.Visible = true;
            HideForm();

            notifyIcon1.Text = "当前账号: " + textBoxUser.Text;
            notifyIcon1.ShowBalloonTip(1000, "登录成功", e.Message, ToolTipIcon.Info);

            if (e.Website.Length > 0)
            {
                this.m_website = e.Website;
                toolStripMenuItemWebsite.Enabled = true;
            }
            else
            {
                this.m_website = null;
                toolStripMenuItemWebsite.Enabled = false;
            }
        }

        /// <summary>
        /// 登录失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amtium_LoginFailure(object sender, AmtiumLoginFailureEventArgs e)
        {
            labelStatus.Text = "";
            timer1.Stop();
            buttonLogin.Enabled = true;
            EnableControls();
            MessageBox.Show(e.Message, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amtium_Logouted(object sender, EventArgs e)
        {
            labelStatus.Text = "";
            timer1.Stop();
            if (m_isClosing)
            {
                amtium.Dispose();
                this.Close();
            }
            else
            {
                EnableControls();
                buttonLogout.Enabled = true;
                buttonLogout.Visible = false;
                buttonLogin.Visible = true;
                ShowForm();
            }
        }

        /// <summary>
        /// 连接中断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amtium_Disconnected(object sender, AmtiumDisconnectedEventArgs e)
        {
            switch (e.Reason)
            {
                case 0:
                    //MessageBox.Show("在线状态维持失败，请重新登录。", "强制下线", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    /* 禽兽蝴蝶 */
                    Login();
                    break;
                case 1:
                    MessageBox.Show("您已被强制下线！", "强制下线", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 2:
                    MessageBox.Show("您的流量已用完。", "强制下线", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
                default:
                    MessageBox.Show("当前账号被强制下线，原因代码：" + e.Reason, "强制下线", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
            }
        }

        /// <summary>
        /// 获取到接入服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amtium_GotServices(object sender, AmtiumGotServicesEventArgs e)
        {
            labelStatus.Text = "";
            timer1.Stop();
            m_config.Services = e.Services;
            EnableControls();

            /* 肯定要显示选项窗口 */
            new FormOptions(this).ShowDialog();
        }

        /// <summary>
        /// 获取到服务器IP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void amtium_GotServerIp(object sender, AmtiumGotServerIpEventArgs e)
        {
            labelStatus.Text = "";
            timer1.Stop();
            m_config.Server = e.Server;

            /* 如果还没获取接入服务 */
            if (this.Services.Length == 0)
            {
                GetServices();
                return;
            }

            /* 显示选项窗口 */
            if (m_isFirstLaunch)
            {
                EnableControls();
                new FormOptions(this).ShowDialog();
                return;
            }

            /* 自动登录 */
            if (checkBoxAutologin.Checked)
            {
                Login();
                return;
            }

            EnableControls();
        }
        #endregion

        #region 操作
        private void Login()
        {
            buttonLogin.Enabled = false;
            labelStatus.Text = "正在登录...";
            timer1.Start();
            amtium.Login(textBoxUser.Text, textBoxPswd.Text, m_config.Service);
        }

        private void Logout()
        {
            buttonLogout.Enabled = false;
            labelStatus.Text = "正在注销...";
            timer1.Start();
            amtium.Logout();
        }

        private void GetServices()
        {
            labelStatus.Text = "正在获取接入服务...";
            timer1.Start();
            amtium.GetServices();
        }

        private void GetServer()
        {
            labelStatus.Text = "正在获取认证服务器...";
            timer1.Start();
            amtium.GetServerIp();
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 询问是否注销
        /// </summary>
        private void AskLogout()
        {
            DialogResult result = MessageBox.Show("您确定要注销吗？", "操作确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Logout();
            }
        }

        /// <summary>
        /// 启用控件
        /// </summary>
        private void EnableControls()
        {
            textBoxUser.Enabled = true;
            textBoxPswd.Enabled = true;
            checkBoxRemind.Enabled = true;
            checkBoxAutologin.Enabled = true;
            buttonOptions.Enabled = true;
            textBoxUser.Focus();
        }

        /// <summary>
        /// 禁用控件
        /// </summary>
        private void DisableControls()
        {
            textBoxUser.Enabled = false;
            textBoxPswd.Enabled = false;
            checkBoxRemind.Enabled = false;
            checkBoxAutologin.Enabled = false;
            buttonOptions.Enabled = false;
        }

        /// <summary>
        /// 显示主窗口
        /// </summary>
        private void ShowForm()
        {
            notifyIcon1.Visible = false;
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            if (amtium.IsOnline)
            {
                buttonLogout.Focus();
            }
        }

        /// <summary>
        /// 隐藏主窗口
        /// </summary>
        private void HideForm()
        {
            notifyIcon1.Visible = true;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void SaveConfig()
        {
            m_config.Username = textBoxUser.Text;
            m_config.Password = checkBoxRemind.Checked ? textBoxPswd.Text : "";
            m_config.AutoLogin = checkBoxAutologin.Checked;
            m_config.Save();
        }
        #endregion
    }
}
