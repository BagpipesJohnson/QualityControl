using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.Xml.Linq;
using System.IO;
using System.Net.Mail; //used for emails

namespace OverSeer
{
    public class ProjectObject
    {
        //properties
        public XDocument XML { get; set; }
        public string ProjectName { get; set; }
        public string SDNumber { get; set; }
        public DirectoryInfo Watchfolder { get; set; }
        public DirectoryInfo MezzaninePassFolder { get; set; }
        public DirectoryInfo WebPassFolder { get; set; }
        public DirectoryInfo FailFolder { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> KeywordExclusions { get; set; }
        public List<MailAddress> Emails { get; set; }
        public List<FileObjects> currentFileObjects { get; set; }

        //constructor
        public ProjectObject(FileInfo xml)
        {
            Keywords = new List<string>();
            Emails = new List<MailAddress>();
            currentFileObjects = new List<FileObjects>();

            XML = XDocument.Load(xml.FullName);

            //serialize the project xml
            UpdateProjectObjectFromXML(XML);
            
        }

        /// <summary>
        /// Parses and xml to add any existing values to the ProjectObject
        /// </summary>
        /// <param name="xml">the xml to parse</param>
        /// <returns>true if successful, false if an exception occurs</returns>
        public bool UpdateProjectObjectFromXML(XDocument xml)
        {
            //update any other information that can be gleaned from xml
            foreach (var element in xml.Element("Project").Elements())
            {
                if (element.Name == "Name")
                {
                    ProjectName = element.Value;
                    continue;
                }
                if (element.Name == "SDNumber")
                {
                    SDNumber = element.Value;
                    continue;
                }
                if (element.Name == "Watchfolder")
                {
                    if(element.Value=="")
                    { 
                        continue; 
                    }
                    else
                    {
                        Watchfolder = new DirectoryInfo(element.Value);
                        continue;
                    }
                }
                if (element.Name == "MezzaninePassFolder")
                {
                    if (element.Value == "")
                    {
                    }
                    else
                    {
                        MezzaninePassFolder = new DirectoryInfo(element.Value);
                        continue;
                    }
                } 
                if (element.Name == "WebPassFolder")
                {
                    if (element.Value == "")
                    {
                    }
                    else
                    {
                        WebPassFolder = new DirectoryInfo(element.Value);
                        continue;
                    }
                }
                if (element.Name == "FailFolder")
                {
                    if (element.Value == "")
                    {
                    }
                    else
                    {
                        FailFolder = new DirectoryInfo(element.Value);
                        continue;
                    }
                }
                if (element.Name == "Keyword")
                {
                    if(element.Value != "")
                    {
                        //parse the value
                        Keywords = element.Value.Split(',').ToList<string>();
                    }
                    else
                    {
                        //traverse the node
                        foreach (var keyword in element.Elements())
                        {
                            Keywords.Add(keyword.Value);
                        }
                    }
                    continue;
                }
                if (element.Name == "ToEmailOnFail")
                {
                    if (element.Value != "")
                    {
                        //parse the value
                        Keywords = element.Value.Split(',').ToList<string>();
                    }
                    else
                    {
                        //traverse the node
                        foreach (var email in element.Elements())
                        {
                            Emails.Add(new MailAddress(email.Value));
                        }
                    }
                    continue;
                }
                if (element.Name == "KeywordsExclusions")
                {
                    foreach (var keyword in element.Elements())
                    {
                        KeywordExclusions.Add(keyword.Value);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Updates and xml with current ProjectObject values
        /// </summary>
        /// <param name="xml">the xml document to update</param>
        /// <returns>true if successful, false if an exception occurs</returns>
        public bool UpdateXMLFromProjectObject(XDocument xml)
        {
            //parse xml for any revision information
            //update revision info
            //update xml with changed ProjectObject information
            return false;
        }

        public void UpdateCurrentFileObjects()
        {
            //search through all filesObjects
            foreach (var file in MainWindow.CurrentFileObjects)
            {
                //find all fileObjects for this project by keyword
                foreach (var keyword in this.Keywords)
                {
                    if (file.CurrentFileInfo.Name.Contains(keyword))
                    {
                        //exclude bad hits by keyword exlusions
                        foreach (var badKeyword in this.KeywordExclusions)
                        {
                            if (file.CurrentFileInfo.Name.Contains(badKeyword))
                            {
                                continue;
                            }
                            else
                            {
                                this.currentFileObjects.Add(file);
                                file.Project = this;
                            }
                        }
                    }
                }
            }
        }
    }
}
