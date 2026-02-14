using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using ServidorLanches.model.dto;
using ServidorLanches.model;
using ServidorLanches.service;

namespace ServidorLanches.repository
{
    public class PedidosRepository
    {
        private readonly DbConnectionManager _dbManager;
        public PedidosRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;


        // ============================
        // GET ALL
        // ============================
        public List<PedidoDTO> GetAllPedidos()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var pedidos = new Dictionary<int, PedidoDTO>();

            // Adicionado p.nome_cliente e join com formas_pagamento
            string sql = @"
            SELECT 
                p.id                 AS PedidoId,
                p.id_usuario         AS IdUsuario,
                u.nome               AS NomeUsuario, 
                p.cpf_cliente        AS CpfCliente,
                p.nome_cliente       AS NomeCliente,
                p.id_status          AS IdStatus,
                sp.nome              AS StatusPedidoNome,
                p.valor_total        AS ValorTotal,
                p.data_criacao       AS DataCriacao,
                p.TipoMovimentacao   as TipoMovimentacao,
                p.OrigemMovimentacaoEstoque as OrigemMovimentacaoEstoque,
                p.id_caixa           as idCaixa,
                -- Campos de Pagamento
                fp.id                AS IdFormaPagamento,
                fp.descricao         AS NomeFormaPagamento,
                -- Campos dos Itens
                i.desconto           as desconto,
                i.decontoComPorcentagem as decontoComPorcentagem,
                i.quantidade         AS Quantidade,
                i.preco_unitario     AS ValorUnitario,
                pr.id                AS IdProduto,
                pr.nome              AS NomeProduto,
                cp.nome              AS CategoriaNome,
                pr.preco_unitario    AS PrecoUnitarioProduto,
                pr.pathImg           AS PathProdutoImg
            FROM pedidos p
            JOIN usuarios u            ON u.id = p.id_usuario
            JOIN statuspedido sp       ON sp.id = p.id_status
            LEFT JOIN formas_pagamento fp   ON fp.id = p.id_forma_pagamento -- Ajuste o nome da FK se for diferente
            JOIN itens_pedido i        ON i.id_pedido = p.id
            JOIN produtos pr           ON pr.id = i.id_produto
            JOIN categoriaProduto cp   ON cp.id = pr.id_categoria
            ORDER BY p.id DESC;"; 

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int pedidoId = reader.GetInt32("PedidoId");

                if (!pedidos.ContainsKey(pedidoId))
                {

                    string tipoStr = reader.IsDBNull(reader.GetOrdinal("TipoMovimentacao")) ? "NENHUMA" : reader.GetString("TipoMovimentacao");
                    if (!Enum.TryParse<TipoMovimentacaoEstoque>(tipoStr, out var tipoResult))
                    {
                        tipoResult = TipoMovimentacaoEstoque.NENHUMA;
                    }

                    // Lógica para OrigemMovimentacao
                    string origemStr = reader.IsDBNull(reader.GetOrdinal("OrigemMovimentacaoEstoque")) ? "PRONTO" : reader.GetString("OrigemMovimentacaoEstoque");
                    if (!Enum.TryParse<OrigemMovimentacaoEstoque>(origemStr, out var origemResult))
                    {
                        origemResult = OrigemMovimentacaoEstoque.PRONTO;
                    }
                    pedidos[pedidoId] = new PedidoDTO
                    {
                        Id = pedidoId,
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        NomeUsuario = reader.GetString("NomeUsuario"),
                        CpfCliente = reader.IsDBNull(reader.GetOrdinal("CpfCliente")) ? "" : reader.GetString("CpfCliente"),
                        NomeCliente = reader.IsDBNull(reader.GetOrdinal("NomeCliente")) ? "Cliente Final" : reader.GetString("NomeCliente"),
                        IdStatus = reader.GetInt32("IdStatus"),
                        StatusPedido = reader.GetString("StatusPedidoNome"),
                        IdFormaPagamento = reader.IsDBNull(reader.GetOrdinal("IdFormaPagamento")) ? 0 : reader.GetInt32("IdFormaPagamento"),
                        FormaPagamento = reader.IsDBNull(reader.GetOrdinal("NomeFormaPagamento")) ? "N/A" : reader.GetString("NomeFormaPagamento"),
                        ValorTotal = reader.GetDecimal("ValorTotal"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        IdCaixa = reader.IsDBNull(reader.GetOrdinal("idCaixa"))
                            ? -1
                            : reader.GetInt32("idCaixa"),

                        TipoMovimentacao = tipoResult,

                        OrigemMovimentacaoEstoque = origemResult,


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
                    desconto = reader.GetDecimal("desconto"),
                    decontoComPorcentagem = reader.GetInt16("decontoComPorcentagem"),
                    ValorUnitario = reader.GetDecimal("ValorUnitario"),
                    CustoDeFabricacao = reader.GetDecimal("PrecoUnitarioProduto")
                });
            }

