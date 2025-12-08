using Microsoft.AspNetCore.Mvc;
using OthelloProject.Models;

namespace OthelloProject.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult RegisterUser()
        {
            return View("Register");
        }

        [HttpPost]
        public IActionResult RegisterUser(UserDetail userDetail){
            
            UserMethods userMethods = new UserMethods();
            int i = 0;
            i = userMethods.RegisterUser(userDetail);
            
            return View("RegisterConfirmation", i);
        }
    }
}