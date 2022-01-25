using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infra.Repository
{
    internal class RepositoryBase
    {
        protected MySqlTransaction _Trans = Ambiente.Trans;
        protected MySqlConnection _CN;

        public RepositoryBase()
        {
            _CN = Ambiente.ObterConexao();
        }

        public int ObterProximo(string pCampo, string pTabela)
        {
            string SQL = "SELECT IFNULL(MAX(" + pCampo + "), 1) + 1 FROM " + pTabela;

            var data = _CN.Query<int>(SQL, null, Ambiente.Trans).SingleOrDefault();

            return data;
        }

        public int ObterProximoClassificacao()
        {
            string SQL = "SELECT IFNULL(MAX(Id), 1) + 1 FROM Classificacao WHERE Id >= 10000";

            var data = _CN.Query<int>(SQL, null, Ambiente.Trans).SingleOrDefault();

            return data;
        }

        public int ObterProximo(string pTabela)
        {
            return ObterProximo("Id", pTabela);
        }

        public int ObterLastInsertId()
        {
            string SQL = "SELECT LAST_INSERT_ID()";

            var data = _CN.Query<int>(SQL, null, Ambiente.Trans).SingleOrDefault();

            return data;
        }

        public int ObterProximo(string pCampo, string pTabela, int pEmpresa)
        {
            string SQL = "SELECT ISNULL(MAX(" + pCampo + "), 0) + 1 FROM " + pTabela + " WHERE EmpresaId = " + pEmpresa;

            var data = _CN.Query<int>(SQL, null, Ambiente.Trans).SingleOrDefault();

            return data;
        }

        public void IniciarTransacao()
        {
            if (Ambiente.TransactionRepositoryBase == null)
                Ambiente.TransactionRepositoryBase = new TransactionRepositoryBase();

            Ambiente.TransactionRepositoryBase.ContTransacoes++;

            if (Ambiente.TransactionRepositoryBase.ContTransacoes == 1)
                Ambiente.Trans = null; // Forçar o NULL para 2 chamadas distintas no controller

            if (Ambiente.Trans == null)
            {
                Ambiente.Trans = (_CN.BeginTransaction(IsolationLevel.ReadCommitted) as MySqlTransaction);
                Ambiente.TransactionRepositoryBase.StatusTransacaoBD = StatusTransacaoBD.Iniciada;
                Ambiente.TransactionRepositoryBase.Trans = Ambiente.Trans;
            }
        }

        public void ConfirmarTransacao()
        {
            Ambiente.TransactionRepositoryBase.ContTransacoes--;

            if (Ambiente.TransactionRepositoryBase.ContTransacoes == 0)
            {
                if (Ambiente.Trans != null && Ambiente.TransactionRepositoryBase.StatusTransacaoBD != StatusTransacaoBD.Confirmada)
                {
                    Ambiente.Trans.Commit();
                    Ambiente.TransactionRepositoryBase.StatusTransacaoBD = StatusTransacaoBD.Confirmada;
                }
            }
        }

        public void CancelarTransacao()
        {
            if (Ambiente.Trans != null && Ambiente.TransactionRepositoryBase.StatusTransacaoBD != StatusTransacaoBD.Cancelada)
            {
                if (Ambiente.TransactionRepositoryBase.ContTransacoes == 0)
                    return;

                Ambiente.Trans.Rollback();
                Ambiente.TransactionRepositoryBase.ContTransacoes = 0;

                Ambiente.TransactionRepositoryBase.StatusTransacaoBD = StatusTransacaoBD.Cancelada;
            }
        }
    }

    public class TransactionRepositoryBase
    {
        public TransactionRepositoryBase()
        {
            StatusTransacaoBD = StatusTransacaoBD.Indefinido;
            ContTransacoes = 0;
        }

        public int ContTransacoes { get; set; }
        public StatusTransacaoBD StatusTransacaoBD { get; set; }
        public MySqlTransaction Trans { get; set; }
    }

    public enum StatusTransacaoBD
    {
        Iniciada,
        Confirmada,
        Cancelada,
        Indefinido
    }
}