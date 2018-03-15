using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace gui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		readonly string messages_source = @"str\strings.txt";
		Dictionary<string, string> messages = new Dictionary<string, string>();

		readonly string func_descr_source = @"str\functions_description.txt";
		bool func_descr_loaded = false;

		readonly string oper_descr_source = @"str\operators_description.txt";
		bool oper_descr_loaded = false;

		mtse.Log program_log = new mtse.Log();
		mtse.Memory program_memory = new mtse.Memory();

		bool clear_console_every_time = false;

		//************************************************************

		public MainWindow()
		{
			InitializeComponent();
			initializeMessages();
			updateZmienne(true);
			updateFunkcje();
			updateOperators();

			clearConsoleEveryTime.IsChecked = clear_console_every_time;
			textWrapEditor.IsChecked = input_textBox.TextWrapping == TextWrapping.Wrap;
			textWrapOutput.IsChecked = output_textBlock.TextWrapping == TextWrapping.Wrap;

			input_textBox.Focus();
		}

		//************************************************************

		private void initializeMessages()
		{
			string[] content = File.ReadAllLines(messages_source);
			foreach (var line in content)
			{
				//output_textBlock.Text += String.Format("[{0}][{1}]\n", s.Length, s);
				int p = line.IndexOf(' ');
				if (p == -1)
					continue;
				string s_key = line.Substring(0, p),
					   s_value = line.Substring(p + 1);
				this.messages[s_key] = s_value;
			}
		}

		//************************************************************

		private void calculate_Button_Click(object sender, RoutedEventArgs e)
		{
			if (clear_console_every_time)
				output_textBlock.Text = String.Empty;

			List<string> lines_of_code = new List<string>();
			string content = input_textBox.Text;
			int pos = 0;
			
			while(pos < content.Length)
			{
				var p1 = content.IndexOf("\r\n", pos);
				if (p1 < 0)
				{
					lines_of_code.Add(content.Substring(pos));
					break;
				}
				else
				{
					lines_of_code.Add(content.Substring(pos, p1 - pos));
					pos = p1 + 2;
				}
			}

			/*int n = input_textBox.LineCount;
			for (int i = 0; i < n; ++i)
				lines_of_code.Add(input_textBox.GetLineText(i));*/

			foreach (var line in lines_of_code)
			{
				try
				{
					mtse.Interpreter inter = new mtse.Interpreter(line, ref program_memory, ref program_log);
					inter.analyze();
					printText(program_log.read());

					var pr = inter.PrintableResult;
					if (!String.IsNullOrEmpty(pr))
					{
						printText(inter.PrintableResult);
						printText("\n");
					}
					program_log.clear();
				}
				catch (mtse.mtseException exc)
				{
					var ss = program_log.read();
					if (ss.Count() != 0)
					{
						printText(ss);
						printText("\n");
					}
					printText(formatErrorMessage(exc.Message, exc.Args));
					printText("\n");
				}
				catch (Exception)
				{
					var ss = program_log.read();
					if (ss.Count() != 0)
					{
						printText(ss);
						printText("\n");
					}
					printText("%%problem_encountered");
					printText("\n");
				}
			}

			updateZmienne();
		}

		//************************************************************

		private void printText(string what)
		{
			var entry = messages.FirstOrDefault(e => e.Key == what);
			if(entry.Key == null)
				output_textBlock.Text += what;
			else
				output_textBlock.Text += messages[what];
		}

		private void printText(List<string> what)
		{
			foreach (var i in what)
				printText(i);
		}

		//************************************************************

		private string formatErrorMessage(string err, object[] args)
		{
			var entry = messages.FirstOrDefault(e => e.Key == err);
			if (entry.Key != null)
				err = messages[err];

			switch (args.Count())
			{
				case 0: break;
				case 1: err = String.Format(err, args[0]);  break;
				case 2: err = String.Format(err, args[0], args[1]); break;
				case 3: err = String.Format(err, args[0], args[1], args[2]); break;
				case 4: err = String.Format(err, args[0], args[1], args[2], args[3]); break;
				default: break;
			}

			return err;
		}

		//************************************************************

		void updateZmienne(bool force_update = false)
		{
			if (force_update || program_memory.hasBeenAltered())
			{
				zmienne_listBox.Items.Clear();

				var const_dict = program_memory.Constants;
				var local_dict = program_memory.LocalVariables;
				string label_text = "";

				foreach (var i in const_dict)
				{
					ListBoxItem lbi = new ListBoxItem();
					lbi.Foreground = Brushes.DarkRed;
					label_text = i.Key + ": " + i.Value.printable();
					lbi.Content = label_text;
					zmienne_listBox.Items.Add(lbi);
				}

				foreach (var i in local_dict)
				{
					ListBoxItem lbi = new ListBoxItem();
					label_text = i.Key + ": " + i.Value.printable();
					lbi.Content = label_text;
					zmienne_listBox.Items.Add(lbi);
				}
			}
		}

		//************************************************************

		void updateFunkcje()
		{
			if (func_descr_loaded)
				return;
			else
				func_descr_loaded = true;

			string[] lines = File.ReadAllLines(func_descr_source);
			foreach(var line in lines)
			{
				int p = line.IndexOf(' ');
				if (p == -1)
					continue;
				string func_name = line.Substring(0, p),
				       description = line.Substring(p + 1);

				StackPanel sp = new StackPanel();
				Label a = new Label(), b = new Label();
				a.Content = func_name;
				a.FontStyle = FontStyles.Italic;
				b.Content = description;
				sp.Orientation = Orientation.Horizontal;
				sp.Children.Add(a);
				sp.Children.Add(b);
				funkcje_listBox.Items.Add(sp);
			}
		}

		//************************************************************

		void updateOperators()
		{
			if (oper_descr_loaded)
				return;
			else
				oper_descr_loaded = true;

			string[] lines = File.ReadAllLines(oper_descr_source);
			foreach(var line in lines)
			{
				int p = line.IndexOf(' ');
				if (p == -1)
					continue;
				string func_name = line.Substring(0, p),
					   description = line.Substring(p + 1);

				StackPanel sp = new StackPanel();
				Label a = new Label(), b = new Label();
				a.Content = func_name;
				a.FontStyle = FontStyles.Italic;
				b.Content = description;
				sp.Orientation = Orientation.Horizontal;
				sp.Children.Add(a);
				sp.Children.Add(b);
				operatory_listBox.Items.Add(sp);
			}
		}

		//************************************************************

		private void addOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " + ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void substractOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " - ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void powerOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " ^ ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void multiplyOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " * ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void divideOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " / ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void assignOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " = ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void lessOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " < ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void equalOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " == ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		private void greaterOp_Button_Click(object sender, RoutedEventArgs e)
		{
			int n = input_textBox.CaretIndex;
			input_textBox.Text = input_textBox.Text.Insert(n, " > ");
			input_textBox.Focus();
			input_textBox.CaretIndex = n + 3;
		}

		//************************************************************

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			OProgramie oprogramie = new OProgramie();
			oprogramie.Show();
		}

		private void zakoncz_menuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void zapisz_menuItem_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog file_dialog = new SaveFileDialog();
			file_dialog.Filter = "Kod MTSE (*.se)|*.se|Wszystkie pliki (*.*)|*.*";
			if (file_dialog.ShowDialog() == true)
				File.WriteAllText(file_dialog.FileName, input_textBox.Text);
		}

		private void otworz_menuItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog file_dialog = new OpenFileDialog();
			file_dialog.Filter = "Kod MTSE (*.se)|*.se|Wszystkie pliki (*.*)|*.*";
			if (file_dialog.ShowDialog() == true)
				input_textBox.Text = File.ReadAllText(file_dialog.FileName);
		}

		//************************************************************

		private void chooseFontEditor_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FontDialog font_dialog = new System.Windows.Forms.FontDialog();
			font_dialog.ShowColor = true;

			if (font_dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
			{
				System.Drawing.Font font = font_dialog.Font;
				input_textBox.FontFamily = new FontFamily(font.Name);
				input_textBox.FontSize = font.Size;
				input_textBox.FontWeight = font.Bold ? FontWeights.Bold : FontWeights.Regular;
				input_textBox.FontStyle = font.Italic ? FontStyles.Italic : FontStyles.Normal;
			}
		}

		private void chooseFontOutput_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FontDialog font_dialog = new System.Windows.Forms.FontDialog();
			font_dialog.ShowColor = true;

			if (font_dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
			{
				System.Drawing.Font font = font_dialog.Font;
				output_textBlock.FontFamily = new FontFamily(font.Name);
				output_textBlock.FontSize = font.Size;
				output_textBlock.FontWeight = font.Bold ? FontWeights.Bold : FontWeights.Regular;
				output_textBlock.FontStyle = font.Italic ? FontStyles.Italic : FontStyles.Normal;
			}
		}

		private void clearConsoleEveryTime_Click(object sender, RoutedEventArgs e)
		{
			clear_console_every_time = clearConsoleEveryTime.IsChecked;
		}

		private void textWrapEditor_Click(object sender, RoutedEventArgs e)
		{
			if (textWrapEditor.IsChecked)
				input_textBox.TextWrapping = TextWrapping.Wrap;
			else
				input_textBox.TextWrapping = TextWrapping.NoWrap;
		}

		private void textWrapOutput_Click(object sender, RoutedEventArgs e)
		{
			if (textWrapOutput.IsChecked)
				output_textBlock.TextWrapping = TextWrapping.Wrap;
			else
				output_textBlock.TextWrapping = TextWrapping.NoWrap;
		}

		//************************************************************

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch(e.Key)
			{
				case Key.F5:
					calculate_Button_Click(sender, e);
					break;
			}
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			HelpWin hw = new HelpWin();
			hw.Show();
		}

		//************************************************************
	}
}
