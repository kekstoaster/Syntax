using System;

namespace BrainfuckExample
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string helloWorld = @"++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.";

			Brainfuck b = Brainfuck.GetProgram (helloWorld);
			b.Run ();

			string charToInt = @"
[
	inputs a char and prints its int representation
]
,   input
>++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++ .  colon:
---------- ---------- ------ .                                      space
[-]
++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++++ 100 for modulo
>[-]>[-]>[-]>[-]<<<<<[->>+<-[>>>]>[[<+>-]>+>>]<<<<<] .  char modulo 100
>>>[-<<<+>>>]<<[-]>[-<+>] .                             move char / 100 to 0; char % 100 to 1
++++++++++                                              10 for modulo
>[-]>[-]>[-]>[-]<<<<<[->>+<-[>>>]>[[<+>-]>+>>]<<<<<] .  (100)char % 10

<[++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++ .              print hundred if char was greater 100
>>>>++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++ . [-] <<<<   if so; print always next number; even if 0
 [-]]
>>>>[++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++ . [-]]      if not; print next if char was greater than 10
<++++++++++ ++++++++++ ++++++++++ ++++++++++ ++++++++ .               always print last digit
";
			Brainfuck b2 = Brainfuck.GetProgram (charToInt);
			b2.Run ();
		}
	}
}
