using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Menus
{
    public abstract class OpenFile : Event
    {
        private string Data { get; set; }

        internal void SetData(string data)
        {
            Data = data;
        }

        public string DataUrl
        {
            get
            {
                return "/GetFile/" + GetEventId();
            }
        }

        public string RequestData
        {
            get
            {
                return Data;
            }
        }

        private string _fileName { get; set; }

        public string FileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fileName))
                {
                    _fileName = GetFileNameAndExtension();
                }
                return _fileName;
            }
        }


        public abstract string GetFileNameAndExtension();

        public abstract Task<FileInfo> GetFileInfo(string data);

        public override EventType ActionType
        {
            get
            {
                return EventType.ViewFile;
            }
        }
    }
}