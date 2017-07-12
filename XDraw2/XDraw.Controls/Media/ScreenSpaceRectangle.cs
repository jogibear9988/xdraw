using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace XDraw.Controls.Media
{
public sealed class ScreenSpaceRectangle : ScreenSpaceShape
{
    // Fields
    private Rect _rect = Rect.Empty;

    public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(
       "RadiusX", typeof(double), typeof(ScreenSpaceRectangle), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(
       "RadiusY", typeof(double), typeof(ScreenSpaceRectangle), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    // Methods
    static ScreenSpaceRectangle()
    {
       ScreenSpaceShape.StretchProperty.OverrideMetadata(typeof(ScreenSpaceRectangle), new FrameworkPropertyMetadata(Stretch.Fill));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double strokeThickness = base.GetStrokeThickness();
        double x = strokeThickness / 2.0;
        this._rect = new Rect(x, x, Math.Max((double) 0.0, (double) (finalSize.Width - strokeThickness)), Math.Max((double) 0.0, (double) (finalSize.Height - strokeThickness)));
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
        Pen pen = base.GetPen();
        drawingContext.DrawRoundedRectangle(base.Fill, pen, this._rect, this.RadiusX, this.RadiusY);
    }

    // Properties
    protected override Geometry DefiningGeometry
    {
        get
        {
            return new RectangleGeometry(this._rect, this.RadiusX, this.RadiusY);
        }
    }

    /*internal override int EffectiveValuesInitialSize
    {
        get
        {
            return 0x13;
        }
    }*/

    public override Transform GeometryTransform
    {
        get
        {
            return Transform.Identity;
        }
    }

    [TypeConverter(typeof(LengthConverter))]
    public double RadiusX
    {
        get
        {
            return (double) base.GetValue(RadiusXProperty);
        }
        set
        {
            base.SetValue(RadiusXProperty, value);
        }
    }

    [TypeConverter(typeof(LengthConverter))]
    public double RadiusY
    {
        get
        {
            return (double) base.GetValue(RadiusYProperty);
        }
        set
        {
            base.SetValue(RadiusYProperty, value);
        }
    }

    public override Geometry RenderedGeometry
    {
        get
        {
            return new RectangleGeometry(this._rect, this.RadiusX, this.RadiusY);
        }
    }
}
}
