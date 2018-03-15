using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// mtse.cs
// A couple of miscellaneous things
//
//---------------------------------------------

namespace mtse
{
	//************************************************************

	public class mtseException : Exception
    {
		public mtseException() : base("%%unknown_exception")
		{ }
		public mtseException(string error_message, params object[] args)
			: base(error_message)
		{
			_args = args;
		}

		public object[] Args
		{
			get { return _args; }
		}

		private object[] _args = null;
	}

	//************************************************************

	enum e_Oper : uint
	{
		// Priority level 1
		assign_result = 1, assign_noresult,  // = <-
		assign_add, assign_substract,        // += -=
		assign_multiply, assign_divide,      // *= /=
		assign_power,                        // ^=

		// Priority level 2
		or_word, and_word, xor_word,    // or and xor

		// Priority level 3
		less, greater, equal,                  // < > ==
		less_equal, greater_equal, not_equal,  // <= >= !=

		// Priority level 4
		add, substract,               // + -

		// Priority level 5
		multiply, divide, mod_word, div_word,   // * / mod

		// Priority level 6
		unary_minus, unary_plus,      // M P

		// Priority level 7
		not_word,   // not

		// Priority level 8
		power,   // ^

		// Special values
		//dot = 121,              // method caller
		comma = 122,
		//left_square = 123,      // [
		//right_square = 124,     // ]
		left_parenth = 125,     // ()
		right_parenth = 126,    // )
		function_call = 127     // x(...) - left_parenth turns into function_call
	}

}
