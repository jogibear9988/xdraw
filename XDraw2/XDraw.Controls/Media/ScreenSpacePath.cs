using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;

namespace XDraw.Controls.Media
{
   public sealed class ScreenSpacePath : OffsetableScreenSpaceShape
   {
      // Fields
      public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
         "Data", typeof(Geometry), typeof(ScreenSpacePath), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure), null);

      // Properties
      public Geometry Data
      {
         get
         {
            return (Geometry)base.GetValue(DataProperty);
         }
         set
         {
            base.SetValue(DataProperty, value);
         }
      }

      protected override Geometry DefiningGeometry
      {
         get
         {
            Geometry data = this.Data;
            if (data == null)
            {
               data = Geometry.Empty;
            }
            return data;
         }
      }

      /*internal override int EffectiveValuesInitialSize
      {
         get
         {
            return 13;
         }
      }*/
   }
}
