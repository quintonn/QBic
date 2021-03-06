﻿using System;

namespace WebsiteTemplate.Menus.InputItems
{
    public class InitializeResult
    {
        public bool Success { get; set; }

        public string Error { get; set; }

        public InitializeResult(bool success, string error = null)
        {
            Success = success;
            Error = error ?? String.Empty;
        }
    }
}