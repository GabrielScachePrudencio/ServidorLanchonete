using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using System.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ServidorLanches.repository
{
    public class AdministrativoRepository
    {
        private readonly DbConnectionManager _dbManager;

        public AdministrativoRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;


        public ConfiguracoesGerais GetConfiguracoes()
        {
            using var conn = new MySqlConnection(
                GetConnectionString()
            );

            conn.Open();
            ConfiguracoesGerais config = new();
            string sql = "SELECT * FROM configuracoesGerais";

            using (var cmd = new MySqlCommand(sql, conn))
            
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    config = new ConfiguracoesGerais
                    {
                        nome = reader.GetString("nome"),
                        nomeFantasia = reader.GetString("nomeFantasia"),
                        telefone = reader.GetString("telefone"),
                        email = reader.GetString("email"),
                        endereco = reader.GetString("endereco"),
                        pathImagemLogo = reader.GetString("pathImagemLogo")
                    };
                }
            }

            return config; 

        }

        public bool AtualizarConfiguracoes(ConfiguracoesGerais config)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            // SQL que atualiza os campos. 
            // Nota: Se sua tabela tiver ID, você pode adicionar "WHERE id = 1"
            string sql = @"UPDATE configuracoesGerais SET 
                    nome = @nome, 
                    nomeFantasia = @nomeFantasia, 
                    telefone = @telefone, 
                    email = @email, 
                    endereco = @endereco, 
                    pathImagemLogo = @pathImagemLogo";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", config.nome);
            cmd.Parameters.AddWithValue("@nomeFantasia", config.nomeFantasia);
            cmd.Parameters.AddWithValue("@telefone", config.telefone);
            cmd.Parameters.AddWithValue("@email", config.email);
            cmd.Parameters.AddWithValue("@endereco", config.endereco);
            cmd.Parameters.AddWithValue("@pathImagemLogo", config.pathImagemLogo ?? (object)DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }


        //GetCategoriaProdutos
        public List<CategoriaProduto> GetCategoriaProdutos()
        {
            using var conn = new MySqlConnection(
                GetConnectionString()
            );

            conn.Open();
            List<CategoriaProduto> lista = new List<CategoriaProduto>();
            string sql = "SELECT * FROM categoriaProduto";

            using (var cmd = new MySqlCommand(sql, conn))

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new CategoriaProduto() {
                        id = reader.GetInt32("id"),
                        nome = reader.GetString("nome"),
                        ativo = reader.GetBoolean("ativo")
                    } 
                    );
                }
            }

            return lista;
        }

        public bool AddCategoriaProduto(CategoriaProduto categoria)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "INSERT INTO categoriaProduto (nome, ativo) VALUES (@nome, @ativo)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", categoria.nome);
            cmd.Parameters.AddWithValue("@ativo", categoria.ativo);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateCategoriaProduto(int id, CategoriaProduto categoria)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"UPDATE categoriaProduto SET
        nome = @nome, 
        ativo = @ativo
    WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nome", categoria.nome);
            cmd.Parameters.AddWithValue("@ativo", categoria.ativo);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteCategoriaProduto(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "DELETE FROM categoriaProduto WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }




        //TipoStatusPedido
        public List<TipoStatusPedido> GetStatusPedido()
        {
            using var conn = new MySqlConnection(
                GetConnectionString()
            );

            conn.Open();
            List<TipoStatusPedido> lista = new List<TipoStatusPedido>();
            string sql = "SELECT * FROM statuspedido";

            using (var cmd = new MySqlCommand(sql, conn))

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new TipoStatusPedido()
                    {
                        id = reader.GetInt32("id"),
                        nome = reader.GetString("nome"),
                        ativo = reader.GetBoolean("ativo")
                    }
                    );
                }
            }

            return lista;
        }

        public bool AddStatusPedido(TipoStatusPedido status)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "INSERT INTO statuspedido (nome, ativo) VALUES (@nome, @ativo)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", status.nome);
            cmd.Parameters.AddWithValue("@ativo", status.ativo);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateStatusPedido(int id, TipoStatusPedido status)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"UPDATE statuspedido SET
        nome = @nome, ativo = @ativo
    WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nome", status.nome);
            cmd.Parameters.AddWithValue("@ativo", status.ativo);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteStatusPedido(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "DELETE FROM statuspedido WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }




        //formas de pagamento
        public List<FormaDePagamento> GetAllFormasDePagamentos()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            var lista = new List<FormaDePagamento>();
            string sql = "SELECT id, descricao, ativo FROM formas_pagamento WHERE ativo = 1";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new FormaDePagamento()
                {
                    Id = reader.GetInt32("id"),
                    Descricao = reader.GetString("descricao"),
                    Ativo = reader.GetBoolean("ativo") 
                });
            }
            return lista;
        }

        public bool AddFormaDePagamento(FormaDePagamento forma)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"INSERT INTO formas_pagamento (descricao, ativo)
                   VALUES (@descricao, @ativo);
                   SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@descricao", forma.Descricao);
            cmd.Parameters.AddWithValue("@ativo", forma.Ativo);

            var idGerado = cmd.ExecuteScalar();

            if (idGerado != null)
            {
                forma.Id = Convert.ToInt32(idGerado); 
                return true;
            }

            return false;
        }
        public bool UpdateFormaDePagamento(int id, FormaDePagamento forma)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"UPDATE formas_pagamento SET
        descricao = @descricao,
        ativo = @ativo
    WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@descricao", forma.Descricao);
            cmd.Parameters.AddWithValue("@ativo", forma.Ativo);

            return cmd.ExecuteNonQuery() > 0;
        }
        public bool DeleteFormaDePagamento(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "DELETE FROM formas_pagamento WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }


        //configuracoes fiscais 
        public ConfiguracoesFiscais GetConfiguracoesFiscais()
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = "SELECT * FROM configuracoes_fiscais WHERE id = 1";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new ConfiguracoesFiscais
            {
                Id = reader.GetInt32("id"),
                Cnpj = reader.GetString("cnpj"),
                InscricaoEstadual = reader.GetString("inscricao_estadual"),
                RegimeTributario = reader.GetString("regime_tributario"),
                AliquotaIcms = reader.GetDecimal("aliquota_icms"),
                Csosn = reader.GetString("csosn"),
                CstPis = reader.GetString("cst_pis"),
                CstCofins = reader.GetString("cst_cofins"),
                SerieNf = reader.GetString("serie_nf"),
                NumeroUltimaNf = reader.GetInt32("numero_ultima_nf"),
                AmbienteProducao = reader.GetBoolean("ambiente_producao"),
                CaminhoCertificado = reader.GetString("caminho_certificado"),
                ValidadeCertificado = reader.GetDateTime("validade_certificado")
            };
        }


        public bool AtualizarConfiguracoesFiscais(ConfiguracoesFiscais config)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"UPDATE configuracoes_fiscais SET
                cnpj = @cnpj,
                inscricao_estadual = @inscricao_estadual,
                regime_tributario = @regime_tributario,
                aliquota_icms = @aliquota_icms,
                csosn = @csosn,
                cst_pis = @cst_pis,
                cst_cofins = @cst_cofins,
                serie_nf = @serie_nf,
                numero_ultima_nf = @numero_ultima_nf,
                ambiente_producao = @ambiente_producao,
                caminho_certificado = @caminho_certificado,
                validade_certificado = @validade_certificado
            WHERE id = 1";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@cnpj", config.Cnpj);
            cmd.Parameters.AddWithValue("@inscricao_estadual", config.InscricaoEstadual);
            cmd.Parameters.AddWithValue("@regime_tributario", config.RegimeTributario);
            cmd.Parameters.AddWithValue("@aliquota_icms", config.AliquotaIcms);
            cmd.Parameters.AddWithValue("@csosn", config.Csosn);
            cmd.Parameters.AddWithValue("@cst_pis", config.CstPis);
            cmd.Parameters.AddWithValue("@cst_cofins", config.CstCofins);
            cmd.Parameters.AddWithValue("@serie_nf", config.SerieNf);
            cmd.Parameters.AddWithValue("@numero_ultima_nf", config.NumeroUltimaNf);
            cmd.Parameters.AddWithValue("@ambiente_producao", config.AmbienteProducao);
            cmd.Parameters.AddWithValue("@caminho_certificado", config.CaminhoCertificado);
            cmd.Parameters.AddWithValue("@validade_certificado", config.ValidadeCertificado);

            return cmd.ExecuteNonQuery() > 0;
        }


        public bool AddConfiguracoesFiscais(ConfiguracoesFiscais config)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"INSERT INTO configuracoes_fiscais (
                cnpj,
                inscricao_estadual,
                regime_tributario,
                aliquota_icms,
                csosn,
                cst_pis,
                cst_cofins,
                serie_nf,
                numero_ultima_nf,
                ambiente_producao,
                caminho_certificado,
                validade_certificado
            ) VALUES (
                @cnpj,
                @inscricao_estadual,
                @regime_tributario,
                @aliquota_icms,
                @csosn,
                @cst_pis,
                @cst_cofins,
                @serie_nf,
                @numero_ultima_nf,
                @ambiente_producao,
                @caminho_certificado,
                @validade_certificado
            )";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@cnpj", config.Cnpj);
            cmd.Parameters.AddWithValue("@inscricao_estadual", config.InscricaoEstadual);
            cmd.Parameters.AddWithValue("@regime_tributario", config.RegimeTributario);
            cmd.Parameters.AddWithValue("@aliquota_icms", config.AliquotaIcms);
            cmd.Parameters.AddWithValue("@csosn", config.Csosn);
            cmd.Parameters.AddWithValue("@cst_pis", config.CstPis);
            cmd.Parameters.AddWithValue("@cst_cofins", config.CstCofins);
            cmd.Parameters.AddWithValue("@serie_nf", config.SerieNf);
            cmd.Parameters.AddWithValue("@numero_ultima_nf", config.NumeroUltimaNf);
            cmd.Parameters.AddWithValue("@ambiente_producao", config.AmbienteProducao);
            cmd.Parameters.AddWithValue("@caminho_certificado", config.CaminhoCertificado);
            cmd.Parameters.AddWithValue("@validade_certificado", config.ValidadeCertificado);

            return cmd.ExecuteNonQuery() > 0;
        }


        



    }
}
