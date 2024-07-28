// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Better.BuildInfo.Internal
{
    internal class WildcardTest
    {
        private enum Mode
        {
            IsEqual,
            MatchAll,
            StartsWith,
            EndsWith,
            Contains,
            Regex
        }

        private readonly Mode m_mode;
        private readonly object m_data;

        private WildcardTest(Mode mode, object data)
        {
            m_mode = mode;
            m_data = data;
        }

        public static WildcardTest Create(string pattern, bool fullMatch = true)
        {
            int wildcardIndex = pattern.IndexOf('*');

            if (wildcardIndex < 0)
            {
                return new WildcardTest(fullMatch ? Mode.IsEqual : Mode.Contains, pattern);
            }
            else if (wildcardIndex == 0)
            {
                if (pattern.Length == 1)
                {
                    return new WildcardTest(Mode.MatchAll, null);
                }
                else
                {
                    var otherWildcardIndex = pattern.IndexOf('*', 1);
                    if (otherWildcardIndex < 0)
                    {
                        return new WildcardTest(fullMatch ? Mode.EndsWith : Mode.Contains, pattern.Substring(1));
                    }
                    else if (otherWildcardIndex == pattern.Length - 1)
                    {
                        return new WildcardTest(Mode.Contains, pattern.Substring(1, pattern.Length - 2));
                    }
                }
            }
            else if (wildcardIndex == pattern.Length - 1)
            {
                return new WildcardTest(fullMatch ? Mode.StartsWith : Mode.Contains, pattern.Substring(0, pattern.Length - 1));
            }

            if (fullMatch)
            {
                return new WildcardTest(Mode.Regex, WildcardToRegex(pattern));
            }
            else
            {
                pattern = '*' + pattern.Trim('*') + '*';
                return new WildcardTest(Mode.Regex, WildcardToRegex(pattern));
            }
        }

        public bool IsMatch(string str)
        {
            if ( string.IsNullOrEmpty(str) )
            {
                if (m_mode == Mode.Regex)
                {
                    return ((Regex)m_data).IsMatch(string.Empty);
                }
                return false;
            }

            switch ( m_mode )
            {
                case Mode.MatchAll:
                    return true;

                case Mode.IsEqual:
                    return str.Equals((string)m_data, StringComparison.OrdinalIgnoreCase);

                case Mode.StartsWith:
                    return str.StartsWith((string)m_data, StringComparison.OrdinalIgnoreCase);

                case Mode.EndsWith:
                    return str.EndsWith((string)m_data, StringComparison.OrdinalIgnoreCase);

                case Mode.Contains:
                    return str.IndexOf((string)m_data, StringComparison.OrdinalIgnoreCase) >= 0;

                case Mode.Regex:
                    return ( (Regex)m_data ).IsMatch(str);
            }

            throw new System.InvalidOperationException();
        }

        private static Regex WildcardToRegex(string pattern)
        {
            var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return new Regex(regex, UnityVersionAgnostic.CompiledRegexOptions | RegexOptions.IgnoreCase);
        }
    }
}
