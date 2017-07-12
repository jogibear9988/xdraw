using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

namespace remes.XDraw
{
   public class XDrawProject : DependencyObject
   {
      public XDrawProject()
      {
         Drawings = new ObservableCollection<XProjectDrawing>();
      }

      public ObservableCollection<XProjectDrawing> Drawings { get; private set; }

      public string ProjectFilePath
      {
         get { return (string)GetValue(ProjectFilePathProperty); }
         set { SetValue(ProjectFilePathProperty, value); }
      }

      public static readonly DependencyProperty ProjectFilePathProperty =
          DependencyProperty.Register("ProjectFilePath", typeof(string), typeof(XDrawProject), new UIPropertyMetadata(String.Empty));

      public string ProjectName
      {
         get { return (string)GetValue(ProjectNameProperty); }
         set { SetValue(ProjectNameProperty, value); }
      }

      public static readonly DependencyProperty ProjectNameProperty =
          DependencyProperty.Register("ProjectName", typeof(string), typeof(XDrawProject), new UIPropertyMetadata(String.Empty));

      public XDrawingExportFormat DefaultExportFormat
      {
         get { return (XDrawingExportFormat)GetValue(DefaultExportFormatProperty); }
         set { SetValue(DefaultExportFormatProperty, value); }
      }

      public static readonly DependencyProperty DefaultExportFormatProperty =
          DependencyProperty.Register("DefaultExportFormat", typeof(XDrawingExportFormat), typeof(XDrawProject), new UIPropertyMetadata(XDrawingExportFormat.DrawingImage));

      public string XAMLFilePath
      {
         get { return (string)GetValue(XAMLFilePathProperty); }
         set { SetValue(XAMLFilePathProperty, value); }
      }

      public static readonly DependencyProperty XAMLFilePathProperty =
          DependencyProperty.Register("XAMLFilePath", typeof(string), typeof(XDrawProject), new UIPropertyMetadata(String.Empty));


