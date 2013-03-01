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
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace OverSeer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AdjudicatorWindow : Window
    {
        public int Timecode { get; set; }

        public DirectoryInfo projectDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\");
        public DirectoryInfo xmlDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\");
        public string currentUser;
        public string currentUserInitials;
        public FileObjects currentFile;
        public ProjectObject currentProject;
        public DirectoryInfo currentUserFolder;
        private Adjudicator adjudicator;
        public Dictionary<string, DirectoryInfo> userDirectoryDictionary;
        public Dictionary<string, string> userInitialsDictionary;
        public List<ProjectObject> projectsReadyToQC = new List<ProjectObject>();
        public string currentReport;
        public Process currentMovie;

        public DateTime currentFileTime;

        private List<CheckBox> qc_keywords = new List<CheckBox>();
        //private bool failCautionReported; //used to check if when someone fails or cautions somthing, they have said why


        public AdjudicatorWindow()
        {
            InitializeComponent();

            //populate project Combobox with currently available ProjectObjects
            ComboBox_Project.DataContext = MainWindow.CurrentProjectObjectsDict.Keys;

            //add all qc keywords to an array
            qc_keywords.Add(CheckBox_SyncIssues);
            qc_keywords.Add(CheckBox_NoAudio);
            qc_keywords.Add(CheckBox_WrongLanguage);
            qc_keywords.Add(CheckBox_LevelIssues);
            qc_keywords.Add(CheckBox_AspectRatioIssues);
            qc_keywords.Add(CheckBox_SlateIssues);
            qc_keywords.Add(CheckBox_CompressionArtifacts);
            qc_keywords.Add(CheckBox_CorruptFile);
            qc_keywords.Add(CheckBox_NoVideo);

            this.disableButtons();
            // Populate the user combo box
            this.userInitialsDictionary = new Dictionary<string, string>();
            this.userDirectoryDictionary = new Dictionary<string, DirectoryInfo>();

            //clear projects variable
            projectsReadyToQC.Clear();

            populateUsers();
            populateProjects();

            // Populate the project combo box

            // Create an adjudicator
            this.adjudicator = new Adjudicator();

            Closing += this.MainWindowDialog_Closing;
        }

        private void MainWindowDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.adjudicator.googleBot.canClose)
            {
                MessageBoxResult result = MessageBox.Show("A file is still being logged, will you wait for it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
            if (this.currentUserFolder != null)
            {
                this.adjudicator.ClearUserFolder(this.currentUserFolder);
            }
        }

        private void populateProjects()
        {
            //List<FileInfo> xmls = utility.checkForSystemFiles(this.projectDirectory.GetFiles().ToList<FileInfo>());

            //look through each project and see if files exist in that project
            foreach (var project in MainWindow.CurrentProjectObjects)
            {
                if (project.currentFileObjects.Count > 0)
                {
                    projectsReadyToQC.Add(project);
                }
            }

            //foreach (FileInfo file in xmls)
            //{
            //    // Now to only add the projects that have a file!!!

            //    string tempProject = utility.getValueFromXML(file, "Name");



            //    int filesInProject = utility.getNumberOfFilesInProject(tempProject);


            //    if (!this.ComboBox_Project.Items.Contains(tempProject) && tempProject != "NA" && filesInProject != 0)
            //    {
            //        this.ComboBox_Project.Items.Add(tempProject);
            //        projectsReadyToQC.Add(tempProject);
            //        continue;
            //        //this.ProjectComboBox.Items.Add(tempProject);
            //    }
            //}

            ComboBox_Project.DataContext = projectsReadyToQC.Distinct();
        }

        private void populateUsers()
        {
            FileInfo file = new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\Users.xml");

            XmlTextReader reader = new XmlTextReader(file.FullName);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name == "User")
                        {

                                while (reader.NodeType != XmlNodeType.Text)
                                {
                                    reader.Read();
                                }
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    string userName = reader.Value;
                                    this.ComboBox_user.Items.Add(userName);
                                    this.userDirectoryDictionary.Add(userName, new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\" + userName + @"\"));
                                    while (reader.Name != "Initials")
                                    {
                                        reader.Read();
                                    }
                                    while (reader.NodeType != XmlNodeType.Text)
                                    {
                                        reader.Read();
                                    }
                                    userInitialsDictionary.Add(userName, reader.Value);

                                }
                            
                        }
                        break;
                }

            }
            return;
        }

        private void ComboBox_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Return the file if there is one to the autoQC folder
            string moveResult = this.adjudicator.ClearUserFolder(this.currentUserFolder);

            if (moveResult != "")
            {
                MessageBox.Show(moveResult);
            }

            this.currentUser = ComboBox_user.SelectedItem.ToString();
            this.currentUserFolder = this.userDirectoryDictionary[this.currentUser];
            this.currentUserInitials = this.userInitialsDictionary[this.currentUser];
            if (this.adjudicator != null)
            {
                this.adjudicator.userName = this.currentUser;
            }

        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {         
            // Return the file that the user had 
            if (this.currentUser == null)
            {
                this.disableButtons();
                MessageBox.Show("You must first select a user before choosing a project!");
                return;
            }

            this.enableButtons();

            this.adjudicator.ClearUserFolder(this.currentUserFolder);

            // Change current project
            this.currentProject = MainWindow.CurrentProjectObjectsDict[ComboBox_Project.SelectedItem.ToString()];

            this.adjudicator.changeProject(this.currentProject);

            updateFileCount();

            getANewFile();
          
        }

        private void updateFileCount()
        {
            if (this.currentProject != null)
            {
                int numberOfFiles = utility.getNumberOfFilesInProject(this.currentProject);
                this.Label_ProjectFileCount.Content = numberOfFiles.ToString();
            }
        }

        private void playFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Play the current file
            if (this.currentFile != null)
            {
                System.Diagnostics.Process movie = System.Diagnostics.Process.Start(this.currentFile.CurrentFileInfo.FullName);

                this.currentMovie = movie;
            }
        }

        private void disableButtons()
        {
            this.Button_Caution.IsEnabled = this.Button_Fail.IsEnabled = this.Button_Pass.IsEnabled = this.Button_WrongProject.IsEnabled = this.playFileButton.IsEnabled = false;
        }

        private void enableButtons()
        {
            this.Button_Caution.IsEnabled = this.Button_Fail.IsEnabled = this.Button_Pass.IsEnabled = this.Button_WrongProject.IsEnabled = this.playFileButton.IsEnabled = true;
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentFile == null)
            {
                
                return;
            }

            // make our adjudicator pass the file
            // Start a new thread in order to do this quickly
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (this.currentMovie != null)
                {
                    try
                    {
                        this.currentMovie.Kill();
                    }
                    catch (Exception badjuju)
                    {
                        if(!badjuju.Message.Contains("exited"))
                        {
                         MessageBox.Show("Please close the movie and click ok");
                        }

                       this.currentMovie = null;
                    }
                    
                    this.currentMovie = null;
                }
                TimeSpan since = (DateTime.Now - this.currentFileTime);
                utility.addFileAndTimeToUser(since.Seconds, this.currentUser);
                utility.addFileAndTimeToUser(since.Seconds, "QC");
                utility.addOneToPassFailCaution("Passed", this.currentUser);
                utility.addOneToPassFailCaution("Passed", "QC");
                //this.adjudicator.ProcessFile(this.currentFile, "Passed", "", this.currentUser, this.currentUserInitials, this.currentProject);
                this.adjudicator.ProcessFile(this.currentFile, this.currentUser, this.currentUserInitials, this.currentProject);
            };



            worker.RunWorkerAsync();

            getANewFile();

            if (this.currentFile != null)
            {
                System.Diagnostics.Process movie = System.Diagnostics.Process.Start(this.currentFile.CurrentFileInfo.FullName);

                this.currentMovie = movie;
            }

            updateFileCount();
        }

        private void getANewFile()
        {
            //look for files already in the users folders
            if (currentUserFolder.GetFiles().Length > 0)
            {
                foreach (var file in currentUserFolder.GetFiles())
                {
                    //foreach(getQCKeyWords()
                    //TODO: fix this crap
                }
            }
            //if there are some, double check that they belong to this project
            //if they don't warn the user
            //add files that are already in the users folder until they are all gone


            // Grab a new file from the new project
            this.currentFile = this.adjudicator.getFileToQC(this.currentProject, this.currentUserFolder);
    

            if (this.currentFile == null)
            {
                MessageBox.Show("There were no files in that project to QC. Please select another");
                this.FileName.Content = "None available in current project";
                this.TextBox_report.Text = "";
                this.UpdateLayout();
                this.disableButtons();
                return;
            }
            this.enableButtons();
            this.FileName.Content = this.currentFile.CurrentFileInfo.Name;

            this.UpdateLayout();

            this.currentFileTime = DateTime.Now;
        }

        private void WrongProjectButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.currentFile == null)
            {

                return;
            }

            // make our adjudicator pass the file
            // Start a new thread in order to do this quickly
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (this.currentMovie != null)
                {
                    try
                    {
                        this.currentMovie.Kill();
                    }
                    catch (Exception badjuju)
                    {
                        if (!badjuju.Message.Contains("exited"))
                        {
                            MessageBox.Show("Please close the movie and click ok");
                        }

                        this.currentMovie = null;
                    }

                    this.currentMovie = null;
                }
                TimeSpan since = (DateTime.Now - this.currentFileTime);
                utility.addFileAndTimeToUser(since.Seconds, this.currentUser);
                utility.addFileAndTimeToUser(since.Seconds, "QC");


                this.adjudicator.ProcessFile(this.currentFile, this.currentUser, this.currentUserInitials, this.currentProject);
            };
            worker.RunWorkerAsync();

            

            getANewFile();
            updateFileCount();
            if (this.currentFile != null)
            {
                System.Diagnostics.Process movie = System.Diagnostics.Process.Start(this.currentFile.CurrentFileInfo.FullName);

                this.currentMovie = movie;
            }
        }

        private void CautionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!checkIfFailCautionHasBeenReported())
            {
                MessageBox.Show("Please enter what was wrong with the file, or I'll have to kill you.");
                return;
            }
            //update the report
            currentReport = TextBox_report.Text;
            //clear report if the user wants to
            clearReport();

            if (this.currentFile == null)
            {
                return;
            }

            // make our adjudicator pass the file
            // Start a new thread in order to do this quickly
            BackgroundWorker worker = new BackgroundWorker();
            string checks = getQCKeyWords();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (this.currentMovie != null)
                {
                    try
                    {
                        this.currentMovie.Kill();
                    }
                    catch (Exception badjuju)
                    {
                        if (!badjuju.Message.Contains("exited"))
                        {
                            MessageBox.Show("Please close the movie and click ok");
                        }

                        this.currentMovie = null;
                    }

                    this.currentMovie = null;
                }


                TimeSpan since = (DateTime.Now - this.currentFileTime);
                utility.addFileAndTimeToUser(since.Seconds, this.currentUser);
                utility.addFileAndTimeToUser(since.Seconds, "QC");

                utility.addOneToPassFailCaution("Cautioned", this.currentUser);
                utility.addOneToPassFailCaution("Cautioned", "QC");

                this.adjudicator.ProcessFile(this.currentFile, this.currentUser, this.currentUserInitials, this.currentProject);
            };
            worker.RunWorkerAsync();

            updateFileCount();
            getANewFile();

            if (this.currentFile != null)
            {
                System.Diagnostics.Process movie = System.Diagnostics.Process.Start(this.currentFile.CurrentFileInfo.FullName);

                this.currentMovie = movie;
            }

        }

        private string getQCKeyWords()
        {
            string results = "";

            foreach (var keyword in qc_keywords)
            {
                if ((bool)keyword.IsChecked)
                {
                    results += keyword.Content + ", ";
                }
            }

            return results;

            //foreach (CheckBox c in this.wrapPanel1.Children)
            //{
            //    if ((bool)c.IsChecked)
            //    {
            //        results += c.Content + ", ";
            //    }
            //}
            //foreach (CheckBox c in this.wrapPanel2.Children)
            //{
            //    if ((bool)c.IsChecked)
            //    {
            //        results += c.Content + ", ";
            //    }
            //}
            //foreach (CheckBox c in this.wrapPanel3.Children)
            //{
            //    if ((bool)c.IsChecked)
            //    {
            //        results += c.Content + ", ";
            //    }
            //}

            //return results;
        }

        private void FailButton_Click(object sender, RoutedEventArgs e)
        {
            failFile();
            getANewFile();
            updateFileCount();
        }

        private void clearFolderButton_Click(object sender, RoutedEventArgs e)
        {
            adjudicator.ClearUserFolder(this.currentUserFolder);
            this.currentFile = null;
            this.FileName.Content = "None Selected";
            this.UpdateLayout();
        }

        private void reportText_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.currentReport = TextBox_report.Text;
        }

        private void ViewProjectInfoButton_Click(object sender, RoutedEventArgs e)
        {
            
            // Get the six strings that we need in order to create the new window.
            if (this.adjudicator != null && this.currentProject != null)
            {
                ProjectInfoWindow w = new ProjectInfoWindow(MainWindow.CurrentProjectObjectsDict[ComboBox_Project.SelectedItem.ToString()]);
                w.Show();
            }
        }

        private void Button_globalLog_Click(object sender, RoutedEventArgs e)
        {
            logger.openGlobalLog(currentProject);
        }

        private void Button_deleteAllInFolder_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in currentUserFolder.GetFiles())
            {
                if (utility.isFileWritable(file))
                {
                    //File.Move(file.FullName, utility.getProjectFileFromString(currentProject).FullName) 

                }
            }
        }

        private void Button_userInfo_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentUser != null)
            {
                try
                {
                    UserPage p = new UserPage(this.currentUser);
                    p.Show();
                }
                catch (Exception)
                {
                    MessageBox.Show("No User Data Exists");
                }
            }

        }

        void failFile()
        {
            //make sure the user reports why the files failed
            
            if (!checkIfFailCautionHasBeenReported())
            {
                MessageBox.Show("Please enter what was wrong with the file, or I'll have to kill you.");
                return;
            }

            //update the report
            currentReport = TextBox_report.Text;

            

            if (this.currentFile == null)
            {

                return;
            }

            // make our adjudicator pass the file
            // Start a new thread in order to do this quickly
            BackgroundWorker worker = new BackgroundWorker();

            string checks = this.getQCKeyWords();

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (this.currentMovie != null)
                {
                    try
                    {
                        this.currentMovie.Kill();
                    }
                    catch (Exception badjuju)
                    {
                        if (!badjuju.Message.Contains("exited"))
                        {
                            MessageBox.Show("Please close the movie and click ok");
                        }

                        this.currentMovie = null;
                    }

                    this.currentMovie = null;
                }


                TimeSpan since = (DateTime.Now - this.currentFileTime);
                utility.addFileAndTimeToUser(since.Seconds, this.currentUser);
                utility.addFileAndTimeToUser(since.Seconds, "QC");
                this.adjudicator.ProcessFile(this.currentFile, this.currentUser, this.currentUserInitials, this.currentProject);
                utility.addOneToPassFailCaution("Failed", this.currentUser);
                utility.addOneToPassFailCaution("Failed", "QC");
                
            };
            worker.RunWorkerAsync();

            //clear report if the user wants to
            clearReport();

            if (this.currentFile != null)
            {
                System.Diagnostics.Process movie = System.Diagnostics.Process.Start(this.currentFile.CurrentFileInfo.FullName);

                this.currentMovie = movie;
            }
        }

        /// <summary>
        /// checks to see if the user reported why something passed or failed
        /// </summary>
        /// <returns>true if the report textbox has text or at least one qc keyword has been check</returns>
        private bool checkIfFailCautionHasBeenReported()
        {
            foreach (var keyword in qc_keywords)
            {
                if (TextBox_report.Text != "")
                {
                    return true;
                }
                if ((bool)keyword.IsChecked)
                {
                    return true;
                }
            }
            return false;
        }

        private void Button_EnterTimecode_Click(object sender, RoutedEventArgs e)
        {
            //currentReport += "timecode";
            Timecode currentTimeCode = new Timecode();
            currentTimeCode.ShowDialog();
        }

        private void Window_Adjudicator_Loaded(object sender, RoutedEventArgs e)
        {
            //load last user and project
            ComboBox_user.SelectedValue = Settings.Default.lastUser;
            foreach (var project in ComboBox_Project.Items)
            {
                if ((string)project == Settings.Default.lastProject)
                {
                    ComboBox_Project.SelectedValue = Settings.Default.lastProject;
                }
            }
        }

        private void Window_Adjudicator_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.lastUser = ComboBox_user.SelectedValue.ToString();
            Settings.Default.lastProject = ComboBox_Project.SelectedValue.ToString();
            Settings.Default.Save();
        }

        private void clearReport()
        {
            //if ((bool)!CheckBox_KeepReason.IsChecked)
            //{
            //    foreach (var keyword in qc_keywords)
            //    {
            //        keyword.IsChecked = false;
            //    }
            //    TextBox_report.Text = "";
            //}
        }
    }
}
