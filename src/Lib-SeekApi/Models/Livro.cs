namespace Lib_SeekApi.Models
{
    public class Livro
    {
        public int Id { get; private set; }
        public string Titulo { get; private set; } = null!;
        public string Autor { get; private set; } = null!;
        public string Isbn { get; private set; } = null!;
        public int AnoPublicacao { get; private set; }
        public int QuantidadeEstoque { get; private set; }
        public IReadOnlyCollection<Emprestimo> Emprestimos { get; private set; } = new List<Emprestimo>();

        protected Livro() { }

        public Livro(string titulo, string autor, string isbn, int anoPublicacao, int quantidadeEstoque)
        {
            Titulo = titulo;
            Autor = autor;
            Isbn = isbn;
            AnoPublicacao = anoPublicacao;
            QuantidadeEstoque = quantidadeEstoque;
        }

        public void RegistrarSaida()
        {
            if(QuantidadeEstoque <= 0)
            {
                throw new InvalidOperationException("Não é possível emprestar: Livro sem estoque no momento.");
            }
            QuantidadeEstoque--;
        }

        public void RegistrarRetorno()
        {
            QuantidadeEstoque++;
        }

        public void AdicionarEstoque(int quantidade)
        {
            if(quantidade <= 0)
            {
                throw new ArgumentException("A quantidade adicionada deve ser maior que 0.");
            }

            QuantidadeEstoque += quantidade;
        }

        public void AtualizarDetalhes(string titulo, string autor, int anoPublicacao)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(autor))
            {
                throw new ArgumentException("Título e Autor são obrigatórios.");
            }

            Titulo = titulo;
            Autor = autor;
            AnoPublicacao = anoPublicacao;
        }
    }
}
