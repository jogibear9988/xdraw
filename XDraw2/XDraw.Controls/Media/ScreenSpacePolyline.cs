using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace XDraw.Controls.Media
{
   public class ScreenSpacePolyline : ScreenSpaceShape
   {
      public static ScreenSpacePolyline CreatePolyline(IEnumerable<Point> points, Brush brush, double thickness)
      {
         var line = new ScreenSpacePolyline();
         line.Stroke = brush;
         line.StrokeThickness = thickness;
         line.Points = new PointCollection(points);
         return line;
      }

      // Fields
      private Geometry _polylineGeometry;

      public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
         "FillRule", typeof(FillRule), typeof(ScreenSpacePolyline), new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender));
      public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
         "Points", typeof(PointCollection), typeof(ScreenSpacePolyline), new FrameworkPropertyMetadata(new PointCollection(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      // Methods
      internal override void CacheDefiningGeometry()
      {
         PointCollection points = this.Points;
         PathFigure figure = new PathFigure();
         if (points == null)
         {
            this._polylineGeometry = Geometry.Empty;
         }
         else
         {
            if (points.Count > 0)
            {
               figure.StartPoint = points[0];
               if (points.Count > 1)
               {
                  Point[] pointArray = new Point[points.Count - 1];
                  for (int i = 1; i < points.Count; i++)
                  {
                     pointArray[i - 1] = points[i];
                  }
                  figure.Segments.Add(new PolyLineSegment(pointArray, true));
               }
            }
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.FillRule = this.FillRule;
            if (geometry.Bounds == Rect.Empty)
            {
               this._polylineGeometry = Geometry.Empty;
            }
            else
            {
               this._polylineGeometry = geometry;
            }
         }
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get
         {
            return this._polylineGeometry;
         }
      }

      public FillRule FillRule
      {
         get
         {
            return (FillRule)base.GetValue(FillRuleProperty);
         }
         set
         {
            base.SetValue(FillRuleProperty, value);
         }
      }

      public PointCollection Points
      {
         get
         {
            return (PointCollection)base.GetValue(PointsProperty);
         }
         set
         {
            base.SetValue(PointsProperty, value);
         }
      }
   }
}
