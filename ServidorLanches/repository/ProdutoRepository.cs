using MySql.Data.MySqlClient;
using ServidorLanches.model;

namespace ServidorLanches.repository
{
    public class ProdutoRepository
    {
        private readonly IConfiguration _config;

        public ProdutoRepository(IConfiguration config)
        {
            _config = config;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(
                _config.GetConnectionString("MySql")
            );
        }

        // GET ALL
        public List<Produto> GetAll()
        {
            using var conn = GetConnection();
            conn.Open();

            List<Produto> produtos = new();

            string sql = "SELECT * FROM produtos";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    Id = reader.GetInt32("id"),
                    Nome = reader.GetString("nome"),
                    IdCategoria = reader.GetInt32("id_categoria"),
                    Valor = reader.GetDecimal("valor"),
                    Disponivel = reader.GetBoolean("disponivel"),
                    pathImg = reader.GetString("pathImg")
                });
            }

            return produtos;
        }

        // GET BY ID
        public Produto GetById(int id)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = "SELECT * FROM produtos WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new Produto
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                IdCategoria = reader.GetInt32("id_categoria"),
                Valor = reader.GetDecimal("valor"),
                Disponivel = reader.GetBoolean("disponivel"),
                pathImg = reader.GetString("pathImg")
            };
        }

        // ADD
        public bool Add(Produto produto)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = @"INSERT INTO produtos
                           (nome, id_categoria, valor, disponivel, pathImg)
                           VALUES (@nome, @id_categoria, @valor, @disponivel, @pathImg)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@id_categoria", produto.IdCategoria);
            cmd.Parameters.AddWithValue("@valor", produto.Valor);
            cmd.Parameters.AddWithValue("@disponivel", produto.Disponivel);
            cmd.Parameters.AddWithValue("@pathImg", produto.pathImg);

            return cmd.ExecuteNonQuery() > 0;
        }

        // UPDATE
        public bool Update(Produto produto)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = @"UPDATE produtos
                           SET nome = @nome,
                               id_categoria = @id_categoria,
                               valor = @valor,
                               disponivel = @disponivel
                           WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@id_categoria", produto.IdCategoria);
            cmd.Parameters.AddWithValue("@valor", produto.Valor);
            cmd.Parameters.AddWithValue("@disponivel", produto.Disponivel);
            cmd.Parameters.AddWithValue("@id", produto.Id);
            cmd.Parameters.AddWithValue("@pathImg", produto.pathImg);

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = "DELETE FROM produtos WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
