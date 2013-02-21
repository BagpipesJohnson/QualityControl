using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//added
using System.Windows; //needed for MessageBox
//google
using Google.GData.Client;
using Google.GData.Spreadsheets;
using System.Windows.Controls;
//Debug
using System.Diagnostics;

namespace OverSeer
{
    public class AutoQCStatusUpdate
    {
        #region Vars
        //properties
        public bool AutoQC_ON { get; set; }

        //private vars
        private string username = @"carlilelance@gmail.com";
        private string password = @"mltemlte";
        private bool hasTrackingSheet = false;
        #endregion

        #region Functions
        //constructor
        public AutoQCStatusUpdate()
        {
            //login to Google
            //login
            SpreadsheetEntry spreadsheet;
            SpreadsheetsService service = new SpreadsheetsService("AutoQCGoogler");
            service.setUserCredentials(username, password);
            //find the spreadsheet
            SpreadsheetQuery query = new SpreadsheetQuery();
            SpreadsheetFeed feed = service.Query(query);
            spreadsheet = (SpreadsheetEntry)feed.Entries[0];
            foreach (var entry in feed.Entries)
            {
                if (entry.Title.Text.Contains("AutoQC"))
                {
                    //MessageBox.Show(entry.Title.Text);
                    spreadsheet = (SpreadsheetEntry)entry;
                    hasTrackingSheet = true;
                }
            }
            if (!hasTrackingSheet)
            {
                MessageBox.Show("Couldn't find tracking sheet");
            }

            //find the right worksheet
            WorksheetFeed wsFeed = spreadsheet.Worksheets;
            WorksheetEntry worksheetEntry = (WorksheetEntry)wsFeed.Entries[0];
            //MessageBox.Show(worksheetEntry.Title.Text);

            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            //get list feed
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);

            //ListEntry row = (ListEntry)listFeed.Entries[1];

            CellQuery cells = new CellQuery(worksheetEntry.CellFeedLink);
            cells.MaximumColumn = 1;
            cells.MinimumColumn = 1;
            cells.MaximumRow = cells.MinimumRow = 2;

            CellFeed found = service.Query(cells);
            CellEntry temp = (CellEntry)found.Entries[0];
            switch (temp.InputValue)
            {
                case "off":
                    AutoQC_ON = false;
                    break;
                case "on":
                    AutoQC_ON = true;
                    break;
                default:
                   // MessageBox.Show("Wassup!");
                    AutoQC_ON = true;
                    break;
            }
            //foreach (ListEntry row in listFeed.Entries)
            //{
            //    if (row.Title.Text == "off")
            //    {
            //        MessageBox.Show(row.Title.Text);
            //    }
            //}
        }
        #endregion

    }
}
