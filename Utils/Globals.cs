﻿using System;
using System.Text;

namespace Ikrito_Fulfillment_Platform.Utils {
    static class Globals {
        // Shopify app creds
        private static readonly string shopifyAPIKey = "40a65778e811be7da9d06d9dcc6d7e8e";
        private static readonly string shopifyAPIPass = "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";

        public static string getBase64ShopifyCreds() {
            string preEncodeCreds = shopifyAPIKey + ":" + shopifyAPIPass;
            byte[] textBytes = Encoding.UTF8.GetBytes(preEncodeCreds);
            string encodedCreds = Convert.ToBase64String(textBytes);

            return "Basic " + encodedCreds;
        }

        // AWS mySQL DB creds
        public const string endpoint = "ikrito-db.c175yaycfw7i.eu-central-1.rds.amazonaws.com";
        public const string port = "3306";
        public const string user = "admin";
        public const string pass = "p3gD2Z5fbztX8Uh";
        public const string db = "main";

    }
}
