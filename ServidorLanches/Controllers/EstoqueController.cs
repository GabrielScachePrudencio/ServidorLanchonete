using Microsoft.AspNetCore.Mvc;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/estoques")]
    public class EstoqueController : ControllerBase
    {
        private readonly EstoqueService _service;

        public EstoqueController(EstoqueService estoqueService)
        {
            _service = estoqueService;
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var dto = _service.GetAll();

            if (dto == null || dto.Count == 0)
                return NotFound("Nenhum estoque encontrado.");

            return Ok(dto);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var dto = _service.GetById(id);

            if (dto == null)
                return NotFound("Estoque não encontrado.");

            return Ok(dto);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sucesso = _service.Delete(id);

            if (!sucesso)
                return NotFound("Estoque não encontrado.");

            return Ok(true);
        }


        [HttpPost("atualizarQuantidade/{idproduto}/{quantidade}")]
        public IActionResult UpdateQtddProduto(int idproduto, int quantidade)
        {
            var sucesso = _service.UpdateQtddProduto(idproduto, quantidade);

            if (!sucesso)
                return NotFound("Estoque não encontrado.");

            return Ok(true);
        }

        // GET ALL
        [HttpGet("allestoques")]
        public IActionResult GetAllEstoques()
        {
            var dto = _service.GetAllEstoques();

            if (dto == null || dto.Count == 0)
                return NotFound("Nenhum estoque encontrado.");

            return Ok(dto);
        }
    }
}
