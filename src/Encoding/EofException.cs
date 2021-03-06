﻿using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// This exception is thrown by a <see cref="Kekstoaster.Syntax.IDocumemtEncoder"/>
	/// when the end of stream is reached and no more characters are
	/// available.
	/// </summary>
	[Serializable]
	public class EofException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:EofException"/> class
		/// </summary>
		public EofException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EofException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public EofException (string message) : base (message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EofException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public EofException (string message, Exception inner) : base (message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EofException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected EofException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
		{
		}
	}
}

