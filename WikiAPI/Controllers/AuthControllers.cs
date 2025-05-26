using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WikiAPI.Models;
using WikiAPI.Data;

namespace WikiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthControllers : ControllerBase
    {
        private readonly ContextoWiki _contexto;
        private readonly IConfiguration _config;

        public AuthControllers(ContextoWiki contexto, IConfiguration config)
        {
            _contexto = contexto;
            _config = config;
        }
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroModel modelo)
        {
            //verifica se o email já existe
            if (_contexto.Usuario.Any(u => u.Email == modelo.Email))
                return BadRequest("Email já cadastrado");

            var usuario = new Usuario
            {
                Nome = modelo.Nome,
                Email = modelo.Email,
                senhaHash = HashSenha(modelo.Senha),
                Perfil = "Leitor" // Padrão é Leitor
            };
            _contexto.Usuario.Add(usuario);
            await _contexto.SaveChangesAsync();

            return Ok(new { mensagem = "Usuario Registrado com sucesso!" });

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel modelo)
        {
            var usuario = await _contexto.Usuario
                .FirstOrDefaultAsync(u => u.Email == modelo.Email);
            if (usuario == null || !VerificarSenha(modelo.Senha, usuario.senhaHash))
                return new ForbidResult();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim("Perfil", usuario.Perfil)
            };
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme
            );
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3),
                IsPersistent = true
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
            return Ok(new
            {
                usuario.Email,
                usuario.Nome,
                usuario.Perfil

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
        public IActionResult AcessoNegado()
        {
            return Unauthorized("Acesso Negado - Perfil Insuficiente");

        }
        private string HashSenha(string senha)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(senha));
        }
        private bool VerificarSenha(string senha, string hash)
        {
            return HashSenha(senha) == hash;
        }

        public class RegistroModel
        {
            public required string Nome { get; set; }
            public required string Email { get; set; }
            public required string Senha { get; set; }
        }
        public class LoginModel {
            public required string Email { get; set; }
            public required string Senha { get; set; }
        }
    }
}