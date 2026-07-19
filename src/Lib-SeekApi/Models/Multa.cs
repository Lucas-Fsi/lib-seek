using Lib_SeekApi.Enums;

namespace Lib_SeekApi.Models
{
    public class Multa
    {
        public int Id { get; private set; }
        public int EmprestimoId { get; private set; }
        public Emprestimo Emprestimo { get; private set; } = null!;
        public decimal Valor { get; private set; }
        public StatusMulta Status { get; private set; }
        public DateTime DataGeracao { get; private set; }
        public DateTime? DataPagamento { get; private set; }

        protected Multa() { }

        public Multa(int emprestimoId, decimal valor)
        {
            if (valor <= 0)
            {
                throw new ArgumentException("O valor da multa deve ser maior que zero.");
            }

            EmprestimoId = emprestimoId;
            Valor = valor;
            Status = StatusMulta.Pendente;
            DataGeracao = DateTime.UtcNow;
        }

        public void Pagar(DateTime dataPagamento)
        {
            if (Status == StatusMulta.Paga)
            {
                throw new InvalidOperationException("Esta multa já consta como paga.");
            }
            
            if (Status == StatusMulta.Cancelada)
            {
                throw new InvalidOperationException("Não é possível pagar uma multa que foi cancelada.");
            }

            Status = StatusMulta.Paga;
            DataPagamento = dataPagamento;
        }

        public void Cancelar()
        {
            if (Status == StatusMulta.Paga)
            {
                throw new InvalidOperationException("Não é possível cancelar uma multa que já foi paga.");
            }

            Status = StatusMulta.Cancelada;
        }

        public void AtualizarValor(decimal novoValor)
        {
            if (Status != StatusMulta.Pendente)
            {
                throw new InvalidOperationException("Só é possível alterar o valor de multas pendentes.");
            }

            if (novoValor <= 0)
            {
                throw new ArgumentException("O novo valor deve ser maior que zero.");
            }

            Valor = novoValor;
        }
    }
}