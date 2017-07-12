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
   /// Interaction logic for LinearGradientBrushEditor.xaml
   /// </summary>
   public partial class LinearGradientBrushEditor : UserControl
   {
      public LinearGradientBrushEditor()
      {
         InitializeComponent();
      }

      public LinearGradientBrush Brush
      {
         get { return (LinearGradientBrush)GetValue(BrushProperty); }
         set { SetValue(BrushProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Brush.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty BrushProperty =
          DependencyProperty.Register("Brush", typeof(LinearGradientBrush), typeof(LinearGradientBrushEditor), new UIPropertyMetadata(null));

      public double OuterSpace
      {
         get { return (double)GetValue(OuterSpaceProperty); }
         set { SetValue(OuterSpaceProperty, value); }
      }

      // Using a DependencyProperty as the backing store for OuterSpace.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty OuterSpaceProperty =
          DependencyProperty.Register("OuterSpace", typeof(double), typeof(LinearGradientBrushEditor), new UIPropertyMetadata(0.5, OuterSpaceChanged));

      private static void OuterSpaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as LinearGradientBrushEditor).OuterSpaceChanged(e);
      }

      private void OuterSpaceChanged(DependencyPropertyChangedEventArgs e)
      {
         SetItemsPos();
      }

      private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         SetItemsPos();
      }

      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         SetItemsPos();
      }

      private void CalcCoos(out double x, out double y, out double sz, out double ix, out double iy, out double isz)
      {
         sz = Math.Min(Canvas.ActualWidth, Canvas.ActualHeight);
         x = (Canvas.ActualWidth - sz) / 2.0;
         y = (Canvas.ActualHeight - sz) / 2.0;

         var outerSpace = Math.Max(0.0, Math.Min(OuterSpace, 2.0));

         isz = sz / (1 + 2 * outerSpace);
         ix = x + outerSpace * isz;
         iy = y + outerSpace * isz;
      }

      private void SetItemsPos()
      {
         double sz;
         double x;
         double y;
         double isz;
         double ix;
         double iy;
         CalcCoos(out x, out y, out sz, out ix, out iy, out isz);

         OuterBorder.SetValue(Canvas.LeftProperty, x);
         OuterBorder.SetValue(Canvas.TopProperty, y);
         OuterBorder.Width = sz;
         OuterBorder.Height = sz;

         InnerBorder.SetValue(Canvas.LeftProperty, ix);
         InnerBorder.SetValue(Canvas.TopProperty, iy);
         InnerBorder.Width = isz;
         InnerBorder.Height = isz;

         if (Brush != null)
         {
            StartPoint.SetValue(Canvas.LeftProperty, ix + Brush.StartPoint.X * isz - StartPoint.Width / 2.0);
            StartPoint.SetValue(Canvas.TopProperty, iy + Brush.StartPoint.Y * isz - StartPoint.Height / 2.0);

            EndPoint.SetValue(Canvas.LeftProperty, ix + Brush.EndPoint.X * isz - EndPoint.Width / 2.0);
            EndPoint.SetValue(Canvas.TopProperty, iy + Brush.EndPoint.Y * isz - EndPoint.Height / 2.0);

            StartEndLine.X1 = ix + Brush.StartPoint.X * isz;
            StartEndLine.Y1 = iy + Brush.StartPoint.Y * isz;
            StartEndLine.X2 = ix + Brush.EndPoint.X * isz;
            StartEndLine.Y2 = iy + Brush.EndPoint.Y * isz;

            StartPoint.Visibility = Visibility.Visible;
            EndPoint.Visibility = Visibility.Visible;
            StartEndLine.Visibility = Visibility.Visible;
         }
         else
         {
            StartPoint.Visibility = Visibility.Collapsed;
            EndPoint.Visibility = Visibility.Collapsed;
            StartEndLine.Visibility = Visibility.Collapsed;
         }
      }

      private FrameworkElement _DragedElement = null;
      private Vector _DragOffset = new Vector();

      private void Point_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         _DragedElement = sender as FrameworkElement;
         if (_DragedElement != null)
         {
            _DragOffset = (Vector)e.GetPosition(_DragedElement) - new Vector(_DragedElement.Width / 2.0, _DragedElement.Height / 2.0);
            _DragedElement.CaptureMouse();
         }
      }

      private void Point_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         if (_DragedElement != null && _DragedElement.IsMouseCaptured)
         {
            _DragedElement.ReleaseMouseCapture();
         }
         _DragedElement = null;
      }

      private void Point_MouseMove(object sender, MouseEventArgs e)
      {
         if (_DragedElement != null)
         {
            var p = e.GetPosition(Canvas) - _DragOffset;

            double sz;
            double x;
            double y;
            double isz;
            double ix;
            double iy;
            CalcCoos(out x, out y, out sz, out ix, out iy, out isz);

            p = (Point)((Vector)(p - new Vector(ix, iy)) / isz);

            if (_DragedElement == StartPoint)
            {
               Brush.StartPoint = p;
            }
            else if (_DragedElement == EndPoint)
            {
               Brush.EndPoint = p;
            }
            SetItemsPos();
         }
      }
   }
}
