using System;
using System.Linq;

namespace Pathoschild.LookupAnything.Common
{
    public class Sift3
    {
        // @yasinkuyu
        // 27/04/2014
        // from https://gist.github.com/yasinkuyu/11350516
        public static float Compare(string s1, string s2, int maxOffset)
        {

            if (string.IsNullOrEmpty(s1))
                return ((string.IsNullOrEmpty(s2)) ? 0 : s2.Length);

            if (string.IsNullOrEmpty(s2))
                return s1.Length;

            int c1 = 0, c2 = 0, lcs = 0;

            while ((c1 < s1.Length) && (c2 < s2.Length))
            {
                if (s1[c1] == s2[c2])
                {
                    lcs++;
                }
                else
                {
                    for (int i = 1; i < maxOffset; i++)
                    {

                        if ((c1 + i < s1.Length) && (s1[c1 + i] == s2[c2]))
                        {
                            c1 += i;
                            break;
                        }

                        if ((c2 + i < s2.Length) && (s1[c1] == s2[c2 + i]))
                        {
                            c2 += i;
                            break;
                        }
                    }
                }
                c1++;
                c2++;
            }
            return ((s1.Length + s2.Length) / 2 - lcs);
        }
    }
}
