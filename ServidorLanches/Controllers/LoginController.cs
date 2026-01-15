using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ServidorLanches.Services;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController : ControllerBase
    {
        private readonly UsuarioService usuarioService;

        public LoginController(UsuarioService usuarioServices)
        {
            this.usuarioService = usuarioServices;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var usuario = usuarioService.Login(request.Nome, request.Senha);

            if (usuario == null)
                return Unauthorized(new { message = "Nome ou senha inválidos." });

            HttpContext.Session.SetObject("UsuarioLogado", usuario);

            return Ok(usuario);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UsuarioLogado");
            return Ok(new { message = "Logout realizado com sucesso." });
        }

        [HttpPost("usuario-logado")]
        public IActionResult UsuarioLogado()
        {
            var usuario = HttpContext.Session.GetObject<model.Usuario>("UsuarioLogado");
            if (usuario == null)
                return Unauthorized(new { message = "Nenhum usuário logado." });
            return Ok(usuario);
        }
    }

    
}
