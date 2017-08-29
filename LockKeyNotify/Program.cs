using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace LockKeyNotify
{
    internal static class Program
    {
        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] Args)
        {
            /** 
           * 当前用户是管理员的时候，直接启动应用程序 
           * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行 
           */
            //获得当前登录的Windows用户标示  
            var identity = WindowsIdentity.GetCurrent();
            //创建Windows用户主题  
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var principal =
                new WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员  
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                //如果是管理员，则直接运行  
                Application.EnableVisualStyles();
                Application.Run(new Form1());
            }
            else
            {
                //创建启动对象  
                var startInfo = new ProcessStartInfo();
                //设置运行文件  
                startInfo.FileName = Application.ExecutablePath;
                //设置启动参数  
                startInfo.Arguments = string.Join(" ", Args);
                //设置启动动作,确保以管理员身份运行  
                startInfo.Verb = "runas";
                //如果不是管理员，则启动UAC  
                Process.Start(startInfo);
                //退出  
                Application.Exit();
            }
        }
    }
}