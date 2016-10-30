using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZUpdater
{
    public static class Parsing
    {
        public static Uri AbsUri(string baseUrl, string relUrl)
        {
            return new Uri(new Uri(baseUrl), relUrl);
        }

        /// <summary>
        /// Gets substring of the text - marked by two strings
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="m1">opening marker</param>
        /// <param name="m2">closing marker</param>
        /// <returns></returns>
        public static string GetBetween(string text, string m1, string m2)
        {
            int oIndex = text.IndexOf(m1) + m1.Length;
            return text.Substring(oIndex, ((text.IndexOf(m2, oIndex) - oIndex) > 0 ? (text.IndexOf(m2, oIndex) - oIndex) : text.Length - oIndex));
        }

        public static string Format(string input)
        {
            return System.Net.WebUtility.HtmlDecode(input);
        }

        /// <summary>
        /// Gets substring of the text - marked by two strings
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="m1">opening marker</param>
        /// <param name="m2">closing marker</param>
        /// <returns></returns>
        public static List<string> GetBetweens(string text, string m1, string m2)
        {
            List<string> ret = new List<string>();
            bool getting = true;
            int offset = 0;
            while (getting)
            {
                int oIndex = text.IndexOf(m1, offset) + m1.Length;
                string s = text.Substring(oIndex, ((text.IndexOf(m2, oIndex) - oIndex) > 0 ? (text.IndexOf(m2, oIndex) - oIndex) : text.Length - oIndex));
                ret.Add(s);

                offset = oIndex + s.Length + m2.Length;
                try
                {
                    getting = text.IndexOf(m1, offset) > 0;
                }
                catch (ArgumentOutOfRangeException)
                {
                    getting = false;
                }
            }
            return ret;
        }

        /// <summary>
        /// Section selection
        /// </summary>
        /// <param name="tag1">Opening marker. E.g: "&lt;a"</param>
        /// <param name="tag2">Closing marker. E.g: "&lt;/a"</param>
        /// <param name="contain">text which must be within the two markers. Method will keep searching until a section with this text is found</param>
        /// <returns></returns>
        public static string GetSection(string text, string tag1, string tag2, params string[] contains)
        {
            bool found = false;
            int offset = 0;
            string section = null;
            int no = 0;
            while (!found)
            {
                int aIndex = text.IndexOf(tag1, offset);
                if (aIndex < 0)
                {
                    return null;
                }
                try
                {
                    section = text.Substring(aIndex, text.IndexOf(tag2, aIndex) - aIndex);

                    bool containsAll = true;

                    foreach (string contain in contains)
                    {
                        if (!section.Contains(contain))
                        {
                            containsAll = false;
                        }
                    }

                    if (containsAll)
                    {
                        found = true;
                    }
                    else
                    {
                        offset = aIndex + tag1.Length;
                    }
                }
                catch
                {
                    return null;
                }

                if (no > text.Length)
                {
                    return null; //could not find section in the page
                }

                no++;
            }
            return section;
        }

        /// <summary>
        /// Sections selection
        /// </summary>
        /// <param name="tag1">Opening marker. E.g: "&lt;a"</param>
        /// <param name="tag2">Closing marker. E.g: "&lt;/a"</param>
        /// <param name="contain">text which must be within the two markers. Method will keep searching until a section with this text is found</param>
        /// <returns></returns>
        public static List<string> GetSections(string text, string tag1, string tag2, params string[] contains)
        {
            List<string> ret = new List<string>();
            int overOffset = 0;
            bool done = false;
            while (!done)
            {
                bool found = false;
                int offset = overOffset;
                string section = null;
                int no = 0;
                while (!found)
                {
                    int aIndex = text.IndexOf(tag1, offset);
                    if (aIndex < 0)
                    {
                        done = true;
                        break;
                    }
                    try
                    {
                        section = text.Substring(aIndex, text.IndexOf(tag2, aIndex) - aIndex);

                        bool containsAll = true;

                        foreach (string contain in contains)
                        {
                            if (!section.Contains(contain))
                            {
                                containsAll = false;
                            }
                        }

                        if (containsAll)
                        {
                            found = true;
                            overOffset = aIndex + section.Length + tag2.Length;
                        }
                        else
                        {
                            offset = aIndex + tag1.Length;
                        }
                    }
                    catch
                    {
                        done = true;
                        break;
                    }

                    if (no > text.Length)
                    {
                        done = true;
                        break; //could not find section in the page
                    }

                    no++;
                }
                ret.Add(section);
            }

            return ret;
        }
    }
}
