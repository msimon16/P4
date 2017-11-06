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
using System.Windows.Navigation;
using System.Windows.Shapes;
using P3_Projekt_WPF.Classes;
using P3_Projekt_WPF.Classes.Utilities;

namespace P3_Projekt_WPF
{
    /// <summary>
    /// Interaction logic for ProductControl.xaml
    /// </summary>
    public partial class ProductControl : UserControl
    {
        public Image txtboxImage
        {
            get { return _displayProduct.Image; }
            set
            {
                if(value != null)
                {
                    _displayProduct.Image = value;
                }
                else
                {
                    img_ProductImage.Source = Utils.ImageSourceForBitmap(Properties.Resources.questionmark_png);
                    img_ProductImage.VerticalAlignment = VerticalAlignment.Center;
                    img_ProductImage.HorizontalAlignment = HorizontalAlignment.Center;
                }

            }
        }


        private Product _displayProduct;
        public ProductControl(Product productForDisplay)
        {
            InitializeComponent();
            _displayProduct = productForDisplay;
            txtboxImage = productForDisplay.Image;
            ShowProductInfo();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public void ShowProductInfo()
        {
            img_ProductImage = _displayProduct.Image;
            txtbox_ID.Text = _displayProduct.ID.ToString();
            txtbox_Navn.Text = _displayProduct.Name;
            txtbox_Gruppe.Text = _displayProduct.ProductGroup.Name;
            txtbox_Price.Text = _displayProduct.SalePrice.ToString();

        }
    }
}
