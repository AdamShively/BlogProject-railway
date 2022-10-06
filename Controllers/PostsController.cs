#nullable disable

using BlogProject.Data;
using BlogProject.Models;
using BlogProject.ViewModels;
using BlogProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using BlogProject.Enums;

namespace BlogProject.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;
        private readonly IConfiguration _config;

        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService, UserManager<BlogUser> userManager, BlogSearchService blogSearchService, IConfiguration config)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
            _config = config;
        }

        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;

            var pageNum = page ?? 1;
            var pageSize = 6;

            var posts = _blogSearchService.Search(searchTerm);

            if (!posts.Any())
            {
                TempData["NoResults"] = "No Results Found";
            }

            ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");

            return View(await posts.ToPagedListAsync(pageNum, pageSize));
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogAuthor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts
        public async Task<IActionResult> BlogPostIndex(int? id, int? page)
        {
            if (id == null)
            {
                return NotFound();  
            }

            var pageNum = page ?? 1;
            var pageSize = 6;

            var posts = await _context.Posts
                    .Where(p => p.BlogId == id)
                    .OrderByDescending(p => p.Created)
                    .ToPagedListAsync(pageNum, pageSize);

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog.ImageData != null)
            {
                ViewData["HeaderImage"] = _imageService.DecodeImage(blog.ImageData, blog.ContentType);
            }
            else
            {
                ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");
            }

            ViewData["MainText"] = blog.Name;
            ViewData["SubText"] = blog.Description;

            return View(posts);
        }

        public async Task<IActionResult> Details(string slug)
        {
            ViewData["Title"] = "Post Details Page";
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.BlogAuthor)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .ThenInclude(c => c.BlogAuthor)
                .Include(p => p.Comments)
                .ThenInclude(c => c.BlogModerator)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (post == null)
            {
                return NotFound();
            }

            var dataVM = new PostDetailViewModel()
            {
                Post = post,
                Tags = _context.Tags
                        .Select(t => t.Text.ToLower())
                        .Distinct().ToList()
            };

            if (post.ImageData != null)
            {
                ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            }
            else
            {
                ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");
            }

            ViewData["MainText"] = post.Title;
            ViewData["SubText"] = post.Abstract;

            return View(dataVM);
        }

        // GET: Posts/Create
        [Authorize(Roles = "Administrator,AdminModDemo")]
        public async Task<IActionResult> Create(int id, int? page)
        {
            TempData["PageNumber"] = page ?? 1;

            var blog = await _context.Blogs.FindAsync(id);

            ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");
            ViewData["BlogName"] = blog.Name;
            ViewData["BlogId"] = id;
            ViewData["BlogAuthorId"] = new SelectList(_context.Users, "Id", "Id");

            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? page, [Bind("BlogId,Title,Abstract,Content,PostStatus,Image")] Post post, List<string> tagValues)
        {

            if (ModelState.IsValid)
            {
                post.Created = DateTime.Now;

                var blogAuthorId = _userManager.GetUserId(User);
                post.BlogAuthorId = blogAuthorId;

                //Use _imageService to store Image.
                post.Image = post.Image;
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ContentType = _imageService.ContentType(post.Image);

                //Create slug and make sure it doesn't already exist.
                var slug = _slugService.UrlUsable(post.Title);

                bool isSlugValid = true;

                if (string.IsNullOrEmpty(slug))
                {
                    isSlugValid = false;
                    ModelState.AddModelError("", "This post title is empty. Please enter a title.");
                }

                if (_slugService.slugExists(slug))
                {
                    isSlugValid = false;
                    ModelState.AddModelError("Title", "This post title already exists. Please choose a different title.");
                }

                if (!isSlugValid)
                {
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    return View(post);
                }

                post.Slug = slug;

                _context.Add(post);
                await _context.SaveChangesAsync();

                foreach (var tagText in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        PostId = post.Id,
                        BlogAuthorId = blogAuthorId,
                        Text = tagText
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("BlogPostIndex", "Posts", new { id = post.BlogId });

            }

            ViewData["BlogId"] = post.BlogId;
            TempData["PageNumber"] = page ?? 1;

            var blog = await _context.Blogs.FindAsync(post.BlogId);

            ViewData["BlogName"] = blog.Name;
            return View(post);
        }

        [Authorize(Roles = "Administrator,AdminModDemo")]
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.ImageData != null)
            {
                ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            }
            else
            {
                ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");
            }

            var blog = await _context.Blogs.FindAsync(post.BlogId);
            ViewData["BlogName"] = blog.Name;
            ViewData["BlogId"] = post.BlogId;

            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,BlogAuthorId,Title,Abstract,Content,PostStatus")] Post post, IFormFile newImage, List<string> tagValues)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (User.IsInRole(UserRole.AdminModDemo.ToString()) && post.BlogAuthorId == _config["AdminId"])
            {
                TempData["SweetAlert"] = "Users logged in as an admin/mod demo user cannot edit posts created by the primary administrator.";
                TempData["SwalIcon"] = "error";
                return RedirectToAction("BlogPostIndex", "Posts", new { id = post.BlogId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newPost = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == post.Id);

                    //newPost a pointer??
                    newPost.Updated = DateTime.Now;
                    newPost.Title = post.Title;
                    newPost.Abstract = post.Abstract;
                    newPost.Content = post.Content;
                    newPost.PostStatus = post.PostStatus;

                    var newSlug = _slugService.UrlUsable(post.Title);
                    if (newPost.Slug != newSlug)
                    {
                        if (!_slugService.slugExists(newSlug))
                        {
                            newPost.Title = post.Title;
                            newPost.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This post title already exists. Please choose a different title.");
                            ViewData["BlogId"] = post.BlogId;
                            ViewData["TagValues"] = string.Join(",", newPost.Tags.Select(t => t.Text));
                            return View(post);

                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ContentType = _imageService.ContentType(newImage);
                    }

                    //Remove all tags.
                    _context.Tags.RemoveRange(newPost.Tags);

                    //Add in tags that were not deleted by the user.
                    foreach(var tagText in tagValues)
                    {
                        _context.Add(new Tag()
                        {
                            PostId = post.Id,
                            BlogAuthorId = newPost.BlogAuthorId,
                            Text = tagText
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("BlogPostIndex", "Posts", new { id = post.BlogId });

            }

            ViewData["BlogId"] = post.BlogId;

            var blog = await _context.Blogs.FindAsync(post.BlogId);
            ViewData["BlogName"] = blog.Name;
            ViewData["BlogAuthorId"] = new SelectList(_context.Users, "Id", "Id", post.BlogAuthorId);
            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
            return View(post);
        }

        [Authorize(Roles = "Administrator,AdminModDemo")]
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogAuthor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.ImageData != null)
            {
                ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            }
            else
            {
                ViewData["HeaderImage"] = Url.Content("~/assets/img/home.jpg");
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);

            if (User.IsInRole(UserRole.AdminModDemo.ToString()) && post.BlogAuthorId == _config["AdminId"])
            {
                TempData["SweetAlert"] = "Users logged in as an admin/mod demo user cannot delete posts created by the primary administrator.";
                TempData["SwalIcon"] = "error";
                return RedirectToAction("BlogPostIndex", "Posts", new { id = post.BlogId });
            }

            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("BlogPostIndex", "Posts", new { id = post.BlogId });
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
