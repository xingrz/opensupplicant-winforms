using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSupplicant
{
    class Crypto3848
    {
        private static byte[] bs_row = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
        private static byte[] bs_col = { 0x00, 0x01, 0x08, 0x09, 0x04, 0x05, 0x0c, 0x0d, 0x02, 0x03, 0x0a, 0x0b, 0x06, 0x07, 0x0e, 0x0f };

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="buf">明文</param>
        /// <returns>密文</returns>
        public static byte[] encrypt(byte[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                int bl, of, row, col;

                if (buf[i] % 2 == 0)
                {
                    bl = (int)Math.Floor((decimal)(buf[i] / 32));
                    of = buf[i] % 32;
                    row = (int)Math.Floor((decimal)(of / 4));
                }
                else
                {
                    bl = (int)Math.Floor((decimal)((buf[i] - 1) / 32));
                    of = (buf[i] - 1) % 32;
                    row = (int)Math.Floor((decimal)(of / 4)) + 8;
                }

                if (of % 4 == 0)
                {
                    col = bl * 2;
                }
                else
                {
                    col = bl * 2 + 1;
                }

                buf[i] = (byte)((int)bs_row[row] * 16 + (int)bs_col[col]);
            }

            return buf;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="buf">密文</param>
        /// <returns>明文</returns>
        public static byte[] decrypt(byte[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                int bl, of;

                byte c1 = (byte)Math.Floor((decimal)(buf[i] / 16));
                byte c2 = (byte)(buf[i] % 16);

                int row = Array.IndexOf(bs_row, c1);
                int col = Array.IndexOf(bs_col, c2);

                if (row < 8)
                {
                    of = row * 4;
                }
                else
                {
                    of = (row - 8) * 4 + 1;
                }

                if (col % 2 == 0)
                {
                    bl = col * 16;
                }
                else
                {
                    bl = (col - 1) * 16 + 2;
                }

                buf[i] = (byte)(bl + of);
            }

            return buf;
        }
    }
}
