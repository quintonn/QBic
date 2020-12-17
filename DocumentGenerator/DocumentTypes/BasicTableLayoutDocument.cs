using DocumentGenerator.Settings;
using DocumentGenerator.Styles;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocumentGenerator.DocumentTypes
{
    public class BasicTableLayoutDocument : DocumentBase
    {
        /// <summary>
        /// Heading/title for the document.
        /// </summary>
        public string DocumentTitle { get; set; }

        /// <summary>
        /// Dictionary of table headings.
        /// Key is key to dictionary, and value is the headings to be displayed.
        /// </summary>
        public Dictionary<string, string> TableHeadings { get; set; }

        /// <summary>
        /// Actual content of the table.
        /// The main list is the rows and the dictionary the actual column values.
        /// The column keys are the same as the keys in <see cref="TableHeadings"/>.
        /// </summary>
        public List<Dictionary<string, string>> TableContent { get; set; }

        /// <summary>
        /// A footer to display on the bottom of all pages
        /// </summary>
        public string DocumentFooter { get; set; }

        /// <summary>
        /// Get or set a value used to determine if the page number should be displayed in the footer.
        /// Will be displayed as Page x of y
        /// </summary>
        public bool ShowPageNumber { get; set; } = true;

        public BasicTableLayoutDocument(StyleSetup styleSetup, DocumentSettings settings)
            :base(styleSetup, settings)
        {
            TableHeadings = new Dictionary<string, string>();
            TableContent = new List<Dictionary<string, string>>();
        }

        public void SetDocumentTitle(string title)
        {
            DocumentTitle = title;
        }

        public void SetDocumentFooter(string footer)
        {
            DocumentFooter = footer;
        }

        public void AddRowHeading(string keyAndHeading)
        {
            AddRowHeading(keyAndHeading, keyAndHeading);
        }

        public void AddRowHeading(string columnKey, string heading)
        {
            TableHeadings.Add(columnKey, heading);
        }

        public Dictionary<string, string> AddRow()
        {
            var row = new Dictionary<string, string>();

            TableContent.Add(row);

            return row;
        }

        /// <summary>
        /// This method will attempt to add a row using values in a list.
        /// The number of items in the list must match the number of items in the <see cref="TableHeadings"/> property.
        /// Items from the <paramref name="rowData"/> parameter, will be added in the order they appear in the list.
        /// </summary>
        /// <param name="rowData"></param>
        public void AddRowUsingList(IList<string> rowData)
        {
            AddRowUsingParams(rowData.ToArray());
        }

        /// <summary>
        /// This method will attempt to add a row using values provided as params.
        /// The number of items in the list must match the number of items in the <see cref="TableHeadings"/> property.
        /// Items from the <paramref name="rowData"/> parameter, will be added in the order they appear in the list.
        /// </summary>
        /// <param name="rowData"></param>
        public void AddRowUsingParams(params string[] rowData)
        {
            if (rowData.Count() != TableHeadings.Keys.Count)
            {
                throw new Exception("The number of items in the list of data to add to a row must match the number of headings in the document");
            }

            var row = AddRow();
            for (var i = 0; i < TableHeadings.Keys.Count; i++)
            {
                var columnKey = TableHeadings.Keys.ElementAt(i);
                var value = rowData[i];
                row.Add(columnKey, value);
            }
        }

        /// <summary>
        /// Adds an empty line on the page, for spacing.
        /// </summary>
        public void AddEmptyRow()
        {
            TableContent.Add(null);
        }

        private MemoryStream RenderDocument()
        {
            switch (DocumentSettings.DocumentType)
            {
                //case DocumentType.Rtf:
                //    var renderer = new RtfDocumentRenderer();
                //    var documentString = renderer.RenderToString(Document, null);
                //    using (var stream = new MemoryStream())
                //    using (var writer = new StreamWriter(stream))
                //    {
                //        writer.Write(documentString);
                //        writer.Flush();
                //        //stream.Position = 0;
                //        stream.Seek(0, SeekOrigin.Begin);
                //        return stream;
                //    }
                case DocumentType.Pdf:
                    var pdfRenderer = new PdfDocumentRenderer(true);
                    pdfRenderer.Document = Document;
                    pdfRenderer.RenderDocument();
                    using (var stream = new MemoryStream())
                    {
                        pdfRenderer.PdfDocument.Save(stream, false);
                        stream.Seek(0, SeekOrigin.Begin);
                        return stream;
                    }
                default:
                    throw new Exception("Unknown document type: " + DocumentSettings.DocumentType);
            }
        }

        private void CreateDocument(bool lastLineBold)
        {
            Document.DefaultPageSetup.Orientation = DocumentSettings.Orientation;

            AddDocumentHeading();

            AddDocumentFooter();
            
            AddTableData(lastLineBold);
        }

        private void AddDocumentHeading()
        {
            var section = Document.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = true;

            var head = section.Headers.FirstPage.AddParagraph(DocumentTitle);
            head.Style = StyleNames.Heading1;

            head = section.Headers.Primary.AddParagraph(DocumentTitle);
            head.Style = StyleNames.Heading1;
        }

        private void AddDocumentFooter()
        {
            if (String.IsNullOrWhiteSpace(DocumentFooter) && ShowPageNumber == false)
            {
                return; // Don't add any footer
            }
            var section = Document.LastSection;

            var footer1 = new Paragraph();
            if (!String.IsNullOrWhiteSpace(DocumentFooter))
            {
                footer1.Style = StyleNames.Footer;
                footer1.AddText(DocumentFooter);

                if (ShowPageNumber)
                {
                    footer1.AddLineBreak();
                }
            }

            if (ShowPageNumber)
            {
                footer1.AddText("Page ");
                footer1.AddPageField();
                footer1.AddText(" of ");
                footer1.AddNumPagesField();
            }

            section.Footers.Primary.Add(footer1.Clone());
            section.Footers.FirstPage.Add(footer1.Clone());
        }

        private void AddTableData(bool lastLineBold)
        {
            // Set the table to be the entire width of the page
            double width;
            double height;
            if (DocumentSettings.Orientation == Orientation.Landscape)
            {
                width = Document.DefaultPageSetup.PageHeight.Centimeter;
                height = Document.DefaultPageSetup.PageWidth.Centimeter;
            }
            else
            {
                height = Document.DefaultPageSetup.PageHeight.Centimeter;
                width = Document.DefaultPageSetup.PageWidth.Centimeter;
            }

            var leftMargin = Document.DefaultPageSetup.LeftMargin.Centimeter;
            var rightMargin = Document.DefaultPageSetup.RightMargin.Centimeter;

            // Make all columns the same length (for now)
            var table = new Table();
            var colSize = (width - leftMargin - rightMargin) / TableHeadings.Keys.Count;

            // Give the table a nice border.
            table.Borders.Width = 0.75;

            // Add enought columns
            for (var i = 0; i < TableHeadings.Keys.Count; i++)
            {
                table.AddColumn(Unit.FromCentimeter(colSize));
            }

            var row = table.AddRow();
            row.Shading.Color = Colors.DarkGray;
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            row.Format.Font.Size = 12;
            row.Borders.Width = 0;
            row.Height = Unit.FromPoint(15);

            for (var i = 0; i < TableHeadings.Keys.Count; i++)
            {
                var heading = TableHeadings.ElementAt(i);
                var cell = row.Cells[i];
                cell.AddParagraph(heading.Value);
            }

            for (var i = 0; i < TableContent.Count; i++)
            {
                var rowData = TableContent.ElementAt(i);
                row = table.AddRow();

                if (rowData == null) // Empty row
                {
                    row.Borders.Width = 0;
                    continue;
                }

                row.Style = StyleNames.Normal;
                row.Borders.Width = 0;
                row.Borders.Top.Width = 0.75;

                if (lastLineBold && i == (TableContent.Count - 1))
                {
                    row.Format.Font.Bold = true;
                    row.Borders.Width = 0;
                }

                for (var j = 0; j < TableHeadings.Keys.Count; j++)
                {
                    var headingKey = TableHeadings.Keys.ElementAt(j);
                    var cell = row.Cells[j];
                    //cell.MergeRight  --ColSpan
                    cell.AddParagraph(rowData[headingKey]);
                }
            }

            //Don't need this, 'table.Borders.Width' above set borders.
            //table.SetEdge(0, 0, 2, 100 + 1, Edge.Interior | Edge.Box, BorderStyle.Single, 1, Colors.Black);
            Document.LastSection.Add(table);
        }

        public MemoryStream GenerateDocument(bool lastLineBold = false) //TODO: Instead of last line bold, might need the ability to set each row's font and stuff.
        {
            var columnCount = TableHeadings.Keys.Count;
            foreach (var row in TableContent)
            {
                if (row == null)
                {
                    continue;
                }
                if (row.Keys.Count != columnCount)
                {
                    throw new Exception("The number of keys/columns in TableHeadings must match the number of keys/columns in each item of TableContent");
                }
            }

            CreateDocument(lastLineBold);

            var stream = RenderDocument();

            return stream;
        }
    }
}
