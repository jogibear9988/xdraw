using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace XDraw.Controls.Media
{
   public abstract class OffsetableScreenSpaceShape : ScreenSpaceShape
   {
      public static readonly DependencyProperty ScreenSpaceOffsetXProperty = DependencyProperty.Register(
         "ScreenSpaceOffsetX", typeof(double), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty ScreenSpaceOffsetYProperty = DependencyProperty.Register(
         "ScreenSpaceOffsetY", typeof(double), typeof(ScreenSpaceEllipse2), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));

      [TypeConverter(typeof(LengthConverter))]
      public double ScreenSpaceOffsetX
      {
         get { return (double)base.GetValue(ScreenSpaceOffsetXProperty); }
         set { base.SetValue(ScreenSpaceOffsetXProperty, value); }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double ScreenSpaceOffsetY
      {
         get { return (double)base.GetValue(ScreenSpaceOffsetYProperty); }
         set { base.SetValue(ScreenSpaceOffsetYProperty, value); }
      }
   }
}
