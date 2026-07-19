using Microsoft.EntityFrameworkCore;
using Lib_SeekApi.Models; 

namespace Lib_SeekApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Livro> Livros { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<Multa> Multas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Livro>(entity =>
            {
                entity.ToTable("livros");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Autor).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Isbn).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AnoPublicacao).IsRequired();
                entity.Property(e => e.QuantidadeEstoque).IsRequired();
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.Ativo).HasDefaultValue(true);
            });

            modelBuilder.Entity<Emprestimo>(entity =>
            {
                entity.ToTable("emprestimos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataEmprestimo).IsRequired();
                entity.Property(e => e.DataDevolucaoPrevista).IsRequired();
                entity.Property(e => e.DataDevolucaoReal).IsRequired(false);
                entity.HasOne(e => e.Livro)
                    .WithMany(l => l.Emprestimos)
                    .HasForeignKey(e => e.LivroId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Emprestimos)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Multa>(entity =>
            {
                entity.ToTable("multas");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Valor)
                      .IsRequired()
                      .HasColumnType("decimal(10,2)");
                entity.Property(m => m.Status).IsRequired();
                entity.Property(m => m.DataGeracao).IsRequired();
                entity.Property(m => m.DataPagamento).IsRequired(false);
                entity.HasOne(m => m.Emprestimo)
                      .WithOne(e => e.Multa)
                      .HasForeignKey<Multa>(m => m.EmprestimoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}