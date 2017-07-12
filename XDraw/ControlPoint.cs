using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Shapes;

namespace remes.XDraw
{
   public class ControlPoint : IDisposable, IDragableObject
   {
      private EllipseGeometry m_CPGeom = null;
      private LineGeometry m_ConnectionLine = null;

      public XDrawing Drawing { get; private set; }
      public XDrawingShape Shape { get; private set; }

      private DependencyObject m_Object;
      private DependencyProperty m_Prop;
      private DependencyProperty m_RelativeToProp;
      private bool m_IsSelected = false;
      private bool m_IsSelectable;

      public object Tag { get; set; }

      public bool IsSelected
      {
         get { return m_IsSelected; }
         set
         {
            if (m_IsSelected != value)
            {
               m_IsSelected = value;
               if (m_CPGeom != null)
               {
                  if (m_IsSelected)
                  {
                     Drawing.ControlPointGroup.Children.Remove(m_CPGeom);
                     Drawing.SelectedControlPointGroup.Children.Add(m_CPGeom);
                  }
                  else
                  {
                     Drawing.SelectedControlPointGroup.Children.Remove(m_CPGeom);
                     Drawing.ControlPointGroup.Children.Add(m_CPGeom);
                  }
               }
            }
         }
      }

      public bool IsSelectable
      {
         get { return m_IsSelectable; }
         set { m_IsSelectable = value; }
      }

      private int m_Dings = 0;

      public ControlPoint SubPoint { get; set; }

      private ControlPoint m_ConnectedTo = null;

      public ControlPoint ConnectedTo
      {
         get { return m_ConnectedTo; }
         set
         {
            m_ConnectedTo = value;
            if (m_ConnectedTo != null)
            {
               m_ConnectionLine = new LineGeometry(m_ConnectedTo.GetPoint(), GetPoint());
               Drawing.ControlLineGroup.Children.Add(m_ConnectionLine);
               DependencyPropertyDescriptor.FromProperty(m_ConnectedTo.m_Prop, m_ConnectedTo.m_Object.GetType()).AddValueChanged(m_ConnectedTo.m_Object, PropertyChanged);
            }
         }
      }

      public ControlPoint(XDrawing drawing, XDrawingShape shape,
         DependencyObject obj, DependencyProperty prop, 
         int dings, bool visible, bool isSelectable)
      {
         ConnectedTo = null;
         Drawing = drawing;
         Shape = shape;
         m_Object = obj;
         m_Prop = prop;
         m_Dings = dings;
         m_IsSelectable = isSelectable;
         if (m_Prop != null)
         {
            DependencyPropertyDescriptor.FromProperty(m_Prop, m_Object.GetType()).AddValueChanged(m_Object, PropertyChanged);
         }
         if (visible)
         {
            m_CPGeom = new EllipseGeometry(GetPoint(), 5.0 / Drawing.ZoomFactor, 5.0 / Drawing.ZoomFactor);
            Drawing.ControlPointGroup.Children.Add(m_CPGeom);
         }
      }

      public DependencyProperty RelativeTo
      {
         get { return m_RelativeToProp; }
         set
         {
            m_RelativeToProp = value;
            DependencyPropertyDescriptor.FromProperty(m_RelativeToProp, m_Object.GetType()).AddValueChanged(m_Object, RelativeToPropertyChanged);
            m_CPGeom.Center = GetPoint();
         }
      }

      private void PropertyChanged(object sender, EventArgs e)
      {
         if (m_CPGeom != null)
         {
            m_CPGeom.Center = GetPoint();
         }
         if (m_ConnectionLine != null)
         {
            m_ConnectionLine.StartPoint = m_ConnectedTo.GetPoint();
            m_ConnectionLine.EndPoint = GetPoint();
         }
      }

      private void RelativeToPropertyChanged(object sender, EventArgs e)
      {
         if (m_CPGeom != null)
         {
            m_CPGeom.Center = GetPoint();
         }
      }

