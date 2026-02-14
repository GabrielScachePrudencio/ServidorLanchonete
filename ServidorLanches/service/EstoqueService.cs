using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PDV_LANCHES.model;
using ServidorLanches.model;
using ServidorLanches.model.dto;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class EstoqueService
    {
        private readonly DbConnectionManager _dbManager;
        private readonly IConfiguration _config;
        private readonly EstoqueRepository estoqueRepository;
        public EstoqueService(IConfiguration config, EstoqueRepository estoqueRepository, DbConnectionManager db)
        {
            _dbManager = db;
            _config = config;
            this.estoqueRepository = estoqueRepository; 
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;

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

        public bool UpdateQtddProduto(int idProduto, int quantidade)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var sucesso = estoqueRepository.AumentarEstoque(
                    idProduto,
                    quantidade,
                    null,
                    OrigemMovimentacaoEstoque.AJUSTE,
                    OrigemMovimentacaoEstoque.AJUSTE.ToString(),
                    conn,
                    transaction
                );

                transaction.Commit();
                return sucesso;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        public List<Estoque> GetAllEstoques()
        {
            return estoqueRepository.GetAllEstoques();

        }



        }
}
