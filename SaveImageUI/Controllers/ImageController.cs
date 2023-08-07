using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SaveImageUI.Models;

namespace SaveImageUI.Controllers
{
    public class ImageController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7150/api/Images");
        private readonly HttpClient _httpClient;
        public ImageController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = baseAddress;
        }

        public IActionResult Index()
        {
            List<Image> images = new List<Image>();
            HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                images = JsonConvert.DeserializeObject<List<Image>>(data);
                images = images.OrderByDescending(u => u.Id).ToList();
            }
            return View(images);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFileCollection files)
        {
            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                content.Add(fileContent, "files", file.FileName);
            }

            HttpResponseMessage response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/upload", content);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return Json(new { success = true, message = "Upload successful." });    
            }
            else
            {
                return Json(new { success = false, message = "Upload failed." });
            }
        }

        public IActionResult Delete(int id)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/{id}").Result;
            return RedirectToAction("Index");
        }
    }
}
