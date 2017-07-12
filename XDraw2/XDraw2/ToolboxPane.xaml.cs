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

namespace XDraw
{
   /// <summary>
   /// Interaction logic for ToolboxPane.xaml
   /// </summary>
   public partial class ToolboxPane : UserControl, IDockPane
   {
      public ToolboxPane(object context, string id)
      {
         _Context = context;
         _PaneId = id;

         InitializeComponent();
      }



      public IEnumerable<INewItemType> ToolboxItems
      {
         get { return (IEnumerable<INewItemType>)GetValue(ToolboxItemsProperty); }
         set { SetValue(ToolboxItemsProperty, value); }
      }

      // Using a DependencyProperty as the backing store for ToolboxItems.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ToolboxItemsProperty =
          DependencyProperty.Register("ToolboxItems", typeof(IEnumerable<INewItemType>), typeof(ToolboxPane), new UIPropertyMetadata(null));


      public INewItemType SelectedItem
      {
         get { return (INewItemType)GetValue(SelectedItemProperty); }
         set { SetValue(SelectedItemProperty, value); }
      }

      // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SelectedItemProperty =
          DependencyProperty.Register("SelectedItem", typeof(INewItemType), typeof(ToolboxPane), new UIPropertyMetadata(null));



      #region IDockPane Members

      private object _Context;
      private string _PaneId;

      public object PaneContext
      {
         get { return _Context; }
      }

      public string PaneId
      {
         get { return _PaneId; }
      }

      public object PaneContent
      {
         get { return this; }
      }

      public string PaneTitle
      {
         get { return "Toolbox"; }
      }

      public string PaneToolTip
      {
         get { return "Toolbox"; }
      }

      public string PaneIconSource
      {
         get { return null; }
      }

      public bool IsPaneVisible
      {
         get { return (bool)GetValue(IsPaneVisibleProperty); }
         set { SetValue(IsPaneVisibleProperty, value); }
      }

      // Using a DependencyProperty as the backing store for IsPaneVisible.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty IsPaneVisibleProperty =
          DependencyProperty.Register("IsPaneVisible", typeof(bool), typeof(ToolboxPane), new UIPropertyMetadata(true));

      public PaneLocation DefaultPaneLocation
      {
         get { return PaneLocation.Left; }
      }

      public bool DefaultAutoHidePane
      {
         get { return true; }
      }

      public bool HidePaneOnClose
      {
         get { return true; }
      }

      public void PaneClosing(ref bool cancel)
      { }

      public void PaneClosed()
      { }

      #endregion

      private Point _MouseDownPoint = new Point();

      private INewItemType _MouseDownItem = null;

      private FrameworkElement _MouseCaptureElement = null;

      private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         var fe = e.OriginalSource as FrameworkElement;
         if (fe != null)
         {
            var item = fe.DataContext as INewItemType;
            if (item != null)
            {
               _MouseDownItem = item;
               _MouseDownPoint = e.GetPosition(this);
            }
            fe.CaptureMouse();
         }
      }

      private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         if (_MouseCaptureElement != null)
         {
            _MouseCaptureElement.ReleaseMouseCapture();
            _MouseCaptureElement = null;
         }
         _MouseDownItem = null;
      }

      private void Item_MouseMove(object sender, MouseEventArgs e)
      {
         if (_MouseDownItem != null)
         {
            var v = e.GetPosition(this) - _MouseDownPoint;
            if (Math.Abs(v.X) >= SystemParameters.MinimumHorizontalDragDistance 
               || Math.Abs(v.Y) >= SystemParameters.MinimumVerticalDragDistance)
            {
               if (_MouseCaptureElement != null)
               {
                  _MouseCaptureElement.ReleaseMouseCapture();
                  _MouseCaptureElement = null;
               }

               DragDrop.DoDragDrop(this, new DataObject(XDrawCore.DragDropItemType, _MouseDownItem), DragDropEffects.Copy);

               _MouseDownItem = null;
            }
         }
      }
   }
}
