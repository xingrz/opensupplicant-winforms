using System;

namespace OpenSupplicant
{
    public class AmtiumLoginFailureEventArgs : EventArgs
    {
        private string m_message;

        public string Message
        {
            get { return m_message; }
            /*set { m_message = value; }*/
        }

        public AmtiumLoginFailureEventArgs(string message)
        {
            this.m_message = message;
        }
    }
}
