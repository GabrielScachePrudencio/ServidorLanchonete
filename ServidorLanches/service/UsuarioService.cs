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

        public List<Usuario> todosUsuarios()
        {
            return _usuarioRepository.allUsuarios();
        }

        public Usuario GetUsuarioById(int id)
        {
            return _usuarioRepository.GetUsuarioById(id);
        }
        public bool AtualizarUsuario(Usuario usuarioNovo)
        {
            return _usuarioRepository.AtualizarUsuario(usuarioNovo);
        }
        public bool deletarUsuario(int id)
        {
            return _usuarioRepository.DeletarUsuarioPorId(id);
        }
        public bool AddUsuario(Usuario novoUsuario)
        {
            return _usuarioRepository.AdicionarUsuario(novoUsuario);
        }
    }
}
