using System;

namespace Kekstoaster.Syntax
{
	
	[Serializable]
	internal class EbnfElementException : Exception
	{
		private Ebnf _faulty;
		/// <summary>
		/// Initializes a new instance of the <see cref="T:EbnfElementException"/> class
		/// </summary>
		internal EbnfElementException (Ebnf faulty = null)
		{
			this._faulty = faulty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EbnfElementException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="faulty">A <see cref="Kekstoaster.Syntax.Ebnf"/> that describes the element that could not be matched.</param>
		internal EbnfElementException (string message, Ebnf faulty = null) : base (message)
		{
			this._faulty = faulty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EbnfElementException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		/// <param name="faulty">A <see cref="Kekstoaster.Syntax.Ebnf"/> that describes the element that could not be matched.</param>
		internal EbnfElementException (string message, Exception inner, Ebnf faulty = null) : base (message, inner)
		{
			this._faulty = faulty;
		}

		internal Ebnf FaultyElement {
			get {
				return this._faulty;
			}
			set{
				if (this._faulty == null) {
					this._faulty = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EbnfElementException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected EbnfElementException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
		{
		}
	}
}

