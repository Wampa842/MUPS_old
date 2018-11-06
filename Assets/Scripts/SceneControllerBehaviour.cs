using Crosstales.FB;
using MUPS.UI;
using PmxSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MUPS.Scene
{
	public class SceneControllerBehaviour : MonoBehaviour
	{
		public ColorBlock normalButtonColors = new ColorBlock() { normalColor = new Color(1, 1, 1, 0.6f), highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1), pressedColor = new Color(0.8f, 0.8f, 0.8f, 1), disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f), colorMultiplier = 1, fadeDuration = 0 };

		public GameObject transformTypeButton;
		public GameObject modelListContent;
		public GizmoBehaviour gizmoRoot;
		public Transform modelAttachment;
		public Camera mainCamera;
		public Camera renderCamera;

		private ColorBlock _selectedButtonColors;

		public GameObject[] Models { get; private set; }
		public int Selected { get; set; }
		public List<GameObject> ButtonList { get; private set; }
		private float _buttonWidth, _buttonHeight;

		public void CycleTransformMode()
		{
			gizmoRoot.GizmoType = (GizmoCoordinateSystem)((int)(gizmoRoot.GizmoType + 1) % (Enum.GetNames(typeof(GizmoCoordinateSystem)).Length));
			transformTypeButton.transform.GetChild(0).GetComponent<Text>().text = gizmoRoot.GizmoButtonName;
		}

		private GameObject CreateButton(GameObject prefab, Transform parent, float height, float width, int index, string text = "Button", bool onlyButton = false)
		{
			GameObject button = GameObject.Instantiate<GameObject>(prefab);
			button.transform.GetChild(0).GetComponent<Text>().text = text;
			RectTransform rt = (RectTransform)button.transform;
			rt.sizeDelta = new Vector2(width, height);

			Button comp = button.GetComponent<Button>();
			comp.onClick = new Button.ButtonClickedEvent();
			comp.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
			{
				SelectModel(index);
			}));
			button.transform.SetParent(parent);
			return button;
		}

		public void ScanModels()
		{
			// Delete the old buttons
			if (ButtonList == null)
				ButtonList = new List<GameObject>();
			foreach (GameObject b in ButtonList)
				GameObject.Destroy(b);
			ButtonList.Clear();

			// Set up "none" button
			GameObject prefab = Resources.Load<GameObject>("Prefabs/ModelListButton");
			ButtonList.Add(CreateButton(prefab, modelListContent.transform, _buttonHeight, _buttonWidth, -1, "--- Scene ---", true));

			Models = PmxModelFinder.AllModels();
			for (int i = 0; i < Models.Length; ++i)
			{
				ButtonList.Add(CreateButton(prefab, modelListContent.transform, _buttonHeight, _buttonWidth, i, Models[i].GetComponent<PmxModelBehaviour>().ModelName));
			}
			modelListContent.GetComponentInParent<ScrollRectWithoutDrag>().verticalNormalizedPosition = 1;
		}

		public void SelectModel(int index)
		{
			// UI interaction
			foreach (GameObject b in ButtonList)
			{
				b.GetComponent<Button>().colors = normalButtonColors;
			}
			ButtonList[index + 1].GetComponent<Button>().colors = _selectedButtonColors;
			Selected = index;

			if (index < 0)
			{
				gizmoRoot.GizmoType = GizmoCoordinateSystem.Off;
				transformTypeButton.transform.GetChild(0).GetComponent<Text>().text = gizmoRoot.GizmoButtonName;
				return;
			}

			gizmoRoot.SelectedModel = Models[Selected].transform;

			foreach (GameObject model in Models)
			{
				if (model != Models[Selected])
					model.GetComponent<PmxModelBehaviour>().OnDeselect();
			}
			Models[Selected].GetComponent<PmxModelBehaviour>().OnSelect();
		}

		public void DeleteSelected()
		{
			if (Selected > -1 && Models[Selected] != null)
				GameObject.DestroyImmediate(Models[Selected]);
			ScanModels();
			SelectModel(-1);
		}

		public GameObject LoadFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			else
			{
				PmxImport import = null;
				try
				{
					import = new PmxImport(path);
					import.Resize(0.1f);
					GameObject pmx = import.GetGameObject();
					pmx.AddComponent<PmxModelBehaviour>().ModelName = import.Name;
					pmx.transform.SetParent(GameObject.Find("/SCENE").transform);

					return pmx;
				}
				catch (Exception ex)
				{
					Modal.Show(Screen.width - 400, Screen.height - 400, ex.Message, ex.ToString(), ButtonDescriptor.DismissPreset);
					return null;
				}
				finally
				{
					if (import != null)
						import.Dispose();
				}
			}
		}

		public void LoadFile()
		{
			GameObject pmx;
			ExtensionFilter[] exts = new ExtensionFilter[] { new ExtensionFilter("PMX files", "pmx", "pmd", "x"), new ExtensionFilter("Other model files", "obj", "fbx"), new ExtensionFilter("All files", "*") };
			string path = FileBrowser.OpenSingleFile("Open file", SettingsBehaviour.Instance.Settings.ModelImportDirectory, exts);
			pmx = LoadFile(path);
			if (pmx != null)
			{
				SettingsBehaviour.Instance.Settings.ModelImportDirectory = System.IO.Path.GetDirectoryName(path);
				ScanModels();
				SelectModel(Models.Length - 1);
			}
		}

		private void Awake()
		{
			_selectedButtonColors = normalButtonColors;
			_selectedButtonColors.colorMultiplier = 0.6f;

			Selected = -1;
			_buttonWidth = ((RectTransform)modelListContent.transform).rect.width;
			_buttonHeight = 30.0f;

			ScanModels();
			SelectModel(-1);
			gizmoRoot.GizmoType = GizmoCoordinateSystem.Off;
			transformTypeButton.transform.GetChild(0).GetComponent<Text>().text = gizmoRoot.GizmoButtonName;
		}

		void Update()
		{
		}
	}
}