// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System.Reflection;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal class PreviewResizer
    {
        private readonly System.Type s_internalType;
        private readonly MethodInfo s_initMethod;
        private readonly MethodInfo s_resizeHandleMethod;

        private object m_actualResizer;

        public PreviewResizer()
        {
            s_internalType = typeof(UnityEditor.EditorGUI).Assembly.GetType("UnityEditor.PreviewResizer");
            if (s_internalType == null)
            {
                throw new System.InvalidOperationException("Unable to find UnityEditor.PreviewResizer type. Have you updated Unity recently?");
            }

            s_initMethod = s_internalType.GetMethod("Init", new[] { typeof(string) });
            if (s_initMethod == null)
            {
                throw new System.InvalidOperationException("Unable to find UnityEditor.PreviewResizer.Init method. Have you updated Unity recently?");
            }

            s_resizeHandleMethod = s_internalType.GetMethod("ResizeHandle", new[] { typeof(Rect), typeof(float), typeof(float), typeof(float) });
            if (s_resizeHandleMethod == null)
            {
                throw new System.InvalidOperationException("Unable to find UnityEditor.PreviewResizer.ResizeHandle method. Have you updated Unity recently?");
            }

            m_actualResizer = System.Activator.CreateInstance(s_internalType);
        }

        public void Init(string prefName)
        {
            s_initMethod.Invoke(m_actualResizer, new object[] { prefName });
        }

        public float ResizeHandle(Rect windowPosition, float minSize, float minRemainingSize, float resizerHeight)
        {
            return (float)s_resizeHandleMethod.Invoke(m_actualResizer, new object[] { windowPosition, minSize, minRemainingSize, resizerHeight });
        }
    }

}