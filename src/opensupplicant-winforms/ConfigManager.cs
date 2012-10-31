using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace OpenSupplicant
{
    public class ConfigManager
    {
        private string m_server = null;         // 认证服务器
        private string m_service = null;        // 接入服务
        private string m_ethernetId = null;     // 网卡ID
        private string m_username = "";         // 账号
        private string m_password = "";         // 密码
        private bool m_autoLogin = false;       // 自动登录
        private bool m_autoUpdate = true;       // 自动更新
        private bool m_isInDevChannel = false;  // 更新到开发版
        private string[] m_services = new string[0];    // 服务列表
        private string m_manualIp = null;             // 手动指定IP

        public string Server
        {
            get { return m_server; }
            set { m_server = value; }
        }

        public string Service
        {
            get { return m_service; }
            set { m_service = value; }
        }

        public string EthernetId
        {
            get { return m_ethernetId; }
            set { m_ethernetId = value; }
        }

        public string Username
        {
            get { return m_username; }
            set { m_username = value; }
        }

        public string Password
        {
            get { return m_password; }
            set { m_password = value; }
        }

        public bool AutoLogin
        {
            get { return m_autoLogin; }
            set { m_autoLogin = value; }
        }

        public bool AutoUpdate
        {
            get { return m_autoUpdate; }
            set { m_autoUpdate = value; }
        }

        public bool IsInDevChannel
        {
            get { return m_isInDevChannel; }
            set { m_isInDevChannel = value; }
        }

        public string[] Services
        {
            get { return m_services; }
            set { m_services = value; }
        }

        public string ManualIp
        {
            get { return m_manualIp; }
            set { m_manualIp = value; }
        }

        public ConfigManager()
        {
            Properties.Settings defaults = Properties.Settings.Default;

            if (!String.IsNullOrEmpty(defaults.Server)) m_server = defaults.Server;
            if (!String.IsNullOrEmpty(defaults.Service)) m_service = defaults.Service;
            if (!String.IsNullOrEmpty(defaults.EthernetId)) m_ethernetId = defaults.EthernetId;
            if (!String.IsNullOrEmpty(defaults.ManualIp)) m_manualIp = defaults.ManualIp;

            m_autoLogin = defaults.AutoLogin && !String.IsNullOrEmpty(defaults.Username) && !String.IsNullOrEmpty(defaults.Password);
            m_autoUpdate = defaults.AutoUpdate;
            m_isInDevChannel = defaults.IsInDevChannel;

            if (defaults.Services != null)
            {
                m_services = new string[defaults.Services.Count];
                defaults.Services.CopyTo(m_services, 0);
            }

            m_username = defaults.Username;

            if (!String.IsNullOrEmpty(defaults.Username) && !String.IsNullOrEmpty(defaults.Password))
            {
                m_password = Convert.BytesToString(Crypto3849.decrypt(Convert.HexToBytes(defaults.Password)));
            }

            Save();
        }

        public void Save()
        {
            Properties.Settings defaults = Properties.Settings.Default;

            defaults.Server = (m_server == null) ? "" : m_server;
            defaults.Service = (m_service == null) ? "" : m_service;
            defaults.EthernetId = (m_ethernetId == null) ? "" : m_ethernetId;
            defaults.ManualIp = (m_manualIp == null) ? "" : m_manualIp;

            defaults.AutoLogin = m_autoLogin;
            defaults.AutoUpdate = m_autoUpdate;
            defaults.IsInDevChannel = m_isInDevChannel;

            if (m_services.Length == 0)
            {
                defaults.Services = null;
            }
            else
            {
                defaults.Services = new StringCollection();
                defaults.Services.AddRange(m_services);
            }

            defaults.Username = m_username;

            if (String.IsNullOrEmpty(m_username) || String.IsNullOrEmpty(m_password))
            {
                defaults.Password = "";
            }
            else
            {
                defaults.Password = Convert.BytesToHex(Crypto3849.encrypt(Convert.StringToBytes(m_password)));
            }

            defaults.Save();
        }

        public static void Clear()
        {
            Properties.Settings.Default.Reset();
        }
    }
}
