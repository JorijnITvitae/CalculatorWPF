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

namespace CalculatorWPF
{
    public partial class MainWindow : Window
    {
        private Calculator calculator;

        public MainWindow()
        {
            InitializeComponent();
            calculator = new Calculator();
            TextBox_Answer.Text = "0";
            TextBox_Memory.Text = "0";
        }

        private void Button_Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Input.Text.Length > 0)
                TextBox_Input.Text = TextBox_Input.Text.Remove(TextBox_Input.Text.Length - 1);
        }

        private void UpdateAnswer()
        {
            TextBox_Answer.Text = calculator.Answer.ToString();
        }

        private void UpdateMemory()
        {
            TextBox_Memory.Text = calculator.Memory.ToString();
        }

        private void Solve()
        {
            int caret = TextBox_Input.CaretIndex;
            string supportedChars = ".,()am1234567890+-*/^v";
            string cleanedInput = "";
            foreach (char c in TextBox_Input.Text)
            {
                if (supportedChars.Contains(c))
                {
                    cleanedInput += c;
                }
            }
            caret -= TextBox_Input.Text.Length - cleanedInput.Length;
            TextBox_Input.Text = cleanedInput;
            TextBox_Input.CaretIndex = caret;
            TextBox_Output.Text = calculator.Solve(cleanedInput);
        }

        private void Button_Equals_Click(object sender, RoutedEventArgs e)
        {
            UpdateAnswer();
        }

        private void TextBox_Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateAnswer();
                Solve(); // Solve the equasion again just in case it has answer in it.
            }
        }

        private void Button_Item_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)(sender);
            var content = button.Content;
            TextBox_Input.Text += content.ToString();
            Solve();
        }

        private void TextBox_Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            Solve();
        }

        private void ClearTextBoxes()
        {
            TextBox_Input.Clear();
            TextBox_Output.Clear();
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBoxes();
            calculator.ClearResult();
        }

        private void Button_AllClear_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBoxes();
            calculator.ClearResult();
            calculator.ClearAnswer();
            UpdateAnswer();
        }

        private void Button_MemoryClear_Click(object sender, RoutedEventArgs e)
        {
            calculator.ClearMemory();
            UpdateMemory();
        }

        private void Button_MemoryPlus_Click(object sender, RoutedEventArgs e)
        {
            calculator.MemoryAdd();
            UpdateMemory();
        }

        private void Button_MemoryMinus_Click(object sender, RoutedEventArgs e)
        {
            calculator.MemorySubtract();
            UpdateMemory();
        }
    }
}
