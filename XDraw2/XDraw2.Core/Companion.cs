using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("XDraw.Editor")]

namespace XDraw
{
   /// <summary>
   /// Base class for XDrawEditor item companions
   /// </summary>
   public abstract class Companion : IDisposable, INotifyPropertyChanged
   {
      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="item">Item</param>
      public Companion(object item)
      {
         Item = item;
      }

      /// <summary>
      /// Gets the item of the companion
      /// </summary>
      [Browsable(false)]
      public object Item { get; private set; }

      /// <summary>
      /// Gets the object which should be shown in the properties tool pane
      /// </summary>
      /// <remarks>
      /// By default the Item is returned.
      /// </remarks>
      [Browsable(false)]
      public virtual object PropertiesObject
      {
         get { return Item; }
      }

      private FrameworkElement _FrameworkElement = null;

      /// <summary>
      /// Gets the FrameworkElement which should be used for visualization
      /// </summary>
      [Browsable(false)]
      public virtual FrameworkElement FrameworkElement
      {
         get { return _FrameworkElement; }
         set
         {
            if (value != _FrameworkElement)
            {
               _FrameworkElement = value;
               OnFrameworkElementChanged();
            }
         }
      }

      /// <summary>
      /// Fires the FrameworkElementChanged event
      /// </summary>
      protected virtual void OnFrameworkElementChanged()
      {
         GeometryChanged();
         var temp = FrameworkElementChanged;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
         NotifyPropertyChanged("FrameworkElement");
      }

      /// <summary>
      /// Occures when the FrameworkElement has changed
      /// </summary>
      public event EventHandler<EventArgs> FrameworkElementChanged;

      private Rect _OuterBounds = new Rect();

      /// <summary>
      /// Gets the bounds of the item
      /// </summary>
      [Browsable(false)]
      public virtual Rect OuterBounds
      {
         get { return _OuterBounds; }
         private set
         {
            if (value != _OuterBounds)
            {
               _OuterBounds = value;
               OnOuterBoundsChanged();
            }
         }
      }

      /// <summary>
      /// Fires the BoundsChanged event
      /// </summary>
      protected virtual void OnOuterBoundsChanged()
      {
         if (AutoApplyPosition && FrameworkElement != null)
         {
            FrameworkElement.SetValue(FrameworkElement.MarginProperty, new Thickness(OuterBounds.Left, OuterBounds.Top, 0.0, 0.0));
         }
         var temp = OuterBoundsChanged;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
         NotifyPropertyChanged("OuterBounds");
      }

      /// <summary>
      /// Occures when the bounds of the items has changed
      /// </summary>
      public event EventHandler<EventArgs> OuterBoundsChanged;

      private Rect _VertexBounds = new Rect();

      /// <summary>
      /// Gets the bounds of the item
      /// </summary>
      [Browsable(false)]
      public virtual Rect VertexBounds
      {
         get { return _VertexBounds; }
         private set
         {
            if (value != _VertexBounds)
            {
               _VertexBounds = value;
               OnVertexBoundsChanged();
            }
         }
      }

      /// <summary>
      /// Fires the VertexBoundsChanged event
      /// </summary>
      protected virtual void OnVertexBoundsChanged()
      {
         var temp = VertexBoundsChanged;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
         NotifyPropertyChanged("VertexBounds");
      }

      /// <summary>
      /// Occures when the bounds of the items has changed
      /// </summary>
      public event EventHandler<EventArgs> VertexBoundsChanged;

      protected virtual void GeometryChanged()
      {
         OuterBounds = CalcOuterBounds();
         VertexBounds = CalcVertexBounds();
      }

      protected abstract Rect CalcOuterBounds();

      protected abstract Rect CalcVertexBounds();

      /// <summary>
      /// Gets if the item is selectedbale
      /// </summary>
      [Browsable(false)]
      public virtual bool Selectable
      { get { return true; } }

      /// <summary>
      /// Gets if the item is movable
      /// </summary>
      [Browsable(false)]
      public virtual bool Movable
      { get { return true; } }

      /// <summary>
      /// Gets if the item is resizable
      /// </summary>
      [Browsable(false)]
      public virtual bool Resizable
      { get { return true; } }

      /// <summary>
      /// Gets if the item is horizontally resizable
      /// </summary>
      [Browsable(false)]
      public virtual bool HorizontallyResizable
      { get { return true; } }

      /// <summary>
      /// Gets if the item is vertically resizable
      /// </summary>
      [Browsable(false)]
      public virtual bool VerticallyResizable
      { get { return true; } }

      [Browsable(false)]
      public virtual bool AutoApplyPosition
      { get { return false; } }

      private bool _IsSelected = false;

      /// <summary>
      /// Gets or sets if the item is selected
      /// </summary>
      [Browsable(false)]
      public bool IsSelected
      {
         get { return _IsSelected; }
         set
         {
            if (value != _IsSelected)
            {
               _IsSelected = value;
               OnSelectionChanged(_IsSelected);
               NotifyPropertyChanged("IsSelected");
            }
         }
      }

      /// <summary>
      /// Fires the SelectionChanged event
      /// </summary>
      /// <param name="isSelected"></param>
      protected virtual void OnSelectionChanged(bool isSelected)
      {
         var temp = SelectionChanged;
         if (temp != null)
         {
            temp(this, new SelectionChangedEventAgrs(isSelected));
         }
      }

