// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal class ColorGenerator
    {
        private System.Random m_random;

        public ColorGenerator(int seed = 1062083343)
        {
            m_random = new System.Random(seed);
        }

        public Color GetNextPastelColor()
        {
            int r = m_random.Next(256) + 255 + 40;
            int g = m_random.Next(256) + 255 + 40;
            int b = m_random.Next(256) + 255 + 40;

            r = Mathf.Min(r / 2, 255);
            g = Mathf.Min(g / 2, 255);
            b = Mathf.Min(b / 2, 255);

            var result = new Color32((byte)r, (byte)g, (byte)b, 255);
            return result;
        }
    }
}
