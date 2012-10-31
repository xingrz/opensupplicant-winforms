using System.Text;
using System;

namespace OpenSupplicant
{
    class Convert
    {
        public static byte[] StringToBytes(string str)
        {
            if (str == null) return new byte[0];
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            Encoding gbk = Encoding.GetEncoding("GB2312");
            return Encoding.Convert(utf8, gbk, utf8.GetBytes(str));
        }

        public static byte[] StringToBytes(string str, int length)
        {
            byte[] src = StringToBytes(str);
            byte[] ret = new byte[length];

            Array.Copy(src, ret, length > src.Length ? src.Length : length);

            return ret;
        }

        public static string BytesToString(byte[] bytes)
        {
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            Encoding gbk = Encoding.GetEncoding("GB2312");
            return utf8.GetString(Encoding.Convert(gbk, utf8, bytes));
        }

        public static string BytesToHex(byte[] bytes)
        {
            string ret = "";
            foreach (byte b in bytes)
            {
                ret += b.ToString("X2");
            }
            return ret;
        }

        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 == 1) hex = "0" + hex;
            byte[] ret = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length / 2; i++)
            {
                ret[i] = byte.Parse(hex.Substring(i * 2, 2), global::System.Globalization.NumberStyles.HexNumber);
            }
            return ret;
        }

        public static byte[] IntToBytes(int number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            Array.Reverse(bytes);
            return bytes;
        }

        public static int BytesToInt(byte[] bytes)
        {
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
