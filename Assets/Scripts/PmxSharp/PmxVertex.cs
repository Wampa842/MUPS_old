using System.Collections.Generic;
using UnityEngine;

namespace PmxSharp
{
	#region Deform classes
	public enum DeformType { BDEF1, BDEF2, BDEF4, SDEF, QDEF }

	/// <summary>
	/// Deform interface that exposes the Type property.
	/// </summary>
	public interface IPmxVertexDeform
	{
		DeformType Type { get; }
		BoneWeight GetUnityWeight();
	}

	/// <summary>
	/// Skin deform using a single bone.
	/// </summary>
	public class Bdef1Deform : IPmxVertexDeform
	{
		public DeformType Type
		{
			get
			{
				return DeformType.BDEF1;
			}
		}

		public int Bone { get; set; }
		

		public Bdef1Deform()
		{
			Bone = -1;
		}

		public Bdef1Deform(int bone)
		{
			this.Bone = bone;
		}

		public BoneWeight GetUnityWeight()
		{
			return new BoneWeight { boneIndex0 = Bone, weight0 = 1.0f };
		}
	}

	/// <summary>
	/// Skin deform using up to two bones.
	/// </summary>
	public class Bdef2Deform : IPmxVertexDeform
	{
		public DeformType Type
		{
			get
			{
				return DeformType.BDEF2;
			}
		}

		/// <summary>
		/// The first bone's index.
		/// </summary>
		public int Bone1 { get; set; }
		/// <summary>
		/// The second bone's index.
		/// </summary>
		public int Bone2 { get; set; }
		/// <summary>
		/// The first bone's weight.
		/// </summary>
		public float Weight1 { get; set; }
		/// <summary>
		/// The second bone's weight.
		/// </summary>
		public float Weight2
		{
			get
			{
				return 1.0f - Weight1;
			}
		}

		/// <summary>
		/// Create an empty deform object.
		/// </summary>
		public Bdef2Deform()
		{
			Bone1 = -1;
			Bone2 = -1;
			Weight1 = 1.0f;
		}

		/// <summary>
		/// Create a deform object that uses the specified bones.
		/// </summary>
		/// <param name="bone1">The first bone's index.</param>
		/// <param name="bone2">The second bone's index.</param>
		public Bdef2Deform(int bone1, int bone2)
		{
			this.Bone1 = bone1;
			this.Bone2 = bone2;
		}

		/// <summary>
		/// Create a deform object that uses the specified bones.
		/// </summary>
		/// <param name="bone1">The first bone's index.</param>
		/// <param name="bone2">The second bone's index.</param>
		/// <param name="weight1">The first bone's weight.</param>
		public Bdef2Deform(int bone1, int bone2, float weight1) : this(bone1, bone2)
		{
			this.Weight1 = weight1;
		}

		public BoneWeight GetUnityWeight()
		{
			return new BoneWeight { boneIndex0 = Bone1, boneIndex1 = Bone2, weight0 = Weight1, weight1 = Weight2 };
		}
	}

	/// <summary>
	/// Skin deform using up to four bones.
	/// </summary>
	public class Bdef4Deform : IPmxVertexDeform
	{
		public DeformType Type
		{
			get
			{
				return DeformType.BDEF4;
			}
		}

		/// <summary>
		/// The first bone's index.
		/// </summary>
		public int Bone1 { get; set; }
		/// <summary>
		/// The second bone's index.
		/// </summary>
		public int Bone2 { get; set; }
		/// <summary>
		/// The third bone's index.
		/// </summary>
		public int Bone3 { get; set; }
		/// <summary>
		/// The fourth bone's index.
		/// </summary>
		public int Bone4 { get; set; }
		/// <summary>
		/// The first bone's weight.
		/// </summary>
		public float Weight1 { get; set; }
		/// <summary>
		/// The second bone's weight.
		/// </summary>
		public float Weight2 { get; set; }
		/// <summary>
		/// The third bone's weight.
		/// </summary>
		public float Weight3 { get; set; }
		/// <summary>
		/// The fourth bone's weight.
		/// </summary>
		public float Weight4 { get; set; }

		public BoneWeight GetUnityWeight()
		{
			return new BoneWeight
			{
				boneIndex0 = Bone1,
				boneIndex1 = Bone2,
				boneIndex2 = Bone3,
				boneIndex3 = Bone4,
				weight0 = Weight1,
				weight1 = Weight2,
				weight2 = Weight3,
				weight3 = Weight4
			};
		}
	}

	/// <summary>
	/// Dual-quaternion skin deform using up to four bones.
	/// </summary>
	public class QdefDeform : IPmxVertexDeform
	{
		public DeformType Type
		{
			get
			{
				return DeformType.QDEF;
			}
		}

