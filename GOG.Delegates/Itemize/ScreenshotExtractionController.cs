﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

using Interfaces.Delegates.Itemize;

namespace GOG.Delegates.Itemize
{
    public class ItemizeScreenshotsDelegate: IItemizeDelegate<string, string>
    {
        const string attributePrefix = "data-src=\"";
        readonly Regex regex = new Regex(attributePrefix + "\\S*\"");

        public IEnumerable<string> Itemize(string pageContent)
        {
            var match = regex.Match(pageContent);
            while (match.Success)
            {
                var screenshot = match.Value.Substring(attributePrefix.Length, // drop the prefix data-src="
                    match.Value.Length - attributePrefix.Length - 1); // and closing "

                yield return screenshot;
                match = match.NextMatch();
            }
        }
    }
}
