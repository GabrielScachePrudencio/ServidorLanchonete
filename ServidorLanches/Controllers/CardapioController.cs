using Microsoft.AspNetCore.Mvc;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class CardapioController : ControllerBase
    {
        private readonly CardapioService service;

        public CardapioController(CardapioService cardapioService)
        {
            service = cardapioService;
        }

        [HttpPost("cardapio-completo")]
        public IActionResult GetAllCardapios()
        {
            var cardapios = service.GetAllCardapios();
            if (cardapios == null || cardapios.Count == 0)
            {
                return Unauthorized(new { message = "Nenhum item no cardápio encontrado." });
            }
            return Ok(cardapios);
        }
    }
}
