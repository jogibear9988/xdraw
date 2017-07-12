using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XDraw
{
   public interface ICompanionTypeResolver
   {
      Type Resolve(object item);

      IEnumerable<INewItemType> NewItemTypes { get; }
   }
}
