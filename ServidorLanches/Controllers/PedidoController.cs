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

        
        [HttpGet("pedidosPorCaixa/{idcaixa}")]
        public IActionResult GetAllByCaixa(int idcaixa)
        {
            var pedidosDTOS = _service.PegarTodosOsPedidosPorCaixa(idcaixa);
            if (pedidosDTOS == null || pedidosDTOS.Count == 0)
                return NotFound("Nenhum pedido encontrado.");

            return Ok(pedidosDTOS);
        }


        [HttpGet("pedidosToday")]
        public IActionResult GetAllToday()
        {
            var pedidosDTOS = _service.PegarTodosOsPedidosFromDia();
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

            var sucesso = _service.CriarPedido(pedido);
            if (sucesso != "ok")
                return BadRequest(sucesso);

            return Ok(sucesso);
        }

        // UPDATE
        [HttpPut]
        public IActionResult Update([FromBody] PedidoDTO pedidoDTO)
        {
            if (pedidoDTO == null || pedidoDTO.Id <= 0)
                return BadRequest("Pedido inválido.");

            var resultado = _service.AtualizarPedido(pedidoDTO);

            if (resultado == "ok")
               return Ok(resultado);

            return BadRequest(resultado);

        }

        // UPDATE STATUS APENAS
        [HttpPut("{id}/status/{idnovoStatus}")]
        public IActionResult UpdateStatus(int id, int idnovoStatus)
        {
            // Chama o serviço passando apenas o ID e o novo Enum
            var resultado = _service.AtualizarStatusSomente(id, idnovoStatus);

            if (resultado)
                return Ok(resultado);

            return BadRequest(resultado);
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

        // SOBRE O ESTOQUE

    }
}
