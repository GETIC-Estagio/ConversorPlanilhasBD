using ConversorPlanilhaBD.Model.RegisterModels;
using ConversorPlanilhaBD.Model.RegisterModels.PessoaModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.AuxModels
{
    public class AuxInstituicaoResponsavel : ModelBase
    {
        //Guarda a chave estrangeira de pessoa
        public int? ResponsavelId { get; set; }
        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }

        //Guarda a chave estrangeira de instituicao
        public int? InstituicaoId { get; set; }
        [ForeignKey("InstituicaoId")]
        public Instituicao? Instituicao { get; set; }

        //Guarda qual a funcao do responsavel na instituicao
        public string? FuncaoInstituicao { get; set; }

        public AuxInstituicaoResponsavel() { }

        public AuxInstituicaoResponsavel(Responsavel? responsavel, Instituicao? instituicao, 
            string? funcaoInsituicao)
        {
            Responsavel = responsavel;
            Instituicao = instituicao;

            FuncaoInstituicao = funcaoInsituicao;
        }
    }
}