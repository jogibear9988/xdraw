using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace XDraw.Controls.Media
{
   public sealed class ScreenSpacePolygon : ScreenSpaceShape
   {
      // Fields
      private Geometry _polygonGeometry;

      public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
         "FillRule", typeof(FillRule), typeof(ScreenSpacePolygon), new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender));
      public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
         "Points", typeof(PointCollection), typeof(ScreenSpacePolygon), new FrameworkPropertyMetadata(new PointCollection(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      // Methods
      internal override void CacheDefiningGeometry()
      {
         PointCollection points = this.Points;
         PathFigure figure = new PathFigure();
         if (points == null)
         {
            this._polygonGeometry = Geometry.Empty;
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
               figure.IsClosed = true;
            }
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.FillRule = this.FillRule;
            this._polygonGeometry = geometry;
         }
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get { return this._polygonGeometry; }
      }

      public FillRule FillRule
      {
         get { return (FillRule)base.GetValue(FillRuleProperty); }
         set { base.SetValue(FillRuleProperty, value); }
      }

      public PointCollection Points
      {
         get { return (PointCollection)base.GetValue(PointsProperty); }
         set { base.SetValue(PointsProperty, value); }
      }
   }
}
