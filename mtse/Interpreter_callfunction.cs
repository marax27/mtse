using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// Interpreter_callfunction.cs
// Handling built-in functions
//
//---------------------------------------------

namespace mtse
{
	partial class Interpreter
	{
		private void callFunction(string name, ref Stack<Object> operands)
		{
			//--------------------------------------------------------
			/*Func<Object, Int64> getInteger = (Object obj) =>
			{
				Int64 res = 0;
				obj.conditionalFloatToInt();
				switch(obj.type)
				{
					case Object.e_ObjectType.obj_int:
						res = obj.IntValue;  break;
					case Object.e_ObjectType.obj_bool:
						res = obj.BoolValue ? 1 : 0;  break;
					default:
						throw new mtseException("%%invalid_argument_type");
				}
				return res;
			};*/
			//--------------------------------------------------------

			// Create a list of function arguments
			List<Object> flipped = new List<Object>(),
						 arguments = new List<Object>();

			while (operands.Peek().type != Object.e_ObjectType.obj_arg_limiter)
				flipped.Add(operands.Pop());
			operands.Pop();  //obj_arg_limiter

			for (int i = flipped.Count() - 1; i >= 0; --i)
				arguments.Add(flipped[i]);

			//--------------------------------------------------------

			int ARGC = arguments.Count();

			Func<int, string, bool> verifyArgNumber = (int expected, string fname) =>
			{
				if (expected != ARGC)
					throw new mtseException("%%invalid_function_arg_number", fname, expected, ARGC);
				return true;
			};

			Func<Object.e_ObjectType, string, bool> checkIfNumerical = (Object.e_ObjectType type, string fname) =>
			{
				if (type == Object.e_ObjectType.obj_float || type == Object.e_ObjectType.obj_int || type == Object.e_ObjectType.obj_bool)
					return true;
				else throw new mtseException("%%invalid_argument_type", fname);
			};

			for (int i = 0; i != arguments.Count(); ++i)
				if (arguments[i].type == Object.e_ObjectType.obj_varcall)
					arguments[i] = internal_memory.getVariable(arguments[i].StringValue);

			Object result = new Object();
			result.type = Object.e_ObjectType.obj_float;

			// Call an appropriate function

			// Trigonometric
			if (name == "cos")
			{
				verifyArgNumber(1, "cos");
				checkIfNumerical(arguments[0].type, "cos");
				result.FloatValue = Math.Cos(arguments[0].toDouble());
			}
			else if (name == "sin")
			{
				verifyArgNumber(1, "sin");
				checkIfNumerical(arguments[0].type, "sin");
				result.FloatValue = Math.Sin(arguments[0].toDouble());
			}
			else if (name == "tan" || name == "tg")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				result.FloatValue = Math.Tan(arguments[0].toDouble());
			}
			else if (name == "cot" || name == "ctg")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				result.FloatValue = 1.0 / Math.Tan(arguments[0].toDouble());
			}

