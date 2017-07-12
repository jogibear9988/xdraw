using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XDraw
{
   public interface IXDrawDrawing
   {
      string UniqueId { get; }

      string Name { get; }

      string Description { get; }

      IList Items { get; }

      ICompanionTypeResolver CompanionTypeResolver { get; }

      IList<Companion> Companions { get; set; }
      
      bool IsModified { get; }

      void SetModified();
   }
}
