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
using System.IO;

namespace OverSeer
{
    /// <summary>
    /// Interaction logic for Logs.xaml
    /// </summary>
    public partial class Logs : Window
    {
        //vars
        DirectoryInfo logsDirectory;
        FileInfo[] files;

        public Logs(DirectoryInfo logsDirectory)
        {
            InitializeComponent();

            files = logsDirectory.GetFiles();

            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains("temp"))
                {
                    ComboBox_Directories.Items.Add(file);
                }
            }
            //ComboBox_Directories.DataContext = files;


        }

        private void Button_Log_Click(object sender, RoutedEventArgs e)
        {
            //TODO: only display the non-temp logs so the user doesn't open those by mistake
            //TODO: check to see if the log temp log is already open.  If so, recursively rename it.
            try
            {
                FileInfo currentLog = (FileInfo)ComboBox_Directories.SelectedItem;
                string destFileName = currentLog.Directory + "\\" + System.IO.Path.GetFileNameWithoutExtension(currentLog.FullName) + "temp" + currentLog.Extension;
                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }
                File.Copy(currentLog.FullName, destFileName);

                utility.openInExplorer(new FileInfo(destFileName));
            }
            catch (Exception exception)
            {
                logger.writeErrorLog("Could not open logs");
                MessageBox.Show(exception.ToString());
            }
        }

        private void ComboBox_Directories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
