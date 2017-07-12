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
   [XDrawGeometry("EllipseGeometry")]
   public class XDrawingEllipse : XDrawingShape
   {
      public static HitTestInfo CreateNewByDrag(XDrawing drawing, Point startPt, out XDrawingModes newMode)
      {
         var path = new Path();
         path.Stroke = new SolidColorBrush(Colors.Black);
         path.StrokeThickness = 1.0;
         path.Data = new EllipseGeometry(startPt, 0.0, 5.0);
         var shape = new XDrawingEllipse(drawing, path);
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

      private ControlPoint m_MoveControlPoint;

      public XDrawingEllipse(XDrawing drawing, Path path)
         : base(drawing, path)
      {
         if (!(Path.Data is EllipseGeometry))
         {
            throw new ArgumentException("path.Data must be of type EllipseGeometry");
         }
         m_MoveControlPoint = new ControlPoint(Drawing, this, EllipseGeom, EllipseGeometry.CenterProperty, 0, false, false);
         Path.Tag = this;
      }

      private EllipseGeometry EllipseGeom
      {
         get { return Path.Data as EllipseGeometry; }
      }

      protected override Point GetPoint()
      {
         return m_MoveControlPoint.GetPoint();
      }

      protected override void SetPoint(Point pt)
      {
         m_MoveControlPoint.SetPoint(pt);
      }

      internal override void CreateControlPoints(bool editMode)
      {
         ControlPoint cp;
         CreateControlPoints(editMode, out cp);
      }

      private void CreateControlPoints(bool editMode, out ControlPoint radiusXcp)
      {
         radiusXcp = new ControlPoint(Drawing, this, EllipseGeom, EllipseGeometry.RadiusYProperty, 1, true, false);
         radiusXcp.RelativeTo = EllipseGeometry.CenterProperty;
         Drawing.AddControlPoint(radiusXcp);

         radiusXcp = new ControlPoint(Drawing, this, EllipseGeom, EllipseGeometry.RadiusXProperty, 0, true, false);
         radiusXcp.RelativeTo = EllipseGeometry.CenterProperty;
         Drawing.AddControlPoint(radiusXcp);
      }

      protected override void ExportGeometry(XElement xParent)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         xParent.Add(new XElement("EllipseGeometry",
            new XAttribute("Center", EllipseGeom.Center.ToString(nfi)),
            new XAttribute("RadiusX", EllipseGeom.RadiusX.ToString(nfi)),
            new XAttribute("RadiusY", EllipseGeom.RadiusY.ToString(nfi))));
      }

      internal static XDrawingShape CreateFromXml(XDrawing drawing, Path path, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var eGeom = new EllipseGeometry(
            Point.Parse(xGeometry.Attribute("Center").Value),
            Double.Parse(xGeometry.Attribute("RadiusX").Value, nfi),
            Double.Parse(xGeometry.Attribute("RadiusY").Value, nfi));

         path.Data = eGeom;
         return new XDrawingEllipse(drawing, path);
      }
   }
}
