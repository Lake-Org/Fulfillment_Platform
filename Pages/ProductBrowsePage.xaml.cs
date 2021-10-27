﻿using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductBrowsePage : Page {
        
        private readonly ProductModule ProdM;
        private readonly List<Product> allProducts;
        private List<Product> filteredProducts;

        private int queryLenght = 0;
        private bool _clearFilters;
        public bool clearFilters {
            get { return _clearFilters; }
            set {
                _clearFilters = value;
                if (value == true) {
                    deleteFilters();
                }
            }
        }
        
        //shit that makes this a singelton
        public static ProductBrowsePage Instance { get; private set; }
        static ProductBrowsePage() {
            Instance = new ProductBrowsePage();
        }


        private ProductBrowsePage() {
            InitializeComponent();
            //getting products
            ProdM = new();
            allProducts = ProdM.GetProducts();
            filteredProducts = allProducts;

            //init DataGrid
            productDG.ItemsSource = filteredProducts;
            //init label
            ChangeCountLabel(filteredProducts.Count);
        }

        private void deleteFilters() {
            TypeFilterSBox.Clear();
            TitleFilterSBox.Clear();
            SKUFilterSBox.Clear();
            VendorFilterSBox.Clear();

            queryLenght = 0;
            filteredProducts = allProducts;
        }

        private void ChangeCountLabel(int count) {
            productCountL.Content = "Product Count: " + count.ToString();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductSyncPage.Instance;
        }

        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Product product = row.Item as Product;
            MainWindow.Instance.mainFrame.Content = new ProductEditPage(product);
        }

        //method for filtering by vendor
        private void VendorFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght) {
                clearFilters = true;
            } else {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2) {

                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }
        //method for filtering by sku
        private void SKUFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;

            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght) {
                clearFilters = true;
            } else {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2) {
                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }
        //method for filtering by title
        private void TitleFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;

            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght) {
                clearFilters = true;
            } else {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2) {
                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }
        //method for filtering by type
        private void TypeFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            
            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght) {
                clearFilters = true;
            } else {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2) {
                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.product_type.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.product_type.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }
    }
}
