## LockKeyNotify（Main function）
* 按键状态显示程序，目前主要监控 ***Capslock*** 和 ***Numslock***
* Show state of Capslock and NumsLock in tray。
## 环境（Environment）
* Viusal Stdio 2017
## 核心逻辑(How it work)
* 使用C# 的WinForm编写的托盘程序，窗口直接隐藏。设置全局钩子监控键盘按键，当键位变化时读取键位表，并启动循环，监控Ts（当前4s）内有无按键按下。
* Use C#'s WinForm tray,hidden form.Use Globe Hook monitoring key，when keydown，read key map,and keep tracking keydown in 4 seconds. 
* 添加了右键菜单开机启动项，直接修改注册表内容。因此需要管理员权限，如果不是以管理员权限登录，会启动UAC获取权限。
* Right Click tray icon, the first means start with system start,this will use regedit. This need administation authority,if don't have this  authority ,the program will open UAC.
## 使用方法(How to use)
* 复制程序到保存文件夹，双击启动（最好以管理员权限启动）
* Donwload and click,suppose to use administrator authority.
* 设置自动启动请右键托盘，选择开机启动，成功后会打钩，再次点击会取消。
* If you want to auto start.Please right Click tray icon, select the first item.
* 想关闭的化右键托盘退出
* Right Click tray icon,the second to exit.
### 图标解析(Icon means)
* 全部关闭(No Key Down)           ![](http://upload-images.jianshu.io/upload_images/6940610-85a53fdea6d44f5d.PNG?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
* NumsLock 按下(NumsLock Down)![](http://upload-images.jianshu.io/upload_images/6940610-a097f5d85db688d4.PNG?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
* CapsLock 按下(CapsLock Down) ![](http://upload-images.jianshu.io/upload_images/6940610-f90a7539a20e2893.PNG?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
* 都按下(All Down)              ![](http://upload-images.jianshu.io/upload_images/6940610-ec3a43830ebcaa91.PNG?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
## 可能遇到的问题(Problems)
* 如果开启了UAC，每次启动后要读取注册表，因此开机需要弹出权限确认框。如果感觉麻烦，请关闭UAC。
* If auto start. You need UAC,may UAC will show each time when your enter system.
* 第一次打开会在托盘区小箭头李，自行拖动到任务栏即可。
* Tray Icon may hidden at first time,just drag out from hidden zone.
##Release 
### V0.9.0
* 实现基本的功能
* Finish the function.
