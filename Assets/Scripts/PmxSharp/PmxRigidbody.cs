using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PmxSharp
{
	/// <summary>
	/// Represents a rigidbody.
	/// </summary>
	public class PmxRigidbody : PmxNamedItem
	{
		public enum PrimitiveType { Sphere, Box, Capsule }
		public enum PhysicsType { Kinematic, Dynamic, LocalDynamic }

		public int Bone { get; set; }
		public byte Group { get; set; }
		public bool[] NoClip { get; set; }
		public PrimitiveType Shape { get; set; }
		public Vector3 Size { get; set; }
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public float Mass { get; set; }
		public float DampenMove { get; set; }
		public float DampenRotation { get; set; }
		public float Repulsion { get; set; }
		public float Friction { get; set; }
		public PhysicsType Physics { get; set; }
	}
}
