using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Reflection;
using Xceed.Wpf.AvalonDock;

namespace XDraw
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, IDockManager, INotifyPropertyChanged
   {
      public MainWindow()
      {
         Documents = new ObservableCollection<IDockDocument>();
         ToolPanes = new ObservableCollection<IDockPane>();

         XDrawCore.Instance.DockManager = this;

         ToolPanes.Add(new PropertiesToolPane(null, "PropertiesPane"));

         _ToolboxPane = new ToolboxPane(null, "ToolboxPane");
         ToolPanes.Add(_ToolboxPane);

         ParsePluginAssembly("XDraw.Xaml");

         InitializeComponent();
      }

      private ToolboxPane _ToolboxPane;

      private void Window_Activated(object sender, EventArgs e)
      {
         // by this the shortcuts of the bound window command work
         this.Focus();
      }

      public IList<IDockPane> ToolPanes { get; private set; }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         var resolver = _FileTypeResolvers.FirstOrDefault();
         if (resolver != null)
         {
            resolver.CreateNewFile();
         }
      }

      private void New_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         var resolver = _FileTypeResolvers.FirstOrDefault();
         if (resolver != null)
         {
            resolver.CreateNewFile();
         }
      }

      private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         var dlg = new OpenFileDialog();
         dlg.Title = "OpenFile";
         dlg.Filter = CreateOpenFileFilter();

         if (dlg.ShowDialog(this) == true)
         {
            OpenFile(dlg.FileName);
         }
      }

      private string _OpenFileFilter = String.Empty;

      private string CreateOpenFileFilter()
      {
         if (String.IsNullOrEmpty(_OpenFileFilter))
         {
            var filter = new StringBuilder();
            if (_FileTypeResolvers.Count > 1)
            {
               var allExtensions = new Dictionary<string, string>();
               foreach (var resolver in _FileTypeResolvers)
               {
                  foreach (var ext in resolver.FileExtensions)
                  {
                     if (!allExtensions.ContainsKey(ext.ToLowerInvariant()))
                     {
                        allExtensions.Add(ext.ToLowerInvariant(), ext);
                     }
                  }
               }

               filter.Append("All XDraw files (");
               bool first = true;
               foreach (var ext in allExtensions.Values)
               {
                  if (first)
                  {
                     first = false;
                  }
                  else
                  {
                     filter.Append(", ");
                  }
                  filter.Append("*");
                  filter.Append(ext);
               }
               filter.Append(")|");
               first = true;
               foreach (var ext in allExtensions.Values)
               {
                  if (first)
                  {
                     first = false;
                  }
                  else
                  {
                     filter.Append(", ");
                  }
                  filter.Append("*");
                  filter.Append(ext);
               }
               filter.Append("|");
            }

            foreach (var resolver in _FileTypeResolvers)
            {
               filter.Append(resolver.FileTypeName);
               filter.Append(" (");
               bool first = true;
               foreach (var ext in resolver.FileExtensions)
               {
                  if (first)
                  {
                     first = false;
                  }
                  else
                  {
                     filter.Append(", ");
                  }
                  filter.Append("*");
                  filter.Append(ext);
               }
               filter.Append(")|");
               first = true;
               foreach (var ext in resolver.FileExtensions)
               {
                  if (first)
                  {
                     first = false;
                  }
                  else
                  {
                     filter.Append(", ");
                  }
                  filter.Append("*");
                  filter.Append(ext);
               }
            }

            filter.Append("|All files (*.*)|*.*");
            _OpenFileFilter = filter.ToString();
         }
         return _OpenFileFilter;
      }

      private void Exit_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

      private List<IFileTypeResolver> _FileTypeResolvers = new List<IFileTypeResolver>();

      private void ParsePluginAssembly(string assemblyName)
      {
         try
         {
            var assembly = Assembly.Load(assemblyName);
            var resolverType = typeof(IFileTypeResolver);
            foreach (var type in assembly.GetTypes())
            {
               if (type.IsClass && !type.IsAbstract && resolverType.IsAssignableFrom(type))
               {
                  _FileTypeResolvers.Add(assembly.CreateInstance(type.FullName) as IFileTypeResolver);
               }
            }
         }
         catch
         { }
      }

      public IDockDocument OpenFile(string path)
      {
         try
         {
            foreach (var resolver in _FileTypeResolvers)
            {
               if (resolver.CanOpenFile(path))
               {
                  foreach (var doc in Documents)
                  {
                     if (resolver.IsSameFile(doc, path))
                     {
                        ActiveDocument = doc;
                        MessageBox.Show(this, "The file is already open!", "OpenFile", MessageBoxButton.OK, MessageBoxImage.Information);
                        return doc;
                     }
                  }

                  return resolver.OpenFile(path);
               }
            }
            MessageBox.Show(this, "Can not open this file!", "OpenFile", MessageBoxButton.OK, MessageBoxImage.Error);
         }
         catch (Exception ex)
         {
            MessageBox.Show(this, "Failed to open file:\n" + ex.Message, "OpenFile", MessageBoxButton.OK, MessageBoxImage.Error);
         }
         return null;
      }

      public IDockDocument FindDocument(string id)
      {
         foreach (var doc in Documents)
         {
            if (String.CompareOrdinal(doc.DocumentId, id) == 0)
            {
               return doc;
            }
         }
         return null;
      }

      public IDockDocument ShowDocument(string id, CreateDockDocumentDelegate createDockCallback)
      {
         IDockDocument doc = FindDocument(id);

         if (doc == null)
         {
            doc = createDockCallback(null, id);
            Documents.Add(doc);
         }

         ActiveDocument = doc;

         return doc;
      }

      private int _NextId = 0;

      public IDockDocument ShowDrawing(IXDrawDrawing drawing)
      {
         foreach (var doc in Documents)
         {
            var editorDoc = doc as XDrawEditorDoc;
            if (editorDoc != null && String.CompareOrdinal(editorDoc.Drawing.UniqueId, drawing.UniqueId) == 0)
            {
               ActiveDocument = doc;
               return doc;
            }
         }
         return ShowDocument(String.Format("Drawing{0}", _NextId++), (context, id) =>
            {
               return new XDrawEditorDoc(context, id) { Drawing = drawing };
            });
      }

      public IList<IDockDocument> Documents { get; private set; }

      private IDockDocument _ActiveDocument = null;

      public IDockDocument ActiveDocument 
      {
         get { return _ActiveDocument; }
         set
         {
            if (_ActiveDocument != value)
            {
               BindingOperations.ClearBinding(_ToolboxPane, ToolboxPane.ToolboxItemsProperty);

               _ActiveDocument = value;

               // correct focus
               if (_ActiveDocument != null && _ActiveDocument.GetDocumentDefaultElement() != null && _ActiveDocument.GetDocumentDefaultElement().Focusable)
               {
                  try
                  {
                     _ActiveDocument.GetDocumentDefaultElement().Focus();
                  }
                  catch { }
               }

               var toolboxSource = _ActiveDocument as IToolboxItemsSource;
               if (toolboxSource != null)
               {
                  var binding = new Binding("ToolboxItems")
                     {
                        Source = toolboxSource
                     };
                  BindingOperations.SetBinding(_ToolboxPane, ToolboxPane.ToolboxItemsProperty, binding);
               }

               NotifyPropertyChanged("ActiveDocument");
            }
         }
      }      

      public void CloseDocument(IDockDocument doc)
      {
         bool cancel = false;
         doc.DocumentClosing(ref cancel);
         if (!cancel)
         {
            Documents.Remove(doc);
            doc.DocumentClosed();
         }
      }

      public void CloseDocument(string id)
      {
         var doc = FindDocument(id);
         if (doc != null)
         {
            CloseDocument(doc);
         }
      }

      private void NotifyPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;
         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void DockManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
      {
         var doc = e.Document.Content as IDockDocument;
         if (doc != null)
         {
            bool cancel = e.Cancel;
            doc.DocumentClosing(ref cancel);
            e.Cancel = cancel;
         }
      }

      private void DockManager_DocumentClosed(object sender, DocumentClosedEventArgs e)
      {
         var doc = e.Document.Content as IDockDocument;
         if (doc != null)
         {
            doc.DocumentClosed();
         }
         if (Documents.Contains(doc))
         {
            Documents.Remove(doc);
         }
      }
    }
}
