using System;
using UnityEngine;

namespace PmxSharp
{
	/// <summary>
	/// Represents a PMX bone object.
	/// </summary>
	public class PmxBone
	{
		[Flags]
		public enum BoneFlags { TailIsIndex = 1, Rotation = 2, Translation = 4, Visible = 8, Enabled = 16, IK = 32, InheritRotation = 64, InheritTranslation = 128, FixedAxis = 256, LocalTransform = 512, PhysicsAfterDeform = 1024, ExternalDeform = 2048 }

		/// <summary>
		/// Primary name.
		/// </summary>
		public string NameJapanese { get; set; }
		/// <summary>
		/// Secondary name.
		/// </summary>
		public string NameEnglish { get; set; }
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
	}
}
