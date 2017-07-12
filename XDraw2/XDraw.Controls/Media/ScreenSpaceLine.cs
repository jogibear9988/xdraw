using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace XDraw.Controls.Media
{
   public sealed class ScreenSpaceLine : ScreenSpaceShape
   {
      // Fields
      private LineGeometry _lineGeometry;
      public static readonly DependencyProperty X1Property = DependencyProperty.Register(
         "X1", typeof(double), typeof(ScreenSpaceLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty X2Property = DependencyProperty.Register(
         "X2", typeof(double), typeof(ScreenSpaceLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty Y1Property = DependencyProperty.Register(
         "Y1", typeof(double), typeof(ScreenSpaceLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));
      public static readonly DependencyProperty Y2Property = DependencyProperty.Register(
         "Y2", typeof(double), typeof(ScreenSpaceLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));

      // Methods
      internal override void CacheDefiningGeometry()
      {
         Point startPoint = new Point(this.X1, this.Y1);
         Point endPoint = new Point(this.X2, this.Y2);
         this._lineGeometry = new LineGeometry(startPoint, endPoint);
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get
         {
            return this._lineGeometry;
         }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double X1
      {
         get
         {
            return (double)base.GetValue(X1Property);
         }
         set
         {
            base.SetValue(X1Property, value);
         }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double X2
      {
         get
         {
            return (double)base.GetValue(X2Property);
         }
         set
         {
            base.SetValue(X2Property, value);
         }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double Y1
      {
         get
         {
            return (double)base.GetValue(Y1Property);
         }
         set
         {
            base.SetValue(Y1Property, value);
         }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double Y2
      {
         get
         {
            return (double)base.GetValue(Y2Property);
         }
         set
         {
            base.SetValue(Y2Property, value);
         }
      }
   }

}
