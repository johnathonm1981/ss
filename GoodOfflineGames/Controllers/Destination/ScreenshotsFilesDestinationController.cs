﻿using Controllers.Destination.Abstract;

namespace Controllers.Destination
{
    public class ScreenshotsFilesDestinationController : ImagesScreenshotsDestinationController
    {
        public override string GetDirectory(string source)
        {
            return "screenshots";
        }

        public override string GetFilename(string source)
        {
            return base.GetFilename(source);
        }
    }
}
