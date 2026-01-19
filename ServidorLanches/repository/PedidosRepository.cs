using ServidorLanches.model;
using MySql.Data.MySqlClient;

namespace ServidorLanches.repository
{
    public class PedidosRepository
    {
        private readonly IConfiguration _config;

        public PedidosRepository(IConfiguration config)
        {
            _config = config;
        }

        public List<Pedido> getAllPedidos()
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();

            List<Pedido> pedidos = new();

            string sqlPedidos = "SELECT * FROM pedidos";
            using (var cmd = new MySqlCommand(sqlPedidos, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    pedidos.Add(new Pedido
                    {
                        Id = reader.GetInt32("id"),
                        IdUsuario = reader.GetInt32("id_usuario"),
                        CpfCliente = reader.GetString("cpf_cliente"),
                        StatusPedido = reader.GetString("status_pedido"),
                        ValorTotal = reader.GetDecimal("valor_total"),
                        DataCriacao = reader.GetDateTime("data_criacao"),
                        DataEntrega = reader.GetDateTime("data_entrega"),
                        Itens = new List<ItemPedido>()
                    });
                }
            }

            // 2️⃣ Para cada pedido, busca os itens
            string sqlItens = "SELECT * FROM itens_pedido WHERE id_pedido = @id";

            foreach (var pedido in pedidos)
            {
                using var cmdItens = new MySqlCommand(sqlItens, conn);
                cmdItens.Parameters.AddWithValue("@id", pedido.Id);

                using var readerItens = cmdItens.ExecuteReader();
                while (readerItens.Read())
                {
                    pedido.Itens.Add(new ItemPedido
                    {
                        Id = readerItens.GetInt32("id"),
                        IdPedido = readerItens.GetInt32("id_pedido"),
                        IdCardapio = readerItens.GetInt32("id_cardapio"),
                        Quantidade = readerItens.GetInt32("quantidade"),
                        PrecoUnitario = readerItens.GetDecimal("preco_unitario")
                    });
                }
            }


            return pedidos;
        }

        public Pedido GetPedidoItensById(int id)
        {
            using var conn = new MySqlConnection(
                _config.GetConnectionString("MySql")
            );

            conn.Open();

            Pedido pedido = new Pedido();

            if(id <= 0) return null;

            // primeira busca do pedido
            string sqlPedidos = "SELECT * FROM pedidos where id = @id";


            using var cmd = new MySqlCommand(sqlPedidos, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    pedido.Id = reader.GetInt32("id");
                    pedido.IdUsuario = reader.GetInt32("id_usuario");
                    pedido.CpfCliente = reader.GetString("cpf_cliente");
                    pedido.StatusPedido = reader.GetString("status_pedido");
                    pedido.ValorTotal = reader.GetDecimal("valor_total");
                    pedido.DataCriacao = reader.GetDateTime("data_criacao");
                    pedido.DataEntrega = reader.GetDateTime("data_entrega");
                    pedido.Itens = new List<ItemPedido>();
                    
                }
            }

            // segunda busca dos itens Para cada pedido
            string sqlItens = "SELECT * FROM itens_pedido WHERE id_pedido = @id";

            
            using var cmdItens = new MySqlCommand(sqlItens, conn);
            cmdItens.Parameters.AddWithValue("@id", pedido.Id);

            using var readerItens = cmdItens.ExecuteReader();
            while (readerItens.Read())
            {
                pedido.Itens.Add(new ItemPedido
                {
                    Id = readerItens.GetInt32("id"),
                    IdPedido = readerItens.GetInt32("id_pedido"),
                    IdCardapio = readerItens.GetInt32("id_cardapio"),
                    Quantidade = readerItens.GetInt32("quantidade"),
                    PrecoUnitario = readerItens.GetDecimal("preco_unitario")
                });
            }


            if (pedido == null) return null;

