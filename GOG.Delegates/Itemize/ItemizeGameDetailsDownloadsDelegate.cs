﻿using System.Collections.Generic;

using Interfaces.Delegates.Itemize;

using Models.Separators;

namespace GOG.Delegates.Itemize
{
    public class ItemizeGameDetailsDownloadsDelegate : IItemizeDelegate<string, string>
    {
        public IEnumerable<string> Itemize(string data)
        {
            // downloads are double array and so far nothing else in the game details data is
            // so we'll leverage this fact to extract actual content

            string result = string.Empty;

            int fromIndex = data.IndexOf(Separators.GameDetailsDownloadsStart, System.StringComparison.Ordinal),
                toIndex = data.IndexOf(Separators.GameDetailsDownloadsEnd, System.StringComparison.Ordinal);

            if (fromIndex < toIndex)
                result = data.Substring(
                    fromIndex,
                    toIndex - fromIndex + Separators.GameDetailsDownloadsEnd.Length);

            return new string[] { result };
        }
    }
}
