using MySql.Data.MySqlClient;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class PedidosService
    {
        private readonly IConfiguration _config;
        private readonly PedidosRepository _pedidoRepo;
        private readonly EstoqueRepository _estoqueRepo;
        private string GetConnectionString() =>
            _config.GetConnectionString("MySql");

        public PedidosService(
            IConfiguration config,
            PedidosRepository pedidoRepo,
            EstoqueRepository estoqueRepo)
        {
            _config = config;
            _pedidoRepo = pedidoRepo;
            _estoqueRepo = estoqueRepo;
        }

        public List<PedidoDTO> PegarTodosOsPedidos()
            => _pedidoRepo.GetAllPedidos();

        public PedidoDTO PegarPedidoComItens(int id)
            => _pedidoRepo.GetPedidoById(id);

        public bool CriarPedido(PedidoDTO pedido)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                DefinirMovimentacaoEstoque(pedido);

               int pedidoId = _pedidoRepo.AddPedido(pedido, conn, transaction);
                pedido.Id = pedidoId;

                ProcessarMovimentacaoEstoque(pedido, conn, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        public bool AtualizarPedido(PedidoDTO pedido)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Pedido antes da atualização
                var pedidoAtual = _pedidoRepo.GetPedidoById(pedido.Id);


                DefinirMovimentacaoEstoque(pedido);
                _pedidoRepo.AtualizarPedido(pedido);

                // Só movimenta se o status mudou
                if (pedidoAtual.IdStatus != pedido.IdStatus)
                {
                    pedido.Itens = pedidoAtual.Itens; // garante itens
                    ProcessarMovimentacaoEstoque(pedido, conn, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
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

                _pedidoRepo.AtualizarStatusDoPedidoById(id, idStatus);

                ProcessarMovimentacaoEstoque(pedido, conn, transaction);

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

        private void ProcessarMovimentacaoEstoque(
            PedidoDTO pedido,
            MySqlConnection conn,
            MySqlTransaction transaction)
            {
                DefinirMovimentacaoEstoque(pedido);

                if (pedido.TipoMovimentacao == TipoMovimentacaoEstoque.NENHUMA)
                    return;

                _estoqueRepo.MovimentarEstoque(pedido, conn, transaction);
            }


        private void DefinirMovimentacaoEstoque(PedidoDTO pedido)
        {
            pedido.TipoMovimentacao = TipoMovimentacaoEstoque.NENHUMA;
            pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.NAO_DEFINIDO;

            switch (pedido.IdStatus)
            {
                case 5: // Finalizado
                    pedido.TipoMovimentacao = TipoMovimentacaoEstoque.SAIDA;
                    pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.VENDA;
                    break;

                case 9: // Estornado
                    pedido.TipoMovimentacao = TipoMovimentacaoEstoque.ENTRADA;
                    pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.ESTORNADO;
                    break;

                case 10: // Compra
                    pedido.TipoMovimentacao = TipoMovimentacaoEstoque.ENTRADA;
                    pedido.OrigemMovimentacaoEstoque = OrigemMovimentacaoEstoque.COMPRA;
                    break;

                default:
                    // Status que NÃO mexem no estoque (Pronto, Cancelado, etc)
                    break;
            }
        }

    }
}