      /// <summary>
      /// Occures when the selection of the item has changed
      /// </summary>
      public event EventHandler<SelectionChangedEventAgrs> SelectionChanged;

      /// <summary>
      /// Toggels the selection
      /// </summary>
      public virtual void ToggleSelection()
      {
         IsSelected = !IsSelected;
      }

      #region IDisposable Members

      /// <summary>
      /// Disposes the companion
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Finalizer
      /// </summary>
      ~Companion()
      {
         Dispose(false);
      }

      /// <summary>
      /// Disposes the companion
      /// </summary>
      /// <param name="disposing">true if managed resources should be released as well</param>
      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
         {
            Item = null;
         }
      }

      #endregion

      #region INotifyPropertyChanged Members

      /// <summary>
      /// Fires the PropertyChanged event
      /// </summary>
      /// <param name="propertyName">Name of the changed propoerty.</param>
      protected virtual void NotifyPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;
         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      /// <summary>
      /// Occures if the value of an property has changed
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      /// <summary>
      /// Starts the drag move action
      /// </summary>
      /// <param name="mouseStartPoint">Start position of the mouse cursor</param>
      public virtual void OnDragStart()
      {
         SetModified();
      }

      /// <summary>
      /// Finishes the drag move action
      /// </summary>
      /// <param name="delta">Moved distance since drag start</param>
      public virtual void OnDragEnd(Vector delta)
      {
         SetModified();
      }

      /// <summary>
      /// Updates the drag move action
      /// </summary>
      /// <param name="delta">Moved distance since drag start</param>
      public virtual void OnDragMove(Vector delta)
      {
         SetModified();
      }

      private Rect _ResizeStartBounds = new Rect();

      protected Rect ResizeStartBounds
      {
         get { return _ResizeStartBounds; }
      }

      public virtual void OnStartResize(ResizePosition resizePos, Point resizeOrigin)
      {
         _ResizeStartBounds = VertexBounds;
         SetModified();
      }

      public virtual void OnUpdateResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         SetModified();
      }

      public virtual void OnFinishResize(Rect newBounds, ResizePosition resizePos, Point resizeOrigin)
      {
         SetModified();
      }

      internal Rect CalcNewCompanionBounds(Rect oldBounds, Rect newBounds)
      {
         var scaleX = 1.0;
         if (newBounds.Width > Double.Epsilon)
         {
            scaleX = newBounds.Width / oldBounds.Width;
         }
         var scaleY = 1.0;
         if (newBounds.Height > Double.Epsilon)
         {
            scaleY = newBounds.Height / oldBounds.Height;
         }
         var offsetX = newBounds.Left - oldBounds.Left * scaleX;
         var offsetY = newBounds.Top - oldBounds.Top * scaleY;

         return new Rect(
            _ResizeStartBounds.Left * scaleX + offsetX,
            _ResizeStartBounds.Top * scaleY + offsetY,
            _ResizeStartBounds.Width * scaleX,
            _ResizeStartBounds.Height * scaleY);
      }

      protected Transform CalcResizeTransform(Rect newBounds, Point resizeOrigin)
      {
         double scaleX = 1.0;
         if (_ResizeStartBounds.Width > Double.Epsilon)
         {
            scaleX = newBounds.Width / _ResizeStartBounds.Width;
         }
         double scaleY = 1.0;
         if (_ResizeStartBounds.Height > Double.Epsilon)
         {
            scaleY = newBounds.Height / _ResizeStartBounds.Height;
         }
         return new ScaleTransform(scaleX, scaleY, resizeOrigin.X, resizeOrigin.Y);
      }

      public virtual void OnCreateCustomElements(IList<Gripper> grippers, IList<FrameworkElement> foregroundElements)
      { }

      public virtual void OnClearCustomElements(IList<Gripper> grippers, IList<FrameworkElement> foregroundElements)
      { }

      private bool _IsModified = false;

      /// <summary>
      /// Gest if the item was modified
      /// </summary>
      [Browsable(false)]
      public bool IsModified
      {
         get { return _IsModified; }
         private set
         {
            if (value != _IsModified)
            {
               _IsModified = value;
               NotifyPropertyChanged("IsModified");
            }
         }
      }

      /// <summary>
      /// Flags the item as modified
      /// </summary>
      public void SetModified()
      {
         IsModified = true;
         OnModified();
      }

      /// <summary>
      /// Clears the IsModified flag
      /// </summary>
      public void ClearModifiedFlag()
      {
         IsModified = false;
      }

      /// <summary>
      /// Fires the Modified event
      /// </summary>
      protected virtual void OnModified()
      {
         var temp = Modified;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }         
      }

      /// <summary>
      /// Occures when the companion is modified
      /// </summary>
      public event EventHandler<EventArgs> Modified;
   }

   /// <summary>
   /// Event args for the SelectionChanged event
   /// </summary>
   public class SelectionChangedEventAgrs : EventArgs
   {
      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="isSelected">true if new state is selected</param>
      public SelectionChangedEventAgrs(bool isSelected)
      {
         IsSelected = isSelected;
      }

      /// <summary>
      /// Gest if the new state is selected
      /// </summary>
      public bool IsSelected { get; private set; }
   }

   public enum ResizePosition
   {
      TopLeft = 0,
      Top = 1,
      TopRight = 2,
      Left = 3,
      Right = 4,
      BottomLeft = 5,
      Bottom = 6,
      BottomRight = 7
   }
}
