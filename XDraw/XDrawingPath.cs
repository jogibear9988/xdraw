using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Globalization;

namespace remes.XDraw
{
   [XDrawGeometry("PathGeometry")]
   public class XDrawingPath : XDrawingShape
   {
      public static HitTestInfo CreateNewByDrag(XDrawing drawing, Point startPt, out XDrawingModes newMode)
      {
         var path = new Path();
         path.Stroke = new SolidColorBrush(Colors.Black);
         path.StrokeThickness = 1.0;

         var pathG = new PathGeometry();
         var figure = new PathFigure();
         figure.StartPoint = startPt;
         var segment = new LineSegment(startPt, true);
         figure.Segments.Add(segment);
         pathG.Figures.Add(figure);
         path.Data = pathG;

         var shape = new XDrawingPath(drawing, path);
         ControlPoint cp;
         shape.CreateControlPoints(true, out cp, null);
         newMode = XDrawingModes.Edit;
         var hti = new HitTestInfo();
         hti.Shape = shape;
         hti.Offset = new Vector(0.0, 0.0);
         hti.ControlPoint = cp;
         return hti;
      }

      public XDrawingPath(XDrawing drawing, Path path)
         : base(drawing, path)
      {
         if (!(Path.Data is PathGeometry))
         {
            throw new ArgumentException("path.Data must be of type PathGeometry");
         }
         Path.Tag = this;
      }

      private PathGeometry PathGeom
      {
         get { return Path.Data as PathGeometry; }
      }

      protected override Point GetPoint()
      {
         return PathGeom.GetFirstStartPoint();
      }

      protected override void SetPoint(Point pt)
      {
         var offset = pt - PathGeom.GetFirstStartPoint();
         PathGeom.DoForAllPoints(new RefPointCallback((ref Point _pt) =>
         {
            _pt += offset;
         }));
      }

      internal override void CreateControlPoints(bool editMode)
      {
         ControlPoint cp;
         CreateControlPoints(editMode, out cp, null);
      }

      private ControlPoint CreateControlPoints(bool editMode, out ControlPoint lastEndPoint, PathSegment psRet)
      {
         ControlPoint cpRet = null;
         lastEndPoint = null;
         if (editMode)
         {
            ControlPoint lastCP = null;
            ControlPoint bezierTmpCP = null;

            PathGeom.DoForAllPointDPs(new DependencyPropertyCallback((DependencyObject obj, DependencyProperty dp) =>
            {
               var cp = new ControlPoint(Drawing, this, obj, dp, 0, true, true);
               cp.Tag = obj;
               Drawing.AddControlPoint(cp);
               if (dp.OwnerType == typeof(BezierSegment))
               {
                  if (dp.Name == "Point1") // control point 1
                  {
                     lastCP.SubPoint = cp;
                     cp.ConnectedTo = lastCP;
                     cp.IsSelectable = false;
                  }
                  else if (dp.Name == "Point2") // control point 2
                  {
                     cp.IsSelectable = false;
                     bezierTmpCP = cp;
                  }
                  else if (dp.Name == "Point3") // end point
                  {
                     cp.SubPoint = bezierTmpCP;
                     bezierTmpCP.ConnectedTo = cp;
                     lastCP = cp;
                  }
               }
               else if (dp.OwnerType == typeof(QuadraticBezierSegment))
               {
                  if (dp.Name == "Point1") // control point 2
                  {
                     cp.IsSelectable = false;
                     bezierTmpCP = cp;
                  }
                  else if (dp.Name == "Point2") // end point
                  {
                     cp.SubPoint = bezierTmpCP;
                     bezierTmpCP.ConnectedTo = cp;
                     lastCP = cp;
                  }
               }
               else
               {
                  lastCP = cp;
               }
               if (obj == psRet)
               {
                  cpRet = lastCP;
               }
            }));
            lastEndPoint = lastCP;
         }
         return cpRet;
      }

      private static bool m_UseGeometryMiniLanguage = true;

      public static bool UseGeometryMiniLanguage
      {
         get { return m_UseGeometryMiniLanguage; }
         set { m_UseGeometryMiniLanguage = value; }
      }

      protected override void ExportGeometry(XElement xParent)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var xPath = new XElement("PathGeometry");

