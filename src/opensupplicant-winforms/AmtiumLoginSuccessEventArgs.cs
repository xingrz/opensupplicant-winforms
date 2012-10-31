using System;

namespace OpenSupplicant
{
    public class AmtiumLoginSuccessEventArgs : EventArgs
    {
        private string m_session = null;
        private string m_message = null;
        private string m_website = null;

        public string Session
        {
            get { return m_session; }
        }

        public string Message
        {
            get { return m_message; }
        }

        public string Website
        {
            get { return m_website; }
        }

        public AmtiumLoginSuccessEventArgs(string session, string message, string website)
        {
            this.m_session = session;
            this.m_message = message;
            this.m_website = website;
        }
    }
}
