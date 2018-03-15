using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// operators.cs
// Computes all implemented operators
//
//---------------------------------------------

namespace mtse
{
	partial class Interpreter
	{
		static private readonly List<Object.e_ObjectType> numerical_types = new List<Object.e_ObjectType>
		{ Object.e_ObjectType.obj_int, Object.e_ObjectType.obj_float, Object.e_ObjectType.obj_bool };

		static bool isVarcall(Object a)
		{
			return a.type == Object.e_ObjectType.obj_varcall;
		}

		static bool numberToBool(Object obj, string operator_name)
		{
			bool result = false;
			switch(obj.type)
			{
				case Object.e_ObjectType.obj_int:
					result = obj.IntValue != 0; break;
				case Object.e_ObjectType.obj_float:
					result = !Object.equal(obj.FloatValue, 0.0); break;
				case Object.e_ObjectType.obj_bool:
					result = obj.BoolValue; break;
				default:
					throw new mtseException("%%operator_failure", operator_name);
			}
			return result;
		}

		static bool bothBelongTo(Object a, Object b, params Object.e_ObjectType[] types)
		{
			bool _a = false, _b = false;
			foreach(var i in types)
			{
				if (a.type == i)
					_a = true;
				if (b.type == i)
					_b = true;
			}
			return _a && _b;
		}

		static bool bothBelongTo(Object a, Object b, List<Object.e_ObjectType> list)
		{
			return a.belongsTo(list) && b.belongsTo(list);
		}

		//************************************************************

		static Object compute_assign_noresult(Object left, Object right, ref Memory mem, ref bool constant_creating)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "=");

