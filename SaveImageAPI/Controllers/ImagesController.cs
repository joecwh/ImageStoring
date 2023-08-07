using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaveImageAPI.Data;
using SaveImageAPI.Models;
using SaveImageAPI.Resources;

namespace SaveImageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImagesController(ApplicationDbContext context = null)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage()
        {
            var files = Request.Form.Files;
            if (files == null || files.Count <= 0)
                return BadRequest("Invalid file.");

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        var imageEntity = new Image { ImageFile = memoryStream.ToArray() };
                        _context.Images.Add(imageEntity);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Image uploaded successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var images = await _context.Images.ToListAsync();
            List<ImageResponse> response = new List<ImageResponse>();
            if(images != null)
            {
                foreach (var image in images)
                {
                    var imageDataUrl = Url.Action("GetImageBytes", new { id = image.Id });
                    var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                    var fullImageUrl = $"{baseUrl}{imageDataUrl}";
                    ImageResponse imageResponse = new ImageResponse { Id = image.Id, ImageFilePath = fullImageUrl };
                    response.Add(imageResponse);
                }
            }
            return Ok(response);
        }


        [HttpGet("{id}")]
        public IActionResult GetImage(int id)
        {
            var imageEntity = _context.Images.FirstOrDefault(i => i.Id == id);
            if (imageEntity == null)
                return NotFound();

            var imageDataUrl = Url.Action("GetImageBytes", new { id = imageEntity.Id });
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";

            var fullImageUrl = $"{baseUrl}{imageDataUrl}";

            return Ok(new { ImageUrl = fullImageUrl });
        }

        [HttpGet("image-bytes/{id}")]
        public IActionResult GetImageBytes(int id)
        {
            var imageEntity = _context.Images.FirstOrDefault(i => i.Id == id);
            if (imageEntity == null)
                return NotFound();

            string contentType = GetImageContentType(imageEntity.ImageFile);

            if (contentType == null)
                return BadRequest("Invalid image format.");

            return File(imageEntity.ImageFile, contentType);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteImage(int id)
        {
            var image = _context.Images.Find(id);
            if(image == null)
                return NotFound();

            try
            {
                _context.Images.Remove(image);
                _context.SaveChanges();

                return Ok("Image deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return BadRequest();
        }

        private string GetImageContentType(byte[] imageBytes)
        {
            // Check the magic bytes at the start of the image file to determine its format
            if (imageBytes.Length >= 2)
            {
                if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                    return "image/jpeg"; // JPEG format
                else if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50)
                    return "image/png"; // PNG format
                else if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49)
                    return "image/gif"; // GIF format
                                        // Add more checks for other image formats as needed
            }

            return null;
        }
    }
}
