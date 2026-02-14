using Microsoft.AspNetCore.Mvc;
using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.model.dto; // Assumindo que você usa DTOs para receber os dados
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/consignacoes")]
    public class ConsignacaoController : ControllerBase
    {
        private readonly ConsignacaoService _Service;

        public ConsignacaoController(ConsignacaoService ConsignacaoService)
        {
            _Service = ConsignacaoService;
        }

        // 1. Listar todas as consignações
        [HttpGet]
        public IActionResult GetAllConsignacoes()
        {
            var consignacoes = _Service.GetAllConsignacoes();
            if (consignacoes == null || !consignacoes.Any())
                return NotFound("Nenhuma consignação encontrada.");

            return Ok(consignacoes);
        }

        // 2. Buscar uma consignação específica pelo ID (Útil para detalhes)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var consignacao = _Service.GetById(id);
            if (consignacao == null) return NotFound("Consignação não encontrada.");
            return Ok(consignacao);
        }


        [HttpPost]
        public IActionResult CriarConsignacao([FromBody] Consignacao dto)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            if (!ModelState.IsValid)
                return BadRequest("Preencha todos os campos obrigatórios.");

            try
            {
                var resultado = _Service.CriarConsignacao(dto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpPost("dar-baixa")]
        public async Task<IActionResult> DarBaixaConsignacao([FromBody] Consignacao consignacaoSerfinalizada)
        {
            try
            {
                var resultado = _Service.ProcessarBaixa( consignacaoSerfinalizada);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarConsignacao(int id)
        {
            try
            {
                var resultado = _Service.ProcessarEstorno(id);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}