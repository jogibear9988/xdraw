using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace remes.ASCOT.AdvancedServices.DockingWindow
{
   public partial class DockingWindowResources
   {
      static DockingWindowResources()
      {
         Instance = new DockingWindowResources();
      }

      public static DockingWindowResources Instance { get; private set;}

      public DockingWindowResources()
      {
         InitializeComponent();
      }

      public void LoadStartPageMenuItems()
      {
         // instanciate resource
         var dummy = this["showStartPageMenuItem"];
      }

      private void ShowStartPageMenuItem_Click(object sender, RoutedEventArgs e)
      {
         if (DockingWindowService.Instance != null)
         {
            DockingWindowService.Instance.ShowStartPage();
         }
      }
   }
}
