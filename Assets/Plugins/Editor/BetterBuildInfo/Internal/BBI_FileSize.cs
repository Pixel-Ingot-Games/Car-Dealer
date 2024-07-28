using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Better.BuildInfo.Internal
{
    [Serializable]
    [System.Reflection.Obfuscation(Exclude = true)]
    public struct FileSize : IComparable<FileSize>, IEquatable<FileSize>
    {
        public static readonly FileSize Zero = new FileSize() { valueKB = 0 };

        private float valueKB;

        public static FileSize FromBytes(long bytes)
        {
            return new FileSize() { valueKB = bytes / 1024.0f };
        }

        public static FileSize FromBytes(float bytes)
        {
            return new FileSize() { valueKB = bytes / 1024.0f };
        }

        public static FileSize FromKiloBytes(float kiloBytes)
        {
            return new FileSize() { valueKB = kiloBytes };
        }

        public static implicit operator FileSize(long value)
        {
            return FileSize.FromBytes(value);
        }

        public static FileSize operator +(FileSize a, FileSize b)
        {
            return new FileSize() { valueKB = a.valueKB + b.valueKB };
        }

        public static FileSize operator -(FileSize a, FileSize b)
        {
            return new FileSize() { valueKB = a.valueKB - b.valueKB };
        }

        public static bool operator >(FileSize a, FileSize b)
        {
            return a.valueKB > b.valueKB;
        }

        public static bool operator <(FileSize a, FileSize b)
        {
            return a.valueKB < b.valueKB;
        }

        public static bool operator ==(FileSize a, FileSize b)
        {
            return a.valueKB == b.valueKB;
        }

        public static bool operator !=(FileSize a, FileSize b)
        {
            return a.valueKB != b.valueKB;
        }

        public static FileSize operator -(FileSize a)
        {
            return FileSize.FromKiloBytes(-a.valueKB);
        }

        public long Bytes
        {
            get { return (long)(valueKB * 1024); }
        }

        public float KiloBytes
        {
            get { return valueKB; }
        }

        public FileSize Abs()
        {
            return FileSize.FromKiloBytes(UnityEngine.Mathf.Abs(valueKB));
        }

        public override string ToString()
        {
            string[] units = new[] { "kB", "MB", "GB", "TB" };

            var num = valueKB;
            int unitIndex = 0;

            for (; num >= 1024.0f && unitIndex < units.Length - 1; ++unitIndex)
            {
                num /= 1024.0f;
            }

            return string.Format("{0:0.00} {1}", num, units[unitIndex]);
        }

        public bool Equals(FileSize other)
        {
            return valueKB == other.valueKB;
        }

        public override bool Equals(object obj)
        {
            return (obj is FileSize) && Equals((FileSize)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(FileSize size)
        {
            return valueKB.CompareTo(size.valueKB);
        }
    }

    public static class FileSizeEnumerableExtensions
    {
        public static FileSize SumFileSize<TSource>(this IEnumerable<TSource> source, Func<TSource, FileSize> selector)
        {
            FileSize result = FileSize.FromKiloBytes(0);
            foreach (var x in source)
                result += selector(x);
            return result;
        }
    }
}
