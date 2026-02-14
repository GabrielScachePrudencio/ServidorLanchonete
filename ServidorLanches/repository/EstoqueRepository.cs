using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using System.Data;
using System.Security.Cryptography;

namespace ServidorLanches.repository
{
    public class EstoqueRepository
    {
        private readonly DbConnectionManager _dbManager;

        public EstoqueRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;




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
                    me.id_consignacao,
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
        public bool TemEstoqueDisponivel(int idProduto, int qtdNecessaria, MySqlConnection conn, MySqlTransaction trans)
        {
            string sql = "SELECT quantidade FROM estoque WHERE id_produto = @id FOR UPDATE";
            using var cmd = new MySqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@id", idProduto);

            var result = cmd.ExecuteScalar();
            if (result == null) return false;

            return Convert.ToInt32(result) >= qtdNecessaria;
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
                idConsignacao = reader.IsDBNull("id_consignacao")
                ? (int?)null
                : reader.GetInt32("id_consignacao"),
                valorOrigem = ((OrigemMovimentacaoEstoque)reader.GetInt32("origem")).ToString(),

                Tipo = Enum.Parse<TipoMovimentacaoEstoque>(
                    reader.GetString("tipo")
                ),

                Origem = (OrigemMovimentacaoEstoque)reader.GetInt32("origem"),


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
















        public string MovimentarEstoque(PedidoDTO pedido, MySqlConnection conn, MySqlTransaction transaction)
        {
            if (pedido.TipoMovimentacao == TipoMovimentacaoEstoque.NENHUMA)
                return "";

            bool isSaida = pedido.TipoMovimentacao == TipoMovimentacaoEstoque.SAIDA;

            foreach (var item in pedido.Itens)
            {
                string sqlCheck = "SELECT quantidade FROM estoque WHERE id_produto = @idProduto FOR UPDATE";
                using var cmdCheck = new MySqlCommand(sqlCheck, conn, transaction);
                cmdCheck.Parameters.AddWithValue("@idProduto", item.IdProduto);

                var result = cmdCheck.ExecuteScalar();
                if (result == null)
                    return $"Produto {item.IdProduto} não existe no estoque";

                int estoqueAtual = Convert.ToInt32(result);

                if (isSaida && estoqueAtual < item.Quantidade)
                    return $"Estoque insuficiente para {item.NomeProduto}";

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
            return "ok";
        }



        public string MovimentarEstoque(
                Consignacao consignacao,
                MySqlConnection conn,
                MySqlTransaction transaction)
        {
            if (consignacao.Itens == null || !consignacao.Itens.Any())
                return "Consignação sem itens para movimentar.";

            foreach (var item in consignacao.Itens)
            {
                // 🔒 Lock do estoque
                string sqlCheck = "SELECT quantidade FROM estoque WHERE id_produto = @idProduto FOR UPDATE";

                using var cmdCheck = new MySqlCommand(sqlCheck, conn, transaction);
                cmdCheck.Parameters.AddWithValue("@idProduto", item.IdProduto);

                var result = cmdCheck.ExecuteScalar();
                if (result == null)
                    return $"Produto {item.IdProduto} não existe no estoque.";

                int estoqueAtual = Convert.ToInt32(result);

                bool isSaida = false;
                bool isEntrada = false;

                // 📌 Definição baseada no status
                switch (consignacao.IdStatus)
                {
                    case 1: // Aberto → Sai do estoque
                        isSaida = true;
                        break;

                    case 3: // Cancelado → Devolve ao estoque
                        isEntrada = true;
                        break;

                    case 2: // Finalizado → não movimenta aqui
                        return "ok";
                }

                if (isSaida && estoqueAtual < item.QuantidadeEnviada)
                    return $"Estoque insuficiente para {item.NomeProduto}";

                string sqlUpdate;

                if (isSaida)
                {
                    sqlUpdate = "UPDATE estoque SET quantidade = quantidade - @qtd WHERE id_produto = @idp";
                }
                else
                {
                    sqlUpdate = "UPDATE estoque SET quantidade = quantidade + @qtd WHERE id_produto = @idp";
                }

                using (var cmdUpdate = new MySqlCommand(sqlUpdate, conn, transaction))
                {
                    cmdUpdate.Parameters.AddWithValue("@idp", item.IdProduto);
                    cmdUpdate.Parameters.AddWithValue("@qtd", item.QuantidadeEnviada);
                    cmdUpdate.ExecuteNonQuery();
                }

                // 🧾 Registrar movimentação
                string sqlMov = @"
            INSERT INTO movimentacao_estoque 
            (id_produto, tipo, quantidade, origem, id_consignacao, id_usuario, data_movimentacao)
            VALUES 
            (@idp, @tipo, @qtd, @origem, @idCong, @idUser, NOW());
        ";

                using var cmdMov = new MySqlCommand(sqlMov, conn, transaction);

                cmdMov.Parameters.AddWithValue("@idp", item.IdProduto);
                cmdMov.Parameters.AddWithValue("@tipo",
                    isSaida
                        ? (int)TipoMovimentacaoEstoque.SAIDA
                        : (int)TipoMovimentacaoEstoque.ENTRADA);

                cmdMov.Parameters.AddWithValue("@qtd", item.QuantidadeEnviada);

                cmdMov.Parameters.AddWithValue("@origem",
                    isSaida
                        ? (int)OrigemMovimentacaoEstoque.CONSIGNACAO_ABERTA
                        : (int)OrigemMovimentacaoEstoque.CONSIGNACAO_CANCELADA);

                cmdMov.Parameters.AddWithValue("@idCong", consignacao.Id);
                cmdMov.Parameters.AddWithValue("@idUser", consignacao.IdUsuario);

                cmdMov.ExecuteNonQuery();
            }

            return "ok";
        }



        public bool AumentarEstoque(
    int idProduto,
    int quantidade,
    int? idConsignacao,
    OrigemMovimentacaoEstoque origem,
    string valorOrigem,
    MySqlConnection conn,
    MySqlTransaction transaction)
        {
            // 🔒 Bloqueia linha e pega quantidade atual
            string sqlSelect = @"
        SELECT quantidade
        FROM estoque
        WHERE id_produto = @idProduto
        FOR UPDATE";

            using var cmdSelect = new MySqlCommand(sqlSelect, conn, transaction);
            cmdSelect.Parameters.AddWithValue("@idProduto", idProduto);

            var result = cmdSelect.ExecuteScalar();
            if (result == null)
                throw new Exception($"Produto {idProduto} não encontrado.");

            int quantidadeAntes = Convert.ToInt32(result);
            int quantidadeDepois = quantidadeAntes + quantidade;

            // 📦 Atualiza estoque
            string sqlUpdate = @"
        UPDATE estoque
        SET quantidade = @novaQtd
        WHERE id_produto = @idProduto";

            using var cmdUpdate = new MySqlCommand(sqlUpdate, conn, transaction);
            cmdUpdate.Parameters.AddWithValue("@novaQtd", quantidadeDepois);
            cmdUpdate.Parameters.AddWithValue("@idProduto", idProduto);
            cmdUpdate.ExecuteNonQuery();

            // 📝 Registra movimentação
            string sqlMov = @"
        INSERT INTO movimentacao_estoque
        (id_produto, tipo, quantidade, origem,  
         id_consignacao, data_movimentacao)
        VALUES
        (@idp, @tipo, @qtd, @origem, 
          @idConsignacao, NOW())";

            using var cmdMov = new MySqlCommand(sqlMov, conn, transaction);
            cmdMov.Parameters.AddWithValue("@idp", idProduto);
            cmdMov.Parameters.AddWithValue("@tipo", (int)TipoMovimentacaoEstoque.ENTRADA);
            cmdMov.Parameters.AddWithValue("@qtd", quantidade);
            cmdMov.Parameters.AddWithValue("@origem", (int)origem);
            cmdMov.Parameters.AddWithValue("@idConsignacao", idConsignacao);
            cmdMov.ExecuteNonQuery();

            return true;
        }

        public bool DiminuirEstoque(
     int idProduto,
     int quantidade,
     int? idConsignacao,
     OrigemMovimentacaoEstoque origem,
     string valorOrigem,
     MySqlConnection conn,
     MySqlTransaction transaction)
        {
            string sqlSelect = @"
        SELECT quantidade
        FROM estoque
        WHERE id_produto = @idProduto
        FOR UPDATE";

            using var cmdSelect = new MySqlCommand(sqlSelect, conn, transaction);
            cmdSelect.Parameters.AddWithValue("@idProduto", idProduto);

            var result = cmdSelect.ExecuteScalar();
            if (result == null)
                throw new Exception($"Produto {idProduto} não encontrado.");

            int quantidadeAntes = Convert.ToInt32(result);

            if (quantidadeAntes < quantidade)
                throw new Exception("Estoque insuficiente.");

            int quantidadeDepois = quantidadeAntes - quantidade;

            string sqlUpdate = @"
        UPDATE estoque
        SET quantidade = @novaQtd
        WHERE id_produto = @idProduto";

            using var cmdUpdate = new MySqlCommand(sqlUpdate, conn, transaction);
            cmdUpdate.Parameters.AddWithValue("@novaQtd", quantidadeDepois);
            cmdUpdate.Parameters.AddWithValue("@idProduto", idProduto);
            cmdUpdate.ExecuteNonQuery();

            string sqlMov = @"
        INSERT INTO movimentacao_estoque
        (id_produto, tipo, quantidade, origem, 
            id_consignacao, data_movimentacao)
        VALUES
        (@idp, @tipo, @qtd, @origem,
         @idConsignacao, NOW())";

            using var cmdMov = new MySqlCommand(sqlMov, conn, transaction);
            cmdMov.Parameters.AddWithValue("@idp", idProduto);
            cmdMov.Parameters.AddWithValue("@tipo", (int)TipoMovimentacaoEstoque.SAIDA);
            cmdMov.Parameters.AddWithValue("@qtd", quantidade);
            cmdMov.Parameters.AddWithValue("@origem", (int)origem);
            cmdMov.Parameters.AddWithValue("@idConsignacao", idConsignacao);
            cmdMov.ExecuteNonQuery();

            return true;
        }


        public List<Estoque> GetAllEstoques()
            {
                var lista = new List<Estoque>();

                using var conn = new MySqlConnection(GetConnectionString());
                conn.Open();

                var sql = @"SELECT * FROM estoque;";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Estoque
                    {
                        Id = reader.GetInt32("id"),
                        IdProduto = reader.GetInt32("id_produto"),
                        Quantidade = reader.GetInt32("quantidade"),
                        UltimaAtualizacao = reader.GetDateTime("ultima_atualizacao"),
                        NomeProduto = reader.GetString("nome_produto")
                    });
                }

                return lista;
        }

    }
}
