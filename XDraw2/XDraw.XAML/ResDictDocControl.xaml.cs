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
using System.Xml.Linq;

namespace XDraw.Xaml
{
   /// <summary>
   /// Interaction logic for ResDictDocControl.xaml
   /// </summary>
   public partial class ResDictDocControl : UserControl, INotifyPropertyChanged, IDockDocument
   {
      public ResDictDocControl(object context, string id)
      {
         _Context = context;
         _DocumentId = id;

         AddDrawingCommand = new CallbackCommand(AddDrawing_Execute);

         // make sure resources where instanciated for sure
         var dummy = ResDictDocResources.Instance;

         InitializeComponent();
      }

      public CallbackCommand AddDrawingCommand { get; private set; }

      private void AddDrawing_Execute(object parameter)
      {
         SelectedItem = Document.AddNewDrawing();
      }

      private object _Context;

      private string _DocumentId;

      public ResDictDoc Document
      {
         get { return (ResDictDoc)GetValue(DocumentProperty); }
         set { SetValue(DocumentProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty DocumentProperty =
         DependencyProperty.Register("Document", typeof(ResDictDoc), typeof(ResDictDocControl), new UIPropertyMetadata(null, OnDocumentChanged));

      private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         (d as ResDictDocControl).OnDocumentChanged(e);
      }

      private void OnDocumentChanged(DependencyPropertyChangedEventArgs e)
      {
         NotifyPropertyChanged("DocumentTitle");
         NotifyPropertyChanged("DocumentToolTip");
         NotifyPropertyChanged("IsDocumentModified");

         var oldDoc = e.OldValue as ResDictDoc;
         if (oldDoc != null)
         {
            oldDoc.PropertyChanged -= Document_PropertyChanged;
         }
         var newDoc = e.NewValue as ResDictDoc;
         if (newDoc != null)
         {
            newDoc.PropertyChanged += Document_PropertyChanged;
         }
      }

      private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         NotifyPropertyChanged("IsDocumentModified");
      }



      public ResDictElement SelectedItem
      {
         get { return (ResDictElement)GetValue(SelectedItemProperty); }
         set { SetValue(SelectedItemProperty, value); }
      }

      // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SelectedItemProperty =
          DependencyProperty.Register("SelectedItem", typeof(ResDictElement), typeof(ResDictDocControl), new UIPropertyMetadata(null));



      private void ListBoxItem_DoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
      {
         var lbi = sender as ListBoxItem;
         if (lbi != null)
         {
            var element = lbi.DataContext as ResDictElement;
            if (element != null)
            {
               if (element.ElementType == ResDictElementTypes.DrawingImage)
               {
                  XDrawCore.Instance.DockManager.ShowDrawing(element as IXDrawDrawing);
               }
            }
         }
      }

      #region IDockDocument Members

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
         get { return Document == null ? String.Empty : Document.Name; }
      }

      public string DocumentToolTip
      {
         get { return Document == null ? String.Empty : Document.FileName; }
      }

      public ImageSource DocumentIcon
      {
         get { return null; }
      }

      public IInputElement GetDocumentDefaultElement()
      {
         return listBox;
      }

      public void DocumentClosing(ref bool cancel)
      { }

      public void DocumentClosed()
      { }

      public bool IsDocumentModified
      {
         get { return Document != null ? Document.IsModified : false; }
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

      private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedItem != null)
         {
            SelectedItem.CopyToClipboard();
            Document.RemoveItem(SelectedItem);
         }
      }

      private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedItem != null;
      }

      private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedItem != null)
         {
            SelectedItem.CopyToClipboard();
         }
      }

      private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedItem != null;
      }

      private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (Clipboard.ContainsText())
         {
            try
            {
               Document.AddFromClipboard();
            }
            catch (Exception ex)
            {
               MessageBox.Show(Application.Current.MainWindow,
                  "Paste failed:\n" + ex.Message,
                  "Paste", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }

      private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = Clipboard.ContainsText();
      }

      private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedItem != null)
         {
            Document.RemoveItem(SelectedItem);
         }
      }

      private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedItem != null;
      }
   }
}
