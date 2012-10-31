using System;

namespace OpenSupplicant
{
    public class AmtiumDisconnectedEventArgs : EventArgs
    {
        private string m_session = null;
        private short m_reason = 0;

        public string Session
        {
            get { return m_session; }
            /*set { m_session = value; }*/
        }

        public short Reason
        {
            get { return m_reason; }
            /*set { m_reason = value; }*/
        }

        public AmtiumDisconnectedEventArgs(string session, short reason)
        {
            this.m_session = session;
            this.m_reason = reason;
        }
    }
}
