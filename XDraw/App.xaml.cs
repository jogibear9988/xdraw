using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      public App()
      {
         StartupProjectFile = String.Empty;
      }

      protected override void OnStartup(StartupEventArgs e)
      {
         base.OnStartup(e);

         if (e.Args.Length >= 1)
         {
            StartupProjectFile = e.Args[0];
         }
      }

      protected override void OnExit(ExitEventArgs e)
      {
         base.OnExit(e);
         remes.XDraw.Properties.Settings.Default.Save();
      }

      public string StartupProjectFile { get; private set; }
   }
}
