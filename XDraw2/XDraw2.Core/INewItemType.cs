using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace XDraw
{
   public interface INewItemType
   {
      string Name { get; }

      string Description { get; }

      ImageSource Icon { get; }

      object CreateNewItem(Point position, ItemCreateMode createMode);
   }

   public enum ItemCreateMode
   {
      Drop,
      Drag
   }
}
