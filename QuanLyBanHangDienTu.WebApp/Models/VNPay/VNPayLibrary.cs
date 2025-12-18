using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using QuanLyBanHangDienTu.WebApp.Models.VNPay;

namespace QuanLyBanHangDienTu.WebApp.Models.VNPay
{
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public VNPayResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VNPayLibrary();

            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value!);
                }
            }

            // TxnRef KHÔNG parse number
            var orderId = vnPay.GetResponseData("vnp_TxnRef");

            // TransactionNo là số
            if (!long.TryParse(
                    vnPay.GetResponseData("vnp_TransactionNo"),
                    out var transactionId))
            {
                return new VNPayResponseModel { Success = false };
            }

            if (!decimal.TryParse(
                    vnPay.GetResponseData("vnp_Amount"),
                    out var amountRaw))
            {
                return new VNPayResponseModel { Success = false };
            }

            var responseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
            var secureHash = collection["vnp_SecureHash"].ToString();

            // ❗ Validate chữ ký TRƯỚC
            if (!vnPay.ValidateSignature(secureHash, hashSecret))
            {
                return new VNPayResponseModel { Success = false };
            }

            // ❗ VNPay chỉ thành công khi ResponseCode = "00"
            if (responseCode != "00")
            {
                return new VNPayResponseModel
                {
                    Success = false,
                    VnPayResponseCode = responseCode
                };
            }

            return new VNPayResponseModel
            {
                Success = true,
                PaymentMethod = "VNPay",
                OrderId = orderId,
                OrderInfo = orderInfo,
                PaymentId = transactionId.ToString(),
                TransactionId = transactionId.ToString(),
                Amount = amountRaw / 100,
                Token = secureHash,
                VnPayResponseCode = responseCode
            };
        }

        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rawData = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rawData);

            return string.Equals(myChecksum, inputHash, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        private string GetResponseData()
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _responseData)
            {
                if (key == "vnp_SecureHash" || key == "vnp_SecureHashType")
                    continue;

                if (!string.IsNullOrEmpty(value))
                {
                    data.Append(WebUtility.UrlEncode(key))
                        .Append("=")
                        .Append(WebUtility.UrlEncode(value))
                        .Append("&");
                }
            }

            if (data.Length > 0)
                data.Length--;

            return data.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.CompareOrdinal(x, y);
        }
    }
}
