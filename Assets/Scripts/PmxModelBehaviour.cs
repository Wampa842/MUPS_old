using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MUPS.Scene
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

		public static GameObject[] FindModelAll(string name, bool exact = false)
		{
			if (exact)
				AllModels().Where(e => e.GetComponent<PmxModelBehaviour>().ModelName == name).ToArray();
			return AllModels().Where(e => e.GetComponent<PmxModelBehaviour>().ModelName.Contains(name)).ToArray();
		}
	}

	public class PmxModelBehaviour : MonoBehaviour
	{
		public string ModelName;
		public string ModelInfo;
		public int Index;
		public Transform BoneParent;
		public RectTransform BoneSpriteParent;

		public void OnSelect()
		{
			GameObject prefab = Resources.Load<GameObject>("Prefabs/GUI/BoneSprite");
			
		}

		public void OnDeselect()
		{

		}
	}
}