         if (m_UseGeometryMiniLanguage)
         {
            var figures = new StringBuilder();
            foreach (var figure in PathGeom.Figures)
            {
               figures.AppendFormat(nfi, "M {0} ", figure.StartPoint);

               foreach (var segment in figure.Segments)
               {
                  if (segment is LineSegment)
                  {
                     figures.AppendFormat(nfi, "L {0} ", (segment as LineSegment).Point);
                  }
                  else if (segment is ArcSegment)
                  {
                     var aSegment = segment as ArcSegment;
                     figures.AppendFormat(nfi, "A {0} {1} {2},{3} {4} ", 
                        aSegment.Size, aSegment.RotationAngle, 
                        aSegment.IsLargeArc ? "1" : "0",
                        aSegment.SweepDirection == SweepDirection.Clockwise ? "1" : "0",
                        aSegment.Point);
                  }
                  else if (segment is BezierSegment)
                  {
                     figures.AppendFormat(nfi, "C {0} {1} {2} ",
                        (segment as BezierSegment).Point1,
                        (segment as BezierSegment).Point2,
                        (segment as BezierSegment).Point3);
                  }
                  else if (segment is QuadraticBezierSegment)
                  {
                     figures.AppendFormat(nfi, "Q {0} {1} ",
                        (segment as QuadraticBezierSegment).Point1,
                        (segment as QuadraticBezierSegment).Point2);
                  }
               }

               if(figure.IsClosed)
               {
                  figures.Append("Z ");
               }
            }
            xPath.Add(new XAttribute("Figures", figures.ToString().Trim()));
         }
         else
         {
            foreach (var figure in PathGeom.Figures)
            {
               var xPathFigure = new XElement("PathFigure",
                  new XAttribute("StartPoint", figure.StartPoint.ToString(nfi)),
                  new XAttribute("IsClosed", figure.IsClosed ? "True" : "False"),
                  new XAttribute("IsFilled", figure.IsFilled ? "True" : "False"));

               foreach (var segment in figure.Segments)
               {
                  var xSegment = new XElement(segment.GetType().Name);
                  segment.DoForAllPointDPs(new DependencyPropertyCallback((DependencyObject obj, DependencyProperty dp) =>
                     {
                        xSegment.Add(new XAttribute(dp.Name, ((Point)obj.GetValue(dp)).ToString(nfi)));
                     }));
                  xPathFigure.Add(xSegment);
               }

               xPath.Add(xPathFigure);
            }
         }
         xParent.Add(xPath);
      }

      internal static XDrawingShape CreateFromXml(XDrawing drawing, Path path, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var pathG = new PathGeometry();

         var figuresStr = (string)xGeometry.Attributes("Figures").FirstOrDefault();
         if (figuresStr != null)
         {
            LoadFromGeomMiniLang(pathG, figuresStr);
         }
         else
         {
            LoadPathFigures(pathG, xGeometry);
         }

         path.Data = pathG;
         return new XDrawingPath(drawing, path);
      }

