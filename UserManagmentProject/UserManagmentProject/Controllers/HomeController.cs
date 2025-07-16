using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using UserManagmentProject.Data;
using UserManagmentProject.Models;
using UserManagmentProject.Repositories;

namespace UserManagmentProject.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<IdentityUser> _userRepository = new Repository<IdentityUser>(new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()));
        private readonly IRepository<IdentityRole> _roleRepository = new Repository<IdentityRole>(new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()));
        private readonly IRepository<IdentityUserRole<string>> _roleUserRepository = new Repository<IdentityUserRole<string>>(new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()));
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var isInRole = User.IsInRole("Admin");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> GiveRole()
        {
            var repoUsers = _userRepository.GetAll().ToList();
            var users = _userManager.Users.ToList();

            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var repoRoles = _roleUserRepository.Get(x => x.UserId == user.Id);
                var roles = await _userManager.GetRolesAsync(user); 
                userRoles[user.Id] = roles.FirstOrDefault() ?? "UNKNOWN"; 

            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> EditRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.UserId = user.Id;
            ViewBag.UserName = user.UserName;
            ViewBag.AllRoles = allRoles;
            ViewBag.UserRoles = userRoles;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

          
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest("Roller kaldýrýlýrken hata oluþtu.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                return BadRequest("Rol eklenirken hata oluþtu.");
            }

            return RedirectToAction("GiveRole");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
