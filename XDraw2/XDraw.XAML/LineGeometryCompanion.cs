using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

namespace XDraw.Xaml
{
   public class LineGeometryCompanion : GeometryDrawingCompanion
   {
      public LineGeometryCompanion(object item) :
         base(item)
      {
         DependencyPropertyDescriptor.FromProperty(LineGeometry.StartPointProperty, typeof(LineGeometry)).AddValueChanged(Line, OnGeometryChanged);
         DependencyPropertyDescriptor.FromProperty(LineGeometry.EndPointProperty, typeof(LineGeometry)).AddValueChanged(Line, OnGeometryChanged);
      }

      public static object CreateNew(Point position, ItemCreateMode createMode)
      {
         LineGeometry geom;
         if (createMode == ItemCreateMode.Drop)
         {
            geom = new LineGeometry(position, position + new Vector(5.0, 0.0));
         }
         else
         {
            geom = new LineGeometry(position, position);
         }
         return new GeometryDrawing(
            null, new Pen(Brushes.Black, 1.0),
            geom);
      }

      private void OnGeometryChanged(object sender, EventArgs e)
      {
         GeometryChanged();
         NotifyPropertyChanged("StartPoint");
         NotifyPropertyChanged("EndPoint");
      }

      [Browsable(false)]
      public LineGeometry Line
      {
         get { return GeometryDrawing == null ? null : GeometryDrawing.Geometry as LineGeometry; }
      }

      [Category("Layout")]
      public Point StartPoint
      {
         get { return Line.StartPoint; }
         set
         {
            if (value != Line.StartPoint)
            {
               Line.StartPoint = value;
            }
         }
      }

      [Category("Layout")]
      public Point EndPoint
      {
         get { return Line.EndPoint; }
         set
         {
            if (value != Line.EndPoint)
            {
               Line.EndPoint = value;
            }
         }
      }

      private Point _DragStartPoint1 = new Point();
      private Point _DragStartPoint2 = new Point();

      public override void OnDragStart()
      {
         base.OnDragStart();

         if (Line == null)
         {
            return;
         }
         _DragStartPoint1 = Line.StartPoint;
         _DragStartPoint2 = Line.EndPoint;
      }

      public override void OnDragMove(Vector delta)
      {
         base.OnDragMove(delta);

         if (Line == null)
         {
            return;
         }
         Line.StartPoint = _DragStartPoint1 + delta;
         Line.EndPoint = _DragStartPoint2 + delta;

         GeometryChanged();
      }

      public override void OnStartResize(ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnStartResize(resizePos, resizeOrigin);

         if (Line == null)
         {
            return;
         }
         _DragStartPoint1 = Line.StartPoint;
         _DragStartPoint2 = Line.EndPoint;
      }

      public override void OnUpdateResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnUpdateResize(newBounds, resizePos, resizeOrigin);

         if (Line == null)
         {
            return;
         }
         var transform = CalcResizeTransform(newBounds, resizeOrigin);

         Line.StartPoint = transform.Transform(_DragStartPoint1);
         Line.EndPoint = transform.Transform(_DragStartPoint2);

         GeometryChanged();
      }

      public override void OnCreateCustomElements(IList<Gripper> grippers, IList<FrameworkElement> foregroundElements)
      {
         grippers.Add(CreateGripper("StartPoint"));
         grippers.Add(CreateGripper("EndPoint"));
      }

      private Gripper CreateGripper(string bindingPath)
      {
         var gripper = Gripper.CreateCircleGripper();
         
         var binding = new Binding(bindingPath);
         binding.Source = Line;
         binding.Mode = BindingMode.TwoWay;
         BindingOperations.SetBinding(gripper, Gripper.PositionProperty, binding);

         return gripper;
      }

      public override void OnClearCustomElements(IList<Gripper> grippers, IList<FrameworkElement> foregroundElements)
      {
         // nothing to do, because all binding get cleared automatically on grippers
      }
   }
}
