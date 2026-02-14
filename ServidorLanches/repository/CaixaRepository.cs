using MySql.Data.MySqlClient;
using ServidorLanches.model;
using ServidorLanches.model.dto;

namespace ServidorLanches.repository
{
    public class CaixaRepository
    {
        private readonly DbConnectionManager _dbManager;

        public CaixaRepository(DbConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        private string GetConnectionString() => _dbManager.CurrentConnectionString;

        public List<TerminalCaixa> GetAllCaixasTerminais()
        {
            var lista = new List<TerminalCaixa>();
            using var conn = new MySqlConnection(GetConnectionString());

            try
            {
                conn.Open();

                var sql = "SELECT id, nome, status FROM Terminais_caixa";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new TerminalCaixa
                    {
                        id = reader.GetInt32("id"),
                        nome = reader.GetString("nome"),
                        status = reader.GetString("status")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar caixas: {ex.Message}");
            }

            return lista;
        }

        public List<Caixa> GetAllCaixas()
        {
            var lista = new List<Caixa>();

            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();

            string sql = @"SELECT * FROM caixa where status = 'FECHADO' ORDER BY id DESC";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Caixa
                {
                    id = reader.GetInt32("id"),

                    idUsuario = reader.GetInt32("id_usuario"),
                    idTerminal = reader.GetInt32("id_terminal"),

                    dataAbertura = reader.GetDateTime("data_abertura"),

                    dataFechamento = reader.IsDBNull(reader.GetOrdinal("data_fechamento"))
                        ? (DateTime?)null
                        : reader.GetDateTime("data_fechamento"),

                    status = reader.GetString("status"),

                    valorInicial = reader.GetDecimal("valor_inicial"),

                    valorFinal = reader.GetOrdinal("valor_final_informado"),

                    valor_calculado = reader.GetOrdinal("valor_calculado"),

                    diferença = reader.GetOrdinal("diferenca")
                });
            }

            return lista;
        }



        public Caixa IniciarCaixaAberto(Caixa caixa)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            try
            {
                conn.Open();
                var sql = @"INSERT INTO caixa (id_usuario, id_terminal, data_abertura, valor_inicial, status) 
                    VALUES (@idUsuario, @idTerminal, @dataAbertura, @valorInicial, @status);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idUsuario", caixa.idUsuario);
                cmd.Parameters.AddWithValue("@idTerminal", caixa.idTerminal);
                cmd.Parameters.AddWithValue("@dataAbertura", caixa.dataAbertura);
                cmd.Parameters.AddWithValue("@valorInicial", caixa.valorInicial);
                cmd.Parameters.AddWithValue("@status", "ABERTO");

                int idGerado = Convert.ToInt32(cmd.ExecuteScalar());

                caixa.id = idGerado;
                return caixa;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao iniciar caixa no banco: {ex.Message}");
            }
        }


        public Caixa FecharCaixa(Caixa caixaIncompleto, List<PedidoDTO> todosOsPedidosPeloIdCaixa)
        {
            decimal totalVendido = todosOsPedidosPeloIdCaixa.Sum(p => p.ValorTotal);

            caixaIncompleto.valor_calculado = caixaIncompleto.valorInicial + totalVendido;

            caixaIncompleto.valorFinal = caixaIncompleto.valorInicial + totalVendido;

            caixaIncompleto.diferença = caixaIncompleto.valorFinal - caixaIncompleto.valor_calculado;

            caixaIncompleto.dataFechamento = DateTime.Now;
            caixaIncompleto.status = "FECHADO";

            return caixaIncompleto;
        }
        public bool SalvarFecharCaixa(Caixa caixaCompleto)
        {
            Caixa c = updateCaixa(caixaCompleto);
            if (c == null){
                return false;
            }
            else
            {
                return true;
            }
        }


        public Caixa addCaixa(Caixa caixa)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            try
            {
                conn.Open();
                var sql = @"INSERT INTO caixa 
                (id_usuario, id_terminal, data_abertura, data_fechamento, 
                 valor_inicial, valor_calculado, valor_final_informado, diferenca, status) 
                VALUES 
                (@idUsuario, @idTerminal, @dataAbertura, @dataFechamento, 
                 @valorInicial, @valorCalculado, @valorFinal, @diferenca, @status);
                SELECT LAST_INSERT_ID();";


                using var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@idUsuario", caixa.idUsuario);
                cmd.Parameters.AddWithValue("@idTerminal", caixa.idTerminal);   
                cmd.Parameters.AddWithValue("@dataAbertura", caixa.dataAbertura);

                cmd.Parameters.AddWithValue("@dataFechamento", (object)caixa.dataFechamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@valorFinal", caixa.valorFinal);

                cmd.Parameters.AddWithValue("@valorInicial", caixa.valorInicial);
                cmd.Parameters.AddWithValue("@valorCalculado", caixa.valor_calculado);
                cmd.Parameters.AddWithValue("@diferenca", caixa.diferença);
                cmd.Parameters.AddWithValue("@status", caixa.status);

                caixa.id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao adicionar caixa: {ex.Message}");
            }
            return caixa;
        }

        public Caixa updateCaixa(Caixa caixa)
        {
            using var conn = new MySqlConnection(GetConnectionString());
            try
            {
                conn.Open();

                var sql = @"UPDATE caixa
                    SET
                        id_usuario = @idUsuario,
                        id_terminal = @idTerminal,
                        data_abertura = @dataAbertura,
                        data_fechamento = @dataFechamento,
                        valor_inicial = @valorInicial,
                        valor_calculado = @valorCalculado,
                        valor_final_informado = @valorFinal,
                        diferenca = @diferenca,
                        status = @status
                    WHERE id = @id;";

                using var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@id", caixa.id);
                cmd.Parameters.AddWithValue("@idUsuario", caixa.idUsuario);
                cmd.Parameters.AddWithValue("@idTerminal", caixa.idTerminal);
                cmd.Parameters.AddWithValue("@dataAbertura", caixa.dataAbertura);
                cmd.Parameters.AddWithValue("@dataFechamento", (object)caixa.dataFechamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@valorInicial", caixa.valorInicial);
                cmd.Parameters.AddWithValue("@valorCalculado", caixa.valor_calculado);
                cmd.Parameters.AddWithValue("@valorFinal", caixa.valorFinal);
                cmd.Parameters.AddWithValue("@diferenca", caixa.diferença);
                cmd.Parameters.AddWithValue("@status", caixa.status);

                cmd.ExecuteNonQuery(); // UPDATE = NonQuery
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar caixa: {ex.Message}");
            }

            return caixa;
        }


    }
}
