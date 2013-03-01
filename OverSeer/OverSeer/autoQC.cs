using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO; //used to get dir and file info
using System.Windows.Threading;
//using MediaInfoLib;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;

namespace OverSeer
{
    //public class testingSpecs
    //{
    //    public int bitRate;         //
    //    public int fileSize;        
    //    public int videoTracks;
    //    public int width, height;   //
    //    public string aspectRatio;     
    //    public string scanType;     //
    //    public int audioTracks;
    //    public int AudioSampleRate;
    //    public int frameRate;       //
    //    public int resolution;      //
    //    public FileInfo xmlFile;
    //    public FileInfo projectFile;
        

        //public testingSpecs(FileObjects file)
        //{
        //    //bool hasXml = false;
        //    // fill out the specs based on the file and the xml

        //    //// Search for an xml in the right directory
        //    //string tentativexml = @"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\" + file.CurrentFileInfo.Name + ".xml";
        //    //if(File.Exists(tentativexml))
        //    //{
        //    //    hasXml = true;
        //    //    FileInfo xml = new FileInfo(tentativexml);
        //    //    this.xmlFile = xml;
        //    //    this.projectFile = utility.getProjectFile(xml);
                
        //    //}

        //   bitRate = fileSize = videoTracks = width = height = audioTracks = AudioSampleRate = frameRate= resolution = -1;
        //   scanType = "";
        //   aspectRatio = "";
           
        //    // Parse the xml to get the info we need!
        //   //if (hasXml)
        //   //{
        //    string temp = utility.getValueFromXML(this.xmlFile, "BitRate");
        //    if (temp != "NA")
        //    {
        //        if (temp == "8000")
        //        {
        //            this.bitRate = 1000000;
        //        }
        //        if (temp == "1000")
        //        {
        //            this.bitRate = 500000;
        //        }
        //        if (temp == "300")
        //        {
        //            this.bitRate = 100000;
        //        }
        //        if (temp == "2500")
        //        {
        //            this.bitRate = 1800000;
        //        }
        //       //}

        //       temp = utility.getValueFromXML(this.xmlFile, "ScanType");
        //       if (temp != "NA")
        //       {
        //           this.scanType = temp;
        //       }

        //       temp = utility.getValueFromXML(this.xmlFile, "AspectRatio");
        //       if (temp != "NA")
        //       {
                   
        //           this.aspectRatio = temp;
        //       }

        //       temp = utility.getValueFromXML(this.xmlFile, "FrameRate");
        //       if (temp != "NA")
        //       {
        //           int doneInt = Int32.Parse(temp);
        //           this.frameRate = doneInt;
        //       }
        //   }
        //   return;
        //}

    //}

    
    static class autoQC
    {
       
        private static int aspectRatio = 0;  //TODO: possibly make this a local variable to the method for aspect ratio checks
        private static string watchfolder;

        private static System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\");
        private static DirectoryInfo passedFolder;
        private static DirectoryInfo failedFolder;

        private static DirectoryInfo xmlFolder = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\xmls");
        //TODO: make this work
        private static List<string> filesToSkip = new List<string>();  // a list of strings to find in filenames and skip checking certain files

        private static DispatcherTimer pollTimer = new DispatcherTimer();
        private static double pollFrequency;

        static int minimumFileSizeInKilobyes;

        static string xmlFilenameAndPath; 

        public static void run(DirectoryInfo dropFolder)
        {
            filesToSkip.Clear();

            //files to skip
            filesToSkip.Add("Music&theSpokenWord");
            filesToSkip.Add(".xml");

            //textBoxWatchfolder.Text = Properties.Settings.Default.Watchfolder;
            pollFrequency = 500;    //how often to check the watchfolder to see if files are ready to be processed
            minimumFileSizeInKilobyes = 600 * 1024;  //used for file size checks

            pollTimer.Interval = TimeSpan.FromMilliseconds(pollFrequency);

            xmlFilenameAndPath = @"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\AutoQCLog\AutoQcing\" + Guid.NewGuid().ToString() + ".xml";

            beginChecking();


        }




