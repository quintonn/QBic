using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator.Settings
{
    public class DocumentSettings
    {
        /// <summary>
        /// Orientation for the document. Default is <see cref="Orientation.Portrait"/>.
        /// </summary>
        public Orientation Orientation { get; set; } = Orientation.Portrait;

        /// <summary>
        /// The default font family to use for the body of the document. Default is <see cref="FontFamily.Helvetica"/>.
        /// </summary>
        public FontFamily BodyFontFamily { get; set; } = FontFamily.Helvetica;

        /// <summary>
        /// The default font family to use for the heading of the document. Default is <see cref="FontFamily.Helvetica"/>.
        /// </summary>
        public FontFamily HeadingFontFamily { get; set; } = FontFamily.Helvetica;

        /// <summary>
        /// The default font family to use for the footer of the document. Default is <see cref="FontFamily.Helvetica"/>.
        /// </summary>
        public FontFamily FooterFontFamily { get; set; } = FontFamily.Helvetica;

        /// <summary>
        /// The default font size for the body of the document.
        /// Default is 10
        /// </summary>
        public double BodyFontSize { get; set; } = 10;

        /// <summary>
        /// The default font size for the heading of the document.
        /// Default is 10
        /// </summary>
        public double HeadingFontSize { get; set; } = 10;

        /// <summary>
        /// The default font size for the footer of the document.
        /// Default is 10
        /// </summary>
        public double FooterFontSize { get; set; } = 10;

        /// <summary>
        /// The document type to generate using these settings. 
        /// Default is PDF.
        /// </summary>
        public DocumentType DocumentType { get; set; } = DocumentType.Pdf;

        public DocumentSettings(DocumentType documentType = DocumentType.Pdf, Orientation orientation = Orientation.Portrait)
        {
            DocumentType = documentType;
            Orientation = orientation;
        }
    }
}
