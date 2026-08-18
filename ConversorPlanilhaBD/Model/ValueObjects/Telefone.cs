using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.ValueObjects
{
    public class Telefone
    {
        //Guarda o id e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Guarda o numero de telefone
        public string? Numero { get; set; }

        //Guarda a chave estrangeira de pessoa
        public int? PessoaId { get; set; }
        [ForeignKey("PessoaId")]
        public Pessoa? Pessoa { get; set; }

        //Guarda a chave estrangeira de instituição
        public int? InstituicaoId { get; set; }
        [ForeignKey("InstituicaoId")]
        public Instituicao? Instituicao { get; set; }

        //Construtor default para o EF CORE pegar
        protected Telefone() { }

        public Telefone(string? numero)
        {
            Numero = numero;
        }
    }
}
