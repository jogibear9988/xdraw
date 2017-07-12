using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace XDraw.Xaml
{
   public class NewXamlItemType : INewItemType
   {
      public NewXamlItemType(string name, string Descriptiuon, ImageSource icon, Func<Point, ItemCreateMode, object> createCallback)
      {
         _Name = name;
         _Description = Description;
         _Icon = icon;
         _CreateCallback = createCallback;
      }

      private string _Name;
      private string _Description;
      private ImageSource _Icon;
      private Func<Point, ItemCreateMode, object> _CreateCallback;

      #region INewItemType Members

      public string Name
      {
         get { return _Name; }
      }

      public string Description
      {
         get { return _Description; }
      }

      public ImageSource Icon
      {
         get { return _Icon; }
      }

      public object CreateNewItem(Point position, ItemCreateMode createMode)
      {
         return _CreateCallback(position, createMode);
      }

      #endregion
   }
}
