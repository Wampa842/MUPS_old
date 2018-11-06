using MUPS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PmxSharp
{
	public class BoneHierarchy
	{
		public static GameObject BuildHierarchy(PmxBone bone, IEnumerable<PmxBone> coll, GameObject sprite)
		{
			GameObject root = GameObject.Instantiate<GameObject>(sprite);

			BoneSpriteBehaviour comp = root.GetComponent<BoneSpriteBehaviour>();

			if (!bone.HasFlag(PmxBone.BoneFlags.Visible))
				comp.Icon = BoneSpriteBehaviour.IconType.Invisible;
			else if (bone.HasFlag(PmxBone.BoneFlags.FixedAxis))
				comp.Icon = BoneSpriteBehaviour.IconType.Twist;
			else if (bone.HasFlag(PmxBone.BoneFlags.Translation))
				comp.Icon = BoneSpriteBehaviour.IconType.Translation;

			root.layer = LayerMask.NameToLayer("UISprites");
			root.name = bone.Name;
			PmxBone[] children = bone.Children(coll);
			for(int i = 0; i < children.Length; ++i)
			{
				GameObject child = BuildHierarchy(children[i], coll, sprite);
				child.transform.SetParent(root.transform, true);
				child.transform.position = children[i].Position - bone.Position;
			}
			return root;
		}
	}

	/// <summary>
	/// Represents a PMX bone object.
	/// </summary>
	public class PmxBone : PmxNamedItem
	{
		[Flags]
		public enum BoneFlags { TailIsIndex = 1, Rotation = 2, Translation = 4, Visible = 8, Enabled = 16, IK = 32, InheritRotation = 256, InheritTranslation = 512, FixedAxis = 1024, LocalTransform = 2048, PhysicsAfterDeform = 4096, ExternalDeform = 8192 }

		public bool HasFlag(BoneFlags flags)
		{
			return (Flags & flags) != 0;
		}

		/// <summary>
		/// The index of the bone.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// The bone's position.
		/// </summary>
		public Vector3 Position { get; set; }
		
		/// <summary>
		/// The parent bone's index.
		/// </summary>
		public int Parent { get; set; }
		
		/// <summary>
		/// The bone's position in the deformation order.
		/// </summary>
		public int Layer { get; set; }
		
		/// <summary>
		/// The bone's flags.
		/// </summary>
		public BoneFlags Flags { get; set; }
		
		/// <summary>
		/// If the TailIsIndex flag is not set, specifies the tail's position.
		/// </summary>
		public Vector3 TailPosition { get; set; }
		
		/// <summary>
		/// If the TailIsIndex flag is set, specifies the tail bone's position.
		/// </summary>
		public int TailIndex { get; set; }
		
		/// <summary>
		/// If an Inherit flag is set, specifies which bone it inherits from.
		/// </summary>
		public int InheritFromIndex { get; set; }
		
		/// <summary>
		/// If an Inherit flag is set, specifies the inherited transformation's scale.
		/// </summary>
		public float InheritWeight { get; set; }
		
		/// <summary>
		/// If the FixedAxis flag is set, specifies the direction vector of the axis. Unit vector.
		/// </summary>
		public Vector3 FixedAxis { get; set; }
		
		/// <summary>
		/// If the LocalTransform flag is set, specifies the direction of the local X axis. Unit vector.
		/// </summary>
		public Vector3 LocalX { get; set; }
		
		/// <summary>
		/// If the LocalTransform flag is set, specifies the direction of the local Z axis. Unit vector.
		/// </summary>
		public Vector3 LocalZ { get; set; }
		
		/// <summary>
		/// If the ExternalDeform flag is set, specifies the deform parent bone.
		/// </summary>
		public int ExternalParentIndex { get; set; }
		
		/// <summary>
		/// If the IK flag is set, specifies the properties of the IK chain.
		/// </summary>
		public PmxIK IK { get; set; }

		/// <summary>
		/// Create an empty bone with unique random names.
		/// </summary>
		public PmxBone()
		{
			NameJapanese = string.Format("Bone {0}", GetHashCode());
			NameEnglish = string.Format("Bone {0}", GetHashCode());
			Parent = -1;
		}

		/// <summary>
		/// Create an empty bone with the specified names.
		/// </summary>
		/// <param name="jp">Japanese name.</param>
		/// <param name="en">English name.</param>
		/// <param name="parent">Parent bone index.</param>
		public PmxBone(string jp, string en, int parent = -1)
		{
			NameJapanese = jp;
			NameEnglish = en;
			Parent = parent;
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder("  Bone\n");

			sb.AppendFormat("    Name: {0} ({1})\n", NameEnglish, NameJapanese)
				.AppendFormat("    Index:  {0}\n    Parent: {1}\n", "??", Parent)
				.AppendFormat("    Position: {0}\n", Position);

			if ((Flags & BoneFlags.TailIsIndex) != 0)
				sb.AppendFormat("    Tail index: {0}\n", TailIndex);
			else
				sb.AppendFormat("    Tail position: {0}\n", TailPosition);

			sb.AppendFormat("    Layer: {0}\n    Flags: {1}\n", Layer, Flags);

			if(HasFlag(BoneFlags.InheritRotation | BoneFlags.InheritTranslation))
			{
				sb.AppendFormat("    Inherit bone\n");
				if (HasFlag(BoneFlags.InheritRotation))
					sb.AppendFormat("      Rotation\n");
				if (HasFlag(BoneFlags.InheritTranslation))
					sb.AppendFormat("      Translation\n");
				sb.AppendFormat("      Bone: {0}\n      Weight: {1:0.000}\n", InheritFromIndex, InheritWeight);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Determines whether the bone has any children in the given collection.
		/// </summary>
		public bool HasChildren(IEnumerable<PmxBone> coll)
		{
			return coll.Any(e => e.Parent == Index);
		}

		/// <summary>
		/// Finds the bone's immediate children in the given collection.
		/// </summary>
		public PmxBone[] Children(IEnumerable<PmxBone> coll)
		{
			return coll.Where(e => e.Parent == Index).ToArray();
		}

		/// <summary>
		/// Finds all root bones (parent is -1) in the given collection.
		/// </summary>
		public static PmxBone[] RootBones(IEnumerable<PmxBone> coll)
		{
			return coll.Where(e => e.Parent < 0).ToArray();
		}
	}
}
