using ConversorPlanilhaBD.Model.RegisterModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.ValueObjects
{
    /// <summary>
    /// Guarda os dados de telefone de uma pessoa ou instituição
    /// </summary>
    public class Telefone : ModelBase
    {
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

        protected Telefone() { }

        public Telefone(string? numero)
        {
            Numero = numero;
        }
    }
}
