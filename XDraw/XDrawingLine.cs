using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Xml.Linq;

namespace remes.XDraw
{
   [XDrawGeometry("LineGeometry")]
   public class XDrawingLine : XDrawingShape
   {
      public static HitTestInfo CreateNewByDrag(XDrawing drawing, Point startPt, out XDrawingModes newMode)
      {
         var path = new Path();
         path.Stroke = new SolidColorBrush(Colors.Black);
         path.StrokeThickness = 1.0;
         path.Data = new LineGeometry(startPt, startPt);
         var shape = new XDrawingLine(drawing, path);
         ControlPoint cp;
         shape.CreateControlPoints(false, out cp);
         //newMode = XDrawingModes.Select;
         newMode = XDrawingModes.Edit;
         var hti = new HitTestInfo();
         hti.Shape = shape;
         hti.Offset = new Vector(0.0, 0.0);
         hti.ControlPoint = cp;
         return hti;
      }

      public XDrawingLine(XDrawing drawing, Path path)
         : base(drawing, path)
      {
         if (!(Path.Data is LineGeometry))
         {
            throw new ArgumentException("path.Data must be of type LineGeometry");
         }
         Path.Tag = this;
      }

      private LineGeometry LineGeom
      {
         get { return Path.Data as LineGeometry; }
      }

      protected override Point GetPoint()
      {
         return LineGeom.StartPoint;
      }

      protected override void SetPoint(Point pt)
      {
         var d = pt - LineGeom.StartPoint;
         LineGeom.StartPoint = pt;
         LineGeom.EndPoint += d;
      }

      internal override void CreateControlPoints(bool editMode)
      {
         ControlPoint cp;
         CreateControlPoints(editMode, out cp);
      }

      private void CreateControlPoints(bool editMode, out ControlPoint endCP)
      {
         Drawing.AddControlPoint(new ControlPoint(Drawing, this, LineGeom, LineGeometry.StartPointProperty, 0, true, false));
         endCP = new ControlPoint(Drawing, this, LineGeom, LineGeometry.EndPointProperty, 0, true, false);
         Drawing.AddControlPoint(endCP);
      }

      protected override void ExportGeometry(XElement xParent)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         xParent.Add(new XElement("LineGeometry",
            new XAttribute("StartPoint", LineGeom.StartPoint.ToString(nfi)),
            new XAttribute("EndPoint", LineGeom.EndPoint.ToString(nfi))));
      }

      internal static XDrawingShape CreateFromXml(XDrawing drawing, Path path, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var lGeom = new LineGeometry(
            Point.Parse(xGeometry.Attribute("StartPoint").Value),
            Point.Parse(xGeometry.Attribute("EndPoint").Value));

         path.Data = lGeom;
         return new XDrawingLine(drawing, path);
      }
   }
}
