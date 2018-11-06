using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace PmxSharp
{
	/// <summary>
	/// Handles the parsing of PMX files.
	/// </summary>
	public sealed class PmxImport : PmxNamedItem, IDisposable
	{
		[Flags]
		public enum InfoField { None = 0, Header = 1, Vertex = 2, Triangle = 4, Mesh = 6, Texture = 8, Material = 16, Bone = 32, Morph = 64, Frame = 128, Rigidbody = 256, Joint = 512, SoftBody = 1024, Physics = 1792 }
		public enum PmxVersion { Unknown, Pmd, Pmx20, Pmx21 }

		private BinaryReader _reader;

		public PmxVersion Version { get; private set; }
		public Encoding TextEncoding { get; set; }
		public string FilePath { get; private set; }
		public string InfoJapanese { get; set; }
		public string InfoEnglish { get; set; }
		public byte AdditionalUVCount { get; private set; }

		public List<PmxVertex> Vertices { get; private set; }
		public List<PmxTriangle> Triangles { get; private set; }
		public List<string> Textures { get; private set; }
		public List<PmxMaterial> Materials { get; private set; }
		public List<PmxBone> Bones { get; private set; }
		public List<PmxMorph> Morphs { get; private set; }
		public List<PmxDisplayFrame> DisplayFrames { get; private set; }
		public List<PmxRigidbody> Rigidbodies { get; private set; }
		public List<PmxJoint> Joints { get; private set; }

		// Constructors
		/// <summary>
		/// Creates an empty importer.
		/// </summary>
		public PmxImport()
		{
			Vertices = new List<PmxVertex>();
			Triangles = new List<PmxTriangle>();
			Textures = new List<string>();
			Materials = new List<PmxMaterial>();
			Bones = new List<PmxBone>();
		}

		/// <summary>
		/// Creates an importer and immediately loads the file from the specified stream.
		/// </summary>
		/// <param name="stream">The stream representing the PMX file.</param>
		public PmxImport(FileStream stream) : this()
		{
			FilePath = stream.Name;
			_reader = new BinaryReader(stream);
			LoadPmx(_reader);
		}

		/// <summary>
		/// Creates an importer and immediately loads the file at the specified path.
		/// </summary>
		/// <param name="path">The path to the PMX file.</param>
		public PmxImport(string path) : this(File.OpenRead(path))
		{
			FilePath = path;
		}

		// Loading
#pragma warning disable 0162
		/// <summary>
		/// Read and process the file.
		/// </summary>
		/// <param name="r">The binary reader pointing at the PMX file.</param>
		private void LoadPmx(BinaryReader r)
		{
			#region Header
			// Signature (4)
			byte[] sig = r.ReadBytes(PmxConstants.SignatureLength);
			if (!sig.ValidatePmxSignature())
			{
				throw new PmxSignatureException(sig);
			}

			// Version (4)
			float parsedVersion = r.ReadSingle();
			if (parsedVersion == 2.0f)
			{
				Version = PmxVersion.Pmx20;
			}
			else if (parsedVersion == 2.1f)
			{
				Version = PmxVersion.Pmx21;
			}

			// Globals (9)
			r.ReadByte();   // This is always 8 in PMX 2.0, safe to ignore
			TextEncoding = r.ReadByte() == 0 ? Encoding.Unicode : Encoding.UTF8;
			AdditionalUVCount = r.ReadByte();   // 0 to 4
			PmxTypes.VertexIndex = r.ReadByte();    // Vertex index types are byte (1), ushort (2) or int (4)
			PmxTypes.TextureIndex = r.ReadByte();   // All other index types are sbyte (1), short (2) or int (4)
			PmxTypes.MaterialIndex = r.ReadByte();
			PmxTypes.BoneIndex = r.ReadByte();
			PmxTypes.MorphIndex = r.ReadByte();
			PmxTypes.RigidbodyIndex = r.ReadByte();

			// Model information
			NameJapanese = r.ReadPmxString(TextEncoding);
			NameEnglish = r.ReadPmxString(TextEncoding);
			InfoJapanese = r.ReadPmxString(TextEncoding);
			InfoEnglish = r.ReadPmxString(TextEncoding);
			#endregion
			#region Vertex data
			Vertices = new List<PmxVertex>(r.ReadInt32());
			for (int i = 0; i < Vertices.Capacity; ++i)
			{
				// Standard vectors
				PmxVertex v = new PmxVertex();
				v.Position = r.ReadVector3();
				v.Normal = r.ReadVector3();
				v.UV = r.ReadVector2() * new Vector2(1, -1);
				// Additional UV
				v.UVAdditional = new List<Vector4>(AdditionalUVCount);
				for (int j = 0; j < AdditionalUVCount; ++j)
				{
					v.UVAdditional.Add(r.ReadVector4());
				}
				// Deform
				byte deformID = r.ReadByte();
				switch (deformID)
				{
					case 0: // BDEF1
						v.DeformData = new Bdef1Deform(r.ReadIndex(PmxTypes.IndexType.Bone));
						break;
					case 1: // BDEF2
						v.DeformData = new Bdef2Deform(r.ReadIndex(PmxTypes.IndexType.Bone), r.ReadIndex(PmxTypes.IndexType.Bone), r.ReadSingle());
						break;
					case 2: // BDEF4
						v.DeformData = new Bdef4Deform()
						{
							Bone1 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone2 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone3 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone4 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Weight1 = r.ReadSingle(),
							Weight2 = r.ReadSingle(),
							Weight3 = r.ReadSingle(),
							Weight4 = r.ReadSingle(),
						};
						break;
					case 3: // SDEF
						v.DeformData = new SdefDeform()
						{
							Bone1 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone2 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Weight1 = r.ReadSingle(),
							C = r.ReadVector3(),
							R0 = r.ReadVector3(),
							R1 = r.ReadVector3()
						};
						break;
					case 4: // QDEF
						v.DeformData = new QdefDeform()
						{
							Bone1 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone2 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone3 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Bone4 = r.ReadIndex(PmxTypes.IndexType.Bone),
							Weight1 = r.ReadSingle(),
							Weight2 = r.ReadSingle(),
							Weight3 = r.ReadSingle(),
							Weight4 = r.ReadSingle(),
						};
						break;
					default:
						throw new PmxDeformException(deformID);
				}
				v.EdgeSize = r.ReadSingle();

				Vertices.Add(v);
			}
			#endregion
			#region Triangle data
			int trianglePointCount = r.ReadInt32();
			if ((trianglePointCount % 3) != 0)
				throw new PmxFormatException(string.Format("The triangle count is incorrect. Expected divisible by 3, got {0} (remainder {1})", trianglePointCount, trianglePointCount % 3));
			Triangles = new List<PmxTriangle>(trianglePointCount / 3);
			for (int i = 0; i < Triangles.Capacity; ++i)
			{
				PmxTriangle tri = new PmxTriangle(r.ReadIndex(PmxTypes.IndexType.Vertex), r.ReadIndex(PmxTypes.IndexType.Vertex), r.ReadIndex(PmxTypes.IndexType.Vertex));
				Triangles.Add(tri);
			}
			#endregion
			#region Texture list
			Textures = new List<string>(r.ReadInt32());
			for (int i = 0; i < Textures.Capacity; ++i)
			{
				Textures.Add(r.ReadPmxString(TextEncoding));
			}
			#endregion
			#region Material data
			int assignedVertices = 0;
			Materials = new List<PmxMaterial>(r.ReadInt32());
			for (int i = 0; i < Materials.Capacity; ++i)
			{
				PmxMaterial m = new PmxMaterial(r.ReadPmxString(TextEncoding), r.ReadPmxString(TextEncoding));
				// Colors
				m.DiffuseColor = r.ReadColor4();
				m.SpecularColor = r.ReadColor3();
				m.SpecularExponent = r.ReadSingle();
				m.AmbientColor = r.ReadColor3();
				// Properties
				m.Flags = (PmxMaterial.MaterialFlags)r.ReadByte();
				m.EdgeColor = r.ReadColor4();
				m.EdgeSize = r.ReadSingle();
				// Textures
				int index = r.ReadIndex(PmxTypes.IndexType.Texture);
				m.DiffuseTexturePath = index < 0 ? "" : Textures[index];
				index = r.ReadIndex(PmxTypes.IndexType.Texture);
				m.SphereTexturePath = index < 0 ? "" : Textures[index];
				m.SphereBlending = (PmxMaterial.SphereBlendingMode)r.ReadByte();
				m.ToonReference = (PmxMaterial.ToonReferenceType)r.ReadByte();
				if (m.ToonReference == PmxMaterial.ToonReferenceType.Internal)
				{
					index = r.ReadIndex(PmxTypes.IndexType.Texture);
					try
					{
						m.ToonTexturePath = index < 0 ? "" : Textures[index];
					}
					catch { }
				}
				else
				{
					m.ToonInternalIndex = r.ReadByte();
				}
				// Comment
				m.Note = r.ReadPmxString(TextEncoding);
				// Surfaces
				m.FirstVertex = assignedVertices;
				m.VertexCount = r.ReadInt32();
				assignedVertices += m.VertexCount;
				Materials.Add(m);


			}
			#endregion
			#region Bone data
			Bones = new List<PmxBone>(r.ReadInt32());
			for (int i = 0; i < Bones.Capacity; ++i)
			{
				PmxBone bone = r.ReadPmxBone(TextEncoding);
				bone.Index = Bones.Count;
				Bones.Add(bone);
			}
			#endregion
			#region Morph data
			Morphs = new List<PmxMorph>(r.ReadInt32());
			for (int i = 0; i < Morphs.Capacity; ++i)
			{
				Morphs.Add(r.ReadPmxMorph(TextEncoding));
			}
			#endregion
			// SKIP FROM HERE
			r.Close(); return;
			#region Frame data

			#endregion
			#region Rigidbody data

			#endregion
			#region Joint data

			#endregion
			#region Soft body data

			#endregion
		}
#pragma warning restore 0162

		// Manipulation
		/// <summary>
		/// Scale the imported model by the given factor.
		/// </summary>
		/// <param name="factor">The multiplier to scale by.</param>
		public void Resize(float factor)
		{
			foreach (PmxVertex v in Vertices)
			{
				v.Position *= factor;
			}
			foreach (PmxBone b in Bones)
			{
				b.Position *= factor;
				if (b.TailPosition != null)
					b.TailPosition *= factor;
			}
		}

		// Unity
		/// <summary>
		/// Convert the PMX model into Unity meshes with correct materials.
		/// </summary>
		/// <returns>A parent GameObject that holds the sub-meshes in its children.</returns>
		public GameObject GetGameObject()
		{
			bool cancel = false;

			GameObject root = new GameObject(Name);
			GameObject modelParent = new GameObject("Model");
			GameObject boneParent = new GameObject("Skeleton");

			Material baseOpaque = Resources.Load<Material>("Materials/DefaultMaterial");
			Material baseTransparent = Resources.Load<Material>("Materials/DefaultMaterialTransparent");
			Material baseFade = Resources.Load<Material>("Materials/DefaultMaterialFade");
			Material baseCutout = Resources.Load<Material>("Materials/DefaultMaterialCutout");
			GameObject boneSpritePrefab = Resources.Load<GameObject>("Prefabs/BoneSprite");

			// Model
			for (int i = 0; i < Materials.Count; ++i)
			{
				PmxMaterial mat = Materials[i];

				// MESH

				List<PmxVertex> vert = new List<PmxVertex>();   // List of vertices that make up the sub-model
				List<int> tri = new List<int>();                // Every index corresponds to an index in vert
				foreach (PmxTriangle t in mat.Triangles(Triangles))
				{
					vert.Add(Vertices[t.Vertex1]);
					tri.Add(vert.Count - 1);
					vert.Add(Vertices[t.Vertex2]);
					tri.Add(vert.Count - 1);
					vert.Add(Vertices[t.Vertex3]);
					tri.Add(vert.Count - 1);
				}

				Mesh mesh = new Mesh();
				mesh.name = mat.NameJapanese;
				mesh.vertices = PmxVertex.GetPositions(vert).ToArray();
				mesh.normals = PmxVertex.GetNormals(vert).ToArray();
				mesh.uv = PmxVertex.GetUVs(vert).ToArray();
				mesh.triangles = tri.ToArray();

				// MATERIAL

				// Texture
				Texture2D tex = null;
				if (!string.IsNullOrEmpty(mat.DiffuseTexturePath))
				{
					string path = Path.Combine(Path.GetDirectoryName(FilePath), mat.DiffuseTexturePath);
					if (File.Exists(path))
						tex = PmxMaterial.LoadTexture(path);
				}

				// Execute early rendering mode directives
				RenderModeDirective.RenderMode mode = RenderModeDirective.RenderMode.Opaque;
				foreach (RenderModeDirective dir in mat.Directives.OfType<RenderModeDirective>())
				{
					mode = dir.Mode;
					if (dir.AutoDetect)
						if (!PmxMaterial.IsTransparent(mat, tex, dir.Threshold))
							mode = RenderModeDirective.RenderMode.Opaque;
				}

				// Set up material

				Material material;
				switch (mode)
				{
					case RenderModeDirective.RenderMode.Cutout:
						material = Material.Instantiate<Material>(baseCutout);
						break;
					case RenderModeDirective.RenderMode.Fade:
						material = Material.Instantiate<Material>(baseFade);
						break;
					case RenderModeDirective.RenderMode.Transparent:
						material = Material.Instantiate<Material>(baseTransparent);
						break;
					default:
						material = Material.Instantiate<Material>(baseOpaque);
						break;
				}

				material.name = mat.NameJapanese;
				material.color = mat.DiffuseColor;
				if (tex != null)
					material.mainTexture = tex;

				// Execute directives
				foreach (MaterialDirective dir in mat.Directives)
				{
					dir.Execute(material);
				}

				// Set up GameObject and components
				GameObject o = new GameObject(string.Format("{0} ({1})", Name, mat.NameJapanese));
				o.AddComponent<MeshFilter>().sharedMesh = mesh;
				MeshRenderer renderer = o.AddComponent<MeshRenderer>();
				renderer.sharedMaterial = material;
				renderer.shadowCastingMode = mat.HasFlag(PmxMaterial.MaterialFlags.CastShadow | PmxMaterial.MaterialFlags.GroundShadow) ? ShadowCastingMode.On : ShadowCastingMode.Off;
				//renderer.receiveShadows = mat.HasFlag(PmxMaterial.MaterialFlags.ReceiveShadow);

				// Execute renderer directives
				foreach (MaterialDirective dir in mat.Directives)
				{
					dir.Execute(renderer);
				}

				o.transform.SetParent(modelParent.transform);
				if (cancel)
					break;
			}

			// Skeleton... recursion is recursion.
			foreach (PmxBone rootBone in PmxBone.RootBones(Bones))
			{
				GameObject rootBoneObject = BoneHierarchy.BuildHierarchy(rootBone, Bones, boneSpritePrefab);
				rootBoneObject.transform.SetParent(boneParent.transform);
				rootBoneObject.transform.position = rootBone.Position;
			}

			modelParent.transform.SetParent(root.transform);
			boneParent.transform.SetParent(root.transform);

			// Release loaded prefabs - source of memory leak? unneeded?
			//GameObject.Destroy(baseOpaque);
			//GameObject.Destroy(baseTransparent);
			//GameObject.Destroy(baseFade);
			//GameObject.Destroy(baseCutout);
			//GameObject.Destroy(boneSpritePrefab);

			if (cancel)
			{
				GameObject.Destroy(root);
				return null;
			}
			return root;
		}

		// Info
		/// <summary>
		/// Compiles a formatted string of human-readable information about the model.
		/// </summary>
		/// <param name="info">Information fields to include.</param>
		public string GetInfo(InfoField info)
		{
			StringBuilder sb = new StringBuilder("Showing info for PMX model\n\n");

			if ((info & InfoField.Header) != 0)
			{
				sb.AppendFormat("PMX version {0}\nName: {1} ({2})\n", Version, string.IsNullOrEmpty(NameEnglish) ? "<english name>" : NameEnglish, string.IsNullOrEmpty(NameJapanese) ? "<japanese name>" : NameJapanese)
					.AppendFormat("\nComments:\n{0}\n{1}\n\nGlobals:\n", string.IsNullOrEmpty(InfoEnglish) ? "<english info>" : InfoEnglish, string.IsNullOrEmpty(InfoJapanese) ? "<japanese info>" : InfoJapanese)
					.AppendFormat("  Text encoding:        {0}\n", TextEncoding.GetType().ToString())
					.AppendFormat("  Additional UV count:  {0}\n", AdditionalUVCount)
					.AppendFormat("Index sizes (byte)\n  Vertex index size:    {0}\n", PmxTypes.VertexIndex)
					.AppendFormat("  Texture index size:   {0}\n", PmxTypes.VertexIndex)
					.AppendFormat("  Material index size:  {0}\n", PmxTypes.VertexIndex)
					.AppendFormat("  Bone index size:      {0}\n", PmxTypes.VertexIndex)
					.AppendFormat("  Morph index size:     {0}\n", PmxTypes.VertexIndex)
					.AppendFormat("  Rigidbody index size: {0}\n\n", PmxTypes.VertexIndex);
			}

			if ((info & InfoField.Vertex) != 0)
			{
				sb.AppendFormat("Vertex info\n")
					.AppendFormat("Count: {0}\n\n", Vertices.Count);
			}

			if ((info & InfoField.Triangle) != 0)
			{
				sb.AppendFormat("Triangle info\n")
					.AppendFormat("Count: {0}\n\n", Triangles.Count);
			}

			if ((info & InfoField.Texture) != 0)
			{
				sb.AppendFormat("Texture table\n")
					.AppendFormat("Count: {0}\n", Textures.Count);
				foreach (string str in Textures)
					sb.AppendFormat("  {0}\n", str);
				sb.Append("\n");
			}

			if ((info & InfoField.Material) != 0)
			{
				sb.AppendFormat("Material data\n")
					.AppendFormat("Count: {0}\n", Materials.Count);
				foreach (PmxMaterial mat in Materials)
				{
					sb.Append(mat.ToString());
				}
				sb.Append("\n");
			}

			if ((info & InfoField.Bone) != 0)
			{
				sb.AppendFormat("Bone data\n")
					.AppendFormat("Count: {0}\n", Bones.Count);
				foreach (PmxBone bone in Bones)
				{
					sb.Append(bone.ToString());
				}
				sb.Append("\n");
			}

			if ((info & InfoField.Morph) != 0)
			{
				sb.Append("Morph data\n")
					.AppendFormat("Count: {0}\n", Morphs.Count);
				foreach (PmxMorph morph in Morphs)
				{
					sb.Append(morph.ToString());
				}
				sb.Append("\n");
			}

			return sb.ToString();
		}

		public void Dispose()
		{
			if (_reader != null)
				_reader.Close();
		}
	}
}
