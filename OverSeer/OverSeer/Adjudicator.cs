using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//added
using System.Xml.Linq;

namespace OverSeer
{
    public class Adjudicator
    {
        //TODO: why are these not properties?
        public DirectoryInfo projectDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\");
        public DirectoryInfo WebPassFolder;
        public DirectoryInfo MezzaninePassFolder;
        public DirectoryInfo FailFolder;
        public DirectoryInfo watchFolder = new DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCPassed\");
        public DirectoryInfo wrongProjectDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCPassed\WrongProjectFiles\");
        public List<string> Keywords;
        public string currentSDNumber;
        public string userName = "";
        System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\");
        public Googler googleBot = new Googler();
        
        private string projectName;

        public Adjudicator()
        {
            // Parse XMLs to get the spreadSheetList
            
            //get a new list for the Keywords property
            Keywords = new List<string>();

            return;
        }

        public string generateTXTLogReport(FileObjects file, string user)
        {
            string txtReport;

            txtReport = file.CurrentFileInfo.Name + "\r\n" + new string('-',60) + "\r\n";
            switch (file.CurrentQCStatus)
            {
                case QCStatus.WrongProject:
                    txtReport += "Moved to the wrong project folder on " + DateTime.Now;
                    break;
                case QCStatus.Cautioned:
                    txtReport += "Caution because " + file.QCReport + " on " + DateTime.Now;
                    break;
                case QCStatus.Failed:
                    txtReport += "Failed because " + file.QCReport + " on " + DateTime.Now;
                    break;
                case QCStatus.Passed:
                    txtReport += "Passed on " + DateTime.Now;
                    break;
                default:
                    System.Windows.MessageBox.Show("The QC status of this file is unknown: \r\n" + file.CurrentFileInfo.FullName);
                    break;
            }
            txtReport += "\r\n" + "QC'ed by " + user;

            txtReport += "\r\n".PadLeft(2); //add the string as many times as need by incrementing the Padleft

            return txtReport;
           // if (file.CurrentQCStatus == QCStatus.WrongProject)
           // {
           //     return "\r\n------------------------------------------------------------\r\n" + file.Name + " Was moved to the wrong project folder  on \n\r" +
           //           DateTime.Now.ToString() + "\r\n" +
           //           "\r\n" +
           //           "\r\n"; 
           // }
           // string finishedreport = file. +
           //           "\r\n------------------------------------------------------------\r\n" +
           //           result + " on \n\r" +
           //           DateTime.Now.ToString();
           //if(result != "Passed")
           //{
           //    finishedreport += " because: " +
           //              report;
           //}
           //           finishedreport += "\r\n" + user +
           //           "\r\n" +
           //           "\r\n";

           //return finishedreport;
        }

        public string generateTabReport(FileObjects file, string user)
        {
            string tabReport;

            tabReport = file.CurrentFileInfo.Name + "\t";
            tabReport += user + "\t";
            tabReport += DateTime.Now + "\t";
            tabReport += file.CurrentQCStatus + "\t";
            tabReport += file.QCReport + "\t";
            tabReport += "\n"; 

            return tabReport;
            //if (result == "WrongProject")
            //{
            //    return file.Name + "\t" +
            //           DateTime.Now.ToString() + "\t" +
            //           "Was moved to the wrong project folder" +"\t" +
            //           user + "\r\n";
                    
            //}
            //string finishedreport = file.Name + "\t" + 
            //                        DateTime.Now.ToString() + "\t" +
            //                        result + "\t" +
            //                        user + "\t" + report + "\r\n";

            //return finishedreport;
        }

