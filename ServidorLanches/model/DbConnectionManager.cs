using System.Text.Json;
using System.Text.Json.Nodes;

namespace ServidorLanches.model
{
    public class DbConnectionManager
    {
        private string _connectionString;
        private readonly string _caminhoPasta;
        private readonly string _caminhoArquivo;

        public DbConnectionManager(IConfiguration configuration)
        {
            // 1. Define os caminhos
            _caminhoPasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PDV_Lanches", "SERVIDOR_Lanches_Config");
            _caminhoArquivo = Path.Combine(_caminhoPasta, "configServidorConexao.json");

            // 2. Tenta carregar do AppData
            if (File.Exists(_caminhoArquivo))
            {
                try
                {
                    string json = File.ReadAllText(_caminhoArquivo);
                    var doc = JsonNode.Parse(json);
                    _connectionString = doc["ConnectionString"]?.ToString();
                    Console.WriteLine("--- Conexão carregada do arquivo AppData.");
                }
                catch { _connectionString = null; }
            }

            // 3. Se não existe o arquivo ou a string está vazia, cria o arquivo com a padrão
            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection");

                try
                {
                    if (!Directory.Exists(_caminhoPasta)) Directory.CreateDirectory(_caminhoPasta);

                    var dadosIniciais = new { ConnectionString = _connectionString };
                    string jsonInicial = JsonSerializer.Serialize(dadosIniciais);

                    File.WriteAllText(_caminhoArquivo, jsonInicial);
                    Console.WriteLine("--- Arquivo de configuração criado no AppData com a conexão padrão.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--- Erro ao criar arquivo inicial: {ex.Message}");
                }
            }
        }

        public string CurrentConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }
    }
}