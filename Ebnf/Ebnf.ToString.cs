using System;

namespace Kekstoaster.Syntax
{
	public partial class Ebnf
	{
		private const int MAX_DEPTH = 10;

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Kekstoaster.Syntax.Ebnf"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Kekstoaster.Syntax.Ebnf"/>.</returns>
		/// <param name="depth">Sets the recursion depth after with the
		/// string creation will break and only set '...' for nested items.</param>
		public string ToString (int depth)
		{
			if (depth < 0) {
				return " ... ";
			}

			string result;

			switch (this._type) {
			case EbnfType.Char:
				result = this._char [0].ToString ();
				break;
			case EbnfType.EOF:
				result = "[[ EOF ]]";
				break;
			case EbnfType.Any:
				result = "(*)";
				break;
			case EbnfType.Range:
				result = "[[ " + this._char [0].ToString () + "-" + this._char [1].ToString () + " ]]";
				break;
			case EbnfType.Choise:
				{
					bool first = true;
					result = "(";
					foreach (var item in _list) {
						result += (first ? "" : ",") + item.ToString (depth - 1);
						first = false;
					}
					result += ")";
				}
				break;
			case EbnfType.List:
				result = "";
				foreach (var item in _list) {
					result += item.ToString (depth - 1);
				}
				break;
			case EbnfType.Optional:
				result = "[" + this._list [0].ToString (depth - 1) + "]";
				break;
			case EbnfType.Repeat:
				result = "{" + this._list [0].ToString (depth - 1) + "}";
				break;
			case EbnfType.Not:
				result = "-" + this._list [0].ToString (depth - 1);
				break;
			case EbnfType.Permutation:
				{
					bool first = true;
					result = "<";
					foreach (var item in _list) {
						result += (first ? "" : ",") + item.ToString (depth - 1);
						first = false;
					}
					result += ">";
				}
				break;
			default:
				result = "";
				break;
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Kekstoaster.Syntax.Ebnf"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Kekstoaster.Syntax.Ebnf"/>.</returns>
		public override string ToString ()
		{
			return ToString (MAX_DEPTH);
		}
	}
}

