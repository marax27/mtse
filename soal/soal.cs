using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------------
// 
// soal.cs
// Tiny, simple library designed to improve
// some string operations
//
//---------------------------------------------

namespace marax27
{
	public class soal
    {
		public enum e_CharGroup { digits = 1, lowercased = 2, uppercased = 4, white_chars = 8 }
		public const int NPOS = int.MaxValue;

		//************************************************************

		public class SoalException : Exception
		{
			public SoalException() : base("Program napotkał problem podczas przetwarzania danych tekstowych")
			{ }

			public SoalException(string err_msg) : base(err_msg)
			{ }
		}

		//************************************************************

		// Sprawdza, czy znak znajduje sie w str
		static public bool belongsTo(char c, string str)
		{
			return str.Contains(c.ToString());
		}

		// Sprawdza, czy c nalezy do jednej z grup podanych w tablicy groups
		static public bool belongsTo(char c, params e_CharGroup[] groups)
		{
			bool result = false;
			
			foreach(var i in groups)
			{
				switch(i)
				{
					case e_CharGroup.digits:
						result = char.IsDigit(c) || result;
						break;
					case e_CharGroup.lowercased:
						result = char.IsLower(c) || result;
						break;
					case e_CharGroup.uppercased:
						result = char.IsUpper(c) || result;
						break;
					case e_CharGroup.white_chars:
						if (c == ' ' || c == '\n' || c == '\t' || c == '\r')
							result = true;
						break;
					default:
						throw new SoalException("%%invalid_char_group");
				}
			}

			return result;
		}

		//************************************************************

		// Usuwa z poczatku i konca stringu znaki nalezace do podanej grupy
		static public string trimOffGivenChars(string s, string which_chars)
		{
			int start = 0;
			while (belongsTo(s[start], which_chars) && start < s.Length - 1)
				++start;

			// Jesli caly string sklada sie z danych znakow, zwroc pusty string
			if (start == s.Length - 1)
				return String.Empty;

			int finish = s.Length - 1;
			while (belongsTo(s[finish], which_chars) && finish > 0)
				--finish;

			return s.Substring(start, finish - start + 1);
		}

		static public string trimOffGivenChars(string s, e_CharGroup which_chars)
		{
			int start = 0;
			while (belongsTo(s[start], which_chars) && start < s.Length - 1)
				++start;

			// Jesli caly string sklada sie z danych znakow, zwroc pusty string
			if (start == s.Length - 1)
				return String.Empty;

			int finish = s.Length - 1;
			while (belongsTo(s[finish], which_chars) && finish > 0)
				--finish;

			return s.Substring(start, finish - start + 1);
		}

		//************************************************************

		// Znajduje najblizszy znak z grupy (nie pomija indeksu przesylanego jako start_pos)
		static public int findFirstOf(string s, string which_chars, int start_pos = 0)
		{
			if (start_pos >= s.Length)
				return NPOS;

			for (int i = start_pos; i != s.Length; ++i)
				if (belongsTo(s[i], which_chars))
					return i;

			return NPOS;
		}

		static public int findFirstOf(string s, e_CharGroup which_chars, int start_pos = 0)
		{
			if (start_pos >= s.Length)
				return NPOS;

			for (int i = start_pos; i != s.Length; ++i)
				if (belongsTo(s[i], which_chars))
					return i;

			return NPOS;
		}

		//************************************************************

		// Znajduje najblizszy znak, ktory nie nalezy do podanej grupy
		static public int findFirstNotOf(string s, string which_chars, int start_pos = 0)
		{
			if (start_pos >= s.Length)
				return NPOS;

			for (int i = start_pos; i != s.Length; ++i)
				if (!belongsTo(s[i], which_chars))
					return i;

			return NPOS;
		}

		static public int findFirstNotOf(string s, e_CharGroup which_chars, int start_pos = 0)
		{
			if (start_pos >= s.Length)
				return NPOS;

			for (int i = start_pos; i != s.Length; ++i)
				if (!belongsTo(s[i], which_chars))
					return i;

			return NPOS;
		}

		//************************************************************

		// Zwraca ilosc wystapien danego znaku
		static public int getAmountOf(char c, string s, int from = 0, int to = soal.NPOS)
		{
			to = (to > s.Length) ? s.Length : to;

			if (from >= s.Length || from < 0 || to < 0)
				throw new SoalException("%%soal_out_of_range");
			if (from >= to)
				throw new SoalException("%%soal_from_greater_than_to");

			if (to > s.Length)
				to = s.Length;

			int result = 0;

			for (int i = from; i != to; ++i)
				if (s[i] == c)
					++result;

			return result;
		}

		//************************************************************
	}
}
