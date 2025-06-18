/******************************************
 * 版权所有Sinner
 * QQ:53811910
 * E-mail:53811910@qq.com
 * 个人Blog:53811910.qzone.qq.com
 * CSDN_Blog:http://blog.csdn.net/jackch88/
 * ****************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;
namespace Jackch
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            InitCommand();

        }
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel=true;
        }

        
    }
}

      