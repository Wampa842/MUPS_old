using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace MUPS
{
	[Serializable]
	public class SettingsData
	{
		public string ModelImportDirectory { get; set; }
		public string SceneImportDirectory { get; set; }
		public string ImageExportDirectory { get; set; }
		public Vector2 RenderSize { get; set; }
		
		public SettingsData()
		{
			ModelImportDirectory = string.Empty;
			SceneImportDirectory = string.Empty;
			ImageExportDirectory = string.Empty;
			RenderSize = new Vector2(Screen.width, Screen.height);
		}

		public static SettingsData Load(string path)
		{
			XmlReader reader = null;
			XmlSerializer serializer;
			try
			{
				serializer = new XmlSerializer(typeof(SettingsData));
				reader = XmlReader.Create(File.Open(path, FileMode.OpenOrCreate, FileAccess.Read));
				return (serializer.Deserialize(reader) as SettingsData) ?? new SettingsData();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			return new SettingsData();
		}

		public static void Save(string path, SettingsData data)
		{
			XmlSerializer serializer = null;
			XmlWriter writer = null;
			try
			{
				serializer = new XmlSerializer(typeof(SettingsData));
				writer = XmlWriter.Create(File.Open(path, FileMode.Create, FileAccess.Write));
				serializer.Serialize(writer, data);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
		}
	}

	public class SettingsBehaviour : MonoBehaviour
	{
		public static string SavePath { get; set; }
		public static SettingsBehaviour Instance { get; private set; }

		public string modelImportPath;
		public string sceneOpenPath;
		public string imageExportPath;
		public Vector2 renderSize;

		private SettingsData _settings;
		public SettingsData Settings
		{
			get
			{
				if (_settings == null)
				{
					_settings = SettingsData.Load(SavePath);
				}
				return _settings ?? new SettingsData();
			}
		}

		private void Awake()
		{
			SavePath = Path.Combine(Application.persistentDataPath, "settings.xml");

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

		private void OnApplicationQuit()
		{
			SettingsData.Save(SavePath, Settings);
		}
	}
}