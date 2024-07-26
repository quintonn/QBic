using System;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class BaseViewEvent : Event
    {
        internal BaseViewEvent() // makes it so only internal project can use it, potentially
        {
            
        }
        /// <summary>
        /// Returns the title that will be displayed when this view is rendered.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return Description;
            }
        }

        protected string GetParameter(string parameterName, string alternateName, GetDataSettings settings, bool allowNull = true)
        {
            var item = GetParameter(parameterName, settings, true);
            if (String.IsNullOrWhiteSpace(item))
            {
                item = GetParameter(alternateName, settings, allowNull);
            }
            return item;
        }

        protected string GetParameter(string parameterName, GetDataSettings settings, bool allowNull = true)
        {
            var data = settings.ViewData;
            var json = JsonHelper.Parse(data);
            var tmpData = json.GetValue("data");
            var result = String.Empty;
            if (!String.IsNullOrWhiteSpace(tmpData))
            {
                var x = JsonHelper.Parse(tmpData);
                result = x.GetValue(parameterName);
                //if (String.IsNullOrWhiteSpace(result))
                //{
                //    var parameters = json.GetValue("parameters");
                //    if (!String.IsNullOrWhiteSpace(parameters))
                //    {
                //        var paramJson = JsonHelper.Parse(parameters);
                //        result = paramJson.GetValue(parameterName);
                //    }
                //    else if (!String.IsNullOrWhiteSpace(data))
                //    {
                //        result = tmpData;
                //    }
                //    //result = tmpData;
                //}
            }
            else
            {
                var eventParams = json.GetValue("eventParameters");
                if (!String.IsNullOrWhiteSpace(eventParams))
                {
                    var x = JsonHelper.Parse(eventParams);
                    result = x.GetValue(parameterName);
                }
                //else if (!String.IsNullOrWhiteSpace(data))
                //{
                //    result = data;
                //}
            }

            if (String.IsNullOrWhiteSpace(result))
            {
                var parameters = json.GetValue("parameters");
                if (!String.IsNullOrWhiteSpace(parameters))
                {
                    var paramJson = JsonHelper.Parse(parameters);
                    result = paramJson.GetValue(parameterName);
                }
                else if (!String.IsNullOrWhiteSpace(tmpData))
                {
                    result = tmpData;
                }
                else
                {
                    // this is not right i don't think, if the data is just 1 value then the calling code should check for it i think.

                    //result = data; // I don't think this is working
                    // If I expect to have "ClientId" for example, and i open a edit screen and come back, 
                    // it does not parse the client id correctly
                }
                //result = tmpData;
            }

            if (!allowNull && String.IsNullOrWhiteSpace(result))
            {
                throw new Exception("Parameter " + parameterName + " should not be null");
            }

            return result;
        }
    }
}
