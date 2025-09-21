using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetroFlow.Controllers
{
    [Route("chatbot")]
    public class ChatbotController : Controller
    {
        private readonly ChatbotService _chatbotService;

        public ChatbotController(ChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        // GET: chatbot
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var chatbots = await _chatbotService.GetAllChatbotsAsync();
            return View(chatbots);
        }

        // GET: chatbot/details/5
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var chatbot = await _chatbotService.GetChatbotAsync(id);
            if (chatbot == null)
            {
                return NotFound();
            }
            return View(chatbot);
        }

        // GET: chatbot/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: chatbot/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Chatbot chatbot)
        {
            if (ModelState.IsValid)
            {
                await _chatbotService.AddChatbotAsync(chatbot);
                TempData["Flash"] = "Chatbot created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(chatbot);
        }

        // GET: chatbot/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var chatbot = await _chatbotService.GetChatbotAsync(id);
            if (chatbot == null)
            {
                return NotFound();
            }
            return View(chatbot);
        }

        // POST: chatbot/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Chatbot chatbot)
        {
            if (id != chatbot.ChatbotId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var success = await _chatbotService.UpdateChatbotAsync(chatbot);
                if (success)
                {
                    TempData["Flash"] = "Chatbot updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Flash"] = "Error updating chatbot.";
            }
            return View(chatbot);
        }

        // GET: chatbot/delete/5
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var chatbot = await _chatbotService.GetChatbotAsync(id);
            if (chatbot == null)
            {
                return NotFound();
            }
            return View(chatbot);
        }

        // POST: chatbot/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _chatbotService.DeleteChatbotAsync(id);
            if (success)
            {
                TempData["Flash"] = "Chatbot deleted successfully!";
            }
            else
            {
                TempData["Flash"] = "Error deleting chatbot.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Chatbot API ----------------
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { response = "Message cannot be empty." });

            var response = await _chatbotService.GetResponseAsync(request.Message);
            return Ok(new { response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
