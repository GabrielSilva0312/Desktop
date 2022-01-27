using Dapper;
using Desktop.Infra.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infra.Repository
{
    internal class UsuarioRepository : RepositoryBase
    {
        public UsuarioViewModel RetornarDadosUsuarioPorLoginESenha(string pUsuario, string pSenha)
        {
            string SenhaMD5 = GerarMD5String(pSenha);

            var SQL = new Select()
                 .Campos("*")
                 .From("Usuario")
                 .Where("Login = @Login AND Senha = @Senha AND Ativo = 1");

            var data = _CN.Query<UsuarioViewModel>(SQL.ToString(), new { Login = pUsuario, Senha = SenhaMD5 }, _Trans).SingleOrDefault();

            return data;
        }

        public void AlterarSenhaUsuarioAtual(string pNovaSenha, string pSenhaAtual)
        {
            string SenhaNova = GerarMD5String(pNovaSenha);
            string SenhaAtual = GerarMD5String(pSenhaAtual);

            var SQLUS = new Select()
                 .Campos("Senha")
                 .From("Usuario")
                 .Where("Id = @Id");

            var data = _CN.Query<UsuarioViewModel>(SQLUS.ToString(), new { Id = Ambiente.DadosUsuarioLogado.Id }, _Trans).SingleOrDefault();

            if (data.Senha.ToString() != SenhaAtual)
                throw new Exception("Senha Atual inválida.");

            var Qtd = _CN.Query<int>(
                "SELECT COUNT(*) FROM Usuario WHERE Senha = @Senha AND Id <> @UsuarioAtualId",
                new { Senha = SenhaNova, UsuarioAtualId = Ambiente.DadosUsuarioLogado.Id }, _Trans).SingleOrDefault();
            if (Qtd > 0)
                throw new Exception("Senha inválida ou em uso");

            string SQL = "UPDATE Usuario SET Senha = @Senha, TrocarSenha = 0 WHERE Id = @Id";
            _CN.Execute(SQL, new { Senha = GerarMD5String(pNovaSenha), Id = Ambiente.DadosUsuarioLogado.Id }, _Trans);
        }

        public List<UsuarioViewModel> RetornarUsuariosParaBusca(string pBusca)
        {
            var SQL = new Select()
                   .Campos("*")
                   .From("Usuario")
                   .Where("Nome LIKE '%" + pBusca + "%'")
                   .OrderBy("Nome"); ;

            var data = _CN.Query<UsuarioViewModel>(SQL.ToString(), null, _Trans).ToList();

            return data;
        }

        public List<UsuarioPermissaoViewModel> RetornarPermissoesUsuario(int pId)
        {
            var SQL = new Select()
                 .Campos("P.Id, P.Grupo as NomeGrupo, P.Nome AS NomePermissao, " +
                 "IFNULL((SELECT CASE WHEN UP.Id IS NULL THEN 0 ELSE 1 END FROM UsuarioPermissao UP WHERE UP.PermissaoId = P.Id AND UP.UsuarioId = @UsuarioId), 0) AS Selecionado")
                 .From("Permissao P")
                 .OrderBy("P.Grupo");

            var data = _CN.Query<UsuarioPermissaoViewModel>(SQL.ToString(), new { UsuarioId = pId }, _Trans).ToList();

            return data;
        }

        public bool RetornarSeLoginEstaDisponivel(string pLogin, int pId)
        {
            Select SQL = new Select()
                .Campos("Id")
                .Limit(1)
                .From("Usuario")
                .Where("Login = @Login");

            var data = _CN.Query<dynamic>(SQL.ToString(),
                new { Login = pLogin }, _Trans).SingleOrDefault();

            if (data == null) // Disponível
                return true;

            return data.Id == pId;
        }

        public bool VerificarPermissao(int pPermissao)
        {
            if (Ambiente.DadosUsuarioLogado.AcessoTotal)
                return true;

            var SQL = new Select()
                   .Campos("Id")
                   .From("UsuarioPermissao")
                   .Where("UsuarioId = @UsuarioId AND PermissaoId = @PermissaoId");

            var data = _CN.Query<int>(SQL.ToString(), new { UsuarioId = Ambiente.DadosUsuarioLogado.Id, PermissaoId = pPermissao }, _Trans).SingleOrDefault();

            return data > 0;
        }

        public bool VerificarPermissaoIgnorandoAcessoTotal(int pPermissao)
        {
            var SQL = new Select()
                   .Campos("Id")
                   .From("UsuarioPermissao")
                   .Where("UsuarioId = @UsuarioId AND PermissaoId = @PermissaoId");

            var data = _CN.Query<int>(SQL.ToString(), new { UsuarioId = Ambiente.DadosUsuarioLogado.Id, PermissaoId = pPermissao }, _Trans).SingleOrDefault();

            return data > 0;
        }

        public UsuarioViewModel RetornarDadosUsuario(int pId)
        {
            var SQL = new Select()
                 .Campos("*")
                 .From("Usuario")
                 .Where("Id = @Id");

            var data = _CN.Query<UsuarioViewModel>(SQL.ToString(), new { Id = pId }, _Trans).SingleOrDefault();

            return data;
        }

        public void IncluirAlterarUsuario(UsuarioViewModel pUsuario)
        {
            IniciarTransacao();

            try
            {
                var SQL = new Select()
                    .Campos("Id")
                    .From("Usuario")
                    .Where("Id = @Id");

                var UsuarioId = _CN.Query<int>(SQL.ToString(), new { Id = pUsuario.Id }, _Trans).SingleOrDefault();

                if (UsuarioId == 0)
                {
                    string SenhaPadraoMD5 = GerarMD5String("123");
                    pUsuario.Senha = SenhaPadraoMD5;

                    string SQLIns = "INSERT INTO Usuario (Login, Senha, Nome, Ativo, TrocarSenha, AcessoTotal) VALUES (@Login, @Senha, @Nome, @Ativo, @TrocarSenha, @AcessoTotal)";
                    _CN.Execute(SQLIns, pUsuario);
                }
                else
                {
                    string SQLIns = "UPDATE Usuario SET Login = @Login, Nome = @Nome, Ativo = @Ativo, TrocarSenha = @TrocarSenha, AcessoTotal = @AcessoTotal WHERE Id = @Id";
                    _CN.Execute(SQLIns, pUsuario);
                }
            }
            catch (Exception pEx)
            {
                CancelarTransacao();
                throw pEx;
            }

            ConfirmarTransacao();
        }

        public void AlterarPermissoesUsuario(List<UsuarioPermissaoViewModel> pPermissoes, int pUsuarioId)
        {
            IniciarTransacao();

            try
            {
                string SQLDel = "DELETE FROM UsuarioPermissao WHERE UsuarioId = @UsuarioId";
                _CN.Execute(SQLDel, new { UsuarioId = pUsuarioId }, _Trans);

                foreach (var Item in pPermissoes)
                {
                    int Proximo = ObterProximo("UsuarioPermissao");

                    SQLDel = "INSERT INTO UsuarioPermissao (UsuarioId, PermissaoId) VALUES (@UsuarioId, @PermissaoId)";
                    _CN.Execute(SQLDel, new { Id = Proximo, UsuarioId = pUsuarioId, PermissaoId = Item.Id }, _Trans);
                }
            }
            catch (Exception pEx)
            {
                CancelarTransacao();
                throw pEx;
            }

            ConfirmarTransacao();
        }

        private string GerarMD5String(string pString)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(pString));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public void Excluir(int pId)
        {
            IniciarTransacao();

            try
            {
                string SQL = "DELETE FROM Usuario WHERE Id = " + pId;
                _CN.Execute(SQL, SQL, _Trans);
            }
            catch (Exception pEx)
            {
                CancelarTransacao();
                throw pEx;
            }

            ConfirmarTransacao();
        }
    }
}