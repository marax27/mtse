using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// Log.cs
// The class responsible for printing text
//
//---------------------------------------------

namespace mtse
{
	public class Log
	{
		public void write(string str)
		{
			content.Add(str);
		}

		public void write(char ch)
		{
			content.Add(ch.ToString());
		}

		public List<string> read()
		{
			return content;
		}

		public void clear()
		{
			content.Clear();
		}

		public bool isEmpty()
		{
			return content.Count == 0;
		}

		//private string content;
		private List<string> content = new List<string>();
	}
}
