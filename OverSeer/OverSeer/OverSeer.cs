using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class OverSeer : Form
    {
       
        public OverSeer()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
         System.IO.DirectoryInfo dropFolder = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\");

         photographer.snapShot(dropFolder);

        }
    }
}
