﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P3_Projekt_WPF.Classes.Database;
using P3_Projekt_WPF.Classes.Utilities;
using System.Collections.Concurrent;

namespace P3_Projekt_WPF.Classes
{
    public class StorageTransaction : Transaction
    {
        private StorageRoom _source;
        private StorageRoom _destination;

        public StorageTransaction(Product product, int amount, int sourceInt, int destinationInt, ConcurrentDictionary<int, StorageRoom> storageWithAmountDictionary) : base(product, amount)
        {
            _source = storageWithAmountDictionary[sourceInt];
            _destination = storageWithAmountDictionary[destinationInt];
        }

        public StorageTransaction(int id) : base(null, 0)
        {
            _id = id;
            GetFromDatabase();
        }

        public override void Execute()
        {
            (Product as Product).StorageWithAmount[_source.ID] -= Amount;
            (Product as Product).StorageWithAmount[_destination.ID] += Amount;
        }

        public override void GetFromDatabase()
        {
            string sql = $"SELECT * FROM `storage_transaction` WHERE `id` = '{_id}'";
            CreateFromRow(Mysql.RunQueryWithReturn(sql).RowData[0]);
        }

        public override void CreateFromRow(Row Table)
        {
            _id = Convert.ToInt32(Table.Values[0]);
            Product = new Product(Convert.ToInt32(Table.Values[1]));
            Amount = Convert.ToInt32(Table.Values[2]);
            Date = Convert.ToDateTime(Table.Values[3]);
            _source = new StorageRoom(Convert.ToInt32(Table.Values[4]));
            _destination = new StorageRoom(Convert.ToInt32(Table.Values[5]));
        }

        public override void UploadToDatabase()
        {
            string sql = "INSERT INTO `storage_transaction` (`id`, `product_id`, `amount`, `source_storageroom_id`, `destination_storageroom_id`)"+
                $" VALUES (NULL, '{Product.ID}', '{Amount}', '{_source.ID}', '{_destination.ID}');";
            Mysql.RunQuery(sql);
            Product.UpdateInDatabase();
        }


        public override void UpdateInDatabase()
        {
            string sql = $"UPDATE `storage_transaction` SET " +
               $"`product_id` = '{Product.ID}'," +
               $"`amount` = '{Amount}'," +
               $"`source_storageroom_id` = '{_source.ID}'," +
               $"`destination_storageroom_id` = '{_destination.ID}' " +
               $"WHERE `id` = {_id};";
            Mysql.RunQuery(sql);
        }
    }
}
