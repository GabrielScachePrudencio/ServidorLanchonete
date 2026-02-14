namespace ServidorLanches.model.dto
{
    public class MovimentacaoEstoqueDTO
    {
        public int Id { get; set; }

        public string Produto { get; set; }
        public string idProduto { get; set; }

        public string Usuario { get; set; }
        public int? idConsignacao { get; set; }


        public TipoMovimentacaoEstoque Tipo { get; set; }

        public OrigemMovimentacaoEstoque Origem { get; set; }
        
        public String valorOrigem { get; set; }

        public int QuantidadeAntes { get; set; }

        public int QuantidadeMovimentada { get; set; }

        public int QuantidadeDepois { get; set; }

        public int? PedidoId { get; set; }

        public string Observacao { get; set; }

        public DateTime DataMovimentacao { get; set; }
    }
}
