using MySql.Data.MySqlClient;
using ServidorLanches.model;
using System.Data;

namespace ServidorLanches.Repositories
{
    public class UsuarioRepository
    {
        private readonly IConfiguration _config;

        public UsuarioRepository(IConfiguration config)
        {
            _config = config;
        }

        public Usuario BuscarPorNomeESenha(string nome, string senha)
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();

            string sql = "SELECT * FROM usuarios WHERE nome = @nome AND senha = @senha";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@senha", senha);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;


            return new Usuario
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                Email = reader.GetString("email"),
                Senha = reader.GetString("senha"),
                DataCriacao = reader.GetDateTime("data_criacao"),
                TipoUsuario = Enum.Parse<TipoUsuario>(reader.GetString("tipoUsuario"))
            };
        }
    }
}