      private static void LoadPathFigures(PathGeometry pathG, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var xFigures = from f in xGeometry.Elements("PathFigure")
                        select new
                        {
                           Node = f,
                           StartPoint = (string)f.Attributes("StartPoint").FirstOrDefault(),
                           IsClosed = (string)f.Attributes("IsClosed").FirstOrDefault(),
                           IsFilled = (string)f.Attributes("IsFilled").FirstOrDefault()
                        };

         foreach (var f in xFigures)
         {
            var figure = new PathFigure();
            if (f.StartPoint != null)
            {
               var props = f.StartPoint.Split(',', ' ');
               figure.StartPoint = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
            }
            if (f.IsClosed != null)
            {
               figure.IsClosed = String.Compare(f.IsClosed.Trim(), "True", StringComparison.OrdinalIgnoreCase) == 0;
            }
            if (f.IsFilled != null)
            {
               figure.IsFilled = String.Compare(f.IsFilled.Trim(), "True", StringComparison.OrdinalIgnoreCase) == 0;
            }

            var xSegments = from s in f.Node.Elements()
                            select new
                            {
                               Node = s,
                               Type = s.Name.LocalName,
                               Point = (string)s.Attributes("Point").FirstOrDefault(),
                               Point1 = (string)s.Attributes("Point1").FirstOrDefault(),
                               Point2 = (string)s.Attributes("Point2").FirstOrDefault(),
                               Point3 = (string)s.Attributes("Point3").FirstOrDefault()
                            };

            foreach (var s in xSegments)
            {
               PathSegment segment;
               if (String.CompareOrdinal(s.Type, "LineSegment") == 0)
               {
                  segment = new LineSegment();
               }
               else if (String.CompareOrdinal(s.Type, "ArcSegment") == 0)
               {
                  var arc = new ArcSegment();
                  if (s.Node.Attribute("Size") != null)
                  {
                     arc.Size = Size.Parse(s.Node.Attribute("Size").Value);
                  }
                  if (s.Node.Attribute("RotationAngle") != null)
                  {
                     arc.RotationAngle = Double.Parse(s.Node.Attribute("RotationAngle").Value, nfi);
                  }
                  if (s.Node.Attribute("SweepDirection") != null)
                  {
                     arc.SweepDirection = (SweepDirection)Enum.Parse(typeof(SweepDirection), s.Node.Attribute("SweepDirection").Value);
                  }
                  segment = arc;
               }
               else if (String.CompareOrdinal(s.Type, "BezierSegment") == 0)
               {
                  segment = new BezierSegment();
               }
               else if (String.CompareOrdinal(s.Type, "QuadraticBezierSegment") == 0)
               {
                  segment = new QuadraticBezierSegment();
               }
               else
               {
                  throw new Exception("Unknown Path segment type: " + s.Type);
               }
               SetSegmentPoint(segment, "Point", s.Point);
               SetSegmentPoint(segment, "Point1", s.Point1);
               SetSegmentPoint(segment, "Point2", s.Point2);
               SetSegmentPoint(segment, "Point3", s.Point3);
               figure.Segments.Add(segment);
            }
            pathG.Figures.Add(figure);
         }
      }

      private static void LoadFromGeomMiniLang(PathGeometry pathG, string figuresStr)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         PathFigure figure = null;
         Point lastEndPoint = new Point();

         var n = figuresStr.IndexOfAny(new char[] { 'F', 'L', 'M', 'H', 'V', 'A', 'C', 'Q', 'S', 'Z' }, 0);
         while (n < figuresStr.Length && n >= 0)
         {
            var nNext = figuresStr.IndexOfAny(new char[] { 'F', 'L', 'M', 'H', 'V', 'A', 'C', 'Q', 'S', 'Z' }, n + 1);
            if (nNext < 0)
            {
               nNext = figuresStr.Length;
            }
            var props = figuresStr.Substring(n + 1, nNext - n - 1).Trim().Split(',', ' ');

            PathSegment segment = null;

            switch (figuresStr[n])
            {
               case 'F':
                  pathG.FillRule = props.Length >= 1 && props[0].Trim() == "0" ? FillRule.EvenOdd : FillRule.Nonzero;
                  break;

               case 'L':
                  if (props.Length >= 2)
                  {
                     lastEndPoint = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
                     segment = new LineSegment(lastEndPoint, true);
                  }
                  break;

               case 'M':
                  if (props.Length >= 2)
                  {
                     figure = new PathFigure();
                     figure.StartPoint = new Point(
                        Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));
                     lastEndPoint = figure.StartPoint;
                     pathG.Figures.Add(figure);
                  }
                  break;

               case 'H':
                  if (props.Length >= 1)
                  {
                     lastEndPoint = new Point(Double.Parse(props[0], nfi), lastEndPoint.Y);
                     segment = new LineSegment(lastEndPoint, true);
                  }
                  break;

               case 'V':
                  if (props.Length >= 1)
                  {
                     lastEndPoint = new Point(lastEndPoint.X, Double.Parse(props[0], nfi));
                     segment = new LineSegment(lastEndPoint, true);
                  }
                  break;

               case 'A':
                  if (props.Length >= 7)
                  {
                     lastEndPoint = new Point(Double.Parse(props[5], nfi), Double.Parse(props[6], nfi));
                     segment = new ArcSegment(
                        lastEndPoint,
                        new Size(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi)),
                        Double.Parse(props[2], nfi),
                        String.Compare(props[3].Trim(), "1", StringComparison.OrdinalIgnoreCase) == 0,
                        String.Compare(props[4].Trim(), "1", StringComparison.OrdinalIgnoreCase) == 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                        true);
                  }
                  break;

