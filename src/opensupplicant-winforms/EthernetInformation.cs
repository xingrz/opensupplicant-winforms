using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSupplicant
{
    public class EthernetInformation
    {
        private string m_name;
        private string m_mac;
        private bool m_dhcp;
        private string m_ip;

        public EthernetInformation(string name, string mac, bool dhcp, string ip)
        {
            this.m_name = name;
            this.m_mac = mac.Replace("-", "").Replace(":", "");
            this.m_dhcp = dhcp;
            this.m_ip = ip;
        }

        public string Name
        {
            get { return m_name; }
        }

        public string Mac
        {
            get { return m_mac; }
        }

        public bool Dhcp
        {
            get { return m_dhcp; }
        }

        public string Ip
        {
            get { return m_ip; }
        }
    }
}
