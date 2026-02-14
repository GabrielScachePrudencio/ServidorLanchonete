using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using PDV_LANCHES.model;
using ServidorLanches.Services;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController : ControllerBase
    {
        private readonly UsuarioService usuarioService;
        
        [HttpGet("teste-conexao")]
        public IActionResult testeConexao()
        {
            try
            {
                var bancoEstaOk = usuarioService.VerificarBancoDeDados();

                if (bancoEstaOk)
                {
                    return Ok(new { status = "sucesso", mensagem = "Servidor e Banco conectados!" });
                }

                return StatusCode(503, "Servidor ativo, mas banco de dados inacessível.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro crítico: {ex.Message}");
            }
        }

        [HttpPost("atualizar-banco")]
        public IActionResult AtualizarConexaoBanco([FromBody] ConfiguracoesBanco dados)
        {
            try
            {


                var bancoEstaOk = usuarioService.AtualizarConexaoBanco(dados);

                if (bancoEstaOk)
                {
                    return Ok(new { status = "sucesso", mensagem = "Servidor e Banco conectados!" });
                }

                return StatusCode(503, "Servidor ativo, mas banco de dados inacessível.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro crítico: {ex.Message}");
            }
        }


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
