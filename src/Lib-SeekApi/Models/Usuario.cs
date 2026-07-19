using System.Text.RegularExpressions; 

namespace Lib_SeekApi.Models
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string Telefone { get; private set; } = null!;
        public bool Ativo { get; private set; }
        public IReadOnlyCollection<Emprestimo> Emprestimos { get; private set; } = new List<Emprestimo>();

        protected Usuario() { }

        public Usuario(string nome, string email, string telefone)
        {
            ValidarEmail(email);

            Nome = nome;
            Email = email;
            Telefone = telefone;
            Ativo = true;
        }

        public void AtualizarDetalhes(string nome, string email, string telefone)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome é obrigatório.");
            }
        
            ValidarEmail(email);

            Nome = nome;
            Email = email;
            Telefone = telefone;
        }

        private void ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("O email não pode ser vazio.");
            }

            string padraoRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, padraoRegex))
            {
                throw new ArgumentException("O formato do email é inválido.");
            }
        }

        public void Inativar()
        {
            Ativo = false;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void AtualizarDados(string nome, string telefone)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome é obrigatório.");
            }
                
            Nome = nome;
            Telefone = telefone;
        }
    }
}