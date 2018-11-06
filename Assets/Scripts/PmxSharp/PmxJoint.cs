using System;
using System.Collections.Generic;
using UnityEngine;

namespace PmxSharp
{
	/// <summary>
	/// Represents a PMX joint.
	/// </summary>
	public class PmxJoint : PmxNamedItem
	{
		public enum JointType { Spring6DOF, Rigid6DOF, P2P, Cone, Slider, Hinge }

		public JointType Type { get; set; }
		public int RigidbodyA { get; set; }
		public int RigidbodyB { get; set; }
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3 PositionMinimum { get; set; }
		public Vector3 PositionMaximum { get; set; }
		public Quaternion RotationMinimum { get; set; }
		public Quaternion RotationMaximum { get; set; }
		public Vector3 PositionSpring { get; set; }
		public Quaternion RotationSpring { get; set; }
	}
}
