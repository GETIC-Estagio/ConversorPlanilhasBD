using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    /// <summary>
    /// Guarda uma classe de alunos, que são pessoas que fazem parte de um projeto
    /// </summary>
    public class Aluno : Pessoa
    {
        //Guarda a chave estrangeira de Projeto
        //Relação (1 Projeto: N alunos)
        public int? ProjetoId { get; set; }
        [ForeignKey("ProjetoId")]
        public Projeto? Projeto { get; set; }

        protected Aluno() { }

        public Aluno(string? nome, string? idGenero, string? raca)
            : base(nome, idGenero, raca) { }
    }
}
