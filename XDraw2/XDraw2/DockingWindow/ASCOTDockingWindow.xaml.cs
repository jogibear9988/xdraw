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
using System.Windows.Shapes;
using remes.Windows;
using AvalonDock;
using System.ComponentModel;
using remes.Windows.Data;
using System.Threading;
using remes.Windows.Threading;

namespace remes.ASCOT.AdvancedServices.DockingWindow
{
   /// <summary>
   /// Interaction logic for ASCOTMainWindow.xaml
   /// </summary>
   public partial class ASCOTDockingWindow : Window, IDockWindow, INotifyPropertyChanged
   {
      public ASCOTDockingWindow()
      {
         this.RegisterWithLocationManager();

         InitializeComponent();

         if (DockingWindowService.Instance != null)
         {
            DockingWindowService.Instance.DockingWindow = this;
         }
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         if (!ASCOTCore.Instance.RequestShutdown(ASCOTShutdownReason.ExitASCOT, "MainWindow closing"))
         {
            e.Cancel = true;
         }
      }

      private void Window_Closed(object sender, EventArgs e)
      {

      }

      private void RemesLogo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         ASCOTCore.Instance.ShowAboutDialog(this);
      }

      private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
      {
         ASCOTCore.Instance.ShowAboutDialog(this);
      }

      private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

      private DocumentContent FindAvalonDocument(string name)
      {
         foreach (var avalonDoc in dockingManager.Documents)
         {
            if (avalonDoc.Name == name)
            {
               return avalonDoc;
            }
         }
         return null;
      }

      private DocumentContent FindAvalonDocument(IDockDocument doc)
      {
         foreach (var avalonDoc in dockingManager.Documents)
         {
            if (Object.ReferenceEquals(avalonDoc.Content, doc))
            {
               return avalonDoc;
            }
         }
         return null;
      }

      public IDockDocument FindDocument(string name)
      {
         var avalonDoc = FindAvalonDocument(name);
         if (avalonDoc != null)
         {
            return avalonDoc.Content as IDockDocument;
         }
         return null;
      }

      public IDockDocument ShowDocument(string name, CreateDockDocumentDelegate createDockCallback)
      {
         IDockDocument doc = null;
         var avalonDoc = FindAvalonDocument(name);

         if (avalonDoc == null)
         {
            if (String.CompareOrdinal(name, "startPage") != 0 && StartPageDockDocument.GlobalAutoClose)
            {
               StartPageDockDocument.GlobalAutoClose = false;
               CloseDocument("startPage");
            }

            avalonDoc = new DocumentContent()
            {
               Name = name
            };
            doc = createDockCallback(avalonDoc, name);

            BindingExtensions.CreateBinding(doc, "DocumentTitle", avalonDoc, DocumentContent.TitleProperty, BindingMode.OneWay);

            avalonDoc.Content = doc;

            avalonDoc.Closing += AvalonDoc_Closing;
            avalonDoc.Closed += AvalonDoc_Closed;
         }

         avalonDoc.Show(dockingManager, false);

         Dispatcher.BeginInvoke(new NoArgsDelegate(() => { avalonDoc.Manager.ActiveDocument = avalonDoc; }), null);

         return doc;
      }

      private void AvalonDoc_Closing(object sender, CancelEventArgs e)
      {
         var doc = (sender as DocumentContent).Content as IDockDocument;
         if (doc != null)
         {
            bool cancel = e.Cancel;
            doc.DocumentClosing(ref cancel);
            e.Cancel = cancel;
         }
      }

      private void AvalonDoc_Closed(object sender, EventArgs e)
      {
         var avalonDoc = sender as DocumentContent;
         avalonDoc.Closing -= AvalonDoc_Closing;
         avalonDoc.Closed -= AvalonDoc_Closed;

         var doc = avalonDoc.Content as IDockDocument;
         if (doc != null)
         {
            doc.DocumentClosed();
         }
      }

      public IDockDocument ActiveDocument
      {
         get
         {
            if (dockingManager.ActiveDocument != null)
            {
               return dockingManager.ActiveDocument.Content as IDockDocument;
            }
            return null;
         }
         set
         {
            if (value == null)
            {
               dockingManager.ActiveDocument = null;
            }
            else
            {
               dockingManager.ActiveDocument = FindAvalonDocument(value);
            }
         }
      }

      public string GetDocumentName(IDockDocument doc)
      {
         var avalonDock = doc.DocumentContext as DocumentContent;
         if (avalonDock == null)
         {
            throw new ArgumentException("doc is not a valid document");
         }
         return avalonDock.Name;
      }

