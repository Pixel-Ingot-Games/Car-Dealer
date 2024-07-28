// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
    internal static class VersionControlUtils
    {
        public static string GetSVNRevision()
        {
            return GetStdOut("svn", "info --show-item revision");
        }

        public static string GetGitCommitHash()
        {
            return GetStdOut("git", "rev-parse HEAD");
        }

        public static string GetGitShortCommitHash()
        {
            return GetStdOut("git", "rev-parse --short HEAD");
        }

        private static string GetStdOut(string program, string arguments)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var stdoutData = proc.StandardOutput.ReadToEnd();
            var stderrData = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                if (string.IsNullOrEmpty(stdoutData))
                    stdoutData = stderrData;
                else if (!string.IsNullOrEmpty(stderrData))
                    stdoutData += "\n" + stderrData;
                throw new System.InvalidOperationException($"Command \"{program} {arguments}\" returned an error: {proc.ExitCode}, details:\n{stdoutData}");
            }

            return stdoutData.Trim();
        }
    }
}
