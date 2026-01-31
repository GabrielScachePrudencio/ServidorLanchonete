using ServidorLanches.model;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class ProdutoService
    {
        private readonly ProdutoRepository _repository;

        public ProdutoService(ProdutoRepository produtoRepository)
        {
            _repository = produtoRepository;
        }

        public List<Produto> GetAllProduto()
            => _repository.GetAll();
        public List<Produto> GetAllProdutoAtivos()
            => _repository.GetAllAtivos();

        public Produto GetByIdProduto(int id)
            => _repository.GetById(id);

        public bool AddProduto(Produto produto)
            => _repository.Add(produto);

        public bool UpdateProduto(Produto produto)
            => _repository.Update(produto);

        public bool DeleteProduto(int id)
            => _repository.Delete(id);
    }
}
