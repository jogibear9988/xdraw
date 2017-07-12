using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;
using System.Reflection;
using System.Windows;

namespace XDraw.Xaml
{
   public class CompanionTypeResolver : ICompanionTypeResolver
   {
      public static CompanionTypeResolver Instance { get; private set;}

      static CompanionTypeResolver()
      {
         Instance = new CompanionTypeResolver();
      }

      public CompanionTypeResolver()
      {
         InitItemTypes();
      }

      public Type Resolve(object item)
      {
         var drawing = item as Drawing;
         if (drawing != null)
         {
            var geomDrawing = drawing as GeometryDrawing;
            if (geomDrawing != null)
            {
               var customGeomDef = geomDrawing.Geometry.GetValue(GeometryExtProps.CustomGeometryProperty);

               if (customGeomDef is string)
               {
                  var xCustomGeom = XDocument.Parse((string)customGeomDef).Root;
                  if (xCustomGeom.Name.LocalName == "XDraw2_CustomGeometry")
                  {
                     var assemblyName = (string)xCustomGeom.Attribute("CompanionAssembly");
                     var typeName = (string)xCustomGeom.Attribute("CompanionType");

                     var assembly = Assembly.Load(assemblyName);
                     if (assembly != null)
                     {
                        return assembly.GetType(typeName);
                     }
                  }
               }
               else if (geomDrawing.Geometry is EllipseGeometry)
               {
                  return typeof(EllipseGeometryCompanion);
               }
               else if (geomDrawing.Geometry is LineGeometry)
               {
                  return typeof(LineGeometryCompanion);
               }
               else if (geomDrawing.Geometry is PathGeometry)
               {
                  return typeof(PathGeometryCompanion);
               }
               else if (geomDrawing.Geometry is RectangleGeometry)
               {
                  return typeof(RectangleGeometryCompanion);
               }

               return typeof(GeometryDrawingCompanion);
            }

            return typeof(DrawingCompanion);
         }
         return null;
      }

      public IEnumerable<INewItemType> NewItemTypes
      {
         get { return _NewItemTypes; }
      }

      private List<INewItemType> _NewItemTypes = new List<INewItemType>();

      private void InitItemTypes()
      {
         var imageResDict = new ResourceDictionary();
         imageResDict.Source = new Uri("/XDraw.Xaml;component/ImageDictionary.xaml", UriKind.RelativeOrAbsolute);

         _NewItemTypes.Add(new NewXamlItemType("Line", "Line item", imageResDict["NewLineImage"] as ImageSource, LineGeometryCompanion.CreateNew));
         _NewItemTypes.Add(new NewXamlItemType("Ellipse", "Ellipse item", imageResDict["NewEllipseImage"] as ImageSource, EllipseGeometryCompanion.CreateNew));
         _NewItemTypes.Add(new NewXamlItemType("Rectangle", "Rectangle item", imageResDict["NewRectangleImage"] as ImageSource, RectangleGeometryCompanion.CreateNew));
         _NewItemTypes.Add(new NewXamlItemType("Path / Polygon", "Path or polygon item", imageResDict["NewPathImage"] as ImageSource, PathGeometryCompanion.CreateNew));
         _NewItemTypes.Add(new NewXamlItemType("Text", "Text item", imageResDict["NewTextImage"] as ImageSource, TextGeometryCompanion.CreateNew));
      }
   }
}
