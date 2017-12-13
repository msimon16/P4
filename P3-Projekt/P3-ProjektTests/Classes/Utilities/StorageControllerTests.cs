﻿using NUnit.Framework;
using P3_Projekt_WPF.Classes.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using P3_Projekt_WPF.Classes.Database;
namespace P3_Projekt_WPF.Classes.Utilities.Tests
{


    [TestFixture()]
    public class StorageControllerTests
    {
        StorageController SC;
        [SetUp]
        public void Init()
        {
            Mysql.UseMockDatabase();
            Mysql.CheckDatabaseConnection();
            SC = new StorageController();
            SC.GetAll();
            while (!SC.ThreadsDone)
            {
                Thread.Sleep(100);
            }

        }

        [TestCase(ExpectedResult = true)]
        public bool TestDatabaseConnection()
        {
            return Mysql.ConnectionWorking;
        }

        [TestCase(11, ExpectedResult = "T-Shirt m. Kloster logo rød2")]
        [TestCase(4, ExpectedResult = "Blå Glas Fugl")]
        [TestCase(5, ExpectedResult = "Guld ring m. kloster logo")]
        [TestCase(8, ExpectedResult = "Blå Kjole")]
        public string GetProductFromDatabase(int id)
        {
            return new Product(id).Name;
        }

        [Test()]
        public void GetProductNextIDTest()
        {
            int old_id = Product.GetNextID();
            Product TestProd = new Product(11);
            TestProd.UploadToDatabase();
            int new_id = Product.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }

        [Test()]
        public void GetServiceProductNextIDTest()
        {
            int old_id = ServiceProduct.GetNextID();
            Product TestProd = new Product(11);
            TestProd.UploadToDatabase();
            int new_id = ServiceProduct.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }


        [Test()]
        public void CreateProductTest1()
        {
            int old_id = Product.GetNextID();
            Product TestProd = new Product(0, "TestProduct", "Test levering", 10m, 1, false, 1m, 5);
            TestProd.UploadToDatabase();
            int new_id = Product.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }

        [Test()]
        public void CreateProductTest2()
        {
            int old_id = Product.GetNextID();
            Product TestProd = new Product(0, "TestProduct", "Test levering", 10m, 1, false, 1m, 5);
            TestProd.UploadToDatabase();
            int new_id = Product.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }

        [Test()]
        public void CreateServiceProductTest1()
        {
            int old_id = ServiceProduct.GetNextID();
            ServiceProduct TestProd = new ServiceProduct(0, 20m, 20m, 20, "TestServiceProd", 1);
            TestProd.UploadToDatabase();
            int new_id = ServiceProduct.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }

