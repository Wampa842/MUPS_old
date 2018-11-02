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
		/// The bone's rotation.
		/// </summary>
		public Quaternion Rotation { get; set; }
		/// <summary>
		/// The real number vector representation of the bone's rotation quaternion.
		/// </summary>
		public Vector4 RotationReal
		{
			get
			{
				return new Vector4(Rotation.x, Rotation.y, Rotation.z, Rotation.w);
			}
			set
			{
				Rotation = new Quaternion(value.x, value.y, value.z, value.w);
			}
		}
		/// <summary>
		/// The Euler representation of the bone's rotation.
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
	}
	#endregion

	/// <summary>
	/// Represents a PMX morph.
	/// </summary>
	public class PmxMorph
	{
		public enum MorphGroup { None = 0, Eyebrow = 1, Eye = 2, Mouth = 3, Other = 4 }
		public enum MorphType { Group = 0, Vertex = 1, Bone = 2, UV = 3, UV1 = 4, UV2 = 5, UV3 = 6, UV4 = 7, Material = 8, Flip = 9, Impulse = 10 }

		public string NameJapanese { get; set; }
		public string NameEnglish { get; set; }
		public MorphGroup Group { get; set; }
		public MorphType Type { get; set; }
		public List<PmxMorphOffset> Offsets { get; set; }

		public PmxMorph()
		{
			NameJapanese = string.Format("Morph {0}", GetHashCode());
			NameEnglish = string.Format("Morph {0}", GetHashCode());
		}

		public PmxMorph(string jp, string en)
		{
			this.NameJapanese = jp;
			this.NameEnglish = en;
		}
	}
}