            return pedidos.Values.ToList();
        }
        // ============================
        // GET ALL by caixa
        // ============================
        public List<PedidoDTO> GetAllPedidosByIdCaixa(int idCaixa)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var pedidos = new Dictionary<int, PedidoDTO>();

            // Adicionado p.nome_cliente e join com formas_pagamento
            string sql = @"
            SELECT 
                p.id                 AS PedidoId,
                p.id_usuario         AS IdUsuario,
                u.nome               AS NomeUsuario, 
                p.cpf_cliente        AS CpfCliente,
                p.nome_cliente       AS NomeCliente,
                p.id_status          AS IdStatus,
                sp.nome              AS StatusPedidoNome,
                p.valor_total        AS ValorTotal,
                p.data_criacao       AS DataCriacao,
                p.TipoMovimentacao   as TipoMovimentacao,
                p.OrigemMovimentacaoEstoque as OrigemMovimentacaoEstoque,
                p.id_caixa           as idCaixa,
                -- Campos de Pagamento
                fp.id                AS IdFormaPagamento,
                fp.descricao         AS NomeFormaPagamento,
                -- Campos dos Itens
                i.quantidade         AS Quantidade,
                i.preco_unitario     AS ValorUnitario,
                i.desconto           as desconto,
                i.decontoComPorcentagem as decontoComPorcentagem,
                pr.id                AS IdProduto,
                pr.nome              AS NomeProduto,
                cp.nome              AS CategoriaNome,
                pr.preco_unitario    AS PrecoUnitarioProduto,
                pr.pathImg           AS PathProdutoImg
            FROM pedidos p
            JOIN usuarios u            ON u.id = p.id_usuario
            JOIN statuspedido sp       ON sp.id = p.id_status
            LEFT JOIN formas_pagamento fp   ON fp.id = p.id_forma_pagamento -- Ajuste o nome da FK se for diferente
            JOIN itens_pedido i        ON i.id_pedido = p.id
            JOIN produtos pr           ON pr.id = i.id_produto
            JOIN categoriaProduto cp   ON cp.id = pr.id_categoria
            WHERE p.id_caixa = @idCaixa
            ORDER BY p.id DESC;"; 

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idCaixa", idCaixa);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int pedidoId = reader.GetInt32("PedidoId");

