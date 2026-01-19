using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.service;
using ServidorLanches.Services;

[ApiController]
[Route("api/administrativo")]
public class AdministrativoController : ControllerBase
{
    private readonly AdministrativoService _administrativoService;
    private readonly UsuarioService _usuarioService;

    public AdministrativoController(
        AdministrativoService administrativoService,
        UsuarioService usuarioService)
    {
        _administrativoService = administrativoService;
        _usuarioService = usuarioService;
    }

    // CONFIGURAÇÕES
    [HttpGet("configuracoes-gerais")]
    public IActionResult GetConfiguracoesGerais()
    {
        var config = _administrativoService.GetConfiguracoes();
        if (config == null)
            return NotFound("Nenhuma configuração encontrada.");

        return Ok(config);
    }

    [HttpPut("configuracoes")]
    public IActionResult UpdateConfiguracoes([FromBody] ConfiguracoesGerais config)
    {
        var sucesso = _administrativoService.AtualizarConfiguracoes(config);
        if (!sucesso)
            return BadRequest("Erro ao atualizar configurações");

        return Ok("Atualizado com sucesso");
    }

    // USUÁRIOS
    [HttpGet("usuarios")]
    public IActionResult TodosUsuarios()
    {
        var usuarios = _usuarioService.todosUsuarios();
        if (usuarios == null || usuarios.Count == 0)
            return NotFound("Nenhum usuário encontrado.");

        return Ok(usuarios);
    }

    [HttpPost("usuarios")]
    public IActionResult AddUsuario([FromBody] Usuario novoUsuario)
    {
        var sucesso = _usuarioService.AddUsuario(novoUsuario);
        if (!sucesso)
            return BadRequest("Erro ao criar usuário");

        return Ok(true);
    }

    [HttpPut("usuarios")]
    public IActionResult AtualizarUsuario([FromBody] Usuario usuario)
    {
        var sucesso = _usuarioService.AtualizarUsuario(usuario);
        if (!sucesso)
            return BadRequest("Erro ao atualizar usuário");

        return Ok(true);
    }

    [HttpDelete("usuarios/{id}")]
    public IActionResult DeletarUsuario(int id)
    {
        var sucesso = _usuarioService.deletarUsuario(id);
        if (!sucesso)
            return NotFound("Usuário não encontrado");

        return Ok(true);
    }
}
