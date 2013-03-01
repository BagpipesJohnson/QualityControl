﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
//added
using System.IO;
using System.Threading;

//TODO:  make it so if you close the main window it will close all other child windows and close the program
namespace OverSeer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<FileObjects> CurrentFileObjects { get; set; }
        static public List<ProjectObject> CurrentProjectObjects { get; set; }
        static public Dictionary<string, ProjectObject> CurrentProjectObjectsDict { get; set; }
        public DirectoryInfo ProjectFolder { get; set; }

        public bool keepRunning;

        //private vars
        private DirectoryInfo currentProjectFolder = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects");

        public MainWindow()
        {
            keepRunning = true;
            InitializeComponent();
            CurrentFileObjects = new List<FileObjects>();
            CurrentProjectObjects = new List<ProjectObject>();
            CurrentProjectObjectsDict = new Dictionary<string, ProjectObject>();

            //load projects
            ProjectFolder = currentProjectFolder;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\");

            taskmaster.runPhotographer(dropFolder);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing");

            taskmaster.runAutoQC(dropFolder);

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            keepRunning = true;

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                while (keepRunning)
                {
                    rectify();

                    System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\");
                    taskmaster.runPhotographer(dropFolder);

                    dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing");
                    taskmaster.runAutoQC(dropFolder);

                }
            };
            worker.RunWorkerAsync();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            keepRunning = false;
          
        }

        private void Rectify_Click(object sender, RoutedEventArgs e)
        {
            rectify();

        }

        private void rectify()
        {
            System.IO.DirectoryInfo rectifyWatchFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\");
            System.IO.DirectoryInfo xmlDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\");

            rectifier r = new rectifier(rectifyWatchFolder);

            List<System.IO.FileInfo> filesToRectify = utility.checkForSystemFiles(r.getDirectoryFiles(rectifyWatchFolder));
            
            //r.createXMLs(filesToRectify, xmlDirectory);
            //FileInfo possibleXML = new FileInfo(System.IO.Path.Combine(xmlDirectory.FullName),
            CurrentFileObjects = r.createFileObjects(filesToRectify, xmlDirectory);

        }

        private void BeginQCButton_Click(object sender, RoutedEventArgs e)
        {
            AdjudicatorWindow QCWindow = new AdjudicatorWindow();
            QCWindow.Show();
        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            
            //TODO: hook this in the overseer settings
            Logs logs = new Logs(new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\"));
            logs.Show();
        }

        private void CheckBox_Loop_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBox_Loop.IsChecked)
            {
                keepRunning = true;

                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += delegate(object s, DoWorkEventArgs args)
                {
                    while (keepRunning)
                    {
                        rectify();

                        System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\");
                       // taskmaster.runPhotographer(dropFolder);

                        dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing");
                      //  taskmaster.runAutoQC(dropFolder);

                        // Add code to clean up really old files in the to-be-deleted folder. AND ONLY THOSE FILES!!!
                        taskmaster.cleanUpToBeDeleted();

                    }
                };
                worker.RunWorkerAsync();

                BackgroundWorker worker2 = new BackgroundWorker();
                worker2.DoWork += delegate(object s, DoWorkEventArgs args)
                {
                    while (keepRunning)
                    {
           

                        System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\");
                        taskmaster.runPhotographer(dropFolder);

                    }
                };
                worker2.RunWorkerAsync();

                BackgroundWorker worker3 = new BackgroundWorker();
                worker3.DoWork += delegate(object s, DoWorkEventArgs args)
                {
                    while (keepRunning)
                    {


                        System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing");
                          taskmaster.runAutoQC(dropFolder);

                    }
                };
                worker3.RunWorkerAsync();

            }
            else
            {
                keepRunning = false;
            }
        }

        private void Button_UpdateQueues_Click(object sender, RoutedEventArgs e)
        {
            this.Button_UpdateQueues.IsEnabled = false;
             BackgroundWorker worker = new BackgroundWorker();

             worker.DoWork += delegate(object s, DoWorkEventArgs args)
             {
                 StatusReporter reporter = new StatusReporter();
                 reporter.updateQueues();
             };
             worker.RunWorkerAsync();

             this.Button_UpdateQueues.IsEnabled = true;
        }

        private void Button_ProjectManagement_Click(object sender, RoutedEventArgs e)
        {
            prepProjects();

            ProjectManagment projectManager = new ProjectManagment();
            projectManager.Show();
        }

        /// <summary>
        /// creates a projectObject for each project xml and adds them to a list of projects
        /// </summary>
        /// <param name="projectFolder">folder containing project xmls</param>
        private void LoadProjects(DirectoryInfo projectFolder)
        {

            //remove system files and put project files in a list
            List<FileInfo> projectFiles = utility.checkForSystemFiles(projectFolder.GetFiles().ToList<FileInfo>());

            //create a project for each project xml and add them to a list of projects
            foreach (var file in projectFiles)
            {
                ProjectObject newProject = new ProjectObject(file);
                CurrentProjectObjects.Add(newProject);
            }
        }

        private void prepProjects()
        {
            //clear any old project info
            CurrentProjectObjects.Clear();
            CurrentProjectObjectsDict.Clear();

            //populate the projects
            LoadProjects(ProjectFolder);

            //convert to dictionary entries
            foreach (var project in CurrentProjectObjects)
            {
                bool alreadyExists = false;

                //if key already exists, skip it
                foreach (var entry in CurrentProjectObjectsDict)
                {
                    if (entry.Key == project.ProjectName)
                    {
                        alreadyExists = true;
                    }
                }

                if (!alreadyExists)
                {
                    CurrentProjectObjectsDict.Add(project.ProjectName, project);
                }
            }
        }
    }
}
