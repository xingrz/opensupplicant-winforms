using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSupplicant
{
    class Crypto3849
    {
        private static byte[] bs_row = { 0x00, 0x01, 0x08, 0x09, 0x04, 0x05, 0x0c, 0x0d, 0x02, 0x03, 0x0a, 0x0b, 0x06, 0x07, 0x0e, 0x0f };
        private static byte[] bs_col = { 0x00, 0x01, 0x04, 0x05, 0x02, 0x03, 0x06, 0x07, 0x08, 0x09, 0x0c, 0x0d, 0x0a, 0x0b, 0x0e, 0x0f };

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="buf">明文</param>
        /// <returns>密文</returns>
        public static byte[] encrypt(byte[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                int bl_row, bl_col, row, col;

                byte c1 = (byte)Math.Floor((decimal)(buf[i] / 16));
                byte c2 = (byte)(buf[i] % 16);

                if (c1 < 8)
                {
                    bl_row = (int)Math.Floor((decimal)(c1 / 2)) * 4;
                    bl_col = (c1 % 2) * 4;
                }
                else
                {
                    bl_row = (int)Math.Floor((decimal)((c1 - 8) / 2)) * 4;
                    bl_col = (c1 % 2 + 2) * 4;
                }

                if (c2 % 2 == 0)
                {
                    row = bs_row[bl_row + (int)Math.Floor((decimal)(c2 / 8)) * 2];
                    col = bs_col[bl_col + (c2 % 8) / 2];
                }
                else
                {
                    row = bs_row[bl_row + (int)Math.Floor((decimal)(c2 / 8)) * 2 + 1];
                    col = bs_col[bl_col + ((c2 - 1) % 8) / 2];
                }

                buf[i] = (byte)(row * 16 + col);
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
                byte c1 = (byte)Math.Floor((decimal)(buf[i] / 16));
                byte c2 = (byte)(buf[i] % 16);

                int row = Array.IndexOf(bs_row, c1);
                int col = Array.IndexOf(bs_col, c2);

                int b1, b2;

                b1 = (int)(Math.Floor((decimal)(row / 4)) * 2 + Math.Floor((decimal)(col / 4)));
                if (col >= 8) b1 += 8;

                b2 = col % 4 * 2 + row % 2;
                if (row % 4 >= 2) b2 += 8;

                buf[i] = (byte)(b1 * 16 + b2);
            }

            return buf;
        }
    }
}
