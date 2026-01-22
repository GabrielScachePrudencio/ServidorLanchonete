using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using ServidorLanches.model.dto;
using ServidorLanches.model;

namespace ServidorLanches.repository
{
    public class PedidosRepository
    {
        private readonly IConfiguration _config;

        public PedidosRepository(IConfiguration config)
        {
            _config = config;
        }

        private string GetConnectionString() =>
            _config.GetConnectionString("MySql");

        // ============================
        // GET ALL
        // ============================
        public List<PedidoDTO> GetAllPedidos()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var pedidos = new Dictionary<int, PedidoDTO>();

            string sql = @"
                SELECT
                    p.id                AS PedidoId,
                    p.id_usuario        AS IdUsuario,
                    p.cpf_cliente       AS CpfCliente,
                    p.status_pedido     AS StatusPedido,
                    p.valor_total       AS ValorTotal,
                    p.data_criacao      AS DataCriacao,

                    i.quantidade        AS Quantidade,
                    i.preco_unitario    AS ValorUnitario,

                    c.id                AS IdCardapio,
                    c.nome              AS NomeCardapio,
                    c.categoria         AS Categoria,
                    c.pathImg           AS pahCardapioImg
                FROM pedidos p
                JOIN itens_pedido i ON i.id_pedido = p.id
                JOIN cardapio c     ON c.id = i.id_cardapio
                ORDER BY p.id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int pedidoId = reader.GetInt32("PedidoId");

                if (!pedidos.ContainsKey(pedidoId))
                {
                    pedidos[pedidoId] = new PedidoDTO
                    {
                        Id = pedidoId,
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        CpfCliente = reader.GetString("CpfCliente"),
                        StatusPedido = reader.GetString("StatusPedido"),
                        ValorTotal = reader.GetDecimal("ValorTotal"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        Itens = new List<ItemPedidoCardapioDTO>()
                    };
                }

                pedidos[pedidoId].Itens.Add(new ItemPedidoCardapioDTO
                {
                    IdCardapio = reader.GetInt32("IdCardapio"),
                    NomeCardapio = reader.GetString("NomeCardapio"),
                    Categoria = reader.GetString("Categoria"),
                    pahCardapioImg = reader.GetString("pahCardapioImg"),
                    Quantidade = reader.GetInt32("Quantidade"),
                    ValorUnitario = reader.GetDecimal("ValorUnitario")
                });
            }

            return pedidos.Values.ToList();
        }

        // ============================
        // GET BY ID
        // ============================
        public PedidoDTO GetPedidoById(int id)
        {
            if (id <= 0) return null;

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"
                SELECT
                    p.id                AS PedidoId,
                    p.id_usuario        AS IdUsuario,
                    p.cpf_cliente       AS CpfCliente,
                    p.status_pedido     AS StatusPedido,
                    p.valor_total       AS ValorTotal,
                    p.data_criacao      AS DataCriacao,

                    i.quantidade        AS Quantidade,
                    i.preco_unitario    AS ValorUnitario,

                    c.id                AS IdCardapio,
                    c.nome              AS NomeCardapio,
                    c.categoria         AS Categoria,
                    c.pathImg           AS pahCardapioImg
                FROM pedidos p
                JOIN itens_pedido i ON i.id_pedido = p.id
                JOIN cardapio c     ON c.id = i.id_cardapio
                WHERE p.id = @id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            var pedido = new PedidoDTO
            {
                Id = reader.GetInt32("PedidoId"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                CpfCliente = reader.GetString("CpfCliente"),
                StatusPedido = reader.GetString("StatusPedido"),
                ValorTotal = reader.GetDecimal("ValorTotal"),
                DataCriacao = reader.GetDateTime("DataCriacao"),
                Itens = new List<ItemPedidoCardapioDTO>()
            };

            do
            {
                pedido.Itens.Add(new ItemPedidoCardapioDTO
                {
                    IdCardapio = reader.GetInt32("IdCardapio"),
                    NomeCardapio = reader.GetString("NomeCardapio"),
                    Categoria = reader.GetString("Categoria"),
                    pahCardapioImg = reader.GetString("pahCardapioImg"),
                    Quantidade = reader.GetInt32("Quantidade"),
                    ValorUnitario = reader.GetDecimal("ValorUnitario")
                });
            }
            while (reader.Read());

            return pedido;
        }

        // ============================
        // POST
        // ============================
        public bool AddPedido(PedidoDTO pedido)
        {
            if (pedido == null || pedido.Itens == null || !pedido.Itens.Any())
                return false;

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                string sqlPedido = @"
                    INSERT INTO pedidos (id_usuario, cpf_cliente, status_pedido, valor_total)
                    VALUES (@id_usuario, @cpf_cliente, @status_pedido, @valor_total);
                    SELECT LAST_INSERT_ID();
                ";

                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
                cmdPedido.Parameters.AddWithValue("@id_usuario", pedido.IdUsuario);
                cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente);
                cmdPedido.Parameters.AddWithValue("@status_pedido", pedido.StatusPedido);
                cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);

