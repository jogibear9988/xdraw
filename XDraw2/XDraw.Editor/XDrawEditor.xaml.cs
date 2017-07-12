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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using XDraw.Controls.Media;
using System.Threading;
using System.ComponentModel;

namespace XDraw.Editor
{
   /// <summary>
   /// Interaction logic for XDrawEditor.xaml
   /// </summary>
   public partial class XDrawEditor : UserControl, INotifyPropertyChanged
   {
      /// <summary>
      /// Constructor
      /// </summary>
      public XDrawEditor()
      {
         CompanionTypeResolver = null;

         InitializeComponent();

         if (ItemsPanel == null)
         {
            ItemsPanel = FindResource("defaultItemsPanel") as ItemsPanelTemplate;
         }

         if (SelectionRectStyle == null)
         {
            SelectionRectStyle = FindResource("selectionRectDefaultStyle") as Style;
         }

         if (MouseSelectShapeStyle == null)
         {
            MouseSelectShapeStyle = FindResource("mouseSelectShapeDefaultStyle") as Style;
         }

         _SelectionRect = new ScreenSpaceRectangle2()
         {
            Style = SelectionRectStyle,
            ScreenSpaceSize = false,
            Visibility = System.Windows.Visibility.Collapsed
         };
         foregroundCanvas.Children.Add(_SelectionRect);

         _SelectionRectGrippers[0] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[0].Tag = ResizePosition.TopLeft;
         _SelectionRectGrippers[0].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[0]);

         _SelectionRectGrippers[1] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[1].Tag = ResizePosition.Top;
         _SelectionRectGrippers[1].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[1]);

