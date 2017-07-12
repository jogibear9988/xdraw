using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Globalization;

namespace remes.XDraw
{
   [XDrawGeometry("TextGeometry")]
   public class XDrawingText : XDrawingShape
   {
      public static HitTestInfo CreateNewByDrag(XDrawing drawing, Point startPt, out XDrawingModes newMode)
      {
         var path = new Path();
         path.Fill = new SolidColorBrush(Colors.Black);
         path.StrokeThickness = 1.0;
         var shape = new XDrawingText(drawing, path);
         shape.TopLeft = startPt;
         shape.BottomRight = startPt;
         ControlPoint cp;
         shape.CreateControlPoints(false, out cp);
         //newMode = XDrawingModes.Select;
         newMode = XDrawingModes.Edit;
         var hti = new HitTestInfo();
         hti.Shape = shape;
         hti.Offset = new Vector(0.0, 0.0);
         hti.ControlPoint = cp;
         return hti;
      }

      private ControlPoint m_MoveControlPoint;

      public XDrawingText(XDrawing drawing, Path path)
         : base(drawing, path)
      {
         m_MoveControlPoint = new ControlPoint(Drawing, this, this, XDrawingText.TopLeftProperty, 0, false, false);
         Path.Tag = this;

         CreateGeom();
      }

      public string Text
      {
         get { return (string)GetValue(TextProperty); }
         set { SetValue(TextProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(XDrawingText), new UIPropertyMetadata("XDraw", TextPropertyChangedCallback));

      public FlowDirection FlowDirection
      {
         get { return (FlowDirection)GetValue(FlowDirectionProperty); }
         set { SetValue(FlowDirectionProperty, value); }
      }

      // Using a DependencyProperty as the backing store for FlowDirection.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FlowDirectionProperty =
          DependencyProperty.Register("FlowDirection", typeof(FlowDirection), typeof(XDrawingText), new UIPropertyMetadata(FlowDirection.LeftToRight, TextPropertyChangedCallback));

      public FontFamily FontFamily
      {
         get { return (FontFamily)GetValue(FontFamilyProperty); }
         set { SetValue(FontFamilyProperty, value); }
      }

      // Using a DependencyProperty as the backing store for TypeFace.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FontFamilyProperty =
          DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(XDrawingText), new UIPropertyMetadata(new FontFamily("Arial"), TextPropertyChangedCallback));

      public FontStyle FontStyle
      {
         get { return (FontStyle)GetValue(FontStyleProperty); }
         set { SetValue(FontStyleProperty, value); }
      }

      // Using a DependencyProperty as the backing store for FontStyle.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FontStyleProperty =
          DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(XDrawingText), new UIPropertyMetadata(FontStyles.Normal, TextPropertyChangedCallback));

      public FontWeight FontWeight
      {
         get { return (FontWeight)GetValue(FontWeightProperty); }
         set { SetValue(FontWeightProperty, value); }
      }

      // Using a DependencyProperty as the backing store for FontWeight.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FontWeightProperty =
          DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(XDrawingText), new UIPropertyMetadata(FontWeights.Normal, TextPropertyChangedCallback));

      public FontStretch FontStretch
      {
         get { return (FontStretch)GetValue(FontStretchProperty); }
         set { SetValue(FontStretchProperty, value); }
      }

