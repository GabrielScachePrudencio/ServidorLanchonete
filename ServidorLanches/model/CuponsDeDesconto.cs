namespace ServidorLanches.model
{
    public class CupomDesconto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal? Percentual { get; set; }
        public decimal? ValorFixo { get; set; }
        public DateTime? DataValidade { get; set; }
        public bool Ativo { get; set; }
    }

}
