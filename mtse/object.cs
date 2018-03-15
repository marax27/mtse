using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// Object.cs
// Definition of Object class
//
//---------------------------------------------

namespace mtse
{
	public class Object
	{
		public Object(e_ObjectType t = e_ObjectType.obj_void)
		{
			type = t;
		}

		public Object(Object model)
		{
			type = model.type;
			switch (type)
			{
				case e_ObjectType.obj_bool:
					bool_value = model.bool_value;
					break;
				case e_ObjectType.obj_int:
					int_value = model.int_value;
					break;
				case e_ObjectType.obj_float:
					float_value = model.float_value;
					break;
				case e_ObjectType.obj_string:
				case e_ObjectType.obj_varcall:
					string_value = model.string_value;
					break;
			}
		}

		//--------------------------------------------------

		public enum e_ObjectType
		{
			obj_void = 0,
			obj_varcall,
			obj_arg_limiter,
			obj_int,
			obj_float,
			obj_bool,
			obj_string
		}

		//--------------------------------------------------

		public string printable()
		{
			string result = String.Empty;
			switch(type)
			{
				case e_ObjectType.obj_int:
					result = int_value.ToString();
					break;
				case e_ObjectType.obj_float:
					result = float_value.ToString("G10");
					break;
				case e_ObjectType.obj_bool:
					result = bool_value ? "%%true" : "%%false";
					break;
				case e_ObjectType.obj_string:
					result = string_value;
					break;
				default:break;
			}
			return result;
		}

		//--------------------------------------------------

		static public bool equal(double a, double b)
		{
			const double epsilon = 1e-10;
			if ((int)Math.Abs(a) == 0 || (int)Math.Abs(b) == 0)
				return a == b;
			else
				return Math.Abs(a - b) < epsilon;
		}

		//--------------------------------------------------

		public double toDouble()
		{
			double result = 0.0;
			switch(type)
			{
				case e_ObjectType.obj_int:
					result = int_value;
					break;
				case e_ObjectType.obj_float:
					result = float_value;
					break;
				case e_ObjectType.obj_bool:
					result = bool_value ? 1.0 : 0.0;
					break;
				default:
					throw new mtseException("%%toDouble_numerical_expected");
			}
			return result;
		}

		//--------------------------------------------------

		public bool belongsTo(List<e_ObjectType> list)
		{
			foreach (var i in list)
				if (i == this.type)
					return true;
			return false;
		}

		//--------------------------------------------------

		public void conditionalFloatToInt()
		{
			if (type == e_ObjectType.obj_float)
			{
				double x = float_value;
				if (equal(x, (Int64)x) /*&& (Int64)x != 0*/)
				{
					type = e_ObjectType.obj_int;
					int_value = (Int64)x;
				}
				else if (equal(x, (Int64)(x + 1)) /*&& (Int64)(x + 1) != 0*/)
				{
					type = e_ObjectType.obj_int;
					int_value = (Int64)(x + 1);
				}
				else if(equal(x, (Int64)(x - 1)) /*&& (Int64)(x - 1) != 0*/)
				{
					type = e_ObjectType.obj_int;
					int_value = (Int64)(x - 1);
				}
			}
		}

		//--------------------------------------------------

		public e_ObjectType type;

		private Int64 int_value;
		private double float_value;
		private bool bool_value;
		private string string_value;

		//--------------------------------------------------

		public Int64 IntValue
		{
			get
			{
				if(type == e_ObjectType.obj_int)
					return int_value;
				else
					throw new mtseException("%%object_not_integer");
			}
			set
			{
				if (type == e_ObjectType.obj_int)
					int_value = value;
				else
					throw new mtseException("%%object_not_integer");
			}
		}

		public double FloatValue
		{
			get
			{
				if (type == e_ObjectType.obj_float)
					return float_value;
				else
					throw new mtseException("%%object_not_floating_point");
			}
			set
			{
				if (type == e_ObjectType.obj_float)
					float_value = value;
				else
					throw new mtseException("%%object_not_floating_point");
			}
		}

		public bool BoolValue
		{
			get
			{
				if (type == e_ObjectType.obj_bool)
					return bool_value;
				else
					throw new mtseException("%%object_not_boolean");
			}
			set
			{
				if (type == e_ObjectType.obj_bool)
					bool_value = value;
				else
					throw new mtseException("%%object_not_boolean");
			}
		}

		public string StringValue
		{
			get
			{
				if (belongsTo(new List<e_ObjectType>{ e_ObjectType.obj_string, e_ObjectType.obj_varcall}))
					return string_value;
				else
					throw new mtseException("%%object_not_string_nor_varcall");
			}
			set
			{
				if (belongsTo(new List<e_ObjectType>{ e_ObjectType.obj_string, e_ObjectType.obj_varcall}))
					string_value = value;
				else
					throw new mtseException("%%object_not_string_nor_varcall");
			}
		}
	}
}
