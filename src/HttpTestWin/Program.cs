using System;
using System.Windows.Forms;
using Common;
using HttpTestWin.App;
using HttpTestWin.ViewModel;

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

            var httpTestConfig = simpleIoc.Resolve<HttpTestConfig>();
            Form theForm = null;
            if (!string.IsNullOrWhiteSpace(httpTestConfig.TraceApiEndPoint))
            {
                theForm = new ClientTraceForm();
            }
            else
            {
                theForm = new MainForm();
            }

            Application.Run(theForm);
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
