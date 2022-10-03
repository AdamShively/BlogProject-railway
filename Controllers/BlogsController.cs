using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlogProject.Enums;
using MovieProject.Models.Settings;
using Microsoft.Extensions.Options;

namespace BlogProject.Controllers
{
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IConfiguration _config;

        //contructor injection..
        public BlogsController(ApplicationDbContext context, IImageService imageService, UserManager<BlogUser> userManager, IConfiguration config)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _config = config;
        }

        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            //Goes out to database, talks to blogs table, includes related blog author to query author of blog
            //Returns an asycronous list 
            var applicationDbContext = _context.Blogs.Include(b => b.BlogAuthor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogAuthor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Blogs/Create
        [Authorize(Roles = "Administrator,AdminModDemo")]
        public IActionResult Create(int? page)
        {
            ViewData["BlogAuthorId"] = new SelectList(_context.Users, "Id", "Id");
            TempData["PageNumber"] = page ?? 1;
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int page, [Bind("Name,Description,Image")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.Created = DateTime.Now;
                blog.BlogAuthorId = _userManager.GetUserId(User);
                blog.ImageData = await _imageService.EncodeImageAsync(blog.Image);
                blog.ContentType = _imageService.ContentType(blog.Image);

                _context.Add(blog); //Add instance of blog to ApplicationDbContext instance dbset.
                await _context.SaveChangesAsync();  //Store data in db, save permentantly.
                return RedirectToAction("Index", "Home", new { page });
            }
            ViewData["BlogAuthorId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogAuthorId);
            return View(blog);
        }

        // GET: Blogs/Edit/5
        [Authorize(Roles = "Administrator,AdminModDemo")]
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            TempData["PageNumber"] = page ?? 1;
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int page, [Bind("Id,Name,Description,BlogAuthorId")] Blog blog, IFormFile? newImage)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (User.IsInRole(UserRole.AdminModDemo.ToString()) && blog.BlogAuthorId == _config["AdminId"])
            {
                TempData["SweetAlert"] = "Users logged in as an admin/mod demo user cannot edit blogs created by the primary administrator.";
                return RedirectToAction("Index", "Home", new { page });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newBlog = await _context.Blogs.FindAsync(blog.Id);
                    newBlog.Updated = DateTime.Now;

                    if (newBlog.Name != blog.Name)
                    {
                        newBlog.Name = blog.Name;
                    }

                    if (newBlog.Description != blog.Description)
                    {
                        newBlog.Description = blog.Description;
                    }

                    if (newImage is not null)
                    {
                        newBlog.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newBlog.ContentType = _imageService.ContentType(newImage);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home", new { page });
            }
            ViewData["BlogAuthorId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogAuthorId);
            return View(blog);
        }

        // GET: Blogs/Delete/5
        [Authorize(Roles = "Administrator,AdminModDemo")]
        public async Task<IActionResult> Delete(int? id, int? page)
        {

            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogAuthor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            TempData["PageNumber"] = page ?? 1;

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int page)
        {

            if (_context.Blogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Blogs'  is null.");
            }

            var blog = await _context.Blogs.FindAsync(id);

            if (User.IsInRole(UserRole.AdminModDemo.ToString()) && blog.BlogAuthorId == _config["AdminId"])
            {
                TempData["SweetAlert"] = "Users logged in as an admin/mod demo user cannot delete blogs created by the primary administrator.";
                return RedirectToAction("Index", "Home", new { page });
            }

            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home", new { page });
        }

        private bool BlogExists(int id)
        {
            return (_context.Blogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}