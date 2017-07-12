using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace XDraw.Controls.Media
{
   internal class BoxedMatrix
   {
      // Fields
      public Matrix Value;

      // Methods
      public BoxedMatrix(Matrix value)
      {
         this.Value = value;
      }
   }
}
