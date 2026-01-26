using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_LANCHES.model
{
    public class ConfiguracoesBanco
    {
        public int Id { get; set; }

        public string TipoConexao { get; set; }   
        public string Host { get; set; }
        public int Porta { get; set; }
        public string NomeBanco { get; set; }
        public string Usuario { get; set; }

        public bool BackupAutomatico { get; set; }
        public string? CaminhoBackup { get; set; }
        public DateTime? UltimoBackup { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}
