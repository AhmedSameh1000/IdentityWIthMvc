using Identity.Dto;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _UserManager;
        private readonly SignInManager<AppUser> _SignInManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager

            )
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDTO)
        {
            //Check for validation errors
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(registerDTO);
            }

            var CurUser = _UserManager.FindByEmailAsync(registerDTO.Email);

            if (CurUser is not null)
            {
                ModelState.AddModelError("Email", "Email is Already Exist");
                return View(registerDTO);
            }

            var user = new AppUser()
            {
                UserFullName = registerDTO.PersonName,
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone,
                EmailConfirmed = true
            };

            IdentityResult result = await _UserManager.CreateAsync(user, registerDTO.Password!);
            if (result.Succeeded)
            {
                await _SignInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return View(registerDTO);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(LogInDto loginDTO)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
        //        return View(loginDTO);
        //    }

        //    var result = await _SignInManager.PasswordSignInAsync(loginDTO.Email!, loginDTO.Password!, isPersistent: false, lockoutOnFailure: false);

        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction(nameof(HomeController.Index), "Home");
        //    }

        //    ModelState.AddModelError("Login", "Invalid email or password");
        //    return View(loginDTO);
        //}

        [HttpPost]
        public async Task<IActionResult> Login(LogInDto loginDTO)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(loginDTO);
            }

            var user = await _UserManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(loginDTO);
            }

            var passwordSignInResult = await _SignInManager.CheckPasswordSignInAsync(user, loginDTO.Password, true);

            if (!passwordSignInResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(loginDTO);
            }

            var claims = new List<Claim>
            {
                new Claim("UserName", user.UserName),
                new Claim("Email", user.Email),
                new Claim("Actor", "Ahmed Sameh ALi"),
                new Claim("newGuid", Guid.NewGuid().ToString()),
            };

            await _SignInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _SignInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}