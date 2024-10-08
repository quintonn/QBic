﻿using DocumentGenerator.DocumentTypes;
using DocumentGenerator.Settings;
using DocumentGenerator.Styles;
using Microsoft.Extensions.DependencyInjection;
using MigraDoc.DocumentObjectModel;
using System;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestPdf : OpenFile
    {
        private IServiceProvider Container { get; set; }

        public TestPdf(IServiceProvider container)
        {
            Container = container;
        }

        public override string Description
        {
            get
            {
                return "Test PDF generation";
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override string GetFileNameAndExtension()
        {
            return "Test.pdf";
        }

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var result = new FileInfo();

            var document = new BasicTableLayoutDocument(Container.GetService<StyleSetup>(), new DocumentSettings(DocumentType.Pdf, Orientation.Landscape));

            document.SetDocumentTitle("This is a test PDF document");

            document.AddRowHeading("Name", "Name");
            document.AddRowHeading("Surname", "Surname");

            for (var i = 0; i < 50; i++)
            {
                document.AddRowUsingParams("Steve", "Brown");
                document.AddRowUsingParams("Jim", "Brown");
                document.AddRowUsingParams("Jack", "Black");
            }

            var user = Container.GetService<ContextService>().GetRequestUser();


            var formats = DateTime.Now.GetDateTimeFormats();
            
            var footer = "Printed by " + user.UserName + " on " + System.DateTime.Now.ToString("yyyy-MM-dd");
            document.SetDocumentFooter(footer);

            document.SetSideMargin(Unit.FromCentimeter(1));

            using (var stream = document.GenerateDocument(false))
            {
                result.Data = stream.ToArray();
                result.FileName = document.DocumentTitle;
                result.MimeType = "application/pdf";
                result.FileExtension = "pdf";
            }

            return result;
        }

        public override EventNumber GetId()
        {
            return new EventNumber(555);
        }
    }
}