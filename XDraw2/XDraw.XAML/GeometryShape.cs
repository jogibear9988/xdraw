using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;

namespace XDraw.Xaml
{
   [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
   public class GeometryShape : FrameworkElement
   {
      public GeometryShape(GeometryDrawing geomDrawing) :
         base()
      {
         GeometryDrawing = geomDrawing;
         DependencyPropertyDescriptor.FromProperty(GeometryDrawing.BrushProperty, typeof(GeometryDrawing)).AddValueChanged(GeometryDrawing, OnVisualChanged);
         DependencyPropertyDescriptor.FromProperty(GeometryDrawing.PenProperty, typeof(GeometryDrawing)).AddValueChanged(GeometryDrawing, OnVisualChanged);
      }

      private void OnVisualChanged(object sender, EventArgs e)
      {
         InvalidateVisual();
      }

      public GeometryDrawing GeometryDrawing { get; private set; }

      // Methods
      protected GeometryShape()
      {
      }

      protected override Size ArrangeOverride(Size finalSize)
      {
         return finalSize;
      }

      internal virtual Size GetNaturalSize()
      {
         Geometry definingGeometry = GeometryDrawing.Geometry;
         Pen pen = GeometryDrawing.Pen;
         DashStyle dashStyle = null;
         if (pen != null)
         {
            dashStyle = pen.DashStyle;
            if (dashStyle != null)
            {
               pen.DashStyle = null;
            }
         }
         Rect renderBounds = definingGeometry.GetRenderBounds(pen);
         if (dashStyle != null)
         {
            pen.DashStyle = dashStyle;
         }
         return new Size(Math.Max(renderBounds.Right, 0.0), Math.Max(renderBounds.Bottom, 0.0));
      }

      protected override Size MeasureOverride(Size constraint)
      {
         return GetNaturalSize();
      }

      protected override void OnRender(DrawingContext drawingContext)
      {
         if (GeometryDrawing.Geometry != Geometry.Empty)
         {
            drawingContext.DrawGeometry(GeometryDrawing.Brush, GeometryDrawing.Pen, GeometryDrawing.Geometry);
         }
      }
   }
}
