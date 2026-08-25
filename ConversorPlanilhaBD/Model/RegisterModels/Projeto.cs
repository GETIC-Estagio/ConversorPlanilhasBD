using ConversorPlanilhaBD.Enums;
using ConversorPlanilhaBD.Model.RegisterModels.PessoaModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.RegisterModels
{
    public class Projeto : ModelCadastro
    {
        public string Nome { get; set; } = null!;

        public EnumParticipacaoDeficiencia Deficiencia { get; set; }

        public EnumParticipacao Participacao { get; set; }

        //Ex: Fundamental I, Ensino Médio etc
        public EnumCategoriaInscricao CategoriaInscricao { get; set; }

        //Data e Hora que o projeto foi registrado
        public DateTime DataHora { get; set; }

        //Guarda as palavras chave referentes ao projeto
        public string? PalavrasChave { get; set; }

        //Com qual Objetivo de Desenvolvimento Sustentável (ODS) seu projeto mais se relaciona?
        public EnumODS ODS { get; set; }

        //Guarda o tema do projeto
        public EnumTema Tema { get; set; }

        public EnumAreasConhecimento Area { get; set; }

        //Guarda o objetivo do projeto
        public string Objetivo { get; set; } = null!;

        //Guarda o resumo do projeto
        public string Resumo { get; set; } = null!;

        // Guarda uma lista de alunos
        // Relação (1 Projeto: N alunos)
        public List<Aluno> Alunos { get; set; } = new();

        // Guarda a chave estrangeira de responsáveis
        // Relação (1 responsavel: N projetos)
        public int? ResponsavelId { get; set; }
        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }

        // Guarda a chave estrangeira de professor
        // Relação (1 professor: N projetos)
        public int? ProfessorId { get; set; }
        [ForeignKey("ProfessorId")]
        public Professor? Professor { get; set; }

        // Guarda a chave estrangeira de feira
        // Relação (1 feira: N projetos)
        public int? FeiraId { get; set; }
        [ForeignKey("FeiraId")]
        public Feira? Feira { get; set; }

        //Cria um construtor vazio, para o EF CORE funcionar
        protected Projeto() { }

        public Projeto(string sNome, DateTime dtDataHora, string? palavrasChave, string sObjetivo, string sResumo)
        {
            Nome = sNome;
            DataHora = dtDataHora;
            PalavrasChave = palavrasChave;
            Objetivo = sObjetivo;
            Resumo = sResumo;
        }
    }
}
