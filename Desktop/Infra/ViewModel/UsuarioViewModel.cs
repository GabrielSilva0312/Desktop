using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infra.ViewModel
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }
        public string Nome { get; set; }
        public bool Ativo { get; set; }
        public bool TrocarSenha { get; set; }
        public bool AcessoTotal { get; set; }
    }

    public class UsuarioPermissaoViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PermissaoId { get; set; }
        public bool Selecionado { get; set; }

        public string NomeGrupo { get; set; }
        public string NomePermissao { get; set; }
    }
}