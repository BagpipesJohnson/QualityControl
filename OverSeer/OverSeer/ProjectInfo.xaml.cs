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
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OverSeer
{

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ProjectInfoWindow : Window
    {
        public Adjudicator adjudicator;
        //private XmlTextReader currentXML;
        private string currentProject;

        public ProjectInfoWindow(ProjectObject project)
        {
            InitializeComponent();

            this.TextBox_MezzaninePassFolder.Text = project.MezzaninePassFolder.FullName;
            this.TextBox_SDNumber.Text = project.SDNumber;
            this.TextBox_WebPassFolder.Text = project.WebPassFolder.FullName;
            this.TextBox_FailedDirectory.Text = project.FailFolder.FullName;
            this.TextBox_WatchFolder.Text = project.Watchfolder.FullName;
            this.TextBox_ProjectName.Text = project.ProjectName;
            this.TextBox_Keywords.Text = project.Keywords.ToArray<string>().ToString();
        }

        public ProjectInfoWindow(string projectname, 
                                 string webPassDir, 
                                 string mezPasDir, 
                                 string failDir, 
                                 string SDnum, 
                                 string watchFolder, 
                                 string keywords, 
                                 Adjudicator adjudicator)
        {
            currentProject = projectname;
            this.adjudicator = adjudicator;
            this.adjudicator.changeProject(utility.getProjectFileFromString(currentProject).FullName);
            
            InitializeComponent();
            
            this.TextBox_MezzaninePassFolder.Text = mezPasDir;
            this.TextBox_SDNumber.Text = SDnum;
            this.TextBox_WebPassFolder.Text = webPassDir;
            this.TextBox_FailedDirectory.Text = failDir;
            this.TextBox_WatchFolder.Text = watchFolder;
            this.TextBox_ProjectName.Text = projectname;
            this.TextBox_Keywords.Text = keywords;
        }

        private void TextBox_WatchFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("Watchfolder", (TextBox)sender);
            this.adjudicator.watchFolder = new DirectoryInfo(TextBox_WatchFolder.Text);
        }

        private void TextBox_Keywords_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("Keyword", (TextBox)sender);
            this.adjudicator.keywords = TextBox_Keywords.Text;
        }
        
        private void TextBox_ProjectName_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("Name", (TextBox)sender);
            this.adjudicator.keywords = TextBox_ProjectName.Text;
        }

        private void TextBox_MezzaninePassFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("MezzaninePassFolder", (TextBox)sender);
            this.adjudicator.keywords = TextBox_MezzaninePassFolder.Text;
        }

        private void TextBox_WebPassFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("WebPassFolder", (TextBox)sender);
            this.adjudicator.keywords = TextBox_WebPassFolder.Text;
        }

        private void TextBox_FailedDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("FailFolder", (TextBox)sender);
            this.adjudicator.keywords = TextBox_FailedDirectory.Text;
        }

        private void TextBox_SDNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            writeTextBoxToXML("SDNumber", (TextBox)sender);
            this.adjudicator.keywords = TextBox_SDNumber.Text;
        }

        /// <summary>
        /// writes an xml element to the project xmls
        /// </summary>
        /// <param name="element">xml element to change</param>
        private void writeTextBoxToXML(string element, TextBox textbox)
        {
            XDocument xml = XDocument.Load(utility.getProjectFileFromString(currentProject).FullName);
            xml.Element("Project").Element(element).Value = textbox.Text;
            xml.Save(utility.getProjectFileFromString(currentProject).FullName);
        }

        /// <summary>
        /// writes an xml element value to the project xmls
        /// </summary>
        /// <param name="parentElement">parent xml element</param>
        /// <param name="element">xml element to change</param>
        /// <param name="value">value to change the element to</param>
        private void writeToXML(string parentElement, string element, string value)
        {
            XDocument xml = XDocument.Load(utility.getProjectFileFromString(currentProject).FullName);
            xml.Element(parentElement).Element(element).Value = value;
            xml.Save(utility.getProjectFileFromString(currentProject).FullName);
        }

        private void Window_ProjectSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            writeTextBoxToXML("Name", TextBox_ProjectName);
            writeTextBoxToXML("SDNumber", TextBox_SDNumber);
            writeTextBoxToXML("Watchfolder", TextBox_WatchFolder);
            writeTextBoxToXML("MezzaninePassFolder", TextBox_MezzaninePassFolder);
            writeTextBoxToXML("WebPassFolder", TextBox_WebPassFolder);
            writeTextBoxToXML("FailFolder", TextBox_FailedDirectory);
            writeTextBoxToXML("Keyword", TextBox_Keywords);
        }
    }
}
