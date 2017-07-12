using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace XDraw.Xaml
{
   public class EllipseGeometryCompanion : GeometryDrawingCompanion
   {
      public EllipseGeometryCompanion(object item) :
         base(item)
      {
         DependencyPropertyDescriptor.FromProperty(EllipseGeometry.RadiusXProperty, typeof(LineGeometry)).AddValueChanged(Ellipse, OnGeometryChanged);
         DependencyPropertyDescriptor.FromProperty(EllipseGeometry.RadiusYProperty, typeof(LineGeometry)).AddValueChanged(Ellipse, OnGeometryChanged);
      }

      public static object CreateNew(Point position, ItemCreateMode createMode)
      {
         EllipseGeometry geom;
         if (createMode == ItemCreateMode.Drop)
         {
            geom = new EllipseGeometry(position, 5.0, 5.0);
         }
         else
         {
            geom = new EllipseGeometry(position, 1.0, 1.0);
         }
         return new GeometryDrawing(
            null, new Pen(Brushes.Black, 1.0),
            geom);
      }

      private void OnGeometryChanged(object sender, EventArgs e)
      {
         GeometryChanged();
         NotifyPropertyChanged("RadiusX");
         NotifyPropertyChanged("RadiusY");
      }
      
      [Browsable(false)]
      public EllipseGeometry Ellipse
      {
         get { return GeometryDrawing == null ? null : GeometryDrawing.Geometry as EllipseGeometry; }
      }

      [Category("Layout")]
      public double RadiusX
      {
         get { return Ellipse.RadiusX; }
         set
         {
            if (value != Ellipse.RadiusX)
            {
               Ellipse.RadiusX = value;
            }
         }
      }
      
      [Category("Layout")]
      public double RadiusY
      {
         get { return Ellipse.RadiusY; }
         set
         {
            if (value != Ellipse.RadiusY)
            {
               Ellipse.RadiusY = value;
            }
         }
      }

      private Point _DragStartCenter = new Point();
      private Rect _DragStartRect = new Rect();

      public override void OnDragStart()
      {
         base.OnDragStart();

         if (Ellipse == null)
         {
            return;
         }
         _DragStartCenter = Ellipse.Center;
      }

      public override void OnDragMove(Vector delta)
      {
         base.OnDragMove(delta);

         if (Ellipse == null)
         {
            return;
         }
         Ellipse.Center = _DragStartCenter + delta;

         GeometryChanged();
      }

      public override void OnStartResize(ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnStartResize(resizePos, resizeOrigin);

         if (Ellipse == null)
         {
            return;
         }
         var s = new Vector(Ellipse.RadiusX, Ellipse.RadiusY);
         _DragStartRect = new Rect(Ellipse.Center - s, Ellipse.Center + s);
      }

      public override void OnUpdateResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnUpdateResize(newBounds, resizePos, resizeOrigin);

         if (Ellipse == null)
         {
            return;
         }
         Ellipse.RadiusX = newBounds.Width / 2.0;
         Ellipse.RadiusY = newBounds.Height / 2.0;
         Ellipse.Center = newBounds.TopLeft + new Vector(Ellipse.RadiusX, Ellipse.RadiusY);

         GeometryChanged();
      }
   }
}
