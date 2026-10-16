using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Compression;
using System.Text;

namespace CursorAtelier {
public static partial class Theme {
 public static readonly string[] Ids={"midnight","sakura","sky","violet","forest","sand"};
 public static readonly string[] Names={"午夜薄荷","樱花奶油","晴空白","紫夜霓虹","森林微光","暖沙拿铁"};
 static readonly string[][] Palettes={
  new[]{"#101520","#171E2C","#1F2839","#EBF1FA","#94A4BB","#83E8CD"},
  new[]{"#FFF4F7","#FFFFFF","#F6E3EB","#422C3B","#7D5C70","#B52D71"},
  new[]{"#EFF5FC","#FFFFFF","#E0EAF7","#20334D","#596F8D","#2563B9"},
  new[]{"#191326","#241D35","#322742","#F2EAFD","#B5A0CC","#CCA7FF"},
  new[]{"#10221C","#192E26","#233D33","#EBF6EF","#9BB9A7","#96DBA6"},
  new[]{"#F7F0E5","#FFFCF5","#EAE0CE","#463A2D","#78664E","#966337"}};
 public static Color[] Current(){return new[]{Bg,Panel,Raised,Text,Muted,Accent};}
 public static Color[] Palette(int i){return Palettes[i].Select(ColorTranslator.FromHtml).ToArray();}
 public static double Luma(Color c){Func<int,double> linear=v=>v/255.0<=0.04045?v/255.0/12.92:Math.Pow((v/255.0+0.055)/1.055,2.4);return .2126*linear(c.R)+.7152*linear(c.G)+.0722*linear(c.B);}
 public static double Contrast(Color a,Color b){double x=Luma(a),y=Luma(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);}
 public static Color OnAccent{get{return Contrast(Color.Black,Accent)>Contrast(Color.White,Accent)?Color.FromArgb(16,21,32):Color.White;}}
 public static void Switch(string id,string custom){Color[] old=Current();int i=Array.IndexOf(Ids,id);if(i<0)i=0;var p=Palette(i);Bg=p[0];Panel=p[1];Raised=p[2];Text=p[3];Muted=p[4];Accent=p[5];if(!string.IsNullOrWhiteSpace(custom))try{Accent=ColorTranslator.FromHtml(custom);}catch{}
  foreach(Form f in Application.OpenForms.Cast<Form>().ToArray()){Recolor(f,old);f.Invalidate(true);}
 }
 static Color Map(Color c,Color[] old){var p=Current();for(int i=0;i<old.Length;i++)if(c.ToArgb()==old[i].ToArgb())return p[i];return c;}
 static void Recolor(Control c,Color[] old){if(Convert.ToString(c.Tag)=="palette")return;c.BackColor=Map(c.BackColor,old);c.ForeColor=Map(c.ForeColor,old);if(Convert.ToString(c.Tag)=="primary"){c.BackColor=Accent;c.ForeColor=OnAccent;}foreach(Control child in c.Controls)Recolor(child,old);}
}
public class AboutDialog:Form {
 // 开源仓库地址：GitHub 页面与 Release 下载页共用同一地址，点击后由系统默认浏览器打开。
 const string RepoUrl="https://github.com/jingpanlong/CursorAtelier";
 public AboutDialog(){Theme.Style(this,"关于 · 光标工坊",620,628);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;
  var title=Theme.Label("↖  Cursor Atelier",25,Theme.Accent);title.Location=new Point(30,25);Controls.Add(title);var sub=Theme.Label("光标工坊 1.6.3  /  把桌面变成你的游乐场",13,Theme.Text);sub.Location=new Point(32,83);Controls.Add(sub);
  var intro=Theme.Label("一款轻巧的 Windows 鼠标换肤工具。\n54 款原创皮肤、动态预览、Sweezy 导入、图片转换，\n搭配主题配色、收藏轮换和随时可用的备份恢复。",10,Theme.Muted);intro.Location=new Point(32,137);Controls.Add(intro);
  var author=Theme.Label("联系作者",15,Theme.Text);author.Location=new Point(32,237);Controls.Add(author);var contact=Theme.Label("微信公众号：景同志\n\n小红书：景同学",12,Theme.Text);contact.Location=new Point(32,282);Controls.Add(contact);
  var source=Theme.Label("开源地址 · GitHub",15,Theme.Text);source.Location=new Point(32,352);Controls.Add(source);
  var link=Theme.Label(RepoUrl,12,Theme.Accent);link.Location=new Point(32,395);link.Cursor=Cursors.Hand;link.Click+=(o,e)=>Theme.Open(RepoUrl);Controls.Add(link);
  var hint=Theme.Label("点击上方链接即可打开项目主页，在 Releases 中下载单文件版与安装版。",9,Theme.Muted);hint.Location=new Point(32,431);Controls.Add(hint);
  var feedback=Theme.Label("欢迎交流皮肤创意、反馈问题和提出功能建议；开源版欢迎在 GitHub 提交 Issue 与 Star。",9,Theme.Muted);feedback.Location=new Point(32,464);Controls.Add(feedback);
  var copy1=Theme.Button("复制公众号名",(o,e)=>Copy("景同志",feedback));copy1.SetBounds(32,510,130,38);Controls.Add(copy1);var copy2=Theme.Button("复制小红书名",(o,e)=>Copy("景同学",feedback));copy2.SetBounds(174,510,130,38);Controls.Add(copy2);var copy3=Theme.Button("复制下载地址",(o,e)=>Copy(RepoUrl,feedback));copy3.SetBounds(316,510,130,38);Controls.Add(copy3);var open=Theme.Button("打开 GitHub",(o,e)=>Theme.Open(RepoUrl),true);open.SetBounds(458,510,130,38);Controls.Add(open);
 }
 static void Copy(string value,Label label){try{Clipboard.SetText(value);label.Text="已复制："+value;}catch(Exception e){Theme.Error(e);}}
}
public class AppearanceDialog:Form {
 string currentId,currentAccent,originalId,originalAccent;Store store;Label sample;
 public AppearanceDialog(Store s){store=s;originalId=currentId=s.Pref.ThemeId;originalAccent=currentAccent=s.Pref.Accent;Theme.Style(this,"主题配色 · 即时预览",690,590);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;
  var title=Theme.Label("挑一种舒服的颜色",20,Theme.Text);title.Location=new Point(25,24);Controls.Add(title);var hint=Theme.Label("点击即预览，保存后记住选择；关闭窗口可取消更改。",9,Theme.Muted);hint.Location=new Point(27,75);Controls.Add(hint);
  for(int i=0;i<Theme.Ids.Length;i++){int index=i;var p=Theme.Palette(i);var card=new Button{Text=Theme.Names[i],BackColor=p[0],ForeColor=p[3],Font=Theme.Font(12),TextAlign=ContentAlignment.BottomCenter,FlatStyle=FlatStyle.Flat,Location=new Point(26+(i%3)*211,117+(i/3)*127),Size=new Size(196,110),Padding=new Padding(0,0,0,12),Cursor=Cursors.Hand,Tag="palette"};card.FlatAppearance.BorderColor=p[5];card.Paint+=(o,e)=>{for(int n=0;n<3;n++)using(var b=new SolidBrush(p[n==0?5:n]))e.Graphics.FillEllipse(b,41+n*34,22,25,25);};card.Click+=(o,e)=>{currentId=Theme.Ids[index];currentAccent="";Theme.Switch(currentId,currentAccent);sample.Text="当前主题："+Theme.Names[index];};Controls.Add(card);}
  sample=Theme.Label("也可以选择你喜欢的强调色。",10,Theme.Muted);sample.Location=new Point(28,391);Controls.Add(sample);var custom=Theme.Button("自定义强调色…",(o,e)=>{using(var d=new ColorDialog{Color=Theme.Accent,FullOpen=true})if(d.ShowDialog(this)==DialogResult.OK){currentAccent="#"+d.Color.R.ToString("X2")+d.Color.G.ToString("X2")+d.Color.B.ToString("X2");Theme.Switch(currentId,currentAccent);sample.Text="自定义强调色："+currentAccent;}});custom.SetBounds(27,431,196,36);Controls.Add(custom);var reset=Theme.Button("恢复该主题默认色",(o,e)=>{currentAccent="";Theme.Switch(currentId,"");sample.Text="已恢复当前主题默认强调色。";});reset.SetBounds(238,431,196,36);Controls.Add(reset);var save=Theme.Button("保存配色",(o,e)=>{store.Pref.ThemeId=currentId;store.Pref.Accent=currentAccent;store.Save();DialogResult=DialogResult.OK;},true);save.SetBounds(27,489,618,38);Controls.Add(save);FormClosing+=(o,e)=>{if(DialogResult!=DialogResult.OK)Theme.Switch(originalId,originalAccent);};
 }
}
public class LibraryItem {public string Name,Entry;public bool Favorite;}
public class LibraryIndex {public int Version=1;public List<LibraryItem> Skins=new List<LibraryItem>();}
public partial class Store {
 public Skin Clone(Skin source){string tmp=Path.Combine(Root,"clone-"+Guid.NewGuid().ToString("N")+".zip");try{Export(source,tmp);var clone=Import(new[]{tmp},Pref.Size,0,0);clone.Name=source.Name+" · 副本";clone.Favorite=false;Save();return clone;}finally{if(File.Exists(tmp))File.Delete(tmp);}}
 public bool IsLibrary(string path){using(var z=ZipFile.OpenRead(path))return z.GetEntry("atelier-library.json")!=null;}
 public void ExportLibrary(IEnumerable<Skin> skins,string target){var list=skins.ToList();if(list.Count==0)throw new InvalidOperationException("没有可导出的皮肤");if(list.Count>100)throw new InvalidOperationException("一次最多备份 100 款皮肤");string temp=Path.Combine(Root,"export-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);string staged=target+"."+Guid.NewGuid().ToString("N")+".tmp";try{if(File.Exists(staged))File.Delete(staged);using(var archive=ZipFile.Open(staged,ZipArchiveMode.Create)){var index=new LibraryIndex();int i=0;foreach(var skin in list){string name="skin-"+(i++).ToString("D3")+".zip",file=Path.Combine(temp,name);Export(skin,file);archive.CreateEntryFromFile(file,name);index.Skins.Add(new LibraryItem{Name=skin.Name,Entry=name,Favorite=skin.Favorite});}var entry=archive.CreateEntry("atelier-library.json");using(var writer=new StreamWriter(entry.Open(),Encoding.UTF8))writer.Write(json.Serialize(index));}if(File.Exists(target))File.Replace(staged,target,null);else File.Move(staged,target);}finally{Directory.Delete(temp,true);if(File.Exists(staged))File.Delete(staged);}}
 public Skin ImportLibrary(string path){string temp=Path.Combine(Root,"import-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);var before=Pref.Skins.ToList();try{Skin last=null;using(var archive=ZipFile.OpenRead(path)){if(archive.Entries.Count>101||archive.Entries.Sum(e=>e.Length)>64L*1024*1024)throw new InvalidDataException("备份包过大或条目过多");var manifest=archive.GetEntry("atelier-library.json");if(manifest==null||manifest.Length>1024*1024)throw new InvalidDataException("无效的备份索引");LibraryIndex index;using(var reader=new StreamReader(manifest.Open()))index=json.Deserialize<LibraryIndex>(reader.ReadToEnd());if(index==null||index.Version!=1||index.Skins==null||index.Skins.Count==0||index.Skins.Count>100)throw new InvalidDataException("不支持的备份格式");long total=0;int i=0;foreach(var item in index.Skins){if(string.IsNullOrEmpty(item.Entry)||string.IsNullOrWhiteSpace(item.Name))throw new InvalidDataException("备份索引不完整");var entry=archive.GetEntry(item.Entry);if(entry==null||!item.Entry.EndsWith(".zip",StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("备份缺少皮肤文件");string file=Path.Combine(temp,(i++).ToString()+".zip");entry.ExtractToFile(file);if(IsLibrary(file))throw new InvalidDataException("不支持嵌套的皮肤库备份");last=Import(new[]{file},Pref.Size,0,0);last.Name=item.Name.Substring(0,Math.Min(item.Name.Length,100));last.Favorite=item.Favorite;total+=Directory.GetFiles(Path.Combine(Root,"library",last.Id)).Sum(f=>new FileInfo(f).Length);if(total>128L*1024*1024)throw new InvalidDataException("皮肤库解压总量超过 128 MB");}}Save();return last;}catch{Pref.Skins=before;Save();throw;}finally{Directory.Delete(temp,true);}}
}
public partial class MainForm {
 public void ShowCategory(string category){filter=category;title.Text=category;RefreshCards();if(category=="趣味造型")SelectSkin(store.Pref.Skins.First(s=>s.Builtin==8));if(category=="新款皮肤")SelectSkin(store.Pref.Skins.First(s=>s.Builtin==39));}
 void BuildLibraryControls(Panel header){var top=new Panel{Dock=DockStyle.Right,Width=174};var l=Theme.Label("皮肤排序",8,Theme.Muted);l.Location=new Point(0,2);top.Controls.Add(l);var sort=Theme.Button(store.Pref.Sort+"  ▾",null);sort.SetBounds(0,25,165,29);var menu=new SafeMenu(this);SortMenu=menu;foreach(string option in new[]{"默认顺序","名称排序","收藏优先","最近使用"}){string value=option;menu.Action(value,()=>{store.Pref.Sort=value;sort.Text=value+"  ▾";store.Save();RefreshCards();});}sort.Click+=(o,e)=>menu.Show(sort,new Point(0,sort.Height));sort.Disposed+=(o,e)=>menu.DisposeLater();top.Controls.Add(sort);AddStartupSwitch(top);top.BringToFront();header.Controls.Add(top);}
 List<Skin> SortSkins(List<Skin> list){if(filter=="最近使用"||store.Pref.Sort=="最近使用")return list.OrderByDescending(s=>s.LastUsed).ToList();if(store.Pref.Sort=="名称排序")return list.OrderBy(s=>s.Name).ToList();if(store.Pref.Sort=="收藏优先")return list.OrderByDescending(s=>s.Favorite).ThenBy(s=>s.Name).ToList();return list;}

 internal SafeMenu CreateToolsMenu(){var menu=new SafeMenu(this,true);menu.Action(store.Pref.Rotate?"暂停自动轮换":"开始自动轮换",()=>Run(()=>{store.Pref.Rotate=!store.Pref.Rotate;nextRotate=DateTime.Now.AddMinutes(store.Pref.RotationMinutes);store.Save();},store.Pref.Rotate?"已暂停自动轮换":"已开启自动轮换"));menu.Action("复制选中皮肤为可编辑副本",()=>Run(()=>{selected=store.Clone(selected);},"已创建副本，可修改角色映射"));menu.Items.Add(new ToolStripSeparator());menu.Action("备份全部皮肤…",()=>ExportCollection(false));menu.Action("仅备份收藏皮肤…",()=>ExportCollection(true));menu.Action("导入皮肤库备份…",()=>ChooseImport());menu.Items.Add(new ToolStripSeparator());menu.Action("清空搜索与筛选",()=>{filter="全部皮肤";title.Text=filter;search.Clear();RefreshCards();});menu.Action("Windows 鼠标设置",()=>Theme.Open("ms-settings:mousetouchpad"));menu.Action("打开错误日志",()=>{Directory.CreateDirectory(ErrorLog.DirectoryPath);Theme.Open(ErrorLog.DirectoryPath);});return menu;} void MoreTools(Control anchor){var menu=CreateToolsMenu();menu.Show(anchor,new Point(anchor.Width,0));}
 void ExportCollection(bool favorites){var skins=store.Pref.Skins.Where(s=>!favorites||s.Favorite).ToList();if(skins.Count==0){MessageBox.Show("请先收藏一些皮肤。","皮肤库备份");return;}using(var d=new SaveFileDialog{Title="备份皮肤库（导入后作为个人皮肤添加）",Filter="皮肤库 ZIP 备份|*.zip",FileName="光标工坊-"+(favorites?"收藏":"全部")+"-"+DateTime.Now.ToString("yyyyMMdd")+".zip"})if(d.ShowDialog(this)==DialogResult.OK)Run(()=>store.ExportLibrary(skins,d.FileName),"已备份 "+skins.Count+" 款皮肤及收藏状态");}
}
}
