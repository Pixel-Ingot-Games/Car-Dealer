// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{

    [Serializable]
    internal class BitArray
    {
        private bool[] m_flags;

        public BitArray(int length, bool value)
        {
            m_flags = new bool[length];
            SetAll(value);
        }

        public BitArray()
        {
            m_flags = new bool[0];
        }

        public bool this[int index]
        {
            get { return m_flags[index]; }
            set { m_flags[index] = value; }
        }

        public int Length
        {
            get { return m_flags.Length; }
        }

        public bool AndAny(BitArray other)
        {
            if ( other.Length != Length )
                throw new ArgumentException();

            for (int i = 0; i < Length; ++i)
            {
                if ( this[i] & other[i] )
                    return true;
            }

            return false;
        }

        internal bool AndAnyNeg(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException();

            for (int i = 0; i < Length; ++i)
            {
                if (this[i] & !(other[i]))
                    return true;
            }

            return false;
        }

        public bool AllEqual(BitArray other)
        {
            return m_flags.SequenceEqual(other.m_flags);
        }

        public bool All(bool value = true)
        {
            for ( int i = 0; i < m_flags.Length; ++i)
            {
                if ( m_flags[i] != value )
                    return false;
            }
            return true;
        }

        public bool Any(bool value = true)
        {
            for ( int i = 0; i < m_flags.Length; ++i )
            {
                if ( m_flags[i] == value )
                    return true;
            }
            return false;
        }

        public void SetAll(bool value)
        {
            for ( int i = 0; i < m_flags.Length; ++i )
            {
                m_flags[i] = value;
            }
        }

        internal int CountSet()
        {
            return m_flags.Count(x => x);
        }
    }
}
