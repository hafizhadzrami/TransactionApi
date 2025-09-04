using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace TransactionApi.TestClient
{
    // ===== Models (ikut API anda) =====
    public class ItemDetail
    {
        public string PartnerItemRef { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Qty { get; set; }
        public long UnitPrice { get; set; } // cents
    }

    public class TransactionRequest
    {
        public string PartnerKey { get; set; } = default!;
        public string PartnerRefNo { get; set; } = default!;
        public string PartnerPassword { get; set; } = default!; // Base64 string
        public long TotalAmount { get; set; }                   // cents
        public List<ItemDetail>? Items { get; set; }
        public string Timestamp { get; set; } = default!;       // ISO 8601 UTC with Z
        public string Sig { get; set; } = default!;             // signature
    }

    public static class SignatureHelper
    {
        /// <summary>
        /// Ikut spec assessment:
        /// RAW = sigtimestamp(yyyyMMddHHmmss) + partnerkey + partnerrefno + totalamount + partnerpassword(Base64)
        /// Hash SHA-256 (input UTF-8) -> output hex lowercase -> Base64(UTF-8 of hex)
        /// </summary>
        public static string GenerateSignature(
            string sigTimestampyyyyMMddHHmmss,
            string partnerKey,
            string partnerRefNo,
            long totalAmount,
            string partnerPasswordBase64)
        {
            string raw = $"{sigTimestampyyyyMMddHHmmss}{partnerKey}{partnerRefNo}{totalAmount}{partnerPasswordBase64}";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

            // hex lowercase
            string hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Base64(UTF-8 of hex)
            byte[] hexBytes = Encoding.UTF8.GetBytes(hex);
            return Convert.ToBase64String(hexBytes);
        }
    }

    internal class Program
    {
        // Tukar ikut port API anda (lihat di Swagger base URL)
        private const string BASE_URL = "https://localhost:7045";
        private const string ENDPOINT = "/api/SubmitTrxMessage";

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("=== TransactionApi TestClient ===");

            // 1) Pilih partner (dua partner dibenarkan dalam assessment)
            Console.WriteLine("Pilih Partner:");
            Console.WriteLine("  1) FAKEGOOGLE (FG-00001) / pwd: FAKEPASSWORD1234");
            Console.WriteLine("  2) FAKEPEOPLE (FG-00002) / pwd: FAKEPASSWORD4578");
            Console.Write("Pilihan [1/2] (default 1): ");
            var input = Console.ReadLine();
            bool isSecond = (input?.Trim() == "2");

            string partnerKey, partnerRefNo, partnerPasswordBase64;
            if (!isSecond)
            {
                partnerKey = "FAKEGOOGLE";
                partnerRefNo = "FG-00001";
                partnerPasswordBase64 = "RkFLRVBBU1NXT1JEMTIzNA=="; // Base64(FAKEPASSWORD1234)
            }
            else
            {
                partnerKey = "FAKEPEOPLE";
                partnerRefNo = "FG-00002";
                partnerPasswordBase64 = "RkFLRVBBU1NXT1JENDU3OA=="; // Base64(FAKEPASSWORD4578)
            }

            // 2) Sediakan items contoh (boleh tukar)
            var items = new List<ItemDetail>
            {
                new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen",   Qty = 2, UnitPrice = 500 }, // 2 x RM5.00 = RM10.00
                // new ItemDetail { PartnerItemRef = "i-00002", Name = "Ruler", Qty = 2, UnitPrice = 100 } // uncomment kalau nak cuba calculation lain
            };

            // 3) Kira totalAmount dari items supaya sentiasa match
            long totalAmount = 0;
            foreach (var it in items)
                totalAmount += it.Qty * it.UnitPrice;

            // 4) Jana timestamp mengikut spec:
            //    - timestamp (ISO untuk body): "yyyy-MM-ddTHH:mm:ss.0000000Z"
            //    - sigTimestamp untuk signature: "yyyyMMddHHmmss"
            var nowUtc = DateTime.UtcNow;
            string timestampISO = nowUtc.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");
            string sigTimestamp = nowUtc.ToString("yyyyMMddHHmmss");

            // 5) Jana signature
            string sig = SignatureHelper.GenerateSignature(
                sigTimestamp,
                partnerKey,
                partnerRefNo,
                totalAmount,
                partnerPasswordBase64
            );

            // 6) Bina request body
            var req = new TransactionRequest
            {
                PartnerKey = partnerKey,
                PartnerRefNo = partnerRefNo,
                PartnerPassword = partnerPasswordBase64, // per spec: hantar password dalam Base64
                TotalAmount = totalAmount,
                Items = items,
                Timestamp = timestampISO,
                Sig = sig
            };

            // 7) Papar JSON untuk rujukan / cURL / Postman
            string json = JsonSerializer.Serialize(req, JsonOpts);
            Console.WriteLine("\n--- JSON Payload ---");
            Console.WriteLine(json);

            // 8) Tawar untuk POST ke API anda
            Console.Write($"\nHantar POST ke {BASE_URL}{ENDPOINT}? [Y/n]: ");
            var yn = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (yn == "n")
            {
                Console.WriteLine("Okay, salin JSON di atas dan test via Swagger/Postman/cURL.");
                return;
            }

            // 9) Hantar POST (bypass dev cert untuk localhost)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true // DEV ONLY
            };

            using var http = new HttpClient(handler) { BaseAddress = new Uri(BASE_URL) };

            try
            {
                var httpResp = await http.PostAsJsonAsync(ENDPOINT, req, JsonOpts);
                string respText = await httpResp.Content.ReadAsStringAsync();

                Console.WriteLine("\n--- Server Response ---");
                Console.WriteLine($"HTTP {(int)httpResp.StatusCode} {httpResp.StatusCode}");
                Console.WriteLine(respText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR ketika call API:");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("\nSelesai. Tekan apa-apa untuk keluar.");
            Console.ReadKey();
        }
    }
}
