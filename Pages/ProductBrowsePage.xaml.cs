﻿using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.UI;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductBrowsePage : Page {
        
        private ReadOnlyCollection<Product> AllProducts;
        private List<Product> StatusFilteredProducts;
        private List<Product> DateFilteredFilteredProducts;
        private List<Product> TextFilteredProducts;

        public List<CheckBoxListItem> StatusList;



        private int queryLenght = 0;
        private bool _clearFilters;
        public bool clearFilters {
            get { return _clearFilters; }
            set {
                _clearFilters = value;
                if (value == true) {
                    DeleteTextFilters();
                }
            }
        }
        
        //shit that makes this a singelton
        public static ProductBrowsePage Instance { get; private set; }
        static ProductBrowsePage() {
            Instance = new ProductBrowsePage();
        }

        //private constructor
        private ProductBrowsePage() {
            InitializeComponent();
            InitStatusListBox();
            InitDatePickers();
            LoadAllProducts();
        }


        //
        // data grid manitulation section 
        //

        //method for  loading products to datagrid
        private void LoadAllProducts() {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += BGW_PreloadAllProducts;
            worker.RunWorkerCompleted += BGW_PreloadAllProductsCompleted;

            //blocking refresh button and animating loading bar
            loadingBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;

            worker.RunWorkerAsync();
        }

        // backgroud worker for loading all products
        private void BGW_PreloadAllProducts(object sender, DoWorkEventArgs e) {
            List<Product> products = ProductModule.GetAllProducts();
            e.Result = products;
        }

        // background Worker for loading all product on complete
        private void BGW_PreloadAllProductsCompleted(object sender, RunWorkerCompletedEventArgs e) {
            AllProducts = (e.Result as List<Product>).AsReadOnly();
            StatusFilteredProducts = AllProducts.ToList();
            DateFilteredFilteredProducts = AllProducts.ToList();
            TextFilteredProducts = AllProducts.ToList();

            //init DataGrid
            productDG.ItemsSource = TextFilteredProducts;
            //init label
            ChangeCountLabel(TextFilteredProducts.Count);
            //unblocking refresh button and unanimating loading bar
            loadingBar.IsIndeterminate = false;
            RefreshButton.IsEnabled = true;
            Debug.WriteLine("BGW_PreloadAllProducts Finished");
        }

        //changes label to reflect product count in datagrid
        private void ChangeCountLabel(int count) {
            productCountL.Content = "Product Count: " + count.ToString();
        }

        //refreshes Product datagrid
        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            DeleteTextFilters();
            LoadAllProducts();
        }

        //removing and reseting all filters
        private void ResetAllFilters() { 
            
        }


        //
        // change pages section
        //

        //goes back to main page
        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        //opens Product Edit page
        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Product product = row.Item as Product;
            MainWindow.Instance.mainFrame.Content = new ProductEditPage(product);
        }


        //
        //status filtering section
        //

        //init status list box
        private void InitStatusListBox() {
            //getting all possible statuses and ading them to observable collection
            StatusList = new();
            foreach (var status in ProductStatus.GetFields()) {

                CheckBoxListItem newItem = new(status);
                StatusList.Add(newItem);
            }

            //binding each checkbox to observable collection 
            CheckBox1.DataContext = StatusList[0];
            CheckBox2.DataContext = StatusList[1];
            CheckBox3.DataContext = StatusList[2];
            CheckBox4.DataContext = StatusList[3];
            CheckBox5.DataContext = StatusList[4];
            CheckBox6.DataContext = StatusList[5];
        }

        //on status change filtering 
        private void FilterByStatus() {
            StatusFilteredProducts.Clear();

            foreach (CheckBoxListItem status in StatusList) {
                if (status.IsSelected) {
                    StatusFilteredProducts.AddRange(AllProducts.ToList().FindAll(x => x.status == status.Name));
                }
            }

            DateFilteredFilteredProducts = StatusFilteredProducts;
            TextFilteredProducts = DateFilteredFilteredProducts;
            productDG.ItemsSource = TextFilteredProducts;
        }

        // this method fires when checkbox was clicked ie selection changed
        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            FilterByStatus();
        }


        //
        //Date filtering section
        //

        //init status list box
        private void InitDatePickers() {
            //setting begin and end date pickers
            BeginDatePicker.DisplayDateStart = new DateTime(2021, 09, 01);
            EndDatePicker.DisplayDateStart = new DateTime(2021, 09, 01);

        }

        //on selected date change
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {

        }


        //
        // Text Filtering section
        //

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

                if (productDG.ItemsSource == TextFilteredProducts) {
                    TextFilteredProducts = TextFilteredProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                } else {
                    TextFilteredProducts = AllProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(AllProducts.Count);
                productDG.ItemsSource = AllProducts;
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

                if (productDG.ItemsSource == TextFilteredProducts) {
                    TextFilteredProducts = TextFilteredProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                } else {
                    TextFilteredProducts = AllProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(AllProducts.Count);
                productDG.ItemsSource = AllProducts;
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

                if (productDG.ItemsSource == TextFilteredProducts) {
                    TextFilteredProducts = TextFilteredProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                } else {
                    TextFilteredProducts = AllProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(AllProducts.Count);
                productDG.ItemsSource = AllProducts;
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

                if (productDG.ItemsSource == TextFilteredProducts) {
                    TextFilteredProducts = TextFilteredProducts.Where(p => p.product_type.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                } else {
                    TextFilteredProducts = AllProducts.Where(p => p.product_type.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(TextFilteredProducts.Count);
                    productDG.ItemsSource = TextFilteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(AllProducts.Count);
                productDG.ItemsSource = AllProducts;
            }
        }

        //method for deleting deleting filters
        private void DeleteTextFilters() {
            TypeFilterSBox.Clear();
            TitleFilterSBox.Clear();
            SKUFilterSBox.Clear();
            VendorFilterSBox.Clear();

            queryLenght = 0;
            TextFilteredProducts = AllProducts.ToList();
        }

        //method that is triggered when clicking remove filters button
        private void RemoveFilters_Click(object sender, RoutedEventArgs e) {
            DeleteTextFilters();
        }
    }
}
