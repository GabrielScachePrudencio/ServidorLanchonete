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
        [Description("Em Andamento")]
        EmAndamento,

        [Description("Concluído")]
        Concluido,

        [Description("Cancelado")]
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

}
