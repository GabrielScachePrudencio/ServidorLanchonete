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

            using var conn = new MySqlConnection(GetConnectionString());

            conn.Open();

            using var transaction = conn.BeginTransaction();



            try

            {

                var sql = "UPDATE consignacoes SET id_status = @status WHERE id = @id";

                using var cmd = new MySqlCommand(sql, conn, transaction);

                cmd.Parameters.AddWithValue("@status", consignacao.IdStatus);

                cmd.Parameters.AddWithValue("@id", consignacao.Id);

                cmd.ExecuteNonQuery();



                foreach (var item in consignacao.Itens)

                {

                    var sqlItem = @"UPDATE consignacao_itens SET 

                             quantidade_vendida = @qtdVenda, 

                             quantidade_devolvida = @qtdDev 

                             WHERE id = @idItem";



                    using var cmdItem = new MySqlCommand(sqlItem, conn, transaction);

                    cmdItem.Parameters.AddWithValue("@qtdVenda", item.QuantidadeVendida);

                    cmdItem.Parameters.AddWithValue("@qtdDev", item.QuantidadeDevolvida);

                    cmdItem.Parameters.AddWithValue("@idItem", item.Id);

                    cmdItem.ExecuteNonQuery();

                }



                transaction.Commit();

                return "ok";

            }

            catch (Exception ex)

            {

                transaction.Rollback();

                return "Erro " + ex.Message;

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



    }
}