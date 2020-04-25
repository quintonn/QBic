using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator.Settings
{
    public class FontFamily
    {
        internal string FontName { get; set; }
        public FontFamily(string name)
        {
            FontName = name;
        }

        public static readonly FontFamily Arial = new FontFamily("Arial");
        public static readonly FontFamily Helvetica = new FontFamily("Helvetica");
        public static readonly FontFamily Calibri = new FontFamily("Calibri");
        public static readonly FontFamily TimesNewRoman = new FontFamily("Times New Roman");
        public static readonly FontFamily CourierNew = new FontFamily("Courier New");

        public static FontFamily FromString(string fontName)
        {
            return new FontFamily(fontName);
        }

        public static implicit operator string(FontFamily fontFamily)
        {
            return fontFamily.FontName;
        }
    }
}
