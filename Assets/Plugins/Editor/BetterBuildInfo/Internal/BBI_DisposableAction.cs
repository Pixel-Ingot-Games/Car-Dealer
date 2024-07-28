// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
    internal class DisposableAction: IDisposable
    {
        private Action m_disposal;

        public DisposableAction(Action disposal)
        {
            m_disposal = disposal;
        }

        public static DisposableAction Create(Action disposal)
        {
            return new DisposableAction(disposal);
        }

        public static DisposableAction<T> Create<T>(Action<T> disposal, T arg)
        {
            return new DisposableAction<T>(disposal, arg);
        }

        public void Dispose()
        {
            m_disposal();
        }

        public static explicit operator DisposableAction(Action disposal)
        {
            return Create(disposal);
        }
    }

    internal sealed class DisposableAction<T>: IDisposable
    {
        private Action<T> m_disposal;
        private T m_arg;

        public DisposableAction(Action<T> disposal, T arg)
        {
            m_disposal = disposal;
            m_arg = arg;
        }

        public void Dispose()
        {
            m_disposal(m_arg);
        }
    }
}
