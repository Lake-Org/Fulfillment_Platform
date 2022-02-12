﻿using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.Supplier;
using Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas;
using Ikrito_Fulfillment_Platform.Modules.Supplier.TDBaltic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//todo: when double ckicking any of these list boxes you edit product and then delete if from list box

namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class ProductUpdatePage : Page
    {

        private readonly ProductSyncModule Sync;
        private List<ProductState> SyncProducts;
        private List<ProductState> FilteredSyncProducts;

        private int queryLenght = 0;
        private bool _clearFilters;
        public bool clearFilters
        {
            get { return _clearFilters; }
            set
            {
                _clearFilters = value;
                if (value == true)
                {
                    deleteFilters();
                }
            }
        }

        //shit makes it a singleton
        public static ProductUpdatePage Instance { get; private set; }
        static ProductUpdatePage()
        {
            Instance = new ProductUpdatePage();
        }

        private ProductUpdatePage()
        {
            InitializeComponent();
            //getting SyncProducts
            LoadSyncProducts();
            Sync = new();
        }


        //
        // General method section
        //

        //method that changes datagrid count label text value
        private void ChangeCountLabel(int count)
        {
            SyncProductCountL.Content = "Sync Product Count: " + count.ToString();
        }

        //method removes SKU filter from the data grid
        private void deleteFilters()
        {
            SKUFilterSBox.Clear();
            queryLenght = 0;
            FilteredSyncProducts = SyncProducts;
        }

        //method for fitering by sku
        private void SKUFilterSBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght)
            {
                clearFilters = true;
            }
            else
            {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2)
            {
                string query = textBox.Text.ToLower();

                if (productSyncDG.ItemsSource == FilteredSyncProducts)
                {
                    FilteredSyncProducts = FilteredSyncProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(FilteredSyncProducts.Count);
                    productSyncDG.ItemsSource = FilteredSyncProducts;
                }
                else
                {
                    FilteredSyncProducts = SyncProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(FilteredSyncProducts.Count);
                    productSyncDG.ItemsSource = FilteredSyncProducts;
                }
            }
            else if (textBox.Text.Length == 0)
            {
                ChangeCountLabel(SyncProducts.Count);
                productSyncDG.ItemsSource = SyncProducts;
            }
        }

        //button that goes back to main screen
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        //button that refreshes the data grid
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            deleteFilters();
            LoadSyncProducts();
        }


        //
        // section for loading Sync Products to datagrid
        //

        //method that creates BGW to load Sync products
        private void LoadSyncProducts()
        {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = false;
            worker.DoWork += BGW_PreloadSyncProducts;
            worker.RunWorkerCompleted += BGW_PreloadSyncProductsCompleted;

            //blocking refresh button and animating loading bar
            progressBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;

            progressBarLabel.Text = "Loading Sync Products from DataBase";

            worker.RunWorkerAsync();
        }

        //BGW load sync products
        private void BGW_PreloadSyncProducts(object sender, DoWorkEventArgs e)
        {
            List<ProductState> products = ProductSyncModule.GetSyncProducts();
            e.Result = products;
        }

        //BGW load sync products onComplete
        private void BGW_PreloadSyncProductsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //changing loading bar state
            progressBar.IsIndeterminate = false;
            progressBarLabel.Text = "";


            SyncProducts = (List<ProductState>)e.Result;
            Sync.syncProducts = (List<ProductState>)e.Result;
            FilteredSyncProducts = SyncProducts;

            //init DataGrid
            productSyncDG.ItemsSource = FilteredSyncProducts;
            //init label
            ChangeCountLabel(FilteredSyncProducts.Count);
            //unblocking refresh button and unanimating loading bar
            progressBar.IsIndeterminate = false;
            RefreshButton.IsEnabled = true;
            Debug.WriteLine("BGW_PreloadAllProducts Finished");
        }

        //
        // set stock 0 to archival
        //

        private void Stock0ArchiveBtn_Click(object sender, RoutedEventArgs e)
        {
            SetStock0Archival();
        }

        //method that creates BGW to load Sync products
        private void SetStock0Archival()
        {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Sync.MarkOutOfStockNeedsArchival;
            worker.ProgressChanged += BGW_SetStock_ProgressChanged;
            worker.RunWorkerCompleted += BGW_SyncProductsCompleted;

            RefreshButton.IsEnabled = false;

            progressBarLabel.Text = "Setting Stock 0 Products to Archival";

            worker.RunWorkerAsync();
        }

        //method that updates progress bar during product export
        private void BGW_SetStock_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            progressBar.Value = progress;
            progressBarLabel.Text = $"Setting Stock 0 Products to Archival: {progress}‰";
        }

        //BGW load sync products onComplete
        private void BGW_SetStock_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            //changing loading bar state
            progressBar.Value = 0;
            progressBarLabel.Text = "";

            LoadSyncProducts();
        }

        //
        // shopify sync section 
        //

        //button that starts shopify sync
        private void SyncProducts_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Sync.ExportShopifyProducts;
            worker.ProgressChanged += workerSync_ProgressChanged;
            worker.RunWorkerCompleted += BGW_SyncProductsCompleted;

            RefreshButton.IsEnabled = false;

            progressBarLabel.Text = "Syncing Products To Shopify: 0‰";

            worker.RunWorkerAsync();
        }

        //method that updates progress bar during product export
        private void workerSync_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            progressBar.Value = progress;
            progressBarLabel.Text = $"Syncing Products To Shopify: {progress}‰";

        }

        //BGW load sync products onComplete
        private void BGW_SyncProductsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //changing loading bar state
            progressBar.Value = 0;
            progressBar.IsIndeterminate = false;
            progressBarLabel.Text = "";

            //init DataGrid
            productSyncDG.ItemsSource = FilteredSyncProducts;
            //init label
            ChangeCountLabel(FilteredSyncProducts.Count);
            //unblocking refresh button and unanimating loading bar
            progressBar.IsIndeterminate = false;
            RefreshButton.IsEnabled = true;
            Debug.WriteLine("BGW_SyncProducts Finished");
            LoadSyncProducts();
        }

        //
        // Changes ListBoxes section
        //

        //method taht clears changes window
        private void ClearChangesListBoxes()
        {
            NewProductListBox.ItemsSource = null;
            UpdatedProductListBox.ItemsSource = null;
            ArchivedProductListBox.ItemsSource = null;

            NewProductsLabel.Content = $"New Products";
            UpdatedProductsLabel.Content = $"Updated Products";
            ArchivedProductsLabel.Content = $"Archived Products";
        }

        //method that populates chnaged products listboxes
        private void PopulateChangeListBoxes(object Changes)
        {
            Dictionary<string, object> ChangesKVP = Changes as Dictionary<string, object>;

            //converting chnages to list to lists of productchanges
            List<ProductChangeRecord> newProducts = ChangesKVP["NewProducts"] as List<ProductChangeRecord>;                     //what new product were added
            List<ProductChangeRecord> archivedProducts = ChangesKVP["ArchivedProducts"] as List<ProductChangeRecord>;           //what products werent added because they were missing datasheet
            List<ProductChangeRecord> updatedProducts = ChangesKVP["UpdatedProducts"] as List<ProductChangeRecord>;             //what products were changed

            NewProductListBox.ItemsSource = newProducts;
            UpdatedProductListBox.ItemsSource = updatedProducts;
            ArchivedProductListBox.ItemsSource = archivedProducts;

            NewProductsLabel.Content = $"New Products ({newProducts.Count})";
            UpdatedProductsLabel.Content = $"Updated Products ({updatedProducts.Count})";
            ArchivedProductsLabel.Content = $"Archived Products ({archivedProducts.Count})";
        }

        //method that allows user to edit list box product by opening it in ProductEditPage
        private void ChangeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listboxItem)
            {
                ProductChangeRecord productChange = listboxItem.Content as ProductChangeRecord;
                FullProduct editProduct = ProductModule.GetProduct(productChange.SKU);
                MainWindow.Instance.mainFrame.Content = new ProductEditPage(editProduct, this);
            }
        }


        //
        // update Vendor Products section
        //

        //button that updates product from TDB
        private void UpdateTDBButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker TDBUpdateWorker = new();
            TDBUpdateWorker.WorkerReportsProgress = true;
            TDBUpdateWorker.DoWork += (sender, e) => UploadSupplierProducts.UpdateProducts("TDB" ,sender, e);
            TDBUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            TDBUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;
            progressBarLabel.Text = "Updating TDB products";

            ClearChangesListBoxes();

            TDBUpdateWorker.RunWorkerAsync();
        }

        //button that updates product from KG
        private void UpdateKGButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker KGUpdateWorker = new();
            KGUpdateWorker.WorkerReportsProgress = true;
            KGUpdateWorker.DoWork += (sender, e) => UploadSupplierProducts.UpdateProducts("KG", sender, e);
            KGUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            KGUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;
            progressBarLabel.Text = "Updating KG products";

            ClearChangesListBoxes();

            KGUpdateWorker.RunWorkerAsync();
        }

        //button that updates product from PD
        private void UpdatePDButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker PDUpdateWorker = new();
            PDUpdateWorker.WorkerReportsProgress = true;
            PDUpdateWorker.DoWork += (sender, e) => UploadSupplierProducts.UpdateProducts("PD", sender, e);
            PDUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            PDUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;
            progressBarLabel.Text = "Updating PD products";

            ClearChangesListBoxes();

            PDUpdateWorker.RunWorkerAsync();
        }

        //button that updates product from BF supplier
        private void UpdateBFButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker PDUpdateWorker = new();
            PDUpdateWorker.WorkerReportsProgress = true;
            PDUpdateWorker.DoWork += (sender, e) => UploadSupplierProducts.UpdateProducts("BF", sender, e);
            PDUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            PDUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;
            progressBarLabel.Text = "Updating BF products";

            ClearChangesListBoxes();

            PDUpdateWorker.RunWorkerAsync();
        }

        //method that updates progress bar during product export
        private void UpdateVendorProductsWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            progressBar.Value = progress;
            progressBarLabel.Text = e.UserState as string;
        }

        //worker on complete method to updating vendor products (disables loading bar, populates listboxes)
        private void UpdateVendorProductsWorkerOnComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBarLabel.Text = "";
            RefreshButton.IsEnabled = true;
            progressBar.Value = 0;
            PopulateChangeListBoxes(e.Result);
        }
    }
}