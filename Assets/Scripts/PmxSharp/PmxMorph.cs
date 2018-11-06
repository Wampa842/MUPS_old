using System;
using System.Collections.Generic;
using UnityEngine;

namespace PmxSharp
{
	#region Offsets
	/// <summary>
	/// Base class for PMX morph offsets.
	/// </summary>
	public abstract class PmxMorphOffset
	{
		/// <summary>
		/// The index of the item that belongs to this morph.
		/// </summary>
		public int Index { get; set; }
	}

	/// <summary>
	/// Offset that influences other morphs.
	/// </summary>
	public class PmxGroupOffset : PmxMorphOffset
	{
		/// <summary>
		/// The scale of influence over the indexed morph.
		/// </summary>
		public float Scale { get; set; }

		public PmxGroupOffset(int index, float scale)
		{
			Index = index;
			Scale = scale;
		}
	}

	/// <summary>
	/// Offset that translates a vertex.
	/// </summary>
	public class PmxVertexOffset : PmxMorphOffset
	{
		/// <summary>
		/// The translation applied to the vertex.
		/// </summary>
		public Vector3 Translation { get; set; }

		public PmxVertexOffset(int index, Vector3 translation)
		{
			Index = index;
			Translation = translation;
		}
	}

	/// <summary>
	/// Offset that transforms a bone.
	/// </summary>
	public class PmxBoneOffset : PmxMorphOffset
	{
		/// <summary>
		/// The translation applied to the bone.
		/// </summary>
		public Vector3 Translation { get; set; }
		/// <summary>
		/// The bone's rotation represented as a quaternion.
		/// </summary>
		public Quaternion Rotation { get; set; }
		/// <summary>
		/// The bone's rotation represented as its Euler angles.
		/// </summary>
		public Vector3 Euler
		{
			get
			{
				return Rotation.eulerAngles;
			}
			set
			{
				Rotation = Quaternion.Euler(value);
			}
		}
	}

	/// <summary>
	/// Offset that transforms a vertex's UV coordinates.
	/// </summary>
	public class PmxUVOffset : PmxMorphOffset
	{
		/// <summary>
		/// The transformation applied to the UV coordinates.
		/// </summary>
		public Vector4 Transform { get; set; }

		public PmxUVOffset(int index, Vector4 transform)
		{
			Index = index;
			Transform = transform;
		}
	}

	/// <summary>
	/// Offset that modifies a material's properties.
	/// </summary>
	public class PmxMaterialOffset : PmxMorphOffset
	{
		public bool Multiplicative { get; set; }
		public Color DiffuseTint { get; set; }
		public Color SpecularTint { get; set; }
		public float SpecularExponent { get; set; }
		public Color AmbientTint { get; set; }
		public Color EdgeTint { get; set; }
		public float EdgeSize { get; set; }
		public Color TextureTint { get; set; }
		public Color SphereTint { get; set; }
		public Color ToonTint { get; set; }
	}
	#endregion

	/// <summary>
	/// Represents a PMX morph.
	/// </summary>
	public class PmxMorph : PmxNamedItem
	{
		public enum MorphGroup { None = 0, Eyebrow = 1, Eye = 2, Mouth = 3, Other = 4 }
		public enum MorphType { Group = 0, Vertex = 1, Bone = 2, UV = 3, UV1 = 4, UV2 = 5, UV3 = 6, UV4 = 7, Material = 8, Flip = 9, Impulse = 10 }

		public MorphGroup Group { get; set; }
		public MorphType Type { get; set; }
		public PmxMorphOffset[] Offsets { get; set; }
	}
}
