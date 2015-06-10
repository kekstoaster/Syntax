using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Scope type for parsing elements.
	/// </summary>
	public enum ScopeType {
		/// <summary>
		/// The default scope type that is used by the EbnfCompiler.
		/// </summary>
		Default,
		/// <summary>
		/// If all subelements have only ScopeType Inhired, the elements has no scope.
		/// Otherwise the element is created with a scope.
		/// </summary>
		Inhired,
		/// <summary>
		/// The scope is forced for the element.
		/// </summary>
		Force,
		/// <summary>
		/// The scope of the parent element will be used.
		/// This means the compile and parse action of the element will not be called and all subelements are added to the parent.
		/// </summary>
		Parent,
		/// <summary>
		/// Indicates that all subelements contain no information and are not parsed or compiled.
		/// </summary>
		Empty
	}
}

