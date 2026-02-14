using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_LANCHES.model
{
    public class ConsignacaoItem
    {
        public int Id { get; set; }
        public int IdConsignacao { get; set; }
        public int IdProduto { get; set; }
        public int QuantidadeEnviada { get; set; }
        public int QuantidadeVendida { get; set; }
        public int QuantidadeDevolvida { get; set; }
        public decimal PrecoUnitarioAcordado { get; set; }
        public string NomeProduto { get; set; }
        public int SugestaoDevolucao => QuantidadeEnviada - QuantidadeVendida;
    }
}
