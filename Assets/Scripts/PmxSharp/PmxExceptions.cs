using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PmxSharp
{
	[Serializable]
	public class PmxException : Exception
	{
		public PmxException() { }
		public PmxException(string message) : base(message) { }
		public PmxException(string message, Exception inner) : base(message, inner) { }
		protected PmxException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class PmxFormatException : PmxException
	{
		public PmxFormatException(byte[] got, byte[] expected) : this(string.Format("Value could not be parsed. Expected {0} ({2} bytes), got {1} ({3} bytes).", BitConverter.ToString(expected), BitConverter.ToString(got), expected.Length, got.Length))
		{
			Data.Add("ValueExpected", expected);
			Data.Add("ValueGot", got);
		}
		public PmxFormatException(string message) : base(message) { }
		public PmxFormatException(string message, Exception inner) : base(message, inner) { }
		protected PmxFormatException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class PmxSignatureException : PmxException
	{
		public PmxSignatureException(byte[] signature) : this(string.Format("The PMX signature is invalid. Expected {0} ({1}), got {2} ({3}).", BitConverter.ToString(PmxConstants.PmxSignature), Encoding.ASCII.GetString(PmxConstants.PmxSignature), BitConverter.ToString(signature), Encoding.ASCII.GetString(signature))) { }
		public PmxSignatureException(string message) : base(message) { }
		public PmxSignatureException(string message, Exception inner) : base(message, inner) { }
		protected PmxSignatureException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class PmxDeformException : PmxException
	{
		public PmxDeformException(byte deform) : this(string.Format("The deform identifier is invalid. Expected 0-4, got {0}", deform)) { }
		public PmxDeformException(string message) : base(message) { }
		public PmxDeformException(string message, Exception inner) : base(message, inner) { }
		protected PmxDeformException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}


	[Serializable]
	public class MaterialDirectiveException : PmxException
	{
		public PmxMaterial Material { get; private set; }
		public string Directive { get; private set; }
		public MaterialDirectiveException() { }
		public MaterialDirectiveException(string message) : base(message) { }
		public MaterialDirectiveException(string message, PmxMaterial material, string directiveContent) : base(message)
		{
			Material = material;
			Directive = directiveContent;
		}
		public MaterialDirectiveException(string message, Exception inner) : base(message, inner) { }
		protected MaterialDirectiveException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
