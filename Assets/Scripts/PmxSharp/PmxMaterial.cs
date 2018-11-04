using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

namespace PmxSharp
{
	/// <summary>
	/// Methods for determining texture information.
	/// </summary>
	public static class TextureInfo
	{
		public enum TextureFileFormat { Unknown, PNG, JPG, TGA, DDS, GIF, BMP, EXR, HDR }
		private static Dictionary<TextureFileFormat, byte[]> ImageFileHeaders;
		private static byte[] TgaFooterSignature;
		static TextureInfo()
		{
			ImageFileHeaders = new Dictionary<TextureFileFormat, byte[]>
			{
				{ TextureFileFormat.PNG, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
				{ TextureFileFormat.JPG, new byte[] { 0xFF, 0xD8 } },
				{ TextureFileFormat.TGA, new byte[] { 0x00 } },   // fuck TGA
				{ TextureFileFormat.DDS, new byte[] { 0x44, 0x44, 0x53, 0x20 } },
				{ TextureFileFormat.GIF, new byte[] { 0x47, 0x49, 0x46, } },
				{ TextureFileFormat.BMP, new byte[] { 0x42, 0x4D } },
				{ TextureFileFormat.EXR, new byte[] { 0x76, 0x2F, 0x31, 0x01 } },
				{ TextureFileFormat.HDR, new byte[] { 0x23, 0x3F, 0x52, 0x41, 0x44, 0x49, 0x41, 0x4E, 0x43, 0x45, 0x0A } }
			};

			TgaFooterSignature = new byte[] { 0x54, 0x52, 0x55, 0x45, 0x56, 0x49, 0x53, 0x49, 0x4F, 0x4E, 0x2D, 0x58, 0x46, 0x49, 0x4C, 0x45, 0x2E, 0x00 }; // fuck TGA
		}

		/// <summary>
		/// Detects the file format of an image from its signature.
		/// </summary>
		/// <param name="data">The contents of the image file.</param>
		/// <returns>The detected format.</returns>
		public static TextureFileFormat DetectFormat(byte[] data)
		{
			TextureFileFormat format = TextureFileFormat.Unknown;

			foreach (KeyValuePair<TextureFileFormat, byte[]> kvp in ImageFileHeaders)
			{
				if (data.Take(kvp.Value.Length).SequenceEqual(kvp.Value))
				{
					format = kvp.Key;
				}
			}

			if (format == TextureFileFormat.TGA)
			{
				// No, seriously, FUCK TGA
				if (!data.Skip(data.Length - TgaFooterSignature.Length).SequenceEqual(TgaFooterSignature))
					return TextureFileFormat.Unknown;
			}

			return format;
		}
	}

	#region Directives
	/// <summary>
	/// Base class for text-based material directives.
	/// </summary>
	public abstract class MaterialDirective
	{
		/// <summary>
		/// When overridden in a derived class, this method is executed after the material has been created.
		/// </summary>
		/// <param name="material">The material to execute the directive on.</param>
		public virtual void Execute(Material material) { }

		/// <summary>
		/// When overridden in a derived class, this method is executed before the material is created.
		/// </summary>
		/// <param name="material">The material to execute the directive on.</param>
		public virtual void ExecuteEarly(Material material) { }

		/// <summary>
		/// When overridden in a derived class, this method is executed after the renderer has been created.
		/// </summary>
		/// <param name="renderer">The renderer to execute the directive on.</param>
		public virtual void Execute(Renderer renderer) { }

		/// <summary>
		/// The raw content of the directive.
		/// </summary>
		public string DirectiveString { get; private set; }

		public MaterialDirective(string raw)
		{
			DirectiveString = raw;
		}
	}

	/// <summary>
	/// Directive to set a property of the material to a predefined value.
	/// </summary>
	public sealed class SetValueDirective : MaterialDirective
	{
		public enum PropertyType { Integer, Float, Color, Texture, Keyword }
		private PropertyType _type;
		private object _value;
		private string _name;

