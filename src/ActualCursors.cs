using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace CursorAtelier {
public static partial class Native {
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr LoadImage(IntPtr instance,string file,uint type,int width,int height,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern IntPtr LoadCursor(IntPtr instance,IntPtr id);
 [DllImport("user32.dll",SetLastError=true)] static extern bool SetSystemCursor(IntPtr cursor,uint id);
 [DllImport("user32.dll",SetLastError=true)] static extern bool GetIconInfo(IntPtr cursor,out IconInfo info);
 [DllImport("gdi32.dll",EntryPoint="GetObjectW")] static extern int GetBitmapObject(IntPtr bitmap,int size,out BitmapInfo info);
 [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr bitmap);
 [StructLayout(LayoutKind.Sequential)] struct IconInfo {public bool Icon;public uint X,Y;public IntPtr Mask,Color;}
 [StructLayout(LayoutKind.Sequential)] struct BitmapInfo {public int Type,Width,Height,WidthBytes;public ushort Planes,BitsPixel;public IntPtr Bits;}
 public static readonly uint[] CursorIds={32512,32649,32513,32514,32650,32515,32651,32648,32646,32645,32644,32642,32643,32516,32631,32671,32672};
 public static Size CursorDimensions(IntPtr handle){IconInfo info;if(handle==IntPtr.Zero||!GetIconInfo(handle,out info))throw new Win32Exception(Marshal.GetLastWin32Error(),"无法读取实际指针尺寸");try{BitmapInfo bitmap;if(GetBitmapObject(info.Color!=IntPtr.Zero?info.Color:info.Mask,Marshal.SizeOf(typeof(BitmapInfo)),out bitmap)==0)throw new Win32Exception(Marshal.GetLastWin32Error());return new Size(bitmap.Width,info.Color==IntPtr.Zero?bitmap.Height/2:bitmap.Height);}finally{if(info.Color!=IntPtr.Zero)DeleteObject(info.Color);if(info.Mask!=IntPtr.Zero)DeleteObject(info.Mask);}}
 // LR_LOADFROMFILE only: LR_DEFAULTSIZE would normalize every cursor to the Windows metric.
 public static IntPtr LoadExactCursor(string file,int width=0,int height=0){var h=LoadImage(IntPtr.Zero,file,2,width,height,0x10);if(h==IntPtr.Zero)throw new Win32Exception(Marshal.GetLastWin32Error(),"无法按原生尺寸载入指针："+Path.GetFileName(file));return h;}
 public static Size SystemCursorSize(string role){int index=Array.IndexOf(Store.Roles,role);if(index<0)throw new ArgumentException("未知指针角色");return CursorDimensions(LoadCursor(IntPtr.Zero,new IntPtr(CursorIds[index])));}
 public static Point CursorHotspot(IntPtr cursor){IconInfo info;if(!GetIconInfo(cursor,out info))throw new Win32Exception(Marshal.GetLastWin32Error());try{return new Point((int)info.X,(int)info.Y);}finally{if(info.Color!=IntPtr.Zero)DeleteObject(info.Color);if(info.Mask!=IntPtr.Zero)DeleteObject(info.Mask);}}
 public static Bitmap DrawSystemCursor(string role,uint frame){int index=Array.IndexOf(Store.Roles,role);var cursor=LoadCursor(IntPtr.Zero,new IntPtr(CursorIds[index]));var size=CursorDimensions(cursor);var image=new Bitmap(size.Width,size.Height);using(var g=Graphics.FromImage(image)){g.Clear(Color.FromArgb(234,238,245));var dc=g.GetHdc();try{if(!DrawIconEx(dc,0,0,cursor,size.Width,size.Height,frame,IntPtr.Zero,3))throw new Win32Exception(Marshal.GetLastWin32Error(),"Cannot draw actual animation frame "+frame);}finally{g.ReleaseHdc(dc);}}return image;}
 public static void InstallExactCursors(Dictionary<string,string> files){
  // Load everything before touching the desktop. SetSystemCursor takes ownership of each handle.
  var handles=new Dictionary<string,IntPtr>();var expected=new Dictionary<string,Size>();try{foreach(var pair in files){var h=LoadExactCursor(pair.Value);handles.Add(pair.Key,h);expected[pair.Key]=CursorDimensions(h);}foreach(var pair in handles.ToArray()){int index=Array.IndexOf(Store.Roles,pair.Key);if(index<0)continue;var h=pair.Value;handles.Remove(pair.Key);if(!SetSystemCursor(h,CursorIds[index]))throw new Win32Exception(Marshal.GetLastWin32Error(),"无法替换系统指针："+pair.Key);}foreach(var pair in expected)if(SystemCursorSize(pair.Key)!=pair.Value)throw new IOException("Windows 实际指针尺寸不匹配："+pair.Key+"，未完成应用");}finally{foreach(var h in handles.Values)DestroyCursor(h);}
 }
}
public partial class Store {
 void RestoreExactSnapshot(Snapshot snapshot){var name=snapshot.Values.FirstOrDefault(v=>v.Name=="");if(name==null||name.Value==null||!name.Value.StartsWith("Cursor Atelier · ",StringComparison.Ordinal))return;var files=new Dictionary<string,string>();foreach(var e in snapshot.Values){if(!e.Exists||!Roles.Contains(e.Name)||string.IsNullOrEmpty(e.Value))continue;string file=Environment.ExpandEnvironmentVariables(e.Value);if(File.Exists(file))files[e.Name]=file;}Native.InstallExactCursors(files);}
}
static partial class App {
 static int NativeProbe(string directory){Directory.CreateDirectory(directory);var store=new Store(Path.Combine(directory,"data"),false);var lines=new List<string>();foreach(int theme in new[]{0,4,22})foreach(int size in new[]{32,96}){var files=store.PrepareSize(store.Pref.Skins[theme],size,System.Threading.CancellationToken.None);string file=files["Arrow"];var old=Native.LoadCursorFromFile(file);var exact=Native.LoadExactCursor(file);try{lines.Add("theme="+theme+" file="+size+" default-loader="+Native.CursorDimensions(old)+" exact-loader="+Native.CursorDimensions(exact));}finally{Native.DestroyCursor(old);Native.DestroyCursor(exact);}}File.WriteAllLines(Path.Combine(directory,"probe.txt"),lines);return 0;}
}
}
