using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace OpenSupplicant
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool initiallyOwned = true;
            bool createdNew;
            new Mutex(initiallyOwned, "openSupplicant", out createdNew);
            if (!(initiallyOwned && createdNew))
            {
                MessageBox.Show("已经有相同的实例在运行。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormLogin());
            }
        }
    }
}
