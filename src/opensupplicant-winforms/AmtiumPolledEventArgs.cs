using System;

namespace OpenSupplicant
{
    public class AmtiumPolledEventArgs : EventArgs
    {
        private bool m_isSuccess = false;
        private string m_session = null;
        private int m_seq = 0;

        public bool IsSuccess
        {
            get { return m_isSuccess; }
            /*set { m_isSuccess = value; }*/
        }

        public string Session
        {
            get { return m_session; }
            /*set { m_session = value; }*/
        }

        public int Seq
        {
            get { return m_seq; }
            /*set { m_seq = value; }*/
        }

        public AmtiumPolledEventArgs(bool isSuccess, string session, int seq)
        {
            this.m_isSuccess = isSuccess;
            this.m_session = session;
            this.m_seq = seq;
        }
    }
}
