﻿/******************************************
 * 版权所有Sinner
 * QQ:53811910
 * E-mail:53811910@qq.com
 * 个人Blog:53811910.qzone.qq.com
 * CSDN_Blog:http://blog.csdn.net/jackch88/
 * ****************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace Jackch
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 

        //只允许运行一个程序
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int WS_SHOWNORMAL = 1;



        [STAThread]
        static void Main()
        {
            /* 
             Process instance = RunningInstance();
             if (instance == null)
             {
        
                 Application.EnableVisualStyles();
                 Application.SetCompatibleTextRenderingDefault(false);
                 Application.Run(new Server());
             }
             else
             {
                 MessageBox.Show("你已经运行了服务端！");
                 //HandleRunningInstance(instance);

             }
            */

            Application.EnableVisualStyles();
             Application.SetCompatibleTextRenderingDefault(false);
             Application.Run(new Server());
            
        }

        //不允许有两个程序同时启动

        public static Process RunningInstance()
        {

            Process current = Process.GetCurrentProcess();

            Process[] processes = Process.GetProcessesByName(current.ProcessName);

            //遍历正在有相同名字运行的例程 

            foreach (Process process in processes)
            {

                //忽略现有的例程 

                if (process.Id != current.Id)
                {

                    //确保例程从EXE文件运行 

                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") ==

                         current.MainModule.FileName)
                    {

                        //返回另一个例程实例 

                        return process;

                    }

                }

            }

            //没有其它的例程，返回Null 

            return null;

        }



        public static void HandleRunningInstance(Process instance)
        {

            //确保窗口没有被最小化或最大化 

            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);



            //设置真实例程为foreground window 

            SetForegroundWindow(instance.MainWindowHandle);

        } 

    }
}