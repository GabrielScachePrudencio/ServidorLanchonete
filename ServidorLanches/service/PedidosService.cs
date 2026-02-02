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
                DefinirMovimentacaoEstoque(pedido);
                string resposta = ProcessarMovimentacaoEstoque(pedido, conn, transaction);


                if(resposta == "ok")
                {
                    int pedidoId = _pedidoRepo.AddPedido(pedido, conn, transaction);
                    pedido.Id = pedidoId;
                }


                transaction.Commit();
                return resposta;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        public string AtualizarPedido(PedidoDTO pedido)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Pedido antes da atualização
                var pedidoAtual = _pedidoRepo.GetPedidoById(pedido.Id);


                string resposta = "";


                // Só movimenta se o status mudou
                if (pedidoAtual.IdStatus != pedido.IdStatus)
                {
                    pedido.Itens = pedidoAtual.Itens; // garante itens
                    resposta = ProcessarMovimentacaoEstoque(pedido, conn, transaction);

                    if(resposta == "ok") _pedidoRepo.AtualizarPedido(pedido);
                    
                }

                transaction.Commit();
                return resposta;
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
