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
using P3_Projekt_WPF.Classes.Utilities;

namespace P3_Projekt_WPF
{
    /// <summary>
    /// Interaction logic for AdminValidation.xaml
    /// </summary>
    public partial class AdminValidation : Window
    {
        public AdminValidation()
        {
            InitializeComponent();
            this.passwordBox_InputPassword.PasswordChanged += new RoutedEventHandler(GreyTextRemover);

        }

        private void btn_Validate_Click(object sender, RoutedEventArgs e)
        {
            if (Admin.VerifyPassword(passwordBox_InputPassword.Password))
            {
                this.Close();
            }
            else
                label_InputNotValid.Visibility = Visibility.Visible;
        }

        private void GreyTextRemover(object sender, RoutedEventArgs e)
        {
            if (passwordBox_InputPassword.Password.Length == 0)
            {
                GreyText.Visibility = Visibility.Visible;
            }
            else
                GreyText.Visibility = Visibility.Collapsed;
        }
    }
}
