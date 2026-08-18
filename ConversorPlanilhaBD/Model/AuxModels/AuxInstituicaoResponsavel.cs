using ConversorPlanilhaBD.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.AuxModels
{
    public class AuxInstituicaoResponsavel
    {
        //Guarda o id e auto incrementa quando for para o banco
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        // Construtor obrigatório do EF Core
        protected AuxInstituicaoResponsavel() { }

        public AuxInstituicaoResponsavel(Responsavel? responsavel, Instituicao? instituicao, 
            string? funcaoInsituicao)
        {
            Responsavel = responsavel;
            Instituicao = instituicao;

            FuncaoInstituicao = funcaoInsituicao;
        }
    }
}