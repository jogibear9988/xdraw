using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace XDraw
{
   public class PanesStyleSelector : StyleSelector
   {
      public PanesStyleSelector()
      {
         DocumentStyle = null;
         PaneStyle = null;
      }

      public Style DocumentStyle { get; set; }

      public Style PaneStyle { get; set; }

      public override Style SelectStyle(object item, DependencyObject container)
      {
         if (item is IDockDocument)
         {
            return DocumentStyle;
         }

         if (item is IDockPane)
         {
            return PaneStyle;
         }

         return base.SelectStyle(item, container);
      }
   }
}
