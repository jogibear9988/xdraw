using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;

namespace XDraw
{
   public class PanesTemplateSelector : DataTemplateSelector
   {
      public PanesTemplateSelector()
      {
         DocumentTemplate = null;
         PaneTemplate = null;
      }

      public DataTemplate DocumentTemplate { get; set; }

      public DataTemplate PaneTemplate { get; set; }

      public override DataTemplate SelectTemplate(object item, DependencyObject container)
      {
         var itemAsLayoutContent = item as LayoutContent;

         if (item is IDockDocument)
         {
            return DocumentTemplate;
         }

         if (item is IDockPane)
         {
            return PaneTemplate;
         }

         return base.SelectTemplate(item, container);
      }
   }
}
