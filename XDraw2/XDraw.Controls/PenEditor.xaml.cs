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
   /// Interaction logic for PenEditor.xaml
   /// </summary>
   public partial class PenEditor : UserControl
   {
      public PenEditor()
      {
         InitializeComponent();
      }


      public Pen Pen
      {
         get { return (Pen)GetValue(PenProperty); }
         set { SetValue(PenProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Pen.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty PenProperty =
          DependencyProperty.Register("Pen", typeof(Pen), typeof(PenEditor), new UIPropertyMetadata(null));


      private void ThicknessMajorUpButton_Click(object sender, RoutedEventArgs e)
      {
         if (Pen != null)
         {
            Pen.Thickness = Math.Max(0.0, Pen.Thickness + 1.0);
         }
      }

      private void ThicknessMajorDownButton_Click(object sender, RoutedEventArgs e)
      {
         if (Pen != null)
         {
            Pen.Thickness = Math.Max(0.0, Pen.Thickness - 1.0);
         }
      }

      private void ThicknessMinorUpButton_Click(object sender, RoutedEventArgs e)
      {
         if (Pen != null)
         {
            Pen.Thickness = Math.Max(0.0, Pen.Thickness + 0.1);
         }
      }

      private void ThicknessMinorDownButton_Click(object sender, RoutedEventArgs e)
      {
         if (Pen != null)
         {
            Pen.Thickness = Math.Max(0.0, Pen.Thickness - 0.1);
         }
      }
   }
}
