using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Runtime.InteropServices;

[assembly:XmlnsDefinition("XDraw.Xaml", "XDraw.Xaml")]

namespace XDraw.Xaml
{
   public class TextShape : FrameworkElement
   {
      public TextShape() :
         base()
      { }


      [TypeConverter(typeof(PointConverter))]
      public Point Position
      {
         get { return (Point)GetValue(PositionProperty); }
         set { SetValue(PositionProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty PositionProperty =
          DependencyProperty.Register("Position", typeof(Point), typeof(TextShape),
          new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));


      [TypeConverter(typeof(SizeConverter))]
      public Size Size
      {
         get { return (Size)GetValue(SizeProperty); }
         set { SetValue(SizeProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SizeProperty =
          DependencyProperty.Register("Size", typeof(Size), typeof(TextShape),
          new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));

      [TypeConverter(typeof(LengthConverter))]
      public double TextSize
      {
         get { return (double)GetValue(TextSizeProperty); }
         set { SetValue(TextSizeProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TextSizeProperty =
          DependencyProperty.Register("TextSize", typeof(double), typeof(TextShape),
          new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));

      public string Text
      {
         get { return (string)GetValue(TextProperty); }
         set { SetValue(TextProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(TextShape),
          new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));


      public Pen Pen
      {
         get { return (Pen)GetValue(PenProperty); }
         set { SetValue(PenProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Pen.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty PenProperty =
          DependencyProperty.Register("Pen", typeof(Pen), typeof(TextShape),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));



      public Brush Fill
      {
         get { return (Brush)GetValue(FillProperty); }
         set { SetValue(FillProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FillProperty =
          DependencyProperty.Register("Fill", typeof(Brush), typeof(TextShape),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

      private static void ResetGeometry(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as TextShape).ResetGeometry(e);
      }

      private void ResetGeometry(DependencyPropertyChangedEventArgs e)
      {
         _Geometry = null;
      }

      private Geometry _Geometry = null;

      protected override Size ArrangeOverride(Size finalSize)
      {
         return finalSize;
      }

      internal virtual Size GetNaturalSize()
      {
         EnsureGeometryCreated();
         Geometry definingGeometry = _Geometry;
         Pen pen = Pen;
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

      private void EnsureGeometryCreated()
      {
         if (_Geometry == null)
         {
            var text = Text;
            if (String.IsNullOrEmpty(text))
            {
               text = "?";
            }
            var txt = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection, new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), TextSize, new SolidColorBrush(Colors.Black));
            txt.TextAlignment = TextAlignment.Left;
            txt.Trimming = TextTrimming.None;
            txt.MaxLineCount = Int32.MaxValue;
            txt.MaxTextWidth = Math.Max(2 * TextSize, Size.Width);
            txt.MaxTextHeight = Math.Max(2 * TextSize, Size.Height);

            _Geometry = txt.BuildGeometry(Position);
         }
      }

      protected override void OnRender(DrawingContext drawingContext)
      {
         EnsureGeometryCreated();
         if (_Geometry != Geometry.Empty)
         {
            drawingContext.DrawGeometry(Fill, Pen, _Geometry);
         }
      }
   }
}