      // Using a DependencyProperty as the backing store for FontStretch.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FontStretchProperty =
          DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(XDrawingText), new UIPropertyMetadata(FontStretches.Normal, TextPropertyChangedCallback));

      public double TextSize
      {
         get { return (double)GetValue(TextSizeProperty); }
         set { SetValue(TextSizeProperty, value); }
      }

      // Using a DependencyProperty as the backing store for TextSize.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TextSizeProperty =
          DependencyProperty.Register("TextSize", typeof(double), typeof(XDrawingText), new UIPropertyMetadata(20.0, TextPropertyChangedCallback));

      public TextTrimming Trimming
      {
         get { return (TextTrimming)GetValue(TrimmingProperty); }
         set { SetValue(TrimmingProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Trimming.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TrimmingProperty =
          DependencyProperty.Register("Trimming", typeof(TextTrimming), typeof(XDrawingText), new UIPropertyMetadata(TextTrimming.CharacterEllipsis, TextPropertyChangedCallback));

      public TextAlignment Alignment
      {
         get { return (TextAlignment)GetValue(AlignmentProperty); }
         set { SetValue(AlignmentProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Alignment.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty AlignmentProperty =
          DependencyProperty.Register("Alignment", typeof(TextAlignment), typeof(XDrawingText), new UIPropertyMetadata(TextAlignment.Left, TextPropertyChangedCallback));

      public Point TopLeft
      {
         get { return (Point)GetValue(TopLeftProperty); }
         set { SetValue(TopLeftProperty, value); }
      }

      // Using a DependencyProperty as the backing store for TopLeft.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TopLeftProperty =
          DependencyProperty.Register("TopLeft", typeof(Point), typeof(XDrawingText), new UIPropertyMetadata(new Point(), TextPropertyChangedCallback));

      public Point BottomRight
      {
         get { return (Point)GetValue(BottomRightProperty); }
         set { SetValue(BottomRightProperty, value); }
      }

      // Using a DependencyProperty as the backing store for BottomRight.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty BottomRightProperty =
          DependencyProperty.Register("BottomRight", typeof(Point), typeof(XDrawingText), new UIPropertyMetadata(new Point(), TextPropertyChangedCallback));

      private static void TextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as XDrawingText).TextPropertyChangedCallback(e);
      }

      private void TextPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
      {
         if (e.Property == TopLeftProperty)
         {
            BottomRight += (Point)e.NewValue - (Point)e.OldValue;
         }
         else
         {
            CreateGeom();
         }
      }

      private void CreateGeom()
      {
         var text = Text;
         if (String.IsNullOrEmpty(text))
         {
            text = "?";
         }
         var txt = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), TextSize, new SolidColorBrush(Colors.Black));
         txt.TextAlignment = Alignment;
         txt.Trimming = Trimming;
         txt.MaxLineCount = Int32.MaxValue;
         txt.MaxTextWidth = Math.Max(2 * TextSize, BottomRight.X - TopLeft.X);
         txt.MaxTextHeight = Math.Max(2 * TextSize, BottomRight.Y - TopLeft.Y);
         Path.Data = txt.BuildGeometry(TopLeft);
      }

      private PathGeometry TextGeom
      {
         get { return Path.Data as PathGeometry; }
      }

      protected override Point GetPoint()
      {
         return m_MoveControlPoint.GetPoint();
      }

      protected override void SetPoint(Point pt)
      {
         m_MoveControlPoint.SetPoint(pt);
      }

      internal override void CreateControlPoints(bool editMode)
      {
         ControlPoint cp;
         CreateControlPoints(editMode, out cp);
      }

      private void CreateControlPoints(bool editMode, out ControlPoint bottomRightControlPt)
      {
         Drawing.AddControlPoint(new ControlPoint(Drawing, this, this, XDrawingText.TopLeftProperty, 0, true, false));
         bottomRightControlPt = new ControlPoint(Drawing, this, this, XDrawingText.BottomRightProperty, 0, true, false);
         Drawing.AddControlPoint(bottomRightControlPt);
      }

      protected override void ExportGeometry(XElement xParent)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var tmpDoc = new XDocument(new XElement("TextGeometry",
            new XAttribute("TopLeft", TopLeft.ToString(nfi)),
            new XAttribute("BottomRight", BottomRight.ToString(nfi)),
            new XAttribute("Text", Text),
            new XAttribute("FontFamily", FontFamily),
            new XAttribute("FontStyle", FontStyle),
            new XAttribute("FontWeight", FontWeight),
            new XAttribute("TextSize", TextSize.ToString(nfi)),
            new XAttribute("Alignment", Alignment),
            new XAttribute("Trimming", Trimming)));

         xParent.Add(new XComment(tmpDoc.Root.ToString()));

         if (Path.Data is GeometryGroup)
         {
            ExportGeometryGroup(xParent, Path.Data as GeometryGroup);
         }
      }

      private void ExportGeometryGroup(XElement xParent, GeometryGroup geometryGroup)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var xGG = new XElement("GeometryGroup");
         xParent.Add(xGG);
         foreach (Geometry g in geometryGroup.Children)
         {
            if (g is GeometryGroup)
            {
               ExportGeometryGroup(xGG, g as GeometryGroup);
            }
            else if (g is PathGeometry)
            {
               var sb = new StringBuilder();
               foreach (var f in (g as PathGeometry).Figures)
               {
                  sb.Append(f.ToString(nfi));
                  sb.Append(" ");
               }
               var xPG = new XElement("PathGeometry",
                  new XAttribute("Figures", sb.ToString()));
               xGG.Add(xPG);
            }
         }
      }

      internal static XDrawingShape CreateFromXml(XDrawing drawing, Path path, XElement xGeometry)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         var tmpDoc = XDocument.Parse((xGeometry.Parent.FirstNode as XComment).Value);

         path.Data = null;
         var txt = new XDrawingText(drawing, path);

         var props = tmpDoc.Root.Attribute("TopLeft").Value.Split(',', ' ');
         txt.TopLeft = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));

         props = tmpDoc.Root.Attribute("BottomRight").Value.Split(',', ' ');
         txt.BottomRight = new Point(Double.Parse(props[0], nfi), Double.Parse(props[1], nfi));

         txt.Text = tmpDoc.Root.Attribute("Text").Value;
         txt.FontFamily = new FontFamily(tmpDoc.Root.Attribute("FontFamily").Value);
         txt.FontStyle = tmpDoc.Root.Attribute("FontStyle").Value == "Italic" ? FontStyles.Italic : FontStyles.Normal;
         txt.FontWeight = tmpDoc.Root.Attribute("FontWeight").Value == "Bold" ? FontWeights.Bold : FontWeights.Normal;
         txt.TextSize = Double.Parse(tmpDoc.Root.Attribute("TextSize").Value, nfi);
         txt.Alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), tmpDoc.Root.Attribute("Alignment").Value);
         txt.Trimming = (TextTrimming)Enum.Parse(typeof(TextTrimming), tmpDoc.Root.Attribute("Trimming").Value);

         return txt;
      }
   }
}
