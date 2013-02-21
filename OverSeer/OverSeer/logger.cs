using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.IO;

namespace OverSeer
{
    /// <summary>
    /// provides two method utilites, saveToTXT and saveToTabDilimited, to help log files
    /// </summary>
    static class logger
    {
        private static DirectoryInfo globalLogDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs");
        /// <summary>
        /// saves a string of text to a .txt file in a specified directory with a specified name
        /// </summary>
        /// <param name="logName">name of the log (note: this is only the name not the name and the path)</param>
        /// <param name="whereToSave">the directory path to where the log will be saved</param>
        /// <param name="textToSave">the contents of the log</param>
        static public void saveToTXT(string logName, DirectoryInfo whereToSave, string textToSave)
        {
            try
            {
                //make log directory if it doesn't exist
                if (!Directory.Exists(whereToSave.FullName))
                {
                    Directory.CreateDirectory(whereToSave.FullName);
                }
                
                //adds the filename to the end of the log directory
                string pathAndNameLog = System.IO.Path.Combine(whereToSave.FullName, logName + ".txt");

                //appends the string text to the file
                File.AppendAllText(pathAndNameLog, textToSave);
            }
            catch(Exception exception)
            {
                writeErrorLog("could not save to text: " + textToSave); 
            }
        }

        /// <summary>
        /// Saves a string of text to a .tab file in a specified directory with a specified name.
        /// Note: tab is the deliminator so you should use it to format the string text to be logged.
        /// </summary>
        /// <param name="logName">name of the log (note: this is only the name not the name and the path)</param>
        /// <param name="whereToSave">the directory path to where the log will be saved</param>
        /// <param name="textToSave">the contents of the log</param>
        static public void saveToTabDilimited(string logName, DirectoryInfo whereToSave, string textToSave)
        {
            
            try
            {
                //make log directory
                if (!Directory.Exists(whereToSave.FullName))
                {
                    Directory.CreateDirectory(whereToSave.FullName);
                }

                string pathAndNameCentralLog = System.IO.Path.Combine(whereToSave.FullName, logName + ".tab");

                File.AppendAllText(pathAndNameCentralLog, textToSave);
            }
            catch(Exception exception)
            {
                writeErrorLog("Could Not Save to Tab Delimited: " + textToSave); 
            }
        }

        /// <summary>
        /// writes and error message to a log .txt file
        /// </summary>
        /// <param name="errorMessage">the exception to be written to the log</param>
        static public void writeErrorLog(string errorMessage)
        {
            //check to see if log exists.  If not, make a new one.  
            FileInfo errorLog = new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\AutoQCLog\errorLog.txt");
            if (!File.Exists(errorLog.FullName))
            {
                File.Create(errorLog.FullName);
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(errorLog.FullName, true);
            string date = DateTime.Now.ToString();

            file.WriteLine(errorMessage + '\t' + date + '\n');
            file.Close();
        }

        /// <summary>
        /// writes and error message to a log .txt file
        /// </summary>
        /// <param name="errorMessage">the exception to be written to the log</param>
        /// <param name="fileName">the name of the log file (note: name, not path and name)</param>
        static public void writeErrorLog(Exception errorMessage, string fileName)
        {
            //check to see if log exists.  If not, make a new one.  
            FileInfo errorLog = new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\AutoQCLog\errorLog.txt");
            if (!File.Exists(errorLog.FullName))
            {
                File.Create(errorLog.FullName);
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(errorLog.FullName, true);
            string date = DateTime.Now.ToString();

            file.WriteLine(fileName + '\t' + errorMessage.ToString() + '\t' + date + '\n');
            file.Close();
        }

        public static void writeGeneralErrorLog(string error)
        {
                       //check to see if log exists.  If not, make a new one.  
            FileInfo errorLog = new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\ErrorLog.txt");
            if (!File.Exists(errorLog.FullName))
            {
                File.Create(errorLog.FullName);
            }

            try
            {

                System.IO.StreamWriter file = new System.IO.StreamWriter(errorLog.FullName, true);
                file.WriteLine(error + "\r\n");
                file.Close();
            }
            catch (Exception e)
            {

            }

        }

        public static void openGlobalLog(string project)
        {
            //TODO: only display the non-temp logs so the user doesn't open those by mistake
            //TODO: check to see if the log temp log is already open.  If so, recursively rename it.
            try
            {
                FileInfo currentLog = new FileInfo(Path.Combine(globalLogDirectory.FullName, project + ".tab"));
                FileInfo destFileName = new FileInfo(currentLog.FullName.Replace(".tab","") + Guid.NewGuid() + "_temp.tab");
                
                //delete all previously created temp logs
                foreach (FileInfo file in globalLogDirectory.GetFileSystemInfos())
                {
                    if (file.Name.Contains("temp"))
                    {
                        if(utility.isFileWritable(file))
                        {
                            File.Delete(file.FullName);
                        }
                    }
                }
               
                File.Copy(currentLog.FullName, destFileName.FullName);

                utility.openInExplorer(destFileName);
            }
            catch (Exception exception)
            {
                logger.writeErrorLog("Could not open global log for project: " + project + " Error message was: " + exception.Message);
                System.Windows.MessageBox.Show(exception.ToString());
            }
        }

        }//<<< who does this belong to?  TODO:
    }

