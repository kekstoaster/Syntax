using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonExample
{
	public abstract class JsonValue
	{
		public abstract object Value { get; }

		public static implicit operator JsonValue (string s)
		{
			return new JsonString (s);
		}

		public static implicit operator JsonValue (double d)
		{
			return new JsonNumber (d);
		}

		public static implicit operator JsonValue (bool b)
		{
			return new JsonBoolean (b);
		}
	}

	public class JsonNull:JsonValue
	{
		private static JsonNull _instance = new JsonNull ();

		#region implemented abstract members of JsonValue

		public override object Value {
			get {
				return null;
			}
		}

		#endregion

		private JsonNull ()
		{
		}

		public static JsonNull Instance {
			get { return _instance; }
		}

		public override string ToString ()
		{
			return "null";
		}
	}

	public class JsonBoolean:JsonValue
	{
		private bool _value;

		public JsonBoolean (bool value)
		{
			this._value = value;
		}

		#region implemented abstract members of JsonValue

		public override object Value {
			get {
				return _value;
			}
		}

		#endregion

		public override string ToString ()
		{
			return _value ? "true" : "false";
		}
	}

	public class JsonNumber: JsonValue
	{
		private double _value;

		public JsonNumber (double value)
		{
			this._value = value;
		}

		#region implemented abstract members of JsonValue

		public override object Value {
			get {
				return _value;
			}
		}

		#endregion

		public override string ToString ()
		{
			return _value.ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}
	}

	public class JsonString:JsonValue
	{
		private string _value;

		public JsonString (string value)
		{
			this._value = value;
		}

		public override object Value {
			get {
				return _value;
			}
		}

		public override string ToString ()
		{
			return '"' + _value + '"';
		}
	}

	public class JsonArray:JsonValue, IEnumerable<JsonValue>
	{
		private List<JsonValue> _list;

		public JsonArray (params JsonValue[] values)
		{
			_list = new List<JsonValue> ();
			if (values != null) {
				foreach (var item in values) {
					_list.Add (item == null ? JsonNull.Instance : item);
				}
			} else {
				_list.Add (JsonNull.Instance);
			}
		}

		public string ValueOf ()
		{
			return this.ToString ();
		}

		public string Join (string seperator = "")
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder ();
			bool first = true;
			foreach (var item in _list) {
				if (!first) {
					result.Append (seperator);				
				}
				first = false;
				result.Append (item);
			}
			return result.ToString ();
		}

		public JsonValue Pop ()
		{
			JsonValue v = _list [_list.Count - 1];
			_list.RemoveAt (_list.Count - 1);
			return v;
		}

		public void Push (JsonValue value)
		{
			_list.Add (value == null ? JsonNull.Instance : value);
		}

		public JsonValue Shift ()
		{
			JsonValue v = _list [0];
			_list.RemoveAt (0);
			return v;
		}

		public void Unshift (JsonValue value)
		{
			_list.Insert (0, value == null ? JsonNull.Instance : value);
		}

		public JsonValue[] Splice (int start, int length, params JsonValue[] newElements)
		{
			List<JsonValue> _result = new List<JsonValue> (length);
			for (int i = 0; i < length && start + i < _list.Count; i++) {
				_result.Add (_list [start + i]);
			}
			_list.RemoveRange (start, length);
			if (newElements != null) {
				for (int i = 0; i < newElements.Length; i++) {
					if (newElements [i] == null) {
						newElements [i] = JsonNull.Instance;
					}
				}
				_list.InsertRange (start, newElements);
			} else {
				_list.Insert (start, JsonNull.Instance);
			}
			return _result.ToArray ();
		}

		public JsonValue this [int index] {
			get {
				return _list [index];
			}
			set {
				_list [index] = value == null ? JsonNull.Instance : value;
			}
		}

		public override object Value {
			get {
				List<object> result = new List<object> ();
				foreach (var item in _list) {
					result.Add (item.Value);
				}
				return result.ToArray ();
			}
		}

		#region IEnumerable implementation

		public IEnumerator<JsonValue> GetEnumerator ()
		{
			return _list.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		public override string ToString ()
		{
			System.Text.StringBuilder s = new System.Text.StringBuilder ();
			s.Append ('[');
			bool first = true;
			foreach (var item in _list) {
				if (!first) {
					s.Append (',');
				}
				s.Append (item.ToString ());
				first = false;
			}
			s.Append (']');
			return s.ToString ();
		}
	}

	public class JsonObject:JsonValue, IEnumerable<KeyValuePair<string, JsonValue>>
	{
		private Dictionary<string, JsonValue> _values;

		public JsonObject (params KeyValuePair<string, JsonValue>[] elements)
		{
			_values = new Dictionary<string, JsonValue> ();
			if (elements != null) {
				foreach (var pair in elements) {
					if (_values.ContainsKey (pair.Key)) {
						_values [pair.Key] = pair.Value == null ? JsonNull.Instance : pair.Value;
					} else {
						_values.Add (pair.Key, pair.Value == null ? JsonNull.Instance : pair.Value);
					}
				}
			}
		}

		public JsonValue this [string index] {
			get {
				return _values [index];
			}
			set {
				if (_values.ContainsKey (index)) {
					_values [index] = value == null ? JsonNull.Instance : value;
				} else {
					_values.Add (index, value == null ? JsonNull.Instance : value);
				}
			}
		}

		public override object Value {
			get {
				List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>> ();

				foreach (var item in _values) {
					result.Add (new KeyValuePair<string, object> (item.Key, item.Value.Value));
				}
				return result.ToArray ();
			}
		}

		#region IEnumerable implementation

		IEnumerator<KeyValuePair<string, JsonValue>> IEnumerable<KeyValuePair<string, JsonValue>>.GetEnumerator ()
		{
			return _values.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		public override string ToString ()
		{
			System.Text.StringBuilder s = new System.Text.StringBuilder ();
			s.Append ('{');
			bool first = true;
			foreach (var item in _values) {
				if (!first) {
					s.Append (',');
				}
				s.Append ('"');
				s.Append (item.Key);
				s.Append ('"');
				s.Append (':');
				s.Append (item.Value);
				first = false;
			}
			s.Append ('}');
			return s.ToString ();
		}
	}
}

