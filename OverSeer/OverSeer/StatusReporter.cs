using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Google.GData.Extensions;
using System.Xml;
using System.IO;

namespace OverSeer
{
    class StatusReporter
    {

        private string username = @"christopherjohnsonpiper@gmail.com";
        private string password = @"ziggy100";

        private DateTime today = DateTime.Today;

        private SpreadsheetEntry ReportSpreadSheet;
        private SpreadsheetEntry failSheet;
        private WorksheetEntry currentWorkSheet;
        private SpreadsheetFeed allSheets;
        private Dictionary<string, string> spreadsheets;
        private SpreadsheetsService service;
        private CellFeed cells;
        private DateTime now = DateTime.Now;
        private string initials;
        private string selectedType;
        public bool canClose;

        private List<FileObjects> fileObjects = new List<FileObjects>();
        private List<ProjectObject> projectObjects = new List<ProjectObject>();

        public StatusReporter()
        {
            fileObjects = MainWindow.CurrentFileObjects;
            projectObjects = MainWindow.CurrentProjectObjects;

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

            SpreadsheetQuery query = new SpreadsheetQuery();
            this.allSheets = this.service.Query(query);

            for (int i = 0; i < allSheets.Entries.Count; i++)
            {
                if (this.allSheets.Entries[i].AlternateUri.Content.Contains("0AnSESHA7JsTFdFFBNjZ1U3JzdEFXdlZOa0dhQXJNMEE"))
                {
                    this.ReportSpreadSheet = (SpreadsheetEntry)allSheets.Entries[i];
                }
                if (this.allSheets.Entries[i].AlternateUri.Content.Contains("0AnSESHA7JsTFdDBGUWtqRVJVbEFFYVlaUmZnRTBaVWc"))
                {
                    this.failSheet = (SpreadsheetEntry)allSheets.Entries[i];
                }
            }
        }

        public void addFilesQueue(ProjectObject project)
        {
            // get worksheet by projectName
            WorksheetEntry worksheet = this.getWorksheetByProjectName(project);

            // Clear Queue Column
            CellFeed queueColumn = createCellFeedFromQuery(1, 1, 2, -1, worksheet);
            this.clearCellFeed(queueColumn);

            //this.quickAdditions(worksheet, "queue", fileNames);

            foreach (FileObjects file in project.currentFileObjects)
            {
                this.InsertRow(worksheet, file.CurrentFileInfo.Name, "Queue");
            }
            
        }

        public WorksheetEntry getWorksheetByProjectName(ProjectObject project)
        {
            foreach (WorksheetEntry worksheet in this.ReportSpreadSheet.Worksheets.Entries)
            {
                if(worksheet.Title.Text.Equals(project.ProjectName))
                {
                    return worksheet;
                }
            }

            // If we get here, we have to create a new sheet because we do not already have one. 

            return this.addWorkSheet(project.ProjectName);
        }

        public void addResult(int column, FileObjects file)
        {
            // Which worksheet are we working on?
            WorksheetEntry worksheet = getWorksheetByProjectName(file.Project);
            // Which column are we entering it into?
            CellFeed ColumnFeed = createCellFeedFromQuery(column, column, 2, -1, worksheet);
            string rowHeader;
            if(column == 2)
            {
                rowHeader = "Passed";
            }
            else if (column == 3)
            {
                rowHeader = "Cautioned";
                //InsertIntoFirstEmptyRowFailOrCaution(column, worksheet, fileName, rowHeader, result, true);
                
            }
            else
            {
                rowHeader = "Failed";
                InsertIntoFirstEmptyRowFailOrCaution(column, worksheet, file.CurrentFileInfo.Name, rowHeader, file.QCReport, false);
                tryToRemoveFromQueue(file.CurrentFileInfo.Name, worksheet);
                return;

            }


            InsirtIntoFirstEmptyRow(column, worksheet, file.CurrentFileInfo.Name, rowHeader);

            // Remove it from the queue if you can! 
            tryToRemoveFromQueue(file.CurrentFileInfo.Name, worksheet);
            
        }

