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
        NAO_DEFINIDO = 0,
        VENDA = 1,
        COMPRA = 2,
        AJUSTE = 3,       // Adicionado para bater com o banco
        CANCELAMENTO = 4, // Adicionado para bater com o banco
        ESTORNADO = 5,
        PRONTO = 6,
        CONSIGNACAO_ABERTA = 7,
        CONSIGNACAO_CANCELADA = 8
    }


}
