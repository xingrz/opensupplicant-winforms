using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;

namespace OpenSupplicant
{
    public class EthernetsManager : Dictionary<string, EthernetInformation>
    {
        public EthernetsManager()
        {
            // 枚举所有网络接口
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 筛选出：以太网卡、无线网卡、G级以太网卡
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet)
                {
                    // 获取网卡参数
                    IPInterfaceProperties properties = adapter.GetIPProperties();

                    string name = adapter.Description;
                    string mac = adapter.GetPhysicalAddress().ToString();
                    bool dhcp = properties.GetIPv4Properties().IsDhcpEnabled;
                    string ip = null;

                    // 过滤VMware的虚拟网卡
                    if (name.Contains("VMware"))
                    {
                        continue;
                    }

                    // 筛选出第一个IPv4地址
                    foreach (UnicastIPAddressInformation ipInformation in properties.UnicastAddresses)
                    {
                        if (ipInformation.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ip = ipInformation.Address.ToString();
                            break;
                        }
                    }

                    // 加入到列表
                    if (ip != null)
                    {
                        this.Add(adapter.Id, new EthernetInformation(name, mac, dhcp, ip));
                    }
                }
            }

        }
    }
}