			// Inverse trigonometric
			else if (name == "arccos")
			{
				verifyArgNumber(1, "arccos");
				checkIfNumerical(arguments[0].type, "arccos");
				result.FloatValue = Math.Acos(arguments[0].toDouble());
			}
			else if (name == "arcsin")
			{
				verifyArgNumber(1, "arcsin");
				checkIfNumerical(arguments[0].type, "arcsin");
				result.FloatValue = Math.Asin(arguments[0].toDouble());
			}
			else if (name == "arctan" || name == "arctg")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				result.FloatValue = Math.Atan(arguments[0].toDouble());
			}
			else if (name == "arccot" || name == "arcctg")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				var x = arguments[0].toDouble();
				if (x == 0.0)
					result.FloatValue = Math.PI / 2;  //arcctg(0) = 90deg
				else
					result.FloatValue = Math.Atan(1.0 / arguments[0].toDouble());
			}

			// Hiperbolic
			else if (name == "cosh")
			{
				verifyArgNumber(1, "cosh");
				checkIfNumerical(arguments[0].type, "cosh");
				result.FloatValue = Math.Cosh(arguments[0].toDouble());
			}
			else if (name == "sinh")
			{
				verifyArgNumber(1, "sinh");
				checkIfNumerical(arguments[0].type, "sinh");
				result.FloatValue = Math.Sinh(arguments[0].toDouble());
			}
			else if (name == "tanh" || name == "tgh")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				result.FloatValue = Math.Tanh(arguments[0].toDouble());
			}
			else if (name == "coth" || name == "ctgh")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				var x = arguments[0].toDouble();
				result.FloatValue = Math.Cos(x) / Math.Sin(x);
			}

			// Inverted hiperbolic
			else if (name == "arcosh")
			{
				verifyArgNumber(1, "arcosh");
				checkIfNumerical(arguments[0].type, "arcosh");
				var x = arguments[0].toDouble();
				result.FloatValue = Math.Log(x + Math.Sqrt(x - 1.0) * Math.Sqrt(x + 1.0));
			}
			else if (name == "arsinh")
			{
				verifyArgNumber(1, "arsinh");
				checkIfNumerical(arguments[0].type, "arsinh");
				var x = arguments[0].toDouble();
				result.FloatValue = Math.Log(x + Math.Sqrt(x * x + 1.0));
			}
			else if (name == "artanh" || name == "artgh")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				var x = arguments[0].toDouble();
				result.FloatValue = 0.5 * Math.Log((x + 1.0) / (1.0 - x));
			}
			else if (name == "arctgh" || name == "arcoth")
			{
				verifyArgNumber(1, name);
				checkIfNumerical(arguments[0].type, name);
				var x = arguments[0].toDouble();
				result.FloatValue = 0.5 * Math.Log((x + 1.0) / (x - 1.0));
			}

			// Logarithms
			else if (name == "log")
			{
				// log(b, a) <==> log a
				//                   b

				verifyArgNumber(2, "log");
				checkIfNumerical(arguments[0].type, "log");
				checkIfNumerical(arguments[1].type, "log");
				var a = arguments[1].toDouble();
				var b = arguments[0].toDouble();

				if (a <= 0.0)
					throw new mtseException("%%domain_error", "log", a, "musi być większe od 0");
				if (b <= 0.0 || b == 1.0)
					throw new mtseException("%%domain_error", "log", b, "musi być większe od 0 i różne od 1");

				result.FloatValue = Math.Log(a, b);
			}
			else if (name == "ln")
			{
				verifyArgNumber(1, "log");
				checkIfNumerical(arguments[0].type, "ln");
				var x = arguments[0].toDouble();

				if (x <= 0.0)
					throw new mtseException("%%domain_error", "ln", x, "musi być większe od 0");

				result.FloatValue = Math.Log(x);
			}

			// Other math functions
			else if (name == "abs")
			{
				verifyArgNumber(1, "abs");
				checkIfNumerical(arguments[0].type, "abs");
				var a = arguments[0];
				result.type = arguments[0].type;
				switch (a.type)
				{
					case Object.e_ObjectType.obj_float:
						result.FloatValue = Math.Abs(a.FloatValue);
						break;
					case Object.e_ObjectType.obj_int:
						result.FloatValue = Math.Abs(a.IntValue);
						break;
					case Object.e_ObjectType.obj_bool:
						result.type = Object.e_ObjectType.obj_int;
						result.IntValue = a.BoolValue ? 1 : 0;
						break;
					default: break;
				}
			}
			else if (name == "sqrt")
			{
				verifyArgNumber(1, "sqrt");
				checkIfNumerical(arguments[0].type, "sqrt");
				var x = arguments[0].toDouble();

				if (x < 0.0)
					throw new mtseException("%%domain_error", "sqrt", x, "%%greater_or_equal_zero");

				result.FloatValue = Math.Sqrt(x);
			}
			else if (name == "sgn")
			{
				verifyArgNumber(1, "sgn");
				checkIfNumerical(arguments[0].type, "sgn");
				var x = arguments[0].toDouble();
				result.type = Object.e_ObjectType.obj_int;
				result.IntValue = (x > 0.0 ? 1 : (x < 0.0 ? -1 : 0));
			}
			else if (name == "factorial")
			{
				verifyArgNumber(1, "factorial");
				arguments[0].conditionalFloatToInt();
				if (arguments[0].type != Object.e_ObjectType.obj_int)
					throw new mtseException("%%invalid_argument_type", "factorial");

				Int64 n = arguments[0].IntValue;
				if (n < 0)
					throw new mtseException("%%domain_error", "factorial", n, "%%greater_or_equal_zero");

				result.type = Object.e_ObjectType.obj_int;
				result.IntValue = 1;
				for (Int64 i = 2; i <= n; ++i)
					result.IntValue *= i;
			}

			// Other
			else if (name == "exit")
				Environment.Exit(0);
			else if (name == "print")
			{
				foreach (var i in arguments)
					internal_log.write(i.printable());
				result.type = Object.e_ObjectType.obj_void;
			}
			else if (name == "println")
			{
				foreach (var i in arguments)
					internal_log.write(i.printable());
				internal_log.write('\n');
				result.type = Object.e_ObjectType.obj_void;
			}
			else if (name == "exec")
			{
				verifyArgNumber(1, "exec");
				if (arguments[0].type != Object.e_ObjectType.obj_string)
					throw new mtseException("%%invalid_argument_type", "exec");
				Interpreter inter = new Interpreter(arguments[0].StringValue, ref internal_memory, ref internal_log);
				inter.analyze();
				result = inter.Result;
			}
			else if (name == "import")
			{
				if (ARGC == 0)
					throw new mtseException("%%invalid_function_arg_number", "import", ">0", "0");

				foreach (var arg in arguments)
					if (arg.type != Object.e_ObjectType.obj_string)
						throw new mtseException("%%invalid_argument_type", "import");

				string[] lines = new string[] { };
				foreach(var f in arguments)
				{
					try{
						lines = System.IO.File.ReadAllLines(f.StringValue);
					}catch(Exception){
						throw new mtseException("%%failed_to_import", f.StringValue);
					}

					foreach(var l in lines)
					{
						Interpreter inter = new Interpreter(l, ref internal_memory, ref internal_log);
						inter.analyze();
					}
				}
				result.type = Object.e_ObjectType.obj_void;
			}
			else
				throw new mtseException("%%unknown_function", name);

			result.conditionalFloatToInt();
			operands.Push(result);
		}
	}
}
