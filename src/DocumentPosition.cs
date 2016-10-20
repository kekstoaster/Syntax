using System;

namespace Kekstoaster.Syntax
{
	public struct DocumentPosition
	{
		int _line;
		int _char;

		public DocumentPosition (int l, int c)
		{
			_line = l;
			_char = c;
		}

		public int Line {
			get{ return _line; }
		}

		public int Character {
			get { return _char; }
		}
	}
}

