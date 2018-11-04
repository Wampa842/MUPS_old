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
	public sealed class PmxImport : IDisposable
	{
		#region Check flags
		public static bool HasFlagsAll(int src, params int[] filters)
		{
			foreach (int filter in filters)
			{
				if ((src & filter) == 0)
					return false;
			}
			return true;
		}
		public static bool HasFlagsAny(int src, params int[] filters)
		{
			foreach (int filter in filters)
			{
				if ((src & filter) != 0)
					return true;
			}
			return false;
		}
		#endregion
		public enum PmxVersion { Unknown, Pmd, Pmx20, Pmx21 }

		private BinaryReader _reader;

		public PmxVersion Version { get; private set; }
		public Encoding TextEncoding { get; set; }
		public string FilePath { get; private set; }
		public string NameJapanese { get; set; }
		public string NameEnglish { get; set; }
		public string Name { get { return string.IsNullOrEmpty(NameEnglish) ? NameJapanese : NameEnglish; } }
		public string InfoJapanese { get; set; }
		public string InfoEnglish { get; set; }
		public byte AdditionalUVCount { get; private set; }

		public List<PmxVertex> Vertices { get; private set; }
		public List<PmxTriangle> Triangles { get; private set; }
		public List<string> Textures { get; private set; }
		public List<PmxMaterial> Materials { get; private set; }
		public List<PmxBone> Bones { get; private set; }

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
			if (!sig.ValidateSignature())
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
			r.Close(); return;
			#region Bone data
			Bones = new List<PmxBone>(r.ReadInt32());
			for (int i = 0; i < Bones.Capacity; ++i)
			{
				//PmxBone bone = new PmxBone(r.ReadPmxString(TextEncoding), r.ReadPmxString(TextEncoding));
				PmxBone bone = new PmxBone();
				r.ReadPmxString(TextEncoding);
				r.ReadPmxString(TextEncoding);

				bone.Position = r.ReadVector3();
				bone.Parent = r.ReadIndex(PmxTypes.IndexType.Bone);
				bone.Layer = r.ReadInt32();
				bone.Flags = (PmxBone.BoneFlags)r.ReadUInt16();
				// Tail
				if (HasFlagsAny((int)bone.Flags, (int)PmxBone.BoneFlags.TailIsIndex))
				{
					bone.TailIndex = r.ReadIndex(PmxTypes.IndexType.Bone);
				}
				else
				{
					bone.TailPosition = r.ReadVector3();
				}
				// Inherit
				if (HasFlagsAny((int)bone.Flags, (int)PmxBone.BoneFlags.InheritRotation, (int)PmxBone.BoneFlags.InheritTranslation))
				{
					bone.InheritFromIndex = r.ReadIndex(PmxTypes.IndexType.Bone);
					bone.InheritWeight = r.ReadSingle();
				}
				// Fixed axis
				if (HasFlagsAny((int)bone.Flags, (int)PmxBone.BoneFlags.FixedAxis))
				{
					bone.FixedAxis = r.ReadVector3();
				}
				// Local transformation
				if (HasFlagsAny((int)bone.Flags, (int)PmxBone.BoneFlags.LocalTransform))
				{
					bone.LocalX = r.ReadVector3();
					bone.LocalZ = r.ReadVector3();
				}
				// Outside parent
				if (HasFlagsAny((int)bone.Flags, (int)PmxBone.BoneFlags.ExternalDeform))
				{
					bone.ExternalParentIndex = r.ReadIndex(PmxTypes.IndexType.Bone);
				}
				// IK
				if (HasFlagsAny((int)bone.Flags, (int)PmxBone.BoneFlags.IK))
				{
					bone.IK = new PmxIK()
					{
						TargetIndex = r.ReadIndex(PmxTypes.IndexType.Bone),
						Loop = r.ReadInt32(),
						Limit = r.ReadSingle(),
						Links = new List<PmxIKLink>(r.ReadInt32())
					};
					for (int j = 0; j < bone.IK.Links.Capacity; ++j)
					{
						bone.IK.Links.Add(new PmxIKLink(r.ReadIndex(PmxTypes.IndexType.Bone), r.ReadByte() == 1, r.ReadVector3(), r.ReadVector3()));
					}
				}

				Bones.Add(bone);
			}
			#endregion
			// SKIP FROM HERE
			#region Morph data

			#endregion
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
		/// <summary>
		/// Formatted text displaying model information.
		/// </summary>
		public string Statistics
		{
			get
			{
				StringBuilder info = new StringBuilder()
					.AppendFormat("PMX MODEL\nVersion {0}, Encoding {1}, Additional UV count {2}\n\n{3} ({4})\n", Version.ToString(), TextEncoding.GetType().ToString(), AdditionalUVCount, NameJapanese, NameEnglish)
					.AppendFormat("Note (JP):\n{0}\nNote (EN)\n{1}\n\n", InfoJapanese, InfoEnglish)
					.AppendFormat("Vertices: {0}\n", Vertices.Count)
					.AppendFormat("Triangles: {0}\n", Triangles.Count)
					.AppendFormat("Textures: {0}\n", Textures.Count);
				foreach (string tex in Textures)
				{
					info.AppendFormat(" > {0}\n", tex);
				}
				info.AppendFormat("Materials: {0}\n", Materials.Count);
				foreach (PmxMaterial mat in Materials)
				{
					info.AppendFormat(" > {0} ({1})\n", mat.NameJapanese, mat.NameEnglish);
				}
				info.AppendFormat("Bones: {0}\n", Bones.Count);
				foreach (PmxBone b in Bones)
				{
					info.AppendFormat(" > {0} ({1})\n", b.NameJapanese, b.NameEnglish);
				}

				return info.ToString();
			}
		}

		/// <summary>
		/// Convert the PMX model into a single Unity mesh with a single material.
		/// </summary>
		/// <returns>GameObject that holds the Mesh and a MeshRenderer.</returns>
		public GameObject FullMesh()
		{
			GameObject o = new GameObject(string.IsNullOrEmpty(NameEnglish) ? NameJapanese : NameEnglish);
			Mesh mesh = new Mesh();

			mesh.vertices = PmxVertex.GetPositions(Vertices).ToArray();
			mesh.normals = PmxVertex.GetNormals(Vertices).ToArray();
			mesh.uv = PmxVertex.GetUVs(Vertices).ToArray();
			mesh.triangles = PmxTriangle.GetVertices(Triangles).ToArray();

			o.AddComponent<MeshFilter>().sharedMesh = mesh;
			o.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
			return o;
		}

		/// <summary>
		/// Convert the PMX model into Unity meshes with correct materials.
		/// </summary>
		/// <returns>A parent GameObject that holds the sub-meshes in its children.</returns>
		public GameObject GetGameObject()
		{
			bool cancel = false;

			GameObject root = new GameObject(Name);
			GameObject parent = new GameObject("Model");
			Material baseOpaque = Resources.Load<Material>("Materials/DefaultMaterial");
			Material baseTransparent = Resources.Load<Material>("Materials/DefaultMaterialTransparent");
			Material baseFade = Resources.Load<Material>("Materials/DefaultMaterialFade");
			Material baseCutout = Resources.Load<Material>("Materials/DefaultMaterialCutout");

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
					string path = string.Empty;
					try
					{
						path = Path.Combine(Path.GetDirectoryName(FilePath), mat.DiffuseTexturePath);
						tex = PmxMaterial.LoadTexture(path);
					}
					catch (Exception ex)
					{
						//MUPS.UI.Modal.Show(Screen.width - 200, Screen.height - 200, ex.Message, ex.ToString(), new MUPS.UI.ButtonDescriptor("Dismiss"), new MUPS.UI.ButtonDescriptor("Cancel loading", MUPS.UI.ButtonDescriptor.ColorPresets.Red, () => { cancel = true; }));
						MUPS.UI.Modal.Show(Screen.width - 200, Screen.height - 200, ex.Message, ex.ToString(), new MUPS.UI.ButtonDescriptor("OK"));
					}
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

				o.transform.SetParent(parent.transform);
				if (cancel)
					break;
			}

			parent.transform.SetParent(root.transform);
			if (cancel)
			{
				GameObject.Destroy(root);
				return null;
			}
			return root;
		}

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
		}

		public void Dispose()
		{
			if (_reader != null)
				_reader.Close();
		}
	}
}
