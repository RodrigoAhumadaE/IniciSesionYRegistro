using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InicioSesionYRegistro.Models;
using Microsoft.AspNetCore.Identity;

namespace InicioSesionYRegistro.Controllers;

public class HomeController : Controller{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context){
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index(){
        return View("Index");
    }

    [HttpGet("exito")]
    public IActionResult Exito(){
        string? email = HttpContext.Session.GetString("email");
        if(email != null){
            Usuario? usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            HttpContext.Session.SetString("Nombre", $"{usuario.Nombre} {usuario.Apellido}");
            return View("Exito");
        }
        return View("Index");
    }

    [HttpGet("logout")]
    public IActionResult Logout(){
        HttpContext.Session.Clear();
        return View("Index");
    }

    [HttpPost("procesa/registro")]
    public IActionResult ProcesaRegistro(Usuario usuario){
        if(ModelState.IsValid){
            PasswordHasher<Usuario> Hasher = new PasswordHasher<Usuario>();
            usuario.Password = Hasher.HashPassword(usuario, usuario.Password);
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            HttpContext.Session.SetString("email", usuario.Email);
            return RedirectToAction("Exito");
        }
        return View("Index");
    }

    [HttpPost("procesa/login")]
    public IActionResult ProcesaLogin(Login login){
        if(ModelState.IsValid){
            Usuario? usuario = _context.Usuarios.FirstOrDefault(u => u.Email == login.EmailLogin);
            if(usuario != null){
                PasswordHasher<Login> Hasher = new PasswordHasher<Login>();
                var result = Hasher.VerifyHashedPassword(login, usuario.Password, login.PasswordLogin);
                if(result != 0){
                    HttpContext.Session.SetString("email", login.EmailLogin);
                    return RedirectToAction("Exito");
                }
            }
            ModelState.AddModelError("PasswordLogin", "Credenciales incorrectas");
            return View("Index");
        }
        return View("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(){
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
