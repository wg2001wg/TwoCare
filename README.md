说起木马没有人不知道吧,想必各位大哥的灰鸽子里都有数也数不清,让我等小菜羡慕不已的肉鸡吧,本人天生愚钝,
怎么也用不明白这个灰鸽子,又要域名又要FTP的.还好在下学过点编程,既然别人的不好用自己写一个得了,自己写的
不应该不会用了吧,哪我们就开始我们的木马之旅吧.
1.通信技术
    几乎所有的网络程序都要涉及到socket的编程,然而socket到底是什么呢?
    socket是在Unix系统上提出来的,一开始主要用于本地进程通信,但很快用到了C/S(client/server)体系中用于网络中两台主机之间的
    通信.说的形像点,socket像一个管子(或许有点不太恰当),你用它把两台主机连接起来,然后在管子的一头放东西,在另外一头就可以
    接收这些东西了.有了这个管子当然我们还要有操作这个管子的工具,这些工具就是socket函数.基本的socket函数如下:
    int socket(int family, int type, int protocol);
    int closesocket(int socket);
    int bind(int s, const struct sockaddr * name, int namelen);
    int listen(int s, int backlog);
    int accept(int s, struct sockaddr *addr, int *addrlen);
    int connect(int s, struct sockaddr *name, int namelen);
    int send(int s, const char *buf, int len, int flags);
    int sendto(int s, const char *buf, int len, int flags, const struct sockaddr *to, int tolen);/* 用于UDP */
    int recv(int s, char *buf, int len, int flasg);
    int recvfrom(int s, char *buf, int len, int flags, struct sockaddr *from, int *fromlen);/* 用于UDP */
    基本函数就这些了,是不是很少呀,原来哪么厉害的木马就是用这么几个函数写成的呀,嘿嘿
    好了下面我就以一个例子给大家介绍一个这几个函数的用法,木马程序应该算是比较典型的C/S体系,所以下面将有两个程序,
    一个server,一个client
	server端的流程如下(括号内为使用的socket函数):
	生成socket(socket)->
	将生成的socket与本地的IP地址和指定端口绑定(bind)->
	在此socket上监听(listen)->
	接受来自客户端的连接请求(accept : accept将会返回一个新的socket,以后的通信都在此socket上进行)->
	开始用accept新生成的socket上进行通信(send, sendto, recv, recvfrom)->
	通信完毕关闭连接(closesocket : accept生成的socket和原始的监听socket都要关闭)
	client端的流程如下:
	生成socket(socket)->
	与指定的服务端(connect : 指定了IP地址和端口)->
	开始通信(send, sendto, recv, recvfrom)->
	通信结束关闭连接(closesocket)
	当然这只是最基本的一些东西,写在这里只是想告诉哪些没有学习过网络编程的人,其实很简单的.
	(如果有时间不要总想着搞别人机子,好好学习天天向上,嘿嘿)

	有了以上的基础,我们已经可以写出最简单的木马了,只要我们把想执行的命令发给服务端然后
	服务端执行就可以了,怎么发挥就看你自己了.

2.自启动技术
	我们的木马可以通信了,但总不能让受害人自己去执行吧(哪有这么傻的人呀,也介绍我一个)
	2.1 加到开始菜单的启动里.
	2.2 加到计划任务里并设成每次启动时执行.
	2.3 加到注册表的启动里,以下为涉及到的键值.
		HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
		HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce
		HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Runservices 
		HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
		HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\currentversion\runonce
		HKEY_CURRENT_USER\SOFTWARE\Microsoft\windowsnt\currentversion\windows 
			建一个字符串名为load键值为自启动程序的路径但是要注意短文件名规则 
		HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\windowsNT\CurrentVersion\Winlogon
	2.4 写成Windows的服务,并设为自动.(提供示例代码)
	2.5 替换windows的正常但无实际用处的服务或程序.
	2.6 利用autorun.inf,现在U盘病毒最常用的方法.以下为一个autorun.inf文件内容
		[AutoRun]
		open=要执行的文件名
		shellexecute=要执行的文件名
		shell\Auto\command=要执行的文件名
		shell=Auto
	2.7 利用文件关联,
		正常情况下txt文件的打开方式为Notepad.exe文件,如果一旦中了文件关联类的木马,这样打开一个txt文件,
		原本应该用Notepad打开该文件的,现在却变成了启动木马程序了
		HKEY_CLASSES_ROOT\exefile\shell\open\command,这里是exe文件的打开方式,默认键值为:"%1"%*.
		如果把默认键值改为horse.exe "%1"%*,您每次运行exe文件,这个horse.exe文件就会被执行,其它的类似
		以下注册表键也涉及到文件关联
		HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts 
	2.8 利用windows启动文件
		可以利用的文件有Win.ini,system.ini,Autoexec.bat,Config.sys

3.隐藏技术
	木马的泛滥也提高的人们的安全意识,人们都会打开任务管理器看看有没有什么可疑进程,又或者用netstat -an看看有没有什么可疑端口
	3.1 进程的隐藏
		1.起一个好名字,不要叫什么MyDoor,MyHorse什么的傻子都看出来了不是什么好东西,比如叫什么svchost呀,rundll32,这样就会好的多了
		2.写DLL木马,直接用rundll32启动,这样只会多一个rundll32进程而已.
			其命令行下的使用方法为:Rundll32.exe DLLname,Functionname [Arguments]  
			DLLname为需要执行的DLL文件名;Functionname为前边需要执行的DLL文件的具体引出函数;[Arguments]为引出函数的具体参数.  
			这里要注意三点:
			1.Dll文件名中不能含有空格,比如该文件位于c:\Program Files\目录,你要把这个路径改成c:\Progra～1\;  
			2.Dll文件名与Dll入口点间的逗号不能少,否则程序将出错并且不会给出任何信息! 
			3.这是最重要的点:Rundll不能用来呼叫含返回值参数的Dll,例如Win32API中的GetUserName(),GetTextFace()等.
		3.用DLL直接插入系统进程比如explorer,svchost等等(以前的,提供例程)
		4.编写驱动,进行内核级进程的隐藏.(学习中...提供示例和网页)

	3.2 端口的隐藏
		1.用一个周知口,但前题是这个端口没有在主机上被使用,比如7.21.22.23......
		2.使用嗅探型木马,既监听所有数据包再进行分析.(提供例程)

	3.3 文件隐藏
		1.和进程隐藏一样起一个好点的名字.
		2.把文件属性设为系统+隐藏
		3.在windows下无法以设备名来命名文件或文件夹,这些设备名主要有aux、com1、com2、prn、con、nul等,但windows2000/XP
		  有个漏洞可以以设备名来命名文件或文件夹,让木马可以躲在那里而不被发现.
		  具体方法是:用命令提示符窗口,然后输入md c:con\命令,可以建立一个名为con的目录.默认请况下,windows是无法建立这
		  类目录的,正是利用了windows的漏洞我们才可以建立此目录.
		4.利用NTFS流
		   首先用记事本新建两个文本文档,分别名为“1.txt”“2.txt”,其内容为“正常文件、数据流文件”,打开CMD命令行窗口,
		   进入两个文件所在文件夹,输入 type 2.txt>1.txt: shujuliu.txt,回车.即可将文件2.txt的内容加入1.txt,
		   内容以数据流方式保存,该数据流名为shujuliu.txt.在资源管理器中查看宿主文件1.txt,发现文件的修改日期和文件大小
		   都无变化,现在删除2.txt,执行命令:notepad 1.txt:shujuliu.txt ,即可查看数据流文件中的文件内容了.(提供例程)
