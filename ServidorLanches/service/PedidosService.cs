using ServidorLanches.model;
using ServidorLanches.model.dto;
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

        public List<PedidoDTO> PegarTodosOsPedidos()
            => _repository.GetAllPedidos();

        public PedidoDTO PegarPedidoComItens(int id)
            => _repository.GetPedidoById(id);

        public bool AdicionarPedidoComItens(PedidoDTO pedido)
            => _repository.AddPedido(pedido);

        public bool AtualizarPedido(PedidoDTO pedido)
            => _repository.AtualizarPedido(pedido);
        public string AtualizarStatusSomente(int id, string status)
            => _repository.AtualizarStatusDoPedidoById(id, status);

        public bool DeletarPedido(int id)
            => _repository.DeletePedido(id);
    }
}
