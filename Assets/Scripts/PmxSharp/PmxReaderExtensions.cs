// https://gist.github.com/felixjones/f8a06bd48f9da9a4539f
// https://gist.github.com/Binsk/3bfd792e2dd4aa9cd6bb45dd05c0727e

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace PmxSharp
{
	public static class PmxReaderExtensions
	{
		#region Read geometry
		/// <summary>
		/// Reads two 4-byte floating point values from the current stream, and advances the current position of the stream by 8 bytes.
		/// </summary>
		/// <returns>The parsed values composed as a Vector2.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		/// <exception cref="IOException"></exception>
		public static Vector2 ReadVector2(this BinaryReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle());
		}

		/// <summary>
		/// Reads three 4-byte floating point values from the current stream, and advances the current position of the stream by 12 bytes.
		/// </summary>
		/// <returns>The parsed values composed as a Vector3.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		/// <exception cref="IOException"></exception>
		public static Vector3 ReadVector3(this BinaryReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		/// <summary>
		/// Reads four 4-byte floating point values from the current stream, and advances the current position of the stream by 16 bytes.
		/// </summary>
		/// <returns>The parsed values composed as a Vector4.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		/// <exception cref="IOException"></exception>
		public static Vector4 ReadVector4(this BinaryReader reader)
		{
			return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		/// <summary>
		/// Reads three 4-byte floating point values from the current stream, and advances the current position of the stream by 12 bytes.
		/// </summary>
		/// <returns>The parsed values composed as a Quaternion.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		/// <exception cref="IOException"></exception>
		public static Quaternion ReadQuaternion(this BinaryReader reader)
		{
			return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		/// <summary>
		/// Reads three 4-byte floating point values from the current stream, and advances the current position of the stream by 12 bytes.
		/// </summary>
		/// <returns>The parsed values composed into a Color with an alpha of 1.</returns>
		public static UnityEngine.Color ReadColor3(this BinaryReader reader)
		{
			Vector3 v = reader.ReadVector3();
			return new Color(v.x, v.y, v.z, 1.0f);
		}

		/// <summary>
		/// Reads four 4-byte floating point values from the current stream, and advances the current position of the stream by 16 bytes.
		/// </summary>
		/// <returns>The parsed values composed into a Color.</returns>
		public static UnityEngine.Color ReadColor4(this BinaryReader reader)
		{
			Vector4 v = reader.ReadVector4();
			return new Color(v.x, v.y, v.z, v.w);
		}
		#endregion
		#region Read text
		/// <summary>
		/// Read and parse a string, which begins with a 32-bit integer that defines its length in bytes, using the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to parse the string with, defined in the file's Globals.</param>
		/// <exception cref="ArgumentNullException">Thrown when encoding is null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="encoding"/> is not supported (only <see cref="System.Text.UnicodeEncoding"/> and <see cref="System.Text.UTF8Encoding"/> are supported.</exception>
		/// <returns>The string parsed from the PMX file.</returns>
		public static string ReadPmxString(this BinaryReader reader, System.Text.Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding", "Encoding cannot be null. It must either be Unicode (UTF-16 LE) or UTF-8.");
			if (!(encoding is System.Text.UnicodeEncoding || encoding is System.Text.UTF8Encoding))
				throw new ArgumentException("Unsupported encoding. PMX only uses Unicode (UTF-16 LE) and UTF-8.", "encoding");

			return encoding.GetString(reader.ReadBytes(reader.ReadInt32()));
		}

		/// <summary>
		/// Read and parse a string, which begins with a 32-bit integer that defines its length in bytes, using UTF-16 LE encoding.
		/// </summary>
		/// <returns>The string parsed from the PMX file.</returns>
		[System.Obsolete("PMX might use the UTF-8 encoding, pass the encoding defined in the PMX file's Globals section.")]
		public static string ReadPmxString(this BinaryReader reader)
		{
			return System.Text.Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
		}
		#endregion
		#region Read index
		/// <summary>
		/// Reads a variable length integer value from the current stream, and advances the current position of the stream by 12 bytes.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static int ReadIndex(this BinaryReader reader, PmxTypes.IndexType type)
		{
			int size = 0;
			switch (type)
			{
				case PmxTypes.IndexType.Vertex:
					size = PmxTypes.VertexIndex;
					break;
				case PmxTypes.IndexType.Texture:
					size = PmxTypes.TextureIndex;
					break;
				case PmxTypes.IndexType.Material:
					size = PmxTypes.MaterialIndex;
					break;
				case PmxTypes.IndexType.Bone:
					size = PmxTypes.BoneIndex;
					break;
				case PmxTypes.IndexType.Morph:
					size = PmxTypes.MorphIndex;
					break;
				case PmxTypes.IndexType.Rigidbody:
					size = PmxTypes.RigidbodyIndex;
					break;
			}

			// Vertex indices are unsigned byte, unsigned short or signed int.
			// All other indices are signed byte, signed short or signed int (to allow -1 to mark nil)
			// All of them are cast to signed int here.
			switch (size)
			{
				case 1:
					if (type == PmxTypes.IndexType.Vertex)
						return reader.ReadByte();
					return reader.ReadSByte();
				case 2:
					if (type == PmxTypes.IndexType.Vertex)
						return reader.ReadUInt16();
					return reader.ReadInt16();
				case 3:
					return reader.ReadInt32();
			}
			throw new System.FormatException();
		}
		#endregion
		#region Read PMX structures
		/// <summary>
		/// Reads a morph structure from the file and advances the stream position to the beginning of the next item.
		/// </summary>
		/// <param name="encoding">The text encoding to use when reading strings.</param>
		/// <returns>The next morph in the PMX file.</returns>
		public static PmxMorph ReadPmxMorph(this BinaryReader reader, System.Text.Encoding encoding)
		{
			PmxMorph morph = new PmxMorph();
			morph.NameJapanese = reader.ReadPmxString(encoding);
			morph.NameEnglish = reader.ReadPmxString(encoding);
			morph.Group = (PmxMorph.MorphGroup)reader.ReadByte();
			morph.Type = (PmxMorph.MorphType)reader.ReadByte();

			int count = reader.ReadInt32();
			morph.Offsets = new PmxMorphOffset[count];

			switch (morph.Type)
			{
				case PmxMorph.MorphType.Group:
					for(int i = 0; i < count; ++i)
					{
						morph.Offsets[i] = new PmxGroupOffset(reader.ReadIndex(PmxTypes.IndexType.Morph), reader.ReadSingle());
					}
					break;
				case PmxMorph.MorphType.Vertex:
					for(int i = 0; i < count; ++i)
					{
						morph.Offsets[i] = new PmxVertexOffset(reader.ReadIndex(PmxTypes.IndexType.Vertex), reader.ReadVector3());
					}
					break;
				case PmxMorph.MorphType.Bone:
					for(int i = 0; i < count; ++i)
					{
						PmxBoneOffset o = new PmxBoneOffset();
						o.Index = reader.ReadIndex(PmxTypes.IndexType.Bone);
						o.Translation = reader.ReadVector3();
						o.Rotation = reader.ReadQuaternion();
						morph.Offsets[i] = o;
					}
					break;
				case PmxMorph.MorphType.UV:
				case PmxMorph.MorphType.UV1:
				case PmxMorph.MorphType.UV2:
				case PmxMorph.MorphType.UV3:
				case PmxMorph.MorphType.UV4:
					for(int i = 0; i < count; ++i)
					{
						morph.Offsets[i] = new PmxUVOffset(reader.ReadIndex(PmxTypes.IndexType.Vertex), reader.ReadVector4());
					}
					break;
				case PmxMorph.MorphType.Material:
					for(int i = 0; i < count; ++i)
					{
						PmxMaterialOffset o = new PmxMaterialOffset();
						o.Index = reader.ReadIndex(PmxTypes.IndexType.Material);
						o.Multiplicative = reader.ReadByte() != 0;
						o.DiffuseTint = reader.ReadColor4();
						o.SpecularTint = reader.ReadColor3();
						o.SpecularExponent = reader.ReadSingle();
						o.AmbientTint = reader.ReadColor3();
						o.EdgeTint = reader.ReadColor4();
						o.EdgeSize = reader.ReadSingle();
						o.TextureTint = reader.ReadColor4();
						o.SphereTint = reader.ReadColor4();
						o.ToonTint = reader.ReadColor4();
						morph.Offsets[i] = o;
					}
					break;
				default:
					throw new PmxException(string.Format("The given morph type ({0}) is not supported.", morph.Type.ToString()));
			}

			return morph;
		}
		/// <summary>
		/// Reads a bone structure from teh file and advances the stream position to the next item.
		/// </summary>
		/// <param name="encoding">The text encoding to use when reading strings.</param>
		/// <returns>The next bone in the PMX file.</returns>
		public static PmxBone ReadPmxBone(this BinaryReader reader, System.Text.Encoding encoding)
		{
			PmxBone bone = new PmxBone(reader.ReadPmxString(encoding), reader.ReadPmxString(encoding));

			bone.Position = reader.ReadVector3();
			bone.Parent = reader.ReadIndex(PmxTypes.IndexType.Bone);
			bone.Layer = reader.ReadInt32();
			bone.Flags = (PmxBone.BoneFlags)reader.ReadInt16();

			// Tail
			if (bone.HasFlag(PmxBone.BoneFlags.TailIsIndex))
			{
				bone.TailIndex = reader.ReadIndex(PmxTypes.IndexType.Bone);
			}
			else
			{
				bone.TailPosition = reader.ReadVector3();
			}

			// Inherit
			if (bone.HasFlag(PmxBone.BoneFlags.InheritRotation | PmxBone.BoneFlags.InheritTranslation))
			{
				bone.InheritFromIndex = reader.ReadIndex(PmxTypes.IndexType.Bone);
				bone.InheritWeight = reader.ReadSingle();
			}

			// Fixed axis
			if (bone.HasFlag(PmxBone.BoneFlags.FixedAxis))
			{
				bone.FixedAxis = reader.ReadVector3();
			}

			// Local transformation
			if (bone.HasFlag(PmxBone.BoneFlags.LocalTransform))
			{
				bone.LocalX = reader.ReadVector3();
				bone.LocalZ = reader.ReadVector3();
			}

			// Outside parent
			if (bone.HasFlag(PmxBone.BoneFlags.ExternalDeform))
			{
				bone.ExternalParentIndex = reader.ReadIndex(PmxTypes.IndexType.Bone);
			}

			// IK
			if (bone.HasFlag(PmxBone.BoneFlags.IK))
			{
				bone.IK = new PmxIK();
				bone.IK.TargetIndex = reader.ReadIndex(PmxTypes.IndexType.Bone);
				bone.IK.Loop = reader.ReadInt32();
				bone.IK.Limit = reader.ReadSingle();
				int linkCount = reader.ReadInt32();
				bone.IK.Links = new List<PmxIKLink>();
				for (int j = 0; j < linkCount; ++j)
				{
					PmxIKLink link = new PmxIKLink();
					link.Index = reader.ReadIndex(PmxTypes.IndexType.Bone);
					link.HasLimit = reader.ReadByte() != 0;
					if (link.HasLimit)
					{
						link.EulerLimitMin = reader.ReadVector3();
						link.EulerLimitMax = reader.ReadVector3();
					}
					else
					{
						link.EulerLimitMin = new Vector3();
						link.EulerLimitMax = new Vector3();
					}
					bone.IK.Links.Add(link);
				}
			}
			return bone;
		}
		#endregion
	}
}