		/// <summary>
		/// The first bone's index.
		/// </summary>
		public int Bone1 { get; set; }
		/// <summary>
		/// The second bone's index.
		/// </summary>
		public int Bone2 { get; set; }
		/// <summary>
		/// The third bone's index.
		/// </summary>
		public int Bone3 { get; set; }
		/// <summary>
		/// The fourth bone's index.
		/// </summary>
		public int Bone4 { get; set; }
		/// <summary>
		/// The first bone's weight.
		/// </summary>
		public float Weight1 { get; set; }
		/// <summary>
		/// The second bone's weight.
		/// </summary>
		public float Weight2 { get; set; }
		/// <summary>
		/// The third bone's weight.
		/// </summary>
		public float Weight3 { get; set; }
		/// <summary>
		/// The fourth bone's weight.
		/// </summary>
		public float Weight4 { get; set; }

		public BoneWeight GetUnityWeight()
		{
			return new BoneWeight
			{
				boneIndex0 = Bone1,
				boneIndex1 = Bone2,
				boneIndex2 = Bone3,
				boneIndex3 = Bone4,
				weight0 = Weight1,
				weight1 = Weight2,
				weight2 = Weight3,
				weight3 = Weight4
			};
		}
	};

	/// <summary>
	/// Spherical deform using two bones. I have no fucken idea how this works.
	/// </summary>
	public class SdefDeform : IPmxVertexDeform
	{
		public DeformType Type
		{
			get
			{
				return DeformType.SDEF;
			}
		}

		/// <summary>
		/// The first bone's index.
		/// </summary>
		public int Bone1 { get; set; }
		/// <summary>
		/// The second bone's index.
		/// </summary>
		public int Bone2 { get; set; }
		/// <summary>
		/// The first bone's weight.
		/// </summary>
		public float Weight1 { get; set; }
		/// <summary>
		/// The second bone's weight.
		/// </summary>
		public float Weight2
		{
			get
			{
				return 1.0f - Weight1;
			}
		}

		/// <summary>
		/// Unknown variable - probably the center of a sphere.
		/// </summary>
		public Vector3 C { get; set; }
		/// <summary>
		/// Unknown variable - probably a radius vector.
		/// </summary>
		public Vector3 R0 { get; set; }
		/// <summary>
		/// Unknown variable - probably a radius vector.
		/// </summary>
		public Vector3 R1 { get; set; }

		public BoneWeight GetUnityWeight()
		{
			throw new System.NotImplementedException();
		}
	}
	#endregion

	/// <summary>
	/// Represents a PMX vertex.
	/// </summary>
	public class PmxVertex
	{
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector2 UV { get; set; }
		public List<Vector4> UVAdditional { get; set; }
		public DeformType Deform { get; set; }
		public IPmxVertexDeform DeformData { get; set; }
		public float EdgeSize { get; set; }

		/// <summary>
		/// Create a vertex at the specified position with the given normal and UV vectors.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="normal"></param>
		/// <param name="uv"></param>
		public PmxVertex(Vector3 position, Vector3 normal, Vector2 uv)
		{
			this.Position = position;
			this.Normal = normal;
			this.UV = uv;

			UVAdditional = new List<Vector4>(4);
			Deform = DeformType.BDEF1;
			DeformData = new Bdef1Deform();
			EdgeSize = 1.0f;
		}

		/// <summary>
		/// Create a vertex at the specified position.
		/// </summary>
		/// <param name="position"></param>
		public PmxVertex(Vector3 position) : this(position, new Vector3(), new Vector2()) { }

		/// <summary>
		/// Create an empty vertex.
		/// </summary>
		public PmxVertex() : this(new Vector3()) { }

		/// <summary>
		/// Return the position vectors from a collection of vertices.
		/// </summary>
		/// <param name="coll">The source collection of vertices.</param>
		/// <returns>An indexed list of position vectors.</returns>
		public static List<Vector3> GetPositions(IEnumerable<PmxVertex> coll)
		{
			List<Vector3> output = new List<Vector3>();
			foreach (PmxVertex v in coll)
			{
				output.Add(v.Position);
			}
			return output;
		}
		/// <summary>
		/// Return the normal vectors from a collection of vertices.
		/// </summary>
		/// <param name="coll">The source collection of vertices.</param>
		/// <returns>An indexed list of normal vectors.</returns>
		public static List<Vector3> GetNormals(IEnumerable<PmxVertex> coll)
		{
			List<Vector3> output = new List<Vector3>();
			foreach (PmxVertex v in coll)
			{
				output.Add(v.Normal);
			}
			return output;
		}
		/// <summary>
		/// Return the UV vectors from a collection of vertices.
		/// </summary>
		/// <param name="coll">The source collection of vertices.</param>
		/// <returns>An indexed list of UV vectors.</returns>
		public static List<Vector2> GetUVs(IEnumerable<PmxVertex> coll)
		{
			List<Vector2> output = new List<Vector2>();
			foreach (PmxVertex v in coll)
			{
				output.Add(v.UV);
			}
			return output;
		}
		/// <summary>
		/// Return the BoneWeight structures from a collection of vertices.
		/// </summary>
		/// <param name="coll"></param>
		/// <returns></returns>
		public static List<BoneWeight> GetUnityWeights(IEnumerable<PmxVertex> coll)
		{
			List<BoneWeight> list = new List<BoneWeight>();
			foreach(PmxVertex v in coll)
			{
				list.Add(v.DeformData.GetUnityWeight());
			}
			return list;
		}
	}
}
