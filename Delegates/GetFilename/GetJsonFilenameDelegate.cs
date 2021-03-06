﻿using Interfaces.Delegates.GetFilename;

namespace Delegates.GetFilename
{
    public class GetJsonFilenameDelegate : IGetFilenameDelegate
    {
        const string extension = ".json";

        public string GetFilename(string source = null)
        {
            return source + extension;
        }
    }
}
