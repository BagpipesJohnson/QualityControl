using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.Xml.Linq;
using System.Net.Mail; //used for emails
using System.IO;

namespace OverSeer
{
    public enum QCStatus {Passed,Failed,Cautioned,Queued,Holding};
    public enum QCKeywords {corruptFile, noAudio};
    public enum FileStatus {compressionQueue, compressed, 
                            muxQueue, muxed, 
                            qcQueue, qced, 
                            brightcoveOUT, videoExported, 
                            online };

    public class FileObjects
    {
        //this files accompanying metadata xml
        public XDocument XML { get; set; }
        //this files accompanying metadata
        public int Bitrate { get; set; }
        public Dictionary<int, string> Revisions { get; set; }
        public int AspectRatio { get; set; }
        public QCStatus CurrentQCStatus { get; set; }
        public int Resolution { get; set; }
        public string QCReport { get; set; }
        public string QCPerson { get; set; }
        public DateTime QCTime { get; set; }
        public DateTime QCQueueTime { get; set; }
        public QCKeywords CurrentQCKeywords { get; set; }
        public List<MailAddress> EmailRecipients { get; set; }
        public FileInfo CurrentFileInfo { get; set; }
        public FileStatus CurrentFileStatus { get; set; }
        public string Project { get; set; }

        public FileObjects(FileInfo filename, FileInfo xml)
        {
            //set properties
            CurrentFileInfo = filename;
            XML = XDocument.Load(xml.FullName);

            //add xml info
            UpdateFileObjectFromXML(XML); //TODO: test to see if this returns true or tell the user if it doesn't
        }

        
        /// <summary>
        /// Parses and xml to add any existing values to the FileObject
        /// </summary>
        /// <param name="xml">the xml to parse</param>
        /// <returns>true if successful, false if an exception occurs</returns>
        public bool UpdateFileObjectFromXML(XDocument xml)
        {
            //parse xml for any revision information
            //update revision info
            //update any other information that can be gleaned from xml
            return false;
        }

        /// <summary>
        /// Updates and xml with current FileObject values
        /// </summary>
        /// <param name="xml">the xml document to update</param>
        /// <returns>true if successful, false if an exception occurs</returns>
        public bool UpdateXMLFromFileObject(XDocument xml)
        {
            //parse xml for any revision information
            //update revision info
            //update xml with changed FileObject information
            return false;
        }

    }
}
