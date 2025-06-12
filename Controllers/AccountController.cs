using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ECommerceGestao.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace ECommerceGestao.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }        // GET: Account/Login
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Limpar cookies existentes
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            
            ViewData["ReturnUrl"] = returnUrl ?? "/";
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? "/";
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Login realizado com sucesso!";
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                    return View(model);
                }
            }

            return View(model);
        }        // GET: Account/Register
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? "/";
            return View();
        }        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? "/";
            if (ModelState.IsValid)
            {
                try
                {                    var user = new ApplicationUser { 
                        UserName = model.Email, 
                        Email = model.Email, 
                        Name = model.Name,
                        PhoneNumber = model.PhoneNumber,
                        IdentityNumber = model.IdentityNumber,
                        Address = model.Address ?? string.Empty,
                        City = model.City ?? string.Empty,
                        State = model.State ?? string.Empty,
                        ZipCode = model.ZipCode ?? string.Empty
                    };
                    
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        try
                        {                            // Adicionar o usuário ao papel de Cliente por padrão
                            try 
                            {
                                var roleManager = HttpContext.RequestServices.GetService<RoleManager<IdentityRole>>();
                                
                                // Verifica se a role Customer existe
                                if (roleManager != null && !await roleManager.RoleExistsAsync("Customer"))
                                {
                                    // Se não existe, cria a role
                                    await roleManager.CreateAsync(new IdentityRole("Customer"));
                                }
                                
                                // Adiciona o usuário à role
                                await _userManager.AddToRoleAsync(user, "Customer");
                            }
                            catch (Exception ex)
                            {
                                // Log the error but continue with the registration
                                Console.WriteLine($"Erro ao adicionar usuário à role: {ex.Message}");
                                // You may want to log this error properly in a production environment
                            }
                            
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            TempData["Success"] = "Registro realizado com sucesso!";
                            return RedirectToLocal(returnUrl);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, $"Erro ao atribuir papel ao usuário: {ex.Message}");
                        }
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // Capturar especificamente o ArgumentOutOfRangeException
                    ModelState.AddModelError(string.Empty, $"Erro de validação: {ex.Message}. Parâmetro: {ex.ParamName}");
                }
                catch (Exception ex)
                {
                    // Capturar qualquer outra exceção
                    ModelState.AddModelError(string.Empty, $"Ocorreu um erro: {ex.Message}");
                }
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Você saiu do sistema com sucesso!";
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                // Tratamento especial para erros de complexidade de senha
                if (error.Code.StartsWith("Password"))
                {
                    ModelState.AddModelError("Password", error.Description);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
