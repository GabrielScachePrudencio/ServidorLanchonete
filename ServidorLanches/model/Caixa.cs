using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorLanches.model
{
    public class Caixa
    {
        public int id { get; set; }
        public int idUsuario { get; set; }
        public int idTerminal { get; set; }
        public DateTime dataAbertura { get; set; }
        public DateTime? dataFechamento { get; set; }
        public string status { get; set; }
        public decimal valorInicial { get; set; }
        public decimal valorFinal { get; set; }
        public decimal valor_calculado { get; set; }
        public decimal diferença { get; set; }
    }
}
