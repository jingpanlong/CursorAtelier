using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CursorAtelier {
public static partial class Art {
 static PointF[] Star(float x,float y,float outer,float inner,int points,float rotation){var result=new PointF[points*2];for(int i=0;i<result.Length;i++){double angle=rotation+i*Math.PI/points;float r=i%2==0?outer:inner;result[i]=new PointF(x+(float)Math.Cos(angle)*r,y+(float)Math.Sin(angle)*r);}return result;}
 static void Face(Graphics g,Brush ink,float x,float y,float space){g.FillEllipse(ink,x,y,3,5);g.FillEllipse(ink,x+space,y,3,5);using(var smile=new Pen(Color.FromArgb(64,52,75),1.5f))g.DrawArc(smile,x+space/2-2,y+5,7,5,0,170);}
 static void Spark(Graphics g,Color c,float x,float y,float radius){using(var b=new SolidBrush(c))g.FillPolygon(b,Star(x,y,radius,radius*.3f,4,0));}
 // Fifteen silhouettes drawn in the same 64px coordinate space as the original art.
 static void DrawExtra(Graphics g,int theme,int frame,int count){
  float phase=frame*6.283185f/Math.Max(1,count),wave=(float)Math.Sin(phase);Color c=ColorTranslator.FromHtml(Colors[theme]);
  using(var fill=new SolidBrush(c))using(var ink=new SolidBrush(Color.FromArgb(57,48,73)))using(var line=new Pen(Color.FromArgb(57,48,73),1.8f))using(var edge=new Pen(Color.FromArgb(255,251,247),2.5f))using(var shade=new SolidBrush(ControlColor(c,.72f))){line.StartCap=line.EndCap=LineCap.Round;edge.LineJoin=LineJoin.Round;
   if(theme==24){ // Moon rabbit: two unmistakable ears, crescent and rosy cheeks.
    g.FillEllipse(fill,18,3,11,30);g.FillEllipse(fill,36,1,11,31);g.DrawEllipse(edge,18,3,11,30);g.DrawEllipse(edge,36,1,11,31);g.FillEllipse(Brushes.MistyRose,22,7,4,19);g.FillEllipse(Brushes.MistyRose,40,5,4,19);
    g.FillEllipse(fill,10,24,45,33);g.DrawEllipse(edge,10,24,45,33);Face(g,ink,23,36,16);g.FillEllipse(Brushes.LightPink,15,44,8,4);g.FillEllipse(Brushes.LightPink,43,44,8,4);
    using(var moon=new GraphicsPath()){moon.AddArc(43,3,15,15,65,290);moon.AddArc(48,1,13,13,235,-250);moon.CloseFigure();g.FillPath(Brushes.LemonChiffon,moon);}Spark(g,Color.White,9,25,3);
   }
   if(theme==25){ // Panda holding a bamboo stalk.
    g.FillEllipse(ink,10,8,16,16);g.FillEllipse(ink,38,8,16,16);g.FillEllipse(Brushes.FloralWhite,10,15,44,36);g.DrawEllipse(edge,10,15,44,36);g.FillEllipse(ink,17,26,11,15);g.FillEllipse(ink,36,26,11,15);g.FillEllipse(Brushes.White,22,30,3,5);g.FillEllipse(Brushes.White,39,30,3,5);g.FillEllipse(ink,29,39,7,5);g.DrawArc(line,27,44,12,5,0,160);
    using(var bamboo=new Pen(c,5)){g.DrawLine(bamboo,47,34,45,61);}g.DrawLine(line,43,45,48,46);g.DrawLine(line,43,54,48,55);Shape(g,fill,edge,new[]{new PointF(47,39),new PointF(58,30),new PointF(57,39)});g.FillEllipse(ink,15,47,15,11);
   }
   if(theme==26){ // Round hamster, tiny ears and a sunflower seed.
    g.FillEllipse(fill,12,8,15,17);g.FillEllipse(fill,38,8,15,17);g.DrawEllipse(edge,12,8,15,17);g.DrawEllipse(edge,38,8,15,17);g.FillEllipse(Brushes.Pink,16,12,7,9);g.FillEllipse(Brushes.Pink,42,12,7,9);g.FillEllipse(fill,9,19,47,39);g.DrawEllipse(edge,9,19,47,39);g.FillEllipse(Brushes.Cornsilk,16,36,34,20);Face(g,ink,22,30,17);g.FillEllipse(Brushes.LightSalmon,14,38,10,6);g.FillEllipse(Brushes.LightSalmon,42,38,10,6);
    using(var seed=new SolidBrush(Color.FromArgb(105,76,64))){g.FillEllipse(seed,27,46,12,14);g.DrawLine(edge,33,48,33,57);}g.FillEllipse(fill,20,47,9,7);g.FillEllipse(fill,39,47,9,7);
   }
   if(theme==27){ // Octopus: eight curved, independently waving arms.
    for(int i=0;i<8;i++){float x=9+i*6.5f,tip=48+4*(float)Math.Sin(phase+i*.9);using(var tentacle=new Pen(c,6)){tentacle.StartCap=tentacle.EndCap=LineCap.Round;g.DrawBezier(tentacle,x,36,x-4,52,x+5,57,x+2,tip);}g.FillEllipse(Brushes.MistyRose,x,48,2.5f,2.5f);}g.FillEllipse(fill,11,9,43,34);g.DrawArc(edge,11,9,43,34,180,180);Face(g,ink,24,25,14);g.FillEllipse(Brushes.LightPink,16,32,7,4);g.FillEllipse(Brushes.LightPink,44,32,7,4);g.FillEllipse(Brushes.White,19,16,9,5);
   }
   if(theme==28){ // Jellyfish: translucent bell, ribbons and little bubbles.
    for(int i=0;i<5;i++)using(var ribbon=new Pen(i%2==0?c:Color.FromArgb(185,198,247),3)){ribbon.StartCap=ribbon.EndCap=LineCap.Round;float x=18+i*7;g.DrawBezier(ribbon,x,33,x-8*wave,42,x+7*wave,51,x+3*(float)Math.Sin(phase+i),59);}
    using(var bell=new GraphicsPath()){bell.AddArc(7,7+wave*2,51,48,180,180);bell.AddBezier(58,31+wave*2,44,40+wave*2,20,40+wave*2,7,31+wave*2);bell.CloseFigure();g.FillPath(fill,bell);g.DrawPath(edge,bell);}Face(g,ink,24,23+wave*2,15);g.FillEllipse(Brushes.White,15,14+wave*2,12,5);using(var bubble=new Pen(Color.FromArgb(170,198,226,255),1.6f)){g.DrawEllipse(bubble,55,4+wave*2,5,5);g.DrawEllipse(bubble,3,45-wave*2,4,4);}
   }
   if(theme==29){ // Butterfly: wings contract while the body stays centered.
    float spread=.82f+.15f*(1+wave)/2;var state=g.Save();g.TranslateTransform(32,30);g.ScaleTransform(spread,1);g.TranslateTransform(-32,-30);
    g.FillEllipse(fill,3,8,28,30);g.FillEllipse(fill,34,8,28,30);g.FillEllipse(fill,9,32,22,23);g.FillEllipse(fill,34,32,22,23);g.DrawEllipse(edge,3,8,28,30);g.DrawEllipse(edge,34,8,28,30);g.DrawEllipse(edge,9,32,22,23);g.DrawEllipse(edge,34,32,22,23);g.FillEllipse(Brushes.LemonChiffon,10,17,13,13);g.FillEllipse(Brushes.LemonChiffon,42,17,13,13);g.FillEllipse(Brushes.LavenderBlush,17,38,8,9);g.FillEllipse(Brushes.LavenderBlush,40,38,8,9);g.Restore(state);g.FillEllipse(ink,29,21,7,29);g.DrawBezier(line,31,22,23,16,24,10,26,11);g.DrawBezier(line,34,22,42,16,41,10,39,11);
   }
   if(theme==30){ // Little dinosaur with back plates and a proud tail.
    Shape(g,Brushes.LightPink,edge,new[]{new PointF(40,15),new PointF(46,10),new PointF(48,21),new PointF(54,20),new PointF(52,31),new PointF(60,33),new PointF(50,44)});
    using(var body=new GraphicsPath()){body.AddBezier(19,29,17,5,51,2,50,26);body.AddBezier(50,26,49,42,55,45,61,45);body.AddBezier(61,45,52,61,37,56,24,56);body.AddLine(24,56,16,56);body.AddLine(16,56,17,44);body.AddBezier(17,44,4,44,2,30,9,27);body.AddLine(9,27,19,29);body.CloseFigure();g.FillPath(fill,body);g.DrawPath(edge,body);}g.FillEllipse(Brushes.PaleGoldenrod,25,34,17,17);g.FillEllipse(ink,28,17,4,5);g.DrawArc(line,17,24,15,7,0,160);g.DrawLine(line,32,52,32,57);g.DrawLine(line,22,34,16,39);
   }
   if(theme==31){ // Cheese pizza: crust, pepperoni and melting cheese.
    using(var slice=new GraphicsPath()){slice.AddArc(7,5,50,16,180,180);slice.AddLine(57,13,32,60);slice.AddLine(32,60,7,13);slice.CloseFigure();g.FillPath(fill,slice);g.DrawPath(edge,slice);}using(var crust=new Pen(Color.FromArgb(211,144,85),9)){crust.StartCap=crust.EndCap=LineCap.Round;g.DrawArc(crust,7,5,50,16,180,180);}using(var tomato=new SolidBrush(Color.FromArgb(220,98,107))){g.FillEllipse(tomato,19,20,11,11);g.FillEllipse(tomato,36,25,10,10);g.FillEllipse(tomato,27,38,9,9);}using(var herb=new Pen(Color.FromArgb(108,160,111),2.2f)){g.DrawLine(herb,35,20,38,16);g.DrawLine(herb,20,34,24,37);}g.FillEllipse(Brushes.White,29,27,4,4);
   }
   if(theme==32){ // Peach soda in a glass, rising bubbles and diagonal straw.
    using(var straw=new Pen(Color.FromArgb(237,160,171),4)){straw.StartCap=straw.EndCap=LineCap.Round;g.DrawLines(straw,new[]{new Point(35,40),new Point(43,5),new Point(55,5)});}Shape(g,fill,edge,new[]{new PointF(13,20),new PointF(51,20),new PointF(46,59),new PointF(19,59)});using(var juice=new SolidBrush(Color.FromArgb(255,202,175)))Shape(g,juice,edge,new[]{new PointF(17,31),new PointF(48,31),new PointF(44,56),new PointF(22,56)});g.FillEllipse(Brushes.LavenderBlush,14,17,37,9);for(int i=0;i<5;i++){float y=53-((frame*2+i*8)%29);g.FillEllipse(Brushes.White,24+i%3*6,y,3+i%2,3+i%2);}Face(g,ink,24,39,13);g.DrawLine(edge,18,26,22,49);Spark(g,Color.White,7,15,4);
   }
   if(theme==33){ // Star wand, glowing halo and orbiting sparks.
    using(var wand=new Pen(c,8)){wand.StartCap=wand.EndCap=LineCap.Round;g.DrawLine(wand,17,57,41,24);}g.DrawLine(edge,16,53,30,33);using(var halo=new SolidBrush(Color.FromArgb((int)(45+25*(1+wave)),255,226,150)))g.FillEllipse(halo,22,2,39,39);Shape(g,Brushes.LemonChiffon,edge,Star(42,20,19,8,5,-1.57f));g.FillEllipse(ink,37,17,2,4);g.FillEllipse(ink,46,17,2,4);g.DrawArc(line,39,22,7,4,0,180);Spark(g,c,13+wave*2,17,4+wave);Spark(g,Color.FromArgb(255,210,153),55,48,4-wave);Spark(g,Color.White,6,39,3);
   }
   if(theme==34){ // Rocket with a flame whose tip changes every frame.
    float bob=wave*1.4f;Shape(g,Brushes.Coral,edge,new[]{new PointF(24,45+bob),new PointF(22,55),new PointF(32,63-3*(1+wave)),new PointF(43,55),new PointF(40,44+bob)});Shape(g,Brushes.LemonChiffon,edge,new[]{new PointF(28,46),new PointF(32,58-3*wave),new PointF(37,46)});Shape(g,fill,edge,new[]{new PointF(23,30+bob),new PointF(11,43+bob),new PointF(12,54+bob),new PointF(26,46+bob),new PointF(39,46+bob),new PointF(53,54+bob),new PointF(52,43+bob),new PointF(42,30+bob)});
    using(var rocket=new GraphicsPath()){rocket.AddBezier(21,46+bob,16,25+bob,24,8+bob,32,3+bob);rocket.AddBezier(32,3+bob,43,10+bob,49,27+bob,43,46+bob);rocket.CloseFigure();g.FillPath(Brushes.FloralWhite,rocket);g.DrawPath(edge,rocket);}g.FillEllipse(fill,25,21+bob,16,17);g.DrawEllipse(line,25,21+bob,16,17);g.FillEllipse(Brushes.White,28,24+bob,5,5);using(var band=new Pen(c,4))g.DrawLine(band,22,43+bob,42,43+bob);Spark(g,Color.White,6,20,3);
   }
   if(theme==35){ // Handheld console with a moving pixel sprite.
    using(var shape=new GraphicsPath()){shape.AddArc(9,6,12,12,180,90);shape.AddArc(44,6,12,12,270,90);shape.AddLine(56,12,56,47);shape.AddArc(35,38,21,21,0,90);shape.AddLine(45,59,15,59);shape.AddArc(9,47,12,12,90,90);shape.CloseFigure();g.FillPath(fill,shape);g.DrawPath(edge,shape);}using(var screen=new SolidBrush(Color.FromArgb(41,69,76)))g.FillRectangle(screen,16,14,33,23);using(var pixel=new SolidBrush(Color.FromArgb(182,237,173))){int step=(frame/4)%2;g.FillRectangle(pixel,25+step*5,20,8,8);g.FillRectangle(pixel,22+step*5,25,14,4);g.FillRectangle(pixel,23+step*5,29,4,4);g.FillRectangle(pixel,30+step*5,29,4,4);g.FillRectangle(pixel,40,18,4,4);}g.FillRectangle(ink,18,42,5,13);g.FillRectangle(ink,14,46,13,5);g.FillEllipse(Brushes.LightPink,39,44,7,7);g.FillEllipse(Brushes.LightPink,47,39,7,7);g.DrawLine(line,33,54,40,54);
   }
   if(theme==36){ // Diamond sword: pixel outline and stepped blade.
    var state=g.Save();g.TranslateTransform(32,33);g.RotateTransform(35);g.TranslateTransform(-32,-33);g.SmoothingMode=SmoothingMode.None;Shape(g,fill,edge,new[]{new PointF(32,2),new PointF(42,12),new PointF(39,37),new PointF(25,37),new PointF(22,12)});using(var gleam=new Pen(Color.FromArgb(222,255,255),3))g.DrawLine(gleam,31,9,29,32);using(var guard=new SolidBrush(Color.FromArgb(114,119,179)))Shape(g,guard,edge,new[]{new PointF(17,34),new PointF(47,34),new PointF(47,41),new PointF(36,41),new PointF(36,57),new PointF(28,57),new PointF(28,41),new PointF(17,41)});g.DrawLine(line,29,46,35,46);g.DrawLine(line,29,51,35,51);g.Restore(state);
   }
   if(theme==37){ // Sunflower, rotating petal tips and smiling seed center.
    using(var stem=new Pen(Color.FromArgb(116,177,124),5)){g.DrawLine(stem,32,40,32,61);}using(var leaves=new SolidBrush(Color.FromArgb(140,204,148))){Shape(g,leaves,edge,new[]{new PointF(31,55),new PointF(14,45),new PointF(14,55),new PointF(31,60)});Shape(g,leaves,edge,new[]{new PointF(34,51),new PointF(52,42),new PointF(50,53),new PointF(34,58)});}var state=g.Save();g.TranslateTransform(32,27);g.RotateTransform(wave*5);for(int i=0;i<10;i++){g.RotateTransform(36);g.FillEllipse(fill,-6,-26,12,21);g.DrawEllipse(edge,-6,-26,12,21);}g.Restore(state);using(var center=new SolidBrush(Color.FromArgb(163,109,74)))g.FillEllipse(center,17,12,30,30);g.DrawEllipse(edge,17,12,30,30);Face(g,Brushes.LemonChiffon,24,22,13);g.FillEllipse(Brushes.LightSalmon,20,29,6,4);g.FillEllipse(Brushes.LightSalmon,40,29,6,4);
   }
   if(theme==38){ // Donut with a transparent hole, frosting, sprinkles and cherry.
    using(var pastry=new SolidBrush(Color.FromArgb(219,165,111)))using(var ring=new GraphicsPath(FillMode.Alternate)){ring.AddEllipse(6,14,52,44);ring.AddEllipse(24,29,17,15);g.FillPath(pastry,ring);g.DrawEllipse(edge,6,14,52,44);}using(var icing=new GraphicsPath(FillMode.Alternate)){icing.AddEllipse(8,13,48,39);icing.AddEllipse(23,28,19,17);g.FillPath(fill,icing);g.DrawEllipse(edge,23,28,19,17);}Color[] sprinkles={Color.White,Color.FromArgb(255,226,141),Color.FromArgb(157,224,210),Color.FromArgb(180,176,248)};for(int i=0;i<8;i++){double angle=i*Math.PI/4;float x=32+19*(float)Math.Cos(angle),y=32+14*(float)Math.Sin(angle);using(var pen=new Pen(sprinkles[i%4],2.6f)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,x-1,y-2,x+2,y+1);}}g.FillEllipse(Brushes.Crimson,31,4+wave*2,12,12);g.FillEllipse(Brushes.White,34,6+wave*2,3,3);g.DrawBezier(line,38,6+wave*2,36,0,43,1,46,3);Spark(g,Color.White,8,9,3+wave);
   }
  }
 }
}
}
