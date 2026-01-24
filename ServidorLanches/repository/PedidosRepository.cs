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

            // Ajustado para usar os nomes reais: id_status, produtos, id_categoria
            string sql = @"
        SELECT
            p.id                AS PedidoId,
            p.id_usuario        AS IdUsuario,
            u.nome              AS NomeUsuario, 
            p.cpf_cliente       AS CpfCliente,
            sp.id               AS IdStatus,
            sp.nome             AS StatusPedidoNome,
            p.valor_total       AS ValorTotal,
            p.data_criacao      AS DataCriacao,
            i.quantidade        AS Quantidade,
            i.preco_unitario    AS ValorUnitario,
            pr.id               AS IdProduto,
            pr.nome             AS NomeProduto,
            cp.nome             AS CategoriaNome,
            pr.pathImg          AS PathProdutoImg
        FROM pedidos p
        JOIN usuarios u            ON u.id = p.id_usuario
        JOIN statuspedido sp       ON sp.id = p.id_status
        JOIN itens_pedido i        ON i.id_pedido = p.id
        JOIN produtos pr           ON pr.id = i.id_produto
        JOIN categoriaProduto cp   ON cp.id = pr.id_categoria
        ORDER BY p.id;";

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
                        CpfCliente = reader.IsDBNull(reader.GetOrdinal("CpfCliente")) ? "" : reader.GetString("CpfCliente"),
                        IdStatus = reader.GetInt32("IdStatus"),
                        StatusPedido = reader.GetString("StatusPedidoNome"),
                        ValorTotal = reader.GetDecimal("ValorTotal"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        Itens = new List<ItemPedidoCardapioDTO>()
                    };
                }

                pedidos[pedidoId].Itens.Add(new ItemPedidoCardapioDTO
                {
                    IdProduto = reader.GetInt32("IdProduto"),
                    NomeProduto = reader.GetString("NomeProduto"),
                    Categoria = reader.GetString("CategoriaNome"),
                    pathProdutoImg = reader.IsDBNull(reader.GetOrdinal("PathProdutoImg")) ? "" : reader.GetString("PathProdutoImg"),
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
                    sp.id               AS IdStatus,
		            sp.nome             AS StatusPedidoNome,
                    p.valor_total       AS ValorTotal,
                    p.data_criacao      AS DataCriacao,
                    i.quantidade        AS Quantidade,
                    i.preco_unitario    AS ValorUnitario,
                    pr.id                AS IdProduto,
                    pr.nome              AS NomeProduto,
                    cp.nome             AS CategoriaNome,
                    pr.pathImg           AS PathProdutoImg
                FROM pedidos p
                JOIN statuspedido sp       ON sp.id = p.id_status
                JOIN itens_pedido i        ON i.id_pedido = p.id
                JOIN produtos pr           ON pr.id = i.id_produto
                JOIN categoriaProduto cp   ON cp.id = pr.id_categoria
                where p.id = @id
                ORDER BY p.id;
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
                CpfCliente = reader.IsDBNull(reader.GetOrdinal("CpfCliente")) ? "" : reader.GetString("CpfCliente"),
                IdStatus = reader.GetInt32("IdStatus"),
                StatusPedido = reader.GetString("StatusPedidoNome"),
                ValorTotal = reader.GetDecimal("ValorTotal"),
                DataCriacao = reader.GetDateTime("DataCriacao"),
                Itens = new List<ItemPedidoCardapioDTO>()
            };

            do
            {
                pedido.Itens.Add(new ItemPedidoCardapioDTO
                {
                    IdProduto = reader.GetInt32("IdProduto"),
                    NomeProduto = reader.GetString("NomeProduto"),
                    Categoria = reader.GetString("CategoriaNome"),
                    pathProdutoImg = reader.IsDBNull(reader.GetOrdinal("PathProdutoImg")) ? "" : reader.GetString("PathProdutoImg"),
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
                    INSERT INTO pedidos (id_usuario, cpf_cliente, id_status, valor_total)
                    VALUES (@id_usuario, @cpf_cliente, @id_status, @valor_total);
                    SELECT LAST_INSERT_ID();
                ";


                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
                cmdPedido.Parameters.AddWithValue("@id_status", pedido.IdStatus);
                cmdPedido.Parameters.AddWithValue("@id_usuario", pedido.IdUsuario);
                cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente);
                cmdPedido.Parameters.AddWithValue("@status_pedido", pedido.StatusPedido);
                cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);

                int pedidoId = Convert.ToInt32(cmdPedido.ExecuteScalar());

                foreach (var item in pedido.Itens)
                {
                    string sqlItem = @"
                        INSERT INTO itens_pedido
                            (id_pedido, id_produto, quantidade, preco_unitario)
                        VALUES
                            (@id_pedido, @id_produto, @quantidade, @preco_unitario);
                    ";
                    using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);
                    cmdItem.Parameters.AddWithValue("@id_pedido", pedidoId);
                    cmdItem.Parameters.AddWithValue("@id_produto", item.IdProduto);
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
            if (pedido == null || pedido.Id <= 0 || pedido.Itens == null)
                return false;

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                string sqlPedido = @"
            UPDATE pedidos
            SET 
                cpf_cliente = @cpf_cliente,
                valor_total = @valor_total,
                id_status = @id_status
            WHERE id = @id;
        ";

                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
                cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente);
                cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);
                cmdPedido.Parameters.AddWithValue("@id_status", pedido.IdStatus);
                cmdPedido.Parameters.AddWithValue("@id", pedido.Id);
                cmdPedido.ExecuteNonQuery();

                // remove itens antigos
                string deleteItens = "DELETE FROM itens_pedido WHERE id_pedido = @id_pedido;";
                using var cmdDelete = new MySqlCommand(deleteItens, conn, transaction);
                cmdDelete.Parameters.AddWithValue("@id_pedido", pedido.Id);
                cmdDelete.ExecuteNonQuery();

                // insere itens novos
                foreach (var item in pedido.Itens)
                {
                    string sqlInsert = @"
                INSERT INTO itens_pedido
                    (id_pedido, id_produto, quantidade, preco_unitario)
                VALUES
                    (@id_pedido, @id_produto, @quantidade, @preco_unitario);
            ";

                    using var cmdItem = new MySqlCommand(sqlInsert, conn, transaction);
                    cmdItem.Parameters.AddWithValue("@id_pedido", pedido.Id);
                    cmdItem.Parameters.AddWithValue("@id_produto", item.IdProduto);
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


        public bool AtualizarStatusDoPedidoById(int id, int idStatus)
        {
            using var conn = new MySqlConnection(GetConnectionString());

            try
            {
                conn.Open();

                string sql = "UPDATE pedidos SET id_status = @id_status WHERE id = @id";
                using var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@id_status", idStatus);
                cmd.Parameters.AddWithValue("@id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }


        // ============================
        // DELETE
        // ============================
        public bool DeletePedido(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            try
            {
                string sql = "DELETE FROM pedidos WHERE id = @id";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }

    }
}
