using System;

namespace OpenSupplicant
{
    public class AmtiumGotServicesEventArgs : EventArgs
    {
        private string[] m_services;

        public string[] Services
        {
            get { return m_services; }
            /*set { m_services = value; }*/
        }

        public AmtiumGotServicesEventArgs(string[] services)
        {
            this.m_services = services;
        }
    }
}