		public static PropertyType ParsePropertyType(string s)
		{
			switch (s.ToLower())
			{
				case "float":
					return PropertyType.Float;
				case "int":
					return PropertyType.Integer;
				case "color":
					return PropertyType.Color;
				case "texture":
					return PropertyType.Texture;
				case "keyword":
					return PropertyType.Keyword;
				default:
					throw new MaterialDirectiveException(string.Format("{0} is not a valid type name.", s));
			}
		}

		public override void Execute(Material material)
		{
			try
			{
				switch (_type)
				{
					case PropertyType.Integer:
						material.SetInt(_name, (int)_value);
						break;
					case PropertyType.Float:
						material.SetFloat(_name, (float)_value);
						break;
					case PropertyType.Color:
						material.SetColor(_name, (Color)_value);
						break;
					case PropertyType.Texture:
						material.SetTexture(_name, (Texture)_value);
						break;
					case PropertyType.Keyword:
						switch (_name.ToLowerInvariant())
						{
							case "opacity":
								material.color = new Color(material.color.r, material.color.g, material.color.b, (float)_value);
								break;
						}
						break;
				}
			}
			catch (Exception ex)
			{
				throw new InvalidCastException(string.Format("Type mis-match. Expected {0}, but value is of type {1}.", _type.ToString(), _value.GetType().ToString()), ex);
			}
		}

		public SetValueDirective(string raw, PropertyType type, string name, object value) : base(raw)
		{
			_type = type;
			_value = value;
			_name = name;
		}
	}

	/// <summary>
	/// Directive to copy a value from the PMX material to a property of a Unity material.
	/// </summary>
	public sealed class CopyValueDirective : MaterialDirective
	{
		private static readonly HashSet<string> _keywords = new HashSet<string> { "diffuse", "ambient", "emissive", "specular", "smoothness", "exponent", "roughness", "edge", "edgesize", "diffusepath", "diffusetex", "spherepath", "spheretex", "sphereblend", "toonindex", "toonpath", "toontex" };
		private PmxMaterial _source;
		private string _name;
		private string _dest;
		public override void Execute(Material material)
		{
			switch (_name)
			{
				case "diffuse":
					material.SetColor(_dest, _source.DiffuseColor);
					break;
				case "ambient":
					material.SetColor(_dest, _source.AmbientColor);
					break;
				case "emissive":
					material.SetColor(_dest, _source.AmbientColor);
					break;
				case "specular":
					material.SetColor(_dest, _source.SpecularColor);
					break;
				case "smoothness":
					material.SetFloat(_dest, _source.SpecularExponent);
					break;
				case "exponent":
					material.SetFloat(_dest, _source.SpecularExponent);
					break;
				case "roughness":
					material.SetFloat(_dest, 1 - _source.SpecularExponent);
					break;
				case "edge":
					material.SetColor(_dest, _source.EdgeColor);
					break;
				case "edgesize":
					material.SetFloat(_dest, _source.EdgeSize);
					break;
				case "diffusetex":
					if (string.IsNullOrEmpty(_source.DiffuseTexturePath))
						break;
					material.SetTexture(_dest, PmxMaterial.LoadTexture(_source.DiffuseTexturePath));
					break;
				case "spheretex":
					if (string.IsNullOrEmpty(_source.SphereTexturePath))
						break;
					material.SetTexture(_dest, PmxMaterial.LoadTexture(_source.SphereTexturePath));
					break;
				case "sphereblend":
					material.SetInt(_dest, (int)_source.SphereBlending);
					break;
				case "toonindex":
					if (_source.ToonReference == PmxMaterial.ToonReferenceType.Internal)
						material.SetInt(_dest, _source.ToonInternalIndex);
					break;
				case "toontex":
					if (_source.ToonReference == PmxMaterial.ToonReferenceType.Texture)
						material.SetTexture(_dest, PmxMaterial.LoadTexture(_source.ToonTexturePath));
					break;
			}
		}

