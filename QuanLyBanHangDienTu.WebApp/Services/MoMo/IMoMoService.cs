using QuanLyBanHangDienTu.WebApp.Models.MoMo;

namespace QuanLyBanHangDienTu.WebApp.Services.MoMo
{
    public interface IMoMoService
    {
        Task<MoMoResponseModel> CreatePaymentAsync(MoMoInformationModel model);
        MoMoInformationModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
