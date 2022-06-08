using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace keybon
{
    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "keybon", out createdNew))
            {
                if (!createdNew)
                {

                    MessageBox.Show("Application has already started", "Error");
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindow());
            }

        }

    }
}
