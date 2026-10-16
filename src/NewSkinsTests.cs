using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace CursorAtelier {
static partial class App {
 static int NewSkinsTest(string directory){Directory.CreateDirectory(directory);ErrorLog.DirectoryPath=Path.Combine(directory,"logs");var store=new Store(Path.Combine(directory,"data"));var baseline=store.Capture();store.Write("emergency-baseline.json",baseline);var log=new List<string>();int roles=0;try{
  var added=store.Pref.Skins.Where(s=>s.Builtin>=39).ToList();Assert(added.Count==15&&added.Count(s=>s.Animated)==9,"15 added, nine animated");var pictures=new HashSet<string>();foreach(var skin in added){using(var picture=Art.Draw(skin.Builtin,"Arrow",96,0,16))using(var stream=new MemoryStream()){picture.Save(stream,System.Drawing.Imaging.ImageFormat.Png);Assert(pictures.Add(Convert.ToBase64String(stream.ToArray())),"distinct new silhouette");}
   foreach(int size in CursorSizing.Sizes){skin.SizeOverride=size;store.Apply(skin);foreach(var role in Store.Roles){var actual=Native.SystemCursorSize(role);Assert(actual==new Size(size,size),"actual Windows "+skin.Name+" / "+role+" size "+size);int index=Array.IndexOf(Store.Roles,role);var system=Native.LoadCursor(IntPtr.Zero,new IntPtr(Native.CursorIds[index]));Assert(Native.CursorHotspot(system)==Art.Hotspot(skin.Builtin,role,size),"actual hotspot "+skin.Name+" / "+role);roles++;}var frames=CursorFrames(File.ReadAllBytes(skin.Roles["Arrow"]));Assert(frames.Count==(skin.Animated?16:1),"frame count");if(skin.Animated){Assert(frames.Select(Convert.ToBase64String).Distinct().Count()>1,"animated art has varying frames");CheckAnimation();}using(var preview=new Preview{Playground=true}){preview.LoadFile(skin.Roles["Arrow"]);Assert(preview.NativeSize==new Size(size,size),"exact preview size");}}
  }log.Add("PASS: 15 distinct additions, nine 16-frame animations and six static skins.");log.Add("PASS: "+roles+" actual shared Windows role/size combinations (15 skins x 17 roles x four sizes), with exact sizes and correct click hotspots.");log.Add("PASS: all new animations vary in encoded frames and still animate as Windows system cursors; previews match actual sizes.");
  // Upgrade a real previous-format library without replacing its existing records.
  string legacyRoot=Path.Combine(directory,"upgrade");Directory.CreateDirectory(legacyRoot);var old=new Preferences{Size=64,ThemeId="sky",Skins=store.Pref.Skins.Where(s=>s.Builtin<39).ToList()};old.Skins[0].Favorite=true;old.Skins[1].SizeOverride=96;File.WriteAllText(Path.Combine(legacyRoot,"settings.json"),new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(old));var upgraded=new Store(legacyRoot,false);Assert(upgraded.Pref.Skins.Count==54&&upgraded.Pref.Skins[0].Favorite&&upgraded.Pref.Skins[1].SizeOverride==96&&upgraded.Pref.Size==64&&upgraded.Pref.ThemeId=="sky","preserve prior library");log.Add("PASS: previous 39-skin library upgrades to 54 while preserving favorites, global/individual sizes and theme.");
  }finally{store.SetSnapshot(baseline);log.Add("FINALLY: original Windows cursor scheme restored.");File.WriteAllLines(Path.Combine(directory,"new-skins-results.txt"),log,System.Text.Encoding.UTF8);}return 0;
 }
}
}
