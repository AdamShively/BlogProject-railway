using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services;
using BlogProject.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using X.PagedList;

namespace BlogProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {

            if (page < 1) {page = 1;}

            var pageSize = 4;

            var blogs = _context.Blogs
                        .OrderByDescending(b => b.Created)
                        .ToPagedListAsync(page, pageSize);

            ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");
            ViewData["MainText"] = "Tech Savvy";
            ViewData["SubText"] = "A Tech Blog Collection";

            return View(await blogs);
        }

        public IActionResult Contact(int? page)
        {
            TempData["PageNumber"] = page ?? 1;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(int page, ContactMe model)
        {
            try
            {
                model.Message = $"{model.Message} <hr/> Phone: {model.PhoneNumber}";
                await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);
                TempData["SweetAlert"] = "Message sent successfully. I'll get back to you ASAP!";
                TempData["SwalIcon"] = "success";
                return RedirectToAction("Index", "Home", new { page });
            }
            catch (SmtpCommandException)
            {
                TempData["SweetAlert"] = "Daily user sending quota exceeded. Please try again later or send an email to adammshively89@gmail.com";
                TempData["SwalIcon"] = "error";
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}