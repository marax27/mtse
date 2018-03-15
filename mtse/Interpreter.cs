using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using marax27;

//---------------------------------------------
// 
// Interpreter.cs
// Core of the Interpreter class, the one
// responsible for parsing the code
//
//---------------------------------------------

namespace mtse
{
	public partial class Interpreter
	{
		const char DECIMAL_MARK = '.';

		public Interpreter(string code, ref Memory mem, ref Log log)
		{
			internal_memory = mem;
			internal_log = log;
			Code = code;
		}

		//--------------------------------------------------

		private string content;
		private int position;
		private Token.e_TokenType last_token_type;

		private Object code_result;
		private Memory internal_memory;
		private Log internal_log;

		private bool __flag__constant_creating;

		//--------------------------------------------------

		public string Code
		{
			get { return content; }
			set
			{
				content = value;
				position = 0;
				last_token_type = Token.e_TokenType.operator_tkn;
				__flag__constant_creating = false;
			}
		}

		public Object Result
		{
			get { return code_result; }
		}

		public string PrintableResult
		{
			get { return code_result.printable(); }
		}

		//************************************************************

		private uint getOperatorPriority(e_Oper op)
		{
			uint result = 0;

			if (op <= e_Oper.assign_power)
				result = 1;
			else if (op <= e_Oper.xor_word)
				result = 2;
			else if (op <= e_Oper.not_equal)
				result = 3;
			else if (op <= e_Oper.substract)
				result = 4;
			else if (op <= e_Oper.div_word)
				result = 5;
			else if (op <= e_Oper.unary_plus)
				result = 6;
			else if (op <= e_Oper.not_word)
				result = 7;
			else if (op <= e_Oper.power)
				result = 8;
			else
				result = (uint)op;

			return result;
		}

		//************************************************************

		public void analyze()
		{
			code_result = new Object();

			// To begin with, remove comment and white chars
			//if (content.Length != 1 || soal.belongsTo(content[0], soal.e_CharGroup.white_chars))

			if (String.IsNullOrEmpty(content))
				return;

			content = soal.trimOffGivenChars(content, soal.e_CharGroup.white_chars);

			int comment_start = 0;
			uint apostrophes = 0;
			while(comment_start < content.Length)
			{
				if (content[comment_start] == '#' && apostrophes % 2 == 0)
				{
					content = content.Substring(0, comment_start);
					break;
				}
				else if (content[comment_start] == '\'')
					++apostrophes;

				++comment_start;
			}//outOfTheLoop

			if (String.IsNullOrEmpty(content))
				return;

			//--------------------------------------------------

			Stack<Object> object_stack = new Stack<Object>();    //objects, values found in code
			Stack<e_Oper> operator_stack = new Stack<e_Oper>();  //operators found in code
			Stack<string> function_stack = new Stack<string>();  //names of functions to be called

			__flag__constant_creating = false;
			position = 0;
			Token t = getNextToken();

			while(t.type != Token.e_TokenType.eol_tkn)
			{
				switch(t.type)
				{
					//* * * * * * * * * * * * * * * * * * * * * * *
					case Token.e_TokenType.value_tkn:
						{
							Object x = new mtse.Object((Object.e_ObjectType)t.value[0]);
							string s = t.value.Substring(1);

							switch(x.type)
							{
								case Object.e_ObjectType.obj_int: x.IntValue = Int64.Parse(s); break;
								case Object.e_ObjectType.obj_float: x.FloatValue = Double.Parse(s, CultureInfo.InvariantCulture); break;
								case Object.e_ObjectType.obj_string: x.StringValue = s; break;
								// 'default' condition isn't supposed to happen
								default: throw new mtseException("%%unexpected_value_token");
							}
							object_stack.Push(x);
							break;
						}
					//* * * * * * * * * * * * * * * * * * * * * * *
					case Token.e_TokenType.operator_tkn:
						{
						_operator_analysis:
							e_Oper x = (e_Oper)t.value[0];

							// Handle right parenthesis - compute all between '(' and ')'
							if (x == e_Oper.right_parenth)
							{
								while (operator_stack.Peek() != e_Oper.left_parenth &&
								       operator_stack.Peek() != e_Oper.function_call)
									computeOperation(operator_stack.Pop(), ref object_stack, ref function_stack);

								switch(operator_stack.Peek())
								{
									case e_Oper.left_parenth:
										operator_stack.Pop();
										break;
									case e_Oper.function_call:
										{
											operator_stack.Pop();
											callFunction(function_stack.Pop(), ref object_stack);
											break;
										}
									default:
										throw new mtseException("%%right_parenth_lacks_left_parenth");
								}
							}
							else if (x == e_Oper.comma)
							{
								if (operator_stack.Count() == 0)
									throw new mtseException("%%not_enough_args_provided");  //should be already handled in getNextToken()
								while (operator_stack.Peek() != e_Oper.left_parenth &&
									   operator_stack.Peek() != e_Oper.function_call)
									computeOperation(operator_stack.Pop(), ref object_stack, ref function_stack);
							}
							else if (operator_stack.Count() == 0 ||
										getOperatorPriority(x) > getOperatorPriority(operator_stack.Peek()) ||
										operator_stack.Peek() == e_Oper.left_parenth ||
										operator_stack.Peek() == e_Oper.function_call ||
										getOperatorPriority(x) == 6 ||
										getOperatorPriority(x) == 7)
							{
								operator_stack.Push(x);
							}
							else
							{
								// Compute last operations on the stack
								// with greater priority than 'x'
								computeOperation(operator_stack.Pop(), ref object_stack, ref function_stack);
								goto _operator_analysis;
							}
							break;
						}
					//* * * * * * * * * * * * * * * * * * * * * * *
					case Token.e_TokenType.function_tkn:
						{
							Object obj = new Object(Object.e_ObjectType.obj_arg_limiter);

							object_stack.Push(obj);
							operator_stack.Push(e_Oper.function_call);
							function_stack.Push(t.value);
							break;
						}
					//* * * * * * * * * * * * * * * * * * * * * * *
					case Token.e_TokenType.variable_tkn:
						{
							// Handle keywords
							if(t.value == "const")
							{
								// Create a constant
								if (__flag__constant_creating)
									throw new mtseException("%%const_repetition");
								else
									__flag__constant_creating = true;
							}
							else
							{
								Object obj = new Object(Object.e_ObjectType.obj_varcall);
								obj.StringValue = t.value;
								object_stack.Push(obj);
							}
							break;
						}
					//* * * * * * * * * * * * * * * * * * * * * * *
					case Token.e_TokenType.eol_tkn:
						break;
				}
				t = getNextToken();
			}

			// End Of Line (EOL)
			while (operator_stack.Count() != 0)
				computeOperation(operator_stack.Pop(), ref object_stack, ref function_stack);

			// Eventually, operator_stack ought to contain no elements,
			// and object_stack - 1 element. Different state occurs when
			// user's code syntax is incorrect

			if (operator_stack.Count() != 0 || object_stack.Count() > 1)
				throw new mtseException("%%incorrect_syntax");

			if (object_stack.Count() == 0)
			{
				Object x = new Object(Object.e_ObjectType.obj_void);
				object_stack.Push(x);
			}

			this.code_result = object_stack.Pop();

			if (code_result.type == Object.e_ObjectType.obj_varcall)
				code_result = internal_memory.getVariable(code_result.StringValue);
		}

