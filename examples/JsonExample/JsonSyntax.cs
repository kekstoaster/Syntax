using System;
using System.Collections.Generic;
using Kekstoaster.Syntax;

namespace JsonExample
{
	public static class JsonSyntax
	{
		public static Ebnf Syntax {
			get {
				// create all special characters
				Ebnf
				carriageReturn = '\r',
				lineFeed = '\n',

				quote = '"',
				backslash = '\\',
				braceLeft = '[',
				braceRight = ']',
				chevronsLeft = '{',
				chevronsRight = '}',

				dot = '.',
				comma = ',',
				colon = ':',
				plus = '+',
				minus = '-';

				Ebnf digitNonZero = new EbnfRange ('1', '9');
				Ebnf digit = EbnfTemplates._0_to_9;
				Ebnf smallLetter = EbnfTemplates.a_to_z;
				Ebnf capitalLetter = EbnfTemplates.A_to_Z;
				Ebnf letter = EbnfTemplates.Alpha;
				Ebnf hexDigit = EbnfTemplates._0_to_9 | new EbnfRange ('a', 'f') | new EbnfRange ('A', 'F');

				// get whitespace from template: space, tabulator, line feed, carriage return
				Ebnf whitespace = EbnfTemplates.Whitespace;
				// set label, to identify while parsing
				whitespace.Label = "whitespace";
				// content of whitespace is not relevant, so discard data
				// already while parsing
				whitespace.ScopeType = ScopeType.Empty;
				Ebnf whitespaceblock = ~whitespace;
				// set label, to identify while parsing
				whitespaceblock.Label = "whitespace";
				// content of whitespace is not relevant, so discard data
				// already while parsing
				whitespaceblock.ScopeType = ScopeType.Empty;
				// define all supported backslash sequences
				Ebnf backslashSquences =
					((Ebnf)"\\\"") |
					((Ebnf)"\\\\") |
					((Ebnf)"\\/") |
					((Ebnf)"\\b") |
					((Ebnf)"\\f") |
					((Ebnf)"\\n") |
					((Ebnf)"\\r") |
					((Ebnf)"\\t") |
					(((Ebnf)"\\u") & new EbnfRepeat (hexDigit, 4, 4));
				Ebnf lineEnding = (carriageReturn & lineFeed) | carriageReturn | lineFeed | Ebnf.EOF;
				// content of whitespace is not relevant, so discard data
				// already while parsing
				lineEnding.ScopeType = ScopeType.Empty;

				// get identifier from templates, used as keys in object
				// key - value - pairs
				Ebnf identifier = EbnfTemplates.Identifier;
				// set scope, so content will create a string
				// if you keep parent, you will get a list with all
				// single characters
				identifier.ScopeType = ScopeType.Inhired;
				EbnfList key = (EbnfList)(Ebnf.EmptyList + quote + identifier + quote);
				// set scope to inhired, so ParseAction will be evaluated
				key.ScopeType = ScopeType.Inhired;
				// key starts with quotes (") so if there is an error, this cannot
				// be matched anyway, so do even try
				key.Unique ();

				// control characters are not allowed in json strings
				// including 
				EbnfExclusion n_controlCharacter = -(new EbnfRange ((char)0, (char)31));
				n_controlCharacter.ErrorMessage = "invalid character in string";
				n_controlCharacter.Serious ();

				// string syntax
				Ebnf stringContent = ~((-backslash & -quote & n_controlCharacter & Ebnf.AnyChar) | backslashSquences);
				stringContent.ScopeType = ScopeType.Inhired;
				Ebnf string_ = quote & stringContent & quote;
				// number syntax
				Ebnf number = !(plus | minus) & (('0' & -(digit | dot)) | (((((!(Ebnf)'0') & dot & digit & ~digit) | (digitNonZero & ~digit & !(dot & ~digit)))) & !((((Ebnf)'e') | ((Ebnf)'E')) & !(plus | minus) & digitNonZero & ~digit)));

				Ebnf keyTrue = "true";
				Ebnf keyFalse = "false";

				// create specific value elements
				Ebnf vNull = "null";
				Ebnf vBoolean = keyTrue | keyFalse;
				Ebnf vNumber = number.Clone ();
				EbnfList vString = (EbnfList)string_.Clone ();
				// same as string, starts with "
				vString.Unique ();
				EbnfList vArray = Ubnf.EmptyList;
				// starts with {, so cannot match anything else
				vArray.Unique ();
				EbnfList vObject = Ubnf.EmptyList;
				// starts with [, so cannot match anything else
				vObject.Unique ();

				// jsonvalue can be any of these
				Ebnf jsonValue = Ebnf.EmptyChoise + vNull + vBoolean + vNumber + vString + vArray + vObject;
				// object contains list of key-value-pairs
				Ebnf keyValuePair = Ebnf.EmptyList + key + whitespaceblock + colon + whitespaceblock + jsonValue;

				{ // vArray
					Ebnf lst1 = Ebnf.EmptyList + whitespaceblock + comma + whitespaceblock + jsonValue;
					Ebnf rpt1 = ~(lst1);
					Ebnf lst2 = Ebnf.EmptyList + whitespaceblock + jsonValue + rpt1;
					Ebnf opt1 = !(lst2);

					// set parent, so they get all passed through
					// and are evaluated at once
					rpt1.ScopeType = ScopeType.Parent;
					lst1.ScopeType = ScopeType.Parent;
					lst2.ScopeType = ScopeType.Parent;
					opt1.ScopeType = ScopeType.Parent;

					vArray = (EbnfList)(vArray + braceLeft + opt1 + whitespaceblock + braceRight);
				}

				{ // vObject 
					Ebnf lst1 = Ebnf.EmptyList + whitespaceblock + comma + whitespaceblock + keyValuePair;
					Ebnf rpt1 = ~(lst1);
					Ebnf lst2 = Ebnf.EmptyList + whitespaceblock + keyValuePair + rpt1;
					Ebnf opt1 = !(lst2);

					// set parent, so they get all passed through
					// and are evaluated at once
					rpt1.ScopeType = ScopeType.Parent;
					lst1.ScopeType = ScopeType.Parent;
					lst2.ScopeType = ScopeType.Parent;
					opt1.ScopeType = ScopeType.Parent;

					vObject = (EbnfList)(vObject + chevronsLeft + opt1 + whitespaceblock + chevronsRight);
				}

				// root element is an object, that can have whitespaces befor and after
				// if you dont check for EOF, evaluation stops after the
				// first object, and all characters after will just be ignored
				// so make sure to check for EOF so there can't be anything
				// after the root object
				Ebnf root = Ebnf.EmptyList + whitespaceblock + vObject + whitespaceblock + Ebnf.EOF;
				root.ScopeType = ScopeType.Force;

				// ignore quotes in keys
				key.ParseAction = new ParseAction (new Func<SyntaxElement, bool> ((SyntaxElement arg2) => {
					if (arg2.Text == "\"") {
						return false;
					} else {
						return true;
					}
				}));

				keyValuePair.ParseAction = new ParseAction (new Func<SyntaxElement, bool> ((SyntaxElement arg2) => {
					// ignore colon and all whitespaces
					// you still get elements that are called "whitespace"
					// but since their scopetype is empty, their content
					// is gone
					switch (arg2.Label) {
					case ":":
					case "whitespace":
						return false;
					default:
						return true;
					}
				}));
				keyValuePair.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					// since you ignore all colons and whitespaces
					// and string keys ignore their quotes, two elements are
					// remaining, the plain key without quotes
					// and the value element
					return new KeyValuePair<string, JsonValue> (arg2 [0].ToString (), arg2 [1] as JsonValue);
				}));
				// forward element
				jsonValue.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					return arg2 [0];
				}));

				root.ParseAction = new ParseAction (new Func<SyntaxElement, bool> ((SyntaxElement arg2) => {
					// ignore elements
					switch (arg2.Label) {
					case EbnfEOF.EOF_LABEL:
					case "whitespace":
						return false;
					default:
						return true;
					}
				}));
				root.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					// only the root element was parsed
					return arg2 [0];
				}));

				vNull.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					return JsonNull.Instance;
				}));

				keyTrue.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					return true;
				}));
				keyFalse.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					return false;
				}));
				vBoolean.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					// get either value from keyTrue or keyFalse
					return new JsonBoolean ((bool)arg2 [0]);
				}));

				vNumber.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {					
					// parsed number is valid double, so double.parse should not fail
					// the value in arg2[0] was created via a default compile
					// and results in a string
					return new JsonNumber (double.Parse (arg2 [0].ToString (), System.Globalization.CultureInfo.InvariantCulture));
				}));
				vString.ParseAction = new ParseAction (new Func<SyntaxElement, bool> ((SyntaxElement arg2) => {
					// again, ignore quotes (")
					if (arg2.Text == "\"") {
						return false;
					} else {
						return true;
					}
				}));
				vString.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {					
					// the value in arg2[0] was created via a default compile
					// and results in a string
					return new JsonString (arg2 [0].ToString ());
				}));
				vArray.ParseAction = new ParseAction (new Func<SyntaxElement, bool> ((SyntaxElement arg2) => {
					// ignore elements without information
					switch (arg2.Label) {
					case ",":
					case "whitespace":
					case "[":
					case "]":
						return false;
					default:
						return true;
					}
				}));
				vArray.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					// add all elements to the array. These should all
					// be JsonValues
					JsonValue[] va = new JsonValue[arg2.Length];
					for (int _i = 0; _i < arg2.Length; _i++) {
						va [_i] = (JsonValue)arg2 [_i];
					}
					return new JsonArray (va);
				}));

				vObject.ParseAction = new ParseAction (new Func<SyntaxElement, bool> ((SyntaxElement arg2) => {
					// ignore elements without information
					switch (arg2.Label) {
					case ",":
					case "whitespace":
					case "{":
					case "}":
						return false;
					default:
						return true;
					}
				}));
				vObject.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext arg1, object[] arg2) {
					// add all key-value-pairs to the object
					KeyValuePair<string, JsonValue>[] va = new KeyValuePair<string, JsonValue>[arg2.Length];
					for (int _i = 0; _i < arg2.Length; _i++) {
						va [_i] = (KeyValuePair<string, JsonValue>)arg2 [_i];
					}
					return new JsonObject (va);
				}));
				// return the root syntax element
				return root;
			}
		}
	}
}
