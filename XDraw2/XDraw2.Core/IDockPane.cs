using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace XDraw
{
   public interface IDockPane
   {
      object PaneContext { get; }

      string PaneId { get; }

      object PaneContent { get; }

      string PaneTitle { get; }

      string PaneToolTip { get; }

      string PaneIconSource { get; }

      bool IsPaneVisible { get; set; }

      PaneLocation DefaultPaneLocation { get; }

      bool DefaultAutoHidePane { get; }

      bool HidePaneOnClose { get; }

      void PaneClosing(ref bool cancel);

      void PaneClosed();
   }
}
