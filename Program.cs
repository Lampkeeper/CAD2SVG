using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;

namespace DXF2SVG
{
    class Program
    {
        private static double scale = 50.0f;
        private static double lineWeight = 0.5f;

        private static double Paper_Size_X = 0f;
        private static double Paper_Size_Y = 0f;

        private static double fontSize = 5f;


        static void Main(string[] args)
        {
            string filename = "C:\\Users\\dev\\Desktop\\test2.dxf";
            OutputAll(filename);
        }

        private static void OutputAll(string filename)
        {
            DxfDocument dxf = DxfDocument.Load(filename);
            InitSize(dxf);
            Console.WriteLine(string.Format("<svg fill-rule=\"evenodd\" stroke-linejoin=\"round\"  style=\"background: black\"  version =\"1.1\" viewBox =\"0 0 1024 768\" xml:space=\"preserve\" xmlns =\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" > "));
            //outputUse(dxf);
            outputLWPolyLine(dxf);
            outputLine(dxf);
            outputCircle(dxf);
            outputText(dxf);
            outputArc(dxf);
            //outputInsert(dxf);
            Console.WriteLine("</svg>");
            Console.ReadLine();
        }

        /*获取图纸尺寸*/
        private static void InitSize(DxfDocument dxf)
        {
            foreach (Block block in dxf.Blocks)
            {
                if (block.Name.Contains("*Model_Space"))
                {
                    foreach (var lt in block.Entities)
                    {
                        if (lt.CodeName == "LINE")
                        {
                            var line = (Line)lt;
                            if (line.EndPoint.X > Paper_Size_X)
                            {
                                Paper_Size_X = line.EndPoint.X;
                            }
                            if (line.EndPoint.Y > Paper_Size_Y)
                            {
                                Paper_Size_Y = line.EndPoint.Y;
                            }
                        }
                        if (lt.CodeName == "LWPOLYLINE")
                        {
                            var line = (LwPolyline)lt;
                            foreach (var li in line.Vertexes)
                            {
                                if (li.Position.X > Paper_Size_X)
                                {
                                    Paper_Size_X = li.Position.X;
                                }
                                if (li.Position.Y > Paper_Size_Y)
                                {
                                    Paper_Size_Y = li.Position.Y;
                                }
                            }
                        }
                    }
                }
            }

        }
        /*输出Block引用*/
        private static void outputUse(DxfDocument dxf)
        {
            Console.WriteLine("<defs>");
            foreach (Block block in dxf.Blocks)
            {
                if (block.Name[0] != '*')
                {
                    Console.WriteLine("<g id=\"" + block.Name.Replace("$", "") + "\">");
                    foreach (var et in block.Entities)
                    {
                        if (et.CodeName == "LINE")
                        {
                            var res = (Line)et;
                            Console.Write("<path d=\"M ");
                            Console.Write(string.Format("{0} {1} {2} {3}\" ",
                                (res.StartPoint.X / scale),
                                (res.StartPoint.Y / scale),
                                (res.EndPoint.X / scale),
                                (res.EndPoint.Y / scale)));
                            Console.Write(string.Format("style=\"fill:rgb({0},{1},{2});stroke:rgb({0},{1},{2});stroke-width:{3}\"/>\n"
                                , res.Color.R, res.Color.G, res.Color.B, lineWeight));
                        }
                        else if (et.CodeName == "LWPOLYLINE")
                        {
                            var res = (LwPolyline)et;
                            if (res.IsClosed)
                            {
                                Console.Write("<polygon  points=\"");
                            }
                            else
                            {
                                Console.Write("<polyline  points=\"");
                            }
                            for (int j = 0; j < res.Vertexes.Count; j++)
                            {
                                if (j != 0)
                                {
                                    Console.Write(" ");
                                }
                                Console.Write(string.Format("{0},{1}",
                                   (res.Vertexes[j].Position.X / scale),
                                   (res.Vertexes[j].Position.Y / scale)));
                            }
                            Console.WriteLine(string.Format("\" fill=\"none\" stroke=\"rgb({0},{1},{2})\" stroke-width=\"{3}\" />",
                                res.Color.R, res.Color.G, res.Color.B, lineWeight));
                        }
                        else if (et.CodeName == "CIRCLE")
                        {
                            var res = (Circle)et;
                            Console.Write(string.Format("<circle cx=\"{0}\" cy=\"{1}\" r=\"{2}\" fill=\"rgb({3},{4},{5})\" stroke=\"rgb({3},{4},{5})\" stroke-width=\"{6}\" />\n",
                                (res.Center.X / scale),
                                (res.Center.Y / scale),
                                res.Radius / scale,
                                res.Color.R,
                                res.Color.G,
                                res.Color.B,
                                lineWeight));
                        }
                        else if (et.CodeName == "TEXT")
                        {
                            var res = (Text)et;
                            Console.Write(string.Format("<text x=\"{0}\" y=\"{1}\" font-size=\"{2}\" font-family=\"{3}\" fill=\"rgb({4},{5},{6})\">{7}</text>\n",
                                (res.Position.X / scale),
                                (res.Position.Y / scale),
                                res.Style.WidthFactor * fontSize,
                                res.Style.FontFamilyName,
                                res.Color.R,
                                res.Color.G,
                                res.Color.B,
                                res.Value));
                        }
                        else if (et.CodeName == "INSERT")
                        {
                            var lt = (Insert)et;
                            Console.WriteLine(string.Format("<use xlink:href=\"#{2}\" transform=\"translate({0} {1}) rotate({3}) scale({4} {5})\" />",
                                /*transform_X*/(lt.Position.X) / scale,
                                /*transform_Y*/(lt.Position.Y) / scale,
                                lt.Block.Name.Replace("$", ""),
                                180.0f - lt.Rotation,
                                lt.Scale.X,
                                lt.Scale.Y));
                        }
                        else if (et.CodeName == "HATCH")
                        {
                            /*var res = (Hatch)et;

                            Console.WriteLine(res.CodeName);*/
                        }
                        else if (et.CodeName == "SOLID")
                        {
                            /*var res = (Solid)et;
                            Console.Write("<polygon id=\"test\" points=\"");
                            Console.Write(string.Format("{0},{1} ",
                                   (res.FirstVertex.X / scale),
                                   (res.FirstVertex.Y / scale)));
                            Console.Write(string.Format("{0},{1} ",
                                   (res.SecondVertex.X / scale),
                                   (res.SecondVertex.Y / scale)));
                            Console.Write(string.Format("{0},{1} ",
                                   (res.ThirdVertex.X / scale),
                                   (res.ThirdVertex.Y / scale)));
                            Console.Write(string.Format("{0},{1}",
                                   (res.FourthVertex.X / scale),
                                   (res.FourthVertex.Y / scale)));
                            Console.WriteLine(string.Format("\" fill=\"red\" stroke=\"rgb({0},{1},{2})\" stroke-width=\"{3}\" />",
                                res.Color.R, res.Color.G, res.Color.B, lineWeight));*/
                        }
                        else
                        {
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine("</g>");
                }
            }
            Console.WriteLine("</defs>");
        }
        /*转换线条*/
        private static void outputLine(DxfDocument dxf)
        {
            for (int i = 0; i < dxf.Lines.Count(); i++)
            {
                var res = dxf.Lines.ElementAt(i);
                Console.Write("<path d=\"M ");
                Console.Write(string.Format("{0} {1} {2} {3}\" ",
                    transform_X(res.StartPoint.X),
                    transform_Y(res.StartPoint.Y),
                    transform_X(res.EndPoint.X),
                    transform_Y(res.EndPoint.Y)));
                Console.Write(string.Format("style=\"fill:rgb({0},{1},{2});stroke:rgb({0},{1},{2});stroke-width:{3}\"/>\n"
                    , res.Color.R, res.Color.G, res.Color.B, lineWeight));
            }


        }
        /*转换不规则图形*/
        private static void outputLWPolyLine(DxfDocument dxf)
        {
            for (int i = 0; i < dxf.LwPolylines.Count(); i++)
            {
                var res = dxf.LwPolylines.ElementAt(i);

                if (res.IsClosed)
                {
                    Console.Write("<polygon  points=\"");
                }
                else
                {
                    Console.Write("<polyline  points=\"");
                }

                for (int j = 0; j < res.Vertexes.Count; j++)
                {
                    if (j != 0)
                    {
                        Console.Write(" ");
                    }
                    Console.Write(string.Format("{0},{1}",
                       transform_X(res.Vertexes[j].Position.X),
                       transform_Y(res.Vertexes[j].Position.Y)));
                }
                Console.WriteLine(string.Format("\" fill=\"none\" stroke=\"rgb({0},{1},{2})\" stroke-width=\"{3}\" />",
                    res.Color.R, res.Color.G, res.Color.B, lineWeight));
            }
        }
        /*转换圆形*/
        private static void outputCircle(DxfDocument dxf)
        {
            for (int i = 0; i < dxf.Circles.Count(); i++)
            {
                var res = dxf.Circles.ElementAt(i);

                Console.Write(string.Format("<circle cx=\"{0}\" cy=\"{1}\" r=\"{2}\" fill=\"none\" stroke=\"rgb({3},{4},{5})\" stroke-width=\"{6}\" />\n",
                    transform_X(res.Center.X),
                    transform_Y(res.Center.Y),
                    res.Radius / scale,
                    res.Color.R,
                    res.Color.G,
                    res.Color.B,
                    lineWeight));
            }
        }
        /*转换文字*/
        private static void outputText(DxfDocument dxf)
        {
            for (int i = 0; i < dxf.Texts.Count(); i++)
            {
                var res = dxf.Texts.ElementAt(i);
                // Console.Write(string.Format("<text x=\"{0}\" y=\"{1}\" font-size=\"{2}\" font-family=\"{3}\" fill=\"rgb({4},{5},{6})\" transform=\"{8}\">{7}</text>\n",
                Console.Write(string.Format("<text font-size=\"{2}\" font-family=\"{3}\" fill=\"rgb({4},{5},{6})\" transform=\"translate({0},{1}) rotate({8})\">{7}</text>\n",
                   transform_X(res.Position.X),
                   transform_Y(res.Position.Y),
                   res.Style.WidthFactor * fontSize,
                   res.Style.FontFamilyName,
                   res.Color.R,
                   res.Color.G,
                   res.Color.B,
                   res.Value,
                   res.Rotation != 0 ? -res.Rotation : res.Rotation));
            }
        }
        /*转换Insert*/
        private static void outputArc(DxfDocument dxf)
        {
            for (int i = 0; i < dxf.Arcs.Count(); i++)
            {
                var res = dxf.Arcs.ElementAt(i);

                var startAngle = res.StartAngle * Math.PI / 180.0f;
                var endAngle = res.EndAngle * Math.PI / 180.0f;

                Console.Write("<path d=\"");
                Console.Write(string.Format("M{0},{1} A{2},{3} {4} {5} {6} {7},{8}\" ",
                    transform_X(res.Center.X) + (res.Radius / scale) * Math.Cos(startAngle),
                    transform_Y(res.Center.Y) + (res.Radius / scale) * Math.Sin(startAngle),
                    res.Radius / scale,
                    res.Radius / scale,
                    res.StartAngle,
                    res.StartAngle > 180 ? 1 : 0,
                    0,
                    transform_X(res.Center.X) + res.Radius / scale * Math.Cos(endAngle),
                    transform_Y(res.Center.Y) + res.Radius / scale * Math.Sin(endAngle)
                    ));
                Console.Write(string.Format("style=\"fill:none;stroke:rgb({0},{1},{2});stroke-width:{3}\"/>\n"
                    , res.Color.R, res.Color.G, res.Color.B, lineWeight));

            }
        }
        /*转换圆弧*/
        private static void outputInsert(DxfDocument dxf)
        {
            foreach (var lt in dxf.Inserts)
            {
                Console.WriteLine(string.Format("<use xlink:href=\"#{2}\" transform=\"translate({0} {1}) rotate({3}) scale({4} {5})\" />",
                transform_X(lt.Position.X),
                transform_Y(lt.Position.Y),
                lt.Block.Name.Replace("$", ""),
                180.0f - lt.Rotation,
                lt.Scale.X,
                lt.Scale.Y));
            }
        }
        /*X轴坐标转换*/
        private static double transform_X(double x)
        {
            return x / scale;
        }
        /*Y轴桌面转换*/
        private static double transform_Y(double y)
        {
            return (Paper_Size_Y - y) / scale;
            //return y/ scale;
        }
    }
}
