using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.repository;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/caixas")]
    public class CaixaController : ControllerBase
    {

        private readonly CaixaService _service;

        public CaixaController(CaixaService caixaService)
        {
            _service = caixaService;
        }

        // GET ALL
        [HttpGet("todosCaixasTerminal")]
        public IActionResult GetAll()
        {
            var caixas = _service.GetAllCaixasTerminais();

            return Ok(caixas ?? new List<TerminalCaixa>());

        }
        // GET ALL
        [HttpGet("todosCaixas")]
        public IActionResult GetAllCaixas()
        {
            var caixas = _service.GetAllCaixas();

            return Ok(caixas ?? new List<Caixa>());

        }

        [HttpPost("abrircaixa")]
        public Caixa IniciarCaixaAberto([FromBody] Caixa caixa)
        {
            return _service.IniciarCaixaAberto(caixa);
        }

        [HttpPost("fecharcaixa")]
        public IActionResult FecharCaixa([FromBody] Caixa caixaIncompleto)
        {
            var caixacompleto = _service.FecharCaixa(caixaIncompleto);

            if(caixacompleto == null)
                return BadRequest("Erro ao fechar o caixa.");

            return Ok(caixacompleto);
        }

        [HttpPost("salvarfecharcaixa")]
        public IActionResult SalvarFecharCaixa([FromBody] Caixa caixaCompleto)
        {
            var sucesso = _service.SalvarFecharCaixa(caixaCompleto);
            if (!sucesso)
                return BadRequest("Erro ao salvar o fechamento do caixa.");
            return Ok(true);
        }


    }
}
