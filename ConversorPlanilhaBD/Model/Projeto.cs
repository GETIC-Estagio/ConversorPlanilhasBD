using ConversorPlanilhaBD.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    public class Projeto
    {

        //Guarda o id e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Se houver inclusão de pessoas com deficiência
        public string? Deficiencia { get; set; }

        //Se a participação vai ser remota ou presencial
        public string? Participacao { get; set; }

        //Ex: Fundamental I, Ensino Médio etc
        public string? CategoriaInscricao { get; set; }

        //Data e Hora que o projeto foi registrado
        public DateTime DataHora { get; set; }

        //Guarda o Nome do projeto
        public string? NomeProjeto { get; set; }

        //Guarda as palavras chave referentes ao projeto
        public string? PalavrasChave { get; set; }

        //Com qual Objetivo de Desenvolvimento Sustentável (ODS) seu projeto mais se relaciona?
        public string? ODS { get; set; }

        //Guarda o tema do projeto
        public string? Tema { get; set; }

        //Guarda a area do projeto
        public string? Area { get; set; }

        //Guarda o objetivo do projeto
        public string? Objetivo { get; set; }

        //Guarda o resumo do projeto
        public string? Resumo { get; set; }

        /// <summary>
        /// Guarda uma lista de alunos que são da classe pessoa
        /// Relação (1 Projeto: N alunos)
        /// </summary>
        public List<Pessoa> Alunos { get; set; } = new();

        /// <summary>
        /// Guarda a chave estrangeira de responsáveis
        /// Relação (1 responsavel: N projetos)
        /// </summary>
        public int? ResponsavelId { get; set; }
        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }

        /// <summary>
        /// Guarda a chave estrangeira de professor
        /// Relação (1 professor: N projetos)
        /// </summary>
        public int? ProfessorId { get; set; }
        [ForeignKey("ProfessorId")]
        public Professor? Professor { get; set; }

        /// <summary>
        /// Guarda a chave estrangeira de feira
        /// Relação (1 feira: N projetos)
        /// </summary>
        public int? FeiraId { get; set; }
        [ForeignKey("FeiraId")]
        public Feira? Feira { get; set; }

        //Cria um construtor vazio, para o EF CORE funcionar
        protected Projeto() { }

        public Projeto(string? deficiencia, string? participacao, string? categoriaInscricao, 
            DateTime dataHora, string? nomeProjeto, string? palavrasChave, string? ods,
            string? tema, string? area, string? objetivo, string? resumo)
        {
            Deficiencia = deficiencia;
            Participacao = participacao;
            CategoriaInscricao = categoriaInscricao;
            DataHora = dataHora;
            NomeProjeto = nomeProjeto;
            PalavrasChave = palavrasChave;
            ODS = ods;
            Tema = tema;
            Area = area;
            Objetivo = objetivo;
            Resumo = resumo;
        }
    }
}
