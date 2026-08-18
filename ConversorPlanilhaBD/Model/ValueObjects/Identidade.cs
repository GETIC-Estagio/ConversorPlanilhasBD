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
        public string? Numero { get; set; }

        //Guarda o orgao que expediu
        public string? OrgaoExpedidor { get; set; }

        //Guarda a chave estrangeira de pessoa
        public int? PessoaId { get; set; }
        [ForeignKey("PessoaId")]
        public Pessoa? Pessoa { get; set; }

        //Construtor default para o EF CORE pegar
        protected Identidade() { }

        //Na planilha várias vezes são enviados apenas o numero
        //Esse construtor é para isso
        public Identidade(string? numero)
        {
            Numero = numero;
        }

        //Esse construtor serve para se vier o orgão de expedição
        public Identidade(string? numero, string? orgaoExpeditor) : this(numero)
        {
            OrgaoExpedidor = orgaoExpeditor;
        }
    }
}
