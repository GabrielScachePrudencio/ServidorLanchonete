namespace ServidorLanches.model
{
    public class Cardapio
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public decimal Valor { get; set; }
        public bool Disponivel { get; set; }

        public string pathImg { get; set; }
    }

}
