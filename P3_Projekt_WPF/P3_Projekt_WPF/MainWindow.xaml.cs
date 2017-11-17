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
using P3_Projekt_WPF.Classes.Database;
using P3_Projekt_WPF.Classes.Utilities;
using P3_Projekt_WPF.Classes.Exceptions;
using P3_Projekt_WPF.Classes;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Collections;
using System.IO;
using System.Collections.Concurrent;
//using System.Drawing;

namespace P3_Projekt_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private SettingsController _settingsController;
        private StorageController _storageController;
        private POSController _POSController;
        private StatisticsController _statisticsController;
        private Grid productGrid = new Grid();

        private Dictionary<int, ProductControl> _productControlDictionary = new Dictionary<int, ProductControl>();
        private bool _ctrlDown = false;
        public static bool runLoading = true;
        public MainWindow()
        {
            _storageController = new StorageController();
            _POSController = new POSController(_storageController);
            _settingsController = new SettingsController();
            _statisticsController = new StatisticsController();
            InitializeComponent();
            Mysql.Connect();
            LoadDatabase();
            InitComponents();

            this.KeyDown += new KeyEventHandler(KeyboardHook);
            this.KeyDown += new KeyEventHandler(CtrlHookDown);
            this.KeyDown += new KeyEventHandler(EnterKeyPressedSearch);
            this.KeyUp += new KeyEventHandler(CtrlHookUp);
            this.WindowState = WindowState.Maximized;
        }

        public void ReloadProducts()
        {
            LoadProductImages();
            LoadProductControlDictionary();
            LoadProductGrid(_storageController.ProductDictionary);
        }

        private void showloadform()
        {
            LoadingScreen load = new LoadingScreen();
            load.ShowDialog();
        }

        private void KeyboardHook(object sender, KeyEventArgs e)
        {
            if (_ctrlDown && (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl))
            {
                ClickQuickButton(e.Key);
            }
        }

        private void ClickQuickButton(Key btn)
        {
            Debug.Print("Pressed quickbutton combo:" + btn.ToString());
            if (_settingsController.quickButtonKeyList.ContainsKey(btn))
            {
                _settingsController.quickButtonKeyList[btn].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void CtrlHookDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _ctrlDown = true;
            }
        }

        private void CtrlHookUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _ctrlDown = false;
            }
        }

        private void InitComponents()
        {
            InitGridQuickButtons();
            InitStorageGridProducts();
            AddProductButton();
            Stopwatch Timer1 = new Stopwatch();
            Timer1.Start();
            LoadProductImages();
            Timer1.Stop();
            Debug.WriteLine("[LoadProductImages] took "+Timer1.ElapsedMilliseconds+"ms");
            LoadProductGrid(_storageController.ProductDictionary);
            BuildInformationTable();
            InitStatisticsTab();
        }

        private void InitGridQuickButtons()
        {
            grid_QuickButton.ColumnDefinitions.Add(new ColumnDefinition());
            grid_QuickButton.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < 7; ++i)
            {
                grid_QuickButton.RowDefinitions.Add(new RowDefinition());
            }
        }

        private void updateGridQuickButtons(object sender, RoutedEventArgs e)
        {
            UpdateGridQuickButtons();
        }

        private void InitStatisticsTab()
        {
            datePicker_StartDate.SelectedDate = DateTime.Now;
            datePicker_EndDate.SelectedDate = DateTime.Now;
            foreach (string brand in _storageController.ProductDictionary.Values.Select(x => x.Brand).Distinct())
            {
                comboBox_Brand.Items.Add(brand);
            }
            foreach (Group group in _storageController.GroupDictionary.Values)
            {
                comboBox_Group.Items.Add(group.Name);
            }

        }

        private void UpdateReceiptList()
        {
            listView_Receipt.Items.Clear();
            foreach (SaleTransaction transaction in _POSController.PlacerholderReceipt.Transactions)
            {
                AddTransactionToReceipt(transaction);
            }
            label_TotalPrice.Content = _POSController.PlacerholderReceipt.TotalPrice;
        }

        public void InitStorageGridProducts()
        {
            productGrid.VerticalAlignment = VerticalAlignment.Stretch;
            productGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            productGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(380) });

            scroll_StorageProduct.Content = productGrid;

        }

        public void LoadDatabase()
        {
            Stopwatch TimeTester = new Stopwatch();
            TimeTester.Start();
            Thread GetAllThread = new Thread(new ThreadStart(_storageController.GetAll));
            GetAllThread.Start();
            while (!_storageController.ThreadsDone)
            {
                Thread.Sleep(1);
            }
            runLoading = false;
            TimeTester.Stop();
            string Output = "";
            while (_storageController.TimerStrings.TryDequeue(out Output))
            {
                Debug.WriteLine(Output);
            }
            Debug.WriteLine("[P3] Det tog " + TimeTester.ElapsedMilliseconds + "ms at hente alt fra databasen");
        }

        Button addProductButton = new Button();
        public void AddProductButton()
        {

            addProductButton.Content = "Tilføj nyt produkt";
            addProductButton.FontSize = 30;

            addProductButton.SetValue(Grid.RowProperty, 0);
            addProductButton.SetValue(Grid.ColumnProperty, 0);
            addProductButton.Style = FindResource("Flat_Button") as Style;
            addProductButton.Margin = new System.Windows.Thickness(2);
            addProductButton.Background = System.Windows.Media.Brushes.Transparent;

            addProductButton.Click += AddProductDialogOpener;


        }

        public void AddProductDialogOpener(object sender, RoutedEventArgs e)
        {
            CreateProduct addProductWindow = new CreateProduct(_storageController, this);

            addProductWindow.btn_SaveAndQuit.Click += delegate
            {
                if (addProductWindow.IsProductInputValid())
                {
                    LoadProductImages();
                }
            };

            addProductWindow.btn_ServiceSaveAndQuit.Click += delegate
            {
                if (addProductWindow.IsServiceProductInputValid())
                {
                    LoadProductImages();
                }
            };

            addProductWindow.Show();
        }

        private bool _firstClick = true;
        private Product _productToEdit = null;
        private void EditProductClick(object sender, RoutedEventArgs e)
        {
            CreateProduct EditProductForm = new CreateProduct(_productToEdit, _storageController, this);
            EditProductForm.Show();
        }

        private void ShowSpecificInfoProductStorage(object sender, RoutedEventArgs e)
        {
            // Hvis det er første gang, skal EditProduct lige hookes op på edit product
            if (_firstClick)
            {
                _firstClick = false;
                btn_EditProduct.Visibility = Visibility.Visible;
            }

            Debug.Print((sender as Button).Tag.ToString());
            _productToEdit = _storageController.ProductDictionary[Convert.ToInt32((sender as Button).Tag)];
            image_ChosenProduct.Source = Utils.ImageSourceForBitmap(Properties.Resources.questionmark_png);

            if (_productToEdit.Image != null)
            {
                image_ChosenProduct.Source = _productToEdit.Image.Source;
            }

            textBlock_ChosenProduct.Text = $"ID: {_productToEdit.ID}\nNavn: {_productToEdit.Name}\nGruppe: {_storageController.GroupDictionary[_productToEdit.ProductGroupID].Name}\nMærke: {_productToEdit.Brand}\nPris: {_productToEdit.SalePrice}\nTilbudspris: {_productToEdit.DiscountPrice}\nIndkøbspris: {_productToEdit.PurchasePrice}\nLagerstatus:";
            foreach (KeyValuePair<int, int> storageWithAmount in _productToEdit.StorageWithAmount)
            {
                textBlock_ChosenProduct.Text += $"\n  - {_storageController.StorageRoomDictionary[storageWithAmount.Key].Name} har {storageWithAmount.Value} stk.";
            }
        }



        public void StorageTabClick(object sender, RoutedEventArgs e)
        {
            //LoadProductGrid();
        }

        public void UpdateStorageTab(object sender, RoutedEventArgs e)
        {
            LoadProductGrid(_storageController.ProductDictionary);
        }

        private void LoadProductControlDictionary()
        {
            Stopwatch Timer2 = new Stopwatch();
            Timer2.Start();
            _productControlDictionary.Clear();
            var ProductList = _storageController.ProductDictionary.OrderBy(x => x.Key);
            foreach (var product in ProductList)
            {
                ProductControl productControl = new ProductControl(product.Value, _storageController.GroupDictionary);
                productControl.btn_ShowMoreInformation.Tag = product.Value.ID;
                productControl.btn_ShowMoreInformation.Click += ShowSpecificInfoProductStorage;

                _productControlDictionary.Add(product.Value.ID, productControl);
            }
            Timer2.Stop();
            Debug.WriteLine("[LoadProductControlDictionary] took "+Timer2.ElapsedMilliseconds+"ms");
        }

        public void LoadProductGrid(ConcurrentDictionary<int, Product> productDictionary)
        {
            productGrid.RowDefinitions.Clear();
            productGrid.ColumnDefinitions.Clear();
            InitStorageGridProducts();
            productGrid.Children.Clear();
            productGrid.Children.Add(addProductButton);

            int i = 1;
            var ProductListSorted = productDictionary.OrderBy(x => x.Key);
            foreach (var product in ProductListSorted)
            {
                if (i % 5 == 0)
                {
                    productGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(380) });
                }

                ProductControl productControl = _productControlDictionary[product.Value.ID];
                productControl.SetValue(Grid.ColumnProperty, i % 5);
                productControl.SetValue(Grid.RowProperty, i / 5);

                productGrid.Children.Add(productControl);
                i++;
            }

        }

        public void LoadProductGrid(ConcurrentDictionary<int, SearchProduct> productDictionary)
        {
            productGrid.RowDefinitions.Clear();
            productGrid.Children.Clear();
            productGrid.Children.Add(addProductButton);

            productGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(380) });
            int i = 1;

            foreach (KeyValuePair<int, SearchProduct> product in productDictionary.OrderByDescending(x => x.Value.BrandMatch + x.Value.GroupMatch + x.Value.NameMatch))
            {
                if (i % 5 == 0)
                {
                    productGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(380) });
                }

                ProductControl productControl = _productControlDictionary[product.Value.CurrentProduct.ID];
                productControl.SetValue(Grid.ColumnProperty, i % 5);
                productControl.SetValue(Grid.RowProperty, i / 5);

                productGrid.Children.Add(productControl);
                i++;
            }

        }

        public void LoadProductImages()
        {
            if (Directory.Exists(Properties.Settings.Default.PictureFilePath))
            {
                DirectoryInfo directory = new DirectoryInfo($@"{ Properties.Settings.Default.PictureFilePath }");
                string[] allowedExtensions = new string[] { ".jpg", ".bmp", ".png", ".jpeg", ".tiff", ".gif" };

                IEnumerable<FileInfo> imageFiles = from file in directory.EnumerateFiles("*", SearchOption.AllDirectories)
                                                   where allowedExtensions.Contains(file.Extension.ToLower())
                                                   select file;

                try
                {
                    foreach (FileInfo productImage in imageFiles)
                    {
                        int productID;
                        int.TryParse((productImage.Name.Replace(productImage.Extension, "")), out productID);

                        BitmapImage bitMap = new BitmapImage(new Uri($"{productImage.DirectoryName}/{productImage}"));

                        Image image = new Image();
                        image.Source = bitMap;

                        _storageController.ProductDictionary[productID].Image = image;
                    }
                }
                catch (UnauthorizedAccessException e)
                {

                }
            }
            /*
            Thread productControlThread = new Thread(LoadProductControlDictionary);
            productControlThread.Name = "Product Control Load Thread";
            productControlThread.SetApartmentState(ApartmentState.STA);
            productControlThread.Start();
            */
            LoadProductControlDictionary();
        }

        public void AddTransactionToReceipt(SaleTransaction transaction)
        {
            if (transaction.Product is TempProduct)
            {
                listView_Receipt.Items.Add(new ReceiptListItem { String_Product = (transaction.Product as TempProduct).Description, Amount = transaction.Amount, Price = $"{transaction.GetProductPrice()}", IDTag = transaction.Product.ID });
            }
            else
            {
                listView_Receipt.Items.Add(new ReceiptListItem { String_Product = transaction.GetProductName(), Amount = transaction.Amount, Price = $"{transaction.GetProductPrice()},-", IDTag = transaction.Product.ID });
            }
        }

        private void btn_MobilePay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Increment_Click(object sender, RoutedEventArgs e)
        {
            int productID = Convert.ToInt32((sender as Button).Tag);
            _POSController.PlacerholderReceipt.Transactions.Where(x => x.Product.ID == productID).First().Amount++;
            _POSController.PlacerholderReceipt.UpdateTotalPrice();
            UpdateReceiptList();
        }

        private void btn_Decrement_Click(object sender, RoutedEventArgs e)
        {
            int productID = Convert.ToInt32((sender as Button).Tag);
            _POSController.PlacerholderReceipt.Transactions.Where(x => x.Product.ID == productID).First().Amount--;
            _POSController.PlacerholderReceipt.UpdateTotalPrice();
            UpdateReceiptList();

        }

        private void btn_DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            int productID = Convert.ToInt32((sender as Button).Tag);
            _POSController.PlacerholderReceipt.RemoveTransaction(productID);
            _POSController.PlacerholderReceipt.UpdateTotalPrice();
            UpdateReceiptList();
        }

        private void listView_Receipt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void btn_AddProduct_Click(object sender, RoutedEventArgs e)
        {
            int inputInt;
            int.TryParse(textBox_AddProductID.Text, out inputInt);

            if (_POSController.GetProductFromID(inputInt) != null)
            {
                _POSController.AddSaleTransaction(_POSController.GetProductFromID(inputInt), int.Parse(textBox_ProductAmount.Text));
                UpdateReceiptList();
            }
            else
            {
                Utils.ShowErrorWarning($"Produkt med ID {inputInt} findes ikke på lageret");
            }
        }

        private void btn_PlusToReciept_Click(object sender, RoutedEventArgs e)
        {
            int inputAmount = Int32.Parse(textBox_ProductAmount.Text);
            if (inputAmount < 99)
            {
                textBox_ProductAmount.Text = (++inputAmount).ToString();
            }
        }

        private void btn_MinusToReciept_Click(object sender, RoutedEventArgs e)
        {
            int inputAmount = Int32.Parse(textBox_ProductAmount.Text);

            if (inputAmount > 1)
            {
                textBox_ProductAmount.Text = (--inputAmount).ToString();
            }
        }

        private void btn_SettingFastBtnAdd_Click(object sender, RoutedEventArgs e)
        {
            int inputInt;
            int.TryParse(textBox_CreateQuickBtnID.Text, out inputInt);

            if (_POSController.GetProductFromID(inputInt) != null && !_settingsController.quickButtonList.Any(x => x.ProductID == inputInt))
            {
                _settingsController.AddNewQuickButton(textBox_CreateQuickBtnName.Text, inputInt, grid_QuickButton.Width, grid_QuickButton.Height, btn_FastButton_click);
                listView_QuickBtn.Items.Add(new FastButton() { Button_Name = textBox_CreateQuickBtnName.Text, ProductID = inputInt });
            }
            else if (_settingsController.quickButtonList.Any(x => x.ProductID == inputInt))
            {
                Utils.ShowErrorWarning($"Produkt med dette ID {inputInt} er allerede oprettet");
            }
            else
            {
                Utils.ShowErrorWarning($"Produkt med ID {inputInt} findes ikke på lageret");
            }
        }

        public void btn_FastButton_click(object sender, RoutedEventArgs e)
        {
            _POSController.AddSaleTransaction(_POSController.GetProductFromID((sender as FastButton).ProductID));
            UpdateReceiptList();
        }

        private void UpdateGridQuickButtons()
        {
            int i = 0;
            grid_QuickButton.Children.Clear();
            foreach (FastButton button in _settingsController.quickButtonList)
            {
                if (!grid_QuickButton.Children.Contains(button))
                {
                    button.Style = FindResource("Flat_Button") as Style;

                    grid_QuickButton.Children.Add(button);
                    button.SetValue(Grid.ColumnProperty, i % 2);
                    button.SetValue(Grid.RowProperty, i / 2);
                    ++i;
                }
            }
        }

        private void btn_Remove_Quick_Button(object sender, RoutedEventArgs e)
        {
            int removeThis = _settingsController.quickButtonList.FindIndex(x => x.ProductID == Convert.ToUInt32((sender as Button).Tag));

            _settingsController.quickButtonList.RemoveAll(x => x.ProductID == Convert.ToUInt32((sender as Button).Tag));

            listView_QuickBtn.Items.RemoveAt(removeThis);

            listView_QuickBtn.Items.Refresh();
            UpdateGridQuickButtons();
        }


        CreateTemporaryProduct _createTempProduct;

        private void btn_Temporary_Click(object sender, RoutedEventArgs e)
        {
                if (_createTempProduct == null)
                {
                    _createTempProduct = new CreateTemporaryProduct();
                    _createTempProduct.Closed += delegate { _createTempProduct = null; };
                    _createTempProduct.btn_AddTempProduct.Click += delegate
                    {
                        string description = _createTempProduct.textbox_Description.Text;
                        decimal price = decimal.Parse(_createTempProduct.textbox_Price.Text);
                        int amount = int.Parse(_createTempProduct.textBox_ProductAmount.Text);
                        TempProduct NewTemp = _storageController.CreateTempProduct(description, price);
                        _POSController.AddSaleTransaction(NewTemp, amount);
                        UpdateReceiptList();
                        _createTempProduct.Close();
                    };
                }
                _createTempProduct.Activate();
                _createTempProduct.Show();
        }

        private void btn_PictureFilePath_Click(object sender, RoutedEventArgs e)
        {
            _settingsController.SpecifyPictureFilePath();
            LoadProductImages();
        }

        private void OnSelectedStartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] dateTime = datePicker_StartDate.ToString().Split(' ');
            label_CurrentStartDate.Text = $"{ dateTime[0]}";
        }

        private void OnSelectedEndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] dateTime = datePicker_EndDate.ToString().Split(' ');
            label_CurrentEndDate.Text = $"{ dateTime[0]}";
        }

        //Today?? Yesterday??
        private void Button_CreateStatistics_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = datePicker_StartDate.SelectedDate.Value;
            DateTime endDate = datePicker_EndDate.SelectedDate.Value;
            ResetStatisticsView();

            string id = null;
            if (textBox_StatisticsProductID.Text.Length > 0)
            {
                id = textBox_StatisticsProductID.Text;
            }
            string brand = comboBox_Brand.Text;
            string groupString = comboBox_Group.Text;
            Group group = null;
            if (comboBox_Group.Text != "")
            {
                group = _storageController.GroupDictionary.Values.First(x => x.Name == groupString);
            }

            CheckboxChecker(ref id, ref brand, ref group);

            _statisticsController.RequestStatisticsDate(startDate, endDate);
            _statisticsController.FilterByParameters(id, brand, group);

            DisplayStatistics();
            if (_statisticsController.TransactionsForStatistics.Count == 0)
            {
                label_NoTransactions.Visibility = Visibility.Visible;
            }
        }

        private void ResetStatisticsView()
        {
            listView_Statistics.Items.Clear();
            label_NoTransactions.Visibility = Visibility.Hidden;
        }

        private void CheckboxChecker(ref string id, ref string brand, ref Group group)
        {
            if (!checkBox_Product.IsChecked.Value)
            {
                id = null;
            }
            if (!checkBox_Brand.IsChecked.Value)
            {
                brand = null;
            }
            if (!checkBox_Group.IsChecked.Value)
            {
                group = null;
            }
        }

        private void DisplayStatistics()
        {
            int totalAmount = 0;
            decimal totalPrice = 0;

            foreach (SaleTransaction transaction in _statisticsController.TransactionsForStatistics)
            {
                listView_Statistics.Items.Add(transaction.StatisticsStrings());
                totalAmount += transaction.Amount;
                totalPrice += transaction.TotalPrice;
            }
            listView_Statistics.Items.Insert(0, new StatisticsListItem("", "Total", $"{totalAmount}", $"{totalPrice}"));
        }

        private void Button_DateToday_Click(object sender, RoutedEventArgs e)
        {
            datePicker_StartDate.SelectedDate = DateTime.Today;
            datePicker_EndDate.SelectedDate = DateTime.Today;
        }

        private void Button_DateYesterday_Click(object sender, RoutedEventArgs e)
        {
            datePicker_StartDate.SelectedDate = DateTime.Today.AddDays(-1);
            datePicker_EndDate.SelectedDate = DateTime.Today.AddDays(-1);
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



        private void TextInputNoNumberWithComma(object sender, TextCompositionEventArgs e)
        {
            // Only allows number in textfield, also with comma
            if (!char.IsDigit(e.Text, e.Text.Length - 1) && !(e.Text[e.Text.Length - 1] == ','))
                e.Handled = true;
        }

        private void BuildInformationTable()
        {
            InformationGrid.CanUserAddRows = false;
            foreach (var item in _storageController.InformationGridData)
            {
                InformationGrid.Items.Add(new { title = item[0], value = item[1] });
            }
            InformationGrid.UpdateLayout();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        ResovleTempProduct _resolveTempProduct;
        private void btn_MergeTempProduct_Click(object sender, RoutedEventArgs e)
        {
            if(_storageController.TempProductList.Where(x => x.Resolved == false).Count() > 0)
            {
                InitMergeWindow();
            }
        }

        private void InitMergeWindow()
        {
            int index = 0;
            List<TempListItem> ItemList = new List<TempListItem>();
            var tempProducts = _storageController.TempProductList.Where(x => x.Resolved == false).ToList();

            if (_resolveTempProduct == null)
            {
                _resolveTempProduct = new ResovleTempProduct();
                _resolveTempProduct.button_Merge.IsEnabled = false;
                _resolveTempProduct.Closed += delegate { _resolveTempProduct = null; };
                _resolveTempProduct.MouseLeftButtonUp += delegate
                {
                    index = _resolveTempProduct.listview_ProductsToMerge.SelectedIndex;
                    _resolveTempProduct.textBox_TempProductInfo.Text = tempProducts[index].Description;
                };
                _resolveTempProduct.textBox_IDToMerge.KeyUp += delegate { IDToMerge(); };
                _resolveTempProduct.button_Merge.Click += delegate {
                    _storageController.MergeTempProduct(tempProducts[index], int.Parse(_resolveTempProduct.textBox_IDToMerge.Text));

                };
            }

            foreach (TempProduct tempProductsToListView in tempProducts)
            {
                ItemList.Add(new TempListItem { Description = tempProductsToListView.Description, Price = tempProductsToListView.SalePrice });
            }
            _resolveTempProduct.listview_ProductsToMerge.ItemsSource = ItemList;
            _resolveTempProduct.Show();
            _resolveTempProduct.Activate();
        }

        private Product IDToMerge()
        {
            int validInput = 0;
            bool input = true;
            if (int.TryParse(_resolveTempProduct.textBox_IDToMerge.Text, out validInput))
            {
                try
                {
                    var productToMerge = _storageController.ProductDictionary[int.Parse(_resolveTempProduct.textBox_IDToMerge.Text)];
                    _resolveTempProduct.Label_MergeInfo.Content = productToMerge.Name;
                    _resolveTempProduct.button_Merge.IsEnabled = true;
                    return productToMerge;
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {

                    _resolveTempProduct.Label_MergeInfo.Content = "Ugyldigt Produkt ID";
                    _resolveTempProduct.button_Merge.IsEnabled = false;
                }
            }
            else
            {
                _resolveTempProduct.Label_MergeInfo.Content = "Forkert Input";
                _resolveTempProduct.button_Merge.IsEnabled = false;
            }
            return null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            listBox_SearchResultsSaleTab.Visibility = Visibility.Visible;
            ConcurrentDictionary<int, SearchProduct> productSearchResults = Utils.SearchForProduct(txtBox_SearchField.Text, _storageController.ProductDictionary, _storageController.GroupDictionary);
            listBox_SearchResultsSaleTab.Items.Clear();
            foreach (SearchProduct product in productSearchResults.Values.OrderByDescending(x=> x.BrandMatch + x.GroupMatch + x.NameMatch))
            {
                listBox_SearchResultsSaleTab.Items.Add(new SaleSearchResultItemControl(product.CurrentProduct.Image, product.CurrentProduct.Name));
            } 
        }

        private void btn_MergeTempProduct_Click_1(object sender, RoutedEventArgs e)
        {

        }

        public void SearchFieldLostFocus(object sender, RoutedEventArgs e)
        {
            listBox_SearchResultsSaleTab.Visibility = Visibility.Hidden;
        }

        private void EnterKeyPressedSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txtBox_SearchField_Storage.IsFocused)
            {
                btn_search_Storage_Click(sender, e);
            } else if (e.Key == Key.Enter && txtBox_SearchField.IsFocused)
            {
                btn_search_Click(sender, e);
            }
        }

        private void btn_search_Storage_Click(object sender, RoutedEventArgs e)
        {
            ConcurrentDictionary<int, SearchProduct> productSearchResults = Utils.SearchForProduct(txtBox_SearchField_Storage.Text, _storageController.ProductDictionary, _storageController.GroupDictionary);
            LoadProductGrid(productSearchResults);
        }

        private void btn_IcecreamID_Click(object sender, RoutedEventArgs e)
        {
            _settingsController.SpecifyIcecreamID(Int32.Parse(textBox_IceID.Text));
        }

        CreateStorageRoom _createStorageRoom;//mangler at ordne på buttons at comboboxen bliver opdateret.
        private void btn_newStorageRoom_Click(object sender, RoutedEventArgs e)
        {
            if (_createStorageRoom == null)
            {
                _createStorageRoom = new CreateStorageRoom();
                _createStorageRoom.Closed += delegate { _createStorageRoom = null; };
                _createStorageRoom.btn_JustQuit.Click += delegate { _createStorageRoom.Close(); _createStorageRoom = null; };
                _createStorageRoom.btn_SaveAndQuit.Click += delegate
                {
                    string storageRoomName = _createStorageRoom.textBox_Name.Text;
                    string storageRoomDescr = _createStorageRoom.textBox_descr.Text;
                    _storageController.CreateStorageRoom(storageRoomName, storageRoomDescr);
                    LoadStorageRooms();
                    _createStorageRoom.Close();
                };
            }
            _createStorageRoom.Activate();
            _createStorageRoom.Show();
        }

        private void btn_editStorageRoom_Click(object sender, RoutedEventArgs e)
        {
            int storageID = Convert.ToInt32(comboBox_storageRoomSelect.Text.Split(' ').First());
            StorageRoom chosenStorage = _storageController.StorageRoomDictionary[storageID];
            if (_createStorageRoom == null)
            {
                _createStorageRoom = new CreateStorageRoom();
                _createStorageRoom.btn_deleteStorageRoom.Visibility = Visibility.Visible;
                _createStorageRoom.textBox_Name.Text = chosenStorage.Name;
                _createStorageRoom.textBox_descr.Text = chosenStorage.Description;
                _createStorageRoom.output_StorageID.Text = chosenStorage.ID.ToString();
                _createStorageRoom.Closed += delegate { _createStorageRoom = null; };
                _createStorageRoom.btn_JustQuit.Click += delegate { _createStorageRoom.Close(); _createStorageRoom = null; };
                _createStorageRoom.btn_SaveAndQuit.Click += delegate
                {
                    string storageRoomName = _createStorageRoom.textBox_Name.Text;
                    string storageRoomDescr = _createStorageRoom.textBox_descr.Text;
                    _storageController.EditStorageRoom(storageID, storageRoomName, storageRoomDescr);
                    LoadStorageRooms();
                    _createStorageRoom.Close();
                };
                _createStorageRoom.btn_deleteStorageRoom.Click += delegate 
                {/*
                    MessageBoxResult results = MessageBox.Show($"Er du sikker på at du vil slette dette lagerrum: {chosenStorage.Name} ?", "Slet lagerrum:", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if(results == MessageBoxResult.Yes)
                    {*/
                        _storageController.DeleteStorageRoom(storageID);
                        LoadStorageRooms();
                        _createStorageRoom.Close();
                    //}
                };
            }
            _createStorageRoom.Activate();
            _createStorageRoom.Show();
        }

        private void LoadStorageRooms()
        {
           comboBox_storageRoomSelect.Items.Clear();
            foreach (KeyValuePair<int, StorageRoom> StorageRoom in _storageController.StorageRoomDictionary)
            {
                comboBox_storageRoomSelect.Items.Add($"{StorageRoom.Key.ToString()} {StorageRoom.Value.Name}");
            }
        }

        private bool firstLoad = true;
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (settingsTab.IsSelected && firstLoad)
            {
                LoadStorageRooms();
                firstLoad = false;
            }

        }

        

        private void btn_Cash_Click(object sender, RoutedEventArgs e)
        {
            _POSController.PlacerholderReceipt.CashOrCard = 1;
            _POSController.ExecuteReceipt();
            listView_Receipt.Items.Clear();
            label_TotalPrice.Content = null;
        }

        private void btn_OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
            new AdminValidation().Show();
        }

        private void btn_ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var check = new AdminValidation();
            check.Closed += delegate
            {
                if (check.IsPasswordCorrect)
                    new AdminNewPassword().Show();
            };
            check.Show();
        }
    }
}
