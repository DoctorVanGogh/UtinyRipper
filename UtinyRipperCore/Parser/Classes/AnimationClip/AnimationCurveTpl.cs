﻿using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationCurveTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public AnimationCurveTpl(bool _)
		{
			Curve = new List<KeyframeTpl<T>>();
			PreInfinity = 2;
			PostInfinity = 2;
			RotationOrder = 4;
		}

		public AnimationCurveTpl(T defaultValue, T defaultWeight):
			this(true)
		{
			KeyframeTpl<T> firstKey = new KeyframeTpl<T>(0.0f, defaultValue, defaultWeight);
			KeyframeTpl<T> secondKey = new KeyframeTpl<T>(1.0f, defaultValue, defaultWeight);
			Curve.Add(firstKey);
			Curve.Add(secondKey);
		}

		public AnimationCurveTpl(T value0, T inSlope0, T outSlope0, T value1, T inSlope1, T outSlope1, T defaultWeight) :
		   this(true)
		{
			KeyframeTpl<T> firstKey = new KeyframeTpl<T>(0.0f, value0, inSlope0, outSlope0, defaultWeight);
			KeyframeTpl<T> secondKey = new KeyframeTpl<T>(1.0f, value1, inSlope1, outSlope1, defaultWeight);
			Curve.Add(firstKey);
			Curve.Add(secondKey);
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadRotationOrder(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(2, 1))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			Curve = new List<KeyframeTpl<T>>();
			Curve.Read(stream);
			stream.AlignStream(AlignType.Align4);

			PreInfinity = stream.ReadInt32();
			PostInfinity = stream.ReadInt32();
			if (IsReadRotationOrder(stream.Version))
			{
				RotationOrder = stream.ReadInt32();
			}
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Curve", GetCurves().ExportYAML(container));
			node.Add("m_PreInfinity", PreInfinity);
			node.Add("m_PostInfinity", PostInfinity);
			node.Add("m_RotationOrder", GetExportRotationOrder(container.Version));

			return node;
		}

		private IReadOnlyList<KeyframeTpl<T>> GetCurves()
		{
			if(Curve.Count == 1)
			{
				KeyframeTpl<T> firstKey = Curve[0];
				KeyframeTpl<T> secondKey = new KeyframeTpl<T>(firstKey.Time + 0.1f, firstKey);
				KeyframeTpl<T>[] curves = new KeyframeTpl<T>[2];
				curves[0] = firstKey;
				curves[1] = secondKey;
				return curves;
			}
			else
			{
				return Curve;
			}
		}

		private int GetExportRotationOrder(Version version)
		{
			return IsReadRotationOrder(version) ? RotationOrder : 4;
		}

		public static AnimationCurveTpl<T> Empty { get; } = new AnimationCurveTpl<T>(true);

		public List<KeyframeTpl<T>> Curve { get; private set; }
		public int PreInfinity { get; private set; }
		public int PostInfinity { get; private set; }
		public int RotationOrder { get; private set; }
	}
}
