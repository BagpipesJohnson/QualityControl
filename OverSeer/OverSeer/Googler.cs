using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Google.GData.Extensions;
using System.Xml;
namespace OverSeer
{
    public class Googler
    {


        private string username = @"christopherjohnsonpiper@gmail.com";
        private string password = @"ziggy100";

        private DateTime today = DateTime.Today;

        private SpreadsheetEntry currentSpreadSheet;
        private WorksheetEntry currentWorkSheet;
        private SpreadsheetFeed allSheets;
        private Dictionary<string, string> spreadsheets;
        private SpreadsheetsService service;
        private CellFeed cells;
        private DateTime now = DateTime.Now;
        private string initials;
        private string selectedType;
        public bool canClose;

        public Googler()
        {
            this.canClose = true;
            this.service = new SpreadsheetsService("Seeker");
            this.service.setUserCredentials(username, password);

            XmlTextReader reader = new XmlTextReader(@"\\cob-hds-1\compression\QC\QCing\otherFiles\SpreadSheetDictionary.xml");

            this.spreadsheets = new Dictionary<string, string>();
            string key = "";
            string value = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {

                    case XmlNodeType.Text:// Display text
                        Console.Write(reader.Name);

                        key = reader.Value;
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Text)
                            {
                                value = reader.Value;
                                break;
                            }
                        }
                        if (value != "")
                        {
                            if (!this.spreadsheets.ContainsKey(key))
                            {
                                this.spreadsheets.Add(key, value);
                            }
                        }
                        break;
                }
            }

        }

        public void logItConference(string fileName, string result)
        {
            try
            {
                this.canClose = false;
                WorksheetEntry worksheet = null;
                CellFeed cells = null;

                // FIND THE SPREADSHEET AND WORKSHEET
                SpreadsheetQuery query = new SpreadsheetQuery();
                this.allSheets = this.service.Query(query);

                String lastCode = "";
                this.currentSpreadSheet = new SpreadsheetEntry();


                string s = fileName;

                string code = this.getCode(s);

                if (code == "NO FILE CODE FOUND")
                {
                    return;
                }
                string spreadsheetCode = spreadsheets[code];


                if (lastCode != this.getFullCode(s))
                {
                    lastCode = this.getFullCode(s);
                    for (int i = 0; i < this.allSheets.Entries.Count; i++)
                    {
                        if (this.allSheets.Entries[i].AlternateUri.Content.Contains(spreadsheetCode))
                        {
                            this.currentSpreadSheet = (SpreadsheetEntry)this.allSheets.Entries[i];
                            WorksheetFeed wsFeed = this.currentSpreadSheet.Worksheets;

                            for (int x = 0; x < wsFeed.Entries.Count; x++)
                            {
                                WorksheetEntry wksht = (WorksheetEntry)wsFeed.Entries[x];
                                worksheet = wksht;
                                CellQuery Querycells = new CellQuery(wksht.CellFeedLink);
                                Querycells.MaximumColumn = 1;
                                Querycells.MaximumRow = 2;
                                Querycells.MinimumColumn = 1;
                                Querycells.MinimumRow = 2;
                                this.cells = this.service.Query(Querycells);
                                string fix = System.IO.Path.GetFileName(s);

                                if (fix.Contains(this.cells.Entries[0].Content.Content))
                                {
                                    this.currentWorkSheet = wksht;
                                    break;
                                }

                            }
                        }
                    }


                    // NOW, if all went well, we are in the right sheet. Create the list of the needed things--

                    //List of languages

                    CellQuery languageQuery = new CellQuery(this.currentWorkSheet.CellFeedLink);
                    languageQuery.MinimumRow = 3;
                    languageQuery.MaximumColumn = 2;
                    languageQuery.MinimumColumn = 2;
                    languageQuery.ReturnEmpty = ReturnEmptyCells.no;

                    CellFeed languages = this.service.Query(languageQuery);

                    //OVP tabs

                    CellQuery OVPTabs = new CellQuery(this.currentWorkSheet.CellFeedLink);
                    OVPTabs.MinimumRow = OVPTabs.MaximumRow = 1;
                    CellFeed columnHeadings = this.service.Query(OVPTabs);

                    //USE THIS TO FIND EACH INDIVIDUAL VIDEO'S COLUMN

                    uint row = this.getLanguageRow(s, languages);
                    uint col = this.getEditColumn(s, columnHeadings, "QC", this.currentWorkSheet);
                    //OVP tabs

                    //USE THIS TO FIND EACH INDIVIDUAL VIDEO'S COLUMN

                    if (col == 0 || row == 0)
                    {
                        // something failed--write to the google doc to say that it failed
                        return;

                    }

                    //Get Individual Cell

                    CellQuery edit = new CellQuery(this.currentWorkSheet.CellFeedLink);
                    edit.MaximumRow = edit.MinimumRow = row;
                    edit.MaximumColumn = edit.MinimumColumn = col;

                    edit.ReturnEmpty = ReturnEmptyCells.yes;

                    CellFeed editCell = this.service.Query(edit);

                    CellEntry temp = (CellEntry)editCell.Entries[0];

                    if (this.initials == "")
                    {
                        temp.InputValue = "";
                    }
                    else
                    {

                        temp.InputValue = result;
                    }
                    temp.Update();
                    this.canClose = true;




                }
            }
            catch (Exception exeption)
            {
                logger.writeErrorLog("Failed to log " + fileName + " in conference google docs " + DateTime.Now);
                this.canClose = true;
            }
            this.canClose = true;

        }

        private uint getEditColumn(string s, CellFeed columnHeadings, string header, WorksheetEntry currentWorksheet)
        {
            string content;
            // find header
            int headerColumn = 0;
            foreach (CellEntry entry in columnHeadings.Entries)
            {
                content = entry.Content.Content.ToLower();

                if (content.Equals(header.ToLower()))
                {
                    headerColumn = (int)entry.Column;
                }
            }

            if (headerColumn != 0)
            {

                CellQuery findCol = new CellQuery(currentWorkSheet.CellFeedLink);
                findCol.MaximumRow = findCol.MinimumRow = 2;
                findCol.MinimumColumn = (uint)headerColumn;
                CellFeed possibleColumns = this.service.Query(findCol);
                string text;
                if (header.Contains("OVP") || header.Contains("QC"))
                {
                    text = this.getBitRateText(s);
                }
                else
                {
                    text = this.getTechnicalText(s);
                }

                foreach (CellEntry entry in possibleColumns.Entries)
                {
                    content = entry.Content.Content.ToLower();
                    if (content.Contains(text.ToLower()))
                    {
                        return (uint)entry.Column;
                    }
                }

            }
            else
            {
                return 0;
            }

            return 0;


        }

        public string getCode(string fileName)
        {
            string name = System.IO.Path.GetFileName(fileName);
            string[] split = name.Split('-');

            if (split.Length >= 3)
            {
                return split[0] + "-" + split[1] + "-" + split[2][0];
            }
            else
                return "NO FILE CODE FOUND";
        }

        public string getFullCode(string fileName)
        {
            string name = System.IO.Path.GetFileName(fileName);
            string[] split = name.Split('-');

            if (split.Length >= 3)
            {
                return split[0] + "-" + split[1] + "-" + split[2];
            }
            else
                return "NO FILE CODE FOUND";
        }

        public string getBitRateText(string fileName)
        {
            if (fileName.Contains("300k"))
            {
                return "300 kbps WMV";
            }
            if (fileName.Contains("1000k"))
            {
                return "1 Mbps MP4";
            }
            if (fileName.Contains("8000k"))
            {
                return "BC Mezzanine";
            }
            if (fileName.Contains("1800"))
            {
                return "1.8 Mbps MP4";
            }
            if (fileName.Contains("AAC"))
            {
                return "AAC";
            }
            if (fileName.Contains(".mp3"))
            {
                return "64k MP3";
            }
            return "NO LANGUAGE CODE FOUND";
        }

        public string getTechnicalText(string fileName)
        {
            if (fileName.Contains("300k") || (fileName.Contains("360p") && fileName.Contains(".wmv")))
            {
                return "360p WMV";
            }
            if (fileName.Contains("1000k") || (fileName.Contains("360p") && fileName.Contains(".mp4")))
            {
                return "360p MP4";
            }
            if (fileName.Contains("8000k") || (fileName.Contains("1080p")))
            {
                return "1080p MP4";
            }
            if (fileName.Contains("1800") || fileName.Contains("720p"))
            {
                return "720p MP4";
            }
            if (fileName.Contains("AAC"))
            {
                return "AAC";
            }
            if (fileName.Contains(".mp3"))
            {
                return "MP3";
            }
            return "NO LANGUAGE CODE FOUND";
        }

        public uint getLanguageRow(string fileName, CellFeed languages)
        {


            foreach (CellEntry entry in languages.Entries)
            {
                if (fileName.Contains(entry.Content.Content + "."))
                {
                    return entry.Row;
                }
            }
            return 0;

        }

        public uint getFileNameRow(string fileName, CellFeed fileNames)
        {
            string target = fileName.Split('_')[0];

            foreach (CellEntry entry in fileNames.Entries)
            {
                if (entry.Content.Content.Contains(target))
                {
                    return entry.Row;
                }
            }
            return 0;
        }
    }
}