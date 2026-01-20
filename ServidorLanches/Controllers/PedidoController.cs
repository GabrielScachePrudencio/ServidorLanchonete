using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.model.dto;
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
            var pedidosDTOS = _service.PegarTodosOsPedidos();
            if (pedidosDTOS == null || pedidosDTOS.Count == 0)
                return NotFound("Nenhum pedido encontrado.");

            return Ok(pedidosDTOS);
        }

        // GET BY ID (com itens)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var pedidoDTO = _service.PegarPedidoComItens(id);
            if (pedidoDTO == null)
                return NotFound("Pedido não encontrado.");

            return Ok(pedidoDTO);
        }

        // ADD
        [HttpPost]
        public IActionResult Add([FromBody] PedidoDTO pedido)
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
        public IActionResult Update([FromBody] PedidoDTO pedidoDTO)
        {
            if (pedidoDTO == null || pedidoDTO.Id <= 0)
                return BadRequest("Pedido inválido.");

            var sucesso = _service.AtualizarPedido(pedidoDTO);
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
