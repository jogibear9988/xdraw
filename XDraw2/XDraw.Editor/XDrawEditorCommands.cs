using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace XDraw.Editor
{
   public static class XDrawEditorCommands
   {
      public static RoutedUICommand ZoomInCommand = new RoutedUICommand("Zoom in", "ZoomInCommand", typeof(XDrawEditorCommands),
         new InputGestureCollection() { new KeyGesture(Key.Add, ModifierKeys.Control) });

      public static RoutedUICommand ZoomOutCommand = new RoutedUICommand("Zoom out", "ZoomOutCommand", typeof(XDrawEditorCommands),
         new InputGestureCollection() { new KeyGesture(Key.Subtract, ModifierKeys.Control) });

      public static RoutedUICommand ZoomFitCommand = new RoutedUICommand("Zoom fit", "ZoomFitCommand", typeof(XDrawEditorCommands),
         new InputGestureCollection() { new KeyGesture(Key.B, ModifierKeys.Control) });

      public static RoutedUICommand Zoom1Command = new RoutedUICommand("Zoom 1:1", "Zoom1Command", typeof(XDrawEditorCommands),
         new InputGestureCollection() { new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift) });
   }
}
