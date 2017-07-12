using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;
using System.Globalization;
using System.ComponentModel;

namespace XDraw.Xaml
{
   public class TextGeometryCompanion : GeometryDrawingCompanion
   {
      public TextGeometryCompanion(object item) :
         base(item)
      {
         var customGeomDef = Geometry.GetValue(GeometryExtProps.CustomGeometryProperty);

         if (customGeomDef is string)
         {
            var xCustomGeom = XElement.Parse((string)customGeomDef);
            var xTextGeom = xCustomGeom.Element("TextGeometry");
            if (xTextGeom != null)
            {
               _Text = (string)xTextGeom.Attribute("Text") ?? String.Empty;
               _Position = Point.Parse((string)xTextGeom.Attribute("Position"));
               _Size = Size.Parse((string)xTextGeom.Attribute("Size"));
               _TextSize = Double.Parse((string)xTextGeom.Attribute("TextSize") ?? "12.0", CultureInfo.InvariantCulture);

               _FontFamily = new FontFamily((string)xTextGeom.Attribute("FontFamily") ?? "Arial");
               var fsc = new FontStyleConverter();
               _FontStyle = (FontStyle)fsc.ConvertFromInvariantString((string)xTextGeom.Attribute("FontStyle") ?? "Normal");
               var fwc = new FontWeightConverter();
               _FontWeight = (FontWeight)fwc.ConvertFromInvariantString((string)xTextGeom.Attribute("FontWeight") ?? "Normal");
               _Alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), (string)xTextGeom.Attribute("TextAlignment") ?? "Left");
               _Trimming = (TextTrimming)Enum.Parse(typeof(TextTrimming), (string)xTextGeom.Attribute("TextTrimming") ?? "None");
            }
         }

