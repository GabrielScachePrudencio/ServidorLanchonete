using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class ConsignacaoService
    {
        private readonly ConsignacaoRepository repository;
        private readonly ProdutoRepository produtoRepository; 
        private readonly EstoqueRepository estoqueRepository; 
        private readonly DbConnectionManager _dbManager;

        public ConsignacaoService(DbConnectionManager dbManager, ConsignacaoRepository repository, ProdutoRepository produtoRepository, EstoqueRepository estoqueRepository)
        {
            _dbManager = dbManager;
            this.estoqueRepository = estoqueRepository;
            this.repository = repository;
            this.produtoRepository = produtoRepository;
        }
        private string GetConnectionString() => _dbManager.CurrentConnectionString;

        public List<Consignacao> GetAllConsignacoes()
        {
            return repository.GetAll();
        }

        public Consignacao GetById(int id)
        {
            return repository.GetById(id);
        }

        public string CriarConsignacao(Consignacao consignacao)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                if (consignacao.Itens == null || !consignacao.Itens.Any())
                    return "Consignação sem itens.";

                // 🔎 1 - Verifica estoque
                foreach (var item in consignacao.Itens)
                {
                    if (!estoqueRepository.TemEstoqueDisponivel(
                            item.IdProduto,
                            item.QuantidadeEnviada,
                            conn,
                            transaction))
                    {
                        transaction.Rollback();
                        return $"Estoque insuficiente para o produto: {item.NomeProduto}";
                    }
                }

                // 💾 2 - Salva consignação
                Consignacao c = repository.Salvar(consignacao, conn, transaction);
                consignacao.Id = c.Id;

                // 📦 3 - Movimenta estoque (SAÍDA)
                string resposta = estoqueRepository.MovimentarEstoque(consignacao, conn, transaction);

                if (resposta != "ok")
                {
                    transaction.Rollback();
                    return resposta;
                }

                transaction.Commit();
                return "ok";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Erro fatal ao criar consignação: {ex.Message}");
            }
        }


        // 2. Processar o Acerto de Contas (Baixa)
        public string ProcessarBaixa(Consignacao consignacao)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var consignacaoBanco = repository.GetById(consignacao.Id);

                if (consignacaoBanco == null || consignacaoBanco.IdStatus != 1)
                    return "Consignação inválida ou já finalizada.";

                decimal valorVendidoTotal = 0;

                foreach (var itemNovo in consignacao.Itens)
                {
                    var itemBanco = consignacaoBanco.Itens
                        .FirstOrDefault(x => x.IdProduto == itemNovo.IdProduto);

                    if (itemBanco == null)
                        continue;

                    // 🔎 Validação lógica
                    if (itemNovo.QuantidadeVendida + itemNovo.QuantidadeDevolvida != itemBanco.QuantidadeEnviada)
                    {
                        transaction.Rollback();
                        return $"Inconsistência no produto {itemNovo.NomeProduto}.";
                    }

                    // 📦 DEVOLUÇÃO → volta pro estoque
                    if (itemNovo.QuantidadeDevolvida > 0)
                    {
                        estoqueRepository.AumentarEstoque(
                            itemNovo.IdProduto,
                            itemNovo.QuantidadeVendida,
                            consignacaoBanco.Id,
                            OrigemMovimentacaoEstoque.ESTORNADO,
                            "ESTORNO",
                            conn,
                            transaction
                        );
                    }

                    // 💰 Soma valor vendido
                    valorVendidoTotal +=
                        itemNovo.QuantidadeVendida * itemNovo.PrecoUnitarioAcordado;
                }

                // 📝 Atualiza consignação
                consignacao.DataPrevisaoAcerto = DateTime.Now;
                consignacao.ValorTotalEstimado = valorVendidoTotal;
                consignacao.NomeStatus = "Finalizado";
                consignacao.IdStatus = 2;

                string respostaAtualizar = repository.AtualizarParaAcerto(consignacao);

                if (respostaAtualizar != "ok")
                {
                    transaction.Rollback();
                    return respostaAtualizar;
                }

                transaction.Commit();
                return "ok";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ex.Message;
            }
        }

        public string ProcessarEstorno(int idConsignacao)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var consignacaoBanco = repository.GetById(idConsignacao);

                if (consignacaoBanco == null || consignacaoBanco.IdStatus != 2)
                    return "Consignação inválida ou não está finalizada.";

                foreach (var item in consignacaoBanco.Itens)
                {
                    if (item.QuantidadeVendida > 0)
                    {
                        estoqueRepository.AumentarEstoque(
                            item.IdProduto,
                            item.QuantidadeVendida,
                            consignacaoBanco.Id,
                            OrigemMovimentacaoEstoque.ESTORNADO,
                            "ESTORNO",
                            conn,
                            transaction
                        );

                    }

                    if (item.QuantidadeDevolvida > 0)
                    {
                        estoqueRepository.DiminuirEstoque(
                            item.IdProduto,
                            item.QuantidadeVendida,
                            consignacaoBanco.Id,
                            OrigemMovimentacaoEstoque.ESTORNADO,
                            "ESTORNO",
                            conn,
                            transaction
                        );
                    }
                }

                consignacaoBanco.IdStatus = 3; 
                consignacaoBanco.ValorTotalEstimado = 0;
                consignacaoBanco.DataPrevisaoAcerto = null;

                string respostaAtualizar = repository.AtualizarParaAcerto(consignacaoBanco);

                if (respostaAtualizar != "ok")
                {
                    transaction.Rollback();
                    return respostaAtualizar;
                }

                transaction.Commit();
                return "ok";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ex.Message;
            }
        }



    }
}