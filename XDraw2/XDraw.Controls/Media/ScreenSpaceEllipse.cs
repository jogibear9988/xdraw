using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace XDraw.Controls.Media
{
   public sealed class ScreenSpaceEllipse : ScreenSpaceShape
   {
      // Fields
      private Rect _rect = Rect.Empty;

      // Methods
      static ScreenSpaceEllipse()
      {
         ScreenSpaceEllipse.StretchProperty.OverrideMetadata(typeof(ScreenSpaceEllipse), new FrameworkPropertyMetadata(Stretch.Fill));
      }

      protected override Size ArrangeOverride(Size finalSize)
      {
         double strokeThickness = base.GetStrokeThickness();
         double x = strokeThickness / 2.0;
         this._rect = new Rect(x, x, Math.Max((double)0.0, (double)(finalSize.Width - strokeThickness)), Math.Max((double)0.0, (double)(finalSize.Height - strokeThickness)));
         switch (base.Stretch)
         {
            case Stretch.None:
               this._rect.Width = this._rect.Height = 0.0;
               break;

            case Stretch.Uniform:
               if (this._rect.Width <= this._rect.Height)
               {
                  this._rect.Height = this._rect.Width;
                  break;
               }
               this._rect.Width = this._rect.Height;
               break;

            case Stretch.UniformToFill:
               if (this._rect.Width >= this._rect.Height)
               {
                  this._rect.Height = this._rect.Width;
                  break;
               }
               this._rect.Width = this._rect.Height;
               break;
         }
         base.ResetRenderedGeometry();
         return finalSize;
      }

      internal override void CacheDefiningGeometry()
      {
         double x = base.GetStrokeThickness() / 2.0;
         this._rect = new Rect(x, x, 0.0, 0.0);
      }

      internal override Rect GetDefiningGeometryBounds()
      {
         return this._rect;
      }

      internal override Size GetNaturalSize()
      {
         double strokeThickness = base.GetStrokeThickness();
         return new Size(strokeThickness, strokeThickness);
      }

      protected override Size MeasureOverride(Size constraint)
      {
         if (base.Stretch != Stretch.UniformToFill)
         {
            return this.GetNaturalSize();
         }
         double width = constraint.Width;
         double height = constraint.Height;
         if (double.IsInfinity(width) && double.IsInfinity(height))
         {
            return this.GetNaturalSize();
         }
         if (double.IsInfinity(width) || double.IsInfinity(height))
         {
            width = Math.Min(width, height);
         }
         else
         {
            width = Math.Max(width, height);
         }
         return new Size(width, width);
      }

      protected override void OnRender(DrawingContext drawingContext)
      {
         if (!this._rect.IsEmpty)
         {
            Pen pen = base.GetPen();
            drawingContext.DrawGeometry(base.Fill, pen, new EllipseGeometry(this._rect));
         }
      }

      // Properties
      protected override Geometry DefiningGeometry
      {
         get
         {
            if (this._rect.IsEmpty)
            {
               return Geometry.Empty;
            }
            return new EllipseGeometry(this._rect);
         }
      }

      /*internal override int EffectiveValuesInitialSize
      {
         get
         {
            return 13;
         }
      }*/

      public override Transform GeometryTransform
      {
         get
         {
            return Transform.Identity;
         }
      }

      public override Geometry RenderedGeometry
      {
         get
         {
            return this.DefiningGeometry;
         }
      }
   }

}
