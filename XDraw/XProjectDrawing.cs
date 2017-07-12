using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;

namespace remes.XDraw
{
   public class XProjectDrawing : DependencyObject
   {
      public XProjectDrawing()
      {

      }

      public XDrawingExportFormat ExportFormat
      {
         get { return (XDrawingExportFormat)GetValue(ExportFormatProperty); }
         set { SetValue(ExportFormatProperty, value); }
      }

      public static readonly DependencyProperty ExportFormatProperty =
          DependencyProperty.Register("ExportFormat", typeof(XDrawingExportFormat), typeof(XProjectDrawing), new UIPropertyMetadata(XDrawingExportFormat.DrawingImage));

      public string DrawingName
      {
         get { return (string)GetValue(DrawingNameProperty); }
         set { SetValue(DrawingNameProperty, value); }
      }

      public static readonly DependencyProperty DrawingNameProperty =
          DependencyProperty.Register("DrawingName", typeof(string), typeof(XProjectDrawing), new UIPropertyMetadata(String.Empty));


      public Size DrawingSize
      {
         get { return (Size)GetValue(DrawingSizeProperty); }
         set { SetValue(DrawingSizeProperty, value); }
      }

      public static readonly DependencyProperty DrawingSizeProperty =
          DependencyProperty.Register("DrawingSize", typeof(Size), typeof(XProjectDrawing), new UIPropertyMetadata(new Size(1000.0, 1000.0)));

      public bool SaveDrawingSize
      {
          get { return (bool)GetValue(SaveDrawingSizeProperty); }
          set { SetValue(SaveDrawingSizeProperty, value); }
      }

      public static readonly DependencyProperty SaveDrawingSizeProperty = 
          DependencyProperty.Register("SaveDrawingSize", typeof(bool), typeof(XProjectDrawing), new UIPropertyMetadata(false));



      private XElement m_XmlCode = null;
      private XDrawing m_Drawing = null;

      public XDrawing Drawing
      {
         get { return m_Drawing; }
      }

      public XDrawing GetDrawing(Canvas canvas)
      {
         if (m_Drawing != null && m_Drawing.Canvas != canvas)
         {
            StoreXmlCode();
            m_Drawing.Canvas.ClearValue(Canvas.WidthProperty);
            m_Drawing.Canvas.ClearValue(Canvas.HeightProperty);
            m_Drawing.Dispose();
            m_Drawing = null;
         }
         if (m_Drawing == null)
         {
            var bw = new Binding("DrawingSize.Width");
            bw.Source = this;
            canvas.SetBinding(Canvas.WidthProperty, bw);
            bw = new Binding("DrawingSize.Height");
            bw.Source = this;
            canvas.SetBinding(Canvas.HeightProperty, bw);

            m_Drawing = new XDrawing(canvas);
            if (m_XmlCode != null)
            {
               m_Drawing.Import(m_XmlCode);
            }
         }
         return m_Drawing;
      }

      public void StoreXmlCode()
      {
         if (m_Drawing != null)
         {
            m_XmlCode = m_Drawing.Export(ExportFormat);
         }
      }

      public XElement XmlCode
      {
         get
         {
            // get lastest xml code
            StoreXmlCode();
            return m_XmlCode;
         }
         set { m_XmlCode = value; }
      }

      internal void Save(XElement xParent)
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         xParent.Add(
            new XElement("Drawing",
               new XAttribute("Name", DrawingName),
               new XElement("ExportFormat", ExportFormat.ToString()),
               new XElement("DrawingSize", DrawingSize.ToString(nfi)),
               new XElement("SaveDrawingSize", SaveDrawingSize.ToString())));
      }

      public void CloseDrawing()
      {
         if (m_Drawing != null)
         {
            StoreXmlCode();
            m_Drawing.Dispose();
            m_Drawing = null;
         }
      }
   }
}
