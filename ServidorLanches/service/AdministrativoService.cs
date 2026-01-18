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


        public ConfiguracoesGerais GetConfiguracoes()
        {
            return administrativoRepository.GetConfiguracoes();
        }

        public bool AtualizarConfiguracoes(ConfiguracoesGerais config)
        {
            if (string.IsNullOrEmpty(config.nome)) return false;

            return administrativoRepository.AtualizarConfiguracoes(config);
        }

    }
}
