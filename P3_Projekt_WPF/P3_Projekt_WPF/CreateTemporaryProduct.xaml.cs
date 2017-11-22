﻿using System;
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
using System.Windows.Shapes;
using P3_Projekt_WPF.Classes.Database;
using P3_Projekt_WPF.Classes.Utilities;
using P3_Projekt_WPF.Classes.Exceptions;
using P3_Projekt_WPF.Classes;

namespace P3_Projekt_WPF
{
    /// <summary>
    /// Interaction logic for CreateTemporaryProduct.xaml
    /// </summary>
    public partial class CreateTemporaryProduct : Window
    {
        int amount = 1;
        public CreateTemporaryProduct()
        {
            InitializeComponent();
        }

        private void btn_PlusToReciept_Click(object sender, RoutedEventArgs e)
        {
            ++amount;
            UpdateBox();
        }

        private void btn_MinusToReciept_Click(object sender, RoutedEventArgs e)
        {
            --amount;
            UpdateBox();
        }

        private void UpdateBox()
        {
            textBox_ProductAmount.Text = amount.ToString();
        }

        private void TextInputNoNumber(object sender, TextCompositionEventArgs e)
        {
            // Only allows number in textfield
            if (e.Text.Length > 0)
            {
                if (!char.IsDigit(e.Text, e.Text.Length - 1))
                    e.Handled = true;
            }

        }

        private void textBox_ProductAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
