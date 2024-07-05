﻿using System.Collections.Generic;
using System.Text;
using WebsiteTemplate.Backend.CsvUpload;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems
{
    public class CsvTest : CsvFileUploader
    {
        public CsvTest(DataService dataService) : base(dataService)
        {
        }

        public override string Description => "Test CSV File Uploader";

        public override List<ColumnSetting> ColumnsToMap()
        {
            return new List<ColumnSetting>()
            {
                new ColumnSetting("name", "1"),
                new ColumnSetting("age", "2")
            };
        }

        public override EventNumber GetId()
        {
            return MenuNumber.TestCsvFileUpload;
        }

        public override FileInfo ProcessMappingResults(List<MappedRow> mappedData, List<string> mappedErrors)
        {
            var result = new FileInfo();
            //using (var stream = document.GenerateDocument(false))
            {
                result.Data = Encoding.UTF8.GetBytes("testing123");
                result.FileName = "Test file";
                result.MimeType = "text/plain";
                result.FileExtension = "txt";
            }

            return result;
        }
    }
}