                int pedidoId = Convert.ToInt32(cmdPedido.ExecuteScalar());

                foreach (var item in pedido.Itens)
                {
                    string sqlItem = @"
                        INSERT INTO itens_pedido
                        (id_pedido, id_cardapio, quantidade, preco_unitario)
                        VALUES
                        (@id_pedido, @id_cardapio, @quantidade, @preco_unitario);
                    ";

                    using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);
                    cmdItem.Parameters.AddWithValue("@id_pedido", pedidoId);
                    cmdItem.Parameters.AddWithValue("@id_cardapio", item.IdCardapio);
                    cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmdItem.Parameters.AddWithValue("@preco_unitario", item.ValorUnitario);
                    cmdItem.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        // ============================
        // PUT
        // ============================
        public bool AtualizarPedido(PedidoDTO pedido)
        {
            if (pedido == null || pedido.Id <= 0)
                return false;

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Atualiza pedido
                string sqlPedido = @"
            UPDATE pedidos
            SET cpf_cliente = @cpf_cliente,
                valor_total = @valor_total,
                status_pedido = @status_pedido
            WHERE id = @id;
        ";

                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
                cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente);
                cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);
                cmdPedido.Parameters.AddWithValue("@status_pedido", pedido.StatusPedido.ToString());
                cmdPedido.Parameters.AddWithValue("@id", pedido.Id);
                cmdPedido.ExecuteNonQuery();

                // REMOVE todos os itens antigos
                string deleteItens = "DELETE FROM itens_pedido WHERE id_pedido = @id_pedido;";
                using var cmdDelete = new MySqlCommand(deleteItens, conn, transaction);
                cmdDelete.Parameters.AddWithValue("@id_pedido", pedido.Id);
                cmdDelete.ExecuteNonQuery();

                // INSERE os itens atuais
                foreach (var item in pedido.Itens)
                {
                    string sqlInsert = @"
                INSERT INTO itens_pedido (id_pedido, id_cardapio, quantidade, preco_unitario)
                VALUES (@id_pedido, @id_cardapio, @quantidade, @preco_unitario);
            ";

                    using var cmdItem = new MySqlCommand(sqlInsert, conn, transaction);
                    cmdItem.Parameters.AddWithValue("@id_pedido", pedido.Id);
                    cmdItem.Parameters.AddWithValue("@id_cardapio", item.IdCardapio);
                    cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmdItem.Parameters.AddWithValue("@preco_unitario", item.ValorUnitario);
                    cmdItem.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return false;
            }
        }


        public string AtualizarStatusDoPedidoById(int id, string status)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            try
            {
                conn.Open();
                // SQL simples: apenas o texto do status
                string sql = "UPDATE pedidos SET status_pedido = @status WHERE id = @id";
                using var cmd = new MySqlCommand(sql, conn);

                // O segredo está no .ToString() aqui embaixo!
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();


                return "ok";
            }
            catch (Exception e)
            {
                return e.Message + e.StackTrace;
            }
        }

        // ============================
        // DELETE
        // ============================
        public bool DeletePedido(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var cmdItens = new MySqlCommand(
                    "DELETE FROM itens_pedido WHERE id_pedido = @id",
                    conn, transaction
                );
                cmdItens.Parameters.AddWithValue("@id", id);
                cmdItens.ExecuteNonQuery();

                var cmdPedido = new MySqlCommand(
                    "DELETE FROM pedidos WHERE id = @id",
                    conn, transaction
                );
                cmdPedido.Parameters.AddWithValue("@id", id);

                int linhas = cmdPedido.ExecuteNonQuery();

                transaction.Commit();
                return linhas > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
