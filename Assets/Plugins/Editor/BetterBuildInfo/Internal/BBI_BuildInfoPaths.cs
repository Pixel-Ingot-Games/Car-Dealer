// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
    internal static class BuildInfoPaths
    {
        public const string Base = "Assets/Plugins/Editor/BetterBuildInfo";

        public const string LegacySettings = Base + "/BuildInfoSettings.asset";
        public const string Settings = Base + "/BetterBuildInfoSettings.asset";
        public const string SkinDirectory = Base + "/BuildInfoSkin";
    }
}
