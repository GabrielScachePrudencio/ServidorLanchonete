using Microsoft.AspNetCore.Mvc;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class CaixaService
    {
        private readonly CaixaRepository _caixaRepository;
        private readonly PedidosRepository _pedidoRepository;

        public CaixaService(CaixaRepository caixaRepository, PedidosRepository pedidoRepository)
        {
            _caixaRepository = caixaRepository;
            _pedidoRepository = pedidoRepository;
        }

        public List<TerminalCaixa> GetAllCaixasTerminais()
        {
            return _caixaRepository.GetAllCaixasTerminais();
        }
        public List<Caixa> GetAllCaixas()
        {
            return _caixaRepository.GetAllCaixas();
        }
        

        public Caixa IniciarCaixaAberto(Caixa caixa)
        {
            return _caixaRepository.IniciarCaixaAberto(caixa);
        }

        public Caixa FecharCaixa( Caixa caixaIncompleto)
        {
            List<PedidoDTO> todosOsPedidosPeloIdCaixa = _pedidoRepository.GetAllPedidosByIdCaixa(caixaIncompleto.id);

            return _caixaRepository.FecharCaixa(caixaIncompleto, todosOsPedidosPeloIdCaixa);
        }

        
        public bool SalvarFecharCaixa(Caixa caixaCompleto)
        {
            return _caixaRepository.SalvarFecharCaixa(caixaCompleto);
        }


    }
}
