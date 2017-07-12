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
using System.Xml.Linq;

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for ImportDrawingDialog.xaml
   /// </summary>
   public partial class ImportDrawingDialog : Window
   {
      public static readonly DependencyProperty DrawingProperty = DependencyProperty.Register(
         "Drawing", typeof(XDrawing), typeof(ImportDrawingDialog), new PropertyMetadata(null));

      public ImportDrawingDialog()
      {
         InitializeComponent();
      }

      public XDrawing Drawing
      {
         get { return (XDrawing)GetValue(DrawingProperty); }
         set { SetValue(DrawingProperty, value); }
      }

      private void btnPaste_Click(object sender, RoutedEventArgs e)
      {
         if (xmlCode.SelectionLength == 0)
         {
            xmlCode.SelectAll();
         }
         xmlCode.Paste();
      }

      private void btnImport_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            var xDoc = XDocument.Parse(xmlCode.Text);
            Drawing.Import(xDoc.Root, chkClearDrawing.IsChecked.HasValue && chkClearDrawing.IsChecked.Value);
            Close();
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.ToString());
         }
      }
   }
}
