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
   /// Interaction logic for GradientStopEditor.xaml
   /// </summary>
   public partial class GradientStopEditor : UserControl
   {
      public GradientStopEditor()
      {
         InitializeComponent();
      }



      public GradientStopCollection GradientStops
      {
         get { return (GradientStopCollection)GetValue(GradientStopsProperty); }
         set { SetValue(GradientStopsProperty, value); }
      }

      // Using a DependencyProperty as the backing store for GradientStops.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty GradientStopsProperty =
          DependencyProperty.Register("GradientStops", typeof(GradientStopCollection), typeof(GradientStopEditor), new UIPropertyMetadata(null));



      public GradientStop SelectedGradientStop
      {
         get { return (GradientStop)GetValue(SelectedGradientStopProperty); }
         set { SetValue(SelectedGradientStopProperty, value); }
      }

      // Using a DependencyProperty as the backing store for SelectedGradientStop.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SelectedGradientStopProperty =
          DependencyProperty.Register("SelectedGradientStop", typeof(GradientStop), typeof(GradientStopEditor), new UIPropertyMetadata(null));

      private void Add_Click(object sender, RoutedEventArgs e)
      {
         if (GradientStops != null)
         {
            var offset = 0.0;
            if (GradientStops.Count == 1)
            {
               offset = 1.0;
            }
            else if (GradientStops.Count > 0)
            {
               offset = GradientStops.Last().Offset + 0.1;
            }
            GradientStops.Add(new GradientStop(Colors.White, offset));
            SelectedGradientStop = GradientStops.Last();
            RefreshGradientStops();
         }
      }

      private void Remove_Click(object sender, RoutedEventArgs e)
      {
         if (GradientStops != null && SelectedGradientStop != null)
         {
            GradientStops.Remove(SelectedGradientStop);
            RefreshGradientStops();
         }
      }

      private void Up_Click(object sender, RoutedEventArgs e)
      {
         if (GradientStops != null && SelectedGradientStop != null && GradientStops.IndexOf(SelectedGradientStop) > 0)
         {
            var gs = SelectedGradientStop;
            int idx = GradientStops.IndexOf(gs);
            GradientStops.RemoveAt(idx);
            GradientStops.Insert(idx - 1, gs);
            SelectedGradientStop = gs;
            RefreshGradientStops();
         }
      }

      private void Down_Click(object sender, RoutedEventArgs e)
      {
         if (GradientStops != null && SelectedGradientStop != null && GradientStops.IndexOf(SelectedGradientStop) < GradientStops.Count - 1)
         {
            var gs = SelectedGradientStop;
            int idx = GradientStops.IndexOf(gs);
            GradientStops.RemoveAt(idx);
            GradientStops.Insert(idx + 1, gs);
            SelectedGradientStop = gs;
            RefreshGradientStops();
         }
      }

      private void RefreshGradientStops()
      {
         _DataGrid.Items.Refresh();
      }
   }
}
