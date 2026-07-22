export interface Emprestimo {
  id: number;
  livroId: number;
  usuarioId: number;
  dataEmprestimo: string;
  dataDevolucaoPrevista: string;
  dataDevolucaoReal: string | null;
}