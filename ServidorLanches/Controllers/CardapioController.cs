using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/cardapio")]
    public class CardapioController : ControllerBase
    {
        private readonly CardapioService _service;

        public CardapioController(CardapioService cardapioService)
        {
            _service = cardapioService;
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var cardapios = _service.GetAllCardapios();
            if (cardapios == null || cardapios.Count == 0)
                return NotFound("Nenhum item no cardápio encontrado.");

            return Ok(cardapios);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var item = _service.GetById(id);
            if (item == null)
                return NotFound("Item não encontrado.");

            return Ok(item);
        }

        // ADD
        [HttpPost]
        public IActionResult Add([FromBody] Cardapio cardapio)
        {
            var sucesso = _service.AddCardapio(cardapio);
            if (!sucesso)
                return BadRequest("Erro ao adicionar item.");

            return Ok(true);
        }

        // UPDATE
        [HttpPut]
        public IActionResult Update([FromBody] Cardapio cardapio)
        {
            var sucesso = _service.UpdateCardapio(cardapio);
            if (!sucesso)
                return BadRequest("Erro ao atualizar item.");

            return Ok(true);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sucesso = _service.DeleteCardapio(id);
            if (!sucesso)
                return NotFound("Item não encontrado.");

            return Ok(true);
        }
    }
}

