using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_LANCHES.model
{
    public class Consignacao
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public int IdStatus { get; set; } // 1: Aberto, 2: Finalizado, 3: Cancelado
        public DateTime DataSaida { get; set; } = DateTime.Now;
        public DateTime? DataPrevisaoAcerto { get; set; }
        public decimal ValorTotalEstimado { get; set; }
        public string Observacao { get; set; }

        // Propriedades auxiliares para a UI (não estão na tabela principal, mas via JOIN)
        public string NomeCliente { get; set; }
        public string? NomeStatus { get; set; }
        public List<ConsignacaoItem> Itens { get; set; } = new List<ConsignacaoItem>();
    }
}
