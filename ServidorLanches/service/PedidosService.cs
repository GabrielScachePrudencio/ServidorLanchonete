using ServidorLanches.model;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class PedidosService
    {
        private readonly PedidosRepository _pedidosRepository;

        public PedidosService(PedidosRepository pedidosRepository)
        {
            _pedidosRepository = pedidosRepository;
        }   
    
        public List<Pedido> pegarTodosOsPedidos()
        {
            return _pedidosRepository.getAllPedidos();
        }
        public Pedido pegarPedidoEItensById(int id)
        {
            return _pedidosRepository.GetPedidoItensById(id);
        }

        
        public bool addPedidoEItens(Pedido novoPedido)
        {
            return _pedidosRepository.addPedidoEItemsNovo(novoPedido);
        }
        public bool atualizarPedido(Pedido novoPedido)
        {
            return _pedidosRepository.AtualizarPedido(novoPedido);
        }
    }
}
