// https://gist.github.com/felixjones/f8a06bd48f9da9a4539f

using System;

namespace PmxSharp
{
	/// <summary>
	/// Length of PMX-related types in bytes
	/// </summary>
	public static class PmxTypes
	{
		#region Standard C types
		public const int Byte = 1;
		public const int UByte = 1;
		public const int Short = 2;
		public const int UShort = 2;
		public const int Int = 4;
		public const int UInt = 4;
		public const int Float = 4;
		#endregion
		#region PMX fixed-size types
		/// <summary>
		/// Vector with two floating point elements
		/// </summary>
		public const int Vector2 = 8;
		/// <summary>
		/// Vector with three floating point elements
		/// </summary>
		public const int Vector3 = 12;
		/// <summary>
		/// Vector with four floating point elements
		/// </summary>
		public const int Vector4 = 16;
		/// <summary>
		/// Up to eight 1-bit binary flags.
		/// </summary>
		public const int Flag = 1;
		/// <summary>
		/// Up to sixteen 1-bit binary flags.
		/// </summary>
		public const int LongFlag = 2;
		#endregion
		#region PMX variable-size indices
		public enum IndexType { Vertex, Texture, Material, Bone, Morph, Rigidbody }
		/// <summary>
		/// Size of vertex indices.
		/// </summary>
		public static int VertexIndex = 1;
		/// <summary>
		/// Size of bone indices.
		/// </summary>
		public static int BoneIndex = 1;
		/// <summary>
		/// Size of texture indices.
		/// </summary>
		public static int TextureIndex = 1;
		/// <summary>
		/// Size of material indices.
		/// </summary>
		public static int MaterialIndex = 1;
		/// <summary>
		/// Size of morph indices.
		/// </summary>
		public static int MorphIndex = 1;
		/// <summary>
		/// Size of rigid body indices.
		/// </summary>
		public static int RigidbodyIndex = 1;
		#endregion
	}
}
