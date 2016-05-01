using System;
using System.Text;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public abstract class OpenFile : Event
    {
        private string Data { get; set; }

        internal void SetData(string data)
        {
            Data = data;
        }

        private byte[] FileData
        {
            get
            {
                return GetFileData(Data);
            }
        }

        private FileType FileType
        {
            get
            {
                return GetFileType(Data);
            }
        }

        public string FileName
        {
            get
            {
                return GetFileName(Data);
            }
        }

        public abstract string GetFileName(string data);
        public abstract byte[] GetFileData(string data);
        public abstract FileType GetFileType(string data);

        public string DataUrl
        {
            get
            {
                return CreateDataURL();
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.ViewFile;
            }
        }

        private string CreateDataURL()
        {
            var dataURL = String.Empty;
            switch (FileType)
            {
                case FileType.Pdf:
                    var bytes = Encoding.UTF8.GetString(FileData);
                    dataURL = "data:application/pdf;base64," + bytes;
                    break;
                default:
                    throw new NotImplementedException("Unkown file type: " + FileType);
            }
            return dataURL;
        }
    }
}