using Desktop.Infra;
using Desktop.Infra.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop
{
    public partial class FormLogin : Form
    {
        public bool LoginOK { get; set; }

        public FormLogin()
        {
            InitializeComponent();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void FormLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && txtLogin.Focused)
            {
                txtSenha.Focus();
                return;
            }

            if (e.KeyCode == Keys.Enter && txtSenha.Focused)
                cmdConfirmar.PerformClick();

            if (e.KeyCode == Keys.Escape && txtLogin.Focused)
            {
                Close();
                return;
            }

            if (e.KeyCode == Keys.Escape && txtSenha.Focused)
                txtLogin.Focus();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
        }

        private void cmdConfirmar_Click(object sender, EventArgs e)
        {
            if (!VerificarCampos())
                return;

            var UsuarioRep = new UsuarioRepository();
            var DadosUsuario = UsuarioRep.RetornarDadosUsuarioPorLoginESenha(txtLogin.Text.Trim(), txtSenha.Text.Trim());

            if (DadosUsuario == null)
            {
                MessageBox.Show("Usuário não encontrato ou inativo", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Ambiente.SenhaDescriptografada = txtSenha.Text.Trim();
            Ambiente.DadosUsuarioLogado = DadosUsuario;

            LoginOK = true;

            Close();
        }

        private bool VerificarCampos()
        {
            string Msg = "";

            if (string.IsNullOrWhiteSpace(txtLogin.Text))
                Msg += "Entre com o Usuário\n";

            if (String.IsNullOrWhiteSpace(txtSenha.Text))
                Msg += "Entre com a Senha\n";

            if (!string.IsNullOrEmpty(Msg))
            {
                MessageBox.Show("Verifique os campos:\n\n" + Msg, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtLogin.Focus();
                txtLogin.SelectAll();
            }

            return Msg == "";
        }

        private void FormLogin_Enter(object sender, EventArgs e)
        {
            txtLogin.SelectAll();
            txtSenha.SelectAll();
        }
    }
}