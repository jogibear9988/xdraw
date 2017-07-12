﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for AboutDialog.xaml
   /// </summary>
   public partial class AboutDialog : Window
   {
      public AboutDialog()
      {
         InitializeComponent();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         txtVersion.Text = Assembly.GetEntryAssembly().GetName().Version.ToString();
      }
   }
}
