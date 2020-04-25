using DocumentGenerator.Settings;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator.Styles
{
    /// <summary>
    /// This class provides a base class for configuring document styles for the application.
    /// Inherit this class and implement its methods to override defaults.
    /// To use this class it needs to be registered with IoC.
    /// </summary>
    public class StyleSetup
    {
        /// <summary>
        /// This method gets called for all <see cref="StyleNames"/> values.
        /// Override this method to change the default styles that are used.
        /// </summary>
        /// <param name="styleName"></param>
        /// <param name="style"></param>
        public virtual void SetStyle(string styleName, Style style)
        {
            if (styleName == StyleNames.Normal)
            {
                style.Font.Name = FontFamily.Helvetica;
                style.Font.Size = 10;
                style.Font.Bold = false;
                style.Font.Color = Colors.Black;
            }
            else if (styleName == StyleNames.Heading1)
            {
                style.Font.Name = FontFamily.Helvetica;
                style.Font.Size = 18;
                style.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                style.Font.Bold = true;
                style.Font.Color = Colors.Black;
            }
            else if (styleName == StyleNames.Footer)
            {
                style.Font.Name = "Helvetica";
                style.Font.Size = 8;
                style.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            }
            else
            {
                throw new Exception("Unknown stylename: " + styleName);
            }
        }
    }
}
