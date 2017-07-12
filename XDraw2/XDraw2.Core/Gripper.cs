using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using XDraw.Controls.Media;
using System.Windows.Input;

namespace XDraw
{
   public class Gripper : FrameworkElement
   {
      public static Gripper CreateCircleGripper()
      {
         return new Gripper() 
         {
            Shape = GripperShape.Circle, 
            Color = Colors.Black,
            FillBrush = new RadialGradientBrush(
               new GradientStopCollection(new[]
                  {
                     new GradientStop(Colors.LightBlue, 1.0),
                     new GradientStop(Colors.LightBlue, 1.0 - 1.0 / 20.0),
                     new GradientStop(Color.FromArgb(0x01, 0xff, 0xff, 0xff), 1.0 - 1.0 / 20.0),
                     new GradientStop(Color.FromArgb(0x01, 0xff, 0xff, 0xff), 0.0)
                  }))
            {
               RadiusX = 0.5,
               RadiusY = 0.5
            },
            Size = 10.0 
         };
      }

      public static Gripper CreateSquareGripper()
      {
         return new Gripper()
         {
            Shape = GripperShape.Square,
            Color = Colors.Black,
            FillBrush = new RadialGradientBrush(Colors.White, Colors.LightBlue)
            {
               RadiusX = 0.5,
               RadiusY = 0.5
            },
            Size = 6.0
         };
      }

      public Gripper() :
         base()
      {
         IsSelectable = false;
      }

      public bool IsSelectable { get; set; }

      public bool IsSelected
      {
         get { return (bool)GetValue(IsSelectedProperty); }
         set { SetValue(IsSelectedProperty, value); }
      }

      // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected", typeof(bool), typeof(Gripper),
          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetPen));

      public GripperShape Shape
      {
         get { return (GripperShape)GetValue(ShapeProperty); }
         set { SetValue(ShapeProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Shape.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ShapeProperty =
          DependencyProperty.Register("Shape", typeof(GripperShape), typeof(Gripper),
          new FrameworkPropertyMetadata(GripperShape.Circle, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));

      public Color Color
      {
         get { return (Color)GetValue(ColorProperty); }
         set { SetValue(ColorProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ColorProperty =
          DependencyProperty.Register("Color", typeof(Color), typeof(Gripper),
          new FrameworkPropertyMetadata(Colors.LightBlue, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetPen));

      public Brush FillBrush
      {
         get { return (Brush)GetValue(FillBrushProperty); }
         set { SetValue(FillBrushProperty, value); }
      }

      // Using a DependencyProperty as the backing store for FillBrush.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FillBrushProperty =
          DependencyProperty.Register("FillBrush", typeof(Brush), typeof(Gripper), new UIPropertyMetadata(Brushes.Transparent));

      public Point Position
      {
         get { return (Point)GetValue(PositionProperty); }
         set { SetValue(PositionProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty PositionProperty =
          DependencyProperty.Register("Position", typeof(Point), typeof(Gripper), 
          new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));

      public double Size
      {
         get { return (double)GetValue(SizeProperty); }
         set { SetValue(SizeProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SizeProperty =
          DependencyProperty.Register("Size", typeof(double), typeof(Gripper),
          new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, ResetGeometry));

      private static void ResetGeometry(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as Gripper).ResetGeometry(e);
      }

      private void ResetGeometry(DependencyPropertyChangedEventArgs e)
      {
         _Geometry = null;
      }

      private static void ResetPen(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as Gripper).ResetPen(e);
      }

      private void ResetPen(DependencyPropertyChangedEventArgs e)
      {
         _Pen = null;
      }

      private Geometry _Geometry = null;

      protected override Size ArrangeOverride(Size finalSize)
      {
         return finalSize;
      }

      internal virtual Size GetNaturalSize()
      {
         EnsureGeometryCreated();
         Geometry definingGeometry = _Geometry;
         Pen pen = GetPen();
         DashStyle dashStyle = null;
         if (pen != null)
         {
            dashStyle = pen.DashStyle;
            if (dashStyle != null)
            {
               pen.DashStyle = null;
            }
         }
         Rect renderBounds = definingGeometry.GetRenderBounds(pen);
         if (dashStyle != null)
         {
            pen.DashStyle = dashStyle;
         }
         return new Size(Math.Max(renderBounds.Right, 0.0), Math.Max(renderBounds.Bottom, 0.0));
      }

      private Pen _Pen = null;

      private Pen GetPen()
      {
         if (_Pen == null)
         {
            var scaleFactor = GetScaleFactor();
            _Pen = new Pen(new SolidColorBrush(Color), IsSelected ? (2.0 / scaleFactor) : (1.0 / scaleFactor));
         }
         return _Pen;
      }


      private void EnsureGeometryCreated()
      {
         if (_Geometry == null)
         {
            var scaleFactor = GetScaleFactor();
            var halfSize = Size / 2.0 / scaleFactor;
            if (Shape == GripperShape.Circle)
            {
               _Geometry = new EllipseGeometry(Position, halfSize, halfSize);
            }
            else
            {
               _Geometry = new RectangleGeometry(new Rect(Position - new Vector(halfSize, halfSize), new Size(Size / scaleFactor, Size / scaleFactor)));
            }
         }
      }

      protected override Size MeasureOverride(Size constraint)
      {
         return GetNaturalSize();
      }

      protected override void OnRender(DrawingContext drawingContext)
      {
         EnsureGeometryCreated();
         if (_Geometry != Geometry.Empty)
         {
            drawingContext.DrawGeometry(FillBrush, GetPen(), _Geometry);
         }
      }

      private void ScreenSpaceScaler_ScaleFactorChanged(object sender, EventArgs e)
      {
         _Pen = null;
         _Geometry = null;
         InvalidateVisual();
         InvalidateMeasure();
      }

      private IScreenSpaceScaler _ScreenSpaceScaler = null;

      private IScreenSpaceScaler GetScreenSpaceScaler()
      {
         var p = VisualTreeHelper.GetParent(this);
         while (p != null && !(p is IScreenSpaceScaler))
         {
            p = VisualTreeHelper.GetParent(p);
         }
         return p as IScreenSpaceScaler;
      }

      internal double GetScaleFactor()
      {
         if (_ScreenSpaceScaler == null)
         {
            _ScreenSpaceScaler = GetScreenSpaceScaler();
            if (_ScreenSpaceScaler != null)
            {
               _ScreenSpaceScaler.ScaleFactorChanged += ScreenSpaceScaler_ScaleFactorChanged;
            }
         }
         double screenScaleFactor = 1.0;
         if (_ScreenSpaceScaler != null && _ScreenSpaceScaler.ScaleFactor > 0.0)
         {
            screenScaleFactor = _ScreenSpaceScaler.ScaleFactor;
         }
         return screenScaleFactor;
      }

      public bool IsHit(Point point)
      {
         if (_Geometry != null && _Geometry.Bounds.Contains(point))
         {
            if (Shape == GripperShape.Circle)
            {
               var radius = _Geometry.Bounds.Width / 2.0;
               return (point - Position).LengthSquared <= (radius * radius);
            }
            else
            {
               return true;
            }
         }
         return false;
      }

      private Point _DragStartPosition = new Point();

      public virtual void OnDragStart()
      {
         _DragStartPosition = Position;
         var temp = DragStart;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      public virtual void OnDragEnd(Vector delta)
      {
         var temp = DragEnd;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      public virtual void OnDragMove(Vector delta)
      {
         Position = _DragStartPosition + delta;
         var temp = DragMove;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      public event EventHandler<EventArgs> DragStart;

      public event EventHandler<EventArgs> DragMove;
      
      public event EventHandler<EventArgs> DragEnd;
   }

   public enum GripperShape
   {
      Circle,
      Square
   }
}
