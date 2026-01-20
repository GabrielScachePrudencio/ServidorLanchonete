namespace ServidorLanches.model.dto
{
    public class ItemPedidoCardapioDTO
    {
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }

        public int IdCardapio { get; set; }
        public string NomeCardapio { get; set; }
        public string pahCardapioImg { get; set; }
        public string Categoria { get; set; }

    }
}
