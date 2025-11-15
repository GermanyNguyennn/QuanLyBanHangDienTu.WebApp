using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBanHangDienTu.WebApp.Models.MoMo;
using QuanLyBanHangDienTu.WebApp.Models.VNPay;
using QuanLyBanHangDienTu.WebApp.Repository;
using QuanLyBanHangDienTu.WebApp.Services.MoMo;
using QuanLyBanHangDienTu.WebApp.Services.VNPay;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVNPayService _vnPayService;
        private readonly IMoMoService _moMoService;
        private readonly DataContext _dataContext;
        public PaymentController(IMoMoService momoService, IVNPayService vnPayService, DataContext dataContext)
        {
            _moMoService = momoService;
            _vnPayService = vnPayService;
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlMoMo(MoMoInformationModel model)
        {
            var response = await _moMoService.CreatePaymentAsync(model);

            if (response == null)
            {
                TempData["error"] = "MoMo is not responding.";
                return RedirectToAction("Cart", "Index");
            }

            if (response.ErrorCode != 0)
            {
                TempData["error"] = $"MoMo error:  {response.LocalMessage ?? response.Message} (Error code: {response.ErrorCode})";
                return RedirectToAction("Cart", "Index");
            }

            if (string.IsNullOrEmpty(response.PayUrl))
            {
                TempData["error"] = "Did not receive payment link from MoMo.";
                return RedirectToAction("Cart", "Index");
            }

            return Redirect(response.PayUrl);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlVNPay(VNPayInformationModel model)
        {
            var response = await _vnPayService.CreatePaymentAsync(model, HttpContext);

            if (string.IsNullOrEmpty(response))
            {
                TempData["error"] = "Did not receive payment link from VNPay.";
                return RedirectToAction("Cart", "Index");
            }

            return Redirect(response);
        }
    }
}