        public void updateQueues()
        {
            // Go to autoQCPassed
            DirectoryInfo passedFolder = new DirectoryInfo(@"\\cob-hds-1\compression\QC\autoQCPassed\");
            //DirectoryInfo projectFolder = new DirectoryInfo(@"\\cob-hds-1\compression\QC\QCing\otherFiles\projects");
            //List<FileInfo> passedFiles = utility.checkForSystemFiles(passedFolder.GetFiles().ToList<FileInfo>());
            //<FileInfo> projects = utility.checkForSystemFiles(projectFolder.GetFiles().ToList<FileInfo>());

            foreach (ProjectObject project in projectObjects)
            {
                if (project.ProjectName.Contains("Jeremiah"))
                {
                   // continue;
                }
                //List<FileInfo> fileNames = new List<FileInfo>();

                //string[] keyWords = null;

                
                //foreach (FileInfo tempProject in projects)
                //{
                //    string tempName = utility.getValueFromXML(tempProject, "Name");


                //    if (tempName == System.IO.Path.GetFileNameWithoutExtension(project.Name))
                //    {
                //        keyWords = utility.getValueFromXML(tempProject, "Keyword").Split(',');
                //        break;
                //    }
                //}

                //if (project.Keywords.Count == 0)
                //{
                //    continue;
                //}
                //foreach (FileObjects file in fileObjects)
                //{
                //    if (file.CurrentQCStatus == QCStatus.Passed)
                //    {
                //        foreach (string keyword in project.Keywords)
                //        {
                //            if (file.CurrentFileInfo.Name.Contains(keyword))
                //            {
                //                fileNames.Add(file);
                //                break;
                //            }
                //        }
                //    }
                //}

                if (project.currentFileObjects.Count > 0)
                {
                    this.addFilesQueue(project);
                }
            }
        }

        public void tryToRemoveFromQueue(string fileName, WorksheetEntry worksheet)
        {
            CellFeed column = createCellFeedFromQuery(1, 1, 2, -1, worksheet);

            foreach (CellEntry cell in column.Entries)
            {
                if (cell.InputValue.Contains(fileName))
                {
                    cell.InputValue = "";
                    cell.Update();
                    return;
                }
            }
        }

        public void addFail(FileObjects file)
        {
            WorksheetEntry workSheet = (WorksheetEntry)this.failSheet.Worksheets.Entries[0];

            AtomLink listFeedLink = workSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            ListEntry row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = "filename", Value = file.CurrentFileInfo.Name });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "reasonforfail", Value = file.QCReport });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "date", Value = DateTime.Today.ToString() });
            service.Insert(listFeed, row);

        }
        /// <summary>
        /// Creates a cell feed by executing a query using the given parameters. If you do not want to specify a parameter, use -1
        /// </summary>
        /// <param name="minColumn"></param>
        /// <param name="maxColumn"></param>
        /// <param name="minRow"></param>
        /// <param name="maxRow"></param>
        /// <param name="workSheet"></param>
        /// <returns></returns>
        public CellFeed createCellFeedFromQuery(int minColumn, int maxColumn, int minRow, int maxRow, WorksheetEntry workSheet)
        {
            CellQuery query = new CellQuery(workSheet.CellFeedLink);
            if (maxColumn != -1)
            {
                query.MaximumColumn = (uint)maxColumn;
            }
            if (minColumn != -1)
            {
                query.MinimumColumn = (uint)minColumn;
            }
            if (minRow != -1)
            {
                query.MinimumRow = (uint)minRow;
            }
            if (maxRow != -1)
            {
                query.MaximumRow = (uint)maxRow;
            }
            query.ReturnEmpty = ReturnEmptyCells.yes;

            return this.service.Query(query);
        }

        public int doesCellFeedContainString(CellFeed feed, string target)
        {
            foreach(CellEntry cell in feed.Entries)
            {
                if (cell.InputValue.Contains(target))
                {
                    return (int)cell.Row;
                }
            }
            return -1;
        }

        // Clears each cell's inputValue
        public void clearCellFeed(CellFeed feed)
        {
            foreach (CellEntry cell in feed.Entries)
            {
                cell.InputValue = "";
                cell.Update();
            }
        }

        public void InsirtIntoFirstEmptyRow(int column, WorksheetEntry workSheet, String content, String columnName)
        {
            // Start going through 
            CellQuery query = new CellQuery(workSheet.CellFeedLink);
            query.MinimumRow = 2;
            query.MaximumColumn = (uint)column;
            query.MinimumColumn = (uint)column;
            query.ReturnEmpty = ReturnEmptyCells.yes;

            CellFeed columnFeed = this.service.Query(query);

            foreach(CellEntry cell in columnFeed.Entries)
            {
                if (cell.InputValue == "")
                {
                    // THIS IS THE FIRST EMPTY CELL
                    try
                    {
                        cell.InputValue = content;
                        cell.Update();
                    }
                    catch(Exception e)
                    {
                        logger.writeErrorLog("Could not write something to the queue: " + content);
                    }

                    return; 
                }
            }
            
            // If we got here, we have to insert a row
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = workSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            ListEntry row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = columnName.ToLower(), Value = content });
            service.Insert(listFeed, row);

        }
        public void InsertRow(WorksheetEntry workSheet, String content, String columnName)
        {
            AtomLink listFeedLink = workSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            ListEntry row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = columnName.ToLower(), Value = content });
            service.Insert(listFeed, row);
        }

        public void InsertIntoFirstEmptyRowFailOrCaution(int column, WorksheetEntry workSheet, String content, String columnName, string report, bool caution)
        {
            // Start going through 
            CellQuery query = new CellQuery(workSheet.CellFeedLink);
            query.MinimumRow = 2;
            query.MaximumColumn = (uint)column;
            query.MinimumColumn = (uint)column;
            query.ReturnEmpty = ReturnEmptyCells.yes;

            CellFeed columnFeed = this.service.Query(query);

            foreach (CellEntry cell in columnFeed.Entries)
            {
                if (cell.InputValue == "")
                {
                    // THIS IS THE FIRST EMPTY CELL
                    cell.InputValue = content;
                    cell.Update();

                    if (caution)
                    {
                        CellFeed reportCell = this.createCellFeedFromQuery(column + 2, column + 2, (int)cell.Row, (int)cell.Row, workSheet);
                        foreach (CellEntry adjacent in reportCell.Entries)
                        {
                            adjacent.InputValue = report;
                            adjacent.Update();
                        }
                    }
                    else
                    {
                        CellFeed reportCell = this.createCellFeedFromQuery(column + 1, column + 1, (int)cell.Row, (int)cell.Row, workSheet);
                        foreach (CellEntry adjacent in reportCell.Entries)
                        {
                            adjacent.InputValue = report;
                            adjacent.Update();
                        }
                    }
                    return;

                }
            }

            // If we got here, we have to insert a row
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = workSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            ListEntry row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = columnName.ToLower(), Value = content });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "reason", Value = report });
            service.Insert(listFeed, row);
        }

        public void quickAdditions(WorksheetEntry workSheet, String columnName, List<FileInfo> files)
        {
            if (files.Count == 0)
            {
                return;
            }
            AtomLink listFeedLink = workSheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            ListEntry row = new ListEntry();

            foreach (FileInfo file in files)
            {
                row.Elements.Add(new ListEntry.Custom() { LocalName = columnName.ToLower(), Value = System.IO.Path.GetFileNameWithoutExtension(file.Name)});
                
            }

            service.Insert(listFeed, row);
        }

        public WorksheetEntry addWorkSheet(String projectName)
        {
            SpreadsheetEntry ReportSheet;
            WorksheetEntry worksheet = new WorksheetEntry();
            worksheet.Title.Text = projectName;
            worksheet.Cols = 5;
            worksheet.Rows = 10;
            WorksheetFeed wsFeed = this.ReportSpreadSheet.Worksheets;
            this.service.Insert(wsFeed, worksheet);
            wsFeed = this.ReportSpreadSheet.Worksheets;
            // Gotta get that same spreadsheet through the wsFeed now!!!
            foreach (WorksheetEntry tempSheet in wsFeed.Entries)
            {
                if (tempSheet.Title.Text.Equals(projectName))
                {
                    worksheet = tempSheet;
                }
            }

                    // Do Some Basic Formatting!!!
            CellFeed topRow = createCellFeedFromQuery(1, 5, 1, 1, worksheet);
                    // Queue
            CellEntry temp;
            temp = (CellEntry)topRow.Entries[0];
            temp.InputValue = "Queue";
            temp.Update();
                    // Passed
            temp = (CellEntry)topRow.Entries[1];
            temp.InputValue = "Passed";
            temp.Update();
                    // Cautioned
            temp = (CellEntry)topRow.Entries[2];
            temp.InputValue = "Cautioned";
            temp.Update();
                    // Failed
            temp = (CellEntry)topRow.Entries[3];
            temp.InputValue = "Failed";
            temp.Update();

            temp = (CellEntry)topRow.Entries[4];
            temp.InputValue = "Reason";
            temp.Update();

            return worksheet;
            
        }

      

        public int hasSheet(String projectName)
        {
            for (int i = 0; i < this.allSheets.Entries.Count; i++)
            {
                if (this.allSheets.Entries[i].AlternateUri.Content.Contains("0AnSESHA7JsTFdFFBNjZ1U3JzdEFXdlZOa0dhQXJNMEE"))
                {
                    SpreadsheetEntry ReportSheet;
                    ReportSheet = (SpreadsheetEntry)allSheets.Entries[i];
                    WorksheetFeed wsFeed = ReportSheet.Worksheets;

                    for (int x = 0; x < wsFeed.Entries.Count; x++)
                    {
                        if (wsFeed.Entries[x].Title.Text.Equals(projectName))
                        {
                            return x;
                        }
                    }
                }
            }

            return -1;
        }
        

    }
}
