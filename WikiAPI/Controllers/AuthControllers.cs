using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WikiAPI.Models;
using WikiAPI.Data;

namespace WikiAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthControllers : ControllerBase
    {
        private readonly ContextoWiki _contexto;

        public AuthControllers(ContextoWiki contexto)
        {
            _contexto = contexto;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroModel modelo)
        {
            if (modelo == null)
                return BadRequest("Dados inválidos");

            if (_contexto.Usuario.Any(u => u.Email == modelo.Email))
                return BadRequest("Email já cadastrado");

            var usuario = new Usuario
            {
                Nome = modelo.Nome,
                Email = modelo.Email,
                senhaHash = HashSenha(modelo.Senha),
                Perfil = "Leitor"
            };

            _contexto.Usuario.Add(usuario);
            await _contexto.SaveChangesAsync();

            return Ok(new { mensagem = "Usuário registrado com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel modelo)
        {
            // Validação explícita
            if (modelo == null)
                return BadRequest("Dados inválidos");

            if (string.IsNullOrEmpty(modelo.Email))
                return BadRequest("Email é obrigatório");

            if (string.IsNullOrEmpty(modelo.Senha))
                return BadRequest("Senha é obrigatória");

            var usuario = await _contexto.Usuario
            .FirstOrDefaultAsync(u => u.Email == modelo.Email);

            if (usuario == null || !VerificarSenha(modelo.Senha, usuario.senhaHash))
                return Unauthorized("Credenciais inválidas");

            // Criação do cookie de autenticação
            var claims = new List<Claim>
                    {
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim("Perfil", usuario.Perfil)
            };

            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims)),
            new AuthenticationProperties { IsPersistent = true });

            return Ok(new {
                usuario.Email,
                usuario.Nome,
                Profile = usuario.Perfil
            });
        
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpGet("acessonegado")]
        public IActionResult AcessoNegado() => Unauthorized("Acesso Negado - Perfil Insuficiente");

       [HttpGet("check")]
public async Task<IActionResult> CheckAuth()
{
   if (!User.Identity.IsAuthenticated)
        return Unauthorized();

    // Obter ID do usuário a partir das claims
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    
    // Buscar dados completos no banco se necessário
    var usuario = await _contexto.Usuario
        .Where(u => u.Id == userId)
        .Select(u => new {
            u.Email,
            u.Nome,
            u.Perfil
        })
        .FirstOrDefaultAsync();

    return usuario == null 
        ? Unauthorized() 
        : Ok(usuario);
}

        private string HashSenha(string senha) =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(senha));

        private bool VerificarSenha(string senha, string hash) => 
            HashSenha(senha) == hash;

        public class RegistroModel
        {
            public required string Nome { get; set; }
            public required string Email { get; set; }
            public required string Senha { get; set; }
        }

        public class LoginModel
        {
            
            public required string Email { get; set; }
            public required string Senha { get; set; }
        }
    }
}