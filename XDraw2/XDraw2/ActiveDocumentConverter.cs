using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace XDraw
{
   public class ActiveDocumentConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is IDockDocument)
         {
            return value;
         }
         return Binding.DoNothing;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is IDockDocument)
         {
            return value;
         }
         return Binding.DoNothing;
      }
   }
}