            return pedido;
        }

        private List<ItemPedido> GetItensByPedidoId(int idPedido, MySqlConnection conn)
        {
            List<ItemPedido> itens = new();

            string sql = "SELECT * FROM itens_pedido WHERE id_pedido = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idPedido);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                itens.Add(new ItemPedido
                {
                    Id = reader.GetInt32("id"),
                    IdPedido = reader.GetInt32("id_pedido"),
                    IdCardapio = reader.GetInt32("id_cardapio"),
                    Quantidade = reader.GetInt32("quantidade"),
                    PrecoUnitario = reader.GetDecimal("preco_unitario")
                });
            }

            return itens;
        }


        public bool addPedidoEItemsNovo(Pedido pedido)
        {
            if (pedido == null || pedido.Itens == null || pedido.Itens.Count == 0)
                return false;

            using var conn = new MySqlConnection(_config.GetConnectionString("MySql"));
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
             
                string sqlPedido = @"
                    INSERT INTO pedidos 
                    (id_usuario, cpf_cliente, status_pedido, valor_total) 
                    VALUES (@id_usuario, @cpf_cliente, @status_pedido, @valor_total);
                    SELECT LAST_INSERT_ID();
                ";

                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
                cmdPedido.Parameters.AddWithValue("@id_usuario", pedido.IdUsuario);
                cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente);
                cmdPedido.Parameters.AddWithValue("@status_pedido", pedido.StatusPedido);
                cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);

                pedido.Id = Convert.ToInt32(cmdPedido.ExecuteScalar());

                foreach (var item in pedido.Itens)
                {
                    string sqlItem = @"
                        INSERT INTO itens_pedido 
                        (id_pedido, id_cardapio, quantidade, preco_unitario) 
                        VALUES (@id_pedido, @id_cardapio, @quantidade, @preco_unitario)
                    ";

                    using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);
                    cmdItem.Parameters.AddWithValue("@id_pedido", pedido.Id);
                    cmdItem.Parameters.AddWithValue("@id_cardapio", item.IdCardapio);
                    cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmdItem.Parameters.AddWithValue("@preco_unitario", item.PrecoUnitario);

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

        public bool AtualizarPedido(Pedido pedido)
        {
            if (pedido == null || pedido.Id <= 0)
                return false;

            using var conn = new MySqlConnection(_config.GetConnectionString("MySql"));
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Atualiza os dados principais do pedido (CPF e Data de Entrega)
                string sqlPedido = @"
                    UPDATE pedidos 
                    SET cpf_cliente = @cpf_cliente, 
                        data_entrega = @data_entrega,
                        valor_total = @valor_total
                    WHERE id = @id";

                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
                cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente);
                cmdPedido.Parameters.AddWithValue("@data_entrega", pedido.DataEntrega);
                cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);
                cmdPedido.Parameters.AddWithValue("@id", pedido.Id);

                cmdPedido.ExecuteNonQuery();

                // 2. Atualiza os itens do pedido
                // Aqui assumimos que os itens já existem e estamos apenas mudando a quantidade
                foreach (var item in pedido.Itens)
                {
                    string sqlItem = @"
                        UPDATE itens_pedido 
                        SET quantidade = @quantidade
                        WHERE id_pedido = @id_pedido AND id_cardapio = @id_cardapio";

                    using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);
                    cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmdItem.Parameters.AddWithValue("@id_pedido", pedido.Id);
                    cmdItem.Parameters.AddWithValue("@id_cardapio", item.IdCardapio);

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


        public bool DeletePedido(int id)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("MySql"));
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                string sqlItens = "DELETE FROM itens_pedido WHERE id_pedido = @id";
                using var cmdItens = new MySqlCommand(sqlItens, conn, transaction);
                cmdItens.Parameters.AddWithValue("@id", id);
                cmdItens.ExecuteNonQuery();

                string sqlPedido = "DELETE FROM pedidos WHERE id = @id";
                using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
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
