using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AdministrativoController
    {
        private readonly AdministrativoService _administrativoService;
        public AdministrativoController(AdministrativoService ads)
        {
            _administrativoService = ads;
        }

        [HttpGet("configuracoes-gerais")]
        public IActionResult GetConfiguracoesGerais()
        {
            var config = _administrativoService.GetConfiguracoes();
            if (config == null)
            {
                return new UnauthorizedObjectResult(new { message = "Nenhuma configuração encontrada." });
            }
            return new OkObjectResult(config);
        }

        [HttpPut("atualizar")]
        public IActionResult Update([FromBody] ConfiguracoesGerais config)
        {
            var sucesso = _administrativoService.AtualizarConfiguracoes(config);
            if (sucesso) return new OkObjectResult(new { message = "Atualizado com sucesso" });
            return new UnauthorizedObjectResult("Erro ao atualizar configurações");
        }


    }
}
