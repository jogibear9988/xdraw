using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace XDraw.Editor
{
   partial class XDrawEditor
   {
      private void HandleDragItemsStart()
      {
         _DragAction = DragAction.Items;
         foreach (var companion in _SelectedCompanions)
         {
            companion.OnDragStart();
         }
      }

      private void HandleDragItemsUpdate(Point mousePoint)
      {
         foreach (var companion in _SelectedCompanions)
         {
            companion.OnDragMove(mousePoint - _MouseDownPoint);
         }
      }

      private void HandleDragItemsEnd(Point mousePoint)
      {
         foreach (var companion in _SelectedCompanions)
         {
            companion.OnDragEnd(mousePoint - _MouseDownPoint);
         }
      }

      private void HandleGripperDragStart()
      {
         _ResizeStartBounds = CalcVertexSelectionBounds();
         var resizePos = (ResizePosition)_MouseDownGripper.Tag;
         var resizeOrigin = GetResizeOrigin(_ResizeStartBounds, resizePos);
         foreach (var companion in _SelectedCompanions)
         {
            companion.OnStartResize(resizePos, resizeOrigin);
         }
      }

      private void HandleGripperDragUpdate(Point mousePoint)
      {
         var resizePos = (ResizePosition)_MouseDownGripper.Tag;
         var resizeOrigin = GetResizeOrigin(_ResizeStartBounds, resizePos);
         var newSelectionBounds = CalcNewResizeBounds(_ResizeStartBounds, resizePos, mousePoint - _MouseDownGripperPoint);
         if (_SelectedCompanions.Count == 1)
         {
            _SelectedCompanions[0].OnUpdateResize(newSelectionBounds, resizePos, resizeOrigin);
         }
         else
         {
            foreach (var companion in _SelectedCompanions)
            {
               companion.OnUpdateResize(companion.CalcNewCompanionBounds(_ResizeStartBounds, newSelectionBounds), resizePos, resizeOrigin);
            }
         }
      }

      private void HandleGripperDragEnd(Point mousePoint)
      {
         var resizePos = (ResizePosition)_MouseDownGripper.Tag;
         var resizeOrigin = GetResizeOrigin(_ResizeStartBounds, resizePos);
         var newSelectionBounds = CalcNewResizeBounds(_ResizeStartBounds, resizePos, mousePoint - _MouseDownGripperPoint);
         if (_SelectedCompanions.Count == 1)
         {
            _SelectedCompanions[0].OnUpdateResize(newSelectionBounds, resizePos, resizeOrigin);
         }
         else
         {
            foreach (var companion in _SelectedCompanions)
            {
               companion.OnFinishResize(companion.CalcNewCompanionBounds(_ResizeStartBounds, newSelectionBounds), resizePos, resizeOrigin);
            }
         }
      }

      private Point GetResizeOrigin(Rect bounds, ResizePosition resizePos)
      {
         switch (resizePos)
         {
            case ResizePosition.TopLeft:
               return bounds.BottomRight;

            case ResizePosition.Top:
               return new Point(bounds.Left + bounds.Width / 2.0, bounds.Bottom);

            case ResizePosition.TopRight:
               return bounds.BottomLeft;

            case ResizePosition.Left:
               return new Point(bounds.Right, bounds.Top + bounds.Height / 2.0);

            case ResizePosition.Right:
               return new Point(bounds.Left, bounds.Top + bounds.Height / 2.0);

            case ResizePosition.BottomLeft:
               return bounds.TopRight;

            case ResizePosition.Bottom:
               return new Point(bounds.Left + bounds.Width / 2.0, bounds.Top);

            case ResizePosition.BottomRight:
               return bounds.TopLeft;
         }
         throw new NotImplementedException("The desired ResizePos is not implemented in GetResizeOrigin");
      }

      private Rect CalcNewResizeBounds(Rect oldBounds, ResizePosition resizePos, Vector delta)
      {
         switch (resizePos)
         {
            case ResizePosition.TopLeft:
               if (delta.X > oldBounds.Width) delta.X = oldBounds.Width;
               if (delta.Y > oldBounds.Height) delta.Y = oldBounds.Height;
               return new Rect(oldBounds.TopLeft + delta, oldBounds.BottomRight);

            case ResizePosition.Top:
               delta.X = 0.0;
               if (delta.Y > oldBounds.Height) delta.Y = oldBounds.Height;
               return new Rect(oldBounds.TopLeft + delta, oldBounds.BottomRight);

            case ResizePosition.TopRight:
               if (delta.X < -oldBounds.Width) delta.X = -oldBounds.Width;
               if (delta.Y > oldBounds.Height) delta.Y = oldBounds.Height;
               return new Rect(oldBounds.TopRight + delta, oldBounds.BottomLeft);

            case ResizePosition.Left:
               if (delta.X > oldBounds.Width) delta.X = oldBounds.Width;
               delta.Y = 0.0;
               return new Rect(oldBounds.TopLeft + delta, oldBounds.BottomRight);

            case ResizePosition.Right:
               if (delta.X < -oldBounds.Width) delta.X = -oldBounds.Width;
               delta.Y = 0.0;
               return new Rect(oldBounds.TopRight + delta, oldBounds.BottomLeft);

            case ResizePosition.BottomLeft:
               if (delta.X > oldBounds.Width) delta.X = oldBounds.Width;
               if (delta.Y < -oldBounds.Height) delta.Y = -oldBounds.Height;
               return new Rect(oldBounds.BottomLeft + delta, oldBounds.TopRight);

            case ResizePosition.Bottom:
               delta.X = 0.0;
               if (delta.Y < -oldBounds.Height) delta.Y = -oldBounds.Height;
               return new Rect(oldBounds.BottomLeft + delta, oldBounds.TopRight);

            case ResizePosition.BottomRight:
               if (delta.X < -oldBounds.Width) delta.X = -oldBounds.Width;
               if (delta.Y < -oldBounds.Height) delta.Y = -oldBounds.Height;
               return new Rect(oldBounds.BottomRight + delta, oldBounds.TopLeft);
         }
         return oldBounds;
      }

      private void HandleAutoScroll(Point point)
      {
         if (point.X < 0.0)
         {
            _AutoScrollX = point.X;
         }
         else if (point.X > scrollViewer.ViewportWidth)
         {
            _AutoScrollX = point.X - scrollViewer.ViewportWidth;
         }
         else
         {
            _AutoScrollX = 0.0;
         }
         if (point.Y < 0.0)
         {
            _AutoScrollY = point.Y;
         }
         else if (point.Y > scrollViewer.ViewportHeight)
         {
            _AutoScrollY = point.Y - scrollViewer.ViewportHeight;
         }
         else
         {
            _AutoScrollY = 0.0;
         }

         if (_AutoScrollX != 0.0 || _AutoScrollY != 0.0)
         {
            lock (_ScrollTimerLock)
            {
               if (_ScrollTimer == null)
               {
                  _ScrollTimer = new Timer(_ =>
                  {
                     if (_AutoScrollX == 0.0 && _AutoScrollY == 0.0)
                     {
                        lock (_ScrollTimerLock)
                        {
                           _ScrollTimer.Dispose();
                           _ScrollTimer = null;
                        }
                     }
                     else
                     {
                        Dispatcher.BeginInvoke(new NoArgDelegate(() =>
                        {
                           scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (_AutoScrollX / 3.0));
                           scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + (_AutoScrollY / 3.0));
                        }), null);
                     }
                  }, null, 20, 20);
               }
            }
         }
      }

      private ObservableCollection<Gripper> _CustomGrippers = new ObservableCollection<Gripper>();
      
      private ObservableCollection<FrameworkElement> _ForegroundElements = new ObservableCollection<FrameworkElement>();

      public IList<Gripper> CustomGrippers
      {
         get { return _CustomGrippers; }
      }

      private Companion _CustomGripperCompanion = null;

      private void UpdateCustomGrippers()
      {
         if (_CustomGripperCompanion != null)
         {
            _CustomGripperCompanion.OnClearCustomElements(_CustomGrippers, _ForegroundElements);
            _CustomGripperCompanion = null;
         }

         foreach (var gripper in _CustomGrippers)
         {
            BindingOperations.ClearAllBindings(gripper);
         }
         _CustomGrippers.Clear();
         foreach (var foregroundElement in _ForegroundElements)
         {
            RemoveForgroundElement(foregroundElement);
         }
         _ForegroundElements.Clear();

         if (SelectedCompanions.Count == 1)
         {
            _CustomGripperCompanion = SelectedCompanions[0];

            _CustomGripperCompanion.OnCreateCustomElements(_CustomGrippers, _ForegroundElements);
            foreach (var foregroundElement in _ForegroundElements)
            {
               AddForgroundElement(foregroundElement);
            }
         }
      }

      private void HandleCustomGripperDragStart()
      {
         if (_MouseDownCustomGripper != null)
         {
            _MouseDownCustomGripper.OnDragStart();
         }
      }

      private void HandleCustomGripperDragUpdate(Point mousePoint)
      {
         if (_MouseDownCustomGripper != null)
         {
            _MouseDownCustomGripper.OnDragMove(mousePoint - _MouseDownPoint);
            _CustomGripperCompanion.SetModified();
         }
      }

      private void HandleCustomGripperDragEnd(Point mousePoint)
      {
         if (_MouseDownCustomGripper != null)
         {
            _MouseDownCustomGripper.OnDragEnd(mousePoint - _MouseDownPoint);
         }
      }

      public void AddForgroundElement(FrameworkElement element)
      {
         foregroundCanvas.Children.Add(element);
      }

      public void RemoveForgroundElement(FrameworkElement element)
      {
         foregroundCanvas.Children.Remove(element);
      }
   }
}
