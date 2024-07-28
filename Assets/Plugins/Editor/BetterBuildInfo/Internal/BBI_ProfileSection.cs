// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
#if BETTERBUILDINFO_DEBUG
    internal sealed class ProfileSection: IDisposable
    {
        public static IDisposable Create(string name)
        {
            return new ProfileSection(name);
        }

        private readonly System.Diagnostics.Stopwatch m_stopwatch;
        private readonly string m_name;

        public ProfileSection(string name)
        {
            m_name = name;
            m_stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Dispose()
        {
            m_stopwatch.Stop();
            UnityEngine.Debug.Log("BetterBuildInfo.Profile: " + m_name + " " + m_stopwatch.Elapsed);
        }
    }
#else
    internal sealed class ProfileSection
    {
        public static IDisposable Create(string name)
        {
            return null;
        }
    }
#endif
}
