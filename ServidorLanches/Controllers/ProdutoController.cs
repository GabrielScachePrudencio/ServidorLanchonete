using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _service;

        public ProdutoController(ProdutoService produtoService)
        {
            _service = produtoService;
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var cardapios = _service.GetAllProduto();
            if (cardapios == null || cardapios.Count == 0)
                return NotFound("Nenhum item no cardápio encontrado.");

            return Ok(cardapios);
        }
        [HttpGet("ativos")]
        public IActionResult GetAllAtivos()
        {
            var cardapios = _service.GetAllProdutoAtivos();
            if (cardapios == null || cardapios.Count == 0)
                return NotFound("Nenhum item no cardápio encontrado.");

            return Ok(cardapios);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var item = _service.GetByIdProduto(id);
            if (item == null)
                return NotFound("Item não encontrado.");

            return Ok(item);
        }

        // ADD
        [HttpPost]
        public IActionResult Add([FromBody] Produto cardapio)
        {
            var sucesso = _service.AddProduto(cardapio);
            if (!sucesso)
                return BadRequest("Erro ao adicionar item.");

            return Ok(true);
        }

        // UPDATE
        [HttpPut]
        public IActionResult Update([FromBody] Produto cardapio)
        {
            var sucesso = _service.UpdateProduto(cardapio);
            if (!sucesso)
                return BadRequest("Erro ao atualizar item.");

            return Ok(true);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sucesso = _service.DeleteProduto(id);
            if (!sucesso)
                return NotFound("Item não encontrado.");

            return Ok(true);
        }
    }
}

