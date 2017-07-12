using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace XDraw.Xaml
{
   public class RectangleGeometryCompanion : GeometryDrawingCompanion
   {
      public RectangleGeometryCompanion(object item) :
         base(item)
      {
         DependencyPropertyDescriptor.FromProperty(RectangleGeometry.RectProperty, typeof(LineGeometry)).AddValueChanged(Rectangle, OnGeometryChanged);
      }

      public static object CreateNew(Point position, ItemCreateMode createMode)
      {
         RectangleGeometry geom;
         if (createMode == ItemCreateMode.Drop)
         {
            geom = new RectangleGeometry(new Rect(position, new Size(5.0, 5.0)));
         }
         else
         {
            geom = new RectangleGeometry(new Rect(position, new Size(1.0, 1.0)));
         }
         return new GeometryDrawing(
            null, new Pen(Brushes.Black, 1.0),
            geom);
      }

      private void OnGeometryChanged(object sender, EventArgs e)
      {
         GeometryChanged();
         NotifyPropertyChanged("Width");
         NotifyPropertyChanged("Height");
      }

      [Browsable(false)]
      public RectangleGeometry Rectangle
      {
         get { return GeometryDrawing == null ? null : GeometryDrawing.Geometry as RectangleGeometry; }
      }

      [Category("Layout")]
      public double Width
      {
         get { return Rectangle.Rect.Width; }
         set
         {
            if (value != Rectangle.Rect.Width)
            {
               Rectangle.Rect = new Rect(Rectangle.Rect.TopLeft, new Size(value, Rectangle.Rect.Height));
            }
         }
      }

      [Category("Layout")]
      public double Height
      {
         get { return Rectangle.Rect.Height; }
         set
         {
            if (value != Rectangle.Rect.Height)
            {
               Rectangle.Rect = new Rect(Rectangle.Rect.TopLeft, new Size(Rectangle.Rect.Width, value));
            }
         }
      }

      private Rect _DragStartRect = new Rect();

      public override void OnDragStart()
      {
         base.OnDragStart();

         if (Rectangle == null)
         {
            return;
         }
         _DragStartRect = Rectangle.Rect;
      }

      public override void OnDragMove(Vector delta)
      {
         base.OnDragMove(delta);

         if (Rectangle == null)
         {
            return;
         }
         Rectangle.Rect = Rect.Offset(_DragStartRect, delta);

         GeometryChanged();
      }

      public override void OnUpdateResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnUpdateResize(newBounds, resizePos, resizeOrigin);

         if (Rectangle == null)
         {
            return;
         }

         Rectangle.Rect = newBounds;

         GeometryChanged();
      }
   }
}
