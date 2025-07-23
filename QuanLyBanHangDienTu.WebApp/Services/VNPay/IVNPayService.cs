using QuanLyBanHangDienTu.WebApp.Models.VNPay;

namespace QuanLyBanHangDienTu.WebApp.Services.VNPay
{
    public interface IVNPayService
    {
        Task<string> CreatePaymentAsync(VNPayInformationModel model, HttpContext context);
        Task<VNPayResponseModel> PaymentExecuteAsync(IQueryCollection collections);
    }
}
