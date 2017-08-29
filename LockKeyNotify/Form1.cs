using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LockKeyNotify.Properties;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace LockKeyNotify
{
    public partial class Form1 : Form

    {
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        private static int hHook;

        public const int WH_KEYBOARD_LL = 13;

        //LowLevel键盘截获，如果是WH_KEYBOARD＝2，并不能对系统键盘截取，Acrobat Reader会在你截取之前获得键盘。 
        private HookProc KeyBoardHookProcedure;

        //键盘Hook结构函数 
        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        #region DllImport

        //设置钩子 
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //抽掉钩子 
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll")]
        //调用下一个钩子 
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        #endregion


        #region 自定义事件

        public void Hook_Start()
        {
            // 安装键盘钩子 
            if (hHook == 0)
            {
                KeyBoardHookProcedure = KeyBoardHookProc;


                hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                    KeyBoardHookProcedure,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                //如果设置钩子失败. 
                if (hHook == 0)
                {
                    Hook_Clear();
                    throw new Exception();
                }
            }
        }

        //取消钩子事件 
        public void Hook_Clear()
        {
            var retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
            //如果去掉钩子失败. 
            if (!retKeyboard) throw new Exception("UnhookWindowsHookEx failed.");
        }

        #endregion


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);

        private readonly MenuItem autoStart = new MenuItem("开机启动");

        //计时器，清理内存用
        private readonly Timer timer1 = new Timer();

        public Form1()
        {
            InitializeComponent();
            //托盘初始化
            InitialTray();
            Hook_Start();
            timer1.Tick += timer1_Tick;
            timer1.Interval = 20000; //触发时间20s
            timer1.Start();
        }

        //计时器函数
        private void timer1_Tick(object sender, EventArgs e)
        {
            ClearMemory();
        }


        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true,
            CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize,
            int maximumWorkingSetSize);

        // 清理内存
        private void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Hide();
            ShowInTaskbar = false;
        }

        private void InitialTray()
        {
            //托盘图标显示的内容  
            keynotify.Text = "锁定按键提示";
            //初始化ICON
            SetStatus();
            //true表示在托盘区可见，false表示在托盘区不可见  
            keynotify.Visible = true;


            //读取开机启动项（需要管理员权限）
            var rk = Registry.LocalMachine;
            var rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (rk2 == null || rk2.GetValue("LockKeyAutoStart") == null)
                autoStart.Checked = false;
            else
                autoStart.Checked = true;


            autoStart.Click += autostart_Click;


            //退出菜单项  
            var exit = new MenuItem("退出");
            exit.Click += exit_Click;


            ////关联托盘控件  
            MenuItem[] childen = {autoStart, exit};
            keynotify.ContextMenu = new ContextMenu(childen);
        }

        private void exit_Click(object sender, EventArgs e)
        {
            //退出程序 
            //清理钩子
            Hook_Clear();
            timer1.Stop();
            //释放托盘
            keynotify.Dispose();
            Environment.Exit(0);
        }


        private void autostart_Click(object sender, EventArgs e)
        {
            if (autoStart.Checked)
            {
                var rk = Registry.LocalMachine;
                var rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (rk2 == null)
                    rk2 = rk.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("LockKeyAutoStart", false);
                rk2.Close();
                rk.Close();

                autoStart.Checked = false;
            }
            else
            {
                var path = Application.ExecutablePath;
                var rk = Registry.LocalMachine;
                var rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (rk2 == null)
                    rk2 = rk.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                rk2.SetValue("LockKeyAutoStart", path);
                rk2.Close();
                rk.Close();
                autoStart.Checked = true;
            }
        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //鼠标左键单击  
            if (e.Button == MouseButtons.Left)
                if (Visible)
                {
                    Visible = false;
                }
                else
                {
                    Visible = true;
                    Activate();
                }
        }

        public int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            KeyBoardHookStruct kbh;
            kbh = (KeyBoardHookStruct) Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));
            if (kbh.vkCode == (int) Keys.CapsLock || kbh.vkCode == (int) Keys.NumLock)
            {
                var isRunning = false;
                if (!isRunning)
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        isRunning = true;
                        var curCount = 150;
                        while (curCount > 0)
                        {
                            SetStatus();
                            Thread.Sleep(20);
                            curCount--;
                        }
                        isRunning = false;
                    });
            }
            return 0;
        }

        private void SetStatus()
        {
            if (GetKeyState(0x14) != 0 && GetKeyState(0x90) == 0)
            {
                keynotify.Visible = true;
                keynotify.Icon = Resources.CAPLOCK;
                keynotify.Text = "CAPSLOCK ON";
            }
            else if (GetKeyState(0x90) != 0 && GetKeyState(0x14) == 0)
            {
                keynotify.Visible = true;
                keynotify.Icon = Resources.NUMLOCK;
                keynotify.Text = "NUMLOCK ON";
            }
            else if (GetKeyState(0x14) != 0 && GetKeyState(0x90) != 0)
            {
                keynotify.Visible = true;
                keynotify.Icon = Resources.CANDNLOCK;
                //keynotify.Icon =
                //    new Icon("D:/Program Files (x86)/Tencent/QQPCMgr/12.5.18746.501/Images/softmgr.ico");
                keynotify.Text = "ALL ON";
            }
            else
            {
                keynotify.Visible = true;
                keynotify.Icon = Resources.ALLCLOSE;
                keynotify.Text = "ALL Off";
            }
        }
    }
}