		//************************************************************

		private Token getNextToken()
		{
			Token default_token = new Token(Token.e_TokenType.eol_tkn, String.Empty);

			if (position >= content.Length)
				return default_token;

			// COT - Create Operator Token
			Func<e_Oper, string> COT = (e_Oper op) => { return new string((char)op, 1); };

			var ffno = soal.findFirstNotOf(content, soal.e_CharGroup.white_chars, position);
			if (ffno == soal.NPOS)
				return default_token;
			else
				position = ffno;

			char first = content[position++];
			Token result = new Token();

			//--------------------------------------------------
			if (soal.belongsTo(first, soal.e_CharGroup.digits))
			{
				// Parser has come across a number
				result.type = Token.e_TokenType.value_tkn;

				// Placing entire number in 'value'
				string value = new string(first, 1);
				while (position < content.Length)
				{
					if (soal.belongsTo(content[position], soal.e_CharGroup.digits) || content[position] == DECIMAL_MARK)
						value += content[position++];
					else break;
				}

				// Excluding results which have got too many decimal marks
				var dm = soal.getAmountOf(DECIMAL_MARK, value);
				if (dm > 1)
					throw new mtseException("%%too_many_decimal_marks_in_number", value);

				last_token_type = Token.e_TokenType.value_tkn;
				if (dm == 0)
					result.value = ((char)Object.e_ObjectType.obj_int).ToString() + value;
				else
					result.value = ((char)Object.e_ObjectType.obj_float).ToString() + value;

				return result;
			}
			//--------------------------------------------------
			else if (first == '\'')
			{
				// Parser has found a string

				bool __flag__end_of_string = false;  //necessary to raise exception
													 //when string is not ended properly
				result.type = Token.e_TokenType.value_tkn;
				result.value = ((char)Object.e_ObjectType.obj_string).ToString();

				// Reading a string (an MTSE string, not just an ordinary, peasant string)
				while (position < content.Length)
				{
					char c = content[position++];
					if (c != '\'')
						result.value += c;
					else
					{
						__flag__end_of_string = true;
						break;
					}
				}

				// Handle invalid end of string
				if (!__flag__end_of_string)
					throw new mtseException("%%invalid_end_of_string");

				last_token_type = Token.e_TokenType.value_tkn;
				return result;
			}
			//--------------------------------------------------
			else if (soal.belongsTo(first, "=<>-+/^*().,!"))
			{
				// Parser has found an operator

				result.type = Token.e_TokenType.operator_tkn;

				// Handle unary operators
				if (last_token_type == Token.e_TokenType.operator_tkn || last_token_type == Token.e_TokenType.function_tkn)
				{
					switch (first)
					{
						case '+': result.value = COT(e_Oper.unary_plus); return result;
						case '-': result.value = COT(e_Oper.unary_minus); return result;
						default: break;
					}
				}

				// Handle situation "a(b)" - convert to "a*(b)" (unless 'a' is a name)
				if (first == '(' && last_token_type == Token.e_TokenType.value_tkn)
				{
					last_token_type = Token.e_TokenType.operator_tkn;
					--position;
					result.value = COT(e_Oper.multiply);
					return result;
				}

				// Handle a comma
				if (first == ',')
				{
					if (last_token_type == Token.e_TokenType.operator_tkn)
						throw new mtseException(position <= 1 ? "%%comma_at_the_beginning" : "%%comma_after_operator");

					last_token_type = Token.e_TokenType.operator_tkn;
					result = new Token(Token.e_TokenType.operator_tkn, COT(e_Oper.comma));
					return result;
				}

				// Handle "(a+b)(c+d)" --> "(a+b)*(c+d)"
				last_token_type = (first == ')') ? Token.e_TokenType.value_tkn : Token.e_TokenType.operator_tkn;

				// Handle binary operators
				result.type = Token.e_TokenType.operator_tkn;
				switch (first)
				{
					case '+':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.assign_add); ++position; break;
								default: result.value = COT(e_Oper.add); break;
							}
							break;
						}
					case '-':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.assign_substract); ++position; break;
								default: result.value = COT(e_Oper.substract); break;
							}
							break;
						}
					case '*':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.assign_multiply); ++position; break;
								default: result.value = COT(e_Oper.multiply); break;
							}
							break;
						}
					case '/':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.assign_divide); ++position; break;
								default: result.value = COT(e_Oper.divide); break;
							}
							break;
						}
					case '^':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.assign_power); ++position; break;
								default: result.value = COT(e_Oper.power); break;
							}
							break;
						}
					case '(': result.value = COT(e_Oper.left_parenth); break;
					case ')': result.value = COT(e_Oper.right_parenth); break;
					case '=':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.equal); ++position; break;
								default: result.value = COT(e_Oper.assign_noresult); break;
							}
							break;
						}
					case '<':
						{
							switch (content[position])
							{
								case '-': result.value = COT(e_Oper.assign_result); ++position; break;
								case '=': result.value = COT(e_Oper.less_equal); ++position; break;
								default: result.value = COT(e_Oper.less); break;
							}
							break;
						}
					case '>':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.greater_equal); ++position; break;
								default: result.value = COT(e_Oper.greater); break;
							}
							break;
						}
					case '!':
						{
							switch (content[position])
							{
								case '=': result.value = COT(e_Oper.not_equal); ++position; break;
								default: throw new mtseException("%%unknown_character", '!');
							}
							break;
						}
					default: break;
				}

				return result;
			}
			//--------------------------------------------------
			else if (soal.belongsTo(first, soal.e_CharGroup.uppercased, soal.e_CharGroup.lowercased) || first == '_')
			{
				// Parser located a word

				// Placing entire word in a string
				string name = new string(first, 1);

				//Func<soal.e_CharGroup, bool> bt = (soal.e_CharGroup g) => { return soal.belongsTo(content[position], g); };

				while (position < content.Length)
				{
					if (soal.belongsTo(content[position], soal.e_CharGroup.digits, soal.e_CharGroup.lowercased, soal.e_CharGroup.uppercased) || content[position] == '_')
						name += content[position++];
					else
						break;
				}

				last_token_type = Token.e_TokenType.operator_tkn;
				result.type = Token.e_TokenType.operator_tkn;

				// Seek keywords - at first, operator words
				if (name == "or") result.value = COT(e_Oper.or_word);
				else if (name == "and") result.value = COT(e_Oper.and_word);
				else if (name == "xor") result.value = COT(e_Oper.xor_word);
				else if (name == "mod") result.value = COT(e_Oper.mod_word);
				else if (name == "div") result.value = COT(e_Oper.div_word);
				else if (name == "not") result.value = COT(e_Oper.not_word);
				else
				{
					result.value = name;
					position = soal.findFirstNotOf(content, soal.e_CharGroup.white_chars, position);
					if (position == soal.NPOS)
						last_token_type = Token.e_TokenType.variable_tkn;
					else if (content[position] == '(')
					{
						++position;
						last_token_type = Token.e_TokenType.function_tkn;
					}
					else
						last_token_type = Token.e_TokenType.variable_tkn;
				}

				result.type = last_token_type;
				return result;
			}
			//--------------------------------------------------
			else throw new mtseException("%%unknown_character", first);

			// return default_token;
		}

		//************************************************************

		private void computeOperation(e_Oper operation, ref Stack<Object> operands, ref Stack<string> functions)
		{
			//--------------------------------------------------------
			Func<Object, Object> varToValue = (Object x) =>
			{
				if (x.type == Object.e_ObjectType.obj_varcall)
					x = internal_memory.getVariable(x.StringValue);
				return x;
			};
			//--------------------------------------------------------

			// Handle function call
			if(operation == e_Oper.function_call)
			{
				if (functions.Count() == 0)
					throw new mtseException("%%no_functions_provided");

				string s = functions.Pop();
				callFunction(s, ref operands);
			}
			else
			{
				Object result = new Object();
				bool unary_found = false;

				var arg2 = operands.Pop();
				arg2 = varToValue(arg2);
				arg2.conditionalFloatToInt();

				// At first, assignment operators
				if(getOperatorPriority(operation) == 1)
				{
					var arg1 = operands.Pop();
					// Don't use varToValue() yet

					switch (operation)
					{
						case e_Oper.assign_noresult:
							result = compute_assign_noresult(arg1, arg2, ref internal_memory, ref __flag__constant_creating); break;
						case e_Oper.assign_result:
							result = compute_assign_result(arg1, arg2, ref internal_memory, ref __flag__constant_creating); break;
						case e_Oper.assign_add:
							result = compute_assign_add(arg1, arg2, ref internal_memory); break;
						case e_Oper.assign_substract:
							result = compute_assign_substract(arg1, arg2, ref internal_memory); break;
						case e_Oper.assign_multiply:
							result = compute_assign_multiply(arg1, arg2, ref internal_memory); break;
						case e_Oper.assign_divide:
							result = compute_assign_divide(arg1, arg2, ref internal_memory); break;
						case e_Oper.assign_power:
							result = compute_assign_power(arg1, arg2, ref internal_memory); break;
						default: break;
					}
				}
				else
				{
					// Unary operators
					switch(operation)
					{
						case e_Oper.unary_minus:
							result = compute_unary_minus(arg2); unary_found = true; break;
						case e_Oper.unary_plus:
							result = compute_unary_plus(arg2); unary_found = true; break;
						case e_Oper.not_word:
							result = compute_not_word(arg2); unary_found = true; break;
						default: break;
					}


					// Binary operators
					if (!unary_found)
					{
						var arg1 = operands.Pop();
						arg1 = varToValue(arg1);
						arg1.conditionalFloatToInt();

						switch(operation)
						{
							case e_Oper.or_word:
								result = compute_or_word(arg1, arg2); break;
							case e_Oper.and_word:
								result = compute_and_word(arg1, arg2); break;
							case e_Oper.xor_word:
								result = compute_xor_word(arg1, arg2); break;

							case e_Oper.less:
								result = compute_less(arg1, arg2); break;
							case e_Oper.greater:
								result = compute_greater(arg1, arg2); break;
							case e_Oper.equal:
								result = compute_equal(arg1, arg2); break;
							case e_Oper.less_equal:
								result = compute_less_equal(arg1, arg2); break;
							case e_Oper.greater_equal:
								result = compute_greater_equal(arg1, arg2); break;
							case e_Oper.not_equal:
								result = compute_not_equal(arg1, arg2); break;

							case e_Oper.add:
								result = compute_add(arg1, arg2); break;
							case e_Oper.substract:
								result = compute_substract(arg1, arg2); break;

							case e_Oper.multiply:
								result = compute_multiply(arg1, arg2); break;
							case e_Oper.divide:
								result = compute_divide(arg1, arg2); break;
							case e_Oper.mod_word:
								result = compute_mod_word(arg1, arg2); break;
							case e_Oper.div_word:
								result = compute_div_word(arg1, arg2); break;

							case e_Oper.power:
								result = compute_power(arg1, arg2); break;

							default:
								throw new mtseException("%%unknown_operator");
						}
					}
				}

				operands.Push(result);
			}
		}

		//************************************************************
	}
}
