using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace CursorAtelier {
public static partial class Native {
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr LoadCursorFromFile(string path);
 [DllImport("user32.dll")] public static extern bool DestroyCursor(IntPtr h);
 [DllImport("user32.dll",SetLastError=true)] public static extern bool SystemParametersInfo(uint action,uint param,IntPtr data,uint flags);
 [DllImport("user32.dll")] public static extern bool DrawIconEx(IntPtr dc,int x,int y,IntPtr h,int w,int height,uint frame,IntPtr brush,uint flags);
 [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr h,int id,uint modifiers,uint key);
 [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr h,int id);
 public static void Reload() { if(!SystemParametersInfo(0x57,0,IntPtr.Zero,0)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),"Windows 无法刷新光标方案"); }
 public static void Validate(string path) { var b=File.ReadAllBytes(path); string e=Path.GetExtension(path).ToLowerInvariant();
  if(b.Length<22 || b.Length>8*1024*1024) throw new InvalidDataException("光标大小无效："+Path.GetFileName(path));
  if(e==".ani") { if(Encoding.ASCII.GetString(b,0,4)!="RIFF" || Encoding.ASCII.GetString(b,8,4)!="ACON") throw new InvalidDataException("无效 ANI 文件"); }
  else if(b[0]!=0 || b[1]!=0 || b[2]!=2 || b[3]!=0 || BitConverter.ToUInt16(b,4)==0) throw new InvalidDataException("无效 CUR 文件");
  var h=LoadCursorFromFile(path); if(h==IntPtr.Zero) throw new InvalidDataException("Windows 无法加载："+Path.GetFileName(path)); DestroyCursor(h);
 }
}
public class Skin {
 public string Id="",Name="",Description="",Color="#77DCCB",Source="";
 public int SizeOverride=0; [ScriptIgnore] public Dictionary<string,string> PreviewRoles; [ScriptIgnore] public Dictionary<string,string> DisplayRoles {get{return PreviewRoles??Roles;}} public int Builtin=-1; public bool Favorite; public long LastUsed; public Dictionary<string,string> Roles=new Dictionary<string,string>();
 public bool Animated { get { return Roles.Values.Any(p=>p.EndsWith(".ani",StringComparison.OrdinalIgnoreCase)); } }
}
public class Preferences { public string Active="",LastSkin="",ThemeId="midnight",Accent="",Sort="默认顺序"; public int SchemaVersion=0,Size=48, RotationMinutes=15; public bool Tray=true,RestoreOnExit=true,ApplyOnStartup=true,Hotkeys=true,Rotate=false,FavoritesOnly=false; public List<Skin> Skins=new List<Skin>(); }
public class Entry { public string Name,Value; public int Kind; public bool Exists; }
public class Snapshot { public List<Entry> Values=new List<Entry>(); }
public partial class Store {
 public static readonly string[] Roles={"Arrow","Hand","IBeam","Wait","AppStarting","Crosshair","Help","No","SizeAll","SizeNS","SizeWE","SizeNWSE","SizeNESW","UpArrow","NWPen","Pin","Person"};
 public static readonly string[] Labels={"普通选择","链接选择","文本选择","忙碌","后台运行","精确选择","帮助选择","不可用","移动","垂直调整","水平调整","斜向调整 ↘","斜向调整 ↗","候选选择","手写","位置选择","人员选择"};
 public string Root; public Preferences Pref; JavaScriptSerializer json=new JavaScriptSerializer();
 public Store(string root,bool allowDesktopChanges=true) { Root=root;AllowDesktopChanges=allowDesktopChanges;Directory.CreateDirectory(root);try{Pref=Read<Preferences>("settings.json")??new Preferences();}catch(Exception error){ErrorLog.Record(error);string file=Path.Combine(root,"settings.json");if(File.Exists(file))File.Copy(file,Path.Combine(root,"settings-recovery-"+DateTime.Now.Ticks+".json"));Pref=new Preferences();StartupNotice="配置文件已备份并恢复默认设置，皮肤文件仍保留。";}Normalize();
  for(int i=0;i<Art.Names.Length;i++) { var s=Pref.Skins.FirstOrDefault(v=>v.Builtin==i); if(s==null) { s=Art.MakeSkin(i); Pref.Skins.Add(s); }if(i>=8)s.Description=Art.MakeSkin(i).Description; EnsureBuiltin(s,48); }
 }
 public T Read<T>(string name) where T:class { var p=Path.Combine(Root,name); if(!File.Exists(p))return null; try{return json.Deserialize<T>(File.ReadAllText(p));}catch { throw new InvalidDataException("无法读取 "+p+"。请备份后修复该文件。"); } }
 public void Write(string name,object value) { string p=Path.Combine(Root,name),tmp=p+".tmp"; File.WriteAllText(tmp,json.Serialize(value),new UTF8Encoding(false)); if(File.Exists(p)) File.Replace(tmp,p,null); else File.Move(tmp,p); }
 public void Save(){Write("settings.json",Pref);}
 public void EnsureBuiltin(Skin s,int size) {EnsureBuiltin(s,size,System.Threading.CancellationToken.None);} public void EnsureBuiltin(Skin s,int size,System.Threading.CancellationToken token) { if(s.Builtin<0)return; string dir=Path.Combine(Root,s.Builtin>=8?"builtin-v4":"builtin-v2",s.Id,size.ToString()); Directory.CreateDirectory(dir);
  foreach(var r in Roles) { token.ThrowIfCancellationRequested();string p=Path.Combine(dir,r+(Art.IsAnimated(s.Builtin)?".ani":".cur")); if(!ValidCursorCache(p)) { var frames=new List<byte[]>(); int count=Art.IsAnimated(s.Builtin)?16:1; for(int f=0;f<count;f++) using(var bmp=Art.Draw(s.Builtin,r,size,f,count)) frames.Add(Art.Cur(bmp,Art.Hotspot(s.Builtin,r,size).X,Art.Hotspot(s.Builtin,r,size).Y)); token.ThrowIfCancellationRequested();string tmp=p+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(tmp,count==1?frames[0]:Art.Ani(frames,5));token.ThrowIfCancellationRequested();if(File.Exists(p))File.Replace(tmp,p,null);else File.Move(tmp,p);}finally{if(File.Exists(tmp))File.Delete(tmp);} } s.Roles[r]=p; }
 }
 public Snapshot Capture() { var b=new Snapshot(); using(var key=Registry.CurrentUser.OpenSubKey(@"Control Panel\Cursors",false)) foreach(string r in Roles.Concat(new[]{"","Scheme Source"})) { bool exists=key!=null&&key.GetValueNames().Contains(r); b.Values.Add(new Entry{Name=r,Exists=exists,Kind=exists?(int)key.GetValueKind(r):1,Value=exists?Convert.ToString(key.GetValue(r,null,RegistryValueOptions.DoNotExpandEnvironmentNames)):null}); } return b; }
 public void SetSnapshot(Snapshot b) { if(!AllowDesktopChanges)throw new InvalidOperationException("只读测试环境禁止改变系统光标");if(b==null)throw new InvalidOperationException("尚无可恢复的备份"); using(var key=Registry.CurrentUser.CreateSubKey(@"Control Panel\Cursors")) foreach(var e in b.Values) { if(!e.Exists)key.DeleteValue(e.Name,false); else key.SetValue(e.Name,e.Kind==(int)RegistryValueKind.DWord?(object)int.Parse(e.Value):e.Value,(RegistryValueKind)e.Kind); } Native.Reload();RestoreExactSnapshot(b); }
 public void Apply(Skin s) {Apply(s,null);} public void Apply(Skin s,Dictionary<string,string> prepared) { if(!AllowDesktopChanges)throw new InvalidOperationException("只读测试环境禁止改变系统光标");if(s==null)throw new InvalidOperationException("请先选择皮肤");var effective=prepared??PrepareSize(s,SkinSize(s),System.Threading.CancellationToken.None);if(s.Builtin>=0)s.Roles=effective;foreach(var p in effective.Values) Native.Validate(p); if(!effective.ContainsKey("Arrow"))throw new InvalidDataException("请先为皮肤设置「普通选择」光标"); var before=Capture();string oldActive=Pref.Active,oldLast=Pref.LastSkin;long oldUsed=s.LastUsed;if(!File.Exists(Path.Combine(Root,"original.json")))Write("original.json",before); Write("undo.json",before);
  try { using(var key=Registry.CurrentUser.CreateSubKey(@"Control Panel\Cursors")) { // Unmapped roles use the original scheme rather than the previous skin.
    var baseline=Read<Snapshot>("original.json"); foreach(var r in Roles) { string p; if(effective.TryGetValue(r,out p)) key.SetValue(r,p,RegistryValueKind.String); else { var e=baseline.Values.FirstOrDefault(v=>v.Name==r); if(e!=null&&e.Exists)key.SetValue(r,e.Value,(RegistryValueKind)e.Kind); else key.DeleteValue(r,false); } } key.SetValue("","Cursor Atelier · "+s.Name); key.SetValue("Scheme Source",0,RegistryValueKind.DWord); } Native.Reload(); Native.InstallExactCursors(effective);Pref.Active=s.Id;Pref.LastSkin=s.Id;s.LastUsed=DateTime.UtcNow.Ticks; Save(); }
  catch { Pref.Active=oldActive;Pref.LastSkin=oldLast;s.LastUsed=oldUsed;SetSnapshot(before); throw; }
 }
 public void Restore(bool original) { var b=Read<Snapshot>(original?"original.json":"undo.json"); var before=Capture(); SetSnapshot(b); Write("undo.json",before); Pref.Active=""; Pref.Rotate=false; Save(); }
 public void WindowsDefault() { if(!AllowDesktopChanges)throw new InvalidOperationException("只读测试环境禁止改变系统光标");var before=Capture(); if(!File.Exists(Path.Combine(Root,"original.json")))Write("original.json",before); Write("undo.json",before); try {NativeExit.WindowsDefault();Pref.Active="";Pref.Rotate=false;Save(); }catch{SetSnapshot(before);throw;} }
 public Skin Import(string[] files,int size,int hx,int hy) { if(files.Length==1&&Path.GetExtension(files[0]).ToLowerInvariant()==".zip"){if(new FileInfo(files[0]).Length>64L*1024*1024)throw new InvalidDataException("备份文件不可超过 64 MB");if(IsLibrary(files[0]))return ImportLibrary(files[0]);} string dir=Path.Combine(Root,"library",Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir); var s=new Skin{Id=Path.GetFileName(dir),Name=Path.GetFileNameWithoutExtension(files[0]),Description="本地导入 · 可在角色映射中自定义",Source="用户导入"}; int index=0; long total=0; var candidates=new List<string>();
  try { foreach(var f in files) { if(new FileInfo(f).Length>64*1024*1024)throw new InvalidDataException("单个导入文件不可超过 64 MB"); if(Path.GetExtension(f).ToLowerInvariant()==".zip") { using(var z=ZipFile.OpenRead(f)) { if(z.Entries.Count>1000)throw new InvalidDataException("压缩包条目过多"); foreach(var e in z.Entries) { total+=e.Length; if(total>64*1024*1024)throw new InvalidDataException("解压总大小不可超过 64 MB"); string ext=Path.GetExtension(e.Name).ToLowerInvariant(); if(ext!=".cur"&&ext!=".ani")continue; if(e.Length>8*1024*1024)throw new InvalidDataException("光标文件过大"); string dest=Path.Combine(dir,(index++).ToString("D3")+"_"+SafeName(e.Name)); e.ExtractToFile(dest); candidates.Add(dest); } } }
   else { string ext=Path.GetExtension(f).ToLowerInvariant(); string dest=Path.Combine(dir,(index++).ToString("D3")+"_"+SafeName(Path.GetFileName(f))); if(ext==".cur"||ext==".ani") { File.Copy(f,dest); candidates.Add(dest); } else if(new[]{".png",".gif",".bmp",".jpg",".jpeg",".ico"}.Contains(ext)) { dest=Path.ChangeExtension(dest,ext==".gif"?".ani":".cur"); Art.ConvertImage(f,dest,size,hx,hy); candidates.Add(dest); } else throw new InvalidDataException("不支持的文件："+ext); }
  } if(candidates.Count==0)throw new InvalidDataException("没有发现 .cur 或 .ani。请从 Sweezy 下载 Windows 文件或 Complete Pack ZIP。");
  foreach(var p in candidates)Native.Validate(p); foreach(var p in candidates) { string role=GuessRole(Path.GetFileNameWithoutExtension(p)); if(role!=null&&!s.Roles.ContainsKey(role))s.Roles[role]=p; }
  if(!s.Roles.ContainsKey("Arrow"))s.Roles["Arrow"]=candidates[0]; Pref.Skins.Add(s);Save();return s;
  } catch { Pref.Skins.Remove(s);Directory.Delete(dir,true);throw; }
 }
 public static string SafeName(string name) { foreach(char c in Path.GetInvalidFileNameChars())name=name.Replace(c,'_');return name.Length>160?name.Substring(name.Length-160):name; }
 public static string GuessRole(string name) { name=name.ToLowerInvariant();if(name.Contains("--pointer--"))return "Hand";if(name.Contains("--cursor--"))return "Arrow";if(name.Contains("pointer")||name.Contains("link"))return "Hand"; foreach(var r in Roles.OrderByDescending(r=>r.Length)) if(System.Text.RegularExpressions.Regex.IsMatch(name,@"(^|[^a-z])"+r.ToLowerInvariant()+@"([^a-z]|$)"))return r; if(name.Contains("cursor")||name.Contains("normal"))return "Arrow"; if(name.Contains("busy"))return "Wait"; if(name.Contains("text"))return "IBeam";return null; }
 public void Export(Skin s,string target) { if(s==null||s.Roles.Count==0)throw new InvalidOperationException("皮肤没有可导出的光标文件");foreach(string file in s.Roles.Values)Native.Validate(file);if(s.Roles.Values.Sum(f=>new FileInfo(f).Length)>64L*1024*1024)throw new InvalidDataException("单皮肤展开后不可超过 64 MB");string staged=target+"."+Guid.NewGuid().ToString("N")+".tmp";try{using(var z=ZipFile.Open(staged,ZipArchiveMode.Create)){foreach(var pair in s.Roles)z.CreateEntryFromFile(pair.Value,pair.Key+Path.GetExtension(pair.Value));var e=z.CreateEntry("README.txt");using(var w=new StreamWriter(e.Open(),Encoding.UTF8))w.Write("Cursor Atelier / "+s.Name+"\r\n"+s.Description+"\r\n"+s.Source+"\r\n按 Windows 光标角色命名。导入本工具即可使用。");}if(File.Exists(target))File.Replace(staged,target,null);else File.Move(staged,target);}finally{if(File.Exists(staged))File.Delete(staged);} }
 public void Delete(Skin s) { if(s.Builtin>=0)throw new InvalidOperationException("内置皮肤不可删除"); if(Pref.Active==s.Id)throw new InvalidOperationException("请先切换其他皮肤或恢复，再删除正在使用的皮肤"); Pref.Skins.Remove(s);Save(); // Preserve files referenced by undo snapshots.
 }
}
}
