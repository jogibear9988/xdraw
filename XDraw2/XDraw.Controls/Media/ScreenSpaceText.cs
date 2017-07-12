using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace XDraw.Controls.Media
{
   public class ScreenSpaceText : OffsetableScreenSpaceShape
   {
      public double X
      {
         get { return (double)GetValue(XProperty); }
         set { SetValue(XProperty, value); }
      }

      public static readonly DependencyProperty XProperty = DependencyProperty.Register(
         "X", typeof(double), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));

      public double Y
      {
         get { return (double)GetValue(YProperty); }
         set { SetValue(YProperty, value); }
      }

      public static readonly DependencyProperty YProperty = DependencyProperty.Register(
         "Y", typeof(double), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ScreenSpaceLine.IsDoubleFinite));

      public string Text
      {
         get { return (string)GetValue(TextProperty); }
         set { SetValue(TextProperty, value); }
      }

      public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
         "Text", typeof(string), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      public string FontFamily
      {
         get { return (string)GetValue(FontFamilyProperty); }
         set { SetValue(FontFamilyProperty, value); }
      }

      public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
         "FontFamily", typeof(string), typeof(ScreenSpaceText), new FrameworkPropertyMetadata("Arial", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      public FontStyle FontStyle
      {
         get { return (FontStyle)GetValue(FontStyleProperty); }
         set { SetValue(FontStyleProperty, value); }
      }

      public static readonly DependencyProperty FontStyleProperty =
          DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      public FontWeight FontWeight
      {
         get { return (FontWeight)GetValue(FontWeightProperty); }
         set { SetValue(FontWeightProperty, value); }
      }

      public static readonly DependencyProperty FontWeightProperty =
          DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      public FontStretch FontStretch
      {
         get { return (FontStretch)GetValue(FontStretchProperty); }
         set { SetValue(FontStretchProperty, value); }
      }

      public static readonly DependencyProperty FontStretchProperty =
          DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(FontStretches.Normal, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      public double FontSize
      {
         get { return (double)GetValue(FontSizeProperty); }
         set { SetValue(FontSizeProperty, value); }
      }

      public static readonly DependencyProperty FontSizeProperty =
          DependencyProperty.Register("FontSize", typeof(double), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      public TextAlignment Alignment
      {
         get { return (TextAlignment)GetValue(AlignmentProperty); }
         set { SetValue(AlignmentProperty, value); }
      }

      public static readonly DependencyProperty AlignmentProperty =
          DependencyProperty.Register("Alignment", typeof(TextAlignment), typeof(ScreenSpaceText), new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

      private FormattedText m_FormattedText = null;

      // Methods
      internal override void CacheDefiningGeometry()
      {
         var scaleFactor = GetScaleFactor();
         var typeface = new Typeface(new FontFamily(FontFamily), FontStyle, FontWeight, FontStretch);
         m_FormattedText = new FormattedText(
               Text, CultureInfo.CurrentCulture, FlowDirection,
               typeface,
               FontSize / scaleFactor, Fill);
         m_FormattedText.TextAlignment = Alignment;
      }

      internal override Rect GetDefiningGeometryBounds()
      {
         var rect = new Rect(new Point(X, Y), new Size(m_FormattedText.Width, m_FormattedText.Height));

         switch (Alignment)
         {
            case TextAlignment.Right:
               rect.X = rect.X - m_FormattedText.Width;
               break;

            case TextAlignment.Center:
               rect.X = rect.X - m_FormattedText.Width / 2.0;
               break;
         }
         return rect;
      }

      internal override Size GetNaturalSize()
      {
         if (m_FormattedText == null)
         {
            return new Size();
         }
         return new Size(m_FormattedText.Width, m_FormattedText.Height);
      }

      protected override void OnRender(DrawingContext drawingContext)
      {
         if (m_FormattedText != null)
         {
            var scaleFactor = GetScaleFactor();
            drawingContext.DrawText(m_FormattedText, new Point(X + ScreenSpaceOffsetX / scaleFactor, Y + ScreenSpaceOffsetY / scaleFactor));
         }
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get
         {
            // is usually not called anymore! but wee keep it, just to be sure ;)
            if (m_FormattedText == null)
            {
               return Geometry.Empty;
            }
            var scaleFactor = GetScaleFactor();
            return m_FormattedText.BuildGeometry(new Point(X + ScreenSpaceOffsetX / scaleFactor, Y + ScreenSpaceOffsetY / scaleFactor));
         }
      }

      public override Transform GeometryTransform
      {
         get
         {
            return Transform.Identity;
         }
      }

      public override Geometry RenderedGeometry
      {
         get
         {
            return this.DefiningGeometry;
         }
      }
   }
}
