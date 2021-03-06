﻿using System;
using System.IO;

using Interfaces.Controllers.File;

namespace Controllers.File
{
    public class FileController : IFileController
    {
        public bool Exists(string uri)
        {
            return System.IO.File.Exists(uri);
        }

        public void Move(string fromUri, string toUri)
        {
            if (!Exists(fromUri)) return;

            var destinationDirectory = Path.GetDirectoryName(toUri);
            if (!string.IsNullOrEmpty(destinationDirectory) &&
                !System.IO.Directory.Exists(destinationDirectory))
                System.IO.Directory.CreateDirectory(destinationDirectory);

            if (Exists(toUri))
                System.IO.File.Delete(toUri);

            System.IO.File.Move(fromUri, toUri);
        }

        public long GetSize(string uri)
        {
            var fileInfo = new FileInfo(uri);
            return fileInfo.Length;
        }

        public DateTime GetTimestamp(string uri)
        {
            var fileInfo = new FileInfo(uri);
            return fileInfo.CreationTimeUtc;
        }
    }
}
