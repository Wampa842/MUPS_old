namespace PmxSharp
{
	/// <summary>
	/// Represents a PMX display frame.
	/// </summary>
	public class PmxDisplayFrame : PmxNamedItem
	{
		public class FrameData
		{
			public enum FrameType { Bone, Morph }
			public FrameType Type { get; set; }
			int Index { get; set; }
		}
		public bool Special { get; set; }
		public FrameData[] Data { get; set; }
	}
}
