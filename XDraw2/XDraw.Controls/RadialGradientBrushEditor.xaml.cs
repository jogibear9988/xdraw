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
   /// Interaction logic for RadialGradientBrushEditor.xaml
   /// </summary>
   public partial class RadialGradientBrushEditor : UserControl
   {
      public RadialGradientBrushEditor()
      {
         InitializeComponent();
      }

      public RadialGradientBrush Brush
      {
         get { return (RadialGradientBrush)GetValue(BrushProperty); }
         set { SetValue(BrushProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Brush.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty BrushProperty =
          DependencyProperty.Register("Brush", typeof(RadialGradientBrush), typeof(RadialGradientBrushEditor), new UIPropertyMetadata(null));


      public bool LockOrigionToCenter
      {
         get { return (bool)GetValue(LockOrigionToCenterProperty); }
         set { SetValue(LockOrigionToCenterProperty, value); }
      }

      // Using a DependencyProperty as the backing store for LockOrigionToCenter.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty LockOrigionToCenterProperty =
          DependencyProperty.Register("LockOrigionToCenter", typeof(bool), typeof(RadialGradientBrushEditor), new UIPropertyMetadata(true));


      public double OuterSpace
      {
         get { return (double)GetValue(OuterSpaceProperty); }
         set { SetValue(OuterSpaceProperty, value); }
      }

      // Using a DependencyProperty as the backing store for OuterSpace.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty OuterSpaceProperty =
          DependencyProperty.Register("OuterSpace", typeof(double), typeof(RadialGradientBrushEditor), new UIPropertyMetadata(0.5, OuterSpaceChanged));

      private static void OuterSpaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as RadialGradientBrushEditor).OuterSpaceChanged(e);
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
            CenterPoint.SetValue(Canvas.LeftProperty, ix + Brush.Center.X * isz - CenterPoint.Width / 2.0);
            CenterPoint.SetValue(Canvas.TopProperty, iy + Brush.Center.Y * isz - CenterPoint.Height / 2.0);

            RadiusXPoint.SetValue(Canvas.LeftProperty, ix + (Brush.Center.X + Brush.RadiusX) * isz - RadiusXPoint.Width / 2.0);
            RadiusXPoint.SetValue(Canvas.TopProperty, iy + Brush.Center.Y * isz - RadiusXPoint.Height / 2.0);

            RadiusYPoint.SetValue(Canvas.LeftProperty, ix + Brush.Center.X * isz - RadiusYPoint.Width / 2.0);
            RadiusYPoint.SetValue(Canvas.TopProperty, iy + (Brush.Center.Y + Brush.RadiusY) * isz - RadiusYPoint.Height / 2.0);

            OriginXLine.SetValue(Canvas.LeftProperty, ix + Brush.GradientOrigin.X * isz - OriginXLine.Width / 2.0);
            OriginYLine.SetValue(Canvas.TopProperty, iy + Brush.GradientOrigin.Y * isz - OriginYLine.Height / 2.0);

            OriginXY.SetValue(Canvas.LeftProperty, ix + Brush.GradientOrigin.X * isz - OriginXY.Width / 2.0);
            OriginXY.SetValue(Canvas.TopProperty, iy + Brush.GradientOrigin.Y * isz - OriginXY.Height / 2.0);

            RadialEllipse.SetValue(Canvas.LeftProperty, ix + (Brush.Center.X - Brush.RadiusX) * isz);
            RadialEllipse.SetValue(Canvas.TopProperty, iy + (Brush.Center.Y - Brush.RadiusY) * isz);
            RadialEllipse.Width = 2.0 * Brush.RadiusX * isz;
            RadialEllipse.Height = 2.0 * Brush.RadiusY * isz;

            CenterPoint.Visibility = Visibility.Visible;
            RadiusXPoint.Visibility = Visibility.Visible;
            RadiusYPoint.Visibility = Visibility.Visible;
            OriginXLine.Visibility = Visibility.Visible;
            OriginYLine.Visibility = Visibility.Visible;
            OriginXY.Visibility = Visibility.Visible;
            RadialEllipse.Visibility = Visibility.Visible;
         }
         else
         {
            CenterPoint.Visibility = Visibility.Collapsed;
            RadiusXPoint.Visibility = Visibility.Collapsed;
            RadiusYPoint.Visibility = Visibility.Collapsed;
            OriginXLine.Visibility = Visibility.Collapsed;
            OriginYLine.Visibility = Visibility.Collapsed;
            OriginXY.Visibility = Visibility.Collapsed;
            RadialEllipse.Visibility = Visibility.Collapsed;
         }
      }

      private FrameworkElement _DragedElement = null;
      private Vector _DragOffset = new Vector();

      private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         _DragedElement = sender as FrameworkElement;
         if (_DragedElement != null)
         {
            _DragOffset = (Vector)e.GetPosition(_DragedElement) - new Vector(_DragedElement.Width / 2.0, _DragedElement.Height / 2.0);
            _DragedElement.CaptureMouse();
         }
      }

      private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         if (_DragedElement != null && _DragedElement.IsMouseCaptured)
         {
            _DragedElement.ReleaseMouseCapture();
         }
         _DragedElement = null;
      }

      private void Element_MouseMove(object sender, MouseEventArgs e)
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

            if (_DragedElement == CenterPoint)
            {
               if (LockOrigionToCenter)
               {
                  Brush.GradientOrigin = new Point(
                     Brush.GradientOrigin.X + (p.X - Brush.Center.X),
                     Brush.GradientOrigin.Y + (p.Y - Brush.Center.Y));
               }
               Brush.Center = p;
            }
            else if (_DragedElement == OriginXLine)
            {
               Brush.GradientOrigin = new Point(p.X, Brush.GradientOrigin.Y);
            }
            else if (_DragedElement == OriginYLine)
            {
               Brush.GradientOrigin = new Point(Brush.GradientOrigin.X, p.Y);
            }
            else if (_DragedElement == OriginXY)
            {
               Brush.GradientOrigin = p;
            }
            else if (_DragedElement == RadiusXPoint)
            {
               Brush.RadiusX = p.X - Brush.Center.X;
            }
            else if (_DragedElement == RadiusYPoint)
            {
               Brush.RadiusY = p.Y - Brush.Center.Y;
            }
            SetItemsPos();
         }
      }
   }
}
