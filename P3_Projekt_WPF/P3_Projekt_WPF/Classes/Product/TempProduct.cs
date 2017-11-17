﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P3_Projekt_WPF.Classes.Database;
using P3_Projekt_WPF.Classes.Utilities;
namespace P3_Projekt_WPF.Classes
{
    public class TempProduct : BaseProduct
    {
        public string Description;
        public bool Resolved;
        public int ResolvedProductID = 0;

        public TempProduct (string description, decimal salePrice) : base(salePrice)
        {
            Description = description;
            Resolved = false;
        }

        public TempProduct(int id) : base(0)
        {
            ID = id;
            GetFromDatabase();
        }

        public TempProduct(Row row) : base(0)
        {
            CreateFromRow(row);
        }

        public static int GetNextID()
        {
            string sql = "SHOW TABLE STATUS LIKE 'temp_products'";
            TableDecode Results = Mysql.RunQueryWithReturn(sql);
            return Convert.ToInt32(Results.RowData[0].Values[10]);
        }

        public override string GetName()
        {
            return Description;
        }

        public void Edit(string newDescription, decimal newSalePrice)
        {
            Description = newDescription;
            SalePrice = newSalePrice;
            UpdateInDatabase();
        }

        public void Resolve(Product MatchedProduct)
        {
            Resolved = true;
            ResolvedProductID = MatchedProduct.ID;
            UpdateInDatabase();
        }

        public SaleTransaction GetTempProductsSaleTransaction()
        {
            if (Resolved)
            {
                string sql = $"SELECT * FROM `sale_transactions` WHERE `product_type` = 'temp_products' AND `product_id` = '{this.ID}'";
                TableDecode getTransaction = Mysql.RunQueryWithReturn(sql);
                SaleTransaction saleTrans = new SaleTransaction(getTransaction.RowData[0]);
                return saleTrans;
            }
            //TODO: Lave custom exception
            throw new Exception("Dette tempproduct er ikke resolved");
        }

        public override void GetFromDatabase()
        {
            string sql = $"SELECT * FROM `temp_product` where `id` = '{ID}'";
            CreateFromRow(Mysql.RunQueryWithReturn(sql).RowData[0]);
        }

        public override void CreateFromRow(Row Table)
        {
            ID = Convert.ToInt32(Table.Values[0]);
            SalePrice = Convert.ToDecimal(Table.Values[1]);
            Description = Table.Values[2];
            Resolved = Convert.ToBoolean(Table.Values[3]);
            ResolvedProductID = Convert.ToInt32(Table.Values[4]);
        }

        public override void UploadToDatabase()
        {
            string sql = "INSERT INTO `temp_products` (`id`, `sale_price`, `description`, `resolved`, `resolved_product_id`)"+
                $"VALUES (NULL, '{SalePrice}', '{GetName()}', '{Convert.ToInt32(Resolved)}', '{ResolvedProductID}');";
            Mysql.RunQuery(sql);
        }

        public override void UpdateInDatabase()
        {
            string sql = $"UPDATE `temp_products` SET " +
                $"`sale_price` = '{SalePrice}'," +
                $"`description` = '{GetName()}'," +
                $"`resolved` = '{Convert.ToInt32(Resolved)}'," +
                $"`resolved_product_id` = '{ResolvedProductID}' " +
                $"WHERE `id` = {ID};";
            Mysql.RunQuery(sql);
        }

    }
}
