using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Reflection;

namespace remes.XDraw
{
   public abstract class XDrawingShape : DependencyObject, IDragableObject, IDisposable
   {
      public static readonly DependencyProperty StrokeDashProperty = DependencyProperty.Register(
         "StrokeDash", typeof(string), typeof(XDrawingShape), new PropertyMetadata(String.Empty, StrokeDashChanged));

      public XDrawing Drawing { get; private set; }
      public Path Path { get; private set; }

      public XDrawingShape(XDrawing drawing, Path path)
      {
         Drawing = drawing;
         Path = path;
         
         SetValue(StrokeDashProperty, DashArrayToString(Path.StrokeDashArray));
      }

      ~XDrawingShape()
      {
         Dispose(false);
      }

      public static string DashArrayToString(DoubleCollection dashArray)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";
         var str = String.Empty;
         for (int n = 0; n < dashArray.Count; ++n)
         {
            str += dashArray[n].ToString("F1", nfi);
            if (n < dashArray.Count - 1)
            {
               str += ", ";
            }
         }
         return str;
      }


      public static void StringToDashArray(string str, DoubleCollection array)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";
         array.Clear();
         str = str.Trim();
         if (!String.IsNullOrEmpty(str))
         {
            var split = str.Split(',', ' ');
            foreach (var s in split)
            {
               double dbl;
               if (Double.TryParse(s.Trim(), NumberStyles.Any, nfi, out dbl))
               {
                  array.Add(dbl);
               }
            }
         }
      }

      private static void StrokeDashChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         StringToDashArray((string)e.NewValue, (d as XDrawingShape).Path.StrokeDashArray);
      }

      public string StrokeDash
      {
         get { return (string)GetValue(StrokeDashProperty); }
         set
         {
            SetValue(StrokeDashProperty, value);
         }
      }

      protected abstract Point GetPoint();

      protected abstract void SetPoint(Point pt);

      internal abstract void CreateControlPoints(bool editMode);

      #region IDragableObject Members

      public HitTestInfo? HitTest(Point pt)
      {
         if (Path != null)
         {
            if (Path.Stroke != null)
            {
               double thickness = Path.StrokeThickness;
               if (Path.Fill == null)
               {
                  thickness = Math.Max(5.0, thickness);
               }
               if (Path.Data.StrokeContains(new Pen(new SolidColorBrush(Colors.Black), thickness), pt))
               {
                  var hti = new HitTestInfo();
                  hti.Shape = this;
                  hti.ControlPoint = null;
                  hti.Offset = pt - GetPoint();
                  return hti;
               }
            }
            if (Path.Stroke == null || Path.Fill != null)
            {
               if (Path.Data.FillContains(pt))
               {
                  var hti = new HitTestInfo();
                  hti.Shape = this;
                  hti.ControlPoint = null;
                  hti.Offset = pt - GetPoint();
                  return hti;
               }
            }
         }
         return null;
      }

      private bool m_IsDraged = false;

      public void StartDrag()
      {
         m_IsDraged = true;
      }

      public void DragObject(Point pt)
      {
         System.Diagnostics.Debug.Assert(m_IsDraged, "DragObject is called but StartDrag was not");
         SetPoint(pt);
      }

      public void EndDrag()
      {
         m_IsDraged = false;
      }

      public bool IsDraged
      {
         get { return m_IsDraged; }
         protected set { m_IsDraged = value; }
      }

      #endregion

      internal void Export(XElement xParent, XDrawingExportFormat format)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         switch (format)
         {
            case XDrawingExportFormat.Canvas:
               var xPath = new XElement("Path",
                  new XAttribute("StrokeThickness", Path.StrokeThickness.ToString(nfi)),
                  new XAttribute("StrokeDashArray", DashArrayToString(Path.StrokeDashArray)),
                  new XAttribute("StrokeDashCap", Path.StrokeDashCap),
                  new XAttribute("StrokeDashOffset", Path.StrokeDashOffset.ToString(nfi)),
                  new XAttribute("StrokeStartLineCap", Path.StrokeStartLineCap),
                  new XAttribute("StrokeEndLineCap", Path.StrokeEndLineCap),
                  new XAttribute("StrokeLineJoin", Path.StrokeLineJoin),
                  new XAttribute("StrokeMiterLimit", Path.StrokeMiterLimit.ToString(nfi)));
               
               ExportBrush(xPath, "Stroke", Path.Stroke);
               ExportBrush(xPath, "Fill", Path.Fill);

               var xPathData = new XElement("Path.Data");
               xPath.Add(xPathData);
               ExportGeometry(xPathData);

               xParent.Add(xPath);
               break;

            case XDrawingExportFormat.DrawingImage:
               var xGeometryDrawing = new XElement("GeometryDrawing");
               ExportBrush(xGeometryDrawing, "Brush", Path.Fill);
               if(Path.Stroke != null)
               {
                  var xPen = new XElement("Pen",
                     new XAttribute("Thickness", Path.StrokeThickness.ToString(nfi)),
                     new XAttribute("DashCap", Path.StrokeDashCap),
                     new XAttribute("StartLineCap", Path.StrokeStartLineCap),
                     new XAttribute("EndLineCap", Path.StrokeEndLineCap),
                     new XAttribute("LineJoin", Path.StrokeLineJoin),
                     new XAttribute("MiterLimit", Path.StrokeMiterLimit.ToString(nfi)));

                  if (Path.StrokeDashArray != null && Path.StrokeDashArray.Count > 0)
                  {
                     xPen.Add(new XElement("Pen.DashStyle",
                        new XElement("DashStyle",
                           new XAttribute("Dashes", DashArrayToString(Path.StrokeDashArray)),
                           new XAttribute("Offset", Path.StrokeDashOffset.ToString(nfi)))));
                  }

                  ExportBrush(xPen, "Brush", Path.Stroke);
                  
                  xGeometryDrawing.Add(new XElement("GeometryDrawing.Pen", xPen));
               }

               var xGeometry = new XElement("GeometryDrawing.Geometry");
               xGeometryDrawing.Add(xGeometry);
               ExportGeometry(xGeometry);

               xParent.Add(xGeometryDrawing);
               break;

            default:
               throw new ArgumentException("format not supported");
         }
      }

      protected abstract void ExportGeometry(XElement xParent);

      private void ExportBrush(XElement xElement, string propertyName, Brush brush)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         if (brush != null)
         {
            if (brush is SolidColorBrush)
            {
               xElement.Add(new XAttribute(propertyName, (brush as SolidColorBrush).Color.ToString()));
            }
            else if (brush is GradientBrush)
            {
               var xBrush = new XElement(brush.GetType().Name);
               var gBrush = brush as GradientBrush;
               foreach(var stop in gBrush.GradientStops)
               {
                  xBrush.Add(new XElement("GradientStop", 
                     new XAttribute("Offset", stop.Offset.ToString(nfi)),
                     new XAttribute("Color", stop.Color.ToString())));
               }

               if (brush is LinearGradientBrush)
               {
                  var lBrush = brush as LinearGradientBrush;
                  xBrush.Add(new XAttribute("StartPoint", lBrush.StartPoint.ToString(nfi)));
                  xBrush.Add(new XAttribute("EndPoint", lBrush.EndPoint.ToString(nfi)));
               }
               else if (brush is RadialGradientBrush)
               {
                  var rBrush = brush as RadialGradientBrush;
                  xBrush.Add(new XAttribute("Center", rBrush.Center.ToString(nfi)));
                  xBrush.Add(new XAttribute("GradientOrigin", rBrush.GradientOrigin.ToString(nfi)));
                  xBrush.Add(new XAttribute("RadiusX", rBrush.RadiusX.ToString(nfi)));
                  xBrush.Add(new XAttribute("RadiusY", rBrush.RadiusY.ToString(nfi)));
               }
               xElement.Add(new XElement(xElement.Name + "." + propertyName, xBrush));
            }
         }
      }

      protected virtual void Dispose(bool disposing)
      {
         if(disposing)
         {
            Drawing = null;
            if(Path != null)
            {
               (Path.Parent as Panel).Children.Remove(Path);
               Path = null;
            }
         }
      }

      #region IDisposable Members

      public void Dispose()
      {
         Dispose(true);
      }

      #endregion

      internal static XDrawingShape LoadFromGeometryDrawing(XDrawing drawing, XElement xGD)
      {
         var gd = new
            {
               SimpleFillBrush = (string)xGD.Attributes("Brush").FirstOrDefault(),
               ComplexFillBrushNode = (from b in xGD.Elements("GeometryDrawing.Brush")
                                       select b.Elements().FirstOrDefault()).FirstOrDefault(),
               Pen = (from gdp in xGD.Elements("GeometryDrawing.Pen")
                      select (from p in gdp.Elements("Pen")
                              select new
                              {
                                 Thickness = (string)p.Attributes("Thickness").FirstOrDefault(),
                                 DashCap = (string)p.Attributes("DashCap").FirstOrDefault(),
                                 StartLineCap = (string)p.Attributes("StartLineCap").FirstOrDefault(),
                                 EndLineCap = (string)p.Attributes("EndLineCap").FirstOrDefault(),
                                 LineJoin = (string)p.Attributes("LineJoin").FirstOrDefault(),
                                 MiterLimit = (string)p.Attributes("MiterLimit").FirstOrDefault(),
                                 SimpleBrush = (string)p.Attributes("Brush").FirstOrDefault(),
                                 ComplexBrushNode = (from b in p.Elements("Pen.Brush")
                                                     select b.Elements().FirstOrDefault()).FirstOrDefault(),
                                 DashStyle = (from pds in p.Elements("Pen.DashStyle")
                                              select (from ds in pds.Elements("DashStyle")
                                                      select new
                                                      {
                                                         Dashes = (string)ds.Attributes("Dashes").FirstOrDefault(),
                                                         Offset = (string)ds.Attributes("Offset").FirstOrDefault(),
                                                      }).FirstOrDefault()).FirstOrDefault()
                              }).FirstOrDefault()).FirstOrDefault(),
               GeometryNode = (from g in xGD.Elements("GeometryDrawing.Geometry")
                               select g.Elements().FirstOrDefault()).FirstOrDefault()
            };

         if (gd.Pen != null)
         {
            if (gd.Pen.DashStyle != null)
            {
               return CreateShape(drawing,
                  CreateBrush(gd.SimpleFillBrush, gd.ComplexFillBrushNode),
                  gd.Pen.Thickness,
                  gd.Pen.DashCap,
                  gd.Pen.StartLineCap,
                  gd.Pen.EndLineCap,
                  gd.Pen.LineJoin,
                  gd.Pen.MiterLimit,
                  CreateBrush(gd.Pen.SimpleBrush, gd.Pen.ComplexBrushNode),
                  gd.Pen.DashStyle.Dashes,
                  gd.Pen.DashStyle.Offset,
                  gd.GeometryNode);
            }
            else
            {
               return CreateShape(drawing,
                  CreateBrush(gd.SimpleFillBrush, gd.ComplexFillBrushNode),
                  gd.Pen.Thickness,
                  gd.Pen.DashCap,
                  gd.Pen.StartLineCap,
                  gd.Pen.EndLineCap,
                  gd.Pen.LineJoin,
                  gd.Pen.MiterLimit,
                  CreateBrush(gd.Pen.SimpleBrush, gd.Pen.ComplexBrushNode),
                  null, null,
                  gd.GeometryNode);
            }
         }
         else
         {
            return CreateShape(drawing,
               CreateBrush(gd.SimpleFillBrush, gd.ComplexFillBrushNode),
               null, null, null, null,
               null, null, null, null, null,
               gd.GeometryNode);
         }
      }

      internal static XDrawingShape LoadFromPath(XDrawing drawing, XElement xPath)
      {
         var p = new
         {
            SimpleFillBrush = (string)xPath.Attributes("Fill").FirstOrDefault(),
            ComplexFillBrushNode = (from fb in xPath.Elements("Path.Fill")
                                    select fb.Elements().FirstOrDefault()).FirstOrDefault(),
            Thickness = (string)xPath.Attributes("StrokeThickness").FirstOrDefault(),
            DashCap = (string)xPath.Attributes("StrokeDashCap").FirstOrDefault(),
            StartLineCap = (string)xPath.Attributes("StrokeStartLineCap").FirstOrDefault(),
            EndLineCap = (string)xPath.Attributes("StrokeEndLineCap").FirstOrDefault(),
            LineJoin = (string)xPath.Attributes("StrokeLineJoin").FirstOrDefault(),
            MiterLimit = (string)xPath.Attributes("StrokeMiterLimit").FirstOrDefault(),
            SimpleStrokeBrush = (string)xPath.Attributes("Stroke").FirstOrDefault(),
            ComplexStrokeBrushNode = (from sb in xPath.Elements("Path.Stroke")
                                      select sb.Elements().FirstOrDefault()).FirstOrDefault(),
            DashArray = (string)xPath.Attributes("StrokeDashArray").FirstOrDefault(),
            DashOffset = (string)xPath.Attributes("StrokeDashOffset").FirstOrDefault(),
            GeometryNode = (from g in xPath.Elements("Path.Data")
                            select g.Elements().FirstOrDefault()).FirstOrDefault()
         };

         return CreateShape(drawing,
            CreateBrush(p.SimpleFillBrush, p.ComplexFillBrushNode),
            p.Thickness,
            p.DashCap,
            p.StartLineCap,
            p.EndLineCap,
            p.LineJoin,
            p.MiterLimit,
            CreateBrush(p.SimpleStrokeBrush, p.ComplexStrokeBrushNode),
            p.DashArray,
            p.DashOffset,
            p.GeometryNode);
      }

      private static XDrawingShape CreateShape(XDrawing drawing, Brush fillBrush, string penThickness, string penDashCap,
         string penStartLineCap, string penEndLineCap, string penLineJoin, string penMiterLimit,
         Brush penBrush, string penDashArray, string penDashOffset, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var path = new Path();
         path.Fill = fillBrush;
         if (penThickness != null)
         {
            path.StrokeThickness = Double.Parse(penThickness, nfi);
         }
         if (penDashCap != null)
         {
            path.StrokeDashCap = (PenLineCap)Enum.Parse(typeof(PenLineCap), penDashCap);
         }
         if (penStartLineCap != null)
         {
            path.StrokeStartLineCap = (PenLineCap)Enum.Parse(typeof(PenLineCap), penStartLineCap);
         }
         if (penEndLineCap != null)
         {
            path.StrokeEndLineCap = (PenLineCap)Enum.Parse(typeof(PenLineCap), penEndLineCap);
         }
         if (penLineJoin != null)
         {
            path.StrokeLineJoin = (PenLineJoin)Enum.Parse(typeof(PenLineJoin), penLineJoin);
         }
         if (penMiterLimit != null)
         {
            path.StrokeMiterLimit = Double.Parse(penMiterLimit, nfi);
         }
         path.Stroke = penBrush;
         if (penDashArray != null)
         {
            XDrawingShape.StringToDashArray(penDashArray, path.StrokeDashArray);
         }
         if (penDashOffset != null)
         {
            path.StrokeDashOffset = Double.Parse(penDashOffset, nfi);
         }

         var types = typeof(XDrawingShape).Assembly.GetTypes();
         foreach(var type in types)
         {
            if(type.IsSubclassOf(typeof(XDrawingShape)))
            {
               var attributes = type.GetCustomAttributes(typeof(XDrawGeometryAttribute), false) as XDrawGeometryAttribute[];
               var geomName = xGeometry.Name.LocalName;

               // check if we have a TextGeometry, which is no "real" XAML node type
               if (String.CompareOrdinal(geomName, "GeometryGroup") == 0)
               {
                  var c = xGeometry.Parent.FirstNode as XComment;

                  if (c != null && c.Value.StartsWith("<TextGeometry"))
                  {
                     geomName = "TextGeometry";
                  }
               }

               if(attributes.Length >= 1 && String.CompareOrdinal(geomName, attributes[0].GeometryName) == 0)
               {
                  var mi = type.GetMethod(
                     "CreateFromXml", 
                     BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                     null,
                     new Type[] { typeof(XDrawing), typeof(Path), typeof(XElement) },
                     null);
                  if(mi == null)
                  {
                     throw new Exception("shape type does not support static method CreateFromXml");
                  }
                  return mi.Invoke(null, new object[] { drawing, path, xGeometry }) as XDrawingShape;
               }
            }
         }

         throw new Exception("Unknown geometry type: " + xGeometry.Name);
      }

      private static Brush CreateBrush(string solidBrushColor, XElement xComplexBrush)
      {
         if (solidBrushColor != null)
         {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(solidBrushColor));
         }
         else if (xComplexBrush != null)
         {
            var nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            GradientBrush gBrush = null;
            if (String.CompareOrdinal(xComplexBrush.Name.LocalName, "LinearGradientBrush") == 0)
            {
               var lBrush = new LinearGradientBrush();
               if (xComplexBrush.Attribute("StartPoint") != null)
               {
                  var props = xComplexBrush.Attribute("StartPoint").Value.Split(',', ' ');
                  lBrush.StartPoint = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
               }
               if (xComplexBrush.Attribute("EndPoint") != null)
               {
                  var props = xComplexBrush.Attribute("EndPoint").Value.Split(',', ' ');
                  lBrush.EndPoint = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
               }
               gBrush = lBrush;
            }
            else if (String.CompareOrdinal(xComplexBrush.Name.LocalName, "RadialGradientBrush") == 0)
            {
               var rBrush = new RadialGradientBrush();
               if (xComplexBrush.Attribute("Center") != null)
               {
                  var props = xComplexBrush.Attribute("Center").Value.Split(',', ' ');
                  rBrush.Center = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
               }
               if (xComplexBrush.Attribute("GradientOrigin") != null)
               {
                  var props = xComplexBrush.Attribute("GradientOrigin").Value.Split(',', ' ');
                  rBrush.GradientOrigin = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
               }
               if (xComplexBrush.Attribute("RadiusX") != null)
               {
                  rBrush.RadiusX = Double.Parse(xComplexBrush.Attribute("RadiusX").Value, nfi);
               }
               if (xComplexBrush.Attribute("RadiusY") != null)
               {
                  rBrush.RadiusY = Double.Parse(xComplexBrush.Attribute("RadiusY").Value, nfi);
               }
               gBrush = rBrush;
            }
            else
            {
               throw new Exception("Unknwon complex brush type: " + xComplexBrush.Name.LocalName);
            }
            if (gBrush != null)
            {
               var xStops = from s in xComplexBrush.Elements("GradientStop")
                            select new
                            {
                               Offset = (string)s.Attributes("Offset").FirstOrDefault(),
                               Color = (string)s.Attributes("Color").FirstOrDefault()
                            };

               foreach (var s in xStops)
               {
                  var stop = new GradientStop();
                  if (s.Offset != null)
                  {
                     stop.Offset = Double.Parse(s.Offset, nfi);
                  }
                  if (s.Color != null)
                  {
                     stop.Color = (Color)ColorConverter.ConvertFromString(s.Color);
                  }
                  gBrush.GradientStops.Add(stop);
               }
               return gBrush;
            }
         }
         return null;
      }

      internal void MoveBy(Vector size)
      {
         SetPoint(GetPoint() + size);

      }
   }

   [AttributeUsage(AttributeTargets.Class)]
   public class XDrawGeometryAttribute : Attribute
   {
      public XDrawGeometryAttribute(string geometryName)
      {
         GeometryName = geometryName;
      }

      public string GeometryName { get; private set; }
   }
}