      public void Load(string path)
      {
         ProjectFilePath = path;

         var doc = XDocument.Load(ProjectFilePath);
         var xPrj = new
            {
               Version = (string)doc.Root.Elements("FileVersion").FirstOrDefault(),
               ProjectName = (string)doc.Root.Elements("ProjectName").FirstOrDefault(),
               DefaultExportFormat = (string)doc.Root.Elements("DefaultExportFormat").FirstOrDefault(),
               XAMLFilePath = (string)doc.Root.Elements("XAMLFilePath").FirstOrDefault(),
               Drawings = from d in doc.Root.Descendants("Drawing")
                          select new
                          {
                             DrawingName = (string)d.Attributes("Name").FirstOrDefault(),
                             ExportFormat = (string)d.Elements("ExportFormat").FirstOrDefault(),
                             DrawingSize = (string)d.Elements("DrawingSize").FirstOrDefault(),
                             SaveDrawingSize = (string)d.Elements("SaveDrawingSize").FirstOrDefault()
                          }
            };

         ProjectName = xPrj.ProjectName ?? String.Empty;
         if (xPrj.DefaultExportFormat != null)
         {
            DefaultExportFormat = (XDrawingExportFormat)Enum.Parse(typeof(XDrawingExportFormat), xPrj.DefaultExportFormat);
         }
         XAMLFilePath = xPrj.XAMLFilePath ?? String.Empty;

         foreach (var d in xPrj.Drawings)
         {
            var drawing = new XProjectDrawing();
            drawing.DrawingName = d.DrawingName ?? String.Empty;
            if (d.ExportFormat != null)
            {
               drawing.ExportFormat = (XDrawingExportFormat)Enum.Parse(typeof(XDrawingExportFormat), d.ExportFormat);
            }
            else
            {
               drawing.ExportFormat = DefaultExportFormat;
            }
            if (d.DrawingSize != null)
            {
               drawing.DrawingSize = Size.Parse(d.DrawingSize);
            }
            if (d.SaveDrawingSize != null)
            {
               drawing.SaveDrawingSize = Boolean.Parse(d.SaveDrawingSize);
            }
            Drawings.Add(drawing);
         }

         var xamlPath = XAMLFilePath;
         if (!Path.IsPathRooted(xamlPath))
         {
            xamlPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ProjectFilePath), xamlPath));
         }
         if (File.Exists(xamlPath))
         {
            var xamlDoc = XDocument.Load(xamlPath);

            XNamespace nsx = "http://schemas.microsoft.com/winfx/2006/xaml";

            var xDrawings = from d in xamlDoc.Root.Elements()
                            where d.Name.LocalName == "Canvas" || d.Name.LocalName == "DrawingImage"
                            select new
                            {
                               XmlCode = d,
                               Key = d.Attribute(nsx + "Key").Value
                            };

            foreach (var d in xDrawings)
            {
               var drawing = GetDrawing(d.Key);
               if (drawing == null)
               {
                  drawing = new XProjectDrawing();
                  drawing.DrawingName = d.Key;
                  drawing.ExportFormat = d.XmlCode.Name.LocalName == "Canvas" ? XDrawingExportFormat.Canvas : XDrawingExportFormat.DrawingImage;
                  Drawings.Add(drawing);
               }
               drawing.XmlCode = new XElement(d.XmlCode);
               foreach (var xe in drawing.XmlCode.DescendantsAndSelf())
               {
                  xe.Name = xe.Name.LocalName;
               }
               drawing.XmlCode.RemoveAttributes();
            }
         }
      }

      public XProjectDrawing GetDrawing(string name)
      {
         foreach(var d in Drawings)
         {
            if (d.DrawingName == name)
            {
               return d;
            }
         }
         return null;
      }

      public void Save()
      {
         if (String.IsNullOrEmpty(ProjectFilePath))
         {
            throw new Exception("ProjectFilePath must be valid to call Save(). Use Save(path) instead!");
         }
         if (String.IsNullOrEmpty(XAMLFilePath))
         {
            throw new Exception("XAMLFilePath must be valid to Save!");
         }

         var xDrawings = new XElement("Drawings");
         foreach (var d in Drawings)
         {
            d.Save(xDrawings);
         }

         var doc = new XDocument(
            new XElement("XDrawProject",
               new XElement("FileVersion", "1.0"),
               new XElement("ProjectName", ProjectName),
               new XElement("DefaultExportFormat", DefaultExportFormat.ToString()),
               new XElement("XAMLFilePath", XAMLFilePath),
               xDrawings));

         doc.Save(ProjectFilePath);

         CreateXAMLFile();
      }

      public void Save(string path)
      {
         ProjectFilePath = path;
         Save();
      }

      private void CreateXAMLFile()
      {
         var nfi = new NumberFormatInfo();
         nfi.NumberDecimalSeparator = ".";

         XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
         XNamespace nsx = "http://schemas.microsoft.com/winfx/2006/xaml";

         var xDict = XElement.Parse("<ResourceDictionary " +
            "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
            "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
            "</ResourceDictionary>");
         var doc = new XDocument(
            new XComment("This file is auto generated by XDraw."),
            new XComment("Do not modify this file directly, or your changes will be overwritten."),
            xDict);

         foreach (var d in Drawings)
         {
            if (d.XmlCode != null)
            {
               var xCode = new XElement(d.XmlCode);
               foreach (var xe in xCode.DescendantsAndSelf())
               {
                  xe.Name = ns + xe.Name.LocalName;
               }
               xCode.Add(new XAttribute(nsx + "Key", d.DrawingName));
               xDict.Add(xCode);

               if (d.SaveDrawingSize && d.ExportFormat == XDrawingExportFormat.Canvas)
               {
                  xCode.Add(new XAttribute("Width", d.DrawingSize.Width.ToString(nfi)));
                  xCode.Add(new XAttribute("Height", d.DrawingSize.Width.ToString(nfi)));
               }
            }
         }

         var path = XAMLFilePath;
         if (!Path.IsPathRooted(path))
         {
            path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ProjectFilePath), path));
         }
         doc.Save(path);
      }

      public void RemoveDrawing(XProjectDrawing pDrawing)
      {
         if (Drawings.Contains(pDrawing))
         {
            pDrawing.CloseDrawing();
            Drawings.Remove(pDrawing);
         }
      }
   }
}
