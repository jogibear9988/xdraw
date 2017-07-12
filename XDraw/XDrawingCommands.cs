using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace remes.XDraw
{
   public static class XDrawingCommands
   {
      public static RoutedCommand SendToBack = new RoutedCommand("SendToBack", typeof(XDrawingCommands));
      public static RoutedCommand OneToBack = new RoutedCommand("OneToBack", typeof(XDrawingCommands));
      public static RoutedCommand OneToFront = new RoutedCommand("OneToFront", typeof(XDrawingCommands));
      public static RoutedCommand SendToFront = new RoutedCommand("SendToFront", typeof(XDrawingCommands));
   }
}
