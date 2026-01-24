using MySql.Data.MySqlClient;
using ServidorLanches.model;
using System.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ServidorLanches.repository
{
    public class AdministrativoRepository
    {
        private readonly IConfiguration _config;

        public AdministrativoRepository(IConfiguration config)
        {
            _config = config;
        }

        public ConfiguracoesGerais GetConfiguracoes()
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();
            ConfiguracoesGerais config = new();
            string sql = "SELECT * FROM configuracoesGerais";

            using (var cmd = new MySqlCommand(sql, conn))
            
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    config = new ConfiguracoesGerais
                    {
                        nome = reader.GetString("nome"),
                        nomeFantasia = reader.GetString("nomeFantasia"),
                        telefone = reader.GetString("telefone"),
                        email = reader.GetString("email"),
                        endereco = reader.GetString("endereco"),
                        pathImagemLogo = reader.GetString("pathImagemLogo")
                    };
                }
            }

            return config; 

        }

        public bool AtualizarConfiguracoes(ConfiguracoesGerais config)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("MySql"));
            conn.Open();

            // SQL que atualiza os campos. 
            // Nota: Se sua tabela tiver ID, você pode adicionar "WHERE id = 1"
            string sql = @"UPDATE configuracoesGerais SET 
                    nome = @nome, 
                    nomeFantasia = @nomeFantasia, 
                    telefone = @telefone, 
                    email = @email, 
                    endereco = @endereco, 
                    pathImagemLogo = @pathImagemLogo";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", config.nome);
            cmd.Parameters.AddWithValue("@nomeFantasia", config.nomeFantasia);
            cmd.Parameters.AddWithValue("@telefone", config.telefone);
            cmd.Parameters.AddWithValue("@email", config.email);
            cmd.Parameters.AddWithValue("@endereco", config.endereco);
            cmd.Parameters.AddWithValue("@pathImagemLogo", config.pathImagemLogo ?? (object)DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<CategoriaProduto> GetCategoriaProdutos()
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();
            List<CategoriaProduto> lista = new List<CategoriaProduto>();
            string sql = "SELECT * FROM categoriaProduto";

            using (var cmd = new MySqlCommand(sql, conn))

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new CategoriaProduto() {
                        id = reader.GetInt32("id"),
                        nome = reader.GetString("nome")
                    } 
                    );
                }
            }

            return lista;
        }

        public List<TipoStatusPedido> GetStatusPedido()
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();
            List<TipoStatusPedido> lista = new List<TipoStatusPedido>();
            string sql = "SELECT * FROM statuspedido";

            using (var cmd = new MySqlCommand(sql, conn))

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new TipoStatusPedido()
                    {
                        id = reader.GetInt32("id"),
                        nome = reader.GetString("nome")
                    }
                    );
                }
            }

            return lista;
        }


    }
}
