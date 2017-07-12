using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace XDraw.Controls.Media
{
   public class ScreenSpaceGrid : Grid, IScreenSpaceScaler
   {
      // Using a DependencyProperty as the backing store for ScaleFactor.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ScaleFactorProperty =
          DependencyProperty.Register("ScaleFactor", typeof(double), typeof(ScreenSpaceGrid), new UIPropertyMetadata(1.0, OnScaleFactorChanged));

      private static void OnScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var temp = (d as ScreenSpaceGrid).ScaleFactorChanged;
         if (temp != null)
         {
            temp(d, new EventArgs());
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
