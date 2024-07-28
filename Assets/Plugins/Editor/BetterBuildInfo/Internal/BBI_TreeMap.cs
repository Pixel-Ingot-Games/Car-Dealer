// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal static class TreeMap
    {
        private class WorkItem
        {
            public float x;
            public float y;
            public float width;
            public float height;
            public int index;
            public float area;
        }

        public static Rect[] CalcMap(IEnumerable<float> input, Rect targetViewport)
        {
            // prepare the workset
            var workset = input.Select((xx, i) => new WorkItem()
            {
                area = xx,
                index = i,
            }).ToList();

            var result = new Rect[workset.Count];

            {
                // remove empty blocks
                workset.RemoveAll(xx => xx.area <= 0);
                workset.Sort((xx, yy) => yy.area.CompareTo(xx.area));

                // normalize the size
                var totalArea = workset.Sum(x => x.area);
                var valueScale = targetViewport.width * targetViewport.height / totalArea;
                foreach ( var item in workset )
                {
                    item.area = valueScale * item.area;
                }
            }

            {
                Rect viewport = targetViewport;
                var vert = viewport.width > viewport.height;
                int start = 0;
                int end = 0;
                float totalArea = 0;
                float aspectCurr = float.MaxValue;

                for ( int i = 0; i < 1000000 && end != workset.Count; ++i )
                {
                    totalArea += workset[end].area;

                    var aspect = GetAspectRatio(workset[end].area, vert, totalArea, viewport.width, viewport.height);

                    if ( ( aspect > aspectCurr ) || ( aspect < 1 ) )
                    {
                        UpdateSizes(workset, start, end, vert, viewport);
                        if ( vert )
                        {
                            viewport.x += workset[start].width;
                            viewport.width -= workset[start].width;
                        }
                        else
                        {
                            viewport.y += workset[start].height;
                            viewport.height -= workset[start].height;
                        }

                        vert = viewport.width > viewport.height;

                        start = end;
                        end = start;

                        aspectCurr = float.MaxValue;
                        totalArea = 0;

                    }
                    else
                    {
                        aspectCurr = aspect;
                        end++;
                    }
                }

                if ( end != workset.Count )
                {
                    throw new InvalidOperationException("Possibly infinite loop detected");
                }

                UpdateSizes(workset, start, end, vert, viewport);
            }

            foreach ( var work in workset )
            {
                result[work.index] = new Rect(
                    work.x,
                    work.y,
                    work.width,
                    work.height
                );
            }

            return result;
        }

        private static void UpdateSizes(List<WorkItem> workset, int start, int end, bool vert, Rect viewport)
        {
            float total = 0;
            for (int i = start; i < end; i++)
            {
                total += workset[i].area;
            }

            float currX = 0;
            float currY = 0;

            if (vert)
            {
                var localWidth = total / viewport.height;
                var localHeight = viewport.height;
                var mult = localHeight / total;

                for (int i = start; i < end; i++)
                {
                    workset[i].x = viewport.x;
                    workset[i].y = viewport.y + currY;

                    workset[i].width = localWidth;
                    workset[i].height = workset[i].area * mult;

                    currY += workset[i].height;
                }
            }
            else
            {
                var localHeight = total / viewport.width;
                var localWidth = viewport.width;
                var mult = localWidth / total;

                for (int i = start; i < end; i++)
                {
                    workset[i].x = viewport.x + currX;
                    workset[i].y = viewport.y;

                    workset[i].width = workset[i].area * mult;
                    workset[i].height = localHeight;

                    currX += workset[i].width;
                }
            }
        }

        private static float GetAspectRatio(float area, bool vert, float totalArea, float viewportWidth, float viewportHeight)
        {
            float aspect = 0;
            float w;
            float h;

            if ( vert )
            {
                h = viewportHeight * ( area / totalArea );
                w = totalArea / viewportHeight;
            }
            else
            {
                h = totalArea / viewportWidth;
                w = viewportWidth * ( area / totalArea );
            }

            aspect = h / w;
            if (aspect < 1)
            {
                aspect = w / h;
            }

            return aspect;
        }
    }
}