                if (!pedidos.ContainsKey(pedidoId))
                {

                    string tipoStr = reader.IsDBNull(reader.GetOrdinal("TipoMovimentacao")) ? "NENHUMA" : reader.GetString("TipoMovimentacao");
                    if (!Enum.TryParse<TipoMovimentacaoEstoque>(tipoStr, out var tipoResult))
                    {
                        tipoResult = TipoMovimentacaoEstoque.NENHUMA;
                    }

                    // Lógica para OrigemMovimentacao
                    string origemStr = reader.IsDBNull(reader.GetOrdinal("OrigemMovimentacaoEstoque")) ? "PRONTO" : reader.GetString("OrigemMovimentacaoEstoque");
                    if (!Enum.TryParse<OrigemMovimentacaoEstoque>(origemStr, out var origemResult))
                    {
                        origemResult = OrigemMovimentacaoEstoque.PRONTO;
                    }
                    pedidos[pedidoId] = new PedidoDTO
                    {
                        Id = pedidoId,
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        NomeUsuario = reader.GetString("NomeUsuario"),
                        CpfCliente = reader.IsDBNull(reader.GetOrdinal("CpfCliente")) ? "" : reader.GetString("CpfCliente"),
                        NomeCliente = reader.IsDBNull(reader.GetOrdinal("NomeCliente")) ? "Cliente Final" : reader.GetString("NomeCliente"),
                        IdStatus = reader.GetInt32("IdStatus"),
                        StatusPedido = reader.GetString("StatusPedidoNome"),
                        IdFormaPagamento = reader.IsDBNull(reader.GetOrdinal("IdFormaPagamento")) ? 0 : reader.GetInt32("IdFormaPagamento"),
                        FormaPagamento = reader.IsDBNull(reader.GetOrdinal("NomeFormaPagamento")) ? "N/A" : reader.GetString("NomeFormaPagamento"),
                        ValorTotal = reader.GetDecimal("ValorTotal"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        IdCaixa = reader.GetOrdinal("idCaixa"),
                        TipoMovimentacao = tipoResult,

                        OrigemMovimentacaoEstoque = origemResult,


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
                    ValorUnitario = reader.GetDecimal("ValorUnitario"),
                    CustoDeFabricacao = reader.GetDecimal("PrecoUnitarioProduto"),
                    desconto = reader.GetDecimal("desconto"),
                    decontoComPorcentagem = reader.GetInt16("decontoComPorcentagem")

                });
            }

            return pedidos.Values.ToList();
        }
        public List<PedidoDTO> GetAllPedidosFromToday()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var pedidos = new Dictionary<int, PedidoDTO>();

            // Adicionado p.nome_cliente e join com formas_pagamento
            string sql = @"
            SELECT 
                p.id                 AS PedidoId,
                p.id_usuario         AS IdUsuario,
                u.nome               AS NomeUsuario, 
                p.cpf_cliente        AS CpfCliente,
                p.nome_cliente       AS NomeCliente,
                p.id_status          AS IdStatus,
                sp.nome              AS StatusPedidoNome,
                p.valor_total        AS ValorTotal,
                p.data_criacao       AS DataCriacao,
                p.TipoMovimentacao   as TipoMovimentacao,
                p.OrigemMovimentacaoEstoque as OrigemMovimentacaoEstoque,
                p.id_caixa           as idCaixa,                
