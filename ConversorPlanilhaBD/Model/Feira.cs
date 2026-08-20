using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    public class Feira
    {
        //Guarda o id e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Guarda oo nome da feira
        public string? Nome { get; set; }

        //Municipal, estadual, escolar etc
        public string? Alcance { get; set; }

        //Guarda o endereco da feira
        public string? Endereco { get; set; }

        //Guarda o estado da feira,
        //o construtor so valida o estado se for BR
        public string? Estado { get; set; }

        //Ex: anual 
        public string? PeriodoRealizacao { get; set; }

        //Guarda vai ser realizado
        //ex: 21-26 outubro
        public string? DataRealizacao { get; set; }

        //Grupo ou individual
        public string? ModalidadeParticipacao { get; set; }

        ////Guarda o numero de projetos a ser apresentado
        public int? NumProjetos { get; set; }

        //Ex:Ciencias exatas
        public string? AreasConhecimento { get; set; }

        //Ex:EnsinoMedio
        public string? NivelEnsino { get; set; }

        //Guarda o numero de escolas participantes
        public int? NumEscolas { get; set; }

        //Se feira é afiliada a alguma feira
        public string? Afiliada { get; set; }

        //Guarda o processo de selecao
        public string? ProcessoSelecao { get; set; }

        //Guarda o periodo de elaboracao da feira
        public string? PeriodoElaboracao { get; set; }

        //Se os projetos são avaliados durante a feira
        public string? ProjetosAvaliados { get; set; }

        //Data e Hora que a feira foi registrada
        public DateTime? DataHora { get; set; }

        /// <summary>
        /// Guarda uma lista de projetos
        /// Relação (1 feira: N projetos)
        /// Projetos podem ou não estar associados a uma feira
        /// </summary>
        public List<Projeto> Projetos { get; set; } = new();


        /// <summary>
        /// Uma feira possui dois tipos de instituicao
        /// Uma instituicao do responsavel que criou a feira na planilha
        /// e uma instituicao onde vai ser realizada
        /// Essas duas propriedades guardam isso
        /// Relação (1 instituicao: N feiras)
        /// </summary>
        public int? InstituicaoId { get; set; }
        [ForeignKey("InstituicaoId")]
        public Instituicao? Instituicao { get; set; }
        public int? InstituicaoOrganizadoraId { get; set; }
        [ForeignKey("InstituicaoOrganizadoraId")]
        public Instituicao? InstituicaoOrganizadora { get; set; }

        /// <summary>
        /// Uma feira possui dois tipos de Responsável
        /// Um responsável que criou a feira na planilha
        /// e um responsável para contato
        /// Essas duas propriedades guardam isso
        /// Relação (1 responsavel: N feiras)
        /// </summary>
        public int? ResponsavelId { get; set; }
        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }
        public int? ResponsavelContatoId { get; set; }
        [ForeignKey("ResponsavelContatoId")]
        public Responsavel? ResponsavelContato { get; set; }

        //Cria um construtor vazio, para o EF CORE funcionar
        private Feira() { }

        public Feira(string? nome, string? alcance, string? endereco, string? estado,
            string? periodoRealizacao, string? dataRealizacao, string? modalidadeParticipacao,
            int? numProjetos, string? areasConhecimento, string? nivelEnsino,
            int? numEscolas, string? afiliada, string? processoSelecao,
            string? periodoElaboracao, string? projetosAvaliados, DateTime? dataHora)
        {
            Nome = nome;
            Alcance = alcance;
            Endereco = endereco;
            Estado = estado;
            PeriodoRealizacao = periodoRealizacao;
            DataRealizacao = dataRealizacao;
            ModalidadeParticipacao = modalidadeParticipacao;
            NumProjetos = numProjetos;
            AreasConhecimento = areasConhecimento;
            NivelEnsino = nivelEnsino;
            NumEscolas = numEscolas;
            Afiliada = afiliada;
            ProcessoSelecao = processoSelecao;
            PeriodoElaboracao = periodoElaboracao;
            ProjetosAvaliados = projetosAvaliados;
            DataHora = dataHora;
        }
    }
}