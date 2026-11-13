using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AMSoluciones.Application.Abstracciones;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMSoluciones.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IAutenticacionService _autenticacion;

    public LoginModel(IAutenticacionService autenticacion)
    {
        _autenticacion = autenticacion;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Escriba su correo.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escriba su contraseña.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var usuario = await _autenticacion.ValidarCredencialesAsync(Input.Email, Input.Contrasena);

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Name, usuario.Nombre)
        };
        claims.AddRange(usuario.Roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("/Index");
    }
}
