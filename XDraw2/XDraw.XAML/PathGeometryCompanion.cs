using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;
using XDraw.Controls.Media;

namespace XDraw.Xaml
{
   public class PathGeometryCompanion : GeometryDrawingCompanion
   {
      public PathGeometryCompanion(object item) :
         base(item)
      {
         Path.Changed += OnGeometryChanged;
      }

      public static object CreateNew(Point position, ItemCreateMode createMode)
      {
         PathGeometry geom;
         if (createMode == ItemCreateMode.Drop)
         {
            geom = new PathGeometry(new[] { new PathFigure(position, new[] { new LineSegment(position + new Vector(5.0, 0.0), true), new LineSegment(position + new Vector(5.0, 5.0), true) }, true) });
         }
         else
         {
            geom = new PathGeometry(new[] { new PathFigure(position, new[] { new LineSegment(position + new Vector(5.0, 0.0), true) }, false) });
         }
         return new GeometryDrawing(
            null, new Pen(Brushes.Black, 1.0),
            geom);
      }

      private void OnGeometryChanged(object sender, EventArgs e)
      {
         GeometryChanged();
      }

      [Browsable(false)]
      public PathGeometry Path
      {
         get { return GeometryDrawing == null ? null : GeometryDrawing.Geometry as PathGeometry; }
      }

      private class PointDelegateArgs
      {
         public PointDelegateArgs(Point pt, DependencyObject o, string path)
         {
            Point = pt;
            Object = o;
            Path = path;

            ParentPoint = null;
            PointModified = false;
         }

         public PointDelegateArgs(Point pt, DependencyObject o, string path, PointDelegateArgs parentPoint)
         {
            Point = pt;
            Object = o;
            Path = path;
            ParentPoint = parentPoint;

            PointModified = false;
         }

         public Point Point;
         public DependencyObject Object;
         public string Path;

         public PointDelegateArgs ParentPoint;

         public bool PointModified;

         public Gripper Gripper = null;
      }

      private delegate void PointDelegate(PointDelegateArgs a);

