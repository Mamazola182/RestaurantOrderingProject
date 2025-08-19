using Microsoft.AspNetCore.Mvc;
using QRCoder;
//using QRCoder.Xaml; // ⬅️ Cần thêm dòng này cho SVG
using System.Text;

namespace RestaurantOrderingProject.Controllers
{
    public class QrController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Generate(int tableId)
        {
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"table-{tableId}"));
            string url = Url.Action("Index", "Order", new { token }, Request.Scheme);

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var svgQrCode = new SvgQRCode(qrCodeData);
                string qrSvg = svgQrCode.GetGraphic(5);

                return Content(qrSvg, "image/svg+xml");
            }
        }
        //public IActionResult Generate(int tableId)
        //{
        //    string token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"table-{tableId}"));
        //    string url = Url.Action("Index", "Order", new { token }, Request.Scheme);

        //    using (var qrGenerator = new QRCodeGenerator())
        //    {
        //        QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        //        SvgQRCode qrCode = new SvgQRCode(qrCodeData);
        //        string qrSvg = qrCode.GetGraphic(5);

        //        return Content(qrSvg, "image/svg+xml");
        //    }
        //}
        public IActionResult GenerateList()
        {
            return View();
        }
    }
}
