using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XDraw
{
   public delegate IDockDocument CreateDockDocumentDelegate(object context, string id);

   public delegate IDockPane CreateDockPaneDelegate(object context, string id);

   public enum PaneLocation
   {
      Left,
      Right,
      Top,
      Bottom,
      Floating
   }
}
