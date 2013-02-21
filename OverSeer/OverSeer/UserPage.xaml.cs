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

namespace OverSeer
{
    /// <summary>
    /// Interaction logic for UserPage.xaml
    /// </summary>
    public partial class UserPage : Window
    {
        public UserPage(string userName)
        {
            FileInfo userXml = new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\UserStats\" + userName + ".xml");
            InitializeComponent();

            this.userLabel.Content = System.IO.Path.GetFileNameWithoutExtension(userXml.Name);

            string trainingCompleted = utility.getValueFromXML(userXml, "Training");

            if (trainingCompleted.Contains("NA"))
            {
                throw new Exception();
            }

            if(trainingCompleted.Contains("web"))
            {
                this.webdeliverable.Opacity = 100;
            }
            if (trainingCompleted.Contains("diva"))
            {
                this.diva.Opacity = 100;
            }
            if (trainingCompleted.Contains("mam"))
            {
                this.mam.Opacity = 100;
            }
            if (trainingCompleted.Contains("ingest"))
            {
                this.ingest.Opacity = 100;
            }
            if (trainingCompleted.Contains("language"))
            {
                this.language.Opacity = 100;
            }
            if (trainingCompleted.Contains("msw"))
            {
                this.msw.Opacity = 100;
            }

        }
    }
}
