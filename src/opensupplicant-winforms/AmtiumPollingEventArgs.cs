using System;

namespace OpenSupplicant
{
    public class AmtiumPollingEventArgs : EventArgs
    {
        private string m_session = null;
        private string m_ip = null;
        private string m_mac = null;
        private int m_seq = 0;

        public string Session
        {
            get { return m_session; }
            /*set { m_session = value; }*/
        }

        public string Ip
        {
            get { return m_ip; }
            /*set { m_ip = value; }*/
        }

        public string Mac
        {
            get { return m_mac; }
            /*set { m_mac = value; }*/
        }

        public int Seq
        {
            get { return m_seq; }
            /*set { m_seq = value; }*/
        }

        public AmtiumPollingEventArgs(string session, string ip, string mac, int seq)
        {
            this.m_session = session;
            this.m_ip = ip;
            this.m_mac = mac;
            this.m_seq = seq;
        }
    }
}
