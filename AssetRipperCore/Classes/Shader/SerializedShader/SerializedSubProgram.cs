using AssetRipper.Core.Extensions;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.Core.Classes.Shader.Parameters;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedSubProgram : IAssetReadable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// KeywordIndices has been renamed to GlobalKeywordIndices
			if (version.IsGreaterEqual(2019))
			{
				return 3;
			}

			// TODO:
			return 2;
			// return 1;
		}


		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlignKeywordIndices(UnityVersion version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasLocalKeywordIndices(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2020.3.0f2 to 2020.3.x<br/>
		/// 2021.1.4 and greater
		/// </summary>
		public static bool HasUnifiedParameters(UnityVersion version)
		{
			if (version.Major == 2020 && version.IsGreaterEqual(2020, 3, 0, UnityVersionType.Final, 2))
				return true;
			else if (version.IsGreaterEqual(2021, 1, 4))
				return true;
			else
				return false;
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasSamplers(UnityVersion version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasShaderRequirements(UnityVersion version) => version.IsGreaterEqual(2017, 2);

		/// <summary>
		/// 2021 and greater
		/// </summary>
		private static bool IsShaderRequirementsInt64(UnityVersion version) => version.IsGreaterEqual(2021);

		public void Read(AssetReader reader)
		{
			BlobIndex = reader.ReadUInt32();
			Channels.Read(reader);
			GlobalKeywordIndices = reader.ReadUInt16Array();
			if (IsAlignKeywordIndices(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasLocalKeywordIndices(reader.Version))
			{
				LocalKeywordIndices = reader.ReadUInt16Array();
				reader.AlignStream();
			}

			ShaderHardwareTier = reader.ReadByte();
			GpuProgramType = reader.ReadByte();
			reader.AlignStream();

			if (HasUnifiedParameters(reader.Version))
			{
				Parameters = reader.ReadAsset<SerializedProgramParameters>();
				VectorParams = Parameters.VectorParams;
				MatrixParams = Parameters.MatrixParams;
				TextureParams = Parameters.TextureParams;
				BufferParams = Parameters.BufferParams;
				ConstantBuffers = Parameters.ConstantBuffers;
				ConstantBufferBindings = Parameters.ConstantBufferBindings;
				UAVParams = Parameters.UAVParams;
			}
			else
			{
				VectorParams = reader.ReadAssetArray<VectorParameter>();
				MatrixParams = reader.ReadAssetArray<MatrixParameter>();
				TextureParams = reader.ReadAssetArray<TextureParameter>();
				BufferParams = reader.ReadAssetArray<BufferBinding>();
				ConstantBuffers = reader.ReadAssetArray<ConstantBuffer>();
				ConstantBufferBindings = reader.ReadAssetArray<BufferBinding>();
				UAVParams = reader.ReadAssetArray<UAVParameter>();
			}

			if (HasSamplers(reader.Version))
			{
				Samplers = reader.ReadAssetArray<SamplerParameter>();
			}
			if (HasShaderRequirements(reader.Version))
			{
				if (IsShaderRequirementsInt64(reader.Version))
					ShaderRequirements = reader.ReadInt64();
				else
					ShaderRequirements = reader.ReadInt32();
			}
		}

		public void Export(ShaderWriter writer, ShaderType type, bool isTier)
		{
			writer.WriteIndent(4);
#warning TODO: convertion (DX to HLSL)
			ShaderGpuProgramType programType = GetProgramType(writer.Version);
			GPUPlatform graphicApi = programType.ToGPUPlatform(writer.Platform);
			writer.Write("SubProgram \"{0} ", graphicApi);
			if (isTier)
			{
				writer.Write("hw_tier{0} ", ShaderHardwareTier.ToString("00"));
			}
			writer.Write("\" {\n");
			writer.WriteIndent(5);

			int platformIndex = writer.Shader.Platforms.IndexOf(graphicApi);
			writer.Shader.Blobs[platformIndex].SubPrograms[BlobIndex].Export(writer, type);

			writer.Write('\n');
			writer.WriteIndent(4);
			writer.Write("}\n");
		}

		public ShaderGpuProgramType GetProgramType(UnityVersion version)
		{
			if (ShaderGpuProgramTypeExtensions.GpuProgramType55Relevant(version))
			{
				return ((ShaderGpuProgramType55)GpuProgramType).ToGpuProgramType();
			}
			else
			{
				return ((ShaderGpuProgramType53)GpuProgramType).ToGpuProgramType();
			}
		}

		public uint BlobIndex { get; set; }
		/// <summary>
		/// KeywordIndices previously
		/// </summary>
		public ushort[] GlobalKeywordIndices { get; set; }
		public ushort[] LocalKeywordIndices { get; set; }
		public byte ShaderHardwareTier { get; set; }
		public byte GpuProgramType { get; set; }
		public SerializedProgramParameters Parameters { get; set; }
		public VectorParameter[] VectorParams { get; set; }
		public MatrixParameter[] MatrixParams { get; set; }
		public TextureParameter[] TextureParams { get; set; }
		public BufferBinding[] BufferParams { get; set; }
		public ConstantBuffer[] ConstantBuffers { get; set; }
		public BufferBinding[] ConstantBufferBindings { get; set; }
		public UAVParameter[] UAVParams { get; set; }
		public SamplerParameter[] Samplers { get; set; }
		public long ShaderRequirements { get; set; }

		public ParserBindChannels Channels;
	}
}
