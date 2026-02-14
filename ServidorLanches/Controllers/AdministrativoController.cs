using Microsoft.AspNetCore.Mvc;
using PDV_LANCHES.model;
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


    //categorias do produto 

    [HttpGet("categoria")]
    public IActionResult getCategorias()
    {
        var lista = _administrativoService.GetAllCategoria();

        if (lista == null || lista.Count == 0)
            return NotFound("Nenhuma categoria encontrada");

        return Ok(lista);
    }

    [HttpPost("categoria")]
    public IActionResult AddCategoria([FromBody] CategoriaProduto categoria)
    {
        var sucesso = _administrativoService.AddCategoria(categoria);
        if (!sucesso)
            return BadRequest("Erro ao adicionar categoria");

        return Ok(true);
    }

    [HttpPut("categoria/{id}")]
    public IActionResult UpdateCategoria(int id, [FromBody] CategoriaProduto categoria)
    {
        var sucesso = _administrativoService.UpdateCategoria(id, categoria);
        if (!sucesso)
            return BadRequest("Erro ao atualizar categoria");

        return Ok(true);
    }

    [HttpDelete("categoria/{id}")]
    public IActionResult DeleteCategoria(int id)
    {
        var sucesso = _administrativoService.DeleteCategoria(id);
        if (!sucesso)
            return NotFound("Categoria não encontrada");

        return Ok(true);
    }


    //stauts pedidos 
    
    [HttpGet("statuspedido")]
    public IActionResult getStatusDosPedidos()
    {
        var lista = _administrativoService.GetAllStatusPedido();

        if (lista == null || lista.Count == 0)
            return NotFound("Nenhum status encontrado");


        return Ok(lista);
    }
    [HttpPost("statuspedido")]
    public IActionResult AddStatusPedido([FromBody] TipoStatusPedido status)
    {
        var sucesso = _administrativoService.AddStatusPedido(status);
        if (!sucesso)
            return BadRequest("Erro ao criar status");

        return Ok(true);
    }

    [HttpPut("statuspedido/{id}")]
    public IActionResult UpdateStatusPedido(int id, [FromBody] TipoStatusPedido status)
    {
        var sucesso = _administrativoService.UpdateStatusPedido(id, status);
        if (!sucesso)
            return BadRequest("Erro ao atualizar status");

        return Ok(true);
    }

    [HttpDelete("statuspedido/{id}")]
    public IActionResult DeleteStatusPedido(int id)
    {
        var sucesso = _administrativoService.DeleteStatusPedido(id);
        if (!sucesso)
            return NotFound("Status não encontrado");

        return Ok(true);
    }



    //formas de pagamentos 
    [HttpGet("formasdepagamentos")]
    public IActionResult getFormasDePagamentos()
    {
        var lista = _administrativoService.GetAllFormasDePagamentos();

        if (lista == null || lista.Count == 0)
            return NotFound("Nenhum formas de pagamentos encontrado");


        return Ok(lista);
    }

    [HttpPost("formasdepagamentos")]
    public IActionResult AddFormaPagamento([FromBody] FormaDePagamento forma)
    {
        var sucesso = _administrativoService.AddFormaPagamento(forma);
        if (!sucesso)
            return BadRequest("Erro ao adicionar forma de pagamento");

        return Ok(true);
    }

    [HttpPut("formasdepagamentos/{id}")]
    public IActionResult UpdateFormaPagamento(int id, [FromBody] FormaDePagamento forma)
    {
        var sucesso = _administrativoService.UpdateFormaPagamento(id, forma);
        if (!sucesso)
            return BadRequest("Erro ao atualizar forma de pagamento");

        return Ok(true);
    }

    [HttpDelete("formasdepagamentos/{id}")]
    public IActionResult DeleteFormaPagamento(int id)
    {
        var sucesso = _administrativoService.DeleteFormaPagamento(id);
        if (!sucesso)
            return NotFound("Forma de pagamento não encontrada");

        return Ok(true);
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


    ///    Configuracoes ficais
    [HttpGet("configuracoesFiscais")]
    public IActionResult getConfiguracoesFiscais()
    {
        var configFisc = _administrativoService.GetConfiguracoesFiscais();

        if (configFisc == null)
            return NotFound("Nenhum GetConfiguracoesFiscais encontrado");


        return Ok(configFisc);
    }

    [HttpPost("configuracoesFiscais")]
    public IActionResult addConfiguracoesFiscais([FromBody] ConfiguracoesFiscais config)
    {
        var sucesso = _administrativoService.AddConfiguracoesFiscais(config);
        if (!sucesso)
            return BadRequest("Erro ao atualizar configurações");

        return Ok("Atualizado com sucesso");
    }

    [HttpPut("configuracoesFiscais")]
    public IActionResult atualizarConfiguracoesFiscais([FromBody] ConfiguracoesFiscais config)
    {
        var sucesso = _administrativoService.AtualizarConfigFiscal(config);
        if (!sucesso)
            return BadRequest("Erro ao atualizar configurações");

        return Ok("Atualizado com sucesso");
    }



}
