using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OverSeer
{
    static class Joshua
    {
        public static void AddToDeliveredReport(FileInfo file)
        {

             System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\");
             logger.saveToTXT("FilesDelivered", logsDirectory, file.Name + '\t' + file.FullName + '\t' + file.CreationTime + '\t' + DateTime.Now + '\t' + file.Length + "\r\n");
             logger.saveToTabDilimited("FilesDelivered", logsDirectory, file.Name + '\t' + file.FullName + '\t' + file.CreationTime + '\t' + DateTime.Now + '\t' + file.Length + "\r\n");


        }

        public static bool isFileInDirectory(DirectoryInfo directory, FileInfo file)
        {

            System.IO.FileInfo[] filesInDirectory = directory.GetFiles();
            foreach (System.IO.FileInfo temp in filesInDirectory)
            {
                if (temp.Name == file.Name)
                {
                   
                    return true;
                }
            }

            return false;
        }
    }
}
