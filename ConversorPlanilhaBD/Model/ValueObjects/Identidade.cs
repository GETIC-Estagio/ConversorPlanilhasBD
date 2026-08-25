using ConversorPlanilhaBD.Model.RegisterModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.ValueObjects
{
    /// <summary>
    /// Guarda os dados de identidade de uma pessoa, como CPF, RG e órgão expedidor.
    /// </summary>
    public class Identidade : ModelBase
    {
        public string? CPF { get; set; }

        //Guarda o orgao que expediu o RG
        public string? OrgaoExpedidor { get; set; }

        public string? RG { get; set; }

        //Guarda a chave estrangeira de pessoa
        public int? PessoaId { get; set; }
        [ForeignKey("PessoaId")]
        public Pessoa? Pessoa { get; set; }

        public Identidade() { }

        //Esse construtor é somente para o CPF
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
