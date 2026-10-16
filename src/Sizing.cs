using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CursorAtelier {
// Work on immutable input copies; only the UI thread commits preferences and registry changes.
public static class CursorSizing {
 public static readonly int[] Sizes={32,48,64,96};
 public static byte[] Resize(byte[] bytes,int size,CancellationToken token){
  token.ThrowIfCancellationRequested();
  if(bytes.Length<22||bytes.Length>8*1024*1024)throw new InvalidDataException("光标文件大小无效");
  if(!Sizes.Contains(size))throw new InvalidDataException("不支持的指针尺寸");
  if(Encoding.ASCII.GetString(bytes,0,4)!="RIFF")return Frame(bytes,size);
  if(Encoding.ASCII.GetString(bytes,8,4)!="ACON"||BitConverter.ToUInt32(bytes,4)!=bytes.Length-8)throw new InvalidDataException("ANI 文件结构无效");
  int frames=0;var body=Chunks(bytes,12,bytes.Length,size,token,ref frames);
  if(frames==0)throw new InvalidDataException("ANI 中没有光标帧");
  using(var ms=new MemoryStream())using(var w=new BinaryWriter(ms)){w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(body.Length+4);w.Write(Encoding.ASCII.GetBytes("ACON"));w.Write(body);if(ms.Length>8*1024*1024)throw new InvalidDataException("缩放后的动画超过 8 MB，请选择较小尺寸");return ms.ToArray();}
 }
 static byte[] Chunks(byte[] b,int start,int end,int size,CancellationToken token,ref int frames){using(var ms=new MemoryStream())using(var w=new BinaryWriter(ms)){int pos=start;while(pos<end){token.ThrowIfCancellationRequested();if(end-pos<8)throw new InvalidDataException("ANI 数据块不完整");string id=Encoding.ASCII.GetString(b,pos,4);uint length=BitConverter.ToUInt32(b,pos+4);if(length>int.MaxValue||length>end-pos-8)throw new InvalidDataException("ANI 数据块长度无效");int n=(int)length;var data=new byte[n];Buffer.BlockCopy(b,pos+8,data,0,n);
   if(id=="icon"){if(++frames>256)throw new InvalidDataException("动画最多支持 256 帧");data=Frame(data,size);}
   else if(id=="LIST"&&n>=4&&Encoding.ASCII.GetString(data,0,4)=="fram"){var nested=Chunks(b,pos+12,pos+8+n,size,token,ref frames);data=new byte[4+nested.Length];Buffer.BlockCopy(b,pos+8,data,0,4);Buffer.BlockCopy(nested,0,data,4,nested.Length);}
   else if(id=="anih"){if(n<36)throw new InvalidDataException("ANI 动画头无效");Buffer.BlockCopy(BitConverter.GetBytes(0),0,data,12,4);Buffer.BlockCopy(BitConverter.GetBytes(0),0,data,16,4);}
   w.Write(Encoding.ASCII.GetBytes(id));w.Write(data.Length);w.Write(data);if((data.Length&1)!=0)w.Write((byte)0);pos+=8+n+(n&1);if(pos>end)throw new InvalidDataException("ANI 对齐数据缺失");
  }return ms.ToArray();}}
 static byte[] Frame(byte[] b,int size){if(b.Length<22||BitConverter.ToUInt16(b,0)!=0)throw new InvalidDataException("光标帧无效");int type=BitConverter.ToUInt16(b,2),count=BitConverter.ToUInt16(b,4);if((type!=1&&type!=2)||count<1||count>256||6+16*count>b.Length)throw new InvalidDataException("光标目录无效");int best=-1;double score=double.MaxValue;
  for(int i=0;i<count;i++){int p=6+i*16;int width=b[p]==0?256:b[p],height=b[p+1]==0?256:b[p+1];double d=Math.Abs(Math.Log((double)Math.Max(width,height)/size));if(d<score){score=d;best=p;}}
  int ow=b[best]==0?256:b[best],oh=b[best+1]==0?256:b[best+1];uint len=BitConverter.ToUInt32(b,best+8),off=BitConverter.ToUInt32(b,best+12);if(len==0||off>b.Length||len>b.Length-off)throw new InvalidDataException("光标位图越界");
  int hx=type==2?BitConverter.ToUInt16(b,best+4):ow/2,hy=type==2?BitConverter.ToUInt16(b,best+6):oh/2;if(hx>=ow||hy>=oh)throw new InvalidDataException("光标热点超出图像范围");
  // An ICO wrapper lets Windows/GDI+ decode PNG, indexed and true-color DIB entries.
  using(var ms=new MemoryStream())using(var w=new BinaryWriter(ms)){w.Write((ushort)0);w.Write((ushort)1);w.Write((ushort)1);w.Write(b,best,4);w.Write((ushort)1);w.Write((ushort)0);w.Write(len);w.Write(22);w.Write(b,(int)off,(int)len);ms.Position=0;
   using(var icon=new Icon(ms,ow,oh))using(var source=icon.ToBitmap()){float scale=(float)size/Math.Max(ow,oh);int nw=Math.Max(1,(int)Math.Round(ow*scale)),nh=Math.Max(1,(int)Math.Round(oh*scale));using(var image=new Bitmap(nw,nh,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(image)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.DrawImage(source,new Rectangle(0,0,nw,nh),0,0,source.Width,source.Height,GraphicsUnit.Pixel);return Art.Cur(image,Math.Min(nw-1,(int)Math.Round(hx*(double)nw/ow)),Math.Min(nh-1,(int)Math.Round(hy*(double)nh/oh)));}}
  }
 }
}
public partial class Store {
 static readonly object sizeLock=new object();
 internal static bool ValidCursorCache(string path){if(!File.Exists(path))return false;try{Native.Validate(path);return true;}catch(InvalidDataException){return false;}catch(IOException){return false;}}
 public int SkinSize(Skin s){return s.SizeOverride==0?Pref.Size:s.SizeOverride;}
 public Dictionary<string,string> PrepareSize(Skin s,int size,CancellationToken token){
  lock(sizeLock){token.ThrowIfCancellationRequested();
  if(s.Builtin>=0){var copy=new Skin{Id=s.Id,Builtin=s.Builtin};EnsureBuiltin(copy,size,token);return copy.Roles;}
  var result=new Dictionary<string,string>();foreach(var pair in s.Roles){token.ThrowIfCancellationRequested();if(size<0){Native.Validate(pair.Value);result[pair.Key]=pair.Value;continue;}byte[] source=File.ReadAllBytes(pair.Value);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(source)).Replace("-","");string dir=Path.Combine(Root,"scaled-v1",hash,size.ToString());Directory.CreateDirectory(dir);string dest=Path.Combine(dir,"cursor"+Path.GetExtension(pair.Value).ToLowerInvariant());if(!ValidCursorCache(dest)){var output=CursorSizing.Resize(source,size,token);string tmp=dest+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(tmp,output);token.ThrowIfCancellationRequested();if(File.Exists(dest))File.Replace(tmp,dest,null);else File.Move(tmp,dest);}finally{if(File.Exists(tmp))File.Delete(tmp);}}Native.Validate(dest);result[pair.Key]=dest;}return result;
  }
 }
}
public partial class MainForm {
 string SizeDescription(Skin s){string text=store.SkinSize(s)<0?"原始尺寸":store.SkinSize(s)+" px";if(store.AllowDesktopChanges&&s.Id==store.Pref.Active){var actual=Native.SystemCursorSize("Arrow");return text+" · 系统 "+actual.Width+" × "+actual.Height+" px";}return text+" · 预览，应用后生效";}
 CancellationTokenSource sizeWork;int sizeGeneration;internal Task SizeTask=Task.FromResult(0);
 void CancelSizeWork(){sizeGeneration++;if(sizeWork!=null)sizeWork.Cancel();}
 void QuickApply(Skin s){SizeTask=ResizeSkinAsync(s,true);}
 async Task ResizeSkinAsync(Skin s,bool applyToWindows){
  // Also establish the UI context when invoked before Application.Run or after a modal loop.
  if(!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
  if(IsDisposed||Disposing||reallyClose)return;CancelSizeWork();int generation=sizeGeneration;var cts=new CancellationTokenSource();sizeWork=cts;int size=store.SkinSize(s);var input=new Skin{Id=s.Id,Builtin=s.Builtin,Roles=new Dictionary<string,string>(s.Roles)};
  status.Text="正在后台准备「"+s.Name+"」"+(size<0?"原始尺寸":size+" px")+"… 可继续浏览或选择其他尺寸";
  try{var files=await Task.Run(()=>store.PrepareSize(input,size,cts.Token));if(cts.IsCancellationRequested||generation!=sizeGeneration||IsDisposed||Disposing||reallyClose)return;
   if(s.Builtin>=0)s.Roles=files;
   s.PreviewRoles=files;
   if(applyToWindows){store.Apply(s,files);selected=s;}
   RefreshCards();if(selected!=null)SelectSkin(selected);status.Text=applyToWindows?"已应用「"+s.Name+"」 · "+(size<0?"原始尺寸":size+" px"):"尺寸预览已更新 · 点击应用到 Windows 生效";
  }catch(OperationCanceledException){}catch(Exception error){if(generation==sizeGeneration&&!IsDisposed&&!Disposing&&!reallyClose){status.Text="尺寸调整失败，当前系统指针保持原状："+error.Message;Theme.Error(error);}}
  finally{if(sizeWork==cts)sizeWork=null;cts.Dispose();}
 }
 void AddSizeControl(Skin s){var label=Theme.Label("指针大小",9,Theme.Muted);label.Location=new Point(18,370);detail.Controls.Add(label);var combo=new DarkCombo{DropDownStyle=ComboBoxStyle.DropDownList};combo.SetBounds(99,364,164,29);combo.Items.Add("跟随全局 ("+store.Pref.Size+" px)");foreach(int n in CursorSizing.Sizes)combo.Items.Add(n+" px");if(s.Builtin<0)combo.Items.Add("保留原始尺寸");combo.SelectedIndex=s.SizeOverride==0?0:s.SizeOverride<0?5:Array.IndexOf(CursorSizing.Sizes,s.SizeOverride)+1;combo.SelectedIndexChanged+=(o,e)=>{s.SizeOverride=combo.SelectedIndex==0?0:combo.SelectedIndex==5?-1:CursorSizing.Sizes[combo.SelectedIndex-1];store.Save();SizeTask=ResizeSkinAsync(s,store.Pref.Active==s.Id);};detail.Controls.Add(combo);var hint=Theme.Label("内置 / 导入均可调节，热点随尺寸等比缩放\n当前皮肤调整后自动生效，其他皮肤先预览",8,Theme.Muted);hint.Location=new Point(18,601);detail.Controls.Add(hint);}
}
}
