using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Rendering;

class ImportEXR
{
	public static Texture2D Load(string path)
	{
		return Load(File.OpenRead(path));
	}

	public static Texture2D Load(Stream stream)
	{
		BinaryReader reader = new BinaryReader(stream);

		int w = 32, h = 32;
		Color32[] data = new Color32[w * h];

		for(int i = 0; i < data.Length; ++i)
		{
			data[i] = new Color32(1, 0, 0, 1);
		}

		Texture2D tex = new Texture2D(w, h);
		tex.SetPixels32(data);
		tex.Apply();

		return tex;
	}
}