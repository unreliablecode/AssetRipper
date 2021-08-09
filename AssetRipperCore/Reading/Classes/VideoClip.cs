﻿using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Reading.Classes
{
	public sealed class VideoClip : NamedObject
    {
        public ResourceReader m_VideoData;
        public string m_OriginalPath;
        public StreamedResource m_ExternalResources;

        public VideoClip(ObjectReader reader) : base(reader)
        {
            m_OriginalPath = reader.ReadAlignedString();
            var m_ProxyWidth = reader.ReadUInt32();
            var m_ProxyHeight = reader.ReadUInt32();
            var Width = reader.ReadUInt32();
            var Height = reader.ReadUInt32();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                var m_PixelAspecRatioNum = reader.ReadUInt32();
                var m_PixelAspecRatioDen = reader.ReadUInt32();
            }
            var m_FrameRate = reader.ReadDouble();
            var m_FrameCount = reader.ReadUInt64();
            var m_Format = reader.ReadInt32();
            var m_AudioChannelCount = reader.ReadUInt16Array();
            reader.AlignStream();
            var m_AudioSampleRate = reader.ReadUInt32Array();
            var m_AudioLanguage = reader.ReadStringArray();
            if (version[0] >= 2020) //2020.1 and up
            {
                var m_VideoShadersSize = reader.ReadInt32();
                var m_VideoShaders = new PPtr<Shader>[m_VideoShadersSize];
                for (int i = 0; i < m_VideoShadersSize; i++)
                {
                    m_VideoShaders[i] = new PPtr<Shader>(reader);
                }
            }
            m_ExternalResources = new StreamedResource(reader);
            var m_HasSplitAlpha = reader.ReadBoolean();
            if (version[0] >= 2020) //2020.1 and up
            {
                var m_sRGB = reader.ReadBoolean();
            }

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(m_ExternalResources.m_Source))
            {
                resourceReader = new ResourceReader(m_ExternalResources.m_Source, assetsFile, m_ExternalResources.m_Offset, m_ExternalResources.m_Size);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, m_ExternalResources.m_Size);
            }
            m_VideoData = resourceReader;
        }
    }
}
