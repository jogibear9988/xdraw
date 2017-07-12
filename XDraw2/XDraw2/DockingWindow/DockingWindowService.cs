using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.ComponentModel;

namespace remes.ASCOT.AdvancedServices.DockingWindow
{
   [ASCOTService("DockingWindowService")]
   public class DockingWindowService : ASCOTService
   {
      private static DockingWindowService _Instance = null;

      public static DockingWindowService Instance
      {
         get { return _Instance; }
      }

      public DockingWindowService() :
         base()
      {
         _Instance = this;
      }

      public override string ServiceName
      {
         get { return "DockingWindow-Service"; }
      }

      private bool _Initialized = false;

      public override void OnStartup(ASCOTCore core, string[] args, XElement parameters)
      {
         base.OnStartup(core, args, parameters);

         StartPageDockDocument.PageFile = (string)parameters.Attribute("StartPage");

         _Initialized = true;

         if (DockingWindow != null)
         {
            InitDockingWindow(DockingWindow);
         }
      }

      public override void OnExit(ASCOTCore core, ASCOTShutdownReason shutdownReason)
      {
         _Initialized = false;
         base.OnExit(core, shutdownReason);
      }

      private IDockWindow _DockingWindow = null;

      public IDockWindow DockingWindow
      {
         get { return _DockingWindow; }
         set
         {
            _DockingWindow = value;

            if (_Initialized && value != null)
            {
               InitDockingWindow(value);
            }
         }
      }

      private void InitDockingWindow(IDockWindow dockWnd)
      {
         OnDockingWindowAvailable();

         var wnd = dockWnd as Window;
         if (wnd.IsLoaded)
         {
            if (!String.IsNullOrEmpty(StartPageDockDocument.PageFile))
            {
               DockingWindowResources.Instance.LoadStartPageMenuItems();
               if (StartPageDockDocument.GlobalShowOnStartup)
               {
                  ShowStartPage();
               }
            }
            OnDockingWindowLoaded();
         }
         else
         {
            wnd.Loaded += DockingWindow_Loaded;
         }

         wnd.Closing += DockingWindow_Closing;
         wnd.Closed += DockingWindow_Closed;
      }

      public void ShowStartPage()
      {
         if (!String.IsNullOrEmpty(StartPageDockDocument.PageFile) && DockingWindow != null)
         {
            DockingWindow.ShowDocument("startPage", 
               (context, name) => new StartPageDockDocument(context));
         }
      }

      private void DockingWindow_Loaded(object sender, RoutedEventArgs e)
      {
         (sender as Window).Loaded -= DockingWindow_Loaded;
         if (!String.IsNullOrEmpty(StartPageDockDocument.PageFile))
         {
            DockingWindowResources.Instance.LoadStartPageMenuItems();
            if (StartPageDockDocument.GlobalShowOnStartup)
            {
               ShowStartPage();
            }
         }
         OnDockingWindowLoaded();
      }

      private void DockingWindow_Closing(object sender, CancelEventArgs e)
      {         
         OnDockingWindowClosing(e);
      }

      private void DockingWindow_Closed(object sender, EventArgs e)
      {
         (sender as Window).Closing -= DockingWindow_Closing;
         (sender as Window).Closed -= DockingWindow_Closed;
         OnDockingWindowClosed();
      }

      private void OnDockingWindowAvailable()
      {
         var temp = DockingWindowAvailable;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      public event EventHandler DockingWindowAvailable;

      private void OnDockingWindowLoaded()
      {
         var temp = DockingWindowLoaded;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      public event EventHandler DockingWindowLoaded;

      private void OnDockingWindowClosing(CancelEventArgs e)
      {
         var temp = DockingWindowClosing;
         if (temp != null)
         {
            temp(this, e);
         }
      }

      public event CancelEventHandler DockingWindowClosing;

      private void OnDockingWindowClosed()
      {
         var temp = DockingWindowClosed;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      public event EventHandler DockingWindowClosed;
   }

   public delegate IDockDocument CreateDockDocumentDelegate(object context, string name);

   public delegate IDockPane CreateDockPaneDelegate(object context, string name);

   public enum PaneLocation
   {
      Left,
      Right,
      Top,
      Bottom,
      Floating
   }

   public interface IDockWindow
   {
      IDockDocument FindDocument(string name);

      IDockDocument ShowDocument(string name, CreateDockDocumentDelegate createDocCallback);

      IDockDocument ActiveDocument { get; set; }

      string GetDocumentName(IDockDocument doc);

      IEnumerable<IDockDocument> Documents { get; }

      void CloseDocument(IDockDocument doc);

      void CloseDocument(string name);

      IDockPane FindPane(string name);

      IDockPane ShowPane(string name, CreateDockPaneDelegate createDockPane);

      void ClosePane(IDockPane pane);

      void ClosePane(string name);

      string GetPaneName(IDockPane pane);

      IEnumerable<IDockPane> Panes { get; }
   }

   public interface IDockDocument
   {
      object DocumentContext { get; }

      string DocumentTitle { get; }

      void DocumentClosing(ref bool cancel);

      void DocumentClosed();
   }

   public interface IDockPane
   {
      object PaneContext { get; }

      string PaneTitle { get; }

      PaneLocation DefaultPaneLocation { get; }

      bool DefaultAutoHidePane { get; }

      bool HidePaneOnClose { get; }

      void PaneClosing(ref bool cancel);

      void PaneClosed();
   }
}
