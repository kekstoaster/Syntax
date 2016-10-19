using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// The default compile behavior for compiling elements with no user definied compile action.
	/// </summary>
	public enum EbnfCompileBehavior
	{
		/// <summary>
		/// Ignore the element. null will be returned.
		/// </summary>
		Ignore,
		/// <summary>
		/// Creates a object array with all subelements
		/// </summary>
		List,
		/// <summary>
		/// Creates a string combining all string representations of all subelements
		/// </summary>
		Text
	}
}