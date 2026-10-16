using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Win32;

[assembly: AssemblyTitle("Cursor Atelier 安装程序")]
[assembly: AssemblyVersion("1.6.3.0")]
[assembly: AssemblyFileVersion("1.6.3.0")]
static class Installer {
 static readonly string InstallRoot=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","CursorAtelier");
 static readonly string MenuRoot=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"Cursor Atelier");
 static string AppPath{get{return Path.Combine(InstallRoot,"CursorAtelier.exe");}}
 [STAThread]static int Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);try{if(args.Length>1&&args[0]=="--verify-payload"){Directory.CreateDirectory(args[1]);Extract(Path.Combine(args[1],"extracted.exe"));return 0;}if(args.Contains("--uninstall"))return Uninstall();using(var f=new SetupForm()){if(args.Length>1&&args[0]=="--render-smoke"){f.Shown+=(s,e)=>{using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(Point.Empty,f.Size));b.Save(args[1]);}f.Close();};}Application.Run(f);}return 0;}catch(Exception e){MessageBox.Show(e.Message,"Cursor Atelier 安装程序",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}}
 static void Extract(string file){using(var resource=Assembly.GetExecutingAssembly().GetManifestResourceStream("CursorAtelier.exe"))using(var target=File.Create(file))resource.CopyTo(target);}
 static void Shortcut(string file,string target,string arguments){Type t=Type.GetTypeFromProgID("WScript.Shell");dynamic shell=Activator.CreateInstance(t);dynamic link=shell.CreateShortcut(file);link.TargetPath=target;link.Arguments=arguments;link.WorkingDirectory=InstallRoot;link.Description="Cursor Atelier · 光标工坊";link.IconLocation=target+",0";link.Save();System.Runtime.InteropServices.Marshal.FinalReleaseComObject(link);System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);}
 static void CheckRunning(){foreach(var process in Process.GetProcessesByName("CursorAtelier")){using(process){try{if(string.Equals(process.MainModule.FileName,AppPath,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("光标工坊正在运行。请从系统托盘右键菜单选择「退出」后重试。");}catch(System.ComponentModel.Win32Exception){throw new InvalidOperationException("无法检查运行状态，请先退出光标工坊再重试。");}}}}
 public static void Install(bool desktop){CheckRunning();Directory.CreateDirectory(InstallRoot);string staged=Path.Combine(InstallRoot,"CursorAtelier.new.exe");Extract(staged);if(File.Exists(AppPath))File.Replace(staged,AppPath,null);else File.Move(staged,AppPath);
  string uninstall=Path.Combine(InstallRoot,"Uninstall.exe");if(!string.Equals(Application.ExecutablePath,uninstall,StringComparison.OrdinalIgnoreCase))File.Copy(Application.ExecutablePath,uninstall,true);File.WriteAllText(Path.Combine(InstallRoot,".cursor-atelier-install"),"Cursor Atelier 1.6.3");Directory.CreateDirectory(MenuRoot);Shortcut(Path.Combine(MenuRoot,"光标工坊.lnk"),AppPath,"");Shortcut(Path.Combine(MenuRoot,"卸载光标工坊.lnk"),uninstall,"--uninstall");if(desktop)Shortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"光标工坊.lnk"),AppPath,"");
  using(var k=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\CursorAtelier")){k.SetValue("DisplayName","Cursor Atelier · 光标工坊");k.SetValue("DisplayVersion","1.6.3");k.SetValue("Publisher","Cursor Atelier");k.SetValue("InstallLocation",InstallRoot);k.SetValue("DisplayIcon",AppPath);k.SetValue("UninstallString","\""+uninstall+"\" --uninstall");k.SetValue("NoModify",1,RegistryValueKind.DWord);k.SetValue("NoRepair",1,RegistryValueKind.DWord);k.SetValue("EstimatedSize",Math.Max(1,(int)(new FileInfo(AppPath).Length/1024)),RegistryValueKind.DWord);}
 }
 static int Uninstall(){if(!File.Exists(Path.Combine(InstallRoot,".cursor-atelier-install"))){MessageBox.Show("未找到本程序的安装记录。","Cursor Atelier");return 1;}if(MessageBox.Show("卸载光标工坊？\n\n将恢复 Windows 默认指针。个人皮肤库和备份会保留，便于重新安装后使用。","卸载光标工坊",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return 0;CheckRunning();
  string data=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CursorAtelier");if(File.Exists(AppPath)){using(var p=Process.Start(new ProcessStartInfo(AppPath,"--reset-default"){UseShellExecute=false})){p.WaitForExit();if(p.ExitCode!=0)throw new InvalidOperationException("光标方案恢复失败，已停止卸载。请先打开工具手动恢复，再重试。");}}
  using(var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true)){if(k!=null){string value=Convert.ToString(k.GetValue("CursorAtelier"));if(value.IndexOf(AppPath,StringComparison.OrdinalIgnoreCase)>=0)k.DeleteValue("CursorAtelier",false);}}
  foreach(var file in new[]{AppPath,Path.Combine(InstallRoot,".cursor-atelier-install"),Path.Combine(MenuRoot,"光标工坊.lnk"),Path.Combine(MenuRoot,"卸载光标工坊.lnk"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"光标工坊.lnk")})if(File.Exists(file))File.Delete(file);
  if(Directory.Exists(MenuRoot)&&!Directory.EnumerateFileSystemEntries(MenuRoot).Any())Directory.Delete(MenuRoot);Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\CursorAtelier",false);
  // Delete only the known installed uninstaller, then the now-empty installation directory.
  string exe=Path.Combine(InstallRoot,"Uninstall.exe");string script="Start-Sleep -Seconds 2; Remove-Item -LiteralPath '"+exe.Replace("'","''")+"' -Force -ErrorAction SilentlyContinue; if ((Test-Path -LiteralPath '"+InstallRoot.Replace("'","''")+"') -and -not (Get-ChildItem -LiteralPath '"+InstallRoot.Replace("'","''")+"' -Force)) { Remove-Item -LiteralPath '"+InstallRoot.Replace("'","''")+"' -ErrorAction SilentlyContinue }";
  MessageBox.Show("卸载完成。个人皮肤库与备份已保留。","Cursor Atelier");Process.Start(new ProcessStartInfo("powershell.exe","-NoProfile -NonInteractive -WindowStyle Hidden -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(script))){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden});return 0;
 }
 public static void Launch(){Process.Start(new ProcessStartInfo(AppPath){UseShellExecute=true});}
}
class SetupForm:Form {
 Label status;CheckBox desktop,launch;Button install;
 public SetupForm(){Text="Cursor Atelier · 安装向导";Size=new Size(660,550);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(16,21,32);ForeColor=Color.FromArgb(235,241,250);Font=new Font("Microsoft YaHei UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  Label brand=new Label{Text="↖  CURSOR ATELIER",Font=new Font("Segoe UI",24,FontStyle.Bold),ForeColor=Color.FromArgb(131,232,205),Location=new Point(32,31),Size=new Size(580,52)};Controls.Add(brand);Controls.Add(new Label{Text="光标工坊  /  让桌面拥有你的风格",Location=new Point(35,91),Size=new Size(580,31),Font=new Font("Microsoft YaHei UI",14)});Controls.Add(new Label{Text="54 款原创皮肤 · 动静态光标 · Sweezy 导入\n趣味造型 · 主题配色 · 批量备份 · 一键恢复",Location=new Point(35,151),Size=new Size(580,70),ForeColor=Color.FromArgb(148,164,187)});
  status=new Label{Text="为当前用户安装，无需管理员权限。\n安装位置："+Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","CursorAtelier"),Location=new Point(35,246),Size=new Size(575,68),Font=new Font("Microsoft YaHei UI",9),ForeColor=Color.FromArgb(148,164,187)};Controls.Add(status);
  desktop=new CheckBox{Text="创建桌面快捷方式",Checked=true,Location=new Point(35,326),Size=new Size(300,30)};launch=new CheckBox{Text="安装完成后打开光标工坊",Checked=true,Location=new Point(35,362),Size=new Size(400,30)};Controls.Add(desktop);Controls.Add(launch);
  install=new Button{Text="安装光标工坊",Location=new Point(35,423),Size=new Size(575,45),BackColor=Color.FromArgb(131,232,205),ForeColor=BackColor,FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand};install.FlatAppearance.BorderSize=0;Controls.Add(install);install.Click+=(s,e)=>{install.Enabled=false;try{Installer.Install(desktop.Checked);status.Text="安装完成。可在 Windows「已安装的应用」中卸载。";install.Text="完成";if(launch.Checked)Installer.Launch();Close();}catch(Exception ex){status.Text="安装未完成："+ex.Message;install.Enabled=true;MessageBox.Show(ex.Message,"安装未完成",MessageBoxButtons.OK,MessageBoxIcon.Warning);}};
 }
}
