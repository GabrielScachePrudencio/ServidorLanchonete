using PDV_LANCHES.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PDV_LANCHES.model
{
    public class Estoque
    {
        public int Id { get; set; }

        public int IdProduto { get; set; }

        public int Quantidade { get; set; }

        public DateTime UltimaAtualizacao { get; set; }

    }
}

