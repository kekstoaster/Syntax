using System;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// An extended dictionary that returns the default value of T if the key does not exist.
	/// </summary>
	public sealed class ItemList<T> {
		private Dictionary<string, T> _items;

		/// <summary>
		/// Initializes a new instance of the ItemList class.
		/// </summary>
		public ItemList() {
			_items = new Dictionary<string, T> ();
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// If the index does not exist, the default value of T is returned and no exception is thrown.
		/// </summary>
		/// <param name="index">The key of the item</param>
		public T this[string index] {
			get {
				if(_items.ContainsKey(index)) {
					return _items [index];
				} else {
					return default(T);
				}
			}
			set {
				if(_items.ContainsKey(index)) {
					_items [index] = value;
				} else {
					_items.Add (index, value);
				}
			}
		}
	}
}

