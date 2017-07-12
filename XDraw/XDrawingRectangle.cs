using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;

namespace remes.XDraw
{
   [XDrawGeometry("RectangleGeometry")]
   public class XDrawingRectangle : XDrawingShape
   {
      public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
         "Width", typeof(double), typeof(XDrawingShape), new PropertyMetadata(1.0, WidthChanged));

      public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
         "Height", typeof(double), typeof(XDrawingShape), new PropertyMetadata(1.0, HeightChanged));

      public static HitTestInfo CreateNewByDrag(XDrawing drawing, Point startPt, out XDrawingModes newMode)
      {
         var path = new Path();
         path.Stroke = new SolidColorBrush(Colors.Black);
         path.StrokeThickness = 1.0;
         path.Data = new RectangleGeometry(new Rect(startPt, new Size(0.0, 0.0)));
         var shape = new XDrawingRectangle(drawing, path);
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

      public XDrawingRectangle(XDrawing drawing, Path path)
         : base(drawing, path)
      {
         if (!(Path.Data is RectangleGeometry))
         {
            throw new ArgumentException("path.Data must be of type RectangleGeometry");
         }
         m_MoveControlPoint = new ControlPoint(Drawing, this, RectGeom, RectangleGeometry.RectProperty, -1, false, false);
         Path.Tag = this;
         Width = RectGeom.Rect.Width;
         Height = RectGeom.Rect.Height;
         DependencyPropertyDescriptor.FromProperty(RectangleGeometry.RectProperty, typeof(RectangleGeometry)).AddValueChanged(Path.Data, RectChanged);
      }

      private void RectChanged(object sender, EventArgs e)
      {
         if (Width != RectGeom.Rect.Width)
         {
            Width = RectGeom.Rect.Width;
         }
         if (Height != RectGeom.Rect.Height)
         {
            Height = RectGeom.Rect.Height;
         }
      }
      
      private static void WidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var rect = (d as XDrawingRectangle).RectGeom.Rect;
         if (rect.Width != (double)e.NewValue)
         {
            rect.Width = (double)e.NewValue;
            (d as XDrawingRectangle).RectGeom.Rect = rect;
         }
      }

      public double Width
      {
         get { return (double)GetValue(WidthProperty); }
         set { SetValue(WidthProperty, value); }
      }

      private static void HeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var rect = (d as XDrawingRectangle).RectGeom.Rect;
         if (rect.Height != (double)e.NewValue)
         {
            rect.Height = (double)e.NewValue;
            (d as XDrawingRectangle).RectGeom.Rect = rect;
         }
      }

      public double Height
      {
         get { return (double)GetValue(HeightProperty); }
         set { SetValue(HeightProperty, value); }
      }

      private RectangleGeometry RectGeom
      {
         get { return Path.Data as RectangleGeometry; }
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

      private void CreateControlPoints(bool editMode, out ControlPoint bottomRightControlPt)
      {
         Drawing.AddControlPoint(new ControlPoint(Drawing, this, RectGeom, RectangleGeometry.RectProperty, 0, true, false));
         Drawing.AddControlPoint(new ControlPoint(Drawing, this, RectGeom, RectangleGeometry.RectProperty, 1, true, false));
         Drawing.AddControlPoint(new ControlPoint(Drawing, this, RectGeom, RectangleGeometry.RectProperty, 2, true, false));
         bottomRightControlPt = new ControlPoint(Drawing, this, RectGeom, RectangleGeometry.RectProperty, 3, true, false);
         Drawing.AddControlPoint(bottomRightControlPt);
      }

      protected override void ExportGeometry(XElement xParent)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         xParent.Add(new XElement("RectangleGeometry",
            new XAttribute("Rect", RectGeom.Rect.ToString(nfi)),
            new XAttribute("RadiusX", RectGeom.RadiusX.ToString(nfi)),
            new XAttribute("RadiusY", RectGeom.RadiusY.ToString(nfi))));
      }

      internal static XDrawingShape CreateFromXml(XDrawing drawing, Path path, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var rGeom = new RectangleGeometry(
            Rect.Parse(xGeometry.Attribute("Rect").Value));

         if (xGeometry.Attribute("RadiusX") != null)
         {
            rGeom.RadiusX = Double.Parse(xGeometry.Attribute("RadiusX").Value, nfi);
         }
         if (xGeometry.Attribute("RadiusY") != null)
         {
            rGeom.RadiusY = Double.Parse(xGeometry.Attribute("RadiusY").Value, nfi);
         }

         path.Data = rGeom;
         return new XDrawingRectangle(drawing, path);
      }
   }
}
