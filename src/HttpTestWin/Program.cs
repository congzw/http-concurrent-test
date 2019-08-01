using System;
using System.Windows.Forms;
using Common;
using HttpTestWin.App;

namespace HttpTestWin
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var simpleIoc = SimpleIoc.Instance;
            simpleIoc.InitHttpTest();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = "系统发生了未处理的异常! ";
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                message = message + exception.Message;
            }

            MessageBox.Show(message);
        }
    }
}
