using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XDraw.Controls.Media
{
   public interface IScreenSpaceScaler
   {
      double ScaleFactor { get; }

      event EventHandler<EventArgs> ScaleFactorChanged;
   }
}
