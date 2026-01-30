using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_LANCHES.model
{
    public class ConfiguracoesBanco
    {

        public int Porta { get; set; }

        // Porta do MySQL (para a Connection String do banco)
        public int PortaBanco { get; set; }

        public string Host { get; set; }
        public string NomeBanco { get; set; }
        public string UsuarioBanco { get; set; }
        public string senhaBanco { get; set; }
        public string TipoConexao { get; set; }







        public int Id { get; set; }
        public bool BackupAutomatico { get; set; }
        public string? CaminhoBackup { get; set; }
        public DateTime? UltimoBackup { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}
