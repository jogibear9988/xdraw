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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XDraw.Controls
{
   /// <summary>
   /// Interaction logic for BrushEditor.xaml
   /// </summary>
   public partial class BrushEditor : UserControl
   {
      public BrushEditor()
      {
         InitializeComponent();
      }

      public Brush Brush
      {
         get { return (Brush)GetValue(BrushProperty); }
         set { SetValue(BrushProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Brush.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty BrushProperty =
          DependencyProperty.Register("Brush", typeof(Brush), typeof(BrushEditor), new UIPropertyMetadata(null));

      private void BrushTypeRadioButton_Click(object sender, RoutedEventArgs e)
      {
         if (EmptyBrushRadioButton.IsChecked == true)
         {
            Brush = null;
         }
         else if (SolidBrushRadioButton.IsChecked == true)
         {
            Brush = new SolidColorBrush(Colors.Red);
         }
         else if (LinearGradientBrushRadioButton.IsChecked == true)
         {
            Brush = new LinearGradientBrush(Colors.Blue, Colors.Purple, 45.0);
         }
         else if (RadialGradientBrushRadioButton.IsChecked == true)
         {
            Brush = new RadialGradientBrush(Colors.Green, Colors.Yellow);
         }
      }
   }
}
