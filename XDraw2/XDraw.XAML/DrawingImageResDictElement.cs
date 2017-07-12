using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Collections;
using System.ComponentModel;
using System.Xml.Linq;
using System.Windows;
using System.Globalization;

namespace XDraw.Xaml
{
   public class DrawingImageResDictElement : ResDictElement, IXDrawDrawing, INotifyPropertyChanged
   {
      public DrawingImageResDictElement(ResDictDoc resDictDoc, XElement xElement) :
         base(resDictDoc, xElement)
      { }

      public override ResDictElementTypes ElementType
      {
         get
         {
            return ResDictElementTypes.DrawingImage;
         }
      }
      protected override object CreateObjectFromXml()
      {
         // nmake a copy to prevent changes in the original xml
         var xElement = new XElement(Xml);

         foreach (var x in xElement.DescendantNodes())
         {
            var xComment = x as XComment;
            if (xComment != null && xComment.NextNode is XElement)
            {
               if (xComment.Value.Trim().StartsWith("<TextGeometry"))
               {
                  // backward compability to XDraw 1.x
                  try
                  {
                     var xTextGeometry = XDocument.Parse(xComment.Value).Root;
                     var position = Point.Parse((string)xTextGeometry.Attribute("TopLeft"));
                     var size = (Size)(Point.Parse((string)xTextGeometry.Attribute("BottomRight")) - position);
                     var xNewTextGeometry = new XElement("TextGeometry",
                        new XAttribute("Text", (string)xTextGeometry.Attribute("Text")),
                        new XAttribute("Position", position.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Size", size.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("TextSize", (string)xTextGeometry.Attribute("TextSize")),
                        new XAttribute("FontFamily", (string)xTextGeometry.Attribute("FontFamily")),
                        new XAttribute("FontStyle", (string)xTextGeometry.Attribute("FontStyle")),
                        new XAttribute("FontWeight", (string)xTextGeometry.Attribute("FontWeight")),
                        new XAttribute("TextAlignment", (string)xTextGeometry.Attribute("Alignment")),
                        new XAttribute("TextTrimming", (string)xTextGeometry.Attribute("Trimming")));

                     (xComment.NextNode as XElement).Add(new XAttribute(xNsXDrawXaml + "GeometryExtProps.CustomGeometry",
                        new XElement("XDraw2_CustomGeometry",
                           new XAttribute("CompanionAssembly", "XDraw.Xaml"),
                           new XAttribute("CompanionType", "XDraw.Xaml.TextGeometryCompanion"),
                           xNewTextGeometry).ToString()));
                  }
                  catch { }
               }
               else if (xComment.Value.Trim().StartsWith("<XDraw2_CustomGeometry"))
               {
                  (xComment.NextNode as XElement).Add(new XAttribute(xNsXDrawXaml + "GeometryExtProps.CustomGeometry", xComment.Value));
               }
            }
         }
         try
         {
            return System.Windows.Markup.XamlReader.Parse(xElement.ToString(), ResDictElement.ParserContext);
         }
         catch (Exception ex)
         {
            return base.CreateObjectFromXml();
         }
      }

      protected override XElement CreateXmlFromObject()
      {
         // make sure all CustomGeometry properties are up to date
         if (_Companions != null)
         {
            foreach (var companion in _Companions)
            {
               if (companion is DrawingCompanion)
               {
                  (companion as DrawingCompanion).UpdateCustomGeometryProperty();
               }
            }
         }

         // use XamlWriter to convert DrawingImage to xml
         var xElement = XElement.Parse(System.Windows.Markup.XamlWriter.Save(Obj as DrawingImage));
         
         // convert CustomGeometry attribute into comment which is inserted before this element
         foreach (var xSub in xElement.Descendants().ToList())
         {
            var xCG = xSub.Attribute(xNsXDrawXaml + "GeometryExtProps.CustomGeometry");
            if (xCG == null)
            {
               xCG = xSub.Attribute(xNsAssembly + "GeometryExtProps.CustomGeometry");
            }
            if (xCG != null)
            {
               var xComment = new XComment(xCG.Value);
               xSub.AddBeforeSelf(xComment);
               xCG.Remove();
            }
         }

         // remove the XDraw namespace
         xElement.SetAttributeValue(XNamespace.Xmlns + "xDrawXaml", null);
         xElement.SetAttributeValue(XNamespace.Xmlns + "assembly", null);
         return xElement;
      }

      public void SetModified()
      {
         base.SetModified();
      }

      public string UniqueId
      {
         get
         {
            return ResDictDoc.FileName.ToLower() + ":" + Key;
         }
      }

      public string Name
      {
         get { return ResDictDoc.Name + " : " + Key; }
      }

      public string Description
      {
         get { return ResDictDoc.FileName + " : " + Key; }
      }

      public DrawingImage DrawingImage
      {
         get { return Obj as DrawingImage; }
      }

      public IList Items
      {
         get
         {
            if (DrawingImage != null && DrawingImage.Drawing is DrawingGroup)
            {
               return (DrawingImage.Drawing as DrawingGroup).Children;
            }
            return null;
         }
      }

      public ICompanionTypeResolver CompanionTypeResolver
      {
         get { return XDraw.Xaml.CompanionTypeResolver.Instance; }
      }

      private IList<Companion> _Companions = null;
      public IList<Companion> Companions 
      {
         get { return _Companions; }
         set
         {
            _Companions = value;
         }
      }

      #region INotifyPropertyChanged Members

      private void NotifyPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;
         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      public static DrawingImageResDictElement CreateNew(ResDictDoc resDictDoc, string key)
      {
         return new DrawingImageResDictElement(resDictDoc, XElement.Parse(
            "<DrawingImage x:Key=\"" + key + "\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
            "<DrawingImage.Drawing><DrawingGroup></DrawingGroup></DrawingImage.Drawing></DrawingImage>"));
      }
   }


   public static class GeometryExtProps
   {
      public static DependencyProperty CustomGeometryProperty = DependencyProperty.RegisterAttached(
         "CustomGeometry", typeof(string), typeof(GeometryExtProps), new PropertyMetadata(null));

      public static void SetCustomGeometry(DependencyObject d, string value)
      {
         d.SetValue(CustomGeometryProperty, value);
      }

      public static string GetCustomGeometry(DependencyObject d)
      {
         return (string)d.GetValue(CustomGeometryProperty);
      }
   }
}
