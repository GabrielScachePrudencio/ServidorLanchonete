using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.service;

namespace ServidorLanches.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidosService _pedidosService;
        public PedidoController(PedidosService pedidosService)
        {
            _pedidosService = pedidosService;
        }

        [HttpPost("all-pedidos")]
        public IActionResult pegarTodosPedidos()
        {
            var pedidos = _pedidosService.pegarTodosOsPedidos();  
            if(pedidos == null)
            {
                return Unauthorized(new {message = "Nenhum pedido encontrado."});
            }   

            return Ok(pedidos);
        }

        [HttpPost("pedido-info")]
        public IActionResult pegarPedidoEItensId([FromQuery] int idPedido)
        {
            var pedido = _pedidosService.pegarPedidoEItensById(idPedido);  
            if(pedido == null)
            {
                return Unauthorized(new {message = "Nenhum pedido encontrado."});
            }   

            return Ok(pedido);
        }


        //atualiza ou adiciona pedido e itens
        [HttpPost("add-pedido-itens")]
        public IActionResult addPedidoEItens([FromBody] model.Pedido novoPedido)
        {
            if (novoPedido == null) return BadRequest("Dados inválidos.");

            var pedidoExistente = _pedidosService.pegarPedidoEItensById(novoPedido.Id);
            

            if (pedidoExistente != null)
            {
                var atualizou = _pedidosService.atualizarPedido(novoPedido);
                if (atualizou)
                {
                    return Ok(new { message = "Pedido adicionado com sucesso!" });
                }
            }
            else
            {
                var adicionou = _pedidosService.addPedidoEItens(novoPedido);
                if (adicionou)
                {
                    return Ok(new { message = "Pedido adicionado com sucesso!" });
                }
            }

            return Unauthorized(new {message = "Não foi possível adicionar o pedido."});
            
        }   


    }
}