		public CopyValueDirective(string raw, PmxMaterial source, string name, string dest) : base(raw)
		{
			if (!_keywords.Contains(name))
				throw new ArgumentException(string.Format("{0} is not a valid keyword.", name), "name");
			_name = name;
			_dest = dest;
			_source = source;
		}
	}

	/// <summary>
	/// Directive to enable or disable a specified keyword.
	/// </summary>
	public sealed class KeywordDirective : MaterialDirective
	{
		string _keyword;
		bool _enable;
		public override void Execute(Material material)
		{
			if (_enable)
				material.EnableKeyword(_keyword);
			else
				material.DisableKeyword(_keyword);
		}

		public KeywordDirective(string raw, string action, string keyword) : base(raw)
		{
			_keyword = keyword;
			_enable = action.ToLowerInvariant() == "enable" || action.ToLowerInvariant() == "on";
		}
	}

	/// <summary>
	/// Directive for setting the material's render queue position.
	/// </summary>
	public sealed class QueueDirective : MaterialDirective
	{
		private int _queue;
		public override void Execute(Material material)
		{
			material.renderQueue = _queue;
		}

		public QueueDirective(string raw, int queue) : base(raw)
		{
			_queue = queue;
		}
	}

	/// <summary>
	/// Directive that overrides how the importer selects the material preset.
	/// </summary>
	public sealed class RenderModeDirective : MaterialDirective
	{
		public enum RenderMode { Opaque, Cutout, Fade, Transparent }
		public RenderMode Mode { get; }
		public bool AutoDetect { get; }
		public float Threshold { get; }

		public RenderModeDirective(string raw, string mode, bool autoDetect, float threshold = 1.0f) : base(raw)
		{
			string m = mode.ToLowerInvariant();
			if (autoDetect && m == "opaque") throw new ArgumentException("Autodetect cannot be used if the defined render mode is opaque.", "mode");
			switch (m)
			{
				case "opaque":
					Mode = RenderMode.Opaque;
					break;
				case "cutout":
					Mode = RenderMode.Cutout;
					break;
				case "fade":
					Mode = RenderMode.Fade;
					break;
				case "transparent":
					Mode = RenderMode.Transparent;
					break;
				default:
					throw new ArgumentException(string.Format("{0} is not a valid render mode.", mode), "mode");
			}
			AutoDetect = autoDetect;
			Threshold = threshold;
		}
	}

	/// <summary>
	/// Directive that overrides the shadow casting and receiving behaviour of the material's renderer component.
	/// </summary>
	public sealed class ShadowModeDirective : MaterialDirective
	{
		public enum ShadowOperation { Cast, Receive }
		public ShadowOperation Operation { get; }
		public bool Detect { get; }
		public ShadowCastingMode Mode { get; }
		public ShadowModeDirective(string raw, string operation, string mode, PmxMaterial.MaterialFlags flags) : base(raw)
		{
			Detect = false;
			switch (operation.ToLower())
			{
				case "cast":
					Operation = ShadowOperation.Cast;
					switch (mode.ToLower())
					{
						case "off":
							Mode = ShadowCastingMode.Off;
							break;
						case "on":
							Mode = ShadowCastingMode.On;
							break;
						case "double":
							Mode = ShadowCastingMode.TwoSided;
							break;
						case "doubleauto":
							Mode = ((flags & PmxMaterial.MaterialFlags.CastShadow) != 0) ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
							break;
						case "shadowonly":
							Mode = ShadowCastingMode.ShadowsOnly;
							break;
					}
					break;
				case "receive":
					Operation = ShadowOperation.Receive;
					switch (mode.ToLower())
					{
						case "on":
							Mode = ShadowCastingMode.On;
							break;
						case "off":
							Mode = ShadowCastingMode.Off;
							break;
					}
					break;
				default: throw new ArgumentException(string.Format("{0} is not a valid shadow operation.", operation), "operation");
			}
		}
		public override void Execute(Renderer renderer)
		{
			if (Operation == ShadowOperation.Cast)
				renderer.shadowCastingMode = Mode;
			else
				renderer.receiveShadows = Mode == ShadowCastingMode.On;
		}
	}
	#endregion

