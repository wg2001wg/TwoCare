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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Timers;

namespace Jackch
{
    public partial class ViewScreen
    {

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, //Ŀ���豸�ľ��
            int nXDest, // Ŀ���������Ͻǵ�X����
            int nYDest, // Ŀ���������Ͻǵ�X����
            int nWidth, // Ŀ�����ľ��εĿ��
            int nHeight, // Ŀ�����ľ��εĳ���
            IntPtr hdcSrc, // Դ�豸�ľ��
            int nXSrc, // Դ��������Ͻǵ�X����
            int nYSrc, // Դ��������Ͻǵ�X����
            System.Int32 dwRop // ��դ�Ĳ���ֵ
            );

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern IntPtr CreateDC(
            string lpszDriver, // ��������
            string lpszDevice, // �豸����
            string lpszOutput, // ���ã������趨λ"NULL"
            IntPtr lpInitData // ����Ĵ�ӡ������
            );

        public Image GetScreen( )
        {
            //this.Hide();
            IntPtr dc1 = CreateDC("DISPLAY", null, null, (IntPtr)null);
            //������ʾ����DC
            Graphics g1 = Graphics.FromHdc(dc1);
            //��һ��ָ���豸�ľ������һ���µ�Graphics����
            Bitmap MyImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, g1);
            //������Ļ��С����һ����֮��ͬ��С��Bitmap����
            Graphics g2 = Graphics.FromImage(MyImage);
            //�����Ļ�ľ��
            IntPtr dc3 = g1.GetHdc();
            //���λͼ�ľ��
            IntPtr dc2 = g2.GetHdc();
            //�ѵ�ǰ��Ļ����λͼ������
            BitBlt(dc2, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, dc3, 0, 0, 13369376);
            //�ѵ�ǰ��Ļ������λͼ��
            g1.ReleaseHdc(dc3);
            //�ͷ���Ļ���
            g2.ReleaseHdc(dc2);
            //�ͷ�λͼ���
            return MyImage;
            //this.Show();

        }

    }
}