        [Test()]
        public void CreateServiceProductTest2()
        {
            int old_id = ServiceProduct.GetNextID();
            ServiceProduct TestProd = new ServiceProduct(0, 10000m, 50000m, 20000, "TestServiceProd2", 1);
            TestProd.UploadToDatabase();
            int new_id = ServiceProduct.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }
        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool CreateProductStorageControllerTest(int diff)
        {
            int old_id = Product.GetNextID();
            ConcurrentDictionary<int, int> StorageWithAmount = new ConcurrentDictionary<int, int>();
            Random Rng = new Random();
            StorageWithAmount.TryAdd(Rng.Next() % 5, Rng.Next() % 10);
            SC.CreateProduct(old_id, "TestProd", "TestBrand", 20m, 1, false, 0m, 10m, StorageWithAmount);
            int new_id = Product.GetNextID();
            bool Identical = (SC.ProductDictionary[old_id] == SC.AllProductsDictionary[old_id]);
            bool Rest = (new_id > old_id) && SC.ProductDictionary.ContainsKey(old_id) && SC.AllProductsDictionary.ContainsKey(old_id);
            return Identical && Rest;
        }
        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool CreateServiceProductStorageControllerTest(int diff)
        {
            int old_id = Product.GetNextID();
            SC.CreateServiceProduct(old_id, 10m, 10m, 10, "TestProd", 1);
            int new_id = Product.GetNextID();
            bool Identical = (SC.ServiceProductDictionary[old_id] == SC.AllProductsDictionary[old_id]);
            bool Rest = (new_id > old_id) && SC.ServiceProductDictionary.ContainsKey(old_id) && SC.AllProductsDictionary.ContainsKey(old_id);
            return Identical && Rest;
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool CreateGroupStorageControllerTest(int diff)
        {
            int old_id = Group.GetNextID();
            SC.CreateGroup("TestGroup", "TestGroupDesc");
            int new_id = Group.GetNextID();
            bool Rest = (new_id > old_id) && SC.GroupDictionary.ContainsKey(old_id);
            return Rest;
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool CreateStorageRoomStorageControllerTest(int diff)
        {
            int old_id = StorageRoom.GetNextID();
            SC.CreateStorageRoom("TestRoom", "Dette TestRum er kun til test");
            int new_id = StorageRoom.GetNextID();
            return (new_id > old_id) && SC.StorageRoomDictionary.ContainsKey(old_id);
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool DeleteStorageRoomStorageControllerTest(int diff)
        {
            int old_id = StorageRoom.GetNextID();
            SC.CreateStorageRoom("TestRoom", "Dette TestRum er kun til test");
            int new_id = StorageRoom.GetNextID();
            bool FirstStep = (new_id > old_id) && SC.StorageRoomDictionary.ContainsKey(old_id);
            if (FirstStep)
            {
                SC.DeleteStorageRoom(old_id);
                return !SC.StorageRoomDictionary.ContainsKey(old_id);
            }
            else
            {
                return false;
            }

        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool DeleteProductStorageControllerTest(int diff)
        {
            int old_id = Product.GetNextID();
            ConcurrentDictionary<int, int> StorageWithAmount = new ConcurrentDictionary<int, int>();
            Random Rng = new Random();
            StorageWithAmount.TryAdd(Rng.Next() % 5, Rng.Next() % 10);
            SC.CreateProduct(old_id, "TestProd", "TestBrand", 20m, 1, false, 0m, 10m, StorageWithAmount);
            int new_id = Product.GetNextID();
            bool Identical = (SC.ProductDictionary[old_id] == SC.AllProductsDictionary[old_id]);
            bool Rest = (new_id > old_id) && SC.ProductDictionary.ContainsKey(old_id) && SC.AllProductsDictionary.ContainsKey(old_id);
            bool FirstStep = Identical && Rest;
            if (FirstStep)
            {
                SC.DeleteProduct(old_id);
                return !(SC.ProductDictionary.ContainsKey(old_id) || SC.AllProductsDictionary.ContainsKey(old_id));
            }
            else
            {
                return false;
            }
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        public bool DeleteGroupStorageControllerTest(int diff)
        {
            int old_id = Group.GetNextID();
            SC.CreateGroup("TestGroup", "TestGroupDesc");
            int new_id = Group.GetNextID();
            bool First = (new_id > old_id) && SC.GroupDictionary.ContainsKey(old_id);
            if (First)
            {
                SC.DeleteGroup(old_id);
                return !SC.GroupDictionary.ContainsKey(old_id);
            }
            else
            {
                return false;
            }
        }
        [Test()]
        public void CreateTempProduct()
        {
            int old_id = TempProduct.GetNextID();
            TempProduct TestProd = new TempProduct("Testing", 10m);
            TestProd.UploadToDatabase();
            int new_id = TempProduct.GetNextID();
            Assert.IsTrue(new_id > old_id);
        }

        [TestCase(3, 5, ExpectedResult = true)]
        [TestCase(4, 1, ExpectedResult = true)]
        [TestCase(5, 20, ExpectedResult = true)]
        [TestCase(7, 100, ExpectedResult = true)]
        public bool CreateOrderTransactionTest(int prodID, int incrementAmount)
        {
            int old_id = OrderTransaction.GetNextID();
            Product TestProd = SC.ProductDictionary[prodID] as Product;
            if (!TestProd.StorageWithAmount.ContainsKey(1))
            {
                TestProd.StorageWithAmount.TryAdd(1, 0);
            }
            int product_amount = TestProd.StorageWithAmount[1];
            OrderTransaction OrderTrans = new OrderTransaction(TestProd, incrementAmount, "test", 1);
            SC.OrderTransactionDictionary.TryAdd(old_id, OrderTrans);
            OrderTrans.Execute();
            OrderTrans.UploadToDatabase();
            int new_id = OrderTransaction.GetNextID();
            int product_amount2 = TestProd.StorageWithAmount[1];
            bool amount = product_amount + incrementAmount == product_amount2;
            bool first = (new_id > old_id) && SC.OrderTransactionDictionary.ContainsKey(old_id);
            return amount && first;
        }
        [TestCase(3, 5, 1, 2, ExpectedResult = true)]
        [TestCase(5, 2, 5, 2, ExpectedResult = true)]
        [TestCase(7, 1, 2, 3, ExpectedResult = true)]
        [TestCase(11, 10, 2, 1, ExpectedResult = true)]
        [TestCase(21, 9, 2, 4, ExpectedResult = true)]
        public bool CreateStorageTransactionTest(int prodID, int amount, int storage1, int storage2)
        {
            int old_id = StorageTransaction.GetNextID();
            Product TestProd = SC.ProductDictionary[prodID];
            if (!TestProd.StorageWithAmount.ContainsKey(storage1))
            {
                TestProd.StorageWithAmount.TryAdd(storage1, 0);
            }
            if (!TestProd.StorageWithAmount.ContainsKey(storage2))
            {
                TestProd.StorageWithAmount.TryAdd(storage2, 0);
            }
            int Storage1Count = TestProd.StorageWithAmount[storage1];
            int Storage2Count = TestProd.StorageWithAmount[storage2];
            StorageTransaction TestTrans = new StorageTransaction(TestProd, amount, storage1, storage2, SC.StorageRoomDictionary);
            TestTrans.Execute();
            TestTrans.UploadToDatabase();
            int new_id = StorageTransaction.GetNextID();
            bool idCompare = new_id > old_id;
            bool storageCompare1 = TestProd.StorageWithAmount[storage1] == Storage1Count - amount;
            bool storageCompare2 = TestProd.StorageWithAmount[storage2] == Storage2Count + amount;
            return idCompare && storageCompare1 && storageCompare2;
        }



        /*[Test()]
        public void ProductIDTest()
        {
            StorageController storageController = new StorageController();
            StorageRoom testStorage1 = new StorageRoom("3", "medium lager");
            KeyValuePair<StorageRoom, int> testPair = new KeyValuePair<StorageRoom, int>(testStorage1, 10);

            Group testGroup = new Group("drikkevarer", "wuhuu drikke");

            storageController.CreateProduct("mælk", "arla", Convert.ToDecimal(5), testGroup, false, Convert.ToDecimal(7), Convert.ToDecimal(10), null, testPair);

            Assert.IsTrue(storageController.ProductDictionary[0].GetFullID == "010000");
        }*/


        /*
        [TestCase("shir with banas", 3, ExpectedResult = true)]
        [TestCase("bok", 1, ExpectedResult = true)]
        [TestCase("smal blu bid with gren head", 4, ExpectedResult = true)]
        [TestCase("bana", 2, ExpectedResult = false)]
        public bool EvaluateStringLimitTest(string searched, int charDiff)
        {
            StorageController strContr = new StorageController();
            return strContr.EvaluateStringLimit(searched, charDiff);
        }

        [Test()]
        public void ComputeLevenshteinsDistanceTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("shirts", "shirts and dresses");
            Product productToBeCompared = new Product(1, "Running shoes", "Adidas", 100, testGroup.ID, false, 20, 50);
            string searchedString = "Runin shos";
            int charDifference = strContr.ComputeLevenshteinsDistance(searchedString, productToBeCompared.Name);
            Assert.IsTrue(charDifference == 3);
        }
        */
        /*
        [Test()]
        public void LevenshteinsProductSearchTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("shirts", "shirts and dresses");
            Product productToBeCompared = new Product("Running shoes", "Adidas", 100, testGroup.ID, false, 20, 50);
            string searchedString = "runin shos";
            ConcurrentQueue<Product> productList = new ConcurrentQueue<Product>();

            strContr.LevenshteinsProductSearch(searchedString, productToBeCompared, ref productList);

            Assert.IsTrue(productList.Contains(productToBeCompared));
        }

        [Test()]
        public void LevenshteinsGroupSearchTrueTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("shirts", "shirts and dresses");
            string[] searchedString = { "sirts", "random" };
            bool groupWasMatched = strContr.LevenshteinsGroupSearch(searchedString, testGroup);

            Assert.IsTrue(groupWasMatched == true);
        }
        [Test()]
        public void LevenshteinsGroupSearchFalseTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("shirts", "shirts and dresses");
            string[] searchedString = { "dresses" };
            bool groupWasMatched = strContr.LevenshteinsGroupSearch(searchedString, testGroup);

            Assert.IsTrue(groupWasMatched == false);
        }

        [Test()]
        public void LevenshteinsBrandSearchTrueTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("shirts", "shirts and dresses");
            Product productToBeCompared = new Product("Running shoes", "Adidas", 100, testGroup.ID, false, 20, 50, null);
            string[] searchedString = { "shoes", "affidas" };
            bool brandWasMatched = strContr.LevenshteinsBrandSearch(searchedString, productToBeCompared.Brand);

            Assert.IsTrue(brandWasMatched == true);
        }

        [Test()]
        public void LevenshteinsBrandSearchFalseTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("shirts", "shirts and dresses");
            Product productToBeCompared = new Product("Running shoes", "Adidas", 100, testGroup.ID, false, 20, 50, null);
            string[] searchedString = { "shoes", "nike" };
            bool brandWasMatched = strContr.LevenshteinsBrandSearch(searchedString, productToBeCompared.Brand);

            Assert.IsTrue(brandWasMatched == false);
        }
        */

        /*
        [Test()]
        public void GroupSearchTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("Adidas", "shoes and clothes");
            strContr.GroupDictionary.Add(101, testGroup);
            Product productToBeCompared = new Product("Running trousers", "Adidas", 100, testGroup.ID, false, 20, 50, null);
            strContr.ProductDictionary.Add(productToBeCompared.ID, productToBeCompared);
            string searchedString = "trousers Affidas";
            List<Product> productList = new List<Product>();

            strContr.GroupSearch(searchedString, ref productList);

            Assert.IsTrue(productList.Contains(productToBeCompared));
        }
        */
        /*
        [Test()]
        public void BrandSearchTest()
        {
            StorageController strContr = new StorageController();
            Group testGroup = new Group("Adidas", "shoes and clothes");
            Product productToBeCompared = new Product("Running shoes", "Adidas", 100, testGroup.ID, false, 20, 50, null);
            strContr.ProductDictionary.Add(100, productToBeCompared);
            string searchedString = "shoes Affidas";
            List<Product> productList = new List<Product>();
            strContr.BrandSearch(searchedString, ref productList);
            Assert.IsTrue(productList.Contains(productToBeCompared));
        }
        */
        /*
        [Test()]
        public void CreateTempProductTest()
        {
            StorageController strContr = new StorageController();
            string productDescription = "A blue shirt with yellow bananas";
            decimal sellPrice = 100;
            strContr.CreateTempProduct(productDescription, sellPrice);
            //TODO: Exists does not exist
            //Assert.IsTrue(strContr.TempProductList.Exists(x => x.Description == "A blue shirt with yellow bananas"));
        }
        */
        [Test()]
        public void ContainsSearchTest()
        {
            StorageController SC = new StorageController();
            BaseProduct TestProduct = new Product(1, "TestProductMedSlagI", "Aner det ikke", 10m, 1, false, 0m, 0m);
            if (SC.ContainsSearch("sLaG", TestProduct))
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test()]
        public void ContainsSearchTest5()
        {
            StorageController SC = new StorageController();
            BaseProduct TestProduct = new Product(1, "TestProductMedSlagI", "Aner det ikke", 10m, 1, false, 0m, 0m);
            if (SC.ContainsSearch("asdf", TestProduct))
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }
        [Test()]
        public void ContainsSearchTest2()
        {
            StorageController SC = new StorageController();
            BaseProduct TestProduct = new Product(1, "TestProductMedSlagI", "Aner det ikke", 10m, 1, false, 0m, 0m);
            if (SC.ContainsSearch("Product", TestProduct))
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test()]
        public void ContainsSearchTest3()
        {
            StorageController SC = new StorageController();
            BaseProduct TestProduct = new Product(1, "asdf", "Aner det ikke", 10m, 1, false, 0m, 0m);
            if (SC.ContainsSearch("Product", TestProduct))
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }
        [Test()]
        public void ContainsSearchTest4()
        {
            StorageController SC = new StorageController();
            BaseProduct TestProduct = new Product(1, "TestProductTing", "Aner det ikke", 10m, 1, false, 0m, 0m);
            if (SC.ContainsSearch("", TestProduct))
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }

    }
}