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
   /// Interaction logic for GridOptionsDialog.xaml
   /// </summary>
   public partial class GridOptionsDialog : Window
   {
      public GridOptionsDialog()
      {
         InitializeComponent();
      }

      internal XDrawing Drawing { get; set; }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         chkShowGrid.IsChecked = Drawing.IsGridVisible;
         chkShowPointGrid.IsChecked = Drawing.PointGridMode;
         chkSnapToGrid.IsChecked = Drawing.IsSnapToGridEnabled;

         edtGridWidth.Text = Drawing.DisplayGridWidth.ToString();
         edtSnapGridWidth.Text = Drawing.SnapGridWidth.ToString();
      }

      private void OK_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            Drawing.IsGridVisible = chkShowGrid.IsChecked.Value;
            Drawing.PointGridMode = chkShowPointGrid.IsChecked.Value;
            Drawing.IsSnapToGridEnabled = chkSnapToGrid.IsChecked.Value;

            Drawing.DisplayGridWidth = Double.Parse(edtGridWidth.Text);
            Drawing.SnapGridWidth = Double.Parse(edtSnapGridWidth.Text);

            DialogResult = true;
            Close();
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }
   }
}
