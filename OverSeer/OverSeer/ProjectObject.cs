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

        //constructor
        public ProjectObject(FileInfo xml)
        {
            Keywords = new List<string>();
            Emails = new List<MailAddress>();

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
                    Watchfolder = new DirectoryInfo(element.Value);
                    continue;
                }
                if (element.Name == "MezzaninePassFolder")
                {
                    MezzaninePassFolder = new DirectoryInfo(element.Value);
                    continue;
                } 
                if (element.Name == "WebPassFolder")
                {
                    WebPassFolder = new DirectoryInfo(element.Value);
                    continue;
                }
                if (element.Name == "FailFolder")
                {
                    FailFolder = new DirectoryInfo(element.Value);
                    continue;
                }
                if (element.Name == "Keyword")
                {
                    if(element.Value.Contains(','))
                    {
                        //parse the value
                    }
                    else
                    {
                        //traverse the node
                    }
                }
            }
            return false;
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
    }
}
