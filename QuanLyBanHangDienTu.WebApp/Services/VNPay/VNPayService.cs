using QuanLyBanHangDienTu.WebApp.Models.VNPay;

namespace QuanLyBanHangDienTu.WebApp.Services.VNPay
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task<string> CreatePaymentAsync(VNPayInformationModel model, HttpContext context)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]!);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var txnRef = Guid.NewGuid().ToString("N");

            var pay = new VNPayLibrary();
            var returnUrl = _configuration["Vnpay:PaymentBackReturnUrl"]!;

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]!);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]!);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]!);
            pay.AddRequestData("vnp_Amount", ((long)(model.Amount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]!);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]!);
            pay.AddRequestData("vnp_OrderInfo", model.OrderInfo!);
            pay.AddRequestData("vnp_OrderType", model.OrderType!);
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", txnRef);

            var paymentUrl = pay.CreateRequestUrl(
                _configuration["Vnpay:BaseUrl"]!,
                _configuration["Vnpay:HashSecret"]!
            );

            return Task.FromResult(paymentUrl);
        }

        public Task<VNPayResponseModel> PaymentExecuteAsync(IQueryCollection collections)
        {
            try
            {
                var pay = new VNPayLibrary();
                var response = pay.GetFullResponseData(
                    collections,
                    _configuration["Vnpay:HashSecret"]!
                );

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("VNPay Callback Error: " + ex);
                return Task.FromResult(new VNPayResponseModel { Success = false });
            }
        }
    }
}
