using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Document encoder interface for parsing documents
	/// that come in a certain encoding.
	/// </summary>
	public interface IDocumemtEncoder
	{
		/// <summary>
		/// Gets the next char in the stream. If the end of the stream
		/// is reached, a <see cref="Kekstoaster.Syntax.EofException"/>
		/// is thrown.
		/// </summary>
		/// <returns>Returns the next character in the stream</returns>
		/// <param name="stream">The stream that will be used to get the character.</param>
		char NextChar (Stream stream);
	}
}

