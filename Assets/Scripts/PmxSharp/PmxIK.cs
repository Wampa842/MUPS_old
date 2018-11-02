using System.Collections.Generic;
using UnityEngine;

namespace PmxSharp
{
	/// <summary>
	/// Represents an IK link.
	/// </summary>
	public class PmxIKLink
	{
		/// <summary>
		/// The index of the target bone.
		/// </summary>
		public int Index { get; set; }
		/// <summary>
		/// Indicates whether the IK link has limited angles.
		/// </summary>
		public bool HasLimit { get; set; }
		/// <summary>
		/// The low angle limits in radians.
		/// </summary>
		public Vector3 EulerLimitMin { get; set; }
		/// <summary>
		/// The high angle limits in radians.
		/// </summary>
		public Vector3 EulerLimitMax { get; set; }

		/// <summary>
		/// Create an empty IK link
		/// </summary>
		public PmxIKLink()
		{
			Index = -1;
			HasLimit = false;
			EulerLimitMax = new Vector3();
			EulerLimitMin = new Vector3();
		}

		/// <summary>
		/// Create an IK link with the specified properties.
		/// </summary>
		/// <param name="index">The target bone's index.</param>
		/// <param name="hasLimit">Specifies whether to use angle limits.</param>
		/// <param name="min">Low angle limit (radians).</param>
		/// <param name="max">High angle limit (radians).</param>
		public PmxIKLink(int index, bool hasLimit, Vector3 min, Vector3 max)
		{
			this.Index = index;
			this.HasLimit = hasLimit;
			this.EulerLimitMin = min;
			this.EulerLimitMax = max;
		}
	}

	/// <summary>
	/// Represents an IK controller bone.
	/// </summary>
	public class PmxIK
	{
		/// <summary>
		/// Index of the IK chain's end bone.
		/// </summary>
		public int TargetIndex { get; set; }
		/// <summary>
		/// Somehow affects the IK chain's behaviour.
		/// </summary>
		public int Loop { get; set; }
		/// <summary>
		/// Somehow affects the IK chain's behaviour.
		/// </summary>
		public float Limit { get; set; }
		/// <summary>
		/// The list of IK links in this chain.
		/// </summary>
		public List<PmxIKLink> Links { get; set; }
	}
}
