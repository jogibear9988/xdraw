using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;

namespace XDraw.Controls.Media
{
   public class ScreenSpaceEllipse2 : OffsetableScreenSpaceShape
   {
      private EllipseGeometry _ellipseGeometry;

      public static readonly DependencyProperty XProperty = DependencyProperty.Register(
         "X", typeof(double), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(
         "RadiusX", typeof(double), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty YProperty = DependencyProperty.Register(
         "Y", typeof(double), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(
         "RadiusY", typeof(double), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));

      public static readonly DependencyProperty ScreenSpaceRadiusProperty = DependencyProperty.Register(
         "ScreenSpaceRadius", typeof(bool), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      // Methods
      internal override void CacheDefiningGeometry()
      {
         var scaleFactor = GetScaleFactor();
         var rX = RadiusX;
         var rY = RadiusY;
         if (ScreenSpaceRadius)
         {
            rX /= scaleFactor;
            rY /= scaleFactor;
         }
         this._ellipseGeometry = new EllipseGeometry(new Point(X + ScreenSpaceOffsetX / scaleFactor, Y + ScreenSpaceOffsetY / scaleFactor), rX, rY);
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get { return this._ellipseGeometry; }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double X
      {
         get { return (double)base.GetValue(XProperty); }
         set { base.SetValue(XProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double RadiusX
      {
         get { return (double)base.GetValue(RadiusXProperty); }
         set { base.SetValue(RadiusXProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double Y
      {
         get { return (double)base.GetValue(YProperty); }
         set { base.SetValue(YProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double RadiusY
      {
         get { return (double)base.GetValue(RadiusYProperty); }
         set { base.SetValue(RadiusYProperty, value); }
      }

      public bool ScreenSpaceRadius
      {
         get { return (bool)GetValue(ScreenSpaceRadiusProperty); }
         set { SetValue(ScreenSpaceRadiusProperty, value); }
      }
   }
}
