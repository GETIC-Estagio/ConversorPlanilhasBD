using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.ValueObjects
{
    public class Identidade
    {
        //Guarda o id e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Guarda o numero da identidade
        public string? CPF { get; set; }

        //Guarda o orgao que expediu
        public string? OrgaoExpedidor { get; set; }

        //Guarda o RG
        public string? RG { get; set; }


        //Guarda a chave estrangeira de pessoa
        public int? PessoaId { get; set; }
        [ForeignKey("PessoaId")]
        public Pessoa? Pessoa { get; set; }

        //Construtor default para o EF CORE pegar
        protected Identidade() { }

        //Esse construtor é para o CPF
        public Identidade(string? cpf)
        {
            CPF = cpf;
        }

        //Se vier Órgão Expedidor é RG
        public Identidade(string? rg, string? orgaoExpeditor)
        {
            OrgaoExpedidor = orgaoExpeditor;
            RG = rg;
        }
    }
}
