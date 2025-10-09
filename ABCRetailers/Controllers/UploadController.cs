using ABCRetailers.Models;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IFunctionsApi _api;

        public UploadController(IFunctionsApi api)
        {
            _api = api;
        }

        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                {
                    // ✅ Upload via API
                    var fileName = await _api.UploadProofAsync(model.ProofOfPayment);

                    // ✅ Optionally, if your API also supports uploading to different containers/shares:
                    // await _api.UploadToFileShareAsync(model.ProofOfPayment, "contracts/payments");

                    TempData["Success"] = $"File uploaded successfully! File name: {fileName}";

                    // Clear the form
                    return View(new FileUploadModel());
                }
                else
                {
                    ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
            }

            return View(model);
        }
    }
}
