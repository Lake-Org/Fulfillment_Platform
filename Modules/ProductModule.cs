﻿using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    static class ProductModule {

        public static Dictionary<string, string> GetCategoriesDictionary() {
            //getting category KVP from database
            Dictionary<string, string> categoriesKVP = new();
            DataBaseInterface db = new();

            var result = db.Table("ProductTypes").Get("ID, ProductType");
            foreach (var cat in result.Values) {

                var id = cat["ID"];
                var type = cat["ProductType"];

                categoriesKVP.Add(id, type);
            }
            return categoriesKVP;
        }

        public static Product GetProduct(string sku) {
            Product prod = new();
            var categoriesKVP = GetCategoriesDictionary();
            string tablePrefix = sku.GetUntilOrEmpty();

            DataBaseInterface db = new();
            Dictionary<string, Dictionary<string, string>> whereCond;
            whereCond = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sku
                }
            };
            var result = db.Table(tablePrefix + "_Products").Where(whereCond).Get();
            foreach (var row in result.Values) {

                prod.DBID = int.Parse(row["ID"]);
                prod.title = row["Title"];
                prod.body_html = row["Body"];
                prod.vendor = row["Vendor"];
                prod.product_type = categoriesKVP[row["ProductType"]];
                prod.price = double.Parse(row["Price"]);
                prod.sku = row["SKU"];
                prod.stock = int.Parse(row["Stock"]);
                prod.barcode = row["Barcode"];
                prod.vendor_price = double.Parse(row["PriceVendor"]);
                prod.weight = double.Parse(row["Weight"]);
                prod.height = int.Parse(row["Height"]);
                prod.lenght = int.Parse(row["Lenght"]);
                prod.width = int.Parse(row["Width"]);
            }

            //getting images faster
            db = new();
            whereCond = new Dictionary<string, Dictionary<string, string>> {
                ["ProductID"] = new Dictionary<string, string> {
                    ["="] = prod.DBID.ToString()
                }
            };
            result = db.Table(tablePrefix + "_Images").Where(whereCond).Get();
            foreach (var imgRow in result.Values) {
                string imageUrl = imgRow["ImgUrl"];
                prod.images.Add(imageUrl);
            }

            //getting tags faster
            db = new();
            whereCond = new Dictionary<string, Dictionary<string, string>> {
                ["ProductID"] = new Dictionary<string, string> {
                    ["="] = prod.DBID.ToString()
                }
            };
            result = db.Table(tablePrefix + "_Tags").Where(whereCond).Get();
            foreach (var tagRow in result.Values) {
                string tag = tagRow["Tag"];

                prod.tags.Add(tag);
            }
            return prod;
        }

        public static void AddProductToDB(Product p) {
            DataBaseInterface db = new();
            string tablePrefix = p.sku.GetUntilOrEmpty();

            //getting category dictionary
            Dictionary<string, string> categoriesKVP = GetCategoriesDictionary();
            string categoryKey = categoriesKVP.FindFirstKeyByValue(p.product_type);

            //adding product to Products table
            MarkProductAsNew(p.sku);

            //inserting to *_Products table
            var InsertData = new Dictionary<string, string> {
                ["Title"] = p.title,
                ["Body"] = p.body_html,
                ["Vendor"] = p.vendor,
                ["ProductType"] = categoryKey,
                ["Price"] = p.price.ToString(),
                ["SKU"] = p.sku,
                ["Stock"] = p.stock.ToString(),
                ["Barcode"] = p.barcode,
                ["PriceVendor"] = p.vendor_price.ToString(),
                ["Weight"] = p.weight.ToString(),
                ["Height"] = p.height.ToString(),
                ["Lenght"] = p.lenght.ToString(),
                ["Width"] = p.width.ToString(),
                ["AddedTimeStamp"] = p.addedTimeStamp,
                ["ProductTypeVendor"] = p.productTypeVendor
            };
            db.Table($"{tablePrefix}_Products").Insert(InsertData);

            //getting ID of the product I just inserted
            var whereQ = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = p.sku
                }
            };
            var newProductFromDB = db.Table($"{tablePrefix}_Products").Where(whereQ).Get();
            var newProductDBID = newProductFromDB[0]["ID"];

            //add new Product images to DB
            foreach (var img in p.images) {
                var insertData = new Dictionary<string, string> {
                    ["ProductID"] = newProductDBID,
                    ["ImgUrl"] = img
                };
                db.Table($"{tablePrefix}_Images").Insert(insertData);
            }

            //add new tags
            foreach (var tag in p.tags) {
                var insertData = new Dictionary<string, string> {
                    ["ProductID"] = newProductDBID,
                    ["Tag"] = tag
                };
                db.Table($"{tablePrefix}_Tags").Insert(insertData);
            }
        }

        public static void UpdateProductToDB(Product p, string status) {
            DataBaseInterface db = new();
            string tablePrefix = p.sku.GetUntilOrEmpty();

            //updating *_Products table
            var updateData = new Dictionary<string, string> {
                ["Title"] = p.title,
                ["Body"] = p.body_html,
                ["Vendor"] = p.vendor,
                //todo: i need to pass int to DB here p has string in it
                //["ProductType"] = p.product_type, 
                ["Price"] = p.price.ToString(),
                ["Stock"] = p.stock.ToString(),
                ["Barcode"] = p.barcode,
                ["PriceVendor"] = p.vendor_price.ToString(),
                ["Weight"] = p.weight.ToString(),
                ["Height"] = p.height.ToString(),
                ["Lenght"] = p.lenght.ToString(),
                ["Width"] = p.width.ToString(),
                ["AddedTimeStamp"] = p.addedTimeStamp,
                ["ProductTypeVendor"] = p.productTypeVendor

            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = p.sku
                }
            };
            db.Table($"{tablePrefix}_Products").Where(whereUpdate).Update(updateData);

            //load all images of the product
            var whereQ = new Dictionary<string, Dictionary<string, string>> {
                ["ProductID"] = new Dictionary<string, string> {
                    ["="] = p.DBID.ToString()
                }
            };
            var oldImages = db.Table($"{tablePrefix}_Images").Where(whereQ).Get();

            //delete all images
            foreach (var img in oldImages.Values) {
                var whereDelete = new Dictionary<string, Dictionary<string, string>> {
                    ["ID"] = new Dictionary<string, string> {
                        ["="] = img["ID"]
                    }
                };

                db.Table($"{tablePrefix}_Images").Where(whereDelete).Delete();
            }

            //add new images
            foreach (var img in p.images) {
                var insertData = new Dictionary<string, string> {
                    ["ProductID"] = p.DBID.ToString(),
                    ["ImgUrl"] = img
                };
                db.Table($"{tablePrefix}_Images").Insert(insertData);
            }

            //load all tags of the product
            whereQ = new Dictionary<string, Dictionary<string, string>> {
                ["ProductID"] = new Dictionary<string, string> {
                    ["="] = p.DBID.ToString()
                }
            };
            var oldTags = db.Table($"{tablePrefix}_Tags").Where(whereQ).Get();

            //delete all tags
            foreach (var tag in oldTags.Values) {
                var whereDelete = new Dictionary<string, Dictionary<string, string>> {
                    ["ID"] = new Dictionary<string, string> {
                        ["="] = tag["ID"]
                    }
                };

                db.Table($"{tablePrefix}_Tags").Where(whereDelete).Delete();
            }

            //add new tags
            foreach (var tag in p.tags) {
                var insertData = new Dictionary<string, string> {
                    ["ProductID"] = p.DBID.ToString(),
                    ["Tag"] = tag
                };
                db.Table($"{tablePrefix}_Tags").Insert(insertData);
            }

            ChangeProductStatus(p.sku, status);
        }

        public static void MarkProductAsNew(string sku) {

            DataBaseInterface db = new();
            var InsertData = new Dictionary<string, string> {
                ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                ["Status"] = ProductStatus.New,
                ["SKU"] = sku
            };
            db.Table("Products").Insert(InsertData);
        }

        public static void ChangeProductStatus(string sku, string status) {

            //first we need to get product status and check if its "New"
            //if its "New" we cant change that
            DataBaseInterface db = new();
            var whereQ = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sku
                }
            };
            var productStatusResult = db.Table("Products").Where(whereQ).Get();
            string productStatus = productStatusResult[0]["Status"];

            var updateData = new Dictionary<string, string> {
                ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                ["Status"] = status
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sku
                }
            };

            if (productStatus == ProductStatus.New) {
                if (status == ProductStatus.Ok) {
                    updateData["Status"] = ProductStatus.Ok;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else if (status == ProductStatus.WaitingShopSync) {
                    updateData["Status"] = ProductStatus.New;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            } else if (productStatus == ProductStatus.Ok) {

                if (status == ProductStatus.NeedsArchiving) {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else if (status == ProductStatus.WaitingShopSync) {
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            } else if (productStatus == ProductStatus.NeedsArchiving) {

                if (status == ProductStatus.Archived) {
                    updateData["Status"] = ProductStatus.Archived;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else if (status == ProductStatus.NeedsArchiving) {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            } else if (productStatus == ProductStatus.Archived) {

                if (status == ProductStatus.Ok) {
                    updateData["Status"] = ProductStatus.Ok;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            } else if (productStatus == ProductStatus.WaitingShopSync) {

                if (status == ProductStatus.Ok) {
                    updateData["Status"] = ProductStatus.Ok;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else if (status == ProductStatus.NeedsArchiving) {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else if (status == ProductStatus.WaitingShopSync) {
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                } else {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            } else {
                throw new Exception($"cant change product status {productStatus} -> {status}");
            }
        }

        private static List<Product> GetTDBProducts() {
            List<Product> products = new();
            Dictionary<string, string> categoriesKVP = GetCategoriesDictionary();

            //getting main product info
            DataBaseInterface db = new();
            var result = db.Table("TDB_Products").Get();
            foreach (var prod in result.Values) {

                Product NewProduct = new();
                NewProduct.DBID = int.Parse(prod["ID"]);
                NewProduct.title = prod["Title"];
                NewProduct.body_html = prod["Body"];
                NewProduct.vendor = prod["Vendor"];
                NewProduct.product_type = categoriesKVP[prod["ProductType"]];
                NewProduct.price = double.Parse(prod["Price"]);
                NewProduct.sku = prod["SKU"];
                NewProduct.stock = int.Parse(prod["Stock"]);
                NewProduct.barcode = prod["Barcode"];
                NewProduct.vendor_price = double.Parse(prod["PriceVendor"]);
                NewProduct.weight = double.Parse(prod["Weight"]);
                NewProduct.height = int.Parse(prod["Height"]);
                NewProduct.lenght = int.Parse(prod["Lenght"]);
                NewProduct.width = int.Parse(prod["Width"]);
                NewProduct.addedTimeStamp = prod["AddedTimeStamp"];
                NewProduct.productTypeVendor = prod["ProductTypeVendor"];

                products.Add(NewProduct);
            }

            //getting images faster
            db = new();
            result = db.Table("TDB_Images").Get();
            foreach (var imgRow in result.Values) {

                int productID = int.Parse(imgRow["ProductID"]);
                string imageUrl = imgRow["ImgUrl"];

                products.Find(x => x.DBID == productID).images.Add(imageUrl);
            }

            //getting tags faster
            db = new();
            result = db.Table("TDB_Tags").Get();
            foreach (var tagRow in result.Values) {

                int productID = int.Parse(tagRow["ProductID"]);
                string tag = tagRow["Tag"];

                products.Find(x => x.DBID == productID).tags.Add(tag);
            }

            return products;
        }

        public static List<Product> GetAllProducts() {
            List<Product> p = new();

            List<Product> TDBproducts = GetTDBProducts();
            p.AddRange(TDBproducts);

            return p;
        }
    }
}