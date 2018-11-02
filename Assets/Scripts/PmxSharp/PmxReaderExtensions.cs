// https://gist.github.com/felixjones/f8a06bd48f9da9a4539f

using System;
using System.IO;

using UnityEngine;

namespace PmxSharp
{
	public static class PmxReaderExtensions
	{
		#region Read vectors
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
				throw new System.ArgumentNullException("encoding", "Encoding cannot be null. It must either be Unicode (UTF-16 LE) or UTF-8.");
			if (!(encoding is System.Text.UnicodeEncoding || encoding is System.Text.UTF8Encoding))
				throw new System.ArgumentException("Unsupported encoding. PMX only uses Unicode (UTF-16 LE) and UTF-8.", "encoding");

			try
			{
				return encoding.GetString(reader.ReadBytes(reader.ReadInt32()));
			}
			catch(Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			return "";
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
			switch(size)
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
	}
}
