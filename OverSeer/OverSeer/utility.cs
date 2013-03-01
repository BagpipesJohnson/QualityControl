using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace OverSeer
{
    static class utility
    {
        // add the move code
        /// <summary>
        /// This moves a specified file to a new location. 
        /// </summary>
        /// <param name="file">The complete file name--including the path-- of the file to be moved</param>
        /// <param name="endLocation">The Path to the target directory</param>
        /// <returns>Returns 0 on error, and 1 on a success</returns>
        static public string moveFile(FileObjects file, System.IO.DirectoryInfo endLocation)
        {
            try
            {
                //check to see if the file is exclusively writable
                if (!isFileWritable(file.CurrentFileInfo))
                {
                    throw new IOException("File is not free to move");
                }

                //check to see if the destination directory exists.  If not, create it.
                if (!Directory.Exists(endLocation.FullName))
                {
                    Directory.CreateDirectory(endLocation.FullName);
                }
                string shortFileName = file.CurrentFileInfo.Name;
                string endLocationWithFile = endLocation + shortFileName;

                //handle duplicates
                if(File.Exists(endLocationWithFile))
                {
                //TODO: backup duplicate, move, then delete
                // Meant to over-ride duplicates to avoid exceptions
                System.IO.File.Delete(endLocationWithFile);
                }

                //Do the heavy moving
                System.IO.File.Move(file.CurrentFileInfo.FullName, endLocationWithFile);
                
                //System.Threading.Thread.Sleep(1000);
                //FileInfo deliveredFile = new FileInfo(endLocationWithFile);

                //if (Joshua.isFileInDirectory(endLocation, deliveredFile))
                //{
                //    Joshua.AddToDeliveredReport(deliveredFile);
                //}

                System.IO.FileInfo[] filesInDirectory = endLocation.GetFiles();
                foreach (System.IO.FileInfo temp in filesInDirectory)
                {
                    if (temp.Name == shortFileName)
                    {
                        file.UpdateCurrentLocation(new FileInfo(endLocationWithFile));
                        return "Moved Successfully";
                    }
                }
                
                // The file we sent over has not gotten to the correct directory....something went wrong!
                throw new IOException("File did not reach destination");

            }
            catch (Exception e)
            {
                if (e.Message.Contains("not free") || e.Message.Contains("being used"))
                {
                    logger.writeErrorLog("File was not free to move: " + file.CurrentFileInfo.Name);
                    return "File not free to move";
                }
                //Something went wrong, return a fail;
                logger.writeErrorLog("File could not be moved:" + file.CurrentFileInfo.Name);
                return "Did Not Arrive";
            }
        }

        /// <summary>
        /// This moves a specified file to a new location. 
        /// </summary>
        /// <param name="file">The complete file name--including the path-- of the file to be moved</param>
        /// <param name="endLocation">The Path to the target directory</param>
        /// <returns>Returns 0 on error, and 1 on a success</returns>
        static public string moveFile(FileInfo file, System.IO.DirectoryInfo endLocation)
        {
            try
            {
                //check to see if the file is exclusively writable
                if (!isFileWritable(file))
                {
                    throw new IOException("File is not free to move");
                }

                //check to see if the destination directory exists.  If not, create it.
                if (!Directory.Exists(endLocation.FullName))
                {
                    Directory.CreateDirectory(endLocation.FullName);
                }
                string shortFileName = file.Name;
                string endLocationWithFile = endLocation + shortFileName;

                //handle duplicates
                if (File.Exists(endLocationWithFile))
                {
                    //TODO: backup duplicate, move, then delete
                    // Meant to over-ride duplicates to avoid exceptions
                    System.IO.File.Delete(endLocationWithFile);
                }

                //Do the heavy moving
                System.IO.File.Move(file.FullName, endLocationWithFile);

                //System.Threading.Thread.Sleep(1000);
                //FileInfo deliveredFile = new FileInfo(endLocationWithFile);

                //if (Joshua.isFileInDirectory(endLocation, deliveredFile))
                //{
                //    Joshua.AddToDeliveredReport(deliveredFile);
                //}

                System.IO.FileInfo[] filesInDirectory = endLocation.GetFiles();
                foreach (System.IO.FileInfo temp in filesInDirectory)
                {
                    if (temp.Name == shortFileName)
                    {

                        return "Moved Successfully";
                    }
                }

                // The file we sent over has not gotten to the correct directory....something went wrong!
                throw new IOException("File did not reach destination");

            }
            catch (Exception e)
            {
                if (e.Message.Contains("not free") || e.Message.Contains("being used"))
                {
                    logger.writeErrorLog("File was not free to move: " + file.Name);
                    return "File not free to move";
                }
                //Something went wrong, return a fail;
                logger.writeErrorLog("File could not be moved:" + file.Name);
                return "Did Not Arrive";
            }
        }

        /// <summary>
        /// Use this to get the project file from an xml file
        /// </summary>
        /// <param name="file">Overseer generated XML containing the project name</param>
        /// <returns></returns>
        public static FileInfo getProjectFile(FileInfo file)
        {
            string currentProject = utility.getValueFromXML(file, "Project");

            foreach (FileInfo tempProject in utility.checkForSystemFiles(new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\").GetFiles().ToList<FileInfo>()))
            {
                string tempName = utility.getValueFromXML(tempProject, "Name");
                if (tempName == currentProject)
                {
                    return tempProject;
                    
                }
            }
            return null;
        }


        public static FileInfo getUserStatsFromString(string userName)
        {
            return new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\UserStats\" + userName + ".xml");
        }

        public static void addFileAndTimeToUser(int seconds, string user)
        {
            FileInfo xmlFile = getUserStatsFromString(user);

            string totalTime = getValueFromXML(xmlFile, "TotalTime");

            int intTime = int.Parse(totalTime);

            intTime = intTime + seconds;
            writeToXML(xmlFile, "Stats", "TotalTime", intTime.ToString());
            
            string totalFiles = getValueFromXML(xmlFile, "FilesQCd");

            int allFiles = int.Parse(totalFiles);
            allFiles++;

            writeToXML(xmlFile, "Stats", "FilesQCd", allFiles.ToString());

        }

        public static void writeToXML(System.IO.FileInfo xmlFile, string parentElement, string element, string value)
        {
            XDocument xml = XDocument.Load(xmlFile.FullName);
            xml.Element(parentElement).Element(element).Value = value;
            xml.Save(xmlFile.FullName);
        }


        public static FileInfo getProjectFileFromString(string currentProject)
        {
            foreach (FileInfo tempProject in utility.checkForSystemFiles(new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\").GetFiles().ToList<FileInfo>()))
            {
                string tempName = utility.getValueFromXML(tempProject, "Name", true);
                if (tempName == currentProject)
                {
                    return tempProject;
                }
            }
            return null;
        }
        /// <summary>
        /// determines if a file is exclusively writable or is still copying over, being opened somewhere else and
        /// being written to, or read-only.
        /// </summary>
        /// <param name="file">the file to test</param>
        /// <returns>
        ///     true if the file is exclusively writable, false if otherwise or there was any problem.
        ///     Note: it will log the exception if there is any other error 
        /// </returns>
        public static bool isFileWritable(FileInfo file)
        {
            try
            {
                //try to open the file with write permissions.  If the files is still moving it won't let you and will throw an exception
                using (File.Open(file.FullName, FileMode.Open, FileAccess.Write)) { };

                return true;
            }
            catch (IOException ioException) //the file is still moving
            {
                return false;
            }
            catch (UnauthorizedAccessException UnauthorizedAccessException) //the files has already been picked up and made Read Only
            {
                return false;
            }
            catch (Exception exception) //something else went wrong
            {
                logger.writeErrorLog(exception, file.FullName);
                return false;
            }
        }

        public static bool isMezzanine(FileObjects file)
        {
            if (file.CurrentFileInfo.Name.Contains("8000k") || file.CurrentFileInfo.Name.Contains("1080p"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int getNumberOfFilesInProject(ProjectObject project)
        {
            //int count = 0;
            // Find the watch folder
            try
            {
                //DirectoryInfo watchFolder = new DirectoryInfo(utility.getValueFromXML(xmlFile, "Watchfolder"));
                // Count all of the files in the watch folder with the right project name
                //string[] keyWords = utility.getValueFromXML(xmlFile, "Keyword").Split(',');
                List<string> projectFiles = new List<string>();

                foreach (FileInfo file in project.Watchfolder.GetFiles()) //TODO: do we need to check for access violations?
                {
                    foreach (string key in project.Keywords)
                    {
                        if (file.Name.Contains(key))
                        {
                            projectFiles.Add(file.Name);
                            //count++;
                            //break;
                        }
                    }
                }
                return projectFiles.Count;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public static FileInfo getXMLForAsset(FileInfo file)
        {
            string tentativexml = @"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\" + file.Name + ".xml";
            if (File.Exists(tentativexml))
            {
                FileInfo xml = new FileInfo(tentativexml);
                return xml;
            }
            return null;
        }

        public static string getValueFromXML(FileInfo file, string target, bool useEffectiveway)
        {
            XDocument projectXML = XDocument.Load(file.FullName);
            string value = projectXML.Element("Project").Element(target).Value;
            return value;
        }

        public static string getValueFromXML(FileInfo file, string target)
        {
            try
            {
                if (file == null)
                {
                    return null;
                }
                // Open up an xml reader on the xml
                // Look inside of each one's key words
                XmlTextReader reader = new XmlTextReader(file.FullName);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (reader.Name == target)
                            {
                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    //return reader.Value;
                                    string temp = reader.Value;
                                    reader.Close();
                                    return temp;
                                }
                            }
                            break;
                    }

                }
                reader.Close();
                return "NA";
            }
            catch
            { 
                return "NA"; 
            }
        }

        /// <summary>
        /// removes system files from a list of files
        /// </summary>
        /// <param name="files">a list of files</param>
        /// <returns>a list of files with all system files removed</returns>
        public static List<FileInfo> checkForSystemFiles(List<FileInfo> files)
        {
            List<FileInfo> cleanFiles = new List<FileInfo>();

            foreach (var file in files)
            {
                //dot and thumb files
                if (file.Name.IndexOf('.') != 0 &&
                    !file.Name.Contains("Thumb"))
                {
                    cleanFiles.Add(file);
                }
            }
            return cleanFiles;
        }

        /// <summary>
        /// opens a file in windows explorer.exe
        /// </summary>
        /// <param name="source">the file to open</param>
        public static void openInExplorer(FileInfo source)
        {
            //open the folder location in explorer
            if (source.Exists)
            {
                try
                {
                    string windir = Environment.GetEnvironmentVariable("WINDIR");
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = windir + @"\explorer.exe";
                    process.StartInfo.Arguments = source.FullName;
                    process.Start();
                }
                catch (Exception exception)
                {
                    logger.writeErrorLog(exception, source.FullName);
                }
            }
            else
            {
                logger.writeErrorLog(new Exception("Cannot open path or file: "), source.FullName);
            }
        }

        //TODO: make a delegate that will do the following:
            //1) do a directory read
            //2) gain write access to all possible files
            //3) create a temp indicating who has exclusive access to the file
            //4) perform an action on the files (thus requiring a delegate to accept functions as parameters)
            //5) move the files
            //6) let go of the files
            //7) log the move

        public static bool MoveThisFile(FileInfo file, DirectoryInfo destination)
        {
            //check to see if directory exists
            //warn user directory does not exist if it doesn't, then create it
            //see if file is in use
            //if files is not in use, lock it
            //if file is in use, warn user
            //check to see if file is already in destination
            //if  yes, update Revision number and record previous version number
            //prompt to understand what is different about this version
            //if file already exists, add temp name
            //move file
            //unlock file if move is successful and return true
            //log move
            //delete temp named copy
            //update FileStatus
            //catch any fails and report them
            return false;
        }

        public static void addOneToPassFailCaution(string which, string user)
        {
            FileInfo xmlFile = getUserStatsFromString(user);

            string value = getValueFromXML(xmlFile, which);

            int incremented = int.Parse(value) + 1;

            writeToXML(xmlFile, "Stats", which, incremented.ToString());
        }

    }
}
