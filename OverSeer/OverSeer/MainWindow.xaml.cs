using System;
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
        public List<FileObjects> CurrentFileObjects { get; set; }

        public bool keepRunning;

        public MainWindow()
        {
            keepRunning = true;
            InitializeComponent();
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
            System.IO.DirectoryInfo watchFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\");
            System.IO.DirectoryInfo xmlDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\");
            
            rectifier r = new rectifier(watchFolder);

            List<System.IO.FileInfo> filesToRectify = utility.checkForSystemFiles(r.getDirectoryFiles(watchFolder));

            r.createXMLs(filesToRectify, xmlDirectory);
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
            ProjectManagment projectManager = new ProjectManagment();
            projectManager.Show();
        }
    }
}
