using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
namespace OverSeer
{
    class rectifier
    {
        //properties
        public uint FileCount { get; private set; }
        //vars
        

        //constructor
        public rectifier(DirectoryInfo watchFolder)
        {
            return;
        }

        //get list of files in drop folder
        public List<FileInfo>getDirectoryFiles(DirectoryInfo watchFolder)
        {
            return watchFolder.GetFiles().ToList<FileInfo>();
        }

        public void createXMLs(List<FileInfo> files, DirectoryInfo XMLDirectory)
        {
            //TODO: get rid of this try catch
            try
            {
                foreach (FileInfo file in files)
                {
                    //TODO: linq this
                    using (XmlWriter writer = XmlWriter.Create(XMLDirectory + file.Name + ".xml"))
                    {
                        // Create a new XML
                        writer.WriteStartDocument();
                        writer.WriteWhitespace("\r\n");
                        writer.WriteStartElement("Asset");
                        writer.WriteWhitespace("\r\n");

                        // Populate XML--Element names CANNOT have spaces!

                        string bitRate = this.getBitRate(file);
                        writer.WriteElementString("BitRate", bitRate);
                        writer.WriteWhitespace("\r\n");

                        string frameRate = getFrameRate(file);
                        addElementToXml(writer, "FrameRate", frameRate);

                        string ScanType = getScanType(file);
                        addElementToXml(writer, "ScanType", ScanType);

                        string Resolution = getResolution(file);
                        addElementToXml(writer, "Resolution", Resolution);

                        string AspectRatio = getAspectRatio(file);
                        addElementToXml(writer, "AspectRatio", AspectRatio);

                        string Project = getProject(file);

                        addElementToXml(writer, "Project", Project);

                        writer.WriteEndElement();
                        writer.WriteWhitespace("\r\n");
                        writer.WriteEndDocument();

                    }
                }
            }
            catch (Exception exception)
            {
                logger.writeErrorLog("Could not create XML files in rectifier " + DateTime.Now);
            }
        }

        public List<FileObjects> createFileObjects(List<FileInfo> files, DirectoryInfo xmlDirectory)
        {
            //list of FileObjects to return
            List<FileObjects> fileObjects = new List<FileObjects>();
            //variable to store each files corresponding xml temporarily
            FileInfo filesXML;

            foreach (var file in files)
            {
                //gets the files fullname, strips off the extension and adds .xml which should give the files xml
                filesXML = new FileInfo(file.FullName.Remove(file.FullName.Length,4) + ".xml");
                //check to see if file's xml's exist, if not, warn the user and continue
                if (!File.Exists(filesXML.FullName))
                {
                    System.Windows.MessageBox.Show("The following file: \n" + file + "\n" + "is missing!");
                    continue;
                }
                fileObjects.Add(new FileObjects(file, filesXML)); 
            }

            return fileObjects;
        }

        private string getProject(FileInfo file)
        {
           // Get to all of the project xmls
            DirectoryInfo xmlDirectory = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects");

            List<FileInfo> files = utility.checkForSystemFiles(xmlDirectory.GetFiles().ToList<FileInfo>());
            string keywords;
            foreach (FileInfo project in files)
            {
                keywords = "";
                string projectName = "";
                // Look inside of each one's key words
                XmlTextReader reader = new XmlTextReader(project.FullName);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (reader.Name == "Keyword")
                            {
                                reader.Read();
                                    if(reader.NodeType == XmlNodeType.Text)
                                    {
                                        keywords = reader.Value;
                                    }
                            }
                            else if(reader.Name == "Name")
                            {
                                 reader.Read();
                                    if(reader.NodeType == XmlNodeType.Text)
                                    {
                                        projectName = reader.Value;
                                    }
                            }
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            break;
                    }
                    if (keywords != "" && projectName != "")
                    {
                        break;
                    }
                }

                // Now we have the key words for this project. They are split by ','

                // Does our file fit any of them?

                string[] splitkeywords = keywords.Split(',');

                foreach (string word in splitkeywords)
                {
                    if (file.Name.Contains(word))
                    {
                        return projectName;
                    }
                }
            }
           
            return "NA";
        }

        private string getResolution(FileInfo file)
        {
            string filename = file.Name;

            if (filename.Contains("1920x1080"))
            {
                return "1920x1080";
            }
            else if (filename.Contains("720x486"))
            {
                return "720x486";
            }
            else if (filename.Contains("720x480"))
            {
                return "720x480";
            }
            else if (filename.Contains("720x576"))
            {
                return "720x576";
            }
            else if (filename.Contains("768x576"))
            {
                return "768x576";
            }
            else
            {
                return "NA";
            }

        }

        private string getAspectRatio(FileInfo file)
        {

            string filename = file.Name;

            if (filename.Contains("16x9") || filename.Contains("16X9"))
            {
                return "1778 1818";
            }
            else if (filename.Contains("4x3") || filename.Contains("4X3"))
            {
                return "1333";
            }
            else if (filename.Contains("3x2") || filename.Contains("3X2"))
            {
                return "1500";
            }
            else
            {
                return "NA";
            }
        }

        private string getScanType(FileInfo file)
        {
            string filename = file.Name;
            if(filename.Contains(".mpg") || (filename.Contains("Music") && filename.Contains("Spoken")) || filename.Contains("BYU"))
            {
                return "Interlaced";
            }
            else
            {
                return "Progressive";
            }
        }

        private string getFrameRate(FileInfo file)
        {
            string aspectRatio = getAspectRatio(file);
            if (aspectRatio == "768x576")
            {
                return "25000";
            }
            string filename = file.Name;

            if(filename.Contains("2997") || filename.Contains("29.97"))
            {
                return "29970";
            }
            else if (filename.Contains("2398") || filename.Contains("23.98"))
            {
                return "23976";
            }
            else if (filename.Contains("PAL"))
            {
                return "25000";
            }
            else
            {
                return "NA";
            }
        }

        private void addElementToXml(XmlWriter writer, string elementName, string elementValue)
        {
            writer.WriteElementString(elementName, elementValue);
            writer.WriteWhitespace("\r\n");
        }

        private string getBitRate(FileInfo file)
        {
            // The idea here is that we are going to parse the file name and get the bit rate

            if (file.Name.Contains("300k") || file.Name.Contains("360p"))
            {
                return "300";
            }
            if (file.Name.Contains("1000k"))
            {
                return "1000";
            }
            if (file.Name.Contains("8000k") || file.Name.Contains("1080p"))
            {
                return "8000";
            }
            if (file.Name.Contains("1800k") || file.Name.Contains("720p"))
            {
                return "1800";
            }
            if (file.Name.Contains("2500"))
            {
                return "2500";
            }
            return "NA";
        }

        //test to see if it is empty
        //test files to see if they are writable

        //get all files and create an xml

        // Always create an XML--either based on file name or the old xml

        //move the files and their xml
    }
    
}