               case 'C':
                  if (props.Length >= 6)
                  {
                     lastEndPoint = new Point(Double.Parse(props[4], nfi), Double.Parse(props[5], nfi));
                     segment = new BezierSegment(
                        new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi)),
                        new Point(Double.Parse(props[2], nfi), Double.Parse(props[3], nfi)),
                        lastEndPoint, true);
                  }
                  break;

               case 'Q':
                  if (props.Length >= 4)
                  {
                     lastEndPoint = new Point(Double.Parse(props[2], nfi), Double.Parse(props[3], nfi));
                     segment = new QuadraticBezierSegment(
                        new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi)),
                        lastEndPoint, true);
                  }
                  break;

               case 'S':
                  if (props.Length >= 4)
                  {
                     lastEndPoint = new Point(Double.Parse(props[2], nfi), Double.Parse(props[3], nfi));
                     segment = new BezierSegment(
                        new Point(),
                        new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi)),
                        lastEndPoint, true);
                     segment.IsSmoothJoin = true;
                  }
                  break;

               case 'Z':
                  if (figure != null)
                  {
                     figure.IsClosed = true;
                  }
                  break;
            }

            if (segment != null)
            {
               if (figure != null)
               {
                  figure.Segments.Add(segment);
               }
            }
            n = nNext;
         }
      }

      private static void SetSegmentPoint(PathSegment segment, string propName, string str)
      {
         if (str != null)
         {
            var pi = segment.GetType().GetProperty(propName);
            if (pi != null)
            {
               var nfi = new NumberFormatInfo();
               nfi.NumberDecimalSeparator = ".";

               var props = str.Split(',', ' ');
               pi.SetValue(segment, new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi)), null);
            }
         }
      }

      internal ControlPoint ChangeSegmentType(ControlPoint controlPoint, string type)
      {
         PathFigure figure = null;
         foreach(var f in PathGeom.Figures)
         {
            if (f.Segments.Contains(controlPoint.Tag as PathSegment))
            {
               figure = f;
            }
         }
         if (figure == null)
         {
            throw new ArgumentException("Segment of controlPoint is not in this path");
         }

         Point startPoint = GetSegmentStartPoint(figure, controlPoint.Tag as PathSegment);
         Point endPoint = GetSegmentEndPoint(figure, controlPoint.Tag as PathSegment);
         Point pathControlPoint1 = new Point();
         Point pathControlPoint2 = new Point();

         if (controlPoint.Tag is LineSegment || controlPoint.Tag is ArcSegment)
         {
            pathControlPoint1 = startPoint + (endPoint - startPoint) * 0.3;
            pathControlPoint2 = endPoint + (startPoint - endPoint) * 0.3;
         }
         else if (controlPoint.Tag is BezierSegment)
         {
            pathControlPoint1 = (controlPoint.Tag as BezierSegment).Point1;
            pathControlPoint2 = (controlPoint.Tag as BezierSegment).Point2;
         }
         else if (controlPoint.Tag is QuadraticBezierSegment)
         {
            pathControlPoint1 = startPoint + (endPoint - startPoint) * 0.3;
            pathControlPoint2 = (controlPoint.Tag as QuadraticBezierSegment).Point1;
         }

         PathSegment newSegment = null;
         if (String.CompareOrdinal(type, "LineSegment") == 0)
         {
            var lSegment = new LineSegment();
            lSegment.Point = endPoint;
            newSegment = lSegment;
         }
         else if (String.CompareOrdinal(type, "ArcSegment") == 0)
         {
            var aSegment = new ArcSegment();
            aSegment.Point = endPoint;
            var d = endPoint - startPoint;
            aSegment.Size = new Size(Math.Abs(d.X) * 2.0, Math.Abs(d.Y) * 2.0);
            aSegment.IsLargeArc = false;
            aSegment.SweepDirection = SweepDirection.Clockwise;
            newSegment = aSegment;
         }
         else if (String.CompareOrdinal(type, "BezierSegment") == 0)
         {
            var bSegment = new BezierSegment();
            bSegment.Point1 = pathControlPoint1;
            bSegment.Point2 = pathControlPoint2;
            bSegment.Point3 = endPoint;
            newSegment = bSegment;
         }
         else if (String.CompareOrdinal(type, "QuadraticBezierSegment") == 0)
         {
            var qSegment = new QuadraticBezierSegment();
            qSegment.Point1 = pathControlPoint2;
            qSegment.Point2 = endPoint;
            newSegment = qSegment;
         }

         if (newSegment != null)
         {
            figure.Segments.Insert(figure.Segments.IndexOf(controlPoint.Tag as PathSegment), newSegment);
            figure.Segments.Remove(controlPoint.Tag as PathSegment);
            Drawing.ClearControlPoints();
            ControlPoint cpDummy;
            controlPoint = CreateControlPoints(true, out cpDummy, newSegment);
         }
         return controlPoint;
      }

      private Point GetSegmentStartPoint(PathFigure figure, PathSegment pathSegment)
      {
         var n = figure.Segments.IndexOf(pathSegment) - 1;
         if (n < 0)
         {
            return figure.StartPoint;
         }
         return GetSegmentEndPoint(figure, figure.Segments[n]);
      }

      private Point GetSegmentEndPoint(PathFigure figure, PathSegment pathSegment)
      {
         if (pathSegment is LineSegment)
         {
            return (pathSegment as LineSegment).Point;
         }
         else if (pathSegment is ArcSegment)
         {
            return (pathSegment as ArcSegment).Point;
         }
         else if (pathSegment is BezierSegment)
         {
            return (pathSegment as BezierSegment).Point3;
         }
         else if (pathSegment is QuadraticBezierSegment)
         {
            return (pathSegment as QuadraticBezierSegment).Point2;
         }
         return new Point();
      }

      internal ControlPoint AddSegment(object relativeTo, bool after)
      {
         var newSegment = new LineSegment();
         if (relativeTo is PathFigure)
         {
            var figure = relativeTo as PathFigure;
            if (after)
            {
               newSegment.Point = figure.StartPoint + (GetSegmentEndPoint(figure, figure.Segments[0]) - figure.StartPoint) * 0.5;
               figure.Segments.Insert(0, newSegment);
            }
            else
            {
               var ept = GetSegmentEndPoint(figure, figure.Segments[figure.Segments.Count - 1]);
               newSegment.Point = ept + (figure.StartPoint - ept) * 0.5;
               figure.Segments.Add(newSegment);
            }
         }
         else if (relativeTo is PathSegment)
         {
            var segment = relativeTo as PathSegment;
            PathFigure figure = null;
            foreach(var f in PathGeom.Figures)
            {
               if (f.Segments.Contains(segment))
               {
                  figure = f;
               }
            }
            if (figure == null)
            {
               throw new ArgumentException("Segment of controlPoint is not in this path");
            }

            if (after)
            {
               var p1 = GetSegmentEndPoint(figure, segment);

               Point p2;
               if (figure.Segments.IndexOf(segment) == figure.Segments.Count - 1)
               {
                  p2 = figure.StartPoint;
               }
               else
               {
                  p2 = GetSegmentEndPoint(figure, figure.Segments[figure.Segments.IndexOf(segment) + 1]);
               }
               newSegment.Point = p1 + (p2 - p1) * 0.5;
               figure.Segments.Insert(figure.Segments.IndexOf(segment) + 1, newSegment);
            }
            else
            {
               var p1 = GetSegmentStartPoint(figure, segment);
               var p2 = GetSegmentEndPoint(figure, segment);
               newSegment.Point = p1 + (p2 - p1) * 0.5;
               figure.Segments.Insert(figure.Segments.IndexOf(segment), newSegment);
            }
         }

         Drawing.ClearControlPoints();
         ControlPoint cpDummy;
         return CreateControlPoints(true, out cpDummy, newSegment);
      }

      internal void DeleteSegment(ControlPoint controlPoint)
      {
         var segment = controlPoint.Tag as PathSegment;
         if (segment != null)
         {
            PathFigure figure = null;
            foreach (var f in PathGeom.Figures)
            {
               if (f.Segments.Contains(segment))
               {
                  figure = f;
               }
            }
            if (figure == null)
            {
               throw new ArgumentException("Segment of controlPoint is not in this path");
            }

            figure.Segments.Remove(segment);

            Drawing.ClearControlPoints();
            CreateControlPoints(true);
         }
      }
   }
}