-- Campos de Pagamento
                fp.id                AS IdFormaPagamento,
                fp.descricao         AS NomeFormaPagamento,
                
                -- Campos dos Itens
                i.quantidade         AS Quantidade,
                i.preco_unitario     AS ValorUnitario,
                i.desconto           as desconto,
                i.decontoComPorcentagem as decontoComPorcentagem,
                pr.id                AS IdProduto,
                pr.nome              AS NomeProduto,
                cp.nome              AS CategoriaNome,
                pr.pathImg           AS PathProdutoImg,
                pr.preco_unitario    AS PrecoUnitarioProduto    
            FROM pedidos p
            JOIN usuarios u            ON u.id = p.id_usuario
            JOIN statuspedido sp       ON sp.id = p.id_status
            LEFT JOIN formas_pagamento fp   ON fp.id = p.id_forma_pagamento -- Ajuste o nome da FK se for diferente
            JOIN itens_pedido i        ON i.id_pedido = p.id
            JOIN produtos pr           ON pr.id = i.id_produto
            JOIN categoriaProduto cp   ON cp.id = pr.id_categoria
            WHERE p.data_criacao >= @inicio
            AND p.data_criacao < @fim
            ORDER BY p.id DESC;"; 

            using var cmd = new MySqlCommand(sql, conn);
            DateTime inicio = DateTime.Today;
            DateTime fim = inicio.AddDays(1);

            cmd.Parameters.AddWithValue("@inicio", inicio);
            cmd.Parameters.AddWithValue("@fim", fim);

            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                int pedidoId = reader.GetInt32("PedidoId");

                if (!pedidos.ContainsKey(pedidoId))
                {

                    string tipoStr = reader.IsDBNull(reader.GetOrdinal("TipoMovimentacao")) ? "NENHUMA" : reader.GetString("TipoMovimentacao");
                    if (!Enum.TryParse<TipoMovimentacaoEstoque>(tipoStr, out var tipoResult))
                    {
                        tipoResult = TipoMovimentacaoEstoque.NENHUMA;
                    }

                    // Lógica para OrigemMovimentacao
                    string origemStr = reader.IsDBNull(reader.GetOrdinal("OrigemMovimentacaoEstoque")) ? "PRONTO" : reader.GetString("OrigemMovimentacaoEstoque");
                    if (!Enum.TryParse<OrigemMovimentacaoEstoque>(origemStr, out var origemResult))
                    {
                        origemResult = OrigemMovimentacaoEstoque.PRONTO;
                    }
                    pedidos[pedidoId] = new PedidoDTO
                    {
                        Id = pedidoId,
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        NomeUsuario = reader.GetString("NomeUsuario"),
                        CpfCliente = reader.IsDBNull(reader.GetOrdinal("CpfCliente")) ? "" : reader.GetString("CpfCliente"),
                        NomeCliente = reader.IsDBNull(reader.GetOrdinal("NomeCliente")) ? "Cliente Final" : reader.GetString("NomeCliente"),
                        IdStatus = reader.GetInt32("IdStatus"),
                        StatusPedido = reader.GetString("StatusPedidoNome"),
                        IdFormaPagamento = reader.IsDBNull(reader.GetOrdinal("IdFormaPagamento")) ? 0 : reader.GetInt32("IdFormaPagamento"),
                        FormaPagamento = reader.IsDBNull(reader.GetOrdinal("NomeFormaPagamento")) ? "N/A" : reader.GetString("NomeFormaPagamento"),
                        ValorTotal = reader.GetDecimal("ValorTotal"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        IdCaixa = reader.GetOrdinal("idCaixa"),
                        TipoMovimentacao = tipoResult,

                        OrigemMovimentacaoEstoque = origemResult,


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
                    CustoDeFabricacao = reader.GetDecimal("PrecoUnitarioProduto"),
                    ValorUnitario = reader.GetDecimal("ValorUnitario"),
                    desconto = reader.GetDecimal("desconto"),
                    decontoComPorcentagem = reader.GetInt16("decontoComPorcentagem")
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
                    p.id                 AS PedidoId,
                    p.id_usuario         AS IdUsuario,
                    u.nome               AS NomeUsuario, 
                    p.cpf_cliente        AS CpfCliente,
                    p.nome_cliente       AS NomeCliente,
                    p.id_status          AS IdStatus,
                    sp.nome              AS StatusPedidoNome,
                    p.valor_total        AS ValorTotal,
                    p.data_criacao       AS DataCriacao,
                    p.TipoMovimentacao   as TipoMovimentacao,
                    p.OrigemMovimentacaoEstoque as OrigemMovimentacaoEstoque,
                    p.id_caixa           as idCaixa,

                    -- Campos de Pagamento
                    fp.id                AS IdFormaPagamento,
                    fp.descricao         AS NomeFormaPagamento,
                    -- Campos dos Itens
                    i.quantidade         AS Quantidade,
                    i.preco_unitario     AS ValorUnitario,
                    i.desconto           as desconto,
                    i.decontoComPorcentagem as decontoComPorcentagem,
                    pr.id                AS IdProduto,
                    pr.preco_unitario    AS PrecoUnitarioProduto,
                    pr.nome              AS NomeProduto,
                    cp.nome              AS CategoriaNome,
                    pr.pathImg           AS PathProdutoImg
                FROM pedidos p
                JOIN usuarios u            ON u.id = p.id_usuario
                JOIN statuspedido sp       ON sp.id = p.id_status
                LEFT JOIN formas_pagamento fp   ON fp.id = p.id_forma_pagamento -- Ajuste o nome da FK se for diferente
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
                NomeUsuario = reader.GetString("NomeUsuario"),
                CpfCliente = reader.IsDBNull(reader.GetOrdinal("CpfCliente")) ? "" : reader.GetString("CpfCliente"),
                NomeCliente = reader.IsDBNull(reader.GetOrdinal("NomeCliente")) ? "Cliente Final" : reader.GetString("NomeCliente"),
                IdStatus = reader.GetInt32("IdStatus"),
                StatusPedido = reader.GetString("StatusPedidoNome"),
                IdFormaPagamento = reader.IsDBNull(reader.GetOrdinal("IdFormaPagamento")) ? 0 : reader.GetInt32("IdFormaPagamento"),
                FormaPagamento = reader.IsDBNull(reader.GetOrdinal("NomeFormaPagamento")) ? "N/A" : reader.GetString("NomeFormaPagamento"),
                ValorTotal = reader.GetDecimal("ValorTotal"),
                DataCriacao = reader.GetDateTime("DataCriacao"),

                IdCaixa = reader.GetOrdinal("idCaixa"),

                TipoMovimentacao = Enum.Parse<TipoMovimentacaoEstoque>(
                    reader.GetString("TipoMovimentacao")
                ),

                OrigemMovimentacaoEstoque = Enum.Parse<OrigemMovimentacaoEstoque>(
                    reader.GetString("OrigemMovimentacaoEstoque")
                ),

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
                    CustoDeFabricacao = reader.GetDecimal("PrecoUnitarioProduto"),
                    ValorUnitario = reader.GetDecimal("ValorUnitario"),
                    desconto = reader.GetDecimal("desconto"),
                    decontoComPorcentagem = reader.GetInt16("decontoComPorcentagem")

                });
            }
            while (reader.Read());

            return pedido;
        }

        // ============================
        // POST
        // ============================
        public int AddPedido(PedidoDTO pedido, MySqlConnection conn, MySqlTransaction transaction)
        {
            string sqlPedido = @"
                INSERT INTO pedidos 
                (id_usuario, cpf_cliente, nome_cliente, id_status, id_forma_pagamento, valor_total, TipoMovimentacao, OrigemMovimentacaoEstoque, id_caixa)
                VALUES 
                (@id_usuario, @cpf_cliente, @nome_cliente, @id_status, @id_forma_pagamento, @valor_total, @TipoMovimentacao, @OrigemMovimentacaoEstoque, @idCaixa);
                SELECT LAST_INSERT_ID();
            ";

            using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
            cmdPedido.Parameters.AddWithValue("@id_usuario", pedido.IdUsuario);
            cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente ?? (object)DBNull.Value);
            cmdPedido.Parameters.AddWithValue("@nome_cliente", pedido.NomeCliente ?? (object)DBNull.Value);
            cmdPedido.Parameters.AddWithValue("@id_status", pedido.IdStatus);
            cmdPedido.Parameters.AddWithValue("@id_forma_pagamento", pedido.IdFormaPagamento);
            cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);
            cmdPedido.Parameters.AddWithValue("@TipoMovimentacao", (int)pedido.TipoMovimentacao);
            cmdPedido.Parameters.AddWithValue("@OrigemMovimentacaoEstoque", (int)pedido.OrigemMovimentacaoEstoque);
            cmdPedido.Parameters.AddWithValue("@idCaixa", (int)pedido.IdCaixa);

            int pedidoId = Convert.ToInt32(cmdPedido.ExecuteScalar());

            foreach (var item in pedido.Itens)
            {
                string sqlItem = @"
                    INSERT INTO itens_pedido 
                    (id_pedido, id_produto, quantidade, preco_unitario, desconto, decontoComPorcentagem)
                    VALUES 
                    (@id_pedido, @id_produto, @quantidade, @preco_unitario, @desconto, @decontoComPorcentagem );
                ";

                using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);
                cmdItem.Parameters.AddWithValue("@id_pedido", pedidoId);
                cmdItem.Parameters.AddWithValue("@id_produto", item.IdProduto);
                cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                cmdItem.Parameters.AddWithValue("@preco_unitario", item.ValorUnitario);
                cmdItem.Parameters.AddWithValue("@desconto", item.desconto);
                cmdItem.Parameters.AddWithValue("@decontoComPorcentagem", item.decontoComPorcentagem);
                cmdItem.ExecuteNonQuery();
            }

            return pedidoId;
        }



        // ============================
        // PUT
        // ============================
        // Versão para ser chamada dentro de uma transação externa no Service
        public void AtualizarPedido(PedidoDTO pedido, MySqlConnection conn, MySqlTransaction transaction)
        {
            // 1. Atualiza o cabeçalho do pedido
            string sqlPedido = @"
    UPDATE pedidos
    SET 
        cpf_cliente = @cpf_cliente,
        nome_cliente = @nome_cliente,
        valor_total = @valor_total,
        id_status = @id_status,
        id_forma_pagamento = @id_forma_pagamento,
        TipoMovimentacao = @TipoMovimentacao,
        OrigemMovimentacaoEstoque = @OrigemMovimentacaoEstoque
    WHERE id = @id;";

            using var cmdPedido = new MySqlCommand(sqlPedido, conn, transaction);
            cmdPedido.Parameters.AddWithValue("@cpf_cliente", pedido.CpfCliente ?? (object)DBNull.Value);
            cmdPedido.Parameters.AddWithValue("@nome_cliente", pedido.NomeCliente ?? (object)DBNull.Value);
            cmdPedido.Parameters.AddWithValue("@valor_total", pedido.ValorTotal);
            cmdPedido.Parameters.AddWithValue("@id_status", pedido.IdStatus);
            cmdPedido.Parameters.AddWithValue("@id_forma_pagamento", pedido.IdFormaPagamento);
            cmdPedido.Parameters.AddWithValue("@TipoMovimentacao", (int)pedido.TipoMovimentacao);
            cmdPedido.Parameters.AddWithValue("@OrigemMovimentacaoEstoque", (int)pedido.OrigemMovimentacaoEstoque);
            cmdPedido.Parameters.AddWithValue("@id", pedido.Id);
            cmdPedido.ExecuteNonQuery();

            // 2. Remove itens antigos
            string deleteItens = "DELETE FROM itens_pedido WHERE id_pedido = @id_pedido;";
            using var cmdDelete = new MySqlCommand(deleteItens, conn, transaction);
            cmdDelete.Parameters.AddWithValue("@id_pedido", pedido.Id);
            cmdDelete.ExecuteNonQuery();

            // 3. Insere itens atualizados
            foreach (var item in pedido.Itens)
            {
                string sqlInsert = @"
        INSERT INTO itens_pedido (id_pedido, id_produto, quantidade, preco_unitario, desconto, decontoComPorcentagem)
        VALUES (@id_pedido, @id_produto, @quantidade, @preco_unitario, @desconto, @decontoComPorcentagem);";

                using var cmdItem = new MySqlCommand(sqlInsert, conn, transaction);
                cmdItem.Parameters.AddWithValue("@id_pedido", pedido.Id);
                cmdItem.Parameters.AddWithValue("@id_produto", item.IdProduto);
                cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                cmdItem.Parameters.AddWithValue("@preco_unitario", item.ValorUnitario);
                cmdItem.Parameters.AddWithValue("@desconto", item.desconto);
                cmdItem.Parameters.AddWithValue("@decontoComPorcentagem", item.decontoComPorcentagem);
                cmdItem.ExecuteNonQuery();
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
