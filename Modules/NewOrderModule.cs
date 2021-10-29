﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using Ikrito_Fulfillment_Platform.Models;

namespace Ikrito_Fulfillment_Platform.Modules {
    class NewOrderModule {
        private readonly string getOrdersEndPoint = "https://real-europe-corp.myshopify.com/admin/api/2021-07/orders.json";
        
        public List<Order> getOrders() {

            var client = new RestClient(getOrdersEndPoint);
            var request = new RestRequest();
            request.AddHeader("Authorization", Globals.getBase64ShopifyCreds());

            var response = client.Get(request);

            if (response.IsSuccessful) {
                string content = response.Content; // Raw content as string

                content = content[content.IndexOf("[")..];
                content = content.Remove(content.Length - 1, 1);

                return JsonConvert.DeserializeObject<List<Order>>(content);
            } else {
                return null;
            }
        }


    }
}