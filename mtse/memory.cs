using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// Memory.cs
// Responsible for storing MTSE variables
//
//---------------------------------------------

namespace mtse
{
	public class Memory
	{
		public Memory()
		{
			constants = new Dictionary<string, Object>();
			localvars = new Dictionary<string, Object>();
			state_altered = false;
		}

		public Object getVariable(string name)
		{
			// Returns the object labeled 'name'
			var entry = constants.FirstOrDefault(e => e.Key == name);
			if(entry.Key == null)
			{
				entry = localvars.FirstOrDefault(f => f.Key == name);
				if (entry.Key == null)
					throw new mtseException("%%variable_not_found", name);
			}
			return entry.Value;
		}

		public void modifyVariable(string name, Object new_value)
		{
			// To start with, looking for constants
			var entry = constants.FirstOrDefault(e => e.Key == name);
			if (entry.Key != null)
				throw new mtseException("%%modify_constant", name);

			localvars[name] = new_value;
			state_altered = true;
		}

		public void createConstant(string name, Object new_value)
		{
			// If name is not used, creates a constant
			var entry = this.constants.FirstOrDefault(e => e.Key == name);
			if (entry.Key != null)
				throw new mtseException("%%modify_constant", name);
			else
			{
				entry = localvars.FirstOrDefault(f => f.Key == name);
				if (entry.Key != null)
					throw new mtseException("%%variable_already_declared", name);

				constants[name] = new_value;
				state_altered = true;
			}
		}

		public void forgetVariable(string name)
		{
			// Removes chosen local variable
			localvars.Remove(name);
			state_altered = true;
		}

		//----------------------------------------

		public bool hasBeenAltered() { return state_altered; }
		public void clearState() { state_altered = false; }

		public Dictionary<string, Object> Constants
		{
			get { return constants; }
		}

		public Dictionary<string, Object> LocalVariables
		{
			get { return localvars; }
		}

		//----------------------------------------

		private Dictionary<string, Object> constants, localvars;
		private bool state_altered;
	}
}
