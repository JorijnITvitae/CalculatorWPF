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
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Input.Clear();
            TextBox_Output.Clear();
        }

        private void UpdateAnswer()
        {
            TextBox_Answer.Text = calculator.Answer.ToString();
        }

        private void Solve()
        {
            TextBox_Output.Text = calculator.Solve(TextBox_Input.Text);
        }

        private void Button_Equals_Click(object sender, RoutedEventArgs e)
        {
            UpdateAnswer();
        }

        private void TextBox_Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) Solve();
            else UpdateAnswer();
        }

        private void Button_Item_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)(sender);
            var content = button.Content;
            TextBox_Input.Text += content.ToString();
            Solve();
        }
    }
}
