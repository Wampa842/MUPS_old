using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MUPS.Scene
{
	public static class PmxModelFinder
	{
		/// <summary>
		/// Finds all imported PMX models.
		/// </summary>
		public static GameObject[] AllModels()
		{
			return Resources.FindObjectsOfTypeAll<PmxModelBehaviour>().Select(e => e.gameObject).Reverse().ToArray();
		}


		/// <summary>
		/// Find the first imported PMX model with the specified name.
		/// </summary>
		/// <param name="name">The name to find.</param>
		/// <param name="exact">If true, looks for exact match. Otherwise looks for a substring.</param>
		/// <returns>The model's GameObject, or null if not found.</returns>
		public static GameObject FindModel(string name, bool exact = true)
		{
			if (exact)
			{
				foreach (GameObject o in AllModels())
				{
					if (o.GetComponent<PmxModelBehaviour>().ModelName == name)
						return o;
				}
			}
			else
			{
				foreach (GameObject o in AllModels())
				{
					if (o.GetComponent<PmxModelBehaviour>().ModelName.Contains(name))
						return o;
				}
			}

			return null;
		}

		/// <summary>
		/// Find all imported PMX models with the specified name.
		/// </summary>
		/// <param name="name">The name to find.</param>
		/// <param name="exact">If true, looks for exact match. Otherwise looks for a substring.</param>
		/// <returns></returns>
		public static GameObject[] FindModelAll(string name, bool exact = false)
		{
			if (exact)
				AllModels().Where(e => e.GetComponent<PmxModelBehaviour>().ModelName == name).ToArray();
			return AllModels().Where(e => e.GetComponent<PmxModelBehaviour>().ModelName.Contains(name)).ToArray();
		}
	}

	public class PmxModelBehaviour : MonoBehaviour
	{
		public string ModelName { get; set; }
		public string ModelInfo { get; set; }
		public int Index { get; set; }
	}
}