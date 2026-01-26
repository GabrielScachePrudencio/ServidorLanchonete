using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorLanches.model
{
    class enums
    {


    }

    public enum TipoCardapio
    {

        [Description("Lanche")]
        Lanche,

        [Description("Bebida")]
        Bebida,

        [Description("Sobremesa")]
        Sobremesa


    }
    public enum StatusPedido
    {
        Preparando,

        Entregue,

        Pendente,

        Cancelado
    }

    enum EtapaPedido
    {
        InformarCpf,
        SelecionarItens,
        ConfirmarPedido
    }

    public enum TipoUsuario
    {
        Vendedor,
        Gerente,
        Administrador,

    }
     public enum TipoMovimentacaoEstoque
    {
        NENHUMA = 0,
        ENTRADA = 1,
        SAIDA = 2
    }
    public enum OrigemMovimentacaoEstoque
    {
        VENDA = 1,
        CANCELAMENTO = 2,
        AJUSTE_MANUAL = 3,
        DEVOLUCAO = 4
    }


}
