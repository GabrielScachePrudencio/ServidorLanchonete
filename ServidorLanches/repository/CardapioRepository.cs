using MySql.Data.MySqlClient;
using ServidorLanches.model;

namespace ServidorLanches.repository
{
    public class CardapioRepository
    {
        private readonly IConfiguration _config;

        public CardapioRepository(IConfiguration config)
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
        public List<Cardapio> GetAll()
        {
            using var conn = GetConnection();
            conn.Open();

            List<Cardapio> cardapios = new();

            string sql = "SELECT * FROM cardapio";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cardapios.Add(new Cardapio
                {
                    Id = reader.GetInt32("id"),
                    Nome = reader.GetString("nome"),
                    Categoria = reader.GetString("categoria"),
                    Valor = reader.GetDecimal("valor"),
                    Disponivel = reader.GetBoolean("disponivel")
                });
            }

            return cardapios;
        }

        // GET BY ID
        public Cardapio GetById(int id)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = "SELECT * FROM cardapio WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new Cardapio
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                Categoria = reader.GetString("categoria"),
                Valor = reader.GetDecimal("valor"),
                Disponivel = reader.GetBoolean("disponivel")
            };
        }

        // ADD
        public bool Add(Cardapio cardapio)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = @"INSERT INTO cardapio
                           (nome, categoria, valor, disponivel)
                           VALUES (@nome, @categoria, @valor, @disponivel)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", cardapio.Nome);
            cmd.Parameters.AddWithValue("@categoria", cardapio.Categoria);
            cmd.Parameters.AddWithValue("@valor", cardapio.Valor);
            cmd.Parameters.AddWithValue("@disponivel", cardapio.Disponivel);

            return cmd.ExecuteNonQuery() > 0;
        }

        // UPDATE
        public bool Update(Cardapio cardapio)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = @"UPDATE cardapio
                           SET nome = @nome,
                               categoria = @categoria,
                               valor = @valor,
                               disponivel = @disponivel
                           WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", cardapio.Nome);
            cmd.Parameters.AddWithValue("@categoria", cardapio.Categoria);
            cmd.Parameters.AddWithValue("@valor", cardapio.Valor);
            cmd.Parameters.AddWithValue("@disponivel", cardapio.Disponivel);
            cmd.Parameters.AddWithValue("@id", cardapio.Id);

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = "DELETE FROM cardapio WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
