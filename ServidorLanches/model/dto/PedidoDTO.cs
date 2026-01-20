namespace ServidorLanches.model.dto
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string CpfCliente { get; set; }
        public string StatusPedido { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataCriacao { get; set; }

        public List<ItemPedidoCardapioDTO> Itens { get; set; } = new();
    }
}
