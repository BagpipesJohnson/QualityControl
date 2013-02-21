using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverSeer
{
    public static class photographer
    {
        /// <summary>
        /// This logs every file that we have ever gotten BEFORE we start our work on it. It then moves each file in the drop folder
        /// to be accessed by AutoQC
        /// </summary>
        /// <param name="dropFolder"></param>
        /// <returns></returns>
        public static bool snapShot(System.IO.DirectoryInfo dropFolder)
        {
            //get all of the files

            System.IO.FileInfo[] files = dropFolder.GetFiles();

            

            List<System.IO.FileInfo> cleanfileList = utility.checkForSystemFiles(files.ToList<System.IO.FileInfo>());

            List<System.IO.FileInfo> toMove = new List<System.IO.FileInfo>();

            foreach (System.IO.FileInfo file in cleanfileList)
            {
                if (utility.isFileWritable(file))
                {
                    // Use the logger to log this
                    
                    System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\");
                    
                    //USED FOR TESTING
                    //System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(@"C:\Users\BagpipesJohnson\Desktop\testing\Target");
                    
                    logger.saveToTXT("FilesReceived", logsDirectory, file.Name + '\t' + file.FullName + '\t' + file.CreationTime + '\t' + DateTime.Now + '\t' + file.Length + "\r\n");
                    logger.saveToTabDilimited("FilesReceived", logsDirectory, file.Name + '\t' + file.FullName + '\t' + file.CreationTime + '\t' + DateTime.Now + '\t' + file.Length + "\r\n");

                    // Create the metaData for the AutoQC bot to use
                    toMove.Add(file);
                }
            }
            taskmaster.moveAfterSnapshot(toMove);

            return false;
        }
    }
}
