// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#if UNITY_4_7
using SerializableLong = Better.BuildInfo.UnionLong;
#else
using SerializableLong = System.Int64;
#endif

namespace Better.BuildInfo
{
#if UNITY_4_7
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    [System.Reflection.Obfuscation(Exclude=true)]
    public struct UnionLong : IXmlSerializable, IComparable<UnionLong>
    {
        [FieldOffset(0)]
        public int part0;
        [FieldOffset(4)]
        public int part1;

        [FieldOffset(0)]
        [System.NonSerialized]
        public long value;

        public int CompareTo(SerializableLong other)
        {
            var result = part0 - other.part0;
            if (result != 0)
                return result;
            return part1 - other.part1;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var str = reader.ReadString();
            value = long.Parse(str);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(value.ToString());
        }

        public static implicit operator long(UnionLong v)
        {
            return v.value;
        }

        public static implicit operator UnionLong(long v)
        {
            var result = new UnionLong();
            result.value = v;
            return result;
        }

    }
#endif

    [Serializable]
    [XmlRoot("buildInfo")]
    public class BuildInfo
    {
        //public static string BuildPlayer(string [] levels, string locationPathName, BuildTarget target, BuildOptions options)
        //{

        //}

        public SerializableLong dateUTC;
        public SerializableLong compressedSize;
        public SerializableLong totalSize;
        public SerializableLong streamingAssetsSize;
        public SerializableLong runtimeSize;
        public SerializableLong compressedRuntimeSize;

        public string buildTarget;
        public string projectPath;
        public string outputPath;
        public string unityVersion;

        public float buildTime;
        public float overheadTime;

        [XmlArrayItem("setting")]
        public List<BuildSetting> buildSettings = new List<BuildSetting>();
        [XmlArrayItem("environmentVariable")]
        public List<BuildSetting> environmentVariables = new List<BuildSetting>();

        [XmlArrayItem("asset", Type = typeof(AssetInfo))]
        public List<AssetInfo> assets = new List<AssetInfo>();
        
        [XmlArrayItem("scene")]
        public List<string> scenes = new List<string>();
        [XmlArrayItem("assetBundles")]
        public List<AssetBundleInfo> assetBundles = new List<AssetBundleInfo>();
    }


    [Serializable]
    [System.Reflection.Obfuscation(Exclude = true)]
    public class AssetBundleInfo
    {
        [XmlElement]
        public string path;
        [XmlAttribute]
        public long size;
    }

    [Serializable]
    [System.Reflection.Obfuscation(Exclude = true)]
    public class BasicAssetInfo
    {
        [XmlAttribute]
        public string path;

#if UNITY_4_7
        [XmlIgnore]
        [UnityEngine.SerializeField]
        private SerializableLong sizeData;
        [XmlAttribute]
        public long size
        {
            get { return sizeData; }
            set { sizeData = value; }
        }
#else
        [XmlAttribute]
        public long size;
#endif

        public override string ToString()
        {
            return "(" + path + ", " + size + ")";
        }
    }

 

    [Serializable]
    [System.Reflection.Obfuscation(Exclude = true)]
    public class AssetInfo : BasicAssetInfo
    {
        [XmlElement("scene")]
        public List<string> scenes = new List<string>();

        [XmlElement("dependency")]
        public List<string> dependencies = new List<string>();

        [XmlAttribute]
        public string spritePackerTag;

        [XmlIgnore]
        public int spritePackerPage;

        [XmlAttribute("spritePackerPage")]
        public string spritePackerPage_XmlProxy
        {
            get { return string.IsNullOrEmpty(spritePackerTag) ? null : spritePackerPage.ToString(); }
            set { spritePackerPage = int.Parse(value);  }
        }

        [XmlElement("info")]
        public AssetProperty[] details;

        [XmlElement("assetBundle")]
        public List<AssetAssetBundleEntry> assetBundles;

        [XmlIgnore]
        public bool assetBundleOnly;

        [XmlAttribute("assetBundleOnly")]
        public string assetBundleOnly_XmlProxy
        {
            get { return assetBundleOnly ? "true" : null; }
            set { assetBundleOnly = bool.Parse(value); }
        }
    }

    [Serializable]
    [System.Reflection.Obfuscation(Exclude = true)]
    public class AssetAssetBundleEntry
    {
        [XmlAttribute]
        public long size;
        [XmlElement]
        public string name;
        [XmlAttribute]
        public bool implicitCopy;
    }

    [Serializable]
    [System.Reflection.Obfuscation(Exclude = true)]
    public class BuildSetting
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string value;

        public override string ToString()
        {
            return "(" + name + ", " + value + ")";
        }
    }

    [Serializable]
    [System.Reflection.Obfuscation(Exclude=true)]
    public class AssetProperty
    {
        public static AssetProperty Create<T>(string name, T value)
        {
            return new AssetProperty()
            {
                name = name,
                value = value.ToString()
            };
        }

        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string value;

        public override string ToString()
        {
            return name + ": " + value;
        }
    }
}
