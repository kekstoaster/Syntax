using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// This class indicated that the element is ignored meaning it is skiped while parsing.
	/// This usually occurs when an optional element was not matched, a repeat list did not match even once
	/// or a not element did not match.
	/// </summary>
	internal sealed class IgnoredElement:SyntaxElement
	{
		private static ParseAction ignoreParseAction = new ParseAction (new Func<SyntaxElement, bool> (delegate(SyntaxElement arg) {
			return false;
		}), new Action<ScopeContext, SyntaxElement[]> (delegate(ScopeContext s1, SyntaxElement[] s2) {
		}));
		private static IgnoredElement _instance = new IgnoredElement ();

		private IgnoredElement () : base (ignoreParseAction, null)
		{

		}

		internal static IgnoredElement Instance {
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

