using Microsoft.AspNetCore.Mvc;
using UserManagmentProject.Data;

namespace UserManagmentProject.Controllers
{
    public class UserController : Controller
    {


        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
