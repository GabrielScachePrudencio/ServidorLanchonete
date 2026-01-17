namespace ServidorLanches.model
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateTime DataCriacao { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
    }
}
