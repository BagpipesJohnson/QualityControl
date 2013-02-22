using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//added
using System.Linq;
using System.Xml.Linq;

namespace OverSeer
{
    /// <summary>
    /// Interaction logic for ProjectManagment.xaml
    /// </summary>
    public partial class ProjectManagment : Window
    {
        public ProjectManagment()
        {
            InitializeComponent();

            //xaml binding
            ComboBox_projects.DataContext = MainWindow.CurrentProjectObjectsDict.Keys;
        }

        private void Button_CreateProject_Click(object sender, RoutedEventArgs e)
        {
            XDocument newProject = new XDocument(
                new XElement("Project",
                    new XElement("Name"),
                    new XElement("SDNumber"),
                    new XElement("Watchfolder"),
                    new XElement("MezzaninePassFolder"),
                    new XElement("WebPassFolder"),
                    new XElement("FailFolder"),
                    new XElement("Keyword"),
                    new XElement("ToEmailOnFail"))
                );

            foreach (var element in newProject.Element("Project").Elements())
            {
                XML_Element_Entry entry = new XML_Element_Entry();
                entry.Label_element.Content = element.Name;
                entry.ShowDialog();

                newProject.Element("Project").Element(element.Name).Value = entry.TextBox_value.Text;
            }

            newProject.Save(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\" + newProject.Element("Project").Element("Name").Value + ".xml");
        }

        private void ComboBox_projects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_projects.SelectedValue != "")
            {
                ProjectObject selectedProject = MainWindow.CurrentProjectObjectsDict[ComboBox_projects.SelectedItem.ToString()];
                openProject(selectedProject);
            }
        }

        private void openProject(ProjectObject project)
        {
            ProjectInfoWindow projectInfo = new ProjectInfoWindow(project);
            projectInfo.ShowDialog();
        }
    }
}
