using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.CsvUpload
{
    public abstract class CsvFileUploader : GetInput
    {
        public override bool AllowInMenu => true;

        public CsvFileUploader(DataService dataService) : base(dataService)
        {
            
        }

        public override IList<InputField> GetInputFields()
        {
            var results = new List<InputField>();

            var extraInputs = GetAdditionalInputs();

            var tabName = extraInputs.Count > 0 ? "Main" : "";

            results.Add(new FileInput("File", "File", mandatory: true, tabName: tabName));
            results.Add(new StringInput("Separator", "Column Separator", ColumnSeparator, mandatory: true, tabName: tabName));
            results.Add(new NumericInput<int>("Skip", "Lines to Skip", LinesToSkip, tabName: tabName));
            results.Add(new ViewInput<CsvRowValue>("Mappings", "Column Mappings", new MappingView(), GetParameters(), mandatory: true, tabName: tabName));
            results.Add(new BooleanInput("IsQuoted", "Are values in quotes", false, null, false));

            results.AddRange(extraInputs);

            return results;
        }

        public virtual IList<InputField> GetAdditionalInputs()
        {
            return new List<InputField>();
        }

        private List<IViewInputValue> GetParameters()
        {
            var columns = ColumnsToMap();
            //var results = new List<IViewInputValue>();

            //var rowNumber = 0;
            //foreach (var column in columns)
            //{
            //    var item = new CsvRowValue()
            //    {
            //        Field = column.Field,
            //        Columns = column.Columns,
            //        rowId = rowNumber++
            //    };
            //    results.Add(item);
            //}

            return columns.Cast<IViewInputValue>().ToList();
        }

        public virtual string ColumnSeparator { get; } = ",";
        public virtual int LinesToSkip { get; } = 0;

        public abstract List<CsvRowValue> ColumnsToMap();

        public abstract FileInfo ProcessMappingResults(List<MappedRow> mappedData, List<string> mappedErrors);

        protected List<MappedRow> MapData(TextFieldParser parser, int linesToSkip, List<CsvRowValue> mappings, List<string> errors)
        {
            var results = new List<MappedRow>();

            var rowIndex = 0;

            while (!parser.EndOfData)
            {
                if (rowIndex < linesToSkip)
                {
                    parser.ReadFields(); // move cursor to next line
                    rowIndex++;
                    continue;
                }
                var fields = parser.ReadFields().ToList();
                    
                var row = new MappedRow(rowIndex);

                var columnIndex = 1;
                foreach (var mapping in mappings)
                {
                    var cols = mapping.Columns;
                    var columnValue = MapColumnData(mapping.Field, fields, mapping.Columns, rowIndex, errors);
                    row.Columns.Add(new MappedColumn(mapping.Field, columnIndex, columnValue));
                }

                results.Add(row);

                rowIndex++;
            }

            return results;
        }

        protected string MapColumnData(string columnName, List<string> columnValues, string columnNumbers, int rowNumber, List<string> errors)
        {
            var result = "";

            var disjunctions = columnNumbers.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var disjunction in disjunctions)
            {
                var conjunctions = disjunction.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                result = "";
                foreach (var conjunction in conjunctions)
                {
                    int columnNumber = -1;
                    if (int.TryParse(conjunction, out columnNumber))
                    {
                        if (columnNumber < 1)
                        {
                            errors.Add($"Unable to map {columnName} on row {rowNumber} because the mapped column number is less than 1, it was {columnNumber}");
                            continue;
                        }
                        if (columnValues.Count < columnNumber)
                        {
                            errors.Add($"Unable to map {columnName} on row {rowNumber} because there are less than {columnNumber} columns");
                            continue;
                        }
                        else
                        {
                            result += columnValues[columnNumber - 1] + " ";
                        }
                    }
                    else
                    {
                        errors.Add($"Unable to process row {rowNumber} because column number {conjunction} is not a valid number");
                        continue;
                    }
                }
                if (!String.IsNullOrWhiteSpace(result.Trim()))
                {
                    break;
                }
            }

            return result.Trim();
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                };
            }
            else if (actionNumber == 0)
            {
                var file = GetValue<FileInfo>("File");
                if (file == null || file.Data == null)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("File empty or contains no data")
                    };
                }
                var separator = GetValue("Separator");
                var isQuoted = GetValue<bool>("IsQuoted");
                var linesToSkip = GetValue<int>("Skip");
                if (linesToSkip < 0)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Number of lines to skip cannot be less than zero")
                    };
                }
                var mappings = GetValue<List<CsvRowValue>>("Mappings") ?? new List<CsvRowValue>();

                if (mappings.Count == 0)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Mappings are mandatory and must be provided, unable to process file.")
                    };
                }

                var parser = new TextFieldParser(new System.IO.StringReader(QBicUtils.GetString(file.Data)));
                parser.HasFieldsEnclosedInQuotes = isQuoted;
                parser.SetDelimiters(separator);

                var errors = new List<string>();
                var mappedData = MapData(parser, linesToSkip, mappings, errors);
                parser.Close();

                var fileData = ProcessMappingResults(mappedData, errors);

                var tempFile = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllBytes(tempFile, fileData.Data);

                var jsonData = new
                {
                    filePath = tempFile,
                    fileName = fileData.FileName,
                    extension = fileData.FileExtension,
                    mimeType = fileData.MimeType
                };
                var data = JsonHelper.SerializeObject(jsonData);

                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ShowCsvProcessResult, data),
                };
            }
            else
            {
                return null;
            }
        }
    }
}