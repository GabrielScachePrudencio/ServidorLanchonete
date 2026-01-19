using ServidorLanches.model;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class PedidosService
    {
        private readonly PedidosRepository _repository;

        public PedidosService(PedidosRepository pedidosRepository)
        {
            _repository = pedidosRepository;
        }

        public List<Pedido> PegarTodosOsPedidos()
            => _repository.getAllPedidos();

        public Pedido PegarPedidoComItens(int id)
            => _repository.GetPedidoItensById(id);

        public bool AdicionarPedidoComItens(Pedido pedido)
            => _repository.addPedidoEItemsNovo(pedido);

        public bool AtualizarPedido(Pedido pedido)
            => _repository.AtualizarPedido(pedido);

        public bool DeletarPedido(int id)
            => _repository.DeletePedido(id);
    }
}