      public void SetPoint(Point pt)
      {
         if (m_Prop != null)
         {
            if (SubPoint != null)
            {
               var offset = pt - GetPoint();
               SubPoint.SetPoint(SubPoint.GetPoint() + offset);
            }
            if (m_Prop.PropertyType == typeof(double))
            {
               if (m_RelativeToProp != null)
               {
                  var ptR = (Point)m_Object.GetValue(m_RelativeToProp);
                  pt -= (Vector)ptR;
               }
               if (m_Dings == 0)
               {
                  m_Object.SetValue(m_Prop, pt.X);
               }
               else
               {
                  m_Object.SetValue(m_Prop, pt.Y);
               }
            }
            else if (m_Prop.PropertyType == typeof(Point))
            {
               m_Object.SetValue(m_Prop, pt);
            }
            else if (m_Prop.PropertyType == typeof(Rect))
            {
               Rect rect = (Rect)m_Object.GetValue(m_Prop);
               switch (m_Dings)
               {
                  case -1:
                     rect.Location = pt;
                     break;

                  case 0:
                     if (pt.X > rect.Right)
                     {
                        pt.X = rect.Right;
                     }
                     if (pt.Y > rect.Bottom)
                     {
                        pt.Y = rect.Bottom;
                     }
                     rect.Width = rect.Right - pt.X;
                     rect.Height = rect.Bottom - pt.Y;
                     rect.Location = pt;
                     break;

                  case 1:
                     if (pt.X < rect.Left)
                     {
                        pt.X = rect.Left;
                     }
                     if (pt.Y > rect.Bottom)
                     {
                        pt.Y = rect.Bottom;
                     }
                     rect.Width = pt.X - rect.Left;
                     rect.Height = rect.Bottom - pt.Y;
                     rect.Location = new Point(rect.Location.X, pt.Y);
                     break;

                  case 2:
                     if (pt.X > rect.Right)
                     {
                        pt.X = rect.Right;
                     }
                     if (pt.Y < rect.Top)
                     {
                        pt.Y = rect.Top;
                     }
                     rect.Width = rect.Right - pt.X;
                     rect.Location = new Point(pt.X, rect.Location.Y);
                     rect.Height = pt.Y - rect.Top;
                     break;

                  case 3:
                     if (pt.X < rect.Left)
                     {
                        pt.X = rect.Left;
                     }
                     if (pt.Y < rect.Top)
                     {
                        pt.Y = rect.Top;
                     }
                     rect.Width = pt.X - rect.Left;
                     rect.Height = pt.Y - rect.Top;
                     break;
               }
               m_Object.SetValue(m_Prop, rect);
            }
         }
      }

      public Point GetPoint()
      {
         if (m_Prop != null)
         {
            if (m_Prop.PropertyType == typeof(double))
            {
               Point pt = new Point();
               if (m_Dings == 0)
               {
                  pt.X = (double)m_Object.GetValue(m_Prop);
                  pt.Y = 0.0;
               }
               else
               {
                  pt.X = 0.0;
                  pt.Y = (double)m_Object.GetValue(m_Prop);
               }
               if (m_RelativeToProp != null)
               {
                  var ptR = (Point)m_Object.GetValue(m_RelativeToProp);
                  pt += (Vector)ptR;
               } return pt;
            }
            else if (m_Prop.PropertyType == typeof(Point))
            {
               return (Point)m_Object.GetValue(m_Prop);
            }
            else if (m_Prop.PropertyType == typeof(Rect))
            {
               Rect rect = (Rect)m_Object.GetValue(m_Prop);
               switch (m_Dings)
               {
                  case -1:
                  case 0: return rect.TopLeft;
                  case 1: return rect.TopRight;
                  case 2: return rect.BottomLeft;
                  case 3: return rect.BottomRight;
               }
            }
         }
         return new Point(0, 0);
      }

      ~ControlPoint()
      {
         Dispose(false);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (m_CPGeom != null)
            {
               if (m_IsSelected)
               {
                  Drawing.SelectedControlPointGroup.Children.Remove(m_CPGeom);
               }
               else
               {
                  Drawing.ControlPointGroup.Children.Remove(m_CPGeom);
               }
            }
            m_CPGeom = null;
            if (m_ConnectionLine != null)
            {
               Drawing.ControlLineGroup.Children.Remove(m_ConnectionLine);
            }
            m_ConnectionLine = null;
         }
      }

      #region IDisposable Members

      public void Dispose()
      {
         Dispose(true);
      }

      #endregion

      #region IDragableObject Members

      private bool m_IsDraged = false;

      public HitTestInfo? HitTest(Point pt)
      {
         var d = pt - GetPoint();
         if (d.X * d.X + d.Y * d.Y <= (5.0 / Drawing.ZoomFactor) * (5.0 / Drawing.ZoomFactor))
         {
            HitTestInfo hti = new HitTestInfo();
            hti.Shape = Shape;
            hti.ControlPoint = this;
            hti.Offset = d;
            return hti;
         }
         return null;
      }

      public void StartDrag()
      {
         m_IsDraged = true;
      }

      public void DragObject(Point pt)
      {
         System.Diagnostics.Debug.Assert(m_IsDraged, "DragObject is called but StartDrag was not");
         SetPoint(pt);
      }

      public void EndDrag()
      {
         m_IsDraged = false;
      }

      public bool IsDraged
      {
         get { return m_IsDraged; }
         internal set { m_IsDraged = value; }
      }

      #endregion

      internal void ZoomChanged(double zoomFactor)
      {
         m_CPGeom.RadiusX = 5.0 / zoomFactor;
         m_CPGeom.RadiusY = 5.0 / zoomFactor;
      }
   }
}
