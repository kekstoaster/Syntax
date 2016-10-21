using System;

namespace BrainfuckExample
{
	public class BrainfuckCommand
	{
		protected BfCommandType _type;
		protected BrainfuckCommand _next;

		protected BrainfuckCommand ()
		{
		}

		public BrainfuckCommand (BfCommandType type)
		{
			if (type == BfCommandType.LeftLoop || type == BfCommandType.RightLoop) {
				throw new ArgumentException ("This is no loop type", "type");
			}

			_type = type;
		}

		public BrainfuckCommand Next {
			get {
				return this._next;
			}
			set {
				this._next = value;
			}
		}

		public virtual void Call (Brainfuck store)
		{
			switch (_type) {
			case BfCommandType.PointerIncrement:
				store.PointerIncrement ();
				break;
			case BfCommandType.PointerDecrement:
				store.PointerDecrement ();
				break;
			case BfCommandType.ValueIncrement:
				store.Increment ();
				break;
			case BfCommandType.ValueDecrement:
				store.Decrement ();
				break;
			case BfCommandType.PutChar:
				store.PutChar ();
				break;
			case BfCommandType.GetChar:
				store.GetChar ();
				break;
			default:
				break;
			}
		}
	}

	public class BrainfuckLoop : BrainfuckCommand
	{
		BrainfuckCommand _jump;

		public BrainfuckLoop (BfCommandType type)
		{
			if (type != BfCommandType.LeftLoop && type != BfCommandType.RightLoop) {
				throw new ArgumentException ("Must be loop type", "type");
			}

			this._type = type;
		}

		public BrainfuckCommand Jump {
			get {
				return this._jump;
			}
			set {
				if (value is BrainfuckLoop) {
					this._jump = value;
					((BrainfuckLoop)value)._jump = this;
				} else {
					throw new ArgumentException ("value must be loop type");
				}
			}
		}

		public override void Call (Brainfuck store)
		{
			switch (_type) {
			case BfCommandType.LeftLoop:
				if (store.CurrentValue == 0) {
					store.Current = _jump;
				}
				break;
			case BfCommandType.RightLoop:
				if (store.CurrentValue != 0) {
					store.Current = _jump;
				}
				break;
			default:
				break;
			}
		}
	}


	public enum BfCommandType // brainfuck base command
	{
		PointerIncrement,
		PointerDecrement,
		ValueIncrement,
		ValueDecrement,
		PutChar,
		GetChar,
		LeftLoop,
		RightLoop
	}
}

