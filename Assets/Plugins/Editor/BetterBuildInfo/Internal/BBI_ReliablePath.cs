// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
    public static class ReliablePath
    {
        public static string EnsureForwardSlash(this string str)
        {
            return str.Replace('\\', '/');
        }

        public static string GetFullPath(string path)
        {
            return EnsureForwardSlash(System.IO.Path.GetFullPath(path));
        }

        public static string Combine(string path1, string path2)
        {
            return EnsureForwardSlash(Path.Combine(path1, path2));
        }

        public static string GetDirectoryName(string path)
        {
            return EnsureForwardSlash(Path.GetDirectoryName(path));
        }

        internal static string GetRandomFileName()
        {
            return EnsureForwardSlash(Path.GetRandomFileName());
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string GetTempPath()
        {
            return EnsureForwardSlash(Path.GetTempPath());
        }

        public static string GetTempFileName()
        {
            return EnsureForwardSlash(Path.GetTempFileName());
        }

        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        internal static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}