      public IEnumerable<IDockDocument> Documents
      {
         get
         {
            foreach (var avalonDoc in dockingManager.Documents)
            {
               var doc = avalonDoc.Content as IDockDocument;
               if (doc != null)
               {
                  yield return doc;
               }
            }
         }
      }

      public void CloseDocument(IDockDocument doc)
      {
         var avalonDock = FindAvalonDocument(doc);
         if (avalonDock != null)
         {
            avalonDock.Close();
         }
      }

      public void CloseDocument(string name)
      {
         var avalonDock = FindAvalonDocument(name);
         if (avalonDock != null)
         {
            avalonDock.Close();
         }
      }

      private DockableContent FindAvalonPane(string name)
      {
         foreach (var avalonDoc in dockingManager.DockableContents)
         {
            if (avalonDoc.Name == name)
            {
               return avalonDoc;
            }
         }
         return null;
      }

      private DockableContent FindAvalonPane(IDockPane pane)
      {
         foreach (var avalonDoc in dockingManager.DockableContents)
         {
            if (Object.ReferenceEquals(avalonDoc.Content, pane))
            {
               return avalonDoc;
            }
         }
         return null;
      }

      public IDockPane FindPane(string name)
      {
         var avalonPane = FindAvalonPane(name);
         if (avalonPane != null)
         {
            return avalonPane.Content as IDockPane;
         }
         return null;
      }

      public IDockPane ShowPane(string name, CreateDockPaneDelegate createDockPane)
      {
         IDockPane pane = null;
         var avalonPane = FindAvalonPane(name);

         if (avalonPane == null)
         {
            avalonPane = new DockableContent()
            {
               Name = name
            };
            pane = createDockPane(avalonPane, name);

            BindingExtensions.CreateBinding(pane, "PaneTitle", avalonPane, DockableContent.TitleProperty, BindingMode.OneWay);

            avalonPane.Content = pane;

            avalonPane.Closing += AvalonPane_Closing;
            avalonPane.Closed += AvalonPane_Closed;

            AnchorStyle anchor = AnchorStyle.None;
            switch (pane.DefaultPaneLocation)
            {
               case PaneLocation.Left:
                  anchor = AnchorStyle.Left;
                  break;
               case PaneLocation.Right:
                  anchor = AnchorStyle.Right;
                  break;
               case PaneLocation.Top:
                  anchor = AnchorStyle.Top;
                  break;
               case PaneLocation.Bottom:
                  anchor = AnchorStyle.Bottom;
                  break;
            }

            avalonPane.HideOnClose = pane.HidePaneOnClose;
            avalonPane.Show(dockingManager, anchor);
         }
         else
         {
            avalonPane.Show();
         }

         Dispatcher.BeginInvoke(new NoArgsDelegate(() => { avalonPane.Manager.ActiveDocument = avalonPane; }), null);

         return pane;
      }

      private void AvalonPane_Closing(object sender, CancelEventArgs e)
      {
         var avalonPane = (sender as DockableContent);
         var pane = avalonPane.Content as IDockPane;
         if (pane != null)
         {
            bool cancel = e.Cancel;
            pane.PaneClosing(ref cancel);
            e.Cancel = cancel;
         }
      }

      private void AvalonPane_Closed(object sender, EventArgs e)
      {
         var avalonDoc = sender as DockableContent;
         avalonDoc.Closing -= AvalonDoc_Closing;
         avalonDoc.Closed -= AvalonDoc_Closed;

         var pane = avalonDoc.Content as IDockPane;
         if (pane != null)
         {
            pane.PaneClosed();
         }
      }

      public void ClosePane(IDockPane pane)
      {
         var avalonPane = FindAvalonPane(pane);
         if (avalonPane != null)
         {
            avalonPane.Close();
         }
      }

      public void ClosePane(string name)
      {
         var avalonPane = FindAvalonPane(name);
         if (avalonPane != null)
         {
            avalonPane.Close();
         }
      }

      public string GetPaneName(IDockPane pane)
      {
         var avalonPane = pane.PaneContext as DockableContent;
         if (avalonPane == null)
         {
            throw new ArgumentException("pane is not a valid pane");
         }
         return avalonPane.Name;
      }

      public IEnumerable<IDockPane> Panes 
      {
         get
         {
            foreach (var avalonPane in dockingManager.DockableContents)
            {
               var pane = avalonPane.Content as IDockPane;
               if (pane != null)
               {
                  yield return pane;
               }
            }
         }
      }

      private void DockingManager_ActiveDocumentChanged(object sender, EventArgs e)
      {
         OnPropertyChanged("ActiveDocument");
      }

      private void OnPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;
         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
