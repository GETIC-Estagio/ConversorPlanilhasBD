using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ConversorPlanilhaBD.Data
{
    public class CienciaJovemDb : DbContext
    {
        public CienciaJovemDb(DbContextOptions<CienciaJovemDb> options) : base(options) { }

        // DbSets para todas as Entidades do Sistema
        public DbSet<Pessoa> Pessoas { get; set; } = null!;
        public DbSet<Responsavel> Responsaveis { get; set; } = null!;
        public DbSet<Professor> Professores { get; set; } = null!;
        public DbSet<Instituicao> Instituicoes { get; set; } = null!;
        public DbSet<Feira> Feiras { get; set; } = null!;
        public DbSet<Projeto> Projetos { get; set; } = null!;
        public DbSet<Email> Emails { get; set; } = null!;
        public DbSet<Identidade> Identidades { get; set; } = null!;
        public DbSet<Telefone> Telefones { get; set; } = null!;
        public DbSet<AuxInstituicaoResponsavel> AuxInstituicoesResponsaveis { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================================
            // 1. CONFIGURAÇÃO DE HERANÇA - TPT (Table-per-Type)
            // Cria as tabelas separadas no Banco
            // ========================================================
            modelBuilder.Entity<Pessoa>().ToTable("Pessoas");
            modelBuilder.Entity<Responsavel>().ToTable("Responsaveis");
            modelBuilder.Entity<Professor>().ToTable("Professores");


            // ========================================================
            // 2. RELACIONAMENTOS DE PESSOA 
            // Responsável e Professor herdam estas relações
            // ========================================================

            // PESSOA -> TELEFONES (1:N)
            modelBuilder.Entity<Pessoa>()
                .HasMany(p => p.Telefone)
                .WithOne(t => t.Pessoa)
                .HasForeignKey(t => t.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);

            // PESSOA -> EMAILS (1:N)
            modelBuilder.Entity<Pessoa>()
                .HasMany(p => p.Email)
                .WithOne(e => e.Pessoa)
                .HasForeignKey(e => e.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);

            // PESSOA -> IDENTIDADES (1:N)
            modelBuilder.Entity<Pessoa>()
                .HasMany(p => p.Identidade)
                .WithOne(i => i.Pessoa)
                .HasForeignKey(i => i.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========================================================
            // 3. RELACIONAMENTOS DE INSTITUIÇÃO
            // ========================================================

            // INSTITUIÇÃO -> TELEFONES (1:N)
            modelBuilder.Entity<Instituicao>()
                .HasMany(i => i.Telefone)
                .WithOne(t => t.Instituicao)
                .HasForeignKey(t => t.InstituicaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // INSTITUIÇÃO -> EMAILS (1:N)
            modelBuilder.Entity<Instituicao>()
                .HasMany(i => i.Email)
                .WithOne(e => e.Instituicao)
                .HasForeignKey(e => e.InstituicaoId)
                .OnDelete(DeleteBehavior.Cascade);


            // ========================================================
            // 4. RELACIONAMENTOS DA FEIRA
            // ========================================================

            // FEIRA -> INSTITUIÇÃO SEDE (1:N)
            modelBuilder.Entity<Feira>()
                .HasOne(f => f.Instituicao)
                .WithMany(i => i.Feiras)
                .HasForeignKey(f => f.InstituicaoId)
                .OnDelete(DeleteBehavior.SetNull);

            // FEIRA -> INSTITUIÇÃO ORGANIZADORA (1:N mapeando a lista de retorno)
            modelBuilder.Entity<Feira>()
                .HasOne(f => f.InstituicaoOrganizadora)
                .WithMany(i => i.FeirasOrganizadas)
                .HasForeignKey(f => f.InstituicaoOrganizadoraId)
                .OnDelete(DeleteBehavior.SetNull);

            // FEIRA -> RESPONSÁVEL PRINCIPAL (1:N)
            modelBuilder.Entity<Feira>()
                .HasOne(f => f.Responsavel)
                .WithMany(r => r.Feiras)
                .HasForeignKey(f => f.ResponsavelId)
                .OnDelete(DeleteBehavior.SetNull);

            // FEIRA -> RESPONSÁVEL CONTATO (1:N mapeando a lista de retorno)
            modelBuilder.Entity<Feira>()
                .HasOne(f => f.ResponsavelContato)
                .WithMany(r => r.FeirasContato)
                .HasForeignKey(f => f.ResponsavelContatoId)
                .OnDelete(DeleteBehavior.SetNull);

            // ========================================================
            // 5. TABELA AUXILIAR
            // ========================================================
            modelBuilder.Entity<AuxInstituicaoResponsavel>()
                .HasOne(a => a.Responsavel)
                .WithMany(r => r.AuxInstituicaoResponsavel)
                .HasForeignKey(a => a.ResponsavelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuxInstituicaoResponsavel>()
                .HasOne(a => a.Instituicao)
                .WithMany(i => i.AuxInstituicaoResponsavel)
                .HasForeignKey(a => a.InstituicaoId)
                .OnDelete(DeleteBehavior.Cascade);


            // ========================================================
            // 6. RELACIONAMENTOS DE PROJETO
            // ========================================================

            // PROJETO -> RESPONSÁVEL (1:N)
            modelBuilder.Entity<Projeto>()
                .HasOne(p => p.Responsavel)
                .WithMany(r => r.Projetos)
                .HasForeignKey(p => p.ResponsavelId)
                .OnDelete(DeleteBehavior.SetNull);

            // PROJETO -> PROFESSOR (1:N)
            modelBuilder.Entity<Projeto>()
                .HasOne(p => p.Professor)
                .WithMany(prof => prof.Projetos)
                .HasForeignKey(p => p.ProfessorId)
                .OnDelete(DeleteBehavior.SetNull);

            // PROJETO -> ALUNOS (1:N)
            modelBuilder.Entity<Projeto>()
                .HasMany(p => p.Alunos)
                .WithOne(aluno => aluno.Projeto)
                .HasForeignKey(aluno => aluno.ProjetoId)
                .OnDelete(DeleteBehavior.SetNull); // SetNull impede que deletar o projeto apague o cadastro físico do aluno
        }
    }
}