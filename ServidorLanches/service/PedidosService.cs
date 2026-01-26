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
        public bool AtualizarStatusSomente(int id, int idstatus)
            => _repository.AtualizarStatusDoPedidoById(id, idstatus);

        public bool DeletarPedido(int id)
            => _repository.DeletePedido(id);


        //estoque
        public bool movimentarEstoque(PedidoDTO pedido)
        {
            // Adicionei o status 2 na verificação de SAÍDA
            if (pedido.IdStatus == 5 || pedido.IdStatus == 2)
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.SAIDA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.VENDA;
            }
            else if (pedido.IdStatus == 6)
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.ENTRADA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.CANCELAMENTO;
            }
            else
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.NENHUMA;
            }

            return _repository.MovimentarEstoque(pedido);
        }


    }
}
