using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Exception, that is thrown when an Error occours during the compiling process
	/// of the Ebnf compiling
	/// </summary>
	[Serializable]
	public class CompileException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:CompileException"/> class
		/// </summary>
		public CompileException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:CompileException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public CompileException (string message) : base (message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:CompileException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public CompileException (string message, Exception inner) : base (message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:CompileException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected CompileException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
		{
		}
	}
}

