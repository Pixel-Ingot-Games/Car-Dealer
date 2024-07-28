using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal class CompressedSizeWorker
    {
        private static readonly int WorkersCount = Mathf.Max(1, Environment.ProcessorCount - 1);
        private Semaphore m_workersLimit = new Semaphore(WorkersCount, WorkersCount);
        private ManualResetEvent m_done = new ManualResetEvent(true);
        private int m_runningTasks = 0;

        public bool BeginWaitIfBusy(UnityEngine.Object asset, Action<long> onCompleted)
        {
            if (asset is Texture2D)
            {
                if (!UnityVersionAgnostic.IsGetRawTextureDataSupported)
                    return false;

                var textureData = UnityVersionAgnostic.GetRawTextureData((Texture2D)asset);
                if (textureData != null && textureData.Length > 0)
                {
                    return RunInBackground(() =>
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                            {
                                ds.Write(textureData, 0, textureData.Length);
                            }

                            onCompleted(ms.Length);
                        }
                    });
                }
            }

            return false;
        }

        public bool Join(int timeoutMS)
        {
            return m_done.WaitOne(timeoutMS);
        }

        private void DecrementRunningTasks()
        {
            if (Interlocked.Decrement(ref m_runningTasks) == 0)
            {
                m_done.Set();
            }

            m_workersLimit.Release();
        }

        private bool RunInBackground(System.Action action)
        {
            m_workersLimit.WaitOne();

            Interlocked.Increment(ref m_runningTasks);
            m_done.Reset();

            try
            {
                WaitCallback work = (state) =>
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        DecrementRunningTasks();
                    }
                };

                if (ThreadPool.QueueUserWorkItem(work))
                {
                    return true;
                }
                else
                {
                    DecrementRunningTasks();
                    return false;
                }
            }
            catch (System.NotSupportedException)
            {
                DecrementRunningTasks();
                return false;
            }
        }
    }
}
