using DocumentGenerator.DocumentTypes;
using DocumentGenerator.Settings;
using DocumentGenerator.Styles;
using Microsoft.Practices.Unity;
using MigraDoc.DocumentObjectModel;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestPdf : OpenFile
    {
        private IUnityContainer Container { get; set; }

        public TestPdf(IUnityContainer container)
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

            var document = new BasicTableLayoutDocument(Container.Resolve<StyleSetup>(), new DocumentSettings(DocumentType.Pdf, Orientation.Landscape));

            document.SetDocumentTitle("This is a test PDF document");

            document.AddRowHeading("Name", "Name");
            document.AddRowHeading("Surname", "Surname");

            for (var i = 0; i < 50; i++)
            {
                document.AddRowUsingParams("Steve", "Brown");
                document.AddRowUsingParams("Jim", "Brown");
                document.AddRowUsingParams("Jack", "Black");
            }

            var userTask = BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync(Container.Resolve<UserContext>());
            userTask.Wait();
            var user = userTask.Result as User;
            var formats = DateTime.Now.GetDateTimeFormats();
            
            var footer = "Printed by " + user.UserName + " on " + System.DateTime.Now.ToString("yyyy-MM-dd");
            document.SetDocumentFooter(footer);

            document.SetSideMargin(Unit.FromCentimeter(1));

            using (var stream = document.GenerateDocument(false))
            {
                result.Data = stream.ToArray();
                result.FileName = document.DocumentTitle;
                result.MimeType = "application/pdf";
            }

            return result;
        }

        public override EventNumber GetId()
        {
            return new EventNumber(555);
        }
    }
}