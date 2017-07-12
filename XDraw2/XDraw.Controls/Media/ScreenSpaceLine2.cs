using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace XDraw.Controls.Media
{
   public class ScreenSpaceLine2 : ScreenSpaceShape
   {
      public ScreenSpaceLine2() :
         base()
      { }

      private LineGeometry _LineGeometry = null;
      
      public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
         "StartPoint", typeof(Point), typeof(ScreenSpaceLine2), new FrameworkPropertyMetadata(new Point(), 
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
      
      public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
         "EndPoint", typeof(Point), typeof(ScreenSpaceLine2), new FrameworkPropertyMetadata(new Point(), 
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      internal override void CacheDefiningGeometry()
      {
         this._LineGeometry = new LineGeometry(StartPoint, EndPoint);
      }

      protected override Geometry DefiningGeometry
      {
         get { return this._LineGeometry; }
      }

      [TypeConverter(typeof(PointConverter))]
      public Point StartPoint
      {
         get { return (Point)base.GetValue(StartPointProperty); }
         set { base.SetValue(StartPointProperty, value); }
      }

      [TypeConverter(typeof(PointConverter))]
      public Point EndPoint
      {
         get { return (Point)base.GetValue(EndPointProperty); }
         set { base.SetValue(EndPointProperty, value); }
      }
   }
}
