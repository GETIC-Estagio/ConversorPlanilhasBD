using ConversorPlanilhaBD.Model.RegisterModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.ValueObjects
{
    /// <summary>
    /// Guarda o endereço de email de uma pessoa ou instituição.
    /// </summary>
    public class Email : ModelBase
    {
        //Guarda o endereco de email
        public string? Endereco { get; set; }

        //Guarda a chave estrangeira de pessoa
        public int? PessoaId { get; set; }
        [ForeignKey("PessoaId")]
        public Pessoa? Pessoa { get; set; }

        //Guarda a chave estrangeira de instituiçao
        public int? InstituicaoId { get; set; }
        [ForeignKey("InstituicaoId")]
        public Instituicao? Instituicao { get; set; }

        protected Email() { }

        public Email(string? endereco)
        {
            Endereco = endereco;
        }
    }
}
