using Microsoft.AspNetCore.Mvc;
using PDV_LANCHES.model;
using ServidorLanches.model.dto;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class EstoqueService
    {
        private readonly IConfiguration _config;
        private readonly EstoqueRepository estoqueRepository;
        public EstoqueService(IConfiguration config, EstoqueRepository estoqueRepository)
        {
            _config = config;
            this.estoqueRepository = estoqueRepository; 
        }


        public List<MovimentacaoEstoqueDTO> GetAll()
        {
            return estoqueRepository.GetAll();
        }

        public MovimentacaoEstoqueDTO GetById(int id)
        {
            return estoqueRepository.GetById(id);
        }

        public bool Delete(int id)
        {
            return estoqueRepository.Delete(id);
        }


    }
}