         UpdateGeometry();
      }

      public static object CreateNew(Point position, ItemCreateMode createMode)
      {
         PathGeometry geom = new PathGeometry();
         var xCustomGeom = new XElement("XDraw2_CustomGeometry",
               new XAttribute("CompanionAssembly", typeof(TextGeometryCompanion).Assembly.GetName().Name),
               new XAttribute("CompanionType", typeof(TextGeometryCompanion).FullName),
               new XElement("TextGeometry",
                  new XAttribute("Text", "Text"),
                  new XAttribute("Position", position.ToString(CultureInfo.InvariantCulture)),
                  new XAttribute("Size", new Size(40.0, 16.0).ToString(CultureInfo.InvariantCulture)),
                  new XAttribute("TextSize", "14.0"),
                  new XAttribute("FontFamily", "Arial")));

         geom.SetValue(GeometryExtProps.CustomGeometryProperty, xCustomGeom.ToString());

         return new GeometryDrawing(
            Brushes.Black, null, 
            geom);
      }

      public override void UpdateCustomGeometryProperty()
      {
         var customGeomDef = Geometry.GetValue(GeometryExtProps.CustomGeometryProperty);
         XElement xCustomGeom;
         if (customGeomDef is string)
         {
            xCustomGeom = XElement.Parse((string)customGeomDef);
         }
         else
         {
            xCustomGeom = new XElement("XDraw2_CustomGeometry",
               new XAttribute("CompanionAssembly", GetType().Assembly.GetName().Name),
               new XAttribute("CompanionType", GetType().FullName));
         }
         var xTextGeom = xCustomGeom.Element("TextGeometry");
         if (xTextGeom == null)
         {
            xTextGeom = new XElement("TextGeometry");
            xCustomGeom.Add(xTextGeom);
         }
         SetAttr(xTextGeom, "Text", _Text);
         SetAttr(xTextGeom, "Position", _Position.ToString(CultureInfo.InvariantCulture));
         SetAttr(xTextGeom, "Size", _Size.ToString(CultureInfo.InvariantCulture));
         SetAttr(xTextGeom, "TextSize", _TextSize.ToString(CultureInfo.InvariantCulture));
         SetAttr(xTextGeom, "FontFamily", _FontFamily.Source);
         var fsc = new FontStyleConverter();
         SetAttr(xTextGeom, "FontStyle", fsc.ConvertToInvariantString(_FontStyle));
         var fwc = new FontWeightConverter();
         SetAttr(xTextGeom, "FontWeight", fwc.ConvertToInvariantString(_FontWeight));
         SetAttr(xTextGeom, "TextAlignment", _Alignment.ToString());
         SetAttr(xTextGeom, "TextTrimming", _Trimming.ToString());

         Geometry.SetValue(GeometryExtProps.CustomGeometryProperty, xCustomGeom.ToString());
      }

      private void SetAttr(XElement xElement, string attrName, string value)
      {
         var xAttr = xElement.Attribute(attrName);
         if (xAttr == null)
         {
            xElement.Add(new XAttribute(attrName, value));
         }
         else
         {
            xAttr.Value = value;
         }
      }

      public override object PropertiesObject
      {
         get { return this; }
      }

      private string _Text;

      [DisplayName("Text")]
      [Description("Text to display")]
      [Category("Text")]
      public string Text
      {
         get { return _Text; }
         set
         {
            _Text = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("Text");
         }
      }

      private Point _Position;

      [DisplayName("Position")]
      [Description("Position of the text")]
      [Category("Position")]
      public Point Position
      {
         get { return _Position; }
         set
         {
            _Position = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("Position");
         }
      }

      private Size _Size;

      [DisplayName("Size")]
      [Description("Size of the text area")]
      [Category("Position")]
      public Size Size
      {
         get { return _Size; }
         set
         {
            _Size = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("Size");
         }
      }

      private double _TextSize;

      [DisplayName("Text size")]
      [Description("Size of the Text")]
      [Category("Font")]
      public double TextSize
      {
         get { return _TextSize; }
         set
         {
            _TextSize = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("TextSize");
         }
      }

      private FontFamily _FontFamily = new FontFamily("Arial");

      [DisplayName("Font family")]
      [Category("Font")]
      public FontFamily FontFamily
      {
         get { return _FontFamily; }
         set
         {
            _FontFamily = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("FontFamily");
         }
      }

      private FontStyle _FontStyle = FontStyles.Normal;

      [DisplayName("Font style")]
      [Category("Font")]
      public FontStyle FontStyle
      {
         get { return _FontStyle; }
         set
         {
            _FontStyle = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("FontStyle");
         }
      }

      private FontWeight _FontWeight = FontWeights.Normal;

      [DisplayName("Font weight")]
      [Category("Font")]
      public FontWeight FontWeight
      {
         get { return _FontWeight; }
         set
         {
            _FontWeight = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("FontWeight");
         }
      }

      private TextAlignment _Alignment = TextAlignment.Left;

      [DisplayName("Text alignment")]
      [Category("Text")]
      public TextAlignment Alignment
      {
         get { return _Alignment; }
         set
         {
            _Alignment = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("Alignment");
         }
      }

      private TextTrimming _Trimming = TextTrimming.None;

      [DisplayName("Text trimming")]
      [Category("Text")]
      public TextTrimming Trimming
      {
         get { return _Trimming; }
         set
         {
            _Trimming = value;
            UpdateGeometry();
            SetModified();
            NotifyPropertyChanged("Trimming");
         }
      }

      protected override Rect CalcOuterBounds()
      {
         return Rect.Union(base.CalcOuterBounds(), CalcVertexBounds());
      }

      protected override Rect CalcVertexBounds()
      {
         return new Rect(Position, Size);
      }

      private void UpdateGeometry()
      {
         var text = Text;
         if (String.IsNullOrEmpty(text))
         {
            text = "?";
         }
         var txt = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal), TextSize, new SolidColorBrush(Colors.Black));
         txt.TextAlignment = Alignment;
         txt.Trimming = Trimming;
         txt.MaxLineCount = Int32.MaxValue;
         txt.MaxTextWidth = Math.Max(2 * TextSize, Size.Width);
         txt.MaxTextHeight = Math.Max(2 * TextSize, Size.Height);

         Geometry = txt.BuildGeometry(Position);

         FrameworkElement.InvalidateMeasure();
         FrameworkElement.InvalidateVisual();

         GeometryChanged();
      }

      [Browsable(false)]
      public Geometry Geometry
      {
         get { return GeometryDrawing == null ? null : GeometryDrawing.Geometry; }
         private set
         {
            if (GeometryDrawing != null)
            {
               GeometryDrawing.Geometry = value;
            }
         }
      }

      private Point _DragStratPosition = new Point();

      public override void OnDragStart()
      {
         base.OnDragStart();
         _DragStratPosition = Position;
      }

      public override void OnDragMove(Vector delta)
      {
         base.OnDragMove(delta);
         Position = _DragStratPosition + delta;
      }

      public override void OnStartResize(ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnStartResize(resizePos, resizeOrigin);
      }

      public override void OnUpdateResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnUpdateResize(newBounds, resizePos, resizeOrigin);
         Position = newBounds.TopLeft;
         Size = newBounds.Size;
      }
   }
}
