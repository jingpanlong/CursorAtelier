using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CursorAtelier {
public static partial class Art {
 static void DrawSecondExtra(Graphics g,int theme,int frame,int count){
  float phase=frame*6.283185f/Math.Max(1,count),wave=(float)Math.Sin(phase);Color c=ColorTranslator.FromHtml(Colors[theme]);
  using(var fill=new SolidBrush(c))using(var ink=new SolidBrush(Color.FromArgb(54,49,70)))using(var line=new Pen(Color.FromArgb(54,49,70),1.7f))using(var edge=new Pen(Color.FromArgb(255,252,245),2.4f)){
   line.StartCap=line.EndCap=LineCap.Round;edge.LineJoin=LineJoin.Round;
   if(theme==39){ // Corgi: upright ears, white blaze, paws and round cheeks.
    g.FillEllipse(fill,16,32,33,27);g.DrawEllipse(edge,16,32,33,27);g.FillEllipse(Brushes.Cornsilk,24,35,17,22);
    Shape(g,fill,edge,new[]{new PointF(10,29),new PointF(9,4),new PointF(26,18),new PointF(39,18),new PointF(55,4),new PointF(54,33),new PointF(44,44),new PointF(20,44)});
    Shape(g,Brushes.MistyRose,edge,new[]{new PointF(13,11),new PointF(23,22),new PointF(14,24)});Shape(g,Brushes.MistyRose,edge,new[]{new PointF(51,11),new PointF(42,22),new PointF(50,24)});
    Shape(g,Brushes.FloralWhite,edge,new[]{new PointF(32,18),new PointF(38,31),new PointF(49,35),new PointF(42,43),new PointF(23,43),new PointF(15,35),new PointF(27,31)});
    g.FillEllipse(ink,20,26,4,5);g.FillEllipse(ink,41,26,4,5);g.FillEllipse(ink,29,33,7,5);g.DrawArc(line,27,37,12,6,0,180);g.FillEllipse(Brushes.Pink,29,40,7,5);g.FillEllipse(Brushes.FloralWhite,16,52,14,9);g.FillEllipse(Brushes.FloralWhite,36,52,14,9);
   }
   if(theme==40){ // Penguin hugging a little golden star.
    g.FillEllipse(fill,13,5,39,54);g.DrawEllipse(edge,13,5,39,54);g.FillEllipse(Brushes.FloralWhite,18,19,29,36);g.FillEllipse(ink,23,20,3,5);g.FillEllipse(ink,39,20,3,5);Shape(g,Brushes.SandyBrown,edge,new[]{new PointF(28,28),new PointF(37,28),new PointF(32,33)});
    using(var scarf=new Pen(Color.FromArgb(231,161,175),5)){g.DrawArc(scarf,16,21,33,23,10,160);g.DrawLine(scarf,41,35,45,48);}g.FillEllipse(Brushes.SandyBrown,15,54,16,8);g.FillEllipse(Brushes.SandyBrown,35,54,16,8);Shape(g,Brushes.Khaki,edge,Star(30,42,12,5,5,-1.57f));g.FillEllipse(fill,10,32,12,20);g.FillEllipse(fill,43,32,12,20);
   }
   if(theme==41){ // Koi: tapered body, red patches and gently swishing fins.
    float sway=wave*4;Shape(g,fill,edge,new[]{new PointF(41,36),new PointF(59,19+sway),new PointF(57,35),new PointF(62,47-sway),new PointF(43,45)});
    using(var body=new GraphicsPath()){body.AddBezier(5,33,17,7,40,14,50,37);body.AddBezier(50,37,35,59,13,57,5,33);body.CloseFigure();g.FillPath(Brushes.FloralWhite,body);g.DrawPath(edge,body);}
    g.FillEllipse(fill,14,23,12,14);g.FillEllipse(fill,28,37,14,12);g.FillEllipse(ink,12,31,3,5);g.DrawArc(line,5,33,11,9,30,110);using(var fin=new Pen(c,3)){g.DrawArc(fin,25,12+wave,16,22,190,140);g.DrawLine(fin,26,46,29,56+wave);}g.DrawBezier(line,7,38,1,41,4,48,8,46);
   }
   if(theme==42){ // Seahorse: long nose, dorsal fin and a curled tail.
    float bob=wave*2;Shape(g,Brushes.LavenderBlush,edge,new[]{new PointF(40,24+bob),new PointF(53,22+bob),new PointF(49,33+bob),new PointF(54,39+bob),new PointF(41,41+bob)});
    using(var body=new GraphicsPath()){body.AddBezier(18,15+bob,22,0+bob,47,5+bob,44,23+bob);body.AddBezier(44,23+bob,42,34+bob,33,35+bob,34,43+bob);body.AddBezier(34,43+bob,51,39+bob,54,59+bob,39,60+bob);body.AddBezier(39,60+bob,26,62+bob,22,51+bob,29,48+bob);body.AddBezier(29,48+bob,19,42+bob,20,34+bob,25,28+bob);body.AddLine(25,28+bob,9,25+bob);body.AddLine(9,25+bob,9,18+bob);body.AddLine(9,18+bob,18,18+bob);body.CloseFigure();g.FillPath(fill,body);g.DrawPath(edge,body);}
    g.FillEllipse(ink,27,14+bob,4,5);g.DrawArc(line,28,48+bob,16,9,200,290);using(var belly=new Pen(Color.FromArgb(225,251,235),2))g.DrawArc(belly,20,30+bob,23,18,160,120);g.FillEllipse(Brushes.White,50,8-wave*2,5,5);g.FillEllipse(Brushes.White,57,1-wave,3,3);
   }
   if(theme==43){ // Snail: spiral shell, waving eyestalks and a ground trail.
    using(var trail=new Pen(Color.FromArgb(135,200,226,205),2))g.DrawLine(trail,3,58,55,58);
    using(var body=new SolidBrush(Color.FromArgb(198,218,167))){g.FillEllipse(body,8,44,48,13);g.FillEllipse(body,42,34,15,20);using(var stem=new Pen(Color.FromArgb(163,190,132),3)){g.DrawLine(stem,46,40,43+wave*2,25);g.DrawLine(stem,53,40,57-wave*2,25);}g.FillEllipse(body,39+wave*2,21,8,9);g.FillEllipse(body,53-wave*2,21,8,9);g.FillEllipse(ink,42+wave*2,23,3,4);g.FillEllipse(ink,56-wave*2,23,3,4);g.DrawArc(line,47,43,8,6,0,150);}
    g.FillEllipse(fill,7,10,37,39);g.DrawEllipse(edge,7,10,37,39);using(var spiral=new Pen(Color.FromArgb(159,113,172),2.8f)){spiral.StartCap=spiral.EndCap=LineCap.Round;var points=new PointF[65];for(int i=0;i<points.Length;i++){float a=i*.19f,r=15*(1-i/75f);points[i]=new PointF(26+r*(float)Math.Cos(a),29+r*(float)Math.Sin(a));}g.DrawLines(spiral,points);}g.FillEllipse(Brushes.White,15,17,7,4);
   }
   if(theme==44){ // Taiyaki pastry: waffle lattice, fins and a tiny smiling eye.
    Shape(g,fill,edge,new[]{new PointF(41,31),new PointF(60,17),new PointF(57,34),new PointF(61,49),new PointF(41,41)});g.FillEllipse(fill,5,14,45,41);g.DrawEllipse(edge,5,14,45,41);
    using(var waffle=new Pen(Color.FromArgb(189,132,83),1.5f)){for(int i=0;i<3;i++){g.DrawLine(waffle,23+i*7,22,17+i*7,45);g.DrawLine(waffle,19,27+i*7,41,31+i*7);}g.DrawLine(waffle,49,33,56,25);g.DrawLine(waffle,49,37,57,43);}g.FillEllipse(ink,13,27,4,5);g.DrawArc(line,7,34,11,8,5,160);using(var fin=new SolidBrush(Color.FromArgb(231,174,111)))Shape(g,fin,edge,new[]{new PointF(23,15),new PointF(29,5),new PointF(37,17)});
   }
   if(theme==45){ // Macaron: cream filling, blueberry crown and little cookie feet.
    using(var baseColor=new SolidBrush(Color.FromArgb(170,158,207))){g.FillEllipse(baseColor,7,31,51,25);g.DrawEllipse(edge,7,31,51,25);}g.FillEllipse(Brushes.FloralWhite,6,28,53,15);g.DrawEllipse(edge,6,28,53,15);g.FillEllipse(fill,6,13,53,26);g.DrawEllipse(edge,6,13,53,26);Face(g,ink,23,23,16);g.FillEllipse(Brushes.White,16,20,6,3);
    using(var berry=new SolidBrush(Color.FromArgb(112,123,187))){g.FillEllipse(berry,28,5,12,12);g.FillEllipse(berry,39,7,10,10);}using(var leaf=new SolidBrush(Color.FromArgb(153,198,162)))Shape(g,leaf,edge,new[]{new PointF(35,7),new PointF(39,1),new PointF(46,3),new PointF(40,8)});g.FillEllipse(Brushes.White,31,7,3,2);g.FillEllipse(fill,15,49,9,9);g.FillEllipse(fill,42,49,9,9);
   }
   if(theme==46){ // Mountain camp: snowy peak, striped tent and evergreen trees.
    Shape(g,fill,edge,new[]{new PointF(8,47),new PointF(27,5),new PointF(49,47)});Shape(g,Brushes.Snow,edge,new[]{new PointF(18,24),new PointF(27,5),new PointF(37,25),new PointF(29,21),new PointF(25,27)});
    using(var tent=new SolidBrush(Color.FromArgb(235,172,144)))Shape(g,tent,edge,new[]{new PointF(18,57),new PointF(36,28),new PointF(60,57)});using(var door=new SolidBrush(Color.FromArgb(91,111,122)))Shape(g,door,edge,new[]{new PointF(27,57),new PointF(36,35),new PointF(44,57)});g.DrawLine(line,36,28,36,35);using(var green=new SolidBrush(Color.FromArgb(119,172,154))){Shape(g,green,edge,new[]{new PointF(1,44),new PointF(10,27),new PointF(19,44)});Shape(g,green,edge,new[]{new PointF(2,51),new PointF(10,37),new PointF(18,51)});}g.DrawLine(line,10,48,10,57);g.FillEllipse(Brushes.LemonChiffon,49,7,10,10);
   }
   if(theme==47){ // Hot-air balloon sways as one object; stripes stay inside the envelope.
    var state=g.Save();g.TranslateTransform(32,33);g.RotateTransform(wave*5);g.TranslateTransform(-32,-33);using(var envelope=new GraphicsPath()){envelope.AddBezier(12,26,4,-4,59,-4,53,26);envelope.AddBezier(53,26,49,35,42,43,38,44);envelope.AddLine(38,44,26,44);envelope.AddBezier(26,44,21,41,15,33,12,26);envelope.CloseFigure();g.FillPath(fill,envelope);g.DrawPath(edge,envelope);var clip=g.Save();g.SetClip(envelope);using(var stripe=new SolidBrush(Color.FromArgb(255,224,186)))g.FillEllipse(stripe,24,0,17,46);g.Restore(clip);}g.DrawLine(line,26,43,27,51);g.DrawLine(line,38,43,37,51);using(var basket=new SolidBrush(Color.FromArgb(193,146,107)))Shape(g,basket,edge,new[]{new PointF(23,50),new PointF(41,50),new PointF(38,61),new PointF(26,61)});g.DrawLine(line,26,55,38,55);g.Restore(state);
   }
   if(theme==48){ // Retro camera: prominent lens, viewfinder and shoulder strap.
    using(var strap=new Pen(Color.FromArgb(202,162,135),3)){g.DrawArc(strap,1,17,62,36,155,230);}Shape(g,fill,edge,new[]{new PointF(7,22),new PointF(19,22),new PointF(23,14),new PointF(39,14),new PointF(43,22),new PointF(57,22),new PointF(57,54),new PointF(7,54)});g.FillRectangle(Brushes.FloralWhite,9,25,46,10);g.FillEllipse(ink,20,25,26,26);g.DrawEllipse(edge,20,25,26,26);using(var lens=new SolidBrush(Color.FromArgb(136,186,203)))g.FillEllipse(lens,25,30,16,16);g.FillEllipse(Brushes.White,28,32,5,5);g.FillRectangle(ink,45,25,7,5);g.FillEllipse(Brushes.Salmon,12,28,4,4);g.DrawLine(line,13,20,18,20);
   }
   if(theme==49){ // Music cassette: visible rotating spokes and a drifting note.
    g.FillRectangle(fill,5,14,54,43);g.DrawRectangle(edge,5,14,54,43);g.FillRectangle(Brushes.FloralWhite,10,20,44,23);using(var label=new SolidBrush(Color.FromArgb(118,123,168)))g.FillRectangle(label,12,22,40,7);
    foreach(int x in new[]{22,43}){g.FillEllipse(ink,x-7,29,14,14);g.DrawEllipse(edge,x-7,29,14,14);using(var spoke=new Pen(Color.FromArgb(226,223,242),1.5f)){for(int i=0;i<3;i++){float a=phase+i*2.0944f;g.DrawLine(spoke,x,36,x+5*(float)Math.Cos(a),36+5*(float)Math.Sin(a));}}}Shape(g,Brushes.Lavender,edge,new[]{new PointF(19,48),new PointF(46,48),new PointF(50,57),new PointF(15,57)});g.FillEllipse(ink,9,49,3,3);g.FillEllipse(ink,52,49,3,3);using(var note=new Pen(c,2.4f)){g.DrawLine(note,51,8+wave*2,51,1+wave*2);g.DrawLine(note,51,1+wave*2,58,3+wave*2);}g.FillEllipse(fill,46,6+wave*2,7,5);
   }
   if(theme==50){ // Pinwheel: four folded colored blades rotate around a fixed axle.
    using(var stick=new Pen(Color.FromArgb(188,151,113),4))g.DrawLine(stick,32,28,32,61);var state=g.Save();g.TranslateTransform(32,28);g.RotateTransform(frame*360f/Math.Max(1,count));Color[] petals={c,Color.FromArgb(246,195,152),Color.FromArgb(164,207,190),Color.FromArgb(186,174,226)};
    for(int i=0;i<4;i++){using(var blade=new SolidBrush(petals[i]))Shape(g,blade,edge,new[]{new PointF(0,0),new PointF(-6,-24),new PointF(16,-24),new PointF(24,-8)});using(var fold=new Pen(Color.FromArgb(140,255,255,255),1.5f))g.DrawLine(fold,0,0,16,-24);g.RotateTransform(90);}g.Restore(state);g.FillEllipse(Brushes.LemonChiffon,27,23,10,10);g.DrawEllipse(edge,27,23,10,10);
   }
   if(theme==51){ // Dragon slime: squash-and-stretch body with tiny horns and wings.
    var state=g.Save();g.TranslateTransform(32,53);g.ScaleTransform(1+wave*.06f,1-wave*.07f);g.TranslateTransform(-32,-53);Shape(g,Brushes.LavenderBlush,edge,new[]{new PointF(16,38),new PointF(2,24),new PointF(5,44),new PointF(17,48)});Shape(g,Brushes.LavenderBlush,edge,new[]{new PointF(48,38),new PointF(62,24),new PointF(59,44),new PointF(47,48)});
    using(var body=new GraphicsPath()){body.AddBezier(7,49,8,21,22,26,32,10);body.AddBezier(32,10,44,26,57,22,58,49);body.AddBezier(58,49,54,62,10,62,7,49);body.CloseFigure();g.FillPath(fill,body);g.DrawPath(edge,body);}Shape(g,Brushes.LemonChiffon,edge,new[]{new PointF(17,29),new PointF(17,13),new PointF(27,24)});Shape(g,Brushes.LemonChiffon,edge,new[]{new PointF(38,24),new PointF(48,13),new PointF(48,30)});Face(g,ink,23,37,16);g.FillEllipse(Brushes.White,15,32,8,5);g.FillEllipse(Brushes.LightPink,14,46,7,4);g.FillEllipse(Brushes.LightPink,44,46,7,4);g.Restore(state);
   }
   if(theme==52){ // Frog on a notched lily pad; breathing and ripples animate.
    using(var pad=new SolidBrush(Color.FromArgb(109,179,147))){using(var leaf=new GraphicsPath()){leaf.AddArc(4,39,56,22,8,326);leaf.AddLine(59,47,34,49);leaf.CloseFigure();g.FillPath(pad,leaf);g.DrawPath(edge,leaf);}}using(var ripple=new Pen(Color.FromArgb(125,158,206,220),1.5f))g.DrawArc(ripple,1-wave*2,42-wave,62+wave*4,19+wave*2,5,160);
    g.FillEllipse(fill,16,25+wave,34,29);g.DrawEllipse(edge,16,25+wave,34,29);g.FillEllipse(fill,13,14+wave,18,21);g.FillEllipse(fill,35,14+wave,18,21);g.DrawEllipse(edge,13,14+wave,18,21);g.DrawEllipse(edge,35,14+wave,18,21);g.FillEllipse(Brushes.FloralWhite,17,18+wave,10,13);g.FillEllipse(Brushes.FloralWhite,39,18+wave,10,13);g.FillEllipse(ink,21,21+wave,4,7);g.FillEllipse(ink,42,21+wave,4,7);g.DrawArc(line,25,36+wave,15,8,0,180);g.FillEllipse(Brushes.LightPink,17,35+wave,7,4);g.FillEllipse(Brushes.LightPink,43,35+wave,7,4);g.FillEllipse(fill,10,47,17,8);g.FillEllipse(fill,39,47,17,8);
   }
   if(theme==53){ // Star mail: folded envelope and a comet orbiting above it.
    using(var wing=new SolidBrush(Color.FromArgb(218,231,246))){Shape(g,wing,edge,new[]{new PointF(15,34),new PointF(2,23),new PointF(6,43),new PointF(15,47)});Shape(g,wing,edge,new[]{new PointF(49,34),new PointF(62,23),new PointF(58,43),new PointF(49,47)});}g.FillRectangle(fill,12,27,40,30);g.DrawRectangle(edge,12,27,40,30);using(var paper=new SolidBrush(Color.FromArgb(255,245,239)))Shape(g,paper,edge,new[]{new PointF(13,28),new PointF(32,44),new PointF(51,28)});g.DrawLine(edge,13,56,27,42);g.DrawLine(edge,51,56,37,42);Shape(g,Brushes.LemonChiffon,edge,Star(32,43,7,3,5,-1.57f));float sx=14+frame*2.25f,sy=13-wave*4;using(var trail=new Pen(Color.FromArgb(170,239,206,145),2.4f)){g.DrawLine(trail,sx-8,sy+4,sx-1,sy);}Shape(g,Brushes.LemonChiffon,edge,Star(sx,sy,7,3,5,-1.57f));Spark(g,Color.White,51,7,3+wave*.6f);
   }
  }
 }
}
}
