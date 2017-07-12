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
using System.ComponentModel;
using XDraw.Editor;

namespace XDraw
{
   /// <summary>
   /// Interaction logic for XDrawEditorDoc.xaml
   /// </summary>
   public partial class XDrawEditorDoc : UserControl, IDockDocument, INotifyPropertyChanged, IToolboxItemsSource
   {
      public XDrawEditorDoc(object context, string documentId)
      {
         _Context = context;
         _DocumentId = documentId;

         var dummy = XDrawEditorDocResources.Instance;

         InitializeComponent();
      }

      public IXDrawDrawing Drawing
      {
         get { return (IXDrawDrawing)GetValue(DrawingProperty); }
         set { SetValue(DrawingProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Drawing.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty DrawingProperty =
          DependencyProperty.Register("Drawing", typeof(IXDrawDrawing), typeof(XDrawEditorDoc), new UIPropertyMetadata(null, Drawing_Changed));

      private static void Drawing_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as XDrawEditorDoc).Drawing_Changed(e);
      }

      private void Drawing_Changed(DependencyPropertyChangedEventArgs e)
      {
         var oldDrawing = e.OldValue as IXDrawDrawing;
         if (oldDrawing != null)
         {
            oldDrawing.Companions = null;
         }

         var newDrawing = e.NewValue as IXDrawDrawing;
         if (newDrawing != null)
         {
            _Editor.CompanionTypeResolver = newDrawing.CompanionTypeResolver;
            _Editor.Items = newDrawing.Items;
            newDrawing.Companions = _Editor.Companions;
            _Editor.PropertyChanged += Editor_PropertyChanged;
         }
         else
         {
            _Editor.Items = null;
            _Editor.CompanionTypeResolver = null;
         }
         NotifyPropertyChanged("DocumentTitle");
         NotifyPropertyChanged("DocumentToolTip");
         NotifyPropertyChanged("IsDocumentModified");
         NotifyPropertyChanged("IsDocumentLocked");
         NotifyPropertyChanged("ToolboxItems");
      }

      private void Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (Drawing != null && e.PropertyName == "IsModified")
         {
            if (_Editor.IsModified)
            {
               Drawing.SetModified();
            }
            NotifyPropertyChanged("IsDocumentModified");
         }
      }

      public XDrawEditor Editor
      {
         get { return _Editor; }
      }



      public bool LassoSelectMode
      {
         get { return (bool)GetValue(LassoSelectModeProperty); }
         set { SetValue(LassoSelectModeProperty, value); }
      }

      // Using a DependencyProperty as the backing store for LassoSelectMode.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty LassoSelectModeProperty =
          DependencyProperty.Register("LassoSelectMode", typeof(bool), typeof(XDrawEditorDoc), new UIPropertyMetadata(false, SelectMode_Changed));

      private static void SelectMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as XDrawEditorDoc).SelectMode_Changed(e);
      }

      private void SelectMode_Changed(DependencyPropertyChangedEventArgs e)
      {
         if ((bool)e.NewValue)
         {
            _Editor.SelectionMode = SelectionModes.LassoClick;
         }
         else
         {
            _Editor.SelectionMode = SelectionModes.RectangleClick;
         }
      }


      #region IDockDocument Members

      private object _Context;
      private string _DocumentId;

      public object DocumentContext
      {
         get { return _Context; }
      }

      public string DocumentId
      {
         get { return _DocumentId; }
      }

      public object DocumentContent
      {
         get { return this; }
      }

      public string DocumentTitle
      {
         get { return Drawing != null ? Drawing.Name : String.Empty; }
      }

      public string DocumentToolTip
      {
         get { return Drawing != null ? Drawing.Description : String.Empty; }
      }

      private ImageSource _DocumentIcon = null;

      public ImageSource DocumentIcon
      {
         get 
         {
            if (_DocumentIcon == null)
            {
               try
               {
                  var resDict = new ResourceDictionary();
                  resDict.Source = new Uri("/XDraw2;component/ImageDictionary.xaml", UriKind.RelativeOrAbsolute);
                  _DocumentIcon = resDict["DrawingImage"] as ImageSource;
               }
               catch { }
            }
            return _DocumentIcon;
         }
      }

      public IInputElement GetDocumentDefaultElement()
      {
         return _Editor;
      }

      public void DocumentClosing(ref bool cancel)
      {
         
      }

      public void DocumentClosed()
      {
         
      }

      public bool IsDocumentModified
      {
         get { return _Editor.IsModified; }
      }

      public bool IsDocumentLocked
      {
         get { return false; }
      }

      #endregion

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

      #region IToolboxItemsSource Members

      public IEnumerable<INewItemType> ToolboxItems
      {
         get { return Drawing != null ? Drawing.CompanionTypeResolver.NewItemTypes : null; }
      }

      #endregion
   }
}
