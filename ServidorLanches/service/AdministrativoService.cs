using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class AdministrativoService
    {
        private readonly AdministrativoRepository administrativoRepository;

        public AdministrativoService(AdministrativoRepository administrativoRepository)
        {
            this.administrativoRepository = administrativoRepository;
        }

        // ================= CONFIGURAÇÕES GERAIS =================

        public ConfiguracoesGerais GetConfiguracoes()
        {
            return administrativoRepository.GetConfiguracoes();
        }

        public bool AtualizarConfiguracoes(ConfiguracoesGerais config)
        {
            if (string.IsNullOrWhiteSpace(config.nome))
                return false;

            return administrativoRepository.AtualizarConfiguracoes(config);
        }

        // ================= CATEGORIA PRODUTO =================

        public List<CategoriaProduto> GetAllCategoria()
        {
            return administrativoRepository.GetCategoriaProdutos();
        }

        public bool AddCategoria(CategoriaProduto categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.nome))
                return false;

            return administrativoRepository.AddCategoriaProduto(categoria);
        }

        public bool UpdateCategoria(int id, CategoriaProduto categoria)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(categoria.nome))
                return false;

            return administrativoRepository.UpdateCategoriaProduto(id, categoria);
        }

        public bool DeleteCategoria(int id)
        {
            if (id <= 0)
                return false;

            return administrativoRepository.DeleteCategoriaProduto(id);
        }

        // ================= STATUS PEDIDO =================

        public List<TipoStatusPedido> GetAllStatusPedido()
        {
            return administrativoRepository.GetStatusPedido();
        }

        public bool AddStatusPedido(TipoStatusPedido status)
        {
            if (string.IsNullOrWhiteSpace(status.nome))
                return false;

            return administrativoRepository.AddStatusPedido(status);
        }

        public bool UpdateStatusPedido(int id, TipoStatusPedido status)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(status.nome))
                return false;

            return administrativoRepository.UpdateStatusPedido(id, status);
        }

        public bool DeleteStatusPedido(int id)
        {
            if (id <= 0)
                return false;

            return administrativoRepository.DeleteStatusPedido(id);
        }

        // ================= FORMAS DE PAGAMENTO =================

        public List<FormaDePagamento> GetAllFormasDePagamentos()
        {
            return administrativoRepository.GetAllFormasDePagamentos();
        }

        public bool AddFormaPagamento(FormaDePagamento forma)
        {
            if (string.IsNullOrWhiteSpace(forma.Descricao))
                return false;

            return administrativoRepository.AddFormaDePagamento(forma);
        }

        public bool UpdateFormaPagamento(int id, FormaDePagamento forma)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(forma.Descricao))
                return false;

            return administrativoRepository.UpdateFormaDePagamento(id, forma);
        }

        public bool DeleteFormaPagamento(int id)
        {
            if (id <= 0)
                return false;

            return administrativoRepository.DeleteFormaDePagamento(id);
        }

        // ================= CONFIGURAÇÕES FISCAIS =================

        public ConfiguracoesFiscais GetConfiguracoesFiscais()
        {
            return administrativoRepository.GetConfiguracoesFiscais();
        }

        public bool AddConfiguracoesFiscais(ConfiguracoesFiscais c)
        {
            return administrativoRepository.AddConfiguracoesFiscais(c);
        }

        public bool AtualizarConfigFiscal(ConfiguracoesFiscais c)
        {
            return administrativoRepository.AtualizarConfiguracoesFiscais(c);
        }


        
    }
}
