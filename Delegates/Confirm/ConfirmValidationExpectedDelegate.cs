﻿using System.IO;
using System.Linq;

using Interfaces.Delegates.Confirm;

namespace Delegates.Confirm
{
    public class ConfirmValidationExpectedDelegate : IConfirmDelegate<string>
    {
        readonly string[] extensionsWhitelist = {
            ".exe", // Windows
            ".bin", // Windows
            ".dmg", // Mac
            ".pkg", // Mac
            ".sh" // Linux
        };

        public bool Confirm(string uri)
        {
            var extension = Path.GetExtension(uri);
            return extensionsWhitelist.Contains(extension);
        }
    }
}
