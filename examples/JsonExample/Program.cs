using System;
using System.IO;
using Kekstoaster.Syntax;

namespace JsonExample
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length > 0) {
				if (File.Exists (args [0])) {
					try {
						Ebnf json = JsonSyntax.Syntax;
						EbnfCompiler compiler = new EbnfCompiler (json, ScopeType.Parent, EbnfCompileBehavior.Text, new EncoderUTF8 ());
						JsonObject jsObj = (JsonObject)compiler.CompileFile (args [0]);
						Console.WriteLine (jsObj);
					} catch (ParseException pe) {
						Console.WriteLine ("Error in line {0} - {1}", pe.DocumentPosition.Line, pe.DocumentPosition.Character);
						Console.WriteLine (pe.Message);			
					} catch (Exception e) {
						Console.WriteLine (e.Message);
					}
				} else {
					Console.WriteLine ("file does not exist");
				}					
			} else {
				Console.WriteLine ("Prints the minimal version of a json-file");
			}
		}
	}
}