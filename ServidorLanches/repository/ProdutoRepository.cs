using MySql.Data.MySqlClient;
using ServidorLanches.model;

namespace ServidorLanches.repository
{
    public class ProdutoRepository
    {
        private readonly DbConnectionManager _dbManager;
        public ProdutoRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;


        // GET ALL
        public List<Produto> GetAll()
        {
            using var conn = new MySqlConnection(GetConnectionString());
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
                    PrecoUnitario = reader.GetDecimal("preco_unitario"), 
                    Disponivel = reader.GetBoolean("disponivel"),
                    pathImg = reader.GetString("pathImg")
                });
            }

            return produtos;
        }
        public List<Produto> GetAllAtivos()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            List<Produto> produtos = new();

            string sql = "SELECT * FROM produtos where disponivel = 1";
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
                    PrecoUnitario = reader.GetDecimal("preco_unitario"),
                    Disponivel = reader.GetBoolean("disponivel"),
                    pathImg = reader.GetString("pathImg")
                });
            }

            return produtos;
        }

        // GET BY ID
        public Produto GetById(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
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
                PrecoUnitario = reader.GetDecimal("preco_unitario"),
                Disponivel = reader.GetBoolean("disponivel"),
                pathImg = reader.GetString("pathImg")
            };
        }

        // ADD
        public bool Add(Produto produto)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            // Iniciamos uma transação para garantir a integridade dos dados
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Inserir o Produto
                string sqlProduto = @"INSERT INTO produtos 
                             (nome, id_categoria, valor, preco_unitario, disponivel, pathImg) 
                             VALUES (@nome, @id_categoria, @valor, @precoUnitario, @disponivel, @pathImg);
                             SELECT LAST_INSERT_ID();"; // Pega o ID gerado

                using var cmdProduto = new MySqlCommand(sqlProduto, conn, transaction);
                cmdProduto.Parameters.AddWithValue("@nome", produto.Nome);
                cmdProduto.Parameters.AddWithValue("@id_categoria", produto.IdCategoria);
                cmdProduto.Parameters.AddWithValue("@valor", produto.Valor);
                cmdProduto.Parameters.AddWithValue("@precoUnitario", produto.PrecoUnitario);
                cmdProduto.Parameters.AddWithValue("@disponivel", produto.Disponivel);
                cmdProduto.Parameters.AddWithValue("@pathImg", produto.pathImg);

                // Executa e recupera o ID do produto recém criado
                int idProdutoGerado = Convert.ToInt32(cmdProduto.ExecuteScalar());

                // 2. Inserir na tabela de Estoque
                string sqlEstoque = @"INSERT INTO estoque (id_produto, quantidade, nome_produto, ultima_atualizacao) 
                             VALUES (@idProduto, @quantidade, @nomeproduto, NOW())";

                using var cmdEstoque = new MySqlCommand(sqlEstoque, conn, transaction);
                cmdEstoque.Parameters.AddWithValue("@idProduto", idProdutoGerado);
                cmdEstoque.Parameters.AddWithValue("@quantidade", produto.quantidade);
                cmdEstoque.Parameters.AddWithValue("@nomeproduto", produto.Nome);

                cmdEstoque.ExecuteNonQuery();

                // Se chegou até aqui sem erros, confirma as duas inserções
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Se der qualquer erro (ex: falta de conexão), desfaz tudo o que foi feito
                transaction.Rollback();
                throw new Exception("Erro ao cadastrar produto e estoque: " + ex.Message);
            }
        }

        // UPDATE
        public bool Update(Produto produto)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"UPDATE produtos
                           SET nome = @nome,
                               id_categoria = @id_categoria,
                               valor = @valor,      
                               pathImg = @pathImg,                            
                               preco_unitario = @precoUnitario, 
                               disponivel = @disponivel
                           WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@id_categoria", produto.IdCategoria);
            cmd.Parameters.AddWithValue("@valor", produto.Valor);
            cmd.Parameters.AddWithValue("@precoUnitario", produto.PrecoUnitario);
            cmd.Parameters.AddWithValue("@disponivel", produto.Disponivel);
            cmd.Parameters.AddWithValue("@id", produto.Id);
            cmd.Parameters.AddWithValue("@pathImg", produto.pathImg);

            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE
        public bool Delete(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "update produtos set disponivel = 0 where id = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
