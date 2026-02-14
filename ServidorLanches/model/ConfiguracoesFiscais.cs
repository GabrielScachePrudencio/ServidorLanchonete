namespace ServidorLanches.model
{
    public class ConfiguracoesFiscais
    {
        public int Id { get; set; }

        public string Cnpj { get; set; }
        public string InscricaoEstadual { get; set; }
        public string RegimeTributario { get; set; }

        public decimal AliquotaIcms { get; set; }
        public string Csosn { get; set; }

        public string CstPis { get; set; }
        public string CstCofins { get; set; }

        public string SerieNf { get; set; }
        public int NumeroUltimaNf { get; set; }
        public bool AmbienteProducao { get; set; }

        public string CaminhoCertificado { get; set; }
        public DateTime ValidadeCertificado { get; set; }
    }

}