	/// <summary>
	/// Represents a PMX material.
	/// </summary>
	public class PmxMaterial
	{
		[Flags]
		public enum MaterialFlags { TwoSided = 1, GroundShadow = 2, CastShadow = 4, ReceiveShadow = 8, Edge = 16, VertexColor = 32, DrawPoints = 64, DrawLines = 128 }
		public enum SphereBlendingMode { Disabled = 0, Additive = 1, Multiplicative = 2, SubTexture = 3 }
		public enum ToonReferenceType { Texture = 0, Internal = 1 }


		/// <summary>
		/// Load an image file as a texture.
		/// </summary>
		/// <param name="path">The path to the image file.</param>
		/// <returns>The bitmap as a Unity texture.</returns>
		public static Texture2D LoadTexture(string path)
		{
			Texture2D tex = new Texture2D(2, 2);
			byte[] data = File.ReadAllBytes(path);
			TextureInfo.TextureFileFormat format = TextureInfo.DetectFormat(data);
			switch (format)
			{
				case TextureInfo.TextureFileFormat.PNG:
					tex.LoadImage(data);
					break;
				case TextureInfo.TextureFileFormat.JPG:
					tex.LoadImage(data);
					break;
				case TextureInfo.TextureFileFormat.TGA:
					try
					{
						tex = TGALoader.LoadTGA(new MemoryStream(data));
					}
					catch (EndOfStreamException ex)
					{
						throw new NotImplementedException(string.Format("Tried reading after end of stream. The file is probably compressed, which is not currently supported. Path: {0}.", path), ex);
					}
					break;
				case TextureInfo.TextureFileFormat.Unknown:
					throw new FormatException(string.Format("Loaded file {0} is not recognised as a valid image format.", path));
				default:
					throw new NotImplementedException(string.Format("The {0} image format is currently not supported (file: {1}).", format, path));
			}

			return tex;
		}
		/// <summary>
		/// Determines whether the specified material should be treated as transparent or opaque.
		/// </summary>
		/// <param name="material">The material in question.</param>
		/// <param name="texture">The material's diffuse texture, if any.</param>
		/// <param name="threshold">The threshold below which a color is considered transparent.</param>
		/// <returns>True if the diffuse color, or at least one pixel in the diffuse texture, is transparent. False otherwise.</returns>
		public static bool IsTransparent(PmxMaterial material, Texture2D texture = null, float threshold = 1.0f)
		{
			if (material.DiffuseColor.a < threshold)
				return true;
			if (texture != null)
			{
				Color[] pixels = texture.GetPixels();
				for (int i = 0; i < pixels.Length; ++i)
				{
					if (pixels[i].a < threshold)
						return true;
				}
			}
			return false;
		}

