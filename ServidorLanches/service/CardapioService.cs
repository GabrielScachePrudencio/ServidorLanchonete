using ServidorLanches.model;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class CardapioService
    {
        private readonly CardapioRepository _repository;

        public CardapioService(CardapioRepository cardapioRepository)
        {
            _repository = cardapioRepository;
        }

        public List<Cardapio> GetAllCardapios()
            => _repository.GetAll();

        public Cardapio GetById(int id)
            => _repository.GetById(id);

        public bool AddCardapio(Cardapio cardapio)
            => _repository.Add(cardapio);

        public bool UpdateCardapio(Cardapio cardapio)
            => _repository.Update(cardapio);

        public bool DeleteCardapio(int id)
            => _repository.Delete(id);
    }
}
