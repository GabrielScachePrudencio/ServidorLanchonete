using ServidorLanches.model;
using ServidorLanches.Repositories;

namespace ServidorLanches.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuarioService(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Usuario Login(string nome, string senha)
        {
            // aqui no futuro entra hash, validações etc
            return _usuarioRepository.BuscarPorNomeESenha(nome, senha);
        }
    }
}
