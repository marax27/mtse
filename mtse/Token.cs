using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// Token.cs
// Definition of Token class
//
//---------------------------------------------

namespace mtse
{
	partial class Interpreter
	{
		//************************************************************

		class Token
		{
			public Token()
			{
				this.type = e_TokenType.eol_tkn;
				this.value = String.Empty;
			}

			public Token(e_TokenType t, string v)
			{
				this.type = t;
				this.value = v;
			}

			public enum e_TokenType : uint
			{
				eol_tkn, value_tkn, operator_tkn, variable_tkn, function_tkn
			}

			public e_TokenType type;
			public string value;
		}

		//************************************************************
	}
}
