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
    public enum QCStatus {Passed,Failed,Cautioned,Queued,Holding,WrongProject};
    public enum QCKeywords {corruptFile, noAudio};
    public enum FileStatus {compressionQueue, compressed, 
                            muxQueue, muxed, 
                            qcQueue, autoQc, qced, 
                            brightcoveOUT, videoExported, 
                            online };

    public class FileObjects
    {
        #region Properties
        //this files accompanying metadata
        public XDocument XML { get; set; }
        public string RevisionInfo { get; set; }
        public int Bitrate { get; set; }
        public int AspectRatio { get; set; }
        public int FrameRate { get; set; }
        public string ScanType { get; set; }
        public string Resolution { get; set; }
        public int VideoTrackCount { get; set; }
        public int AudioTrackCount { get; set; }
        public int AudioSampleRate { get; set; }
        public string Language { get; set; }
        public FileStatus CurrentFileStatus { get; set; }
        public ProjectObject Project { get; set; }
        public List<MailAddress> EmailRecipients { get; set; } 
        //QC only metadata (won't be in incoming xml)
        public string QCReport { get; set; }
        public QCStatus CurrentQCStatus { get; set; }
        public string QCPerson { get; set; }
        public DateTime QCTime { get; set; }
        public DateTime QCQueueTime { get; set; }
        public FileInfo CurrentFileInfo { get; set; }
        #endregion

        //private vars
        List<string> audioFileExtensions = new List<string>();
        List<string> videoFileExtensions = new List<string>();

        //Constructor for files who have no existing xml data to consume
        public FileObjects(FileInfo filename)
        {
            //initialize vars
            intilizeFileExtensionInfo();

            //set properties
            CurrentFileInfo = filename;
            setDefaultProperties();
            setPropertiesFromFilename();
        }

        public FileObjects(FileInfo filename, FileInfo xml)
        {
            //initialize vars
            intilizeFileExtensionInfo();

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
        public void UpdateFileObjectFromXML(XDocument xml)
        {
            //parse xml for any revision information
            //update revision info
            //update any other information that can be gleaned from xml
            foreach (var element in xml.Descendants())
            {
                //assign default values first
                setDefaultProperties();

                //check for audio only files
                foreach (var extension in audioFileExtensions)
                {
                    if (this.CurrentFileInfo.Extension == extension)
                    {
                        this.VideoTrackCount = 0;
                    }
                }

                //TODO: check for video only files
                foreach (var extension in videoFileExtensions)
                {
                    if (this.CurrentFileInfo.Extension == extension)
                    {
                        this.AudioTrackCount = 0;
                    }
                }

                //check to see if element values are null
                if (element.Value == "")
                {
                    continue;
                }

                //assign fileObject properties according to corresponding xml element values (changes default values)
                switch (element.ToString())
                {
                    case "BitRate":
                        int tempBitrate;
                        int.TryParse(element.Value, out tempBitrate);
                        this.Bitrate = tempBitrate;
                        break;
                    case "FrameRate":
                        int tempFrameRate;
                        int.TryParse(element.Value, out tempFrameRate);
                        this.FrameRate = 1000 * tempFrameRate;
                        break;
                    case "ScanType":
                        this.ScanType = element.Value; 
                        break;
                    case "Resolution":
                        this.Resolution = element.Value;
                        break;
                    case "AspectRatio":
                        int tempAspectRatio;
                        int.TryParse(element.Value, out tempAspectRatio);
                        this.AspectRatio = tempAspectRatio;
                        break;
                    case "Language":
                        this.Language = element.Value;
                        break;
                    case "VideoTrackCount":
                        int tempVideoTrackCount;
                        int.TryParse(element.Value, out tempVideoTrackCount);
                        this.VideoTrackCount = tempVideoTrackCount;
                        break;
                    case "AudioTrackCount":
                        int tempAudioTrackCount;
                        int.TryParse(element.Value, out tempAudioTrackCount);
                        this.AudioTrackCount = tempAudioTrackCount;
                        break;
                }

                
            }
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

        /// <summary>
        /// Updates the 
        /// </summary>
        /// <param name="file"></param>
        public void UpdateCurrentLocation(FileInfo file)
        {
            this.CurrentFileInfo = new FileInfo(file.FullName);
        }

        /// <summary>
        /// sets up the lists of audio and video types according to file extension
        /// </summary>
        private void intilizeFileExtensionInfo()
        {
            //audio only
            audioFileExtensions.Add(".mp3");
            audioFileExtensions.Add(".ac3");
            audioFileExtensions.Add(".wma");

            //video only
            videoFileExtensions.Add(".mk2");
            videoFileExtensions.Add(".mpg");
        }

        /// <summary>
        /// sets the default proerty values for this class
        /// </summary>
        private void setDefaultProperties()
        {
            //set default values
            this.RevisionInfo = "first revision";
            this.Bitrate = 8000;
            this.FrameRate = 2997;
            this.ScanType = "Progressive";
            this.Resolution = "1920x1080";
            this.AspectRatio = 1778;
            this.Language = "eng";
            this.VideoTrackCount = 1;
            this.AudioTrackCount = 1;
            this.AudioSampleRate = 48000;
            this.CurrentFileStatus = FileStatus.qcQueue;
            this.Project = new ProjectObject(new FileInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects\autoCompressed.xml"));
        }

        /// <summary>
        /// parses the fillname to try a glean out metadata such as bitrate and langauge code
        /// </summary>
        private void setPropertiesFromFilename()
        {
            //TODO: test for diva name parsing for informaiton, not just web-deliverable

            //get a list of currently used bitrates
            List<string> bitrates = new List<string>();
            bitrates.Add("8000k");
            bitrates.Add("300k");
            bitrates.Add("1800k");
            bitrates.Add("2500k");
            bitrates.Add("1000k");
            bitrates.Add("1080p");
            bitrates.Add("720p");

            //parse filename for extra information
            //get bitrate
            if (this.CurrentFileInfo.Name.Contains("0k") || this.CurrentFileInfo.Name.Contains("0p"))
            {
                foreach (var bitrate in bitrates)
                {
                    if (this.CurrentFileInfo.Name.Contains(bitrate))
                    {
                        int tempBitrate;
                        int.TryParse(bitrate.Replace("k",""), out tempBitrate);
                        this.Bitrate = tempBitrate;
                    }
                }
            }

            //get language
            string possibleLanguageCode;
            string languageCode;
            possibleLanguageCode = this.CurrentFileInfo.Name.Remove(0, this.CurrentFileInfo.Name.Length - 7);
            if (possibleLanguageCode.Contains('-'))
            {
                //check to see if it is the first character
                if (possibleLanguageCode[0] == '-')
                {
                    //stip off the extensions
                    languageCode = possibleLanguageCode.Remove(possibleLanguageCode.Length - 4);
                    //strip off the '-'
                    languageCode = languageCode.Remove(0, 1);
                    //set language code
                    this.Language = languageCode;
                }
            }
        }
    }
}
