using System;

namespace WebsiteTemplate.Data.BaseTypes
{
    /// <summary>
    /// This is pretty much a string (or that's the idea at least).
    /// But the only reason for this is to be able to indicate that we want a DB field to be NVarchar(max) or equivalent.
    /// </summary>
    public class LongString
    {
        private string Base { get; set; }

        public LongString()
            : this(String.Empty)
        {

        }

        public LongString(string value)
        {
            Base = value;
        }

        public static implicit operator string(LongString ls)
        {
            return ls.Base;
        }

        public static implicit operator LongString(string ls)
        {
            return new LongString(ls);
        }

        public override string ToString()
        {
            return Base;
        }
    }
}