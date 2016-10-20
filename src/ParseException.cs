using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// An exception that is thrown during the parsing step of the compiling process.
	/// </summary>
	[Serializable]
	public class ParseException : Exception
	{
		private Ebnf _faulty;
		private DocumentPosition _pos;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		public ParseException (Ebnf faulty = null)
		{
			this._faulty = faulty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="faulty">A <see cref="Kekstoaster.Syntax.Ebnf"/> that describes the element that could not be matched.</param>
		public ParseException (string message, Ebnf faulty = null) : base (message)
		{
			this._faulty = faulty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		/// <param name="faulty">A <see cref="Kekstoaster.Syntax.Ebnf"/> that describes the element that could not be matched.</param>
		public ParseException (string message, Exception inner, Ebnf faulty = null) : base (message, inner)
		{
			this._faulty = faulty;
		}

		/// <summary>
		/// Gets or sets the faulty Ebnf element that could not be matched
		/// </summary>
		/// <value>The faulty element.</value>
		public Ebnf FaultyElement {
			get {
				return this._faulty;
			}
			set {
				if (this._faulty == null) {
					this._faulty = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected ParseException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
		{
		}

		public DocumentPosition DocumentPosition {
			get{ return _pos; }
			internal set { _pos = value; }
		}
	}
}

