using ServidorLanches.model.dto;

namespace ServidorLanches.model
{
    public class Pedido
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string CpfCliente { get; set; }
        public string StatusPedido { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataEntrega { get; set; }
        public string pahCardapioImg { get; set; }

        public TipoMovimentacaoEstoque TipoMovimentacao { get; set; }
        public OrigemMovimentacaoEstoque OrigemMovimentacaoEstoque { get; set; }

        public int IdCaixa { get; set; }
        public List<ItemPedidoCardapioDTO> Itens { get; set; }


    }
}
