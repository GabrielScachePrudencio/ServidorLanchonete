namespace ServidorLanches.model
{
    public class ItemPedido
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public int IdCardapio { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }


        public decimal desconto { get; set; }
        public int decontoComPorcentagem { get; set; }
    }

}
