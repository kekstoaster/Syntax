using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// This class indicated that the element is ignored meaning it is skiped while parsing.
	/// This usually occurs when an optional element was not matched, a repeat list did not match even once
	/// or a not element did not match.
	/// </summary>
	internal sealed class IgnoredElement:SyntaxElement {
		private static IgnoredElement _instance = new IgnoredElement ();

		private IgnoredElement():base(null){

		}

		public static IgnoredElement Instance {
			get {
				return _instance;
			}
		}

		internal override object Compile (ScopeContext parentContext)
		{
			return null;
		}

		public override string Text {
			get {
				return "";
			}
		}
	}
}

