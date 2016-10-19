using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Document encoder for UTF8 strings and documents.
	/// </summary>
	public class EncoderUTF8:IDocumemtEncoder
	{
		/// <summary>
		/// Gets the next char in the stream. If the end of the stream
		/// is reached, a <see cref="Kekstoaster.Syntax.EofException"/>
		/// is thrown.
		/// </summary>
		/// <returns>Returns the next character in the stream</returns>
		/// <param name="stream">The stream that will be used to get the character.</param>
		public char NextChar (Stream stream)
		{
			int b = stream.ReadByte ();
			if (b == -1) {
				throw new EofException ();
			} else {
				// first 7 bit are equal to ASCII - 0xxxxxxx
				// 1 byte encoded
				if (b < 128) {
					return (char)b;
				} else {
					if (b < 192) {
						// bitmask: 10xxxxxx is only used for following bytes
						// the first byte of a character is not allowed to
						// have this encoding
						throw new ParseException ("The encoding of this document is not supported");
					} else {
						if (b < 224) { // bitmask: 110xxxxx, 2 byte encoded
							int b2 = stream.ReadByte ();
							if (b2 < 128 || b2 > 191) {
								// all following bytes must have a 
								// bitmask of 10xxxxxx
								throw new ParseException ("The encoding of this document is not supported");
							} else {
								return (char)(
								    ((b & 0x1F) << 6) | // lowest 5 bits of first byte
								    (b2 & 0x3F)         // lowest 6 bits of second byte
								);
							}
						} else {
							if (b < 240) { // bitmask: 1110xxxx, 3 byte encoded
								int b2 = stream.ReadByte (), b3 = stream.ReadByte ();
								if (b2 < 128 || b2 > 191 || b3 < 128 || b3 > 191) {
									// all following bytes must have a 
									// bitmask of 10xxxxxx
									throw new ParseException ("The encoding of this document is not supported");
								} else {
									return (char)(
									    ((b & 0x0F) << 12) | // lowest 5 bits of first byte
									    (b2 & 0x3F) << 6 | // lowest 6 bits of second byte
									    (b3 & 0x3F)        // lowest 6 bits of third byte
									);
								}
							} else {
								// higher than 3 byte encoded, not compatible
								// with char type
								throw new ParseException ("The encoding of this document is not supported");
							}
						}
					}
				}
			}
		}
	}
}

