using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using System.Data;

namespace ServidorLanches.repository
{
    public class EstoqueRepository
    {
        private readonly IConfiguration _config;

        public EstoqueRepository(IConfiguration config)
        {
            _config = config;
        }

        private string GetConnectionString() =>
            _config.GetConnectionString("MySql");



        // 🔹 GET ALL (DTO)
        public List<MovimentacaoEstoqueDTO> GetAll()
        {
            var lista = new List<MovimentacaoEstoqueDTO>();

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var sql = @"
                SELECT
                    me.id,
                    p.nome AS produto,
                    me.id_produto,
                    u.nome AS usuario,
                    me.tipo,
                    me.origem,
                    me.quantidade AS quantidade_movimentada,

                    -- quantidade antes
                    CASE
                        WHEN me.tipo = 'ENTRADA' THEN e.quantidade - me.quantidade
                        ELSE e.quantidade + me.quantidade
                    END AS quantidade_antes,

                    -- quantidade depois
                    e.quantidade AS quantidade_depois,

                    me.id_pedido,
                    me.observacao,
                    me.data_movimentacao
                FROM movimentacao_estoque me
                INNER JOIN produtos p ON p.id = me.id_produto
                INNER JOIN estoque e ON e.id_produto = me.id_produto
                LEFT JOIN usuarios u ON u.id = me.id_usuario
                ORDER BY me.data_movimentacao DESC;

            ";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(MapDTO(reader));
            }

            return lista;
        }

        // 🔹 GET por ID
        public MovimentacaoEstoqueDTO GetById(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var sql = @"
                SELECT
                    me.id,
                    p.nome AS produto,
                    me.id_produto,
                    u.nome AS usuario,
                    me.tipo,
                    me.origem,
                    me.quantidade AS quantidade_movimentada,

                    CASE
                        WHEN me.tipo = 'ENTRADA' THEN e.quantidade - me.quantidade
                        ELSE e.quantidade + me.quantidade
                    END AS quantidade_antes,

                    e.quantidade AS quantidade_depois,

                    me.id_pedido,
                    me.observacao,
                    me.data_movimentacao
                FROM movimentacao_estoque me
                INNER JOIN produtos p ON p.id = me.id_produto
                INNER JOIN estoque e ON e.id_produto = me.id_produto
                LEFT JOIN usuarios u ON u.id = me.id_usuario
                WHERE me.id = @id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
                return MapDTO(reader);

            return null;
        }


        // 🔹 DELETE
        public bool Delete(int id)
        {
            try
            {
                using var conn = new MySqlConnection(GetConnectionString());
                conn.Open();

                var sql = "DELETE FROM movimentacao_estoque WHERE id = @id";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch(Exception ex)
            {
                throw new Exception("Erro ao deletar movimentação de estoque: " + ex.Message);
            }
        }


        private MovimentacaoEstoqueDTO MapDTO(MySqlDataReader reader)
        {
            return new MovimentacaoEstoqueDTO
            {
                Id = reader.GetInt32("id"),
                Produto = reader.GetString("produto"),
                idProduto = reader.GetInt32("id_produto").ToString(),
                Usuario = reader.IsDBNull("usuario") ? "Sistema" : reader.GetString("usuario"),

                Tipo = Enum.Parse<TipoMovimentacaoEstoque>(
                    reader.GetString("tipo")
                ),

                Origem = Enum.Parse<OrigemMovimentacaoEstoque>(
                    reader.GetString("origem")
                ),

                QuantidadeMovimentada = reader.GetInt32("quantidade_movimentada"),
                QuantidadeAntes = reader.GetInt32("quantidade_antes"),
                QuantidadeDepois = reader.GetInt32("quantidade_depois"),

                PedidoId = reader.IsDBNull("id_pedido")
                    ? null
                    : reader.GetInt32("id_pedido"),

                Observacao = reader.IsDBNull("observacao")
                    ? null
                    : reader.GetString("observacao"),

                DataMovimentacao = reader.GetDateTime("data_movimentacao")
            };
        }


















        public void MovimentarEstoque(PedidoDTO pedido, MySqlConnection conn, MySqlTransaction transaction)
        {
            if (pedido.TipoMovimentacao == TipoMovimentacaoEstoque.NENHUMA)
                return;

            bool isSaida = pedido.TipoMovimentacao == TipoMovimentacaoEstoque.SAIDA;

            foreach (var item in pedido.Itens)
            {
                string sqlCheck = "SELECT quantidade FROM estoque WHERE id_produto = @idProduto FOR UPDATE";
                using var cmdCheck = new MySqlCommand(sqlCheck, conn, transaction);
                cmdCheck.Parameters.AddWithValue("@idProduto", item.IdProduto);

                var result = cmdCheck.ExecuteScalar();
                if (result == null)
                    throw new Exception($"Produto {item.IdProduto} não existe no estoque");

                int estoqueAtual = Convert.ToInt32(result);

                if (isSaida && estoqueAtual < item.Quantidade)
                    throw new Exception($"Estoque insuficiente para {item.NomeProduto}");

                string sqlUpdate = isSaida
                    ? "UPDATE estoque SET quantidade = quantidade - @qtd WHERE id_produto = @idp"
                    : "UPDATE estoque SET quantidade = quantidade + @qtd WHERE id_produto = @idp";

                using var cmdUpdate = new MySqlCommand(sqlUpdate, conn, transaction);
                cmdUpdate.Parameters.AddWithValue("@idp", item.IdProduto);
                cmdUpdate.Parameters.AddWithValue("@qtd", item.Quantidade);
                cmdUpdate.ExecuteNonQuery();

                string sqlMov = @"
                    INSERT INTO movimentacao_estoque 
                    (id_produto, tipo, quantidade, origem, id_pedido, id_usuario, data_movimentacao)
                    VALUES 
                    (@idp, @tipo, @qtd, @origem, @idPed, @idUser, NOW());
                ";

                using var cmdMov = new MySqlCommand(sqlMov, conn, transaction);
                cmdMov.Parameters.AddWithValue("@idp", item.IdProduto);
                cmdMov.Parameters.AddWithValue("@tipo", (int)pedido.TipoMovimentacao);
                cmdMov.Parameters.AddWithValue("@qtd", item.Quantidade);
                cmdMov.Parameters.AddWithValue("@origem", (int)pedido.OrigemMovimentacaoEstoque);
                cmdMov.Parameters.AddWithValue("@idPed", pedido.Id);
                cmdMov.Parameters.AddWithValue("@idUser", pedido.IdUsuario);
                cmdMov.ExecuteNonQuery();
            }
        }
    }
}
