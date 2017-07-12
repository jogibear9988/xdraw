using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace XDraw.Controls.Media
{
   public class ScreenSpaceRectangle2 : OffsetableScreenSpaceShape
   {
      private RectangleGeometry _rectangleGeometry;

      public static readonly DependencyProperty XProperty = DependencyProperty.Register(
         "X", typeof(double), typeof(ScreenSpaceRectangle2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty RectWidthProperty = DependencyProperty.Register(
         "RectWidth", typeof(double), typeof(ScreenSpaceRectangle2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty YProperty = DependencyProperty.Register(
         "Y", typeof(double), typeof(ScreenSpaceRectangle2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty RectHeightProperty = DependencyProperty.Register(
         "RectHeight", typeof(double), typeof(ScreenSpaceRectangle2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));

      public static readonly DependencyProperty ScreenSpaceSizeProperty = DependencyProperty.Register(
         "ScreenSpaceSize", typeof(bool), typeof(ScreenSpaceRectangle2), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      // Methods
      internal override void CacheDefiningGeometry()
      {
         var scaleFactor = GetScaleFactor();
         var w = RectWidth;
         var h = RectHeight;
         if (ScreenSpaceSize)
         {
            w /= scaleFactor;
            h /= scaleFactor;
         }
         this._rectangleGeometry = new RectangleGeometry(new Rect(new Point(X + ScreenSpaceOffsetX / scaleFactor, Y + ScreenSpaceOffsetY / scaleFactor), new Size(w, h)));
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get { return this._rectangleGeometry; }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double X
      {
         get { return (double)base.GetValue(XProperty); }
         set { base.SetValue(XProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double RectWidth
      {
         get { return (double)base.GetValue(RectWidthProperty); }
         set { base.SetValue(RectWidthProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double Y
      {
         get { return (double)base.GetValue(YProperty); }
         set { base.SetValue(YProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double RectHeight
      {
         get { return (double)base.GetValue(RectHeightProperty); }
         set { base.SetValue(RectHeightProperty, value); }
      }

      public bool ScreenSpaceSize
      {
         get { return (bool)GetValue(ScreenSpaceSizeProperty); }
         set { SetValue(ScreenSpaceSizeProperty, value); }
      }
   }
}
