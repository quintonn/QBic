using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Backend.Processing.InputProcessing
{
    public class ProcessingResult
    {
        public string Message { get; set; }

        public bool Success { get; set; }

        public ProcessingResult(bool success, string message = null)
        {
            Success = success;
            Message = message;
        }
    }
}