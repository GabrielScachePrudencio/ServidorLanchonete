namespace ServidorLanches.model
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int IdCategoria { get; set; }
        public decimal Valor { get; set; }
        public decimal PrecoUnitario { get; set; }
        public bool Disponivel { get; set; }
        public int quantidade { get; set; }
        public string pathImg { get; set; }
    }

}
