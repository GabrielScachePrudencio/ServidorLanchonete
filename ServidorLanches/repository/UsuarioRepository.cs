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

        public List<Usuario> allUsuarios()
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );


            conn.Open();
            List<Usuario> lista = new List<Usuario>();
            string sql = "SELECT * FROM usuarios";

            using var cmd = new MySqlCommand(sql, conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Usuario { 
                    Id = reader.GetInt32("id"),
                    Nome = reader.GetString("nome"),
                    Email = reader.GetString("email"),
                    Senha = reader.GetString("senha"),
                    DataCriacao = reader.GetDateTime("data_criacao"),
                    TipoUsuario = Enum.Parse<TipoUsuario>(reader.GetString("tipoUsuario"))
                });
            }

            return lista;
        }

        public Usuario GetUsuarioById(int id)
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );
            conn.Open();
            string sql = "SELECT * FROM usuarios WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
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
        public bool AtualizarUsuario(Usuario usuario)
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();

            string sql = @"UPDATE usuarios 
                   SET nome = @nome, 
                       email = @email, 
                       senha = @senha, 
                       tipoUsuario = @tipoUsuario
                   WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@senha", usuario.Senha);
            cmd.Parameters.AddWithValue("@tipoUsuario", usuario.TipoUsuario.ToString());
            cmd.Parameters.AddWithValue("@id", usuario.Id);

            int linhasAfetadas = cmd.ExecuteNonQuery();

            return linhasAfetadas > 0;
        }

        public bool DeletarUsuarioPorId(int id)
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();

            string sql = "DELETE FROM usuarios WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int linhasAfetadas = cmd.ExecuteNonQuery();

            return linhasAfetadas > 0;
        }
        public bool AdicionarUsuario(Usuario usuario)
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();

            string sql = @"INSERT INTO usuarios 
                   (nome, email, senha, data_criacao, tipoUsuario) 
                   VALUES 
                   (@nome, @email, @senha, @data_criacao, @tipoUsuario)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@senha", usuario.Senha);
            cmd.Parameters.AddWithValue("@data_criacao", usuario.DataCriacao);
            cmd.Parameters.AddWithValue("@tipoUsuario", usuario.TipoUsuario.ToString());

            int linhasAfetadas = cmd.ExecuteNonQuery();

            return linhasAfetadas > 0;
        }

    }
}
