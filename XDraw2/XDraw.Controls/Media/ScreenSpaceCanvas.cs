using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace XDraw.Controls.Media
{
   public class ScreenSpaceCanvas : Canvas, IScreenSpaceScaler
   {
      // Using a DependencyProperty as the backing store for ScaleFactor.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ScaleFactorProperty =
          DependencyProperty.Register("ScaleFactor", typeof(double), typeof(ScreenSpaceCanvas), new UIPropertyMetadata(1.0, OnScaleFactorChanged));

      private static void OnScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var temp = (d as ScreenSpaceCanvas).ScaleFactorChanged;
         if (temp != null)
         {
            temp(d, new EventArgs());
         }
      }

      public void AddElement(FrameworkElement element)
      {
         Children.Add(element);
      }

      public void AddElements(IEnumerable<FrameworkElement> elements)
      {
         foreach (var element in elements)
         {
            Children.Add(element);
         }
      }

      public void RemoveElement(FrameworkElement element)
      {
         Children.Remove(element);
      }

      public void RemoveElements(IEnumerable<FrameworkElement> elements)
      {
         foreach (var element in elements)
         {
            Children.Remove(element);
         }
      }

      #region IScreenSpaceScaler Members

      public double ScaleFactor
      {
         get { return (double)GetValue(ScaleFactorProperty); }
         set { SetValue(ScaleFactorProperty, value); }
      }

      public event EventHandler<EventArgs> ScaleFactorChanged;

      #endregion
   }
}
