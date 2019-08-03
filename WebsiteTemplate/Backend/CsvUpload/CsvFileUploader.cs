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

        protected DataService DataService { get; set; }

        public CsvFileUploader(DataService dataService)
        {
            DataService = dataService;
        }

        public override IList<InputField> GetInputFields()
        {
            var results = new List<InputField>();

            var extraInputs = GetAdditionalInputs();

            var tabName = extraInputs.Count > 0 ? "Main" : "";

            results.Add(new FileInput("File", "File", mandatory: true, tabName: tabName));
            results.Add(new StringInput("Separator", "Column Separator", ColumnSeparator, mandatory: true, tabName: tabName));
            results.Add(new NumericInput<int>("Skip", "Lines to Skip", 0, tabName: tabName));
            results.Add(new ViewInput("Mappings", "ColumnMappings", new MappingView(), GetParameters(), mandatory: true, tabName: tabName));

            results.AddRange(extraInputs);

            return results;
        }

        public virtual IList<InputField> GetAdditionalInputs()
        {
            return new List<InputField>();
        }

        private string GetParameters()
        {
            var results = ColumnsToMap();
            return JsonHelper.SerializeObject(results);
        }

        public virtual string ColumnSeparator { get; } = ",";

        public abstract List<ColumnSetting> ColumnsToMap();

        public abstract FileInfo ProcessMappingResults(List<MappedRow> mappedData, List<string> mappedErrors);

        private List<MappedRow> MapData(List<string> lines, string separator, List<ColumnSetting> mappings, List<string> errors)
        {
            var results = new List<MappedRow>();

            var rowIndex = 1;

            foreach (var line in lines)
            {
                var columnValues = line.Split(separator.ToCharArray()).ToList();

                var row = new MappedRow(rowIndex);

                var columnIndex = 1;
                foreach (var mapping in mappings)
                {
                    var cols = mapping.ColumnNumbers;
                    var columnValue = MapColumnData(mapping.ColumnName, columnValues, mapping.ColumnNumbers, rowIndex, errors);
                    row.Columns.Add(new MappedColumn(mapping.ColumnName, columnIndex, columnValue));
                }

                results.Add(row);

                rowIndex++;
            }

            return results;
        }

        private string MapColumnData(string columnName, List<string> columnValues, string columnNumbers, int rowNumber, List<string> errors)
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
                var linesToSkip = GetValue<int>("Skip");
                if (linesToSkip < 0)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Number of lines to skip cannot be less than zero")
                    };
                }
                var mappings = GetValue<List<JToken>>("Mappings") ?? new List<JToken>();

                if (mappings.Count == 0)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Mappings are mandatory and must be provided, unable to process file.")
                    };
                }

                var columnSettings = new List<ColumnSetting>();
                foreach (JObject mapping in mappings)
                {
                    var field = mapping.GetValue("Field")?.ToString();
                    var columns = mapping.GetValue("Columns")?.ToString();
                    columnSettings.Add(new ColumnSetting(field, columns));
                }

                var lines = QBicUtils.GetString(file.Data)
                                     .Split("\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => x.Trim())
                                     .Where(x => !String.IsNullOrWhiteSpace(x))
                                     .Skip(linesToSkip)
                                     .ToList();

                var errors = new List<string>();
                var mappedData = MapData(lines, separator, columnSettings, errors);

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