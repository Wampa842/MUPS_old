using System;
using System.Collections.Generic;
using UnityEngine;
using Crosstales.FB;
using MUPS.UI;

namespace MUPS
{
	class SkyControllerBehaviour : MonoBehaviour
	{
		public static SkyControllerBehaviour Instance { get; private set; }

		public enum SkyboxMapping { None, Cube, LatLong }

		private SkyboxMapping _mapping = SkyboxMapping.None;
		private string _skyboxPath;

		public void LoadSkybox()
		{
			_skyboxPath = FileBrowser.OpenSingleFile("Open skybox file", SettingsBehaviour.Instance.Settings.SkyboxImportDirectory, "");
			if (!string.IsNullOrEmpty(_skyboxPath))
			{
				Modal.Show(500, 100, "Skybox mapping type", "",
					new ButtonDescriptor("Latitude-longitude", () => { _mapping = SkyboxMapping.LatLong; }),
					new ButtonDescriptor("Six sided", () => { _mapping = SkyboxMapping.Cube; }),
					new ButtonDescriptor("Cancel", ButtonDescriptor.ColorPresets.Red)
					);
			}
		}

		#region Mono
		private void Awake()
		{
			if(Instance == null)
			{
				Instance = this;
			}
			else if(Instance != this)
			{
				Destroy(Instance);
				Instance = this;
			}
		}

		private void Update()
		{
			if(_mapping != SkyboxMapping.None)
			{
				Material mat = _mapping == SkyboxMapping.LatLong ? Resources.Load<Material>("Materials/SkyboxLatLong") : Resources.Load<Material>("Materials/SkyboxSixSided");
				Texture2D tex = new Texture2D(2, 2);
				tex.LoadImage(System.IO.File.ReadAllBytes(_skyboxPath));
				tex.wrapMode = TextureWrapMode.Clamp;
				tex.requestedMipmapLevel = 0;
				mat.mainTexture = tex;
				RenderSettings.skybox = mat;

				_mapping = SkyboxMapping.None;
			}
		}
		#endregion
	}
}
