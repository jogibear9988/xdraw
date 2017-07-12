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
using System.Xml.Linq;
using System.IO;
using IOPath = System.IO.Path;
using remes.ASCOT;
using System.Windows.Threading;
using System.Windows.Markup;

namespace remes.ASCOT.AdvancedServices.DockingWindow
{
   /// <summary>
   /// Interaction logic for StartPageDockDocument.xaml
   /// </summary>
   public partial class StartPageDockDocument : UserControl, IDockDocument
   {
      static StartPageDockDocument()
      {
         PageFile = null;
         GlobalShowOnStartup = true;
         GlobalAutoClose = false;
      }

      public static string PageFile { get; set; }

      public static bool GlobalShowOnStartup { get; set; }
      public static bool GlobalAutoClose { get; set; }

      public StartPageDockDocument(object context)
      {
         _Context = context;

         InitializeComponent();

         Load();
      }

      private object _Context;

      public bool ShowOnStartup
      {
         get { return GlobalShowOnStartup; }
         set { GlobalShowOnStartup = value; }
      }

      public bool AutoClose
      {
         get { return GlobalAutoClose; }
         set { GlobalAutoClose = value; }
      }

      public void Reload()
      {
         try
         {
            if (_NavigatedToErrorMessage)
            {
               Load();
            }
            else
            {
               Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
               try
               {
                  pageFrame.Refresh();
               }
               finally
               {
                  Application.Current.DispatcherUnhandledException -= Application_DispatcherUnhandledException;
               }
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }


      public void Load()
      {
         if (PageFile != null)
         {
            Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            try
            {
               var path = ASCOTCore.Instance.ExpandPath(PageFile, ASCOTCore.Instance.ParamPath);
               if (File.Exists(path))
               {
                  try
                  {
                     _NavigatedToErrorMessage = false;
                     pageFrame.Navigate(new Uri(path));
                  }
                  catch (Exception ex)
                  {
                     MessageBox.Show(ex.Message);
                  }
               }
            }
            finally
            {
               Application.Current.DispatcherUnhandledException -= Application_DispatcherUnhandledException;
            }
         }
      }

      private bool _NavigatedToErrorMessage = false;

      private void PageFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
      {
         HandleNavigationError(e.Exception);
         e.Handled = true;
      }

      void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
      {
         var xamlParseException = e.Exception as XamlParseException;
         if (xamlParseException != null && xamlParseException.BaseUri.LocalPath.EndsWith(PageFile, StringComparison.OrdinalIgnoreCase))
         {
            HandleNavigationError(e.Exception);
            e.Handled = true;
         }
      }

      private void HandleNavigationError(Exception ex)
      {
         if (!_NavigatedToErrorMessage)
         {
            _NavigatedToErrorMessage = true;
            pageFrame.Navigate(new Run(String.Format("Loading Start page failed!\n{0}", ex.Message)));
         }
      }

      public object DocumentContext
      {
         get { return _Context; }
      }

      public string DocumentTitle
      {
         get { return "Start Page"; }
      }

      public void DocumentClosing(ref bool cancel)
      { }

      public void DocumentClosed()
      { }
   }
}
