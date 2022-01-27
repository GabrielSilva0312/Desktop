using Desktop.Infra.Repository;
using Desktop.Infra.ViewModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infra
{
    internal class Ambiente
    {
        public static string ConnectionStringBD;

        public static void TestarConexao()
        {
            var Conexao = ObterConexao();
        }

        public static string PastaGravacaoArquivos { get; set; }

        public static bool IniciarNS = false;
        public static bool BalancaAutomatica = false;

        public static UsuarioViewModel DadosUsuarioLogado { get; set; }

        public static bool SolicitarSenhaPermissoesInternas = false;

        public static string DadosConexaoBD
        {
            get
            {
                return string.Format("Servidor: {0} BD: {1}", _CN.DataSource, _CN.Database);
            }
        }

        public static int PortadorEstacaoId { get; set; }

        public static string SenhaDescriptografada { get; set; }

        private static MySqlConnection _CN;

        public static string VersaoApp { get; set; }

        public static MySqlTransaction Trans;
        public static TransactionRepositoryBase TransactionRepositoryBase;

        public static MySqlConnection ObterConexao()
        {
            if (_CN == null)
            {
                _CN = new MySqlConnection(ConnectionStringBD);
                try
                {
                    _CN.Open();
                }
                catch (Exception pEx)
                {
                    throw new Exception("Falha na conexão com o banco de dados.\n\n" + pEx.Message);
                }
            }
            else
            {
                if (!_CN.Ping())
                {
                    _CN = new MySqlConnection(ConnectionStringBD);
                    _CN.Open();
                }
            }

            return _CN;
        }
    }
}