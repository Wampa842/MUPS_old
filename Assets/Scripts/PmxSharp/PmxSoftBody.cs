using System;
using UnityEngine;

namespace PmxSharp
{
	/// <summary>
	/// Represents a PMX 2.1 soft body. NOT SUPPORTED.
	/// </summary>
	public class PmxSoftBody : PmxNamedItem
	{
		public PmxSoftBody()
		{
			throw new NotImplementedException("Soft body is a PMX 2.1 feature, which is currently not supported.");
		}
	}
}
