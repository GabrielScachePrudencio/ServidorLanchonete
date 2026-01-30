namespace ServidorLanches.model
{
    public class DbConnectionManager
    {
        public string CurrentConnectionString { get; set; }

        public DbConnectionManager(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MySql");

            CurrentConnectionString = !string.IsNullOrEmpty(connectionString)
                                      ? connectionString
                                      : "";
        }
    }
}