using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ServidorLanches.Repositories
{
    public class UsuarioRepository
    {
        private readonly DbConnectionManager _dbManager;

        public UsuarioRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;


        public Usuario BuscarPorNomeESenha(string nome, string senha)
        {
            using var conn = new MySqlConnection(GetConnectionString());

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
            using var conn = new MySqlConnection(GetConnectionString());


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
            using var conn = new MySqlConnection(GetConnectionString());

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
            using var conn = new MySqlConnection(GetConnectionString());

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
                GetConnectionString()
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
                GetConnectionString()
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

        public bool VerificarBancoDeDados()
        {
            try
            {
                using var conn = new MySqlConnection(GetConnectionString());

                conn.Open();

                using var cmd = new MySqlCommand("SELECT 1", conn);
                cmd.ExecuteScalar();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AtualizarConexaoBanco(ConfiguracoesBanco config)
        {
            try
            {
                // 1. Limpeza ultra-rápida (sem regex, apenas string)
                string hostOriginal = config.Host ?? "localhost";
                string hostLimpo = hostOriginal
                    .Replace("https://", "")
                    .Replace("http://", "")
                    .Split('/')[0]   // Remove qualquer coisa após a barra
                    .Split(':')[0];  // Remove a porta se o usuário digitou ex: localhost:5000

                // 2. Montagem da string (A linha que deu o 408)
                string novaCS = $"Server={hostLimpo};Port={config.PortaBanco};Database={config.NomeBanco};Uid={config.UsuarioBanco};Pwd={config.senhaBanco};Connect Timeout=5;";

                // Log para você ver no console do Servidor se a string ficou bonita
                Console.WriteLine($"--- Tentando conexão: {novaCS}");

                using var conn = new MySqlConnection(novaCS);
                conn.Open();

                _dbManager.CurrentConnectionString = novaCS;

                SalvarConexaoNoArquivo(novaCS);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- Falha no MySQL: {ex.Message}");
                return false;
            }
        }

        public void SalvarConexaoNoArquivo(string novaCS)
        {
            try
            {
                string pastaAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PDV_Lanches");
                string pastaApp = Path.Combine(pastaAppData, "SERVIDOR_Lanches_Config");
                if (!Directory.Exists(pastaApp)) Directory.CreateDirectory(pastaApp);

                string caminhoArquivo = Path.Combine(pastaApp, "configServidorConexao.json");

                // Criamos um objeto simples para salvar
                var dados = new { ConnectionString = novaCS };
                string json = JsonSerializer.Serialize(dados);

                File.WriteAllText(caminhoArquivo, json);
                Console.WriteLine($"--- Conexão salva com sucesso em: {caminhoArquivo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao persistir no JSON: {ex.Message}");
            }
        }
    }
}
