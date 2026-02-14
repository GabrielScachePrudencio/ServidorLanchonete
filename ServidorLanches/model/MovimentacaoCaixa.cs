using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorLanches.model
{
    public class MovimentacaoCaixa
    {
        public int id { get; set; }
        public int idCaixa { get; set; }
        public TipoMovimentacaoEstoque TipoMovimentacaoEstoque { get; set; }
        public decimal valor { get; set; }
        public OrigemMovimentacaoEstoque OrigemMovimentacaoEstoque { get; set; }
        public int idPedido { get; set; }
        public int idUsuario { get; set; }
        public string observacao { get; set; }
        public DateTime dataMovimentacao { get; set; }
    }
}
