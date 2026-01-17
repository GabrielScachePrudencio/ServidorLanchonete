using ServidorLanches.model;
using ServidorLanches.repository;

namespace ServidorLanches.service
{
    public class CardapioService
    {
        private readonly CardapioRepository _cardapioRepository;
        public CardapioService(repository.CardapioRepository cardapioRepository)
        { 

            _cardapioRepository = cardapioRepository;
        }
        public List<Cardapio> GetAllCardapios()
        {
            return _cardapioRepository.getAllCardapios();
        }
    }
}
