using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for ExportDrawingDialog.xaml
   /// </summary>
   public partial class ExportDrawingDialog : Window
   {
      public static readonly DependencyProperty DrawingProperty = DependencyProperty.Register(
         "Drawing", typeof(XDrawing), typeof(ExportDrawingDialog), new PropertyMetadata(null, DrawingChanged));

      public ExportDrawingDialog()
      {
         InitializeComponent();

         UpdateXml();
      }

      public XDrawing Drawing
      {
         get { return (XDrawing)GetValue(DrawingProperty); }
         set { SetValue(DrawingProperty, value); }
      }

      private static void DrawingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as ExportDrawingDialog).UpdateXml();
      }

      private void UpdateXml()
      {
         if (Drawing != null)
         {
            XDrawingPath.UseGeometryMiniLanguage = chkUseGeomMiniLang.IsChecked.HasValue && chkUseGeomMiniLang.IsChecked.Value;
            var xElement = Drawing.Export(btnFormatCanvas.IsChecked.HasValue && btnFormatCanvas.IsChecked.Value ? XDrawingExportFormat.Canvas : XDrawingExportFormat.DrawingImage);
            xmlCode.Text = xElement.ToString();
         }
         else
         {
            xmlCode.Text = String.Empty;
         }
      }

      private void btnFormat_Click(object sender, RoutedEventArgs e)
      {
         UpdateXml();
      }

      private void btnCopy_Click(object sender, RoutedEventArgs e)
      {
         if (xmlCode.SelectionLength == 0)
         {
            xmlCode.SelectAll();
         }
         xmlCode.Copy();
      }

      private void chkUseGeomMiniLang_Click(object sender, RoutedEventArgs e)
      {
         UpdateXml();
      }
   }
}
