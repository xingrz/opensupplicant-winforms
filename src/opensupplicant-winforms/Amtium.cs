using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenSupplicant
{
    public class Amtium
    {
        #region 委托
        public delegate void AmtiumLoginSuccessEventHandler(object sender, AmtiumLoginSuccessEventArgs e);
        public delegate void AmtiumLoginFailureEventHandler(object sender, AmtiumLoginFailureEventArgs e);
        public delegate void AmtiumPollingEventHandler(object sender, AmtiumPollingEventArgs e);
        public delegate void AmtiumPolledEventHandler(object sender, AmtiumPolledEventArgs e);
        public delegate void AmtiumDisconnectedEventHandler(object sender, AmtiumDisconnectedEventArgs e);
        public delegate void AmtiumGotServicesEventHandler(object sender, AmtiumGotServicesEventArgs e);
        public delegate void AmtiumGotServerIpEventHandler(object sender, AmtiumGotServerIpEventArgs e);

        private delegate void UdpSend();
        private delegate void UdpListen();
        #endregion

        #region 事件
        public event AmtiumLoginSuccessEventHandler LoginSuccess;
        public event AmtiumLoginFailureEventHandler LoginFailure;
        public event AmtiumPollingEventHandler Polling;
        public event AmtiumPolledEventHandler Polled;
        public event AmtiumDisconnectedEventHandler Disconnected;
        public event EventHandler Logouted;
        public event AmtiumGotServicesEventHandler GotServices;
        public event AmtiumGotServerIpEventHandler GotServerIp;
        #endregion
        
        #region 私有变量
        private SynchronizationContext mainThreadContext;

        private string m_ip = null;
        private string m_mac = null;
        private string m_server = null;
        private bool m_dhcp = false;
        private string m_version = null;

        private IPEndPoint m_endPoint3850;
        private IPEndPoint m_endPoint3848;
        private IPEndPoint m_endPoint4999;
        private UdpClient m_client4999;
        private UdpClient m_client3848;

        private Timer m_timer;
        private string m_sessionId = null;
        private int m_seq = 0;

        private AmtiumStatus m_status;
        #endregion

        #region 公有属性
        /// <summary>
        /// 客户IP
        /// </summary>
        public string Ip
        {
            get { return m_ip; }
            set { if (m_sessionId == null) m_ip = value; }
        }

        /// <summary>
        /// 客户MAC
        /// </summary>
        public string Mac
        {
            get { return m_mac; }
            set { if (m_sessionId == null) m_mac = value; }
        }

        /// <summary>
        /// 认证服务器
        /// </summary>
        public string Server
        {
            get { return m_server; }
            set
            {
                if (m_sessionId == null)
                {
                    m_server = value;
                    m_endPoint3848 = new IPEndPoint(IPAddress.Parse(value), 3848);
                }
            }
        }

        /// <summary>
        /// 是否已启用DHCP
        /// </summary>
        public bool Dhcp
        {
            get { return m_dhcp; }
            set { if (m_sessionId == null) m_dhcp = value; }
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline
        {
            get { return m_sessionId != null; }
        }

        /// <summary>
        /// 会话
        /// </summary>
        public string SessionId
        {
            get { return m_sessionId; }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public AmtiumStatus Status
        {
            get { return m_status; }
            set { m_status = value; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 创建一个安腾协议对象
        /// </summary>
        /// <param name="ip">客户IP</param>
        /// <param name="mac">客户MAC</param>
        /// <param name="server">认证服务器</param>
        /// <param name="dhcp">是否已启用DHCP</param>
        /// <param name="version">协议版本</param>
        public Amtium(string ip, string mac, string server, bool dhcp, string version)
        {
            mainThreadContext = SynchronizationContext.Current;

            this.m_ip = ip;
            this.m_mac = mac;
            this.m_server = server;
            this.m_dhcp = false;
            this.m_version = version;

            m_endPoint3850 = new IPEndPoint(IPAddress.Parse("1.1.1.8"), 3850);

            if (server != null)
            {
                m_endPoint3848 = new IPEndPoint(IPAddress.Parse(server), 3848);
                m_endPoint4999 = new IPEndPoint(IPAddress.Parse(server), 4999);
            }

            try
            {
                m_client4999 = new UdpClient(4999);
                m_client3848 = new UdpClient(3848);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    throw new AmtiumException();
                }
            }

            /* 监听报文返回 */
            new UdpListen(() =>
            {
                try
                {
                    while (m_client3848 != null)
                    {
                        byte[] bytes = m_client3848.Receive(ref m_endPoint3848);
                        unpack(bytes);
                    }
                }
                catch { }
            }).BeginInvoke(null, null);

            /* 监听服务器消息 */
            new UdpListen(() =>
            {
                try
                {
                    while (m_client4999 != null)
                    {
                        byte[] bytes = m_client4999.Receive(ref m_endPoint4999);
                        unpack(bytes);
                    }
                }
                catch { }
            }).BeginInvoke(null, null);

            /* 初始化状态包定时器 */
            m_timer = new Timer(new TimerCallback(Timer_Callback), null, Timeout.Infinite, 30000);
            
            /* 发送初始化请求 */
            byte[] buf = Protocal.Initialize();
            new UdpSend(() =>
            {
                m_client4999.Send(buf, buf.Length, m_endPoint3850);
            }).BeginInvoke(null, null);

            /* 完成初始化 */
            m_status = AmtiumStatus.Initialized;
        }

        /// <summary>
        /// 创建一个安腾协议对象
        /// </summary>
        /// <param name="ip">客户IP</param>
        /// <param name="mac">客户MAC</param>
        /// <param name="server">认证服务器</param>
        public Amtium(string ip, string mac, string server)
            : this(ip, mac, server, false, "3.6.5")
        {
        }

        /// <summary>
        /// 创建一个安腾协议对象
        /// </summary>
        /// <param name="ip">客户IP</param>
        /// <param name="mac">客户MAC</param>
        /// <param name="server">认证服务器</param>
        /// <param name="dhcp">是否已启用DHCP</param>
        public Amtium(string ip, string mac, string server, bool dhcp)
            : this(ip, mac, server, dhcp, "3.6.5")
        {
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            m_status = AmtiumStatus.Closing;
            m_client4999.Close();
            m_client3848.Close();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">账号</param>
        /// <param name="password">密码</param>
        /// <param name="service">接入服务</param>
        public void Login(string username, string password, string service)
        {
            if (this.m_endPoint3848 == null) return;

            m_status = AmtiumStatus.LoggingIn;

            byte[] buf = Protocal.Login(this.m_server, this.m_mac, username, password, this.m_ip, service, this.m_dhcp, this.m_version);
            new UdpSend(() =>
            {
                m_client3848.Send(buf, buf.Length, m_endPoint3848);
            }).BeginInvoke(null, null);
        }

        /// <summary>
        /// 发送状态
        /// </summary>
        public void Poll()
        {
            if (this.m_endPoint3848 == null) return;

            m_status = AmtiumStatus.Polling;
            OnPolling(new AmtiumPollingEventArgs(this.m_sessionId, this.m_ip, this.m_mac, this.m_seq));

            byte[] buf = Protocal.Poll(this.m_server, this.m_sessionId, this.m_ip, this.m_mac, this.m_seq);
            new UdpSend(() =>
            {
                m_client3848.Send(buf, buf.Length, m_endPoint3848);
            }).BeginInvoke(null, null);
        }

        /// <summary>
        /// 注销
        /// </summary>
        public void Logout()
        {
            if (this.m_endPoint3848 == null) return;

            m_status = AmtiumStatus.LoggingOut;
            m_timer.Change(Timeout.Infinite, 30000);

            byte[] buf = Protocal.Logout(this.m_server, this.m_sessionId, this.m_ip, this.m_mac, this.m_seq);
            new UdpSend(() =>
            {
                m_client3848.Send(buf, buf.Length, m_endPoint3848);
            }).BeginInvoke(null, null);

            this.m_sessionId = null;
        }

        /// <summary>
        /// 获取接入服务列表
        /// </summary>
        public void GetServices()
        {
            if (this.m_endPoint3848 == null) return;

            m_status = AmtiumStatus.RecivingServices;

            byte[] buf = Protocal.GetServices(m_server, m_mac);
            new UdpSend(() =>
            {
                m_client3848.Send(buf, buf.Length, m_endPoint3848);
            }).BeginInvoke(null, null);
        }

        /// <summary>
        /// 获取认证服务器
        /// </summary>
        public void GetServerIp()
        {
            m_status = AmtiumStatus.RecivingServerIp;

            byte[] buf = Protocal.GetServerIp(m_ip, m_mac);
            new UdpSend(() =>
            {
                m_client3848.Send(buf, buf.Length, m_endPoint3850);
            }).BeginInvoke(null, null);
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 解析返回数据
        /// </summary>
        /// <param name="buf">返回字节流</param>
        private void unpack(byte[] buf)
        {
            Dictionary<byte, List<byte[]>> result = ProtocalUtil.ParseResponseBody(Crypto3848.decrypt(buf));

            switch (result[ProtocalUtil.FIELD_ACTION][0][0])
            {
                case ProtocalUtil.ACTION_LOGIN_RET:     // 登录返回
                    if (result[ProtocalUtil.FIELD_SUCCESS][0][0] == 1)
                    {
                        m_sessionId = Convert.BytesToString(result[ProtocalUtil.FIELD_SESSION][0]);
                        m_timer.Change(30000, 30000);

                        m_status = AmtiumStatus.Free;

                        OnLoginSuccess(new AmtiumLoginSuccessEventArgs(
                            m_sessionId,
                            Convert.BytesToString(result[ProtocalUtil.FIELD_MESSAGE][0]),
                            Convert.BytesToString(result[ProtocalUtil.FIELD_WEBSITE][0])
                            ));
                    }
                    else
                    {
                        m_status = AmtiumStatus.Free;

                        OnLoginFailure(new AmtiumLoginFailureEventArgs(
                            Convert.BytesToString(result[ProtocalUtil.FIELD_MESSAGE][0])
                            ));
                    }
                    break;
                case ProtocalUtil.ACTION_POLL_RET:      // 状态返回
                    // 递增序号+3
                    int seq = Convert.BytesToInt(result[ProtocalUtil.FIELD_SEQ][0]) - 0x01000000;
                    m_seq = seq + 3;

                    m_status = AmtiumStatus.Free;

                    OnPolled(new AmtiumPolledEventArgs(
                        result[ProtocalUtil.FIELD_SUCCESS][0][0] == 1,
                        Convert.BytesToString(result[ProtocalUtil.FIELD_SESSION][0]),
                        seq
                        ));
                    break;
                case ProtocalUtil.ACTION_LOGOUT_RET:    // 注销返回
                    m_status = AmtiumStatus.Free;

                    OnLogouted(new EventArgs());
                    break;
                case ProtocalUtil.ACTION_SERVICE_RET:   // 获取接入服务返回
                    List<string> services = new List<string>();
                    foreach (byte[] name in result[ProtocalUtil.FIELD_SERVICE])
                    {
                        services.Add(Convert.BytesToString(name));
                    }

                    m_status = AmtiumStatus.Free;

                    OnGotServices(new AmtiumGotServicesEventArgs(services.ToArray()));
                    break;
                case ProtocalUtil.ACTION_DISCONNECT:    // 中断连接
                    Logout();

                    m_status = AmtiumStatus.Disconnected;

                    OnDisconnected(new AmtiumDisconnectedEventArgs(
                        Convert.BytesToString(result[ProtocalUtil.FIELD_SESSION][0]),
                        (short)result[ProtocalUtil.FIELD_REASON][0][0]
                        ));
                    break;
                /*case ProtocalUtil.ACTION_CONFIRM_RET: // 旧协议，已废弃
                    break;*/
                case ProtocalUtil.ACTION_SERVER_RET:    // 获取服务器返回
                    byte[] server = result[ProtocalUtil.FIELD_SERVER][0];
                    string ip = server[0] + "." + server[1] + "." + server[2] + "." + server[3];

                    Server = ip;

                    m_status = AmtiumStatus.Free;

                    OnGotServerIp(new AmtiumGotServerIpEventArgs(ip));
                    break;
            }
        }

        private void Timer_Callback(object state)
        {
            Poll();
        }
        #endregion

        #region 事件触发
        protected virtual void OnLoginSuccess(AmtiumLoginSuccessEventArgs e)
        {
            if (LoginSuccess != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    LoginSuccess(sender, e);
                }), this);
            }
        }

        protected virtual void OnLoginFailure(AmtiumLoginFailureEventArgs e)
        {
            if (LoginFailure != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    LoginFailure(sender, e);
                }), this);
            }
        }

        protected virtual void OnPolling(AmtiumPollingEventArgs e)
        {
            if (Polling != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    Polling(sender, e);
                }), this);
            }
        }

        protected virtual void OnPolled(AmtiumPolledEventArgs e)
        {
            if (Polled != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    Polled(sender, e);
                }), this);
            }
        }

        protected virtual void OnDisconnected(AmtiumDisconnectedEventArgs e)
        {
            if (Disconnected != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    Disconnected(sender, e);
                }), this);
            }
        }

        protected virtual void OnLogouted(EventArgs e)
        {
            if (Logouted != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    Logouted(sender, e);
                }), this);
            }
        }

        protected virtual void OnGotServices(AmtiumGotServicesEventArgs e)
        {
            if (GotServices != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    GotServices(sender, e);
                }), this);
            }
        }

        protected virtual void OnGotServerIp(AmtiumGotServerIpEventArgs e)
        {
            if (GotServerIp != null)
            {
                mainThreadContext.Post(new SendOrPostCallback((object sender) =>
                {
                    GotServerIp(sender, e);
                }), this);
            }
        }
        #endregion
    }
}
