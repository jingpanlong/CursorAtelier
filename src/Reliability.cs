using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CursorAtelier {
public static class ErrorLog {
 public static string DirectoryPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CursorAtelier","logs");
 public static void Record(Exception error){try{Directory.CreateDirectory(DirectoryPath);string path=Path.Combine(DirectoryPath,"error-"+DateTime.Now.ToString("yyyyMMdd")+".log");if(File.Exists(path)&&new FileInfo(path).Length>4*1024*1024)File.Move(path,path+"."+DateTime.Now.Ticks);File.AppendAllText(path,DateTime.Now.ToString("O")+"\r\n"+error+"\r\n\r\n");}catch{}}
}
// Menu callbacks run after ToolStrip has finished dismissing the drop-down.
// Never dispose a menu synchronously from Closed or from an item callback.
public sealed class SafeMenu:ContextMenuStrip {
 readonly Form owner;readonly bool transient;bool disposeQueued;readonly EventHandler ownerDisposed;
 public SafeMenu(Form form,bool autoDispose=false){owner=form;transient=autoDispose;ownerDisposed=(o,e)=>Dispose();form.Disposed+=ownerDisposed;}
 protected override void Dispose(bool disposing){if(disposing)owner.Disposed-=ownerDisposed;base.Dispose(disposing);}
 public ToolStripItem Action(string text,System.Action callback){return Items.Add(text,null,(o,e)=>Post(callback));}
 public void Post(System.Action callback){if(owner.IsDisposed||owner.Disposing||!owner.IsHandleCreated)return;try{owner.BeginInvoke(new MethodInvoker(()=>{if(owner.IsDisposed||owner.Disposing)return;try{callback();}catch(Exception error){Theme.Error(error);}}));}catch(InvalidOperationException){if(!owner.IsDisposed&&!owner.Disposing)throw;}}
 public void DisposeLater(){if(IsDisposed||disposeQueued)return;disposeQueued=true;if(owner.IsDisposed||owner.Disposing||!owner.IsHandleCreated){Dispose();return;}Post(()=>Dispose());}
 protected override void OnClosed(ToolStripDropDownClosedEventArgs e){base.OnClosed(e);if(transient)DisposeLater();}
}
public sealed class StartupService {
 public const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
 public const string ValueName="CursorAtelier";
 public static string Command(string executable){return "\""+Path.GetFullPath(executable)+"\" --tray";}
 public static bool Enabled{get{using(var key=Registry.CurrentUser.OpenSubKey(RunKey,false))return key!=null&&!string.IsNullOrWhiteSpace(Convert.ToString(key.GetValue(ValueName)));}}
 public static void Set(bool enabled){using(var key=Registry.CurrentUser.CreateSubKey(RunKey)){if(enabled)key.SetValue(ValueName,Command(Application.ExecutablePath),RegistryValueKind.String);else key.DeleteValue(ValueName,false);}}
 public static void RepairExistingPath(){if(!Enabled)return;using(var key=Registry.CurrentUser.OpenSubKey(RunKey,false)){if(string.Equals(Convert.ToString(key.GetValue(ValueName)),Command(Application.ExecutablePath),StringComparison.OrdinalIgnoreCase))return;}Set(true);}
}
public static partial class NativeExit {
 public static void WindowsDefault(){using(var key=Registry.CurrentUser.CreateSubKey(@"Control Panel\Cursors")){foreach(var role in Store.Roles)key.SetValue(role,"",RegistryValueKind.String);key.SetValue("","",RegistryValueKind.String);key.SetValue("Scheme Source",0,RegistryValueKind.DWord);}Native.Reload();}
}
public partial class Store {
 public bool AllowDesktopChanges=true;public string StartupNotice="";
 public void ResetOnExit(){if(!AllowDesktopChanges)return;NativeExit.WindowsDefault();Pref.Active="";try{Save();}catch(Exception e){ErrorLog.Record(e);}}
 void Normalize(){if(Pref.Skins==null)Pref.Skins=new List<Skin>();Pref.Skins=Pref.Skins.Where(s=>s!=null).ToList();if(!new[]{32,48,64,96}.Contains(Pref.Size))Pref.Size=48;Pref.RotationMinutes=Math.Max(1,Math.Min(1440,Pref.RotationMinutes));if(!Theme.Ids.Contains(Pref.ThemeId))Pref.ThemeId="midnight";if(!new[]{"默认顺序","名称排序","收藏优先","最近使用"}.Contains(Pref.Sort))Pref.Sort="默认顺序";
  if(Pref.SchemaVersion<13){Pref.Tray=true;Pref.RestoreOnExit=true;if(string.IsNullOrEmpty(Pref.LastSkin))Pref.LastSkin=Pref.Active;Pref.SchemaVersion=13;}
  Pref.Tray=true;var seen=new HashSet<string>();foreach(var skin in Pref.Skins.ToArray()){if(skin.Builtin>=0){if(skin.Builtin>=Art.Names.Length){Pref.Skins.Remove(skin);continue;}skin.Id="atelier-"+skin.Builtin;}else if(string.IsNullOrEmpty(skin.Id)||!System.Text.RegularExpressions.Regex.IsMatch(skin.Id,"^[0-9a-fA-F]{32}$")){Pref.Skins.Remove(skin);continue;}if(!seen.Add(skin.Id)){Pref.Skins.Remove(skin);continue;}if(string.IsNullOrWhiteSpace(skin.Name))skin.Name=skin.Builtin>=0?Art.Names[skin.Builtin]:"未命名皮肤";if(skin.SizeOverride!=0&&skin.SizeOverride!=-1&&!CursorSizing.Sizes.Contains(skin.SizeOverride))skin.SizeOverride=0;if(skin.Builtin>=0&&skin.SizeOverride<0)skin.SizeOverride=0;if(skin.Roles==null)skin.Roles=new Dictionary<string,string>();skin.Roles=skin.Roles.Where(p=>Roles.Contains(p.Key)&&!string.IsNullOrWhiteSpace(p.Value)).ToDictionary(p=>p.Key,p=>p.Value);}
 }
 public bool Available(Skin s){return s!=null&&s.Roles!=null&&s.Roles.ContainsKey("Arrow")&&s.Roles.Values.All(File.Exists);}
}
public partial class MainForm {
 Button startupSwitch;bool startupOn;bool resettingExit;internal SafeMenu RoleMenu,SortMenu;
 // 高 DPI 缩放下 CheckBox(Appearance=Button, FlatStyle=Flat) 会出现文字不渲染、AutoSize 高度塌陷的问题；
 // 改用与界面其他按钮一致的普通平坦 Button，保证任何缩放比例下文字都可见。
 void AddStartupSwitch(Control parent){startupSwitch=new Button{FlatStyle=FlatStyle.Flat,TextAlign=ContentAlignment.MiddleCenter,Location=new Point(0,74),Size=new Size(165,33),Cursor=Cursors.Hand,Tag="normal",Font=Theme.Font(9)};startupSwitch.FlatAppearance.BorderSize=0;startupSwitch.UseVisualStyleBackColor=false;
  // 部分高 DPI 缩放环境下，系统平坦按钮渲染器会漏画该按钮的文字：在 Paint 事件里用 TextRenderer 主动再画一次，确保任何缩放比例下文字都可见。
  startupSwitch.Paint+=(o,e)=>{var b=(Button)o;TextRenderer.DrawText(e.Graphics,b.Text,b.Font,b.ClientRectangle,b.Enabled?b.ForeColor:Theme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPadding);};
  startupSwitch.Click+=(o,e)=>{try{StartupService.Set(!startupOn);startupOn=!startupOn;RefreshStartupSwitch();status.Text=startupOn?"已开启开机自启动":"已关闭开机自启动";}catch(Exception error){Theme.Error(error);RefreshStartupSwitch();}};parent.Controls.Add(startupSwitch);RefreshStartupSwitch();}
 void RefreshStartupSwitch(){if(startupSwitch==null||startupSwitch.IsDisposed)return;try{startupOn=StartupService.Enabled;startupSwitch.Text="开机自启："+(startupOn?"开启":"关闭");startupSwitch.Tag=startupOn?"primary":"normal";startupSwitch.BackColor=startupOn?Theme.Accent:Theme.Raised;startupSwitch.ForeColor=startupOn?Theme.OnAccent:Theme.Text;}catch(Exception e){ErrorLog.Record(e);startupSwitch.Text="开机自启：不可用";startupSwitch.Enabled=false;}}
 public void ExitApplication(){reallyClose=true;Close();}
 public void RestoreLastSkin(){if(!string.IsNullOrEmpty(store.StartupNotice))status.Text=store.StartupNotice;if(!store.Pref.ApplyOnStartup)return;var skin=store.Pref.Skins.FirstOrDefault(s=>s.Id==store.Pref.LastSkin);if(skin==null)return;if(store.Available(skin))QuickApply(skin);else status.Text="上次皮肤文件缺失，未自动应用；请重新导入。";}
 protected override void Dispose(bool disposing){if(disposing){CancelSizeWork();if(rotate!=null)rotate.Dispose();if(tray!=null){var menu=tray.ContextMenuStrip;tray.Visible=false;tray.Dispose();if(menu!=null)menu.Dispose();}if(IsHandleCreated)for(int i=1;i<=3;i++)Native.UnregisterHotKey(Handle,i);}base.Dispose(disposing);}
}
}