        //TODO: figure out what the report parameter report does.  Johnson?
        public bool ProcessFile(FileObjects file, string currentUser, string currentUserInitials, ProjectObject currentProject)
        {
            // Log it

            StatusReporter reporter = new StatusReporter();
            string fullReport = this.generateTXTLogReport(file, currentUser);
            string fullTabReport = this.generateTabReport(file, currentUser);
            //user logs
            logger.saveToTXT("AdjudicatorLogs_" + currentUser, logsDirectory, fullReport);
            logger.saveToTabDilimited("AdjudicatorLogs_" + currentUser, logsDirectory, fullTabReport);
            //project log
            logger.saveToTXT(currentProject.ProjectName, logsDirectory, fullReport);
            logger.saveToTabDilimited(currentProject.ProjectName, logsDirectory, fullTabReport);
            string moveStatus;
            switch (file.CurrentQCStatus)
            {
                case QCStatus.Passed:
                    {
                        // move it
                        // is it a mezzanine?
                        bool mezzanine = utility.isMezzanine(file);
                        if (mezzanine)
                        {
                            moveStatus = utility.moveFile(file, this.MezzaninePassFolder);
                            //utility.moveFile(file, this.MezzaninePassFolder);

                        }
                        else
                        {
                            moveStatus = utility.moveFile(file, this.WebPassFolder);
                            //utility.moveFile(file, this.WebPassFolder);
                        }

                        reporter.addResult(2, file);
                        googleBot.logItConference(file.CurrentFileInfo.Name, file.QCReport + currentUserInitials + DateTime.Now);
                        break;
                    }
                case QCStatus.Failed:
                    {
                        // move it
                        //moveStatus = utility.moveFile(file, FailFolder);
                        this.emailFail(file); 
                        utility.moveFile(file, FailFolder);
                        googleBot.logItConference(file.CurrentFileInfo.Name, "Failed: " + file.QCReport + currentUserInitials + DateTime.Now);
                        reporter.addFail(file);
                        reporter.addResult(4, file);

                       

                        break;
                    }
                case QCStatus.Cautioned:
                    {
                        // move it
                        // is it a mezzanine?
                        bool mezzanine = utility.isMezzanine(file);
                        if (mezzanine)
                        {
                            //moveStatus = utility.moveFile(file, this.MezzaninePassFolder);
                            utility.moveFile(file, this.MezzaninePassFolder);
                        }
                        else
                        {
                            //moveStatus = utility.moveFile(file, this.WebPassFolder);
                            utility.moveFile(file, this.WebPassFolder);
                        }

                        googleBot.logItConference(file.CurrentFileInfo.Name, "Cautioned: " + file.QCReport + currentUserInitials + DateTime.Now);

                        // email on a caution
                        emailCaution(file);
                        reporter.addResult(3, file);
                        break;
                    }
                case QCStatus.WrongProject:
                    {
                        // move it to the brig
                       //moveStatus = utility.moveFile(file, wrongProjectDirectory);
                       utility.moveFile(file, wrongProjectDirectory);
                       try
                       {
                           reporter.tryToRemoveFromQueue(file.CurrentFileInfo.Name, reporter.getWorksheetByProjectName(currentProject));
                       }
                       catch (Exception e)
                       {

                       }
                        break;
                    }
                default:
                     return false;

            }

            return true;
        }

        public void emailCaution(FileObjects file)
        {
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();

            //TODO:email interested parties
            foreach (var personToEmail in file.Project.Emails)
            {
                email.To.Add(personToEmail);
            }

            //email.To.Add("psd-qc@ldschurch.org");
            // Add Compression
            //email.To.Add("MSD-Compression@ldschurch.org");

            // Add another contact if there is a person to email on fail-----
            //FileInfo xml = utility.getXMLForAsset(file);

            //string recipient = null;
            //if (xml != null)
            //{
            //    recipient = utility.getValueFromXML(utility.getProjectFile(xml), "ToEmailOnFail");
            //}

            //if (recipient != "NA" && recipient != null)
            //{
            //    email.To.Add(recipient);
            //}


            email.Subject = file.CurrentFileInfo.Name + " was Cautioned by the QC team";

            email.From = new System.Net.Mail.MailAddress("psd-qc@ldschurch.org");
            email.Body = "Hello,\r" + 
                         "\n" + 
                         "The following file has been cautioned and may or may not meet Quality Control standards:\r" + 
                         "\n" +
                         "\n" + 
                         file.CurrentFileInfo.Name + "\r" + 
                         "\n" + 
                         " The reason was: " + file.QCReport + ". This asset was cautioned by: " + this.userName + "\r" +
                         "\n" +
                         "Thank You," + "\r" +
                         "\n" + 
                         "PSD-QC";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.ldschurch.org");
            smtp.Send(email);
        }


        public void emailFail(FileObjects file)
        {
            
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();

            //email.To.Add("psd-qc@ldschurch.org");
            // Add Compression
            email.To.Add("MSD-Compression@ldschurch.org");
            email.To.Add("carlilelance@ldschurch.org");

            //TODO: get other email addresses from ProjectObjects and email to who needs it.
            //      This may require getting a Project as one of the arguments
            //// Add another contact if there is a person to email on fail-----
            //FileInfo xml = utility.getXMLForAsset(file);

            //string recipient = null;
            //if (xml != null)
            //{
            //    recipient = utility.getValueFromXML(utility.getProjectFile(xml), "ToEmailOnFail");
            //}

            //if (recipient != "NA" && recipient != null)
            //{
            //    email.To.Add(recipient);
            //}

            
            email.Subject = file.CurrentFileInfo.Name + " Has failed to pass QC Standards: " + file.QCReport;

            email.From = new System.Net.Mail.MailAddress("psd-qc@ldschurch.org");
            email.Body = "Hello," + "\r" + 
                         "\n" + 
                         "The following file has failed Quality Control standards:" + "\r" +
                         "\n" + 
                         file.CurrentFileInfo.Name + "\r" + 
                         "\n" + 
                         " The reason was: " + file.QCReport + "." + 
                         "This asset was failed by: " + this.userName + " on " + DateTime.Now + "." + "\r" + 
                         "\n" + 
                         "Thank You," + "\r" +
                         "\n" + 
                         "PSD QC";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.ldschurch.org");
            smtp.Send(email);
        }