         _SelectionRectGrippers[2] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[2].Tag = ResizePosition.TopRight;
         _SelectionRectGrippers[2].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[2]);

         _SelectionRectGrippers[3] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[3].Tag = ResizePosition.Left;
         _SelectionRectGrippers[3].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[3]);

         _SelectionRectGrippers[4] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[4].Tag = ResizePosition.Right;
         _SelectionRectGrippers[4].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[4]);

         _SelectionRectGrippers[5] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[5].Tag = ResizePosition.BottomLeft;
         _SelectionRectGrippers[5].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[5]);

         _SelectionRectGrippers[6] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[6].Tag = ResizePosition.Bottom;
         _SelectionRectGrippers[6].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[6]);

         _SelectionRectGrippers[7] = Gripper.CreateSquareGripper();
         _SelectionRectGrippers[7].Tag = ResizePosition.BottomRight;
         _SelectionRectGrippers[7].Visibility = System.Windows.Visibility.Collapsed;
         foregroundCanvas.Children.Add(_SelectionRectGrippers[7]);
      }

      /// <summary>
      /// Registers a companion type for a specified item type
      /// </summary>
      /// <param name="itemType">Type of the item</param>
      /// <param name="companionType">Type of the companion</param>
      /// <remarks>
      /// If there is no itemType for an item registered, but a base type is, then the base type is used.
      /// To have a fallback companion type for any type of item you can register a itemType for 'Object'.
      /// </remarks>
      public void RegisterCompanionType(Type itemType, Type companionType)
      {
         _CompanionTypes.Add(itemType, companionType);
      }

      /// <summary>
      /// Gets or sets the compaion type resolver
      /// </summary>
      public ICompanionTypeResolver CompanionTypeResolver { get; set; }


      /// <summary>
      /// Gets or sets the background brush for the workspace
      /// </summary>
      public Brush WorkspaceBrush
      {
         get { return (Brush)GetValue(WorkspaceBrushProperty); }
         set { SetValue(WorkspaceBrushProperty, value); }
      }

      /// <summary>
      /// Dependency property for the WorkspaceBrush property
      /// </summary>
      public static readonly DependencyProperty WorkspaceBrushProperty =
          DependencyProperty.Register("WorkspaceBrush", typeof(Brush), typeof(XDrawEditor), new UIPropertyMetadata(Brushes.White));


      /// <summary>
      /// Gets or sets the with of the workspace
      /// </summary>
      /// <remarks>
      /// If WorkspaceSizeMode is set to Auto then the WorkspaceWidth does not need to be set
      /// </remarks>
      public double WorkspaceWidth
      {
         get { return (double)GetValue(WorkspaceWidthProperty); }
         set { SetValue(WorkspaceWidthProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the WorkspaceWidth property
      /// </summary>
      public static readonly DependencyProperty WorkspaceWidthProperty =
          DependencyProperty.Register("WorkspaceWidth", typeof(double), typeof(XDrawEditor), new UIPropertyMetadata(0.0));

      /// <summary>
      /// Gets or sets the height of the workspace
      /// </summary>
      /// <remarks>
      /// If WorkspaceSizeMode is set to Auto then the WorkspaceHeight does not need to be set
      /// </remarks>
      public double WorkspaceHeight
      {
         get { return (double)GetValue(WorkspaceHeightProperty); }
         set { SetValue(WorkspaceHeightProperty, value); }
      }

      /// <summary>
      /// Dependency property for the WorkspaceHeight property
      /// </summary>
      public static readonly DependencyProperty WorkspaceHeightProperty =
          DependencyProperty.Register("WorkspaceHeight", typeof(double), typeof(XDrawEditor), new UIPropertyMetadata(0.0));


      /// <summary>
      /// Gets or sets the Workspace size mode
      /// </summary>
      public WorkspaceSizeMode WorkspaceSizeMode
      {
         get { return (WorkspaceSizeMode)GetValue(WorkspaceSizeModeProperty); }
         set { SetValue(WorkspaceSizeModeProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the WorkspaceSizeMode property
      /// </summary>
      public static readonly DependencyProperty WorkspaceSizeModeProperty =
          DependencyProperty.Register("WorkspaceSizeMode", typeof(WorkspaceSizeMode), typeof(XDrawEditor), new UIPropertyMetadata(WorkspaceSizeMode.Auto));


      public ItemsPanelTemplate ItemsPanel
      {
         get { return (ItemsPanelTemplate)GetValue(ItemsPanelProperty); }
         set { SetValue(ItemsPanelProperty, value); }
      }

      // Using a DependencyProperty as the backing store for ItemsPanel.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ItemsPanelProperty =
          DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(XDrawEditor), new UIPropertyMetadata(null));
      

      /// <summary>
      /// Gets or sets the active tool
      /// </summary>
      public Tool ActiveTool
      {
         get { return (Tool)GetValue(ActiveToolProperty); }
         set { SetValue(ActiveToolProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the ActiveTool property.
      /// </summary>
      public static readonly DependencyProperty ActiveToolProperty =
          DependencyProperty.Register("ActiveTool", typeof(Tool), typeof(XDrawEditor), new UIPropertyMetadata(Tool.Select));


      /// <summary>
      /// Gets or sets the current selection mode
      /// </summary>
      /// <remarks>
      /// The selection mode can be a combination of any of the SelectionModes elements with one exception:
      /// Rectangle and Lasso can not be combined.
      /// </remarks>
      public SelectionModes SelectionMode
      {
         get { return (SelectionModes)GetValue(SelectionModeProperty); }
         set { SetValue(SelectionModeProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the SelectionMode property.
      /// </summary>
      public static readonly DependencyProperty SelectionModeProperty =
          DependencyProperty.Register("SelectionMode", typeof(SelectionModes), typeof(XDrawEditor), new UIPropertyMetadata(SelectionModes.RectangleClick));

      /// <summary>
      /// Gets or sets the list with items in the editor
      /// </summary>
      public IList Items
      {
         get { return (IList)GetValue(ItemsProperty); }
         set { SetValue(ItemsProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the Items property
      /// </summary>
      public static readonly DependencyProperty ItemsProperty =
          DependencyProperty.Register("Items", typeof(IList), typeof(XDrawEditor), new UIPropertyMetadata(null, Items_Changed));

      private static void Items_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as XDrawEditor).Items_Changed(e);
      }

      private void Items_Changed(DependencyPropertyChangedEventArgs e)
      {
         _ItemsSupportsCollectionChanged = false;

         var oldCollection = e.OldValue as IList;
         var newCollection = e.NewValue as IList;

         if (oldCollection == newCollection)
         {
            return;
         }

         if (oldCollection != null)
         {
            var ncc = oldCollection as INotifyCollectionChanged;
            if (ncc != null)
            {
               ncc.CollectionChanged -= Items_CollectionChanged;
            }
            RemoveCompanions(oldCollection);
         }

         _CompanionDict.Clear();
         _Companions.Clear();
         
         if (newCollection != null)
         {
            AddCompanions(newCollection);
            var ncc = newCollection as INotifyCollectionChanged;
            if (ncc != null)
            {
               _ItemsSupportsCollectionChanged = true;
               ncc.CollectionChanged += Items_CollectionChanged;
            }
         }
         if (WorkspaceSizeMode == Editor.WorkspaceSizeMode.Auto)
         {
            UpdateWorkspaceSize();
         }
      }

      private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if (e.Action == NotifyCollectionChangedAction.Reset)
         {
            _CompanionDict.Clear();
            _Companions.Clear();

            var selectedItems = SelectedItems.ToList();

            AddCompanions(Items);

            foreach (var item in selectedItems)
            {
               if (Items.Contains(item))
               {
                  GetItemCompanion(item).IsSelected = true;
               }
            }
         }
         else if (e.Action != NotifyCollectionChangedAction.Move)
         {
            if (e.OldItems != null)
            {
               RemoveCompanions(e.OldItems);
            }
            if (e.NewItems != null)
            {
               AddCompanions(e.NewItems);
            }

            if (WorkspaceSizeMode == Editor.WorkspaceSizeMode.Auto)
            {
               UpdateWorkspaceSize();
            }
         }
         
         ReorderCompanions();
      }

      private bool _ItemsSupportsCollectionChanged = false;

      private void ReorderCompanions()
      {
         // todo: replace this with an optimized algorithm using the CollectionChanged event args
         Companion companion;
         int n = 0;
         foreach(var item in Items)
         {
            if (_CompanionDict.TryGetValue(item, out companion))
            {
               var i = _Companions.IndexOf(companion);
               if (i != n)
               {
                  _Companions.Move(i, n);
               }
               ++n;
            }
         }
      }

      /// <summary>
      /// Updates the Workspace size to fit the items in the Editor
      /// </summary>
      private void UpdateWorkspaceSize()
      {
         double width = 0.0;
         double height = 0.0;
         foreach (var companion in _CompanionDict.Values)
         {
            var bounds = companion.OuterBounds;
            width = Math.Max(width, bounds.Right);
            height = Math.Max(height, bounds.Bottom);
         }
         WorkspaceWidth = width;
         WorkspaceHeight = height;
      }

      /// <summary>
      /// Adds a companion for every item in the given list
      /// </summary>
      /// <param name="items">Items</param>
      private void AddCompanions(IList items)
      {
         foreach (var newItem in items)
         {
            CreateCompanion(newItem);
         }
      }

      /// <summary>
      /// Removes the companions for every item in the given list
      /// </summary>
      /// <param name="items">Items</param>
      private void RemoveCompanions(IList items)
      {
         foreach (var oldItem in items)
         {
            RemoveCompanion(oldItem);
         }
      }

      /// <summary>
      /// Creates an Companion for the given Item
      /// </summary>
      /// <param name="item">Item</param>
      /// <returns>Return the created Companion or null if no mathcing itemType was found</returns>
      private Companion CreateCompanion(object item)
      {
         // find best matching companion type
         Type companionType = null;

         if (CompanionTypeResolver != null)
         {
            companionType = CompanionTypeResolver.Resolve(item);
         }

         if (companionType == null)
         {
            Type itemType = item.GetType();
            Type foundItemType = null;
            bool iterateAgain;
            do
            {
               iterateAgain = false;

               foreach (var typeTuple in _CompanionTypes)
               {
                  if (foundItemType != typeTuple.Key)
                  {
                     if (itemType == typeTuple.Key)
                     {
                        foundItemType = typeTuple.Key;
                        break;
                     }
                     else if (itemType.IsSubclassOf(typeTuple.Key))
                     {
                        if (foundItemType == null)
                        {
                           foundItemType = typeTuple.Key;
                           iterateAgain = true;
                           break;
                        }
                        else if (typeTuple.Key.IsSubclassOf(foundItemType))
                        {
                           foundItemType = typeTuple.Key;
                           iterateAgain = true;
                           break;
                        }
                     }
                  }
               }
            }
            while (iterateAgain);

            if (foundItemType != null)
            {
               companionType = _CompanionTypes[foundItemType];
            }
         }

         if (companionType != null)
         {
            var companion = companionType.Assembly.CreateInstance(companionType.FullName, true, BindingFlags.Default, null, new object[] { item }, null, null) as Companion;
            companion.SelectionChanged += Companion_SelectionChanged;
            companion.OuterBoundsChanged += Companion_BoundsChanged;
            companion.Modified += Companion_Modified;
            _CompanionDict.Add(item, companion);
            _Companions.Add(companion);
            return companion;
         }
         return null;
      }

      private void Companion_Modified(object sender, EventArgs e)
      {
         SetModified();
      }

      /// <summary>
      /// Removes the companion of the given item
      /// </summary>
      /// <param name="item">Item</param>
      private void RemoveCompanion(object item)
      {
         Companion companion;
         if (_CompanionDict.TryGetValue(item, out companion))
         {
            if (_SelectedCompanions.Contains(companion))
            {
               _SelectedCompanions.Remove(companion);
               UpdateSelectionRect();
               OnSelectionChanged();
               UpdateCustomGrippers();
            }
            _CompanionDict.Remove(item);
            _Companions.Remove(companion);
            companion.SelectionChanged -= Companion_SelectionChanged;
            companion.OuterBoundsChanged -= Companion_BoundsChanged;
            companion.Modified -= Companion_Modified;
            companion.Dispose();
         }
         else
         {
            Debug.Assert(_CompanionDict.ContainsKey(item), "_Companion dictionary does not contain item to remove!");
         }
      }

      private List<Companion> _SelectedCompanions = new List<Companion>();

      private IList<Companion> _ReadOnlySelectedCompanions = null;

      /// <summary>
      /// Gets the list with the selected Companions
      /// </summary>
      /// <remarks>
      /// The returned list is read only.
      /// </remarks>
      public IList<Companion> SelectedCompanions
      {
         get
         {
            if (_ReadOnlySelectedCompanions == null)
            {
               _ReadOnlySelectedCompanions = new ReadOnlyCollection<Companion>(_SelectedCompanions);
            }
            return _ReadOnlySelectedCompanions;
         }
      }

      private void Companion_SelectionChanged(object sender, SelectionChangedEventAgrs e)
      {
         var companion = sender as Companion;
         if (companion != null)
         {
            if (e.IsSelected && !_SelectedCompanions.Contains(companion))
            {
               _SelectedCompanions.Add(companion);
            }
            else if (!e.IsSelected && _SelectedCompanions.Contains(companion))
            {
               _SelectedCompanions.Remove(companion);
            }
            UpdateSelectionRect();
            OnSelectionChanged();
            UpdateCustomGrippers();
         }
      }

      /// <summary>
      /// Gets an enumeration of all selected items
      /// </summary>
      public IEnumerable<object> SelectedItems
      {
         get
         {
            foreach (var companion in _SelectedCompanions)
            {
               yield return companion.Item;
            }
         }
      }

      private void OnSelectionChanged()
      {
         var lastSelectedCompanion = _SelectedCompanions.LastOrDefault();
         if (lastSelectedCompanion != null)
         {
            XDrawCore.Instance.PropertiesObject = lastSelectedCompanion.PropertiesObject;
         }
         else
         {
            XDrawCore.Instance.PropertiesObject = null;
         }

         var temp = SelectionChanged;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
         CommandManager.InvalidateRequerySuggested();
         NotifyPropertyChanged("SelectedComanions");
         NotifyPropertyChanged("SelectedItems");
      }

      /// <summary>
      /// Occures after the selection has changed
      /// </summary>
      public event EventHandler<EventArgs> SelectionChanged;

      private ScreenSpaceRectangle2 _SelectionRect = null;

      private Gripper[] _SelectionRectGrippers = new Gripper[8];

      private Rect CalcOuterSelectionBounds()
      {
         double left = Double.MaxValue;
         double top = Double.MaxValue;
         double right = Double.MinValue;
         double bottom = Double.MinValue;
         foreach (var companion in _SelectedCompanions)
         {
            var bounds = companion.OuterBounds;
            left = Math.Min(left, bounds.Left);
            top = Math.Min(top, bounds.Top);
            right = Math.Max(right, bounds.Right);
            bottom = Math.Max(bottom, bounds.Bottom);
         }
         return new Rect(left, top, right - left, bottom - top);
      }

      private Rect CalcVertexSelectionBounds()
      {
         double left = Double.MaxValue;
         double top = Double.MaxValue;
         double right = Double.MinValue;
         double bottom = Double.MinValue;
         foreach (var companion in _SelectedCompanions)
         {
            var bounds = companion.VertexBounds;
            left = Math.Min(left, bounds.Left);
            top = Math.Min(top, bounds.Top);
            right = Math.Max(right, bounds.Right);
            bottom = Math.Max(bottom, bounds.Bottom);
         }
         return new Rect(left, top, right - left, bottom - top);
      }

      /// <summary>
      /// Updates the rectangle around the selected items
      /// </summary>
      private void UpdateSelectionRect()
      {
         if (_SelectedCompanions.Count > 0)
         {
            var outerBounds = CalcOuterSelectionBounds();

            _SelectionRect.X = outerBounds.Left;
            _SelectionRect.Y = outerBounds.Top;
            _SelectionRect.RectWidth = outerBounds.Width;
            _SelectionRect.RectHeight = outerBounds.Height;
            _SelectionRect.Visibility = Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.TopLeft].Position = outerBounds.TopLeft;
            _SelectionRectGrippers[(int)ResizePosition.TopLeft].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.Top].Position = new Point(outerBounds.Left + outerBounds.Width / 2.0, outerBounds.Top);
            _SelectionRectGrippers[(int)ResizePosition.Top].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.TopRight].Position = outerBounds.TopRight;
            _SelectionRectGrippers[(int)ResizePosition.TopRight].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.Left].Position = new Point(outerBounds.Left, outerBounds.Top + outerBounds.Height / 2.0);
            _SelectionRectGrippers[(int)ResizePosition.Left].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.Right].Position = new Point(outerBounds.Right, outerBounds.Top + outerBounds.Height / 2.0);
            _SelectionRectGrippers[(int)ResizePosition.Right].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.BottomLeft].Position = outerBounds.BottomLeft;
            _SelectionRectGrippers[(int)ResizePosition.BottomLeft].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.Bottom].Position = new Point(outerBounds.Left + outerBounds.Width / 2.0, outerBounds.Bottom);
            _SelectionRectGrippers[(int)ResizePosition.Bottom].Visibility = System.Windows.Visibility.Visible;

            _SelectionRectGrippers[(int)ResizePosition.BottomRight].Position = outerBounds.BottomRight;
            _SelectionRectGrippers[(int)ResizePosition.BottomRight].Visibility = System.Windows.Visibility.Visible;
         }
         else if (_SelectionRect != null)
         {
            foreach (var gripper in _SelectionRectGrippers)
            {
               gripper.Visibility = System.Windows.Visibility.Collapsed;
            }

            _SelectionRect.Visibility = Visibility.Collapsed;
         }
      }

      private void Companion_BoundsChanged(object sender, EventArgs e)
      {
         var companion = sender as Companion;
         if (companion != null)
         {
            UpdateWorkspaceSize();

            if (companion.IsSelected)
            {
               UpdateSelectionRect();
            }
         }
      }

      private Dictionary<object, Companion> _CompanionDict = new Dictionary<object, Companion>();

      private ObservableCollection<Companion> _Companions = new ObservableCollection<Companion>();
      private IList<Companion> _CompanionsReadOnly = null;

      /// <summary>
      /// Gets a list with all companions
      /// </summary>
      public IList<Companion> Companions
      {
         get
         {
            if (_CompanionsReadOnly == null)
            {
               _CompanionsReadOnly = new ReadOnlyObservableCollection<Companion>(_Companions);
            }
            return _CompanionsReadOnly;
         }
      }

      private Dictionary<Type, Type> _CompanionTypes = new Dictionary<Type, Type>();

      /// <summary>
      /// Gets the companion to a given item
      /// </summary>
      /// <param name="item">Item</param>
      /// <returns>Returns the companion of the gioven item</returns>
      public Companion GetItemCompanion(object item)
      {
         Companion companion;
         if (_CompanionDict.TryGetValue(item, out companion))
         {
            return companion;
         }
         return null;
      }

      private bool _SetZoomFitPending = false;

      /// <summary>
      /// Sets the zoom and panning in a way that the workspace it fully visibla at a maximum zoom factor
      /// </summary>
      public void SetZoomFit()
      {
         if (scrollViewer.ViewportWidth > 0 && scrollViewer.ViewportHeight > 0)
         {
            if (WorkspaceWidth > 0.0 && WorkspaceHeight > 0.0)
            {
               scrollViewer.UpdateLayout();
               var newFactor = Math.Min(scrollViewer.ViewportWidth / WorkspaceWidth, scrollViewer.ViewportHeight / WorkspaceHeight);
               if (newFactor <= 0.0)
               {
                  newFactor = 1.0;
               }
               ZoomFactor = newFactor;
               scrollViewer.UpdateLayout();
               var newZoomCenterPt = scrollViewer.TranslatePoint(new Point(scrollViewer.ViewportWidth / 2.0, scrollViewer.ViewportHeight / 2.0), workspace);
               var scrollBy = (new Point(WorkspaceWidth / 2.0, WorkspaceHeight / 2.0) - newZoomCenterPt) * ZoomFactor;

               scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + scrollBy.X);
               scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollBy.Y);
            }
         }
         else
         {
            _SetZoomFitPending = true;
         }
      }

      /// <summary>
      /// Sets the zoom factor to 1 (100%)
      /// </summary>
      public void SetZoom1()
      {
         SetZoomFactor(1.0);
      }

      /// <summary>
      /// Increases the zoom factor
      /// </summary>
      /// <param name="factor">Factor by which to zoom in (default is 25%)</param>
      /// <remarks>
      /// The center of the viewport is used a zoom center.
      /// </remarks>
      public void ZoomIn(double factor = 1.25)
      {
         SetZoomFactor(ZoomFactor * factor);
      }

      /// <summary>
      /// Increases the zoom factor
      /// </summary>
      /// <param name="zoomCenter">zoom center in screen coordinates</param>
      /// <param name="factor">Factor by which to zoom in (default is 25%)</param>
      public void ZoomIn(Point zoomCenter, double factor = 1.25)
      {
         SetZoomFactor(ZoomFactor * factor, zoomCenter);
      }

      /// <summary>
      /// Decreases the zoom factor
      /// </summary>
      /// <param name="factor">Factor by which to zoom out (default is 25%)</param>
      /// <remarks>
      /// The center of the viewport is used a zoom center.
      /// </remarks>
      public void ZoomOut(double factor = 1.25)
      {
         SetZoomFactor(ZoomFactor / factor);
      }

      /// <summary>
      /// Decreases the zoom factor
      /// </summary>
      /// <param name="zoomCenter">zoom center in screen coordinates</param>
      /// <param name="factor">Factor by which to zoom out (default is 25%)</param>
      public void ZoomOut(Point zoomCenter, double factor = 1.25)
      {
         SetZoomFactor(ZoomFactor / factor, zoomCenter);
      }

      /// <summary>
      /// Sets the zoom factor to a given value
      /// </summary>
      /// <param name="factor">New zoom factor</param>
      /// <remarks>
      /// The center of the viewport is used a zoom center.
      /// </remarks>
      /// <exception cref="ArgumentOutOfRangeException">Is thrown if the factor is less or equal to 0.</exception>
      public void SetZoomFactor(double factor)
      {
         if (factor <= 0.0)
         {
            throw new ArgumentOutOfRangeException("factor", "must be greater than 0");
         }
         SetZoomFactor(factor, new Point(scrollViewer.ViewportWidth / 2.0, scrollViewer.ViewportHeight / 2.0));
      }

      /// <summary>
      /// Sets the zoom factor to a given value
      /// </summary>
      /// <param name="factor">New zoom factor</param>
      /// <param name="zoomCenter">zoom center in screen coordinates</param>
      /// <exception cref="ArgumentOutOfRangeException">Is thrown if the factor is less or equal to 0.</exception>
      public void SetZoomFactor(double factor, Point zoomCenter)
      {
         if (factor <= 0.0)
         {
            throw new ArgumentOutOfRangeException("factor", "must be greater than 0");
         }

         _SetZoomFitPending = false; 
         
         var oldZoomCenterPt = scrollViewer.TranslatePoint(zoomCenter, workspace);

         ZoomFactor = factor;

         scrollViewer.UpdateLayout();
         var newZoomCenterPt = scrollViewer.TranslatePoint(zoomCenter, workspace);
         var scrollBy = (oldZoomCenterPt - newZoomCenterPt) * ZoomFactor;

         scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + scrollBy.X);
         scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollBy.Y);
      }

      /// <summary>
      /// Gets or sets the actual zoom factor
      /// </summary>
      public double ZoomFactor
      {
         get { return (double)GetValue(ZoomFactorProperty); }
         set { SetValue(ZoomFactorProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the ZoomFactor property
      /// </summary>
      public static readonly DependencyProperty ZoomFactorProperty =
          DependencyProperty.Register("ZoomFactor", typeof(double), typeof(XDrawEditor), new UIPropertyMetadata(1.0, ZoomFactorChanged));

      private static void ZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as XDrawEditor).ZoomFactorChanged(e);
      }

      private void ZoomFactorChanged(DependencyPropertyChangedEventArgs e)
      {
         UpdateZoom((double)e.NewValue);
      }

      private void UpdateZoom(double newZoom)
      {
         workspace.ScaleFactor = newZoom;
         workspace.LayoutTransform = new ScaleTransform(newZoom, newZoom);
         UpdateWorkspaceMargin();
      }

      private void UpdateWorkspaceMargin()
      {
         workspace.Margin = new Thickness(
            scrollViewer.ViewportWidth / 2.0, scrollViewer.ViewportHeight / 2.0,
            scrollViewer.ViewportWidth / 2.0, scrollViewer.ViewportHeight / 2.0);
      }



      private void ZoomInCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.Handled = true;
         e.CanExecute = true;
      }

      private void ZoomInCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         e.Handled = true;
         ZoomIn();
      }

      private void ZoomOutCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.Handled = true;
         e.CanExecute = true;
      }

      private void ZoomOutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         e.Handled = true;
         ZoomOut();
      }

      private void ZoomFitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.Handled = true;
         e.CanExecute = WorkspaceWidth > 0.0 && WorkspaceHeight > 0.0;
      }

      private void ZoomFitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         e.Handled = true;
         SetZoomFit();
      }

      private void Zoom1Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.Handled = true;
         e.CanExecute = true;
      }

      private void Zoom1Command_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         e.Handled = true;
         SetZoom1();
      }

      private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         if (_SetZoomFitPending)
         {
            _SetZoomFitPending = false;
            scrollViewer.UpdateLayout();
            SetZoomFit();
         }
         else
         {
            UpdateZoom(ZoomFactor);
         }
      }

      private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
      {
         if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
         {
            e.Handled = true;
            if (e.Delta > 0)
            {
               ZoomIn(e.GetPosition(scrollViewer));
            }
            else
            {
               ZoomOut(e.GetPosition(scrollViewer));
            }
         }
      }

      /// <summary>
      /// Gets or sets the style which is used for the rectangle around the selected items.
      /// </summary>
      /// <remarks>
      /// A value of null indicates that the default style should be used.
      /// The TargetType for the style must be XDraw.Controls.Media.ScreenSpaceRectangle2.
      /// </remarks>
      public static Style SelectionRectStyle { get; set; }

      /// <summary>
      /// Gets or sets the style which is used for the drag select rectangle or lasso shape.
      /// </summary>
      /// <remarks>
      /// A value of null indicates that the default style should be used.
      /// The TargetType for the style must be XDraw.Controls.Media.ScreenSpaceShape.
      /// </remarks>
      public static Style MouseSelectShapeStyle { get; set; }

      /// <summary>
      /// Gets or sets the minimum offset in screen coordinates by which the mouse can move without starting an drag operation.
      /// </summary>
      public double MinDragOffset
      {
         get { return (double)GetValue(MinDragOffsetProperty); }
         set { SetValue(MinDragOffsetProperty, value); }
      }

      /// <summary>
      /// DependencyProperty for the MinDragOffset property
      /// </summary>
      public static readonly DependencyProperty MinDragOffsetProperty =
          DependencyProperty.Register("MinDragOffset", typeof(double), typeof(XDrawEditor), new UIPropertyMetadata(2.0));



      private Companion _MouseDownCompanion = null;
      private Gripper _MouseDownGripper = null;
      private Gripper _MouseDownCustomGripper = null;
      private Point _MouseDownGripperPoint = new Point();
      private Point _MouseDownPoint = new Point();

      private void DrawingCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         var p = e.GetPosition(scrollViewer);
         if (p.X < 0.0 || p.X > scrollViewer.ViewportWidth ||
            p.Y < 0.0 || p.Y > scrollViewer.ViewportHeight)
         {
            return;
         }

         Focus();

         e.Handled = true;
         scrollViewer.CaptureMouse();

         _DragAction = DragAction.None;
         _MouseDownPoint = e.GetPosition(workspace);
         _MouseDownCompanion = null;
         _MouseDownGripper = null;
         _MouseDownCustomGripper = null;

         foreach (var gripper in _CustomGrippers)
         {
            if (gripper.IsHit(_MouseDownPoint))
            {
               _MouseDownCustomGripper = gripper;
               _MouseDownGripperPoint = gripper.Position;
               break;
            }
         }

         if (_MouseDownCustomGripper == null)
         {
            foreach (var gripper in _SelectionRectGrippers)
            {
               if (gripper.IsHit(_MouseDownPoint))
               {
                  _MouseDownGripper = gripper;
                  _MouseDownGripperPoint = gripper.Position;
                  break;
               }
            }
         }

         if (_MouseDownGripper == null && _MouseDownCustomGripper == null)
         {
            HitTestResult htr = null;
            VisualTreeHelper.HitTest(workspace, null,
               _ =>
               {
                  if ((_.VisualHit as FrameworkElement).DataContext is Companion &&
                     (_ as GeometryHitTestResult).IntersectionDetail != IntersectionDetail.Empty)
                  {
                     htr = _;
                     return HitTestResultBehavior.Stop;
                  }
                  return HitTestResultBehavior.Continue;
               },
               new GeometryHitTestParameters(
                  new EllipseGeometry(_MouseDownPoint, MinDragOffset / ZoomFactor, MinDragOffset / ZoomFactor)));

            if (htr != null)
            {
               var frameworkElement = htr.VisualHit as FrameworkElement;
               if (frameworkElement != null)
               {
                  _MouseDownCompanion = frameworkElement.DataContext as Companion;
               }
            }
         }
      }

      private enum SelectionOperation
      {
         None,
         Set,
         Add,
         Toggle
      }

      private void DrawingCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
      {
         _AutoScrollX = 0.0;
         _AutoScrollY = 0.0;

         if (scrollViewer.IsMouseCaptured)
         {
            scrollViewer.ReleaseMouseCapture();
         }
         else
         {
            return;
         }

         e.Handled = true;

         var selectionCompanions = new List<Companion>();
         SelectionOperation selectionOperation;
         if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
         {
            selectionOperation = SelectionOperation.Add;
         }
         else if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
         {
            selectionOperation = SelectionOperation.Toggle;
         }
         else
         {
            selectionOperation = SelectionOperation.Set;
         }

         var point = e.GetPosition(workspace);

         if (_DragAction == DragAction.None)
         {
            // this is a click
            if (_MouseDownCompanion != null)
            {
               if ((SelectionMode & SelectionModes.Click) != 0)
               {
                  if (_MouseDownCompanion.IsSelected && selectionOperation == SelectionOperation.Set)
                  {
                     selectionOperation = SelectionOperation.None;
                  }
                  else
                  {
                     selectionCompanions.Add(_MouseDownCompanion);
                  }
               }
               else
               {
                  selectionOperation = SelectionOperation.None;
               }
            }
         }
         else if (_DragAction == DragAction.Selection)
         {
            if ((SelectionMode & SelectionModes.Rectangle) != 0)
            {
               FinishRectSelect(_MouseDownPoint, point, selectionCompanions);
            }
            else if ((SelectionMode & SelectionModes.Lasso) != 0)
            {
               FinishLassoSelect(point, selectionCompanions);
            }
         }
         else if (_DragAction == DragAction.Gripper)
         {
            if (_MouseDownGripper != null)
            {
               HandleGripperDragEnd(point);
            }
            else if (_MouseDownCustomGripper != null)
            {
               HandleCustomGripperDragEnd(point);
            }
            selectionOperation = SelectionOperation.None;
         }
         else if (_DragAction == DragAction.Items)
         {
            HandleDragItemsEnd(point);
            selectionOperation = SelectionOperation.None;
         }

         if (selectionOperation == SelectionOperation.Set)
         {
            foreach (var companion in _SelectedCompanions.ToList())
            {
               companion.IsSelected = false;
            }
         }

         foreach (var companion in selectionCompanions)
         {
            switch (selectionOperation)
            {
               case SelectionOperation.Set:
               case SelectionOperation.Add:
                  companion.IsSelected = true;
                  break;

               case SelectionOperation.Toggle:
                  companion.ToggleSelection();
                  break;
            }
         }

         _MouseDownCompanion = null;
         _MouseDownGripper = null;
         _MouseDownCustomGripper = null;
         _DragAction = DragAction.Ignore;
      }

      private enum DragAction
      {
         Ignore,
         None,
         Selection,
         Items,
         Gripper
      }

      private double _AutoScrollX = 0.0;
      private double _AutoScrollY = 0.0;
      private object _ScrollTimerLock = new Object();
      private Timer _ScrollTimer = null;
      private delegate void NoArgDelegate();

      private DragAction _DragAction = DragAction.Ignore;

      private Rect _ResizeStartBounds = new Rect();

      private void DrawingCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
      {
         if (!scrollViewer.IsMouseCaptured)
         {
            return;
         }
         e.Handled = true;

         if (_DragAction != DragAction.Ignore && _DragAction != DragAction.None)
         {
            HandleAutoScroll(e.GetPosition(scrollViewer));
         }
         else
         {
            _AutoScrollX = 0.0;
            _AutoScrollY = 0.0;
         }

         var point = e.GetPosition(workspace);

         if (_DragAction == DragAction.None)
         {
            if ((point - _MouseDownPoint).Length > MinDragOffset / ZoomFactor)
            {
               if (_MouseDownGripper != null)
               {
                  _DragAction = DragAction.Gripper;
                  HandleGripperDragStart();
                  HandleGripperDragUpdate(point);
               }
               else if (_MouseDownCustomGripper != null)
               {
                  _DragAction = DragAction.Gripper;
                  HandleCustomGripperDragStart();
                  HandleCustomGripperDragUpdate(point);
               }
               else if (_MouseDownCompanion != null)
               {
                  if (!_MouseDownCompanion.IsSelected)
                  {
                     // switch selection to draged object
                     foreach (var companion in _SelectedCompanions.ToList())
                     {
                        companion.IsSelected = false;
                     }

                     _MouseDownCompanion.IsSelected = true;
                  }

                  bool canDragItems = true;
                  foreach (var companion in _SelectedCompanions)
                  {
                     if (!companion.Movable)
                     {
                        canDragItems = false;
                        break;
                     }
                  }

                  if (canDragItems)
                  {
                     HandleDragItemsStart();
                     HandleDragItemsUpdate(point);
                  }
               }
               else
               {
                  if ((SelectionMode & SelectionModes.Rectangle) != 0)
                  {
                     StartRectSelect(_MouseDownPoint, point);
                     _DragAction = DragAction.Selection;
                  }
                  else if ((SelectionMode & SelectionModes.Lasso) != 0)
                  {
                     StartLassoSelect(_MouseDownPoint, point);
                     _DragAction = DragAction.Selection;
                  }
               }
            }
         }
         else if (_DragAction == DragAction.Selection)
         {
            if ((SelectionMode & SelectionModes.Rectangle) != 0)
            {
               UpdateRectSelect(_MouseDownPoint, point);
            }
            else if ((SelectionMode & SelectionModes.Lasso) != 0)
            {
               UpdateLassoSelect(point);
            }
         }
         else if (_DragAction == DragAction.Gripper)
         {
            if (_MouseDownGripper != null)
            {
               HandleGripperDragUpdate(point);
            }
            else if (_MouseDownCustomGripper != null)
            {
               HandleCustomGripperDragUpdate(point);
            }
         }
         else if (_DragAction == DragAction.Items)
         {
            HandleDragItemsUpdate(point);
         }
      }

      private ScreenSpaceRectangle2 _RectSelectShape = null;

      private void StartRectSelect(Point point1, Point point2)
      {
         if (_RectSelectShape != null)
         {
            _RectSelectShape.Visibility = Visibility.Collapsed;
            foregroundCanvas.Children.Remove(_RectSelectShape);
            _RectSelectShape = null;
         }
         _RectSelectShape = new ScreenSpaceRectangle2()
         {
            Style = MouseSelectShapeStyle,
            ScreenSpaceSize = false
         };

         UpdateRectSelect(point1, point2);

         foregroundCanvas.Children.Add(_RectSelectShape);

         _RectSelectShape.Visibility = Visibility.Visible;
      }

      private void UpdateRectSelect(Point point1, Point point2)
      {
         if (_RectSelectShape != null)
         {
            _RectSelectShape.X = Math.Min(point1.X, point2.X);
            _RectSelectShape.Y = Math.Min(point1.Y, point2.Y);
            _RectSelectShape.RectWidth = Math.Abs(point1.X - point2.X);
            _RectSelectShape.RectHeight = Math.Abs(point1.Y - point2.Y);
         }
      }

      private void FinishRectSelect(Point point1, Point point2, List<Companion> selectionCompanions)
      {
         if (_RectSelectShape != null)
         {
            _RectSelectShape.Visibility = Visibility.Collapsed;
            foregroundCanvas.Children.Remove(_RectSelectShape);
            _RectSelectShape = null;
         }

         VisualTreeHelper.HitTest(workspace, null,
            _ =>
            {
               var frameworkElement = _.VisualHit as FrameworkElement;
               if (frameworkElement != null)
               {
                  if (frameworkElement.DataContext is Companion &&
                  ((((SelectionMode & SelectionModes.IntercesctionSelects) != 0) && (_ as GeometryHitTestResult).IntersectionDetail != IntersectionDetail.Empty) ||
                   (((SelectionMode & SelectionModes.IntercesctionSelects) == 0) && (_ as GeometryHitTestResult).IntersectionDetail == IntersectionDetail.FullyInside)))
                  {                  
                     selectionCompanions.Add(frameworkElement.DataContext as Companion);
                  }
               }
               return HitTestResultBehavior.Continue;

            },
            new GeometryHitTestParameters(
               new RectangleGeometry(new Rect(point1, point2))));
      }


      private ScreenSpacePolygon _LassoSelectShape = null;

      private void StartLassoSelect(Point point1, Point point2)
      {
         if (_LassoSelectShape != null)
         {
            _LassoSelectShape.Visibility = Visibility.Collapsed;
            foregroundCanvas.Children.Remove(_LassoSelectShape);
            _LassoSelectShape = null;
         }
         _LassoSelectShape = new ScreenSpacePolygon()
         {
            Style = MouseSelectShapeStyle
         };

         _LassoSelectShape.Points = new PointCollection();
         _LassoSelectShape.Points.Add(point1);
         _LassoSelectShape.Points.Add(point2);

         foregroundCanvas.Children.Add(_LassoSelectShape);

         _LassoSelectShape.Visibility = Visibility.Visible;
      }

      private void UpdateLassoSelect(Point point)
      {
         if (_LassoSelectShape != null)
         {
            if ((point - _LassoSelectShape.Points[_LassoSelectShape.Points.Count - 1]).Length * ZoomFactor >= 2.0)
            {
               _LassoSelectShape.Points.Add(point);
            }
         }
      }

      private void FinishLassoSelect(Point point, List<Companion> selectionCompanions)
      {
         if (_LassoSelectShape != null)
         {
            _LassoSelectShape.Points.Add(point);
            VisualTreeHelper.HitTest(workspace, null,
               _ =>
               {
                  var frameworkElement = _.VisualHit as FrameworkElement;
                  if (frameworkElement != null)
                  {
                     if (frameworkElement.DataContext is Companion &&
                        ((((SelectionMode & SelectionModes.IntercesctionSelects) != 0) && (_ as GeometryHitTestResult).IntersectionDetail != IntersectionDetail.Empty) ||
                         (((SelectionMode & SelectionModes.IntercesctionSelects) == 0) && (_ as GeometryHitTestResult).IntersectionDetail == IntersectionDetail.FullyInside)))
                     {
                        selectionCompanions.Add(frameworkElement.DataContext as Companion);
                     }
                  }
                  return HitTestResultBehavior.Continue;

               },
               new GeometryHitTestParameters(
                  _LassoSelectShape.RenderedGeometry));

            _LassoSelectShape.Visibility = Visibility.Collapsed;
            foregroundCanvas.Children.Remove(_LassoSelectShape);
            _LassoSelectShape = null;
         }
      }

      private void DeleteSelectedCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.Handled = true;
         e.CanExecute = _SelectedCompanions.Count > 0;
      }

      private void DeleteSelectedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         e.Handled = true;
         if (_SelectedCompanions.Count > 0)
         {
            var selectedItems = new List<object>();
            foreach (var companion in _SelectedCompanions)
            {
               selectedItems.Add(companion.Item);
            }
            _SelectedCompanions.Clear();
            UpdateSelectionRect();
            OnSelectionChanged();
            UpdateCustomGrippers();

            if (!_ItemsSupportsCollectionChanged)
            {
               RemoveCompanions(selectedItems);
            }
            foreach (var item in selectedItems)
            {
               Items.Remove(item);
            }

            UpdateWorkspaceSize();
            SetModified();
         }
      }

      private bool _IsModified = false;

      public bool IsModified
      {
         get { return _IsModified; }
      }

      private void SetModified(bool isModified = true)
      {
         _IsModified = isModified;
         NotifyPropertyChanged("IsModified");
      }

      #region INotifyPropertyChanged Members

      private void NotifyPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;
         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      private void Workspace_DragOver(object sender, DragEventArgs e)
      {
         if ((e.AllowedEffects & DragDropEffects.Copy) != 0 && e.Data.GetDataPresent(XDrawCore.DragDropItemType))
         {
            e.Effects = DragDropEffects.Copy;
         }
      }

      private void Workspace_Drop(object sender, DragEventArgs e)
      {
         if ((e.AllowedEffects & DragDropEffects.Copy) != 0 && e.Data.GetDataPresent(XDrawCore.DragDropItemType))
         {
            var newItemType = e.Data.GetData(XDrawCore.DragDropItemType) as INewItemType;
            if (newItemType != null)
            {
               Point position;
               if (Items.Count == 0)
               {
                  position = new Point(0.0, 0.0);
               }
               else
               {
                  position = e.GetPosition(workspace);
               }
               var newItem = newItemType.CreateNewItem(position, ItemCreateMode.Drop);

               Items.Add(newItem);
               if (!_ItemsSupportsCollectionChanged)
               {
                  CreateCompanion(newItem);
               }

               foreach (var cmp in _SelectedCompanions.ToList())
               {
                  cmp.IsSelected = false;
               }
               _SelectedCompanions.Clear();

               var companion = GetItemCompanion(newItem);
               if (companion != null && companion.Selectable)
               {
                  companion.IsSelected = true;
               }

               UpdateWorkspaceSize();
               UpdateSelectionRect();
               OnSelectionChanged();
               UpdateCustomGrippers();
            }
         }
      }
   }
}
