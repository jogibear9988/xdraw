using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace XDraw
{
   public static class ValueConverters
   {
      static ValueConverters()
      {
         ObjectNotNullVisibity = new ObjectNotNullVisibityConverter();
         BoolInverse = new BoolInverseConverter();
      }

      public static ObjectNotNullVisibityConverter ObjectNotNullVisibity { get; private set; }

      public static BoolInverseConverter BoolInverse { get; private set; }
   }

   public class ObjectNotNullVisibityConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return value != null ? Visibility.Visible : Visibility.Collapsed;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }

      #endregion
   }

   public class BoolInverseConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return !(bool)value;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return !(bool)value;
      }

      #endregion
   }
}
