namespace ServidorLanches.model
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }

        // nunca expor senha fora da API
        public string Senha { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