		#region Properties
		/// <summary>
		/// Material drawing flags.
		/// </summary>
		public MaterialFlags Flags { get; set; }
		/// <summary>
		/// Primary name.
		/// </summary>
		public string NameJapanese { get; set; }
		/// <summary>
		/// Secondary name.
		/// </summary>
		public string NameEnglish { get; set; }
		/// <summary>
		/// Diffuse RGBA color.
		/// </summary>
		public Color DiffuseColor { get; set; }
		/// <summary>
		/// Specular RGB color.
		/// </summary>
		public Color SpecularColor { get; set; }
		/// <summary>
		/// Specular exponent (glossiness, shine).
		/// </summary>
		public float SpecularExponent { get; set; }
		/// <summary>
		/// Ambient/emissive RGB color.
		/// </summary>
		public Color AmbientColor { get; set; }
		/// <summary>
		/// Pencil edge RGB color.
		/// </summary>
		public Color EdgeColor { get; set; }
		/// <summary>
		/// Edge size in pixels.
		/// </summary>
		public float EdgeSize { get; set; }
		/// <summary>
		/// Diffuse texture path. Absolute, or relative to the PMx file.
		/// </summary>
		public string DiffuseTexturePath { get; set; }
		/// <summary>
		/// Sphere map texture path. Absolute, or relative to the PMx file.
		/// </summary>
		public string SphereTexturePath { get; set; }
		/// <summary>
		/// Sphere map blending mode.
		/// </summary>
		public SphereBlendingMode SphereBlending { get; set; }
		/// <summary>
		/// Toon reference type.
		/// </summary>
		public ToonReferenceType ToonReference { get; set; }
		/// <summary>
		/// Index of the internal toon texture (0-9).
		/// </summary>
		public int ToonInternalIndex { get; set; }
		/// <summary>
		/// Path to the external toon texture. Absolute, or relative to the PMx file.
		/// </summary>
		public string ToonTexturePath { get; set; }
		private string _note;
		/// <summary>
		/// Material note content, including directives.
		/// </summary>
		public string Note
		{
			get
			{
				return _note;
			}
			set
			{
				_note = value;
				ReadDirectives();
			}
		}
		/// <summary>
		/// Determines whether at least one of the specified flags is set in the material.
		/// </summary>
		/// <param name="flags">The flags to look for.</param>
		/// <returns></returns>
		public bool HasFlag(MaterialFlags flag)
		{
			return (Flags & flag) != 0;
		}
		#endregion
		#region Directives
		public MaterialDirective[] Directives { get; private set; }
		private void ReadDirectives()
		{
			// Do not process the note if [begin] is not found.
			if (!_note.ToLower().Contains("[begin]"))
			{
				Directives = new MaterialDirective[0];
				return;
			}

			List<MaterialDirective> list = new List<MaterialDirective>();

			// Find the block's beginning and begin processing.
			string block = Regex.Match(_note, @"\[begin\]([\s\S]*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ECMAScript).Groups[1].Value;
			foreach (Match match in Regex.Matches(block, @"\[(.*?)\]", RegexOptions.IgnoreCase))
			{
				string dir = match.Groups[1].Value;

				// If [end] is reached, finish processing.
				if (dir.ToLower() == "end")
				{
					Directives = list.ToArray();
					return;
				}
				// Throw an exception if two opening brackets follow each other.
				if (dir.Contains("["))
					throw new MaterialDirectiveException(string.Format("Expected closing bracket (]) before opening bracket ([) in directive {0}", dir), this, dir);

				string[] split = dir.Split(' ');
				string name = split[0];
				string[] args = split.Skip(1).ToArray();

				switch (name.ToLower())
				{
					case "set":
						if (args.Length < 2)
							throw new MaterialDirectiveException(string.Format("Invalid number of parameters. Expected at least 2, got {0}.", args.Length), this, dir);
						SetValueDirective.PropertyType type = SetValueDirective.ParsePropertyType(args[0]);
						object value = null;
						switch (type)
						{
							case SetValueDirective.PropertyType.Integer:
								value = int.Parse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
								break;
							case SetValueDirective.PropertyType.Float:
								value = float.Parse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture);
								break;
							case SetValueDirective.PropertyType.Color:
								float r = float.Parse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture);
								float g = float.Parse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture);
								float b = float.Parse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture);
								float a = args.Length > 5 ? float.Parse(args[5], NumberStyles.Float, CultureInfo.InvariantCulture) : 1.0f;
								value = new Color(r, g, b, a);
								break;
							case SetValueDirective.PropertyType.Texture:
								value = string.Join(" ", args.Skip(2).ToArray());
								break;
							case SetValueDirective.PropertyType.Keyword:
								break;
						}
						list.Add(new SetValueDirective(dir, type, args[1], value));
						break;
					case "copy":
						if (args.Length < 2)
							throw new MaterialDirectiveException(string.Format("Invalid number of parameters. Expected 2, got {0}.", args.Length), this, dir);
						list.Add(new CopyValueDirective(dir, this, args[0], args[1]));
						break;
					case "keyword":
						if (args.Length < 2)
							throw new MaterialDirectiveException(string.Format("Invalid number of parameters. Expected 2, got {0}.", args.Length), this, dir);
						list.Add(new KeywordDirective(dir, args[0], args[1]));
						break;
					case "rename":
						break;
					case "delete":
						break;
					case "visible":
						break;
					case "shadow":
						if (args.Length < 2)
							throw new MaterialDirectiveException(string.Format("Invalid number of parameters. Expected 2, got {0}.", args.Length), this, dir);
						list.Add(new ShadowModeDirective(dir, args[0], args[1], Flags));
						break;
					case "queue":
						if (args.Length < 1)
							throw new MaterialDirectiveException(string.Format("Invalid number of parameters. Expected 1, got {0}.", args.Length), this, dir);
						list.Add(new QueueDirective(dir, int.Parse(args[0])));
						break;
					case "rendermode":
						if (args.Length < 1)
							throw new MaterialDirectiveException(string.Format("Invalid number of parameters. Expected 1, got {0}.", args.Length), this, dir);
						float threshold = 1.0f;
						foreach (string arg in args)
						{
							if (float.TryParse(arg, out threshold))
								break;
						}

