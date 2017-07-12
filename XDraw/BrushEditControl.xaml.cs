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
using Microsoft.Samples.CustomControls;
using System.ComponentModel;

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for BrushEditControl.xaml
   /// </summary>
   public partial class BrushEditControl : UserControl
   {
      public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
         "Text", typeof(string), typeof(BrushEditControl), new PropertyMetadata(""));

      public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
         "Brush", typeof(Brush), typeof(BrushEditControl), new PropertyMetadata(BrushChanged));

      public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
         "IsExpanded", typeof(bool), typeof(BrushEditControl), new PropertyMetadata(true, IsExpandedChanged));

      public BrushEditControl()
      {
         InitializeComponent();
         expander.IsExpanded = IsExpanded;
         DependencyPropertyDescriptor.FromProperty(Expander.IsExpandedProperty, typeof(Expander)).AddValueChanged(expander, ExpanderIsExpandedChanged);
      }

      public string Text
      {
         get { return (string)GetValue(TextProperty); }
         set { SetValue(TextProperty, value); }
      }

      public Brush Brush
      {
         get { return (Brush)GetValue(BrushProperty); }
         set { SetValue(BrushProperty, value); }
      }

      private static void BrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var ctrl = d as BrushEditControl;
         if (ctrl != null)
         {
            ctrl.rdoEmptyBrush.IsChecked = ctrl.Brush == null;
            ctrl.rdoSolidColorBrush.IsChecked = ctrl.Brush is SolidColorBrush;
            ctrl.rdoLinearGradientBrush.IsChecked = ctrl.Brush is LinearGradientBrush;
            ctrl.rdoRadialGradientBrush.IsChecked = ctrl.Brush is RadialGradientBrush;
         }
      }

      public bool IsExpanded
      {
         get { return (bool)GetValue(IsExpandedProperty); }
         set
         {
            if (IsExpanded != value)
            {
               SetValue(IsExpandedProperty, value);
            }
         }
      }

      private static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         if((d as BrushEditControl).expander.IsExpanded != (bool)e.NewValue)
         {
         (d as BrushEditControl).expander.IsExpanded = (bool)e.NewValue;
         }
      }

      private void ExpanderIsExpandedChanged(object sender, EventArgs e)
      {
         IsExpanded = expander.IsExpanded;
      }

      private void BrushType_Click(object sender, RoutedEventArgs e)
      {
         var offsets = GetOffsets();
         var colors = GetColors();
         if (e.OriginalSource == rdoEmptyBrush)
         {
            Brush = null;
         }
         else if (e.OriginalSource == rdoSolidColorBrush)
         {
            if(!(Brush is SolidColorBrush))
            {
               Brush = new SolidColorBrush(colors[0]);
            }
         }
         else if (e.OriginalSource == rdoLinearGradientBrush)
         {
            if (!(Brush is LinearGradientBrush))
            {
               Brush = new LinearGradientBrush();
            }
         }
         else if (e.OriginalSource == rdoRadialGradientBrush)
         {
            if (!(Brush is RadialGradientBrush))
            {
               Brush = new RadialGradientBrush();
            }
         }

         var gradientBrush = Brush as GradientBrush;
         if (gradientBrush != null && gradientBrush.GradientStops.Count == 0)
         {
            for (int n = 0; n < offsets.Length && n < colors.Length; ++n)
            {
               gradientBrush.GradientStops.Add(new GradientStop(colors[n], offsets[n]));
            }
         }
      }

      private Color[] GetColors()
      {
         if (Brush is SolidColorBrush)
         {
            return new Color[] { (Brush as SolidColorBrush).Color, Colors.White };
         }
         else if (Brush is GradientBrush)
         {
            var colors = new List<Color>();
            foreach (var stop in (Brush as GradientBrush).GradientStops)
            {
               colors.Add(stop.Color);
            }
            return colors.ToArray();
         }
         return new Color[] { Colors.Black, Colors.White };
      }

      private double[] GetOffsets()
      {
         if (Brush is GradientBrush)
         {
            var stops = new List<double>();
            foreach (var stop in (Brush as GradientBrush).GradientStops)
            {
               stops.Add(stop.Offset);
            }
            return stops.ToArray();
         }
         return new double[] { 0.0, 1.0 };
      }

      private Window GetOwnerWindow()
      {
         DependencyObject owner = this;
         while (owner != null && !(owner is Window))
         {
            owner = VisualTreeHelper.GetParent(owner);
         }
         return owner as Window;
      }

      private void SelectSolidColor_Click(object sender, RoutedEventArgs e)
      {
         var dlg = new ColorPickerDialog();
         dlg.Owner = GetOwnerWindow();
         dlg.StartingColor = (Brush as SolidColorBrush).Color;
         var result = dlg.ShowDialog();
         if (result.HasValue && result.Value)
         {
            Brush = new SolidColorBrush(dlg.SelectedColor);
         }
      }

      private void btnAddGradientStop_Click(object sender, RoutedEventArgs e)
      {
         var gBrush = Brush as GradientBrush;
         if (gBrush != null)
         {
            gBrush.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
            Brush = null;
            Brush = gBrush;
         }
      }

      private void btnRemoveGradientStop_Click(object sender, RoutedEventArgs e)
      {
         var gBrush = Brush as GradientBrush;
         if (gBrush != null)
         {
            if (gBrush.GradientStops.Count > 2)
            {
               gBrush.GradientStops.RemoveAt(gBrush.GradientStops.Count - 1);
               Brush = null;
               Brush = gBrush;
            }
         }
      }

      private void SelectGradientStopColor_Click(object sender, RoutedEventArgs e)
      {
         var gStop = (e.OriginalSource as Button).Tag as GradientStop;
         if (gStop != null)
         {
            var dlg = new ColorPickerDialog();
            dlg.Owner = GetOwnerWindow();
            dlg.StartingColor = gStop.Color;
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
            {
               gStop.Color = dlg.SelectedColor;
            }
         }
      }
   }
}
