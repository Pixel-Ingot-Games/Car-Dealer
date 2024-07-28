// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
    public static class Utils
    {
        public static bool TryParseInvariant(string str, out double result)
        {
            return double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture.NumberFormat, out result)
                || double.TryParse(str, out result);
        }

        public static bool NearlyEqual(float a, float b, float epsilon = 0.0001f)
        {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);
            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < float.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < epsilon;
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        public static string PoorMansRelativePath(string path)
        {
            // to unity and unix-like
            var cd = Environment.CurrentDirectory.Replace('\\', '/');
            path = path.Replace('\\', '/');

            if (path.StartsWith(cd))
            {
                return path.Substring(cd.TrimEnd('/').Length + 1);
            }
            else
            {
                return path;
            }
        }

        public static void InsertionSort<T>(List<T> list, Comparison<T> comparison)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            int count = list.Count;
            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }

        public static void SafeDispose(IDisposable disposable)
        {
            if ( disposable == null )
                return;

            try
            {
                disposable.Dispose();
            }
            catch ( System.Exception ) { }

            disposable = null;
        }
    }
}
