namespace ServidorLanches.model.dto
{
    public class ItemPedidoCardapioDTO
    {
        public int IdProduto { get; set; }
        public string NomeProduto { get; set; }
        public string Categoria { get; set; }
        public string pathProdutoImg { get; set; }

        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }

        //custo de fabricação do produto
        public decimal CustoDeFabricacao { get; set; }

        public decimal desconto { get; set; }
        public int decontoComPorcentagem { get; set; }

    }

}
