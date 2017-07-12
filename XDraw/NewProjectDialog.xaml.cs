using System;
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

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for NewProjectDialog.xaml
   /// </summary>
   public partial class NewProjectDialog : Window
   {
      public NewProjectDialog()
      {
         InitializeComponent();
      }

      public XDrawProject Project { get; set; }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         if (Project != null)
         {
            edtProjectName.Text = Project.ProjectName;
            m_OldProjectName = Project.ProjectName;
            edtXamlFileName.Text = Project.ProjectName + ".xaml";
         }
      }

      private void OK_Click(object sender, RoutedEventArgs e)
      {
         Project.ProjectName = edtProjectName.Text;
         Project.XAMLFilePath = edtXamlFileName.Text;
         DialogResult = true;
         Close();
      }

      private string m_OldProjectName = String.Empty;

      private void ProjectName_TextChanged(object sender, TextChangedEventArgs e)
      {
         if (edtXamlFileName.Text == m_OldProjectName + ".xaml")
         {
            edtXamlFileName.Text = edtProjectName.Text + ".xaml";
         }
         m_OldProjectName = edtProjectName.Text;
      }
   }
}