			if (constant_creating)
			{
				mem.createConstant(left.StringValue, right);
				constant_creating = false;
			}
			else
				mem.modifyVariable(left.StringValue, right);
			return new Object();
		}

		//************************************************************

		static Object compute_assign_result(Object left, Object right, ref Memory mem, ref bool constant_creating)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "<-");

			if (constant_creating)
			{
				mem.createConstant(left.StringValue, right);
				constant_creating = false;
			}
			else
				mem.modifyVariable(left.StringValue, right);
			return right;
		}

		//************************************************************

		static Object compute_assign_add(Object left, Object right, ref Memory mem)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "+=");

			Object a = mem.getVariable(left.StringValue);
			try
			{
				a = compute_add(a, right);
			}
			catch (mtseException)
			{
				throw new mtseException("%%operator_failure", "+=");
			}

			mem.modifyVariable(left.StringValue, a);
			return new Object();
		}

		//**********************************************************************

		static Object compute_assign_substract(Object left, Object right, ref Memory mem)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "-=");

			Object a = mem.getVariable(left.StringValue);
			try
			{
				a = compute_substract(a, right);
			}
			catch (mtseException)
			{
				throw new mtseException("%%operator_failure", "-=");
			}

			mem.modifyVariable(left.StringValue, a);
			return new Object();
		}

		//**********************************************************************

		static Object compute_assign_multiply(Object left, Object right, ref Memory mem)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "*=");

			Object a = mem.getVariable(left.StringValue);
			try
			{
				a = compute_multiply(a, right);
			}
			catch (mtseException)
			{
				throw new mtseException("%%operator_failure", "*=");
			}

			mem.modifyVariable(left.StringValue, a);
			return new Object();
		}

		//**********************************************************************

		static Object compute_assign_divide(Object left, Object right, ref Memory mem)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "/=");

			Object a = mem.getVariable(left.StringValue);
			try
			{
				a = compute_divide(a, right);
			}
			catch (mtseException)
			{
				throw new mtseException("%%operator_failure", "/=");
			}

			mem.modifyVariable(left.StringValue, a);
			return new Object();
		}

		//**********************************************************************

		static Object compute_assign_power(Object left, Object right, ref Memory mem)
		{
			if (!isVarcall(left))
				throw new mtseException("%%operator_failure", "^=");

			Object a = mem.getVariable(left.StringValue);
			try
			{
				a = compute_power(a, right);
			}
			catch (mtseException)
			{
				throw new mtseException("%%operator_failure", "^=");
			}

			mem.modifyVariable(left.StringValue, a);
			return new Object();
		}

		//**********************************************************************

		static Object compute_or_word(Object left, Object right)
		{
			bool l = numberToBool(left, "or"),
				 r = numberToBool(right, "or");

			Object result = new Object(Object.e_ObjectType.obj_bool);
			result.BoolValue = l || r;
			return result;
		}

		//**********************************************************************

		static Object compute_and_word(Object left, Object right)
		{
			bool l = numberToBool(left, "and"),
				 r = numberToBool(right, "and");

			Object result = new Object(Object.e_ObjectType.obj_bool);
			result.BoolValue = l && r;
			return result;
		}

		//**********************************************************************

		static Object compute_xor_word(Object left, Object right)
		{
			bool l = numberToBool(left, "xor"),
				 r = numberToBool(right, "xor");

			Object result = new Object(Object.e_ObjectType.obj_bool);
			result.BoolValue = (l && !r) || (!l && r);
			return result;
		}

		//**********************************************************************

		static Object compute_less(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", "<");

			Object result = new Object(Object.e_ObjectType.obj_bool);

			if (bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				result.BoolValue = (left.IntValue < right.IntValue);
			else
				result.BoolValue = (left.toDouble() < right.toDouble());

			return result;
		}

		//**********************************************************************

		static Object compute_greater(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", ">");

			Object result = new Object(Object.e_ObjectType.obj_bool);

			if (bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				result.BoolValue = (left.IntValue > right.IntValue);
			else
				result.BoolValue = (left.toDouble() > right.toDouble());

			return result;
		}

		//**********************************************************************

		static Object compute_equal(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types) &&
				!bothBelongTo(left, right, Object.e_ObjectType.obj_string))
				throw new mtseException("%%operator_failure", "==");

			Object result = new Object(Object.e_ObjectType.obj_bool);

			if (bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				result.BoolValue = (left.IntValue == right.IntValue);
			else if (left.type == Object.e_ObjectType.obj_string)  //left.type==right.type
				result.BoolValue = (left.StringValue == right.StringValue);
			else
				result.BoolValue = Object.equal(left.toDouble(), right.toDouble());

			return result;
		}

		//**********************************************************************

		static Object compute_less_equal(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", "<=");

			Object result = new Object(Object.e_ObjectType.obj_bool);

			if (bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				result.BoolValue = (left.IntValue <= right.IntValue);
			else
			{
				double x = left.toDouble(),
					   y = right.toDouble();
				result.BoolValue = Object.equal(x, y) || x < y;
			}

			return result;
		}

		//**********************************************************************

		static Object compute_greater_equal(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", ">=");

			Object result = new Object(Object.e_ObjectType.obj_bool);

			if (bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				result.BoolValue = (left.IntValue >= right.IntValue);
			else
			{
				double x = left.toDouble(),
					   y = right.toDouble();
				result.BoolValue = Object.equal(x, y) || x > y;
			}

			return result;
		}

		//**********************************************************************

		static Object compute_not_equal(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", "!=");

			Object result = new Object(Object.e_ObjectType.obj_bool);

			if (bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				result.BoolValue = (left.IntValue != right.IntValue);
			else
				result.BoolValue = !Object.equal(left.toDouble(), right.toDouble());

			return result;
		}

		//**********************************************************************

		static Object compute_add(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types) &&
				!bothBelongTo(left, right, Object.e_ObjectType.obj_string))
				throw new mtseException("%%operator_failure", "+");

			Object result = new Object();

			if(bothBelongTo(left, right, Object.e_ObjectType.obj_string))
			{
				result.type = Object.e_ObjectType.obj_string;
				result.StringValue = left.StringValue + right.StringValue;
			}
			//else if(bothBelongTo(left, right, Object.e_ObjectType.obj_int))
			else
			{
				result.type = Object.e_ObjectType.obj_float;
				result.FloatValue = left.toDouble() + right.toDouble();
				result.conditionalFloatToInt();
			}

			return result;
		}

		//**********************************************************************

		static Object compute_substract(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", "-");

			Object result = new Object();

			result.type = Object.e_ObjectType.obj_float;
			result.FloatValue = left.toDouble() - right.toDouble();
			result.conditionalFloatToInt();

			return result;
		}

		//**********************************************************************

		static Object compute_multiply(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types) &&
				left.type != Object.e_ObjectType.obj_string && right.type != Object.e_ObjectType.obj_string)
				throw new mtseException("%%operator_failure", "*");

			Object result = new Object();

			// 'xyz' * 3 == 'xyzxyzxyz'
			if ((left.type == Object.e_ObjectType.obj_string && right.type == Object.e_ObjectType.obj_int) ||
				(left.type == Object.e_ObjectType.obj_int && right.type == Object.e_ObjectType.obj_string))
			{
				result.type = Object.e_ObjectType.obj_string;
				result.StringValue = "";
				string s = "";
				Int64 n = 0;
				switch(left.type)
				{
					case Object.e_ObjectType.obj_string:
						s = left.StringValue;  n = right.IntValue;  break;
					default:
						s = right.StringValue;  n = left.IntValue;  break;
				}

				if(n < 1)
					throw new mtseException("%%operator_failure", "*");

				for (int i = 0; i != n; ++i)
					result.StringValue += s;
			}
			else
			{
				result.type = Object.e_ObjectType.obj_float;
				result.FloatValue = left.toDouble() * right.toDouble();
				result.conditionalFloatToInt();
			}

			return result;
		}

		//**********************************************************************

		static Object compute_divide(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", "/");

			Object result = new Object();

			result.type = Object.e_ObjectType.obj_float;
			result.FloatValue = left.toDouble() / right.toDouble();
			result.conditionalFloatToInt();

			return result;
		}

		//**********************************************************************

		static Object compute_mod_word(Object left, Object right)
		{
			if (!bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				throw new mtseException("%%operator_failure", "mod");

			Object result = new Object(Object.e_ObjectType.obj_int);
			result.IntValue = left.IntValue % right.IntValue;
			return result;
		}

		//**********************************************************************

		static Object compute_div_word(Object left, Object right)
		{
			if (!bothBelongTo(left, right, Object.e_ObjectType.obj_int))
				throw new mtseException("%%operator_failure", "div");

			Object result = new Object(Object.e_ObjectType.obj_int);
			result.IntValue = (Int64)(left.IntValue / right.IntValue);
			return result;
		}

		//**********************************************************************

		static Object compute_unary_minus(Object right)
		{
			Object result = new Object(right.type);

			switch(right.type)
			{
				case Object.e_ObjectType.obj_int:
					result.IntValue = -(right.IntValue);  break;
				case Object.e_ObjectType.obj_float:
					result.FloatValue = -(right.FloatValue);  break;
				default:
					throw new mtseException("%%operator_failure", "- (1arg.)");
			}

			return result;
		}

		//**********************************************************************

		static Object compute_unary_plus(Object right)
		{
			if (!right.belongsTo(numerical_types))
				throw new mtseException("%%operator_failure", "+ (1arg.)");

			return right;
		}

		//**********************************************************************

		static Object compute_not_word(Object right)
		{
			if (!right.belongsTo(numerical_types))
				throw new mtseException("%%operator_failure", "not");

			Object result = new Object(Object.e_ObjectType.obj_bool);
			result.BoolValue = Object.equal(right.toDouble(), 0.0);
			return result;
		}

		//**********************************************************************

		static Object compute_power(Object left, Object right)
		{
			if (!bothBelongTo(left, right, numerical_types))
				throw new mtseException("%%operator_failure", "^");

			double l = left.toDouble(),
				   r = right.toDouble();

			Object result = new Object(Object.e_ObjectType.obj_float);
			result.FloatValue = Math.Pow(l, r);
			result.conditionalFloatToInt();
			return result;
		}

		//**********************************************************************
	}
}
