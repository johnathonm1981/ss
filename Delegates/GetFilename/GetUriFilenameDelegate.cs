﻿using System;
using System.IO;

using Interfaces.Delegates.GetFilename;

using Models.Separators;

namespace Delegates.GetFilename
{
    public class GetUriFilenameDelegate : IGetFilenameDelegate
    {
        public virtual string GetFilename(string source = null)
        {
            if (string.IsNullOrEmpty(source))
                return source;

            var filename = Path.GetFileName(source);

            if (!filename.Contains(Separators.UriPart) &&
                !filename.Contains(Separators.QueryString))
                return filename;

            var uriParts = filename.Split(
                new string[] { Separators.UriPart },
                StringSplitOptions.RemoveEmptyEntries);

            // filename is the last uri part
            var filenameWithQueryString = uriParts[uriParts.Length - 1];

            if (!filenameWithQueryString.Contains(Separators.QueryString))
                return filenameWithQueryString;

            var filenameParts = filenameWithQueryString.Split(
                new string[] { Separators.QueryString },
                StringSplitOptions.RemoveEmptyEntries);

            // filename without querystring is the first uri part
            return filenameParts[0];
        }
    }
}