        private static void beginChecking()
        {
            

            //watchfolder = wheretoLook.FullName;

            //Check to see if the Google Docs has changed AutoQC status to "no"
            AutoQCStatusUpdate autoQCStatusUpdate = new AutoQCStatusUpdate();
            if (!autoQCStatusUpdate.AutoQC_ON)
            {
                return;
            }

            List<FileObjects> filesToCheck = new List<FileObjects>();
            //List<FileInfo> filesInWatchfolder = new List<FileInfo>();

            //find out which fileObjects are in autoQC
            foreach (var file in MainWindow.CurrentFileObjects)
            {
                if (file.CurrentFileStatus == FileStatus.autoQc)
                {
                    filesToCheck.Add(file);
                }
            }

            // check the files
            if (filesToCheck.Count > 0)
            {
                checkFiles(filesToCheck);
                filesToCheck.Clear();
                //filesInWatchfolder.Clear();
            }


            //if (Directory.Exists(watchfolder))
            //{
            //    //find the files in the directory
            //    DirectoryInfo dirInfo = new DirectoryInfo(watchfolder);
            //    filesInWatchfolder = dirInfo.GetFiles().ToList();

            //    //get rid of system files
            //    filesInWatchfolder = utility.checkForSystemFiles(filesInWatchfolder);

            //    //check if files are writable
            //    foreach (var file in filesInWatchfolder)
            //    {
            //        if (utility.isFileWritable(file))
            //        {
            //            filesToCheck.Add(file);
            //        }
            //    }

            //    //check the files
            //    if (filesToCheck.Count > 0)
            //    {
            //        checkFiles(filesToCheck);
            //        filesToCheck.Clear();
            //        filesInWatchfolder.Clear();
            //    }
            //}
            //else
            //{
            //    logger.writeGeneralErrorLog("The wathfolder does not exist " + DateTime.Now);
            //}

            //check files



            //BackgroundWorker backGroundWorker = new BackgroundWorker();
            //backGroundWorker.DoWork += delegate(object sender2, DoWorkEventArgs args2)
            //{
            //    try
            //    {


            //    }
            //    catch (Exception e)
            //    {

            //        String errorMessage = "Unsuccessful in checking files because " + e.Message + "/r/n" +
            //                          "---------------------------------------\r\n" + e.ToString();
            //    }
            //};
            //backGroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Checks files to see if they pass some Quality Control Checks and moves them to a passed or failed folder
        /// </summary>
        /// <param name="files">a list of files to be checked</param>
        private static void checkFiles(List<FileObjects> files)
        {
            //run checks
            

            foreach (var file in files)
            {
                if (file.CurrentFileInfo.Exists == false)
                {
                    throw new Exception("File Does not Exist and is in our list!");
                }
                //get any xmls out of the way first
                if (file.CurrentFileInfo.FullName.Contains(".xml"))
                {
                    try
                    {
                        string destination = System.IO.Path.Combine(xmlFolder.FullName, file.CurrentFileInfo.Name);

                        //overwrite
                        if (File.Exists(destination))
                        {
                            File.Delete(destination);
                        }

                        //move xmls to xml folder
                        File.Move(file.CurrentFileInfo.FullName, destination); // What happens to our iterative list if we move something out of it?!?!
                    }
                    catch (Exception exception)
                    {


                        logger.writeErrorLog("Something went wrong in auto QC: Could not move a file: " + file.CurrentFileInfo.Name);
                        //MessageBox.Show(exception.ToString());
                    }

                    continue;
                }
                else
                {
                    // HERE IS WHERE WE CREATE THE SPECS BASED ON THE FILE
                    //testingSpecs targetSpecs = new testingSpecs(file);

                    
                    //video---
                    //Can use new specs
                    if (!checkFileSize(file)) 
                        continue;
                    //Can use new specs
                    if (!checkVideoTrackCount(file)) 
                        continue;

                    if (!checkBitrate(file)) 
                        continue;

                    if (!checkResolution(file)) 
                        continue;

                    if (!checkAspectRatio(file)) 
                        continue;

                    if (!file.CurrentFileInfo.Name.Contains("DCVR")) //ignore DCVR files because they have variable frame rates
                    {
                        if (!checkFrameRate(file)) 
                            continue;
                    }

                    if (!checkScanType(file)) 
                        continue;


                    //audio
                    if (!checkAudioTrackCount(file)) 
                        continue;

                    if (!checkAudioSampleRate(file)) 
                        continue;

                    if (!checkForLongFile(file))
                        continue;
                    // length

                    
                }

                //move, log, and email failed files
                 
                passFile(file);
            }

            //clean up files
            files.Clear();
        }


        /// <summary>
        /// moves files into a passed folder and logs the files
        /// </summary>
        /// <param name="file">the file to process</param>
        private static void passFile(FileObjects file)
        {

            
            
                try
                {
                  
                    taskmaster.moveAfterAutoQCPass(file);

                }
                catch (Exception e)
                {

                    //MessageBox.Show("Could not move the file because: \n" + e.Message + "\n---------------------------------------\n" + e.ToString());
                    String errorMessage = "Could not move the file because: \n" + e.Message + "\n---------------------------------------\n" + e.ToString();
                    logger.writeErrorLog(e, file.CurrentFileInfo.Name);

                }
            


            //save to txt file
            logger.saveToTXT("AutoQCLog",logsDirectory, file.CurrentFileInfo.Name +
                      "\r\n------------------------------------------------------------\r\n" +
                      "passed on \n\r" +
                      DateTime.Now.ToString() +
                      "\r\n" +
                      "\r\n" +
                      "\r\n");

            logger.saveToTabDilimited("AutoQCTabLog", logsDirectory, file.CurrentFileInfo.Name + "\t\t" + DateTime.Now.ToString() + "\t" + "AutoQC" + "\r\n");
        }

        /// <summary>
        /// moves files to a failed folder and emails a recipient and logs the fail
        /// </summary>
        /// <param name="file">the file to be processed</param>
        /// <param name="report">the report to email</param>
        private static void failFile(FileObjects file)
        {
            StatusReporter reporter = new StatusReporter();
            reporter.addFail(file);

            //string projectFileName;
            //if (target.projectFile == null)
            //{
            //     projectFileName = "No Project";
            //}
            //else
            //{
            //     projectFileName = System.IO.Path.GetFileNameWithoutExtension(target.projectFile.Name);
            //}
            reporter.addResult(4, file);

            Adjudicator adjudicator = new Adjudicator();
            adjudicator.googleBot.logItConference(file.CurrentFileInfo.Name, file.QCReport);
                try
                {
                    taskmaster.moveAfterAutoQCFail(file.CurrentFileInfo);
                }
                catch (Exception e)
                {
                    logger.writeErrorLog(e, file.CurrentFileInfo.FullName);
                    //MessageBox.Show("Could not move the file because: \n" + e.Message + "\n---------------------------------------\n" + e.ToString());
                }
            

            // use the reporter for the mini sheet


            //save to txt file
            logger.saveToTXT("AutoQCLog", logsDirectory,file.CurrentFileInfo.Name +
                      "\r\n------------------------------------------------------------\r\n" +
                      "failed on \n\r" +
                      DateTime.Now.ToString() +
                      " because: " +
                      file.QCReport +
                      "\r\n" +
                      "\r\n" +
                      "\r\n");

            logger.saveToTabDilimited("AutoQCTabLog", logsDirectory, file.CurrentFileInfo.Name + "\t" + " :" + file.QCReport + "\t" + DateTime.Now.ToString() + "\t" + "AutoQC" + "\r\n");

            string emailMessage = "Dear User,\r\n\n" + "The following file: \r\n\n" + file.CurrentFileInfo.Name + "\r\n\n" + file.QCReport + "\r\n\n" + "Thanks, \r\n\n" + "QC Android Lady";

            //string recipient = utility.getValueFromXML(target.projectFile, "ToEmailOnFail");

            //if (recipient == null || recipient == "NA")
            //{
            //    recipient = "carlilelance@ldschurch.org";
            //}

            if (file.CurrentFileInfo.Name.Contains("Training"))
            {
                return;
            }
            email(file.CurrentFileInfo.Name + " failed auto-qc: " + file.QCReport, emailMessage, file.Project);
  
            //if(file.Name.Contains("2012-10-"))
            //{
            //    //track it on the Google doc
            //    new Googler(file.Name, report);
            //}
        }

        /// <summary>
        /// emails a string to a recipient
        /// </summary>
        /// <param name="report">the message to email</param>
        /// <param name="recipient">who to send the message to</param>
        /// /// <param name="subject">subject of the email</param>
        private static void email(string subject, string report, ProjectObject project)
        {
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();
            foreach (var emailRecipient in project.Emails)
            {
                email.To.Add(emailRecipient);
            }
            
            //email.To.Add("psd-qc@ldschurch.org");
            //email.To.Add("msd-compression@ldschurch.org");
            // email.To.Add("cjjohnson@ldschurch.org");
            email.Subject = subject;
            email.From = new System.Net.Mail.MailAddress("psd-qc@ldschurch.org");
            email.Body = report;
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.ldschurch.org");
            smtp.Send(email);

        }

        //TODO: make the bitrate sorting more efficient
        private static string getBitrateString(FileObjects file)
        {
            int kIndex = file.CurrentFileInfo.Name.IndexOf('k');

            return "";
        }

        /// <summary>
        /// check to make sure filesize is large enough for a good file
        /// </summary>
        /// <param name="file">file to check</param>
        /// <returns>result of the check</returns>
        private static bool checkFileSize(FileObjects file)
        {
            //if (targetSpecs.fileSize != -1)
            //{

            //TODO: make this more accurate.  Test for 300's different than 8000k's etc.
            if (file.CurrentFileInfo.Length > 600 * 1024)
            {
                return true;
            }
            else
            {
                file.QCReport = "failed because its filesize (" + (file.CurrentFileInfo.Length / 1024).ToString("#,##0") + "K) is too small";
                failFile(file);
                return false;
            }
            //}

            //if (file.CurrentFileInfo.Length > minimumFileSizeInKilobyes)
            //{
            //    return true;
            //}
            //else
            //{
            //    file.QCReport = "failed because its filesize (" + (file.CurrentFileInfo.Length / 1024).ToString("#,##0") + "K) is too small";
            //    failFile(file);
            //    return false;
            //}
        }

        /// <summary>
        /// checks to see if the bitrates are correct (not too high, not too low)
        /// </summary>
        /// <param name="file">the file to examine</param>
        /// <returns>returns whether the file's bitrate is correct or not</returns>
        private static bool checkBitrate(FileObjects file)
        {

            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            
            int bitrate = 0;
            
            string sBitrate = mediaInfo.Get(StreamKind.Video, 0, "BitRate");
            if(sBitrate.Contains('/'))
            {
                int badCharacterBegins = 0;
                badCharacterBegins = sBitrate.IndexOf('/');
                sBitrate = sBitrate.Remove(badCharacterBegins);
            }

            try
            {
                if (sBitrate != null)
                {
                    Int32.TryParse(sBitrate, out bitrate);
                }
                else
                {
                    bitrate = 0;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            file.QCReport = "failed because the bitrate was wrong (" + (bitrate / 1000).ToString() + " Kbps)";


            //if (targetSpecs.bitRate != -1)
            //{
            if (bitrate >= file.Bitrate)
            {
                mediaInfo.Close();
                return true;
            }
            failFile(file);
            mediaInfo.Close();
            return false;
            //}

            //if (file.Name.Contains("8000k"))
            //{
            //    if (bitrate < 1000000 || bitrate > 9000000)
            //    {
            //        failFile(file, badBitrateReport, targetSpecs);
            //        mediaInfo.Close();
            //        return false;
            //    }
            //}
            //else if (file.Name.Contains("2500k"))
            //{
            //    if (bitrate < 1000000 || bitrate > 3000000)
            //    {
            //        failFile(file, badBitrateReport, targetSpecs);
            //        mediaInfo.Close();
            //        return false;
            //    }
            //}
            //else if (file.Name.Contains("1800k"))
            //{
            //    if (bitrate < 800000 || bitrate > 2000000)
            //    {
            //        failFile(file, badBitrateReport, targetSpecs);
            //        mediaInfo.Close();
            //        return false;
            //    }
            //}
            //else if (file.Name.Contains("1000k"))
            //{
            //    if (bitrate < 500000 || bitrate > 1800000)
            //    {
            //        failFile(file, badBitrateReport, targetSpecs);
            //        mediaInfo.Close();
            //        return false;
            //    }
            //}
            //else if (file.Name.Contains("300k"))
            //{
            //    if (bitrate < 100000 || bitrate > 7000000)
            //    {
            //        failFile(file, badBitrateReport, targetSpecs);
            //        mediaInfo.Close();
            //        return false;
            //    }
            //}
            //else if (file.Name.Contains("2500k"))
            //{
            //    if (bitrate < 1800000 || bitrate > 3000000)
            //    {
            //        failFile(file, badBitrateReport, targetSpecs);
            //        mediaInfo.Close();
            //        return false;
            //    }
            //}

            //mediaInfo.Close();
            //return true;
        }

        private static bool checkForLongFile(FileObjects file)
        {
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            string length = mediaInfo.Get(StreamKind.General, 0, "Duration");
            int duration;
            int.TryParse(length, out duration);

            if (duration != null)
            {
                double minutes = duration / 60000;

                if (minutes > 240)
                {
                    file.QCReport = "is over 4 hours long!";
                    failFile(file);
                    mediaInfo.Close();
                    return false;
                }
               

            }
                mediaInfo.Close();
                return true;
        }
        /// <summary>
        /// checks to make sure the resolution is acceptable
        /// </summary>
        /// <param name="file">the file to be checked</param>
        /// <returns>the result of the check</returns>
        private static bool checkResolution(FileObjects file)
        {

            if (file.CurrentFileInfo.Extension.Contains(".mp3") ||
                file.CurrentFileInfo.Extension.Contains(".ac3") ||
                file.CurrentFileInfo.Extension.Contains(".aac"))
            {
                return true;
            }
           
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            int width;
            int height;

            string sWidth = mediaInfo.Get(StreamKind.Video, 0, "Width");
            string sHeight = mediaInfo.Get(StreamKind.Video, 0, "Height");

            if (sWidth == "")
            {
                width = 0;
            }
            else
            {
                width = Int32.Parse(sWidth);
            }

            if (sHeight == "")
            {
                height = 0;
            }
            else
            {
                height = Int32.Parse(sHeight);
            }

            //if (targetSpecs.width != -1)
            //{
                //if (targetSpecs.height != -1)
                //{

            //calculate the file width and height according to the fileObjects resolution if it exists
            if (file.Resolution != null ||
               file.Resolution != "")
            {
                int filewidth, fileheight;
                string[] tempResolution = file.Resolution.Split('x');
                int.TryParse(tempResolution[0], out filewidth);
                int.TryParse(tempResolution[1], out fileheight);

                //check against mediaInfo
                if (filewidth == width && fileheight == height)
                {
                    mediaInfo.Close();
                    return true;
                }
                else
                {
                    file.QCReport = "has an incorrect resolution (" + width + "x" + height + ")";
                    failFile(file);
                    mediaInfo.Close();
                    return false;
                }
            }
                //}


            //}

            //TODO: figure out why this code is unreachable
            //MSW crap
            if (file.CurrentFileInfo.Name.ToUpper().Contains("MUSIC&THESPOKENWORD") ||
                file.CurrentFileInfo.Name.ToUpper().Contains("MSW"))
            {
                if (file.CurrentFileInfo.Name.ToUpper().Contains("PAL"))
                {
                    if (width == 720 && height == 576)
                    {
                        mediaInfo.Close();
                        return true;
                    }
                }
            }



            //common checks
            if ((width == 640 || width == 1920 || width == 720 || width == 1280 ||
                 width == 480 || width == 486 || width == 460 || width == 1440) &&
                (height == 360 || height == 1080 || height == 480 || height == 720 ||
                 height == 360 || height == 960 || height == 248 || height == 486))
            {
                mediaInfo.Close();
                return true;
            }

            file.QCReport = "has an incorrect resolution (" + width + "x" + height + ")";
            failFile(file);
            mediaInfo.Close();
            return false;
        }

        /// <summary>
        /// checks to see if the framerate of a file is correct
        /// </summary>
        /// <param name="file">file to check</param>
        /// <returns>whether the file's framerate is correct</returns>
        private static bool checkFrameRate(FileObjects file)
        {
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            float frameRate;

            string tempFrameRate = mediaInfo.Get(StreamKind.Video, 0, "FrameRate"); //in case there is a null value
            if (tempFrameRate != "")
            {
                frameRate = float.Parse(tempFrameRate);
            }
            else
            {
                return true;
            }

            int intFrameRate = (int)(frameRate * 1000);

            //if (targetSpecs.frameRate != -1)
            //{
                if (file.FrameRate == intFrameRate)
                {
                    mediaInfo.Close();
                    return true;
                }
                else
                {
                    file.QCReport = "has an incorrect frame rate (" + intFrameRate + ")";
                    failFile(file);
                    mediaInfo.Close();
                    return false;

                }
            //}

            if (intFrameRate == 14984 ||
                intFrameRate == 29970 ||
                intFrameRate == 14985 ||
                intFrameRate == 14986 ||
                intFrameRate == 23976 ||
                intFrameRate == 15000 ||
                intFrameRate == 30000 ||
                intFrameRate == 25000 ||
                intFrameRate == 24000)
            {
                mediaInfo.Close();
                return true;
            }

            file.QCReport = "has an incorrect frame rate (" + intFrameRate + ")";
            failFile(file);
            mediaInfo.Close();
            return false;
        }

        /// <summary>
        /// Returns the frame rate of a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static int getFrameRate(FileObjects file)
        {
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            float frameRate;

            string tempFrameRate = mediaInfo.Get(StreamKind.Video, 0, "FrameRate"); //in case there is a null value
            if (tempFrameRate != "")
            {
                frameRate = float.Parse(tempFrameRate);
            }
            else
            {
                return -1;
            }

            int intFrameRate = (int)(frameRate * 1000);
            mediaInfo.Close();
            return intFrameRate;
        }
        /// <summary>
        /// checks a file to make sure it is progressive
        /// </summary>
        /// <param name="file">file to check</param>
        /// <returns>result of the check</returns>
        private static bool checkScanType(FileObjects file)
        {
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);

            string scanType = mediaInfo.Get(StreamKind.Video, 0, "ScanType");


            mediaInfo.Close();

            if (getFrameRate(file) != 23976)
            {
                if (scanType == "Interlaced")
                {
                    return true;
                }
            }

            //if (targetSpecs.scanType != "")
            //{
            if (file.ScanType == scanType)
            {
                mediaInfo.Close();
                return true;
            }
            else
            {
                file.QCReport = "has an incorrect scan type (" + scanType + ")";
                failFile(file);
                return false;
            }
            //}
            //string report = "has an incorrect scan type (" + scanType + ")";
            //failFile(file, report, targetSpecs);
            
            //return false;
        }

        /// <summary>
        /// checks the aspect ratio to make sure it is correct
        /// </summary>
        /// <param name="file">file to be check</param>
        /// <returns>the result of the check</returns>
        private static bool checkAspectRatio(FileObjects file)
        {
            if (!(file.CurrentFileInfo.Extension == ".ac3" || file.CurrentFileInfo.Extension == ".mp3"))
            {
                MediaInfo mediaInfo = new MediaInfo();
                mediaInfo.Open(file.CurrentFileInfo.FullName);
                //int aspectRatio;
                //TODO: get rid of this test try and make aspect ration a local variable
                try
                {
                    float aspectRatioFloat;
                    float.TryParse(mediaInfo.Get(StreamKind.Video, 0, "DisplayAspectRatio"), out aspectRatioFloat);
                    aspectRatio = (int)(aspectRatioFloat * 1000); //multiple by 1000 so the int doesn't cut of the decimal places
                }
                catch (Exception exception)
                {
                    logger.writeErrorLog("Could not parse aspect ratio for " + file.CurrentFileInfo.Name);
                    MessageBox.Show(exception.ToString());
                }

                //if (targetSpecs.aspectRatio != "")
                //{
                    if (file.AspectRatio != aspectRatio)
                    {
                        file.QCReport = "has an incorrect aspect ratio (" + aspectRatio + ")";
                        failFile(file);
                        mediaInfo.Close();
                        return false;
                    }
                    else
                    {
                        mediaInfo.Close();
                        return true;
                    }
                //}
                if (aspectRatio == 1778 || aspectRatio == 1333 || aspectRatio == 1500 || aspectRatio == 1855 ||
                    aspectRatio == 1818 || aspectRatio == 2000)
                {
                    mediaInfo.Close();
                    return true;
                }

                file.QCReport = "has an incorrect aspect ratio (" + aspectRatio + ")";
                failFile(file);
                mediaInfo.Close();
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks to see if a file has one video track
        /// </summary>
        /// <param name="file">file to check</param>
        /// <returns>result of the check</returns>
        private static bool checkVideoTrackCount(FileObjects file)
        {
            // Using our xml!


            if (file.CurrentFileInfo.Extension.Contains(".ac3") || 
                file.CurrentFileInfo.Extension.Contains(".aac") || 
                file.CurrentFileInfo.Extension.Contains(".mp3"))
            {
                return true;
            }
            int videoCount;
            string report;
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);


            string sVideoCount = mediaInfo.Get(StreamKind.Video, 0, "StreamCount");

            if (sVideoCount == "")
            {
                videoCount = 0;

            }
            else
            {
                videoCount = Int32.Parse(mediaInfo.Get(StreamKind.Video, 0, "StreamCount"));
            }

            //if (targetSpecs.videoTracks != -1)
            //{
            if (videoCount == file.VideoTrackCount)
            {
                mediaInfo.Close();
                return true;
            }
            else
            {
                    report = "has an incorrect number of video tracks (" + videoCount + ")";
                    failFile(file);
                mediaInfo.Close();
                return false;
            }
            //}

            //TODO: find out if this is really unreachable code
            //if (videoCount == 1 || (videoCount == 0 && file.CurrentFileInfo.Extension.Contains(".mp3")))
            //{
            //    mediaInfo.Close();
            //    return true;
            //}

            //report = "has an incorrect number of video tracks (" + videoCount + ")";
            //failFile(file);
            //mediaInfo.Close();
            //return false;
        }

        /// <summary>
        /// Checks a file to see if it has one audio track
        /// </summary>
        /// <param name="file">the file to check</param>
        /// <returns>the result of the check</returns>
        private static bool checkAudioTrackCount(FileObjects file)
        {
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            int audioCount;
            string sAudioCount = mediaInfo.Get(StreamKind.Audio, 0, "StreamCount");

            if (file.CurrentFileInfo.Extension == ".mpg" || 
                file.CurrentFileInfo.Extension == ".mxf" ||
                file.CurrentFileInfo.Extension == ".m2v")
            {
                return true;
            }
            //MSW
            if (file.CurrentFileInfo.Name.ToUpper().Contains("MUSIC&THESPOKENWORD"))
            {
                return true;
            }
            if (sAudioCount == "")
            {
                audioCount = 0;
            }
            else
            {
                audioCount = Int32.Parse(sAudioCount);
            }

            //if (targetSpecs.audioTracks != -1)
            //{
            if (file.AudioTrackCount == audioCount)
            {
                mediaInfo.Close();
                return true;
            }
            else
            {
                file.QCReport = "has an incorrect audio track count (" + audioCount + ")";
                failFile(file);
                mediaInfo.Close();
                return false;
            }
            //}

            //if (audioCount == 1 || (audioCount == 0 && file.CurrentFileInfo.Name.Contains("-ase")))
            //{
            //    mediaInfo.Close();
            //    return true;
            //}

            //file.QCReport = "has an incorrect audio track count (" + audioCount + ")";
            //failFile(file);
            //mediaInfo.Close();
            //return false;
        }

        /// <summary>
        /// Checks a file to make sure it's sample rate is acceptable
        /// </summary>
        /// <param name="file">the file to check</param>
        /// <returns>the result of the check</returns>
        private static bool checkAudioSampleRate(FileObjects file)
        {
            MediaInfo mediaInfo = new MediaInfo();
            int audioSampleRate;
            mediaInfo.Open(file.CurrentFileInfo.FullName);
            try
            {
                 audioSampleRate = Int32.Parse(mediaInfo.Get(StreamKind.Audio, 0, "SamplingRate"));
            }
            catch(Exception e)
            {
                return true;
            }

            //if (targetSpecs.AudioSampleRate != -1)
            //{
            if (file.AudioSampleRate == audioSampleRate)
            {
                mediaInfo.Close();
                return true;
            }
            else
            {
                file.QCReport = "has an incorrect audio samples rate (" + audioSampleRate + ")";     
                failFile(file);
                mediaInfo.Close();
                return false;
            }
            //}
            //if (audioSampleRate == 44100 || audioSampleRate == 48000 || audioSampleRate == 22050)
            //{
            //    mediaInfo.Close();
            //    return true;
            //}

            //string report = "has an incorrect audio samples rate (" + audioSampleRate + ")";
            //failFile(file, report, targetSpecs);
            //mediaInfo.Close();
            //return false;
        }

    }
}
