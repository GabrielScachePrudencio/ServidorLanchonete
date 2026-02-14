using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class PedidosService
    {
        private readonly DbConnectionManager _dbManager;
        private readonly PedidosRepository _pedidoRepo;
        private readonly EstoqueRepository _estoqueRepo;


        public PedidosService(
            DbConnectionManager dbManager,
            PedidosRepository pedidoRepo,
            EstoqueRepository estoqueRepo)
        {
            _dbManager = dbManager;
            _pedidoRepo = pedidoRepo;
            _estoqueRepo = estoqueRepo;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;



        public List<PedidoDTO> PegarTodosOsPedidos()
            => _pedidoRepo.GetAllPedidos();

        public List<PedidoDTO> PegarTodosOsPedidosPorCaixa(int idca)
            => _pedidoRepo.GetAllPedidosByIdCaixa(idca);

        

        public List<PedidoDTO> PegarTodosOsPedidosFromDia()
            => _pedidoRepo.GetAllPedidosFromToday();

        public PedidoDTO PegarPedidoComItens(int id)
            => _pedidoRepo.GetPedidoById(id);

        public string CriarPedido(PedidoDTO pedido)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. PRÉ-VALIDAÇÃO (Garante que não vai dar erro de falta de estoque depois)
                DefinirMovimentacaoEstoque(pedido);

                if (pedido.TipoMovimentacao == TipoMovimentacaoEstoque.SAIDA)
                {
                    foreach (var item in pedido.Itens)
                    {
                        // Método simples que apenas faz um SELECT e retorna bool
                        if (!_estoqueRepo.TemEstoqueDisponivel(item.IdProduto, item.Quantidade, conn, transaction))
                        {
                            return $"Estoque insuficiente para o produto: {item.NomeProduto}";
                        }
                    }
                }

                // 2. INSERIR PEDIDO (Agora é seguro, pois sabemos que há estoque)
                int pedidoId = _pedidoRepo.AddPedido(pedido, conn, transaction);
                pedido.Id = pedidoId;

                // 3. PROCESSAR MOVIMENTAÇÃO REAL
                // Agora o método MovimentarEstoque não vai falhar por estoque nem por FK
                string resposta = _estoqueRepo.MovimentarEstoque(pedido, conn, transaction);

                if (resposta == "ok" || resposta == "")
                {
                    transaction.Commit();
                    return "ok";
                }
                else
                {
                    transaction.Rollback();
                    return resposta;
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Erro fatal: {ex.Message}");
            }
        }



        public string AtualizarPedido(PedidoDTO pedido)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var pedidoAtual = _pedidoRepo.GetPedidoById(pedido.Id);
                if (pedidoAtual == null) return "Pedido não encontrado";

                // Só fazemos algo se o status realmente mudou
                if (pedidoAtual.IdStatus != pedido.IdStatus)
                {
                    pedido.Itens = pedidoAtual.Itens; // Garante que os itens estão carregados
                    DefinirMovimentacaoEstoque(pedido);

                    // 1. Se for uma SAÍDA (venda), precisamos validar se tem estoque antes de mudar o status
                    if (pedido.TipoMovimentacao == TipoMovimentacaoEstoque.SAIDA)
                    {
                        foreach (var item in pedido.Itens)
                        {
                            if (!_estoqueRepo.TemEstoqueDisponivel(item.IdProduto, item.Quantidade, conn, transaction))
                            {
                                return $"Estoque insuficiente para {item.NomeProduto}. Atualização cancelada.";
                            }
                        }
                    }

                    // 2. Atualiza o status do pedido no banco
                    _pedidoRepo.AtualizarPedido(pedido, conn, transaction); // Certifique-se de passar conn e transaction

                    // 3. Registra a movimentação no estoque e o histórico (MovimentarEstoque)
                    string resposta = _estoqueRepo.MovimentarEstoque(pedido, conn, transaction);

                    if (resposta != "ok" && resposta != "")
                    {
                        transaction.Rollback();
                        return resposta;
                    }
                }

                transaction.Commit();
                return "ok";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Erro ao atualizar pedido: {ex.Message}");
            }
        }



        public bool AtualizarStatusSomente(int id, int idStatus)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var pedido = _pedidoRepo.GetPedidoById(id);
                if (pedido == null)
                    throw new Exception("Pedido não encontrado");

                pedido.IdStatus = idStatus;

                ProcessarMovimentacaoEstoque(pedido, conn, transaction);

                _pedidoRepo.AtualizarStatusDoPedidoById(id, idStatus);


                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        public bool DeletarPedido(int id)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var pedido = _pedidoRepo.GetPedidoById(id);
                if (pedido == null)
                    return false;

                // Reverter estoque
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.ENTRADA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.ESTORNADO;

                _estoqueRepo.MovimentarEstoque(pedido, conn, transaction);

                bool certo2 = _pedidoRepo.DeletePedido(id);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        //estoque

        private string ProcessarMovimentacaoEstoque(
            PedidoDTO pedido,
            MySqlConnection conn,
            MySqlTransaction transaction)
            {
                DefinirMovimentacaoEstoque(pedido);

                if (pedido.TipoMovimentacao == TipoMovimentacaoEstoque.NENHUMA)
                    return "";

                return _estoqueRepo.MovimentarEstoque(pedido, conn, transaction);
            }


        private void DefinirMovimentacaoEstoque(PedidoDTO pedido)
        {
            pedido.TipoMovimentacao = TipoMovimentacaoEstoque.NENHUMA;
            pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.NAO_DEFINIDO;



            // pronto
            if (pedido.IdStatus == 1 || pedido.StatusPedido == "pronto")
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.NENHUMA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.PRONTO;
            }
            // Finalizado
            if (pedido.IdStatus == 2 || pedido.StatusPedido == "Finalizado")
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.SAIDA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.VENDA;
            }
            // Cancelado
            if (pedido.IdStatus == 3 || pedido.StatusPedido == "Cancelado")
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.NENHUMA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.COMPRA;
            }
            // Estornado
            if (pedido.IdStatus == 4 || pedido.StatusPedido == "Estornado")
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.ENTRADA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.ESTORNADO;
            }
            // Compra
            if (pedido.IdStatus == 5 || pedido.StatusPedido == "Compra")
            {
                pedido.TipoMovimentacao = TipoMovimentacaoEstoque.ENTRADA;
                pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.COMPRA;
            }


        }

    }
}
