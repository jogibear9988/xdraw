using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xml.Linq;
using System.Windows;
using System.Globalization;
using System.Windows.Media;

[assembly: XmlnsDefinitionAttribute("xDrawXaml", "XDraw.Xaml", AssemblyName = "XDraw.Xaml")]

namespace XDraw.Xaml
{
   public class ResDictElement : INotifyPropertyChanged
   {
      static ResDictElement()
      {
         ParserContext = new ParserContext();
         ParserContext.XamlTypeMapper = new XamlTypeMapper(new string[] { });
         ParserContext.XamlTypeMapper.AddMappingProcessingInstruction("xDrawXaml", "XDraw.Xaml", "XDraw.Xaml");
         ParserContext.XmlnsDictionary.Add("xDrawXaml", "XDraw.Xaml");
      }

      public static ParserContext ParserContext { get; private set; }

      public readonly static XNamespace xNs = @"http://schemas.microsoft.com/winfx/2006/xaml/presentation";
      public readonly static XNamespace xNsX = @"http://schemas.microsoft.com/winfx/2006/xaml";
      public readonly static XNamespace xNsXDrawXaml = "xDrawXaml";
      public readonly static XNamespace xNsAssembly = "XDraw.Xaml";

      public ResDictElement(ResDictDoc resDictDoc, XElement xElement)
      {
         ResDictDoc = resDictDoc;

         _Key = (string)xElement.Attribute(xNsX + "Key");
         _Xml = xElement;
         _Obj = CreateObjectFromXml();
      }

      protected virtual object CreateObjectFromXml()
      {
         return new XElement(Xml);
      }

      public ResDictDoc ResDictDoc { get; private set; }
      
      private string _Key;

      public string Key
      {
         get { return _Key; }
      }

      private XElement _Xml;

      public XElement Xml
      {
         get { return _Xml; }
      }

      private object _Obj;

      public object Obj
      {
         get { return _Obj; }
      }

      public virtual ResDictElementTypes ElementType
      {
         get
         {
            return ResDictElementTypes.Other;
         }
      }

      private bool _IsXmlValid = true;

      private bool _IsModified = false;

      public bool IsModified
      {
         get { return _IsModified; }
      }

      internal void SetModified(bool isModified = true, bool isXmlValid = false)
      {
         if (isModified)
         {
            _IsModified = true;
            _IsXmlValid = isXmlValid;
         }
         else
         {
            _IsModified = false;
         }
         NotifyPropertyChanged("IsModified");
      }

      internal virtual void UpdateXml()
      {
         if (!_IsXmlValid)
         {
            var temp = UpdatingXml;
            if (temp != null)
            {
               temp(this, new EventArgs());
            }

            _IsXmlValid = true;

            var xNewElement = CreateXmlFromObject();

            var xKeyAttr = xNewElement.Attribute(xNsX + "Key");
            if (xKeyAttr == null)
            {
               xNewElement.Add(new XAttribute(xNsX + "Key", Key));
            }
            else
            {
               xKeyAttr.Value = Key;
            }
            var parent = _Xml.Parent;
            if (xNewElement.Parent != parent && parent != null)
            {
               _Xml.AddBeforeSelf(xNewElement);
               _Xml.Remove();               
            }
            _Xml = xNewElement;

            NotifyPropertyChanged("Xml");
         }
      }

      protected virtual XElement CreateXmlFromObject()
      {
         return new XElement(Obj as XElement);
      }

      public event EventHandler<EventArgs> UpdatingXml;

      internal void CopyToClipboard()
      {
         UpdateXml();
         var xmlText = Xml.ToString(SaveOptions.OmitDuplicateNamespaces);

         var data = new DataObject();
         data.SetData(DataFormats.Text, RemoveNamespaces(xmlText));
         data.SetData(DataFormats.Xaml, xmlText);

         Clipboard.SetDataObject(data, true);
      }

      public static string RemoveNamespaces(string xmlText)
      {
         int n = xmlText.IndexOf(" xmlns:x=\"http://");
         if (n >= 0)
         {
            int n2 = xmlText.IndexOf('\"', n + 17);
            if (n2 > n)
            {
               xmlText = xmlText.Substring(0, n) + xmlText.Substring(n2 + 1);
            }
         }
         n = xmlText.IndexOf(" xmlns=\"http://");
         if (n >= 0)
         {
            int n2 = xmlText.IndexOf('\"', n + 15);
            if (n2 > n)
            {
               xmlText = xmlText.Substring(0, n) + xmlText.Substring(n2 + 1);
            }
         }

         return xmlText;
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


      internal static ResDictElement FromXElement(ResDictDoc resDictDoc, XElement xElement)
      {
         if (xElement.Name == ResDictElement.xNs + "DrawingImage")
         {
            return new DrawingImageResDictElement(resDictDoc, xElement);
         }
         else
         {
            return new ResDictElement(resDictDoc, xElement);
         }
      }
   }

   public enum ResDictElementTypes
   {
      Other = 0,
      DrawingImage = 1
   }
}
