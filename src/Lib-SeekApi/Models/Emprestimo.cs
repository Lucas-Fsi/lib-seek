namespace Lib_SeekApi.Models
{
    public class Emprestimo
    {
        public int Id { get; private set; }
        public int LivroId { get; private set; }
        public Livro Livro { get; private set; } = null!;
        public int UsuarioId { get; private set; }
        public Usuario Usuario { get; private set; } = null!;
        public DateTime DataEmprestimo { get; private set; }
        public DateTime DataDevolucaoPrevista { get; private set; }
        public DateTime? DataDevolucaoReal { get; private set; }
        public Multa? Multa { get; private set; }

        protected Emprestimo() { }

        public Emprestimo(int livroId, int usuarioId, int diasParaDevolucao = 7)
        {
            LivroId = livroId;
            UsuarioId = usuarioId;
            DataEmprestimo = DateTime.UtcNow;
            DataDevolucaoPrevista = DataEmprestimo.AddDays(diasParaDevolucao);
        }

        public void FinalizarEmprestimo(DateTime dataDevolucaoReal)
        {
            DataDevolucaoReal = dataDevolucaoReal;
        }

        public bool EstaAtrasado(DateTime dataAtual)
        {
            if (DataDevolucaoReal.HasValue)
            {
                return DataDevolucaoReal.Value > DataDevolucaoPrevista;
            }
            return dataAtual > DataDevolucaoPrevista;
        }

        public int CalcularDiasAtraso(DateTime dataAtual)
        {
            if (!EstaAtrasado(dataAtual)) return 0;

            DateTime dataDevolvidoOuAtual = DataDevolucaoReal ?? dataAtual;
            TimeSpan diferenca = dataDevolvidoOuAtual - DataDevolucaoPrevista;

            return diferenca.Days > 0 ? diferenca.Days : 0;
        }

        public void RenovarEmprestimo(int diasExtras = 7)
        {
            if (DataDevolucaoReal.HasValue)
            {
                throw new InvalidOperationException("Não é possível renovar um empréstimo já finalizado.");
            }

            DataDevolucaoPrevista = DataDevolucaoPrevista.AddDays(diasExtras);
        }
    }
}
