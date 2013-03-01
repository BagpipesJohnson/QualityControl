using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OverSeer
{
    static class taskmaster
    {
        public static void cleanUpToBeDeleted()
        {
            // Get all of the files in ToBeDeleted
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo toBeDeleted = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\toBeDeleted\TestToBeDeleted\");
            
            //create the toBeDeleted folder if it does not exist
            if (!Directory.Exists(toBeDeleted.FullName))
            {
                Directory.CreateDirectory(toBeDeleted.FullName);
            }

            foreach (FileInfo file in utility.checkForSystemFiles(toBeDeleted.GetFiles().ToList<FileInfo>()))
            {
                files.Add(file);
            }

            // Check the age of each of those.
            int expirationDate = 14;
            TimeSpan since;
            List<String> toDelete = new List<String>();
            foreach (FileInfo oldFile in files)
            {
                since = (DateTime.UtcNow - oldFile.LastWriteTime);

                if (since.TotalDays > expirationDate)
                {
                    toDelete.Add(oldFile.FullName);
                }

            }

            // Delete all of the old files
            foreach (string s in toDelete)
            {
                File.Delete(s);
            }
            
        }
        public static void runPhotographer(System.IO.DirectoryInfo dropFolder)
        {
            photographer.snapShot(dropFolder);
        }

        public static void moveAfterSnapshot(List<System.IO.FileInfo> files)
        {
            System.IO.DirectoryInfo targetDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\");

            foreach (System.IO.FileInfo file in files)
            {
                if (utility.moveFile(file, targetDirectory) == "Passed")
                {
                    continue;
                }
                else
                    logger.writeGeneralErrorLog("Could not move the file: " + file.Name + " After Taking A Snapshot");
            }
        }

        public static bool moveAfterAutoQCPass(FileObjects file)
        {
            System.IO.DirectoryInfo targetDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCPassed\");

            if (utility.moveFile(file, targetDirectory) == "Passed")
            {
                return true;
            }
            else
                return false;
        }

        public static bool moveAfterAutoQCFail(System.IO.FileInfo file)
        {
            System.IO.DirectoryInfo targetDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCFailed\");

            if (utility.moveFile(file, targetDirectory) == "Passed")
            {
                return true;
            }
            else
                return false;
        }

        public static void runAutoQC(System.IO.DirectoryInfo watchFolder)
        {
            autoQC.run(watchFolder);
        }

    }
}