						list.Add(new RenderModeDirective(dir, args[0], Array.Exists(args, e => e.ToLower() == "auto"), threshold));
						break;
					default:
						throw new MaterialDirectiveException(string.Format("Unknown directive name: {0}.", name), this, dir);
				}

				Directives = list.ToArray();
			}
		}
		#endregion
		#region Textures
		private Texture2D _diffuseTexture;
		private Texture2D _sphereTexture;
		private Texture2D _toonTexture;
		public Texture2D DiffuseTexture;
		#endregion
		#region Mesh connection
		/// <summary>
		/// The number of vertices assigned to the material.
		/// </summary>
		public int VertexCount { get; set; }
		/// <summary>
		/// The index of the first vertex assigned to the material.
		/// </summary>
		public int FirstVertex { get; set; }
		/// <summary>
		/// The index of the last vertex assigned to the material.
		/// </summary>
		public int LastVertex { get { return FirstVertex + VertexCount - 1; } }
		/// <summary>
		/// The number of triangles assigned to the material.
		/// </summary>
		public int TriangleCount { get { return VertexCount / 3; } }
		/// <summary>
		/// The index of the first triangle assigned to the material.
		/// </summary>
		public int FirstTriangle { get { return FirstVertex / 3; } }
		/// <summary>
		/// The index of the last triangle assigned to the material.
		/// </summary>
		public int LastTriangle { get { return FirstTriangle + TriangleCount - 1; } }
		/// <summary>
		/// Returns the array of triangles that belong to this material from a collection of all triangles.
		/// </summary>
		/// <param name="coll">Collection that holds all of the model's triangles.</param>
		/// <returns>Array of triangles that belong to the material.</returns>
		public PmxTriangle[] Triangles(IEnumerable<PmxTriangle> coll)
		{
			return coll.Skip(FirstTriangle).Take(TriangleCount).ToArray();
		}
		#endregion
		#region Constructors
		/// <summary>
		/// Create an empty material with unique random names.
		/// </summary>
		public PmxMaterial()
		{
			NameJapanese = string.Format("Material {0}", GetHashCode());
			NameEnglish = string.Format("Material {0}", GetHashCode());
		}
		/// <summary>
		/// Create an empty material with the given names.
		/// </summary>
		/// <param name="jp">Japanese name.</param>
		/// <param name="en">English name.</param>
		public PmxMaterial(string jp, string en)
		{
			NameJapanese = jp;
			NameEnglish = en;
		}
		#endregion
	}
}
