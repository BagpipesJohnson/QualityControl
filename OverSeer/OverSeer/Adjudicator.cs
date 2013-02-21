using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace OverSeer
{
    public class Adjudicator
    {
        public DirectoryInfo projectDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\");
        public DirectoryInfo WebPassedDirectory;
        public DirectoryInfo MezzaninePassedDirectory;
        public DirectoryInfo failedDirectory;
        public DirectoryInfo watchFolder = new DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCPassed\");
        public DirectoryInfo wrongProjectDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCPassed\WrongProjectFiles\");
        public string keywords;
        public string currentSDNumber;
        public string userName = "";
        System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\logs\OverseerLogs\");
        public Googler googleBot = new Googler();
        
        private string projectName;

        public Adjudicator()
        {
            // Parse XMLs to get the spreadSheetList
            //
            
            return;
        }

        public string generateReport(FileInfo file, string report, string result, string user)
        {
            if (result == "WrongProject")
            {
                return "\r\n------------------------------------------------------------\r\n" + file.Name + " Was moved to the wrong project folder  on \n\r" +
                      DateTime.Now.ToString() + "\r\n" +
                      "\r\n" +
                      "\r\n"; 
            }
            string finishedreport = file.Name +
                      "\r\n------------------------------------------------------------\r\n" +
                      result + " on \n\r" +
                      DateTime.Now.ToString();
           if(result != "Passed")
           {
               finishedreport += " because: " +
                         report;
           }
                      finishedreport += "\r\n" + user +
                      "\r\n" +
                      "\r\n";

            return finishedreport;
        }

        public string generateTabReport(FileInfo file, string report, string result, string user)
        {
            if (result == "WrongProject")
            {
                return file.Name + "\t" +
                       DateTime.Now.ToString() + "\t" +
                       "Was moved to the wrong project folder" +"\t" +
                       user + "\r\n";
                    
            }
            string finishedreport = file.Name + "\t" + 
                                    DateTime.Now.ToString() + "\t" +
                                    result + "\t" +
                                    user + "\t" + report + "\r\n";

            return finishedreport;
        }

        //TODO: figure out what the report parameter report does.  Johnson?
        public bool ProcessFile(FileInfo file, string result, string report, string currentUser, string currentUserInitials, string currentProject)
        {
            // Log it

            StatusReporter reporter = new StatusReporter();
            string fullReport = this.generateReport(file, report, result, currentUser);
            string fullTabReport = this.generateTabReport(file, report, result, currentUser);
            //user logs
            logger.saveToTXT("AdjudicatorLogs" + currentUser, logsDirectory, fullReport);
            logger.saveToTabDilimited("AdjudicatorLogs" + currentUser, logsDirectory, fullTabReport);
            //project log
            logger.saveToTXT(currentProject, logsDirectory, fullReport);
            logger.saveToTabDilimited(currentProject, logsDirectory, fullTabReport);
            string moveStatus;
            switch (result)
            {
                case("Passed"):
                    {
                        // move it
                        // is it a mezzanine?
                        bool mezzanine = utility.isMezzanine(file);
                        if (mezzanine)
                        {
                            moveStatus = utility.moveFile(file, this.MezzaninePassedDirectory);
                            //utility.moveFile(file, this.MezzaninePassedDirectory);

                        }
                        else
                        {
                            moveStatus = utility.moveFile(file, this.WebPassedDirectory);
                            //utility.moveFile(file, this.WebPassedDirectory);
                        }

                        reporter.addResult(2, file.Name, currentProject, "");
                        googleBot.logItConference(file.Name, report + currentUserInitials + DateTime.Now);
                        break;
                    }
                case ("Failed"):
                    {
                        // move it
                        //moveStatus = utility.moveFile(file, failedDirectory);
                        this.emailFail(file, report);
                        utility.moveFile(file, failedDirectory);
                        googleBot.logItConference(file.Name, "Failed: " + report + currentUserInitials + DateTime.Now);
                        reporter.addFail(file.Name, report);
                        reporter.addResult(4, file.Name, currentProject, report);

                       

                        break;
                    }
                case("Cautioned"):
                    {
                        // move it
                        // is it a mezzanine?
                        bool mezzanine = utility.isMezzanine(file);
                        if (mezzanine)
                        {
                            //moveStatus = utility.moveFile(file, this.MezzaninePassedDirectory);
                            utility.moveFile(file, this.MezzaninePassedDirectory);
                        }
                        else
                        {
                            //moveStatus = utility.moveFile(file, this.WebPassedDirectory);
                            utility.moveFile(file, this.WebPassedDirectory);
                        }

                        googleBot.logItConference(file.Name, "Cautioned: " + report + currentUserInitials + DateTime.Now);

                        // email on a caution
                        emailCaution(file, report);
                        reporter.addResult(3, file.Name, currentProject, report);
                        break;
                    }
                case("WrongProject"):
                    {
                        // move it to the brig
                       //moveStatus = utility.moveFile(file, wrongProjectDirectory);
                       utility.moveFile(file, wrongProjectDirectory);
                       try
                       {
                           reporter.tryToRemoveFromQueue(file.Name, reporter.getWorksheetByProjectName(currentProject));
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

        public void emailCaution(FileInfo file, string report)
        {
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();

            email.To.Add("psd-qc@ldschurch.org");
            // Add Compression
            //email.To.Add("MSD-Compression@ldschurch.org");

            // Add another contact if there is a person to email on fail-----
            FileInfo xml = utility.getXMLForAsset(file);

            string recipient = null;
            if (xml != null)
            {
                recipient = utility.getValueFromXML(utility.getProjectFile(xml), "ToEmailOnFail");
            }

            if (recipient != "NA" && recipient != null)
            {
                email.To.Add(recipient);
            }


            email.Subject = file.Name + " was Cautioned by the QC team";

            email.From = new System.Net.Mail.MailAddress("psd-qc@ldschurch.org");
            email.Body = "Hello \n The following file has been cautioned and may or may not meet Quality Control standards: \n" + file.Name + "\n" + " The reason was: " + report + "\n This asset was cautioned by: " + this.userName + "\n Thank You \n QCTeam";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.ldschurch.org");
            smtp.Send(email);
        }


        public void emailFail(FileInfo file, string report)
        {
            
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();

            email.To.Add("psd-qc@ldschurch.org");
            // Add Compression
            email.To.Add("MSD-Compression@ldschurch.org");

            // Add another contact if there is a person to email on fail-----
            FileInfo xml = utility.getXMLForAsset(file);

            string recipient = null;
            if (xml != null)
            {
                recipient = utility.getValueFromXML(utility.getProjectFile(xml), "ToEmailOnFail");
            }

            if (recipient != "NA" && recipient != null)
            {
                email.To.Add(recipient);
            }

            
            email.Subject = file.Name + " Has failed to pass QC Standards: " + report;

            email.From = new System.Net.Mail.MailAddress("psd-qc@ldschurch.org");
            email.Body = "Hello, \n The following file has failed Quality Control standards: \n \n" + file.Name + "\n" + " The reason was: " + report + "\n This asset was failed by: " + this.userName + "\n\n Thank You, \n\n QCTeam";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.ldschurch.org");
            smtp.Send(email);
        }

        public FileInfo getFileToQC(string project, DirectoryInfo userDirectory)
        {
            // Which project are we in?

            // Get the first one out of the project
            FileInfo file = getFileFromCurrentProject(project);

            if (file == null)
            {
                return null;
            }
            // Move it into the USER'S folder to watch

            utility.moveFile(file, userDirectory);
            // Return the finished path to that file
            FileInfo readyFile = new FileInfo(userDirectory + file.Name);  //TODO: why not just file.FullName?
            return readyFile;
        }

        public FileInfo getFileFromCurrentProject(string currentProject)
        {
            // Are we doing projects from directories?
            // Or from XML's?

            // get list of files in the passed folder
            string[] keyWords = null;
            List<FileInfo> fileList = utility.checkForSystemFiles(this.watchFolder.GetFiles().ToList<FileInfo>());
            foreach (FileInfo tempProject in utility.checkForSystemFiles(this.projectDirectory.GetFiles().ToList<FileInfo>()))
            {
                string tempName = utility.getValueFromXML(tempProject, "Name");
                if (tempName == currentProject)
                {
                     keyWords = utility.getValueFromXML(tempProject, "Keyword").Split(',');
                     break;
                }
            }
           
            // return first file in list
            if (keyWords != null)
            {
                foreach (FileInfo file in fileList)
                {
                    // Does it have an xml?
                    System.IO.DirectoryInfo xmlDirectory = new System.IO.DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\OverSeerGeneratedxmls\");

                    foreach (string keyword in keyWords)
                    {
                        if (file.Name.Contains(keyword))
                        {
                            return file;
                        }
                    }
                }
            }
            return null;
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

        public void changeProject(string projectName)
        {
            // get the xml
            foreach (FileInfo project in utility.checkForSystemFiles(projectDirectory.GetFiles().ToList<FileInfo>()))
            {
                // is this the right project?
                string tempProject = utility.getValueFromXML(project, "Name");
                if (tempProject == projectName)
                {
                    // get the watch folder
                    this.watchFolder = new DirectoryInfo(utility.getValueFromXML(project, "Watchfolder"));
                    // get the pass folder
                    this.MezzaninePassedDirectory = new DirectoryInfo(utility.getValueFromXML(project, "MezzaninePassFolder"));
                    this.WebPassedDirectory = new DirectoryInfo(utility.getValueFromXML(project, "WebPassFolder"));
                    // get the fail folder
                    this.failedDirectory = new DirectoryInfo(utility.getValueFromXML(project, "FailFolder"));
                    // get the SD number
                    this.currentSDNumber = utility.getValueFromXML(project, "SDNumber");
                    // get the keywords
                    this.keywords = utility.getValueFromXML(project, "Keyword");
                    return;
                }
               
            }


        }

        public void refreshAdjudicator()
        {
        }

    }
}
