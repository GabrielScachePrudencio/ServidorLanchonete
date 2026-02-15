using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.NetworkInformation;

namespace ServidorLanches.repository
{
    public class ConsignacaoRepository
    {
        private readonly DbConnectionManager _dbManager;

        public ConsignacaoRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;

        public Consignacao Salvar(
     Consignacao consignacao,
     MySqlConnection conn,
     MySqlTransaction transaction)
        {
            try
            {
                // 🔹 1 - Inserir consignação
                var sqlConsignacao = @"
            INSERT INTO consignacoes 
            (id_cliente, id_usuario, id_status, data_saida, data_previsao_acerto, valor_total_estimado, observacao) 
            VALUES 
            (@idCliente, @idUsuario, @idStatus, @dataSaida, @dataPrevisao, @total, @obs);

            SELECT LAST_INSERT_ID();
        ";

                using (var cmd = new MySqlCommand(sqlConsignacao, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@idCliente", consignacao.IdCliente);
                    cmd.Parameters.AddWithValue("@idUsuario", consignacao.IdUsuario);
                    cmd.Parameters.AddWithValue("@idStatus", consignacao.IdStatus); // agora usa o que veio
                    cmd.Parameters.AddWithValue("@dataSaida", consignacao.DataSaida);
                    cmd.Parameters.AddWithValue("@dataPrevisao",
                        (object)consignacao.DataPrevisaoAcerto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@total", consignacao.ValorTotalEstimado);
                    cmd.Parameters.AddWithValue("@obs", consignacao.Observacao ?? "");

                    consignacao.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 🔹 2 - Inserir itens
                if (consignacao.Itens != null && consignacao.Itens.Any())
                {
                    var sqlItens = @"
                INSERT INTO consignacao_itens 
                (id_consignacao, id_produto, quantidade_enviada, preco_unitario_acordado) 
                VALUES 
                (@idCons, @idProd, @qtdEnv, @preco);
            ";

                    foreach (var item in consignacao.Itens)
                    {
                        using var cmdItem = new MySqlCommand(sqlItens, conn, transaction);

                        cmdItem.Parameters.AddWithValue("@idCons", consignacao.Id);
                        cmdItem.Parameters.AddWithValue("@idProd", item.IdProduto);
                        cmdItem.Parameters.AddWithValue("@qtdEnv", item.QuantidadeEnviada);
                        cmdItem.Parameters.AddWithValue("@preco", item.PrecoUnitarioAcordado);

                        cmdItem.ExecuteNonQuery();
                    }
                }

                return consignacao;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar consignação: {ex.Message}");
            }
        }




        public List<Consignacao> GetAll()

        {

            var lista = new List<Consignacao>();

            using var conn = new MySqlConnection(GetConnectionString());

            conn.Open();



            var sql = @"SELECT c.*, cli.nome as NomeCliente 

                 FROM consignacoes c 

                 INNER JOIN clientes cli ON c.id_cliente = cli.id 

                 ORDER BY c.id DESC";



            using var cmd = new MySqlCommand(sql, conn);

            using var reader = cmd.ExecuteReader();



            while (reader.Read())

            {
                int idStatus = reader.GetInt32("id_status");


                lista.Add(new Consignacao

                {

                    Id = reader.GetInt32("id"),

                    IdCliente = reader.GetInt32("id_cliente"),

                    NomeCliente = reader.GetString("NomeCliente"),
                    DataPrevisaoAcerto = reader.IsDBNull(reader.GetOrdinal("data_previsao_acerto"))
                         ? (DateTime?)null
                         : reader.GetDateTime("data_previsao_acerto"),
                    IdUsuario = reader.GetInt32("id_usuario"),

                    IdStatus = idStatus,
                    NomeStatus = idStatus switch
                    {
                        1 => "Em Aberto",
                        2 => "Finalizado",
                        3 => "Cancelado",
                        _ => "Desconhecido"
                    },


                    DataSaida = reader.GetDateTime("data_saida"),

                    ValorTotalEstimado = reader.GetDecimal("valor_total_estimado"),

                    Observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? "" : reader.GetString("observacao")

                });

            }

            reader.Close();



            foreach (var c in lista)

            {

                var sqlItens = @"SELECT i.*, p.nome as NomeProduto 

                          FROM consignacao_itens i 

                          INNER JOIN produtos p ON i.id_produto = p.id 

                          WHERE i.id_consignacao = @id";



                using var cmdItens = new MySqlCommand(sqlItens, conn);

                cmdItens.Parameters.AddWithValue("@id", c.Id);

                using var readerItens = cmdItens.ExecuteReader();



                while (readerItens.Read())

                {

                    c.Itens.Add(new ConsignacaoItem

                    {

                        Id = readerItens.GetInt32("id"),

                        NomeProduto = readerItens.GetString("NomeProduto"),

                        QuantidadeEnviada = readerItens.GetInt32("quantidade_enviada"),

                        PrecoUnitarioAcordado = readerItens.GetDecimal("preco_unitario_acordado")

                        // ... adicione os demais campos se precisar

                    });

                }

                readerItens.Close();

            }



            return lista;

        }



        public string AtualizarParaAcerto(Consignacao consignacao)
        {
            if (consignacao == null) return "Consignação inválida.";

            try
            {
                using var conn = new MySqlConnection(GetConnectionString());
                conn.Open();

                using var transaction = conn.BeginTransaction();

                // Atualizar status e data de previsão de acerto
                var sql = @"UPDATE consignacoes 
                    SET id_status = @status, 
                        data_previsao_acerto = @dataAcerto 
                    WHERE id = @id";

                using (var cmd = new MySqlCommand(sql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@status", consignacao.IdStatus);
                    cmd.Parameters.AddWithValue("@dataAcerto", consignacao.DataPrevisaoAcerto.HasValue
                                                                ? (object)consignacao.DataPrevisaoAcerto.Value
                                                                : DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", consignacao.Id);
                    cmd.ExecuteNonQuery();
                }

                // Atualizar itens da consignação
                if (consignacao.Itens != null)
                {
                    foreach (var item in consignacao.Itens)
                    {
                        var sqlItem = @"UPDATE consignacao_itens 
                                SET quantidade_vendida = @qtdVenda, 
                                    quantidade_devolvida = @qtdDev 
                                WHERE id = @idItem";

                        using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);
                        cmdItem.Parameters.AddWithValue("@qtdVenda", item.QuantidadeVendida);
                        cmdItem.Parameters.AddWithValue("@qtdDev", item.QuantidadeDevolvida);
                        cmdItem.Parameters.AddWithValue("@idItem", item.Id);
                        cmdItem.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return "ok";
            }
            catch (Exception ex)
            {
                return "Erro: " + ex.Message;
            }
        }




        public Consignacao GetById(int id)

        {

            using var conn = new MySqlConnection(GetConnectionString());

            conn.Open();



            var sql = @"SELECT c.*, cli.nome as NomeCliente 

                FROM consignacoes c 

                INNER JOIN clientes cli ON c.id_cliente = cli.id 

                WHERE c.id = @id";



            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);



            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;



            var c = new Consignacao

            {

                Id = reader.GetInt32("id"),

                IdCliente = reader.GetInt32("id_cliente"),

                NomeCliente = reader.GetString("NomeCliente"),
                DataPrevisaoAcerto = reader.IsDBNull(reader.GetOrdinal("data_previsao_acerto"))
                         ? (DateTime?)null
                         : reader.GetDateTime("data_previsao_acerto"),
                IdUsuario = reader.GetInt32("id_usuario"),

                IdStatus = reader.GetInt32("id_status"),

                DataSaida = reader.GetDateTime("data_saida"),

                ValorTotalEstimado = reader.GetDecimal("valor_total_estimado"),

                Observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? "" : reader.GetString("observacao")

            };

            reader.Close();



            var sqlItens = @"SELECT i.*, p.nome as NomeProduto 

                     FROM consignacao_itens i 

                     INNER JOIN produtos p ON i.id_produto = p.id 

                     WHERE i.id_consignacao = @id";



            using var cmdItens = new MySqlCommand(sqlItens, conn);

            cmdItens.Parameters.AddWithValue("@id", id);

            using var readerItens = cmdItens.ExecuteReader();



            while (readerItens.Read())

            {

                c.Itens.Add(new ConsignacaoItem

                {

                    Id = readerItens.GetInt32("id"),

                    IdConsignacao = readerItens.GetInt32("id_consignacao"),

                    IdProduto = readerItens.GetInt32("id_produto"),

                    NomeProduto = readerItens.GetString("NomeProduto"),

                    QuantidadeEnviada = readerItens.GetInt32("quantidade_enviada"),

                    QuantidadeVendida = readerItens.GetInt32("quantidade_vendida"),

                    QuantidadeDevolvida = readerItens.GetInt32("quantidade_devolvida"),

                    PrecoUnitarioAcordado = readerItens.GetDecimal("preco_unitario_acordado")

                });

            }



            return c;

        }

        //clientes

        // 🔹 CREATE

    public string Inserir(Cliente cliente)
    {
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"
        INSERT INTO clientes
        (nome, cpf_cnpj, telefone, email, endereco, ativo)
        VALUES
        (@nome, @cpf, @telefone, @email, @endereco, @ativo)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", cliente.Nome);
            cmd.Parameters.AddWithValue("@cpf", cliente.CpfCnpj);
            cmd.Parameters.AddWithValue("@telefone", cliente.Telefone);
            cmd.Parameters.AddWithValue("@email", cliente.Email);
            cmd.Parameters.AddWithValue("@endereco", cliente.Endereco);
            cmd.Parameters.AddWithValue("@ativo", cliente.Ativo);

            int linhasAfetadas = cmd.ExecuteNonQuery();
            if (linhasAfetadas > 0)
                return "Cliente cadastrado com sucesso!";
            else
                return "Nenhuma linha foi afetada. Cliente não cadastrado.";

        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062)
                return "Erro: Cliente já cadastrado (CPF ou email duplicado).";
            else
                return $"Erro no banco de dados: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Erro inesperado: {ex.Message}";
        }
    }

    // 🔹 READ - Buscar por ID
    public Cliente GetByIdCliente(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "SELECT * FROM clientes WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapearCliente(reader);
        }

        // 🔹 READ - Listar todos
        public List<Cliente> ListarTodos()
        {
            var lista = new List<Cliente>();

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "SELECT * FROM clientes where ativo = 1 ORDER BY 1 desc";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(MapearCliente(reader));
            }

            return lista;
        }

        // 🔹 UPDATE
        public bool Atualizar(Cliente cliente)
        {
            try
            {
                using var conn = new MySqlConnection(GetConnectionString());
                conn.Open();

                string sql = @"
                    UPDATE clientes
                    SET nome = @nome,
                        cpf_cnpj = @cpf,
                        telefone = @telefone,
                        email = @email,
                        endereco = @endereco,
                        ativo = @ativo
                    WHERE id = @id";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", cliente.Id);
                cmd.Parameters.AddWithValue("@nome", cliente.Nome);
                cmd.Parameters.AddWithValue("@cpf", cliente.CpfCnpj);
                cmd.Parameters.AddWithValue("@telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@email", cliente.Email);
                cmd.Parameters.AddWithValue("@endereco", cliente.Endereco);
                cmd.Parameters.AddWithValue("@ativo", cliente.Ativo);

                int linhasAfetadas = cmd.ExecuteNonQuery();

                // Debug: quantas linhas foram atualizadas
                System.Diagnostics.Debug.WriteLine($"Linhas afetadas: {linhasAfetadas}");

                if (linhasAfetadas == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Nenhum registro foi atualizado. Verifique se o ID existe.");
                }

                return linhasAfetadas > 0;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MySQL Error {ex.Number}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro inesperado: {ex.Message}");
                return false;
            }
        }


        // 🔹 DELETE (soft delete)
        public bool Desativar(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "UPDATE clientes SET ativo = false WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        // 🔹 Verificar CPF existente
        public Cliente VerificaSeExiste(string cpf, int? idIgnorar = null)
        { 
        using var conn = new MySqlConnection(GetConnectionString());
        conn.Open();

        string sql = @"
                SELECT id, nome, cpf_cnpj, telefone, email, endereco, data_cadastro, ativo
            FROM clientes
            WHERE cpf_cnpj = @cpf";

        if (idIgnorar.HasValue)
            sql += " AND id <> @idIgnorar";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cpf", cpf);

        if (idIgnorar.HasValue)
        cmd.Parameters.AddWithValue("@idIgnorar", idIgnorar.Value);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Cliente
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                CpfCnpj = reader.GetString("cpf_cnpj"),
                Telefone = reader["telefone"]?.ToString(),
                Email = reader["email"]?.ToString(),
                Endereco = reader["endereco"]?.ToString(),
                DataCadastro = reader.GetDateTime("data_cadastro"),
                Ativo = reader.GetBoolean("ativo")
            };
        }

            return null;
        }


        // 🔹 Método auxiliar para mapear
        private Cliente MapearCliente(MySqlDataReader reader)
        {
            return new Cliente
            {
                Id = reader.GetInt32("id"),
                Nome = reader.GetString("nome"),
                CpfCnpj = reader["cpf_cnpj"]?.ToString(),
                Telefone = reader["telefone"]?.ToString(),
                Email = reader["email"]?.ToString(),
                Endereco = reader["endereco"]?.ToString(),
                DataCadastro = reader.GetDateTime("data_cadastro"),
                Ativo = reader.GetBoolean("ativo")
            };
        }


    }
}