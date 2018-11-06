using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PmxSharp
{
	public static class PmxModelFinder
	{
		public static GameObject[] AllModels()
		{
			return Resources.FindObjectsOfTypeAll<PmxModelBehaviour>().Select(e => e.gameObject).Reverse().ToArray();
		}

		public static GameObject FindModel(string name, bool exact = true)
		{
			if (exact)
			{
				foreach (GameObject o in AllModels())
				{
					if (o.GetComponent<PmxModelBehaviour>().modelName == name)
						return o;
				}
			}
			else
			{
				foreach (GameObject o in AllModels())
				{
					if (o.GetComponent<PmxModelBehaviour>().modelName.Contains(name))
						return o;
				}
			}

			return null;
		}

		public static GameObject[] FindModelAll(string name, bool exact = false)
		{
			if (exact)
				AllModels().Where(e => e.GetComponent<PmxModelBehaviour>().modelName == name).ToArray();
			return AllModels().Where(e => e.GetComponent<PmxModelBehaviour>().modelName.Contains(name)).ToArray();
		}
	}

	public class PmxModelBehaviour : MonoBehaviour
	{
		public string modelName;
		public string modelInfo;
		public int index;

		public void OnSelect()
		{

		}

		public void OnDeselect()
		{

		}
	}
}