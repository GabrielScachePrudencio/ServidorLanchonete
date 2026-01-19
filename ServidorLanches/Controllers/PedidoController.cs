using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidosService _service;

        public PedidoController(PedidosService pedidosService)
        {
            _service = pedidosService;
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var pedidos = _service.PegarTodosOsPedidos();
            if (pedidos == null || pedidos.Count == 0)
                return NotFound("Nenhum pedido encontrado.");

            return Ok(pedidos);
        }

        // GET BY ID (com itens)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var pedido = _service.PegarPedidoComItens(id);
            if (pedido == null)
                return NotFound("Pedido não encontrado.");

            return Ok(pedido);
        }

        // ADD
        [HttpPost]
        public IActionResult Add([FromBody] Pedido pedido)
        {
            if (pedido == null)
                return BadRequest("Dados inválidos.");

            var sucesso = _service.AdicionarPedidoComItens(pedido);
            if (!sucesso)
                return BadRequest("Erro ao adicionar pedido.");

            return Ok(true);
        }

        // UPDATE
        [HttpPut]
        public IActionResult Update([FromBody] Pedido pedido)
        {
            if (pedido == null || pedido.Id <= 0)
                return BadRequest("Pedido inválido.");

            var sucesso = _service.AtualizarPedido(pedido);
            if (!sucesso)
                return BadRequest("Erro ao atualizar pedido.");

            return Ok(true);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sucesso = _service.DeletarPedido(id);
            if (!sucesso)
                return NotFound("Pedido não encontrado.");

            return Ok(true);
        }


    }
}
