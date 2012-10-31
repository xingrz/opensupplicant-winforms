using System;

namespace OpenSupplicant
{
    public class AmtiumGotServerIpEventArgs : EventArgs
    {
        private string m_server = null;

        public string Server
        {
            get { return m_server; }
            /*set { m_server = value; }*/
        }

        public AmtiumGotServerIpEventArgs(string server)
        {
            this.m_server = server;
        }
    }
}
