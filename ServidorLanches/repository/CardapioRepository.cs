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

        public List<Cardapio> getAllCardapios()
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")

            );


            conn.Open();

            List<Cardapio> cardapios = new();   

            string sql = "SELECT * FROM cardapio";
            using (var cmd = new MySqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
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
            }

            return cardapios;
        }

        
    }
}