      private void ForEachAbsPoint(PointDelegate ptCallback)
      {
         foreach (var figure in Path.Figures)
         {
            var args = new PointDelegateArgs(figure.StartPoint, figure, "StartPoint");
            ptCallback(args);
            if (args.PointModified)
            {
               figure.StartPoint = args.Point;
            }

            var lastEndPoint = args;

            foreach (var segment in figure.Segments)
            {
               if (segment is ArcSegment)
               {
                  var arc = segment as ArcSegment;
                  args = new PointDelegateArgs(arc.Point, segment, "Point");
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     arc.Point = args.Point;
                  }
                  lastEndPoint = args;
               }
               else if (segment is BezierSegment)
               {
                  var b = segment as BezierSegment;
                  args = new PointDelegateArgs(b.Point1, segment, "Point1", lastEndPoint);
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     b.Point1 = args.Point;
                  }
                  args = new PointDelegateArgs(b.Point3, segment, "Point3");
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     b.Point3 = args.Point;
                  }
                  lastEndPoint = args;
                  args = new PointDelegateArgs(b.Point2, segment, "Point2", lastEndPoint);
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     b.Point2 = args.Point;
                  }
               }
               else if (segment is LineSegment)
               {
                  var l = segment as LineSegment;
                  args = new PointDelegateArgs(l.Point, segment, "Point");
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     l.Point = args.Point;
                  }
                  lastEndPoint = args;
               }
               else if (segment is PolyBezierSegment)
               {
                  var pb = segment as PolyBezierSegment;
                  for (int n = 0; n < pb.Points.Count; n += 3)
                  {
                     args = new PointDelegateArgs(pb.Points[n + 0], segment, String.Format("Points[{0}]", n + 0), lastEndPoint);
                     ptCallback(args);
                     if (args.PointModified)
                     {
                        pb.Points[n + 0] = args.Point;
                     }
                     args = new PointDelegateArgs(pb.Points[n + 2], segment, String.Format("Points[{0}]", n + 2));
                     ptCallback(args);
                     if (args.PointModified)
                     {
                        pb.Points[n + 2] = args.Point;
                     }
                     lastEndPoint = args;
                     args = new PointDelegateArgs(pb.Points[n + 1], segment, String.Format("Points[{0}]", n + 1), lastEndPoint);
                     ptCallback(args);
                     if (args.PointModified)
                     {
                        pb.Points[n + 1] = args.Point;
                     }
                  }
               }
               else if (segment is PolyLineSegment)
               {
                  var pl = segment as PolyLineSegment;
                  for (int n = 0; n < pl.Points.Count; ++n)
                  {
                     args = new PointDelegateArgs(pl.Points[n], segment, String.Format("Points[{0}]", n));
                     ptCallback(args);
                     if (args.PointModified)
                     {
                        pl.Points[n ] = args.Point;
                     }
                     lastEndPoint = args;
                  }
               }
               else if (segment is PolyQuadraticBezierSegment)
               {
                  var pqb = segment as PolyQuadraticBezierSegment;
                  for (int n = 0; n < pqb.Points.Count; ++n)
                  {
                     args = new PointDelegateArgs(pqb.Points[n + 1], segment, String.Format("Points[{0}]", n + 1));
                     ptCallback(args);
                     if (args.PointModified)
                     {
                        pqb.Points[n + 1] = args.Point;
                     }
                     lastEndPoint = args;
                     args = new PointDelegateArgs(pqb.Points[n + 0], segment, String.Format("Points[{0}]", n + 0), lastEndPoint);
                     ptCallback(args);
                     if (args.PointModified)
                     {
                        pqb.Points[n + 0] = args.Point;
                     }
                  }
               }
               else if (segment is QuadraticBezierSegment)
               {
                  var qb = segment as QuadraticBezierSegment;
                  args = new PointDelegateArgs(qb.Point2, segment, "Point2");
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     qb.Point2 = args.Point;
                  }
                  lastEndPoint = args;
                  args = new PointDelegateArgs(qb.Point1, segment, "Point1", lastEndPoint);
                  ptCallback(args);
                  if (args.PointModified)
                  {
                     qb.Point1 = args.Point;
                  }
               }
            }
         }
      }

      private List<Point> _DragStartPoints = null;

      public override void OnDragStart()
      {
         base.OnDragStart();

         if (Path == null)
         {
            return;
         }

         _DragStartPoints = new List<Point>();

         ForEachAbsPoint(args => _DragStartPoints.Add(args.Point) );
      }

      public override void OnDragMove(Vector delta)
      {
         base.OnDragMove(delta);

         if (Path == null || _DragStartPoints == null)
         {
            return;
         }

         int n = 0;
         ForEachAbsPoint(args =>
         {
            if (n < _DragStartPoints.Count)
            {
               args.Point = _DragStartPoints[n++] + delta;
               args.PointModified = true;
            }
         });

         GeometryChanged();
      }

      public override void OnStartResize(ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnStartResize(resizePos, resizeOrigin);

         if (Path == null)
         {
            return;
         }
         _DragStartPoints = new List<Point>();

         ForEachAbsPoint(args => _DragStartPoints.Add(args.Point) );
      }

      public override void OnUpdateResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         base.OnUpdateResize(newBounds, resizePos, resizeOrigin);

         if (Path == null || _DragStartPoints == null)
         {
            return;
         }
         var transform = CalcResizeTransform(newBounds, resizeOrigin);

         int n = 0;
         ForEachAbsPoint(args =>
         {
            if (n < _DragStartPoints.Count)
            {
               args.Point = transform.Transform(_DragStartPoints[n++]);
               args.PointModified = true;
            }
         });

         GeometryChanged();
      }

      private class PointRalation : IDisposable
      {
         public PointRalation(PointDelegateArgs point, PointDelegateArgs parentPoint)
         {
            Line = new ScreenSpaceLine2()
            {
               Stroke = Brushes.Blue,
               StrokeDashArray = new DoubleCollection(new[] { 5.0, 3.0 }),
               StrokeThickness = 1.0
            };

            var binding = new Binding(parentPoint.Path);
            binding.Source = parentPoint.Object;
            binding.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(Line, ScreenSpaceLine2.StartPointProperty, binding);
            DependencyPropertyDescriptor.FromProperty(ScreenSpaceLine2.StartPointProperty, typeof(ScreenSpaceLine2)).AddValueChanged(Line, OnParentPointChanged);
            parentPoint.Gripper.DragStart += ParentGripper_DragStart;
            parentPoint.Gripper.DragEnd += ParentGripper_DragEnd;

            binding = new Binding(point.Path);
            binding.Source = point.Object;
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(Line, ScreenSpaceLine2.EndPointProperty, binding);
            DependencyPropertyDescriptor.FromProperty(ScreenSpaceLine2.EndPointProperty, typeof(ScreenSpaceLine2)).AddValueChanged(Line, OnPointChanged);

            _Delta = Line.EndPoint - Line.StartPoint;
         }

         private void ParentGripper_DragStart(object sender, EventArgs e)
         {
            _ParentPointIsDragged = true;
         }

         private void ParentGripper_DragEnd(object sender, EventArgs e)
         {
            _ParentPointIsDragged = false;
         }

         private Vector _Delta;

         private bool _ParentPointIsDragged = false;

         private void OnParentPointChanged(object sender, EventArgs e)
         {
            if (_ParentPointIsDragged)
            {
               Line.EndPoint = Line.StartPoint + _Delta;
            }
            else
            {
               _Delta = Line.EndPoint - Line.StartPoint;
            }
         }

         private void OnPointChanged(object sender, EventArgs e)
         {
            _Delta = Line.EndPoint - Line.StartPoint;
         }

         public ScreenSpaceLine2 Line = null;

         #region IDisposable Members

         ~PointRalation()
         {
            Dispose(false);
         }

         public void Dispose()
         {
            Dispose(true);
         }

         private void Dispose(bool disposing)
         {
            if (disposing)
            {
               if (Line != null)
               {
                  BindingOperations.ClearAllBindings(Line);
                  Line = null;
               }
            }
         }

         #endregion
      }

      private List<PointRalation> _PointRalations = new List<PointRalation>();

      public override void OnCreateCustomElements(IList<Gripper> grippers, IList<FrameworkElement> foregroundElements)
      {
         ForEachAbsPoint(args =>
         {
            if (args.Object != null && args.Path != null)
            {
               args.Gripper = Gripper.CreateCircleGripper();

               var binding = new Binding(args.Path);
               binding.Source = args.Object;
               binding.Mode = BindingMode.TwoWay;
               BindingOperations.SetBinding(args.Gripper, Gripper.PositionProperty, binding);

               grippers.Add(args.Gripper);

               if (args.ParentPoint != null)
               {
                  var pr = new PointRalation(args, args.ParentPoint);
                  _PointRalations.Add(pr);
                  foregroundElements.Add(pr.Line);
               }
            }
         });
      }

      public override void OnClearCustomElements(IList<Gripper> grippers, IList<FrameworkElement> foregroundElements)
      {
         foreach (var pr in _PointRalations)
         {
            pr.Dispose();
         }
         _PointRalations.Clear();
      }
   }
}