        public FileObjects getFileToQC(ProjectObject project, DirectoryInfo userDirectory)
        {
            // Which project are we in?

            //TODO: sort files that are "gotten" by priority or assignment
            // Get the first one out of the project
            FileObjects file = getFileFromCurrentProject(project);

            if (file == null)
            {
                return null;
            }
            // Move it into the USER'S folder to watch

            utility.moveFile(file, userDirectory);
            // Return the finished path to that file
            //FileObjects readyFile = new FileInfo(userDirectory + file.CurrentFileInfo.Name);  //TODO: why not just file.FullName?
            return file;
        }

        public FileObjects getFileFromCurrentProject(ProjectObject currentProject)
        {
            // Are we doing projects from directories?
            // Or from XML's?

            // get list of files in the passed folder
            //string[] Keywords = null;
            List<FileInfo> fileList = utility.checkForSystemFiles(this.watchFolder.GetFiles().ToList<FileInfo>());
            FileObjects firstFile;

            //look through all the files waiting to be QC'ed
            foreach (var file in fileList)
            {
                //check for keywords to pair it with the correct project
                foreach (var keyword in currentProject.Keywords)
                {
                    if (file.Name.Contains(keyword))
                    {
                        //TODO: exclude bad keyworded files (excluded keywords)
                        foreach (var badKeyword in currentProject.KeywordExclusions)
                        {
                            if (file.Name.Contains(badKeyword))
                            {
                            }
                        }

                        //get fileObject for first file found that is part of the project and return it
                        foreach (var fileObject in MainWindow.CurrentFileObjects)
                        {
                            if (fileObject.CurrentFileInfo.Name == file.Name)
                            {
                                return fileObject;
                            }
                        }

                    }
                }
            }

            return null; //file couldn't be found
            //foreach (FileInfo tempProject in utility.checkForSystemFiles(this.projectDirectory.GetFiles().ToList<FileInfo>()))
            //{
            //    string tempName = utility.getValueFromXML(tempProject, "Name");
            //    if (tempName == currentProject)
            //    {
            //         Keywords = utility.getValueFromXML(tempProject, "Keyword").Split(',');
            //         break;
            //    }
            //}
           
            //// return first file in list
            //if (Keywords != null)
            //{
            //    foreach (FileInfo file in fileList)
            //    {
            //        // Does it have an xml?
            //        System.IO.DirectoryInfo xmlDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\");

            //        foreach (string keyword in Keywords)
            //        {
            //            if (file.Name.Contains(keyword))
            //            {
            //                return file;
            //            }
            //        }
            //    }
            //}
            //return null;
        }

        public string ClearUserFolder(DirectoryInfo userFolder)
        {
            bool hadProblems = false;
            if (userFolder == null)
            {
                return "";
            }
            foreach (FileInfo file in utility.checkForSystemFiles(userFolder.GetFiles().ToList<FileInfo>()))
            {
                string tempResult = utility.moveFile(file, this.watchFolder);
                if (tempResult == "File is not free to move")
                {
                    return "Failed to move " + file.Name + " Because it is in use";
                }
            }
            return "";
        }

        public void changeProject(ProjectObject project)
        {
            // get the xml and populate all values
            this.watchFolder = project.Watchfolder;
            this.MezzaninePassFolder = project.MezzaninePassFolder;
            this.WebPassFolder = project.WebPassFolder;
            this.FailFolder = project.FailFolder;
            this.currentSDNumber = project.SDNumber;
            this.Keywords = project.Keywords;

            //foreach (FileInfo project in utility.checkForSystemFiles(projectDirectory.GetFiles().ToList<FileInfo>()))
            //{
            //    // is this the right project?
            //    string tempProject = utility.getValueFromXML(project, "Name");
            //    if (tempProject == projectName)
            //    {
            //        // get the watch folder
            //        this.watchFolder = new DirectoryInfo(utility.getValueFromXML(project, "Watchfolder"));
            //        // get the pass folder
            //        this.MezzaninePassFolder = new DirectoryInfo(utility.getValueFromXML(project, "MezzaninePassFolder"));
            //        this.WebPassFolder = new DirectoryInfo(utility.getValueFromXML(project, "WebPassFolder"));
            //        // get the fail folder
            //        this.FailFolder = new DirectoryInfo(utility.getValueFromXML(project, "FailFolder"));
            //        // get the SD number
            //        this.currentSDNumber = utility.getValueFromXML(project, "SDNumber");
            //        // get the Keywords
            //        this.Keywords = utility.getValueFromXML(project, "Keyword");
            //        return;
            //    }
               
            //}


        }

        public void refreshAdjudicator()
        {
        }

    }
}
