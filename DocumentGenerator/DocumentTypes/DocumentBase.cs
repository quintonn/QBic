using DocumentGenerator.Settings;
using DocumentGenerator.Styles;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator.DocumentTypes
{
    public abstract class DocumentBase
    {
        protected Document Document { get; set; }
        protected StyleSetup StyleSetup { get; set; }
        protected DocumentSettings DocumentSettings { get; set; }

        public DocumentBase(StyleSetup styleSetup, DocumentSettings settings)
        {
            Document = new Document();
            DocumentSettings = settings;
            StyleSetup = styleSetup;

            Setup();
        }

        private void Setup()
        {
            StyleSetup.SetStyle(StyleNames.Normal, Document.Styles[StyleNames.Normal]);
            StyleSetup.SetStyle(StyleNames.Heading1, Document.Styles[StyleNames.Heading1]);
            StyleSetup.SetStyle(StyleNames.Footer, Document.Styles[StyleNames.Footer]);
        }

        public void SetSideMargin(Unit unit)
        {
            Document.DefaultPageSetup.LeftMargin = unit;
            Document.DefaultPageSetup.RightMargin = unit;
        }

        public void SetTopAndBottomMargin(Unit unit)
        {
            Document.DefaultPageSetup.TopMargin = unit;
            Document.DefaultPageSetup.BottomMargin = unit;
        }
    }
}
