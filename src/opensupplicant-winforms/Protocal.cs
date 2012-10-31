using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace OpenSupplicant
{
    class Protocal
    {
        public static byte[] Initialize()
        {
            return Convert.StringToBytes("info sock ini");
        }

        public static byte[] Login(string server, string mac, string user, string password, string ip, string service, bool dhcp, string version)
        {
            Dictionary<byte, byte[]> p = new Dictionary<byte, byte[]>();
            p.Add(ProtocalUtil.FIELD_ACTION, new byte[] { ProtocalUtil.ACTION_LOGIN });
            p.Add(ProtocalUtil.FIELD_MAC, Convert.HexToBytes(mac));
            p.Add(ProtocalUtil.FIELD_USER, Convert.StringToBytes(user));
            p.Add(ProtocalUtil.FIELD_PSWD, Convert.StringToBytes(password));
            p.Add(ProtocalUtil.FIELD_IP, Convert.StringToBytes(ip));
            p.Add(ProtocalUtil.FIELD_SERVICE, Convert.StringToBytes(service));
            p.Add(ProtocalUtil.FIELD_DHCP, dhcp ? new byte[] { 1 } : new byte[] { 0 });
            p.Add(ProtocalUtil.FIELD_VERSION, Convert.StringToBytes(version));

            return Crypto3848.encrypt(ProtocalUtil.BuildRequestBody(p));
        }

        public static byte[] Poll(string server, string session, string ip, string mac, int seq)
        {
            Dictionary<byte, byte[]> p = new Dictionary<byte, byte[]>();
            p.Add(ProtocalUtil.FIELD_ACTION, new byte[] { ProtocalUtil.ACTION_POLL });
            p.Add(ProtocalUtil.FIELD_SESSION, Convert.StringToBytes(session));
            p.Add(ProtocalUtil.FIELD_IP, Convert.StringToBytes(ip, 16));
            p.Add(ProtocalUtil.FIELD_MAC, Convert.HexToBytes(mac));
            p.Add(ProtocalUtil.FIELD_SEQ, Convert.IntToBytes(0x01000000 + seq));
            p.Add(ProtocalUtil.FIELD_BLOCK2A, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2B, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2C, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2D, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2E, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2F, Convert.IntToBytes(0));

            return Crypto3848.encrypt(ProtocalUtil.BuildRequestBody(p));
        }

        public static byte[] Logout(string server, string session, string ip, string mac, int seq)
        {
            Dictionary<byte, byte[]> p = new Dictionary<byte, byte[]>();
            p.Add(ProtocalUtil.FIELD_ACTION, new byte[] { ProtocalUtil.ACTION_LOGOUT });
            p.Add(ProtocalUtil.FIELD_SESSION, Convert.StringToBytes(session));
            p.Add(ProtocalUtil.FIELD_IP, Convert.StringToBytes(ip, 16));
            p.Add(ProtocalUtil.FIELD_MAC, Convert.HexToBytes(mac));
            p.Add(ProtocalUtil.FIELD_SEQ, Convert.IntToBytes(0x01000000 + seq));
            p.Add(ProtocalUtil.FIELD_BLOCK2A, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2B, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2C, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2D, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2E, Convert.IntToBytes(0));
            p.Add(ProtocalUtil.FIELD_BLOCK2F, Convert.IntToBytes(0));

            return Crypto3848.encrypt(ProtocalUtil.BuildRequestBody(p));
        }

        public static byte[] GetServices(string server, string mac)
        {
            Dictionary<byte, byte[]> p = new Dictionary<byte, byte[]>();
            p.Add(ProtocalUtil.FIELD_ACTION, new byte[] { ProtocalUtil.ACTION_SERVICE });
            p.Add(ProtocalUtil.FIELD_SESSION, Convert.StringToBytes(DateTime.Now.ToLongTimeString()));
            p.Add(ProtocalUtil.FIELD_MAC, Convert.HexToBytes(mac));

            return Crypto3848.encrypt(ProtocalUtil.BuildRequestBody(p));
        }

        public static byte[] Confirm(string server, string user, string mac, string ip, string service)
        {
            Dictionary<byte, byte[]> p = new Dictionary<byte, byte[]>();
            p.Add(ProtocalUtil.FIELD_ACTION, new byte[] { ProtocalUtil.ACTION_CONFIRM });
            p.Add(ProtocalUtil.FIELD_USER, Convert.StringToBytes(user));
            p.Add(ProtocalUtil.FIELD_MAC, Convert.HexToBytes(mac));
            p.Add(ProtocalUtil.FIELD_IP, Convert.StringToBytes(ip));
            p.Add(ProtocalUtil.FIELD_SERVICE, Convert.StringToBytes(service));

            return Crypto3849.encrypt(ProtocalUtil.BuildRequestBody(p));
        }

        public static byte[] GetServerIp(string ip, string mac)
        {
            Dictionary<byte, byte[]> p = new Dictionary<byte, byte[]>();
            p.Add(ProtocalUtil.FIELD_ACTION, new byte[] { ProtocalUtil.ACTION_SERVER });
            p.Add(ProtocalUtil.FIELD_SESSION, Convert.StringToBytes(DateTime.Now.ToLongTimeString()));
            p.Add(ProtocalUtil.FIELD_IP, Convert.StringToBytes(ip, 16));
            p.Add(ProtocalUtil.FIELD_MAC, Convert.HexToBytes(mac));

            return Crypto3848.encrypt(ProtocalUtil.BuildRequestBody(p));
        }

        private static byte[] Send(byte[] buf, string targetIp, int targetPort, int localPort)
        {
            IPAddress hostIp = IPAddress.Parse(targetIp);
            IPEndPoint host = new IPEndPoint(hostIp, targetPort);

            UdpClient client = new UdpClient(localPort);

            client.Send(buf, buf.Length, host);
            
            byte[] ret = client.Receive(ref host);
            client.Close();
            
            return ret;
        }
    }
}
