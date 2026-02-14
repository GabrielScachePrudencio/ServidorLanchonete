using ServidorLanches.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_LANCHES.model
{
    public class MovimentacaoEstoque
    {
        public int Id { get; set; }

        public int IdProduto { get; set; }

        public int idConsignacao { get; set; }
        public TipoMovimentacaoEstoque Tipo { get; set; }

        public int Quantidade { get; set; }

        public OrigemMovimentacaoEstoque Origem { get; set; }

        public int? IdPedido { get; set; }

        public int? IdUsuario { get; set; }

        public string Observacao { get; set; }

        public DateTime DataMovimentacao { get; set; }
    }
}
