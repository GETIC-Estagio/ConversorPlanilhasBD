using ConversorPlanilhaBD.Enums;
using ConversorPlanilhaBD.Model.RegisterModels.PessoaModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.RegisterModels
{
    /// <summary>
    /// Guarda uma feira, que é um evento onde projetos são apresentados e avaliados.
    /// </summary>
    public class Feira : ModelCadastro
    {
        public string Nome { get; set; } = null!;

        public EnumAlcance Alcance { get; set; }

        public string Endereco { get; set; } = null!;

        //Caso a feira seja Nacional
        public EnumEstadosBrasileiros EstadoBr { get; set; }

        //Caso a feira seja Internacional
        public string? EstadoInternacional { get; set; }

        public EnumPeriodoRealizacao PeriodoRealizacao { get; set; }

        //Guarda quando vai ser realizado
        //ex: 21-26 outubro
        public string DataRealizacao { get; set; } = null!;

        public EnumModalidadeParticipacao ModalidadeParticipacao { get; set; }

        ////Guarda o numero de projetos a ser apresentado
        public int? NumProjeto { get; set; }

        public EnumAreasConhecimento AreasConhecimento { get; set; }

        public EnumNivelEnsino NivelEnsino { get; set; }

        //Guarda o numero de escolas participantes
        public int? NumEscola { get; set; }

        //Se feira é afiliada a alguma outra feira
        public string? Afiliada { get; set; }

        //Guarda o processo de selecao dos projetos
        public string? ProcessoSelecao { get; set; }

        //Guarda o periodo de elaboracao dos projetos feira
        public string? PeriodoElaboracao { get; set; }

        //Se os projetos são avaliados durante a feira
        //E se sim, como são avaliados
        public string? ProjetoAvaliado { get; set; }

        //Data e Hora que a feira foi registrada
        public DateTime DataHora { get; set; }

        // Guarda uma lista de projetos
        // Relação (1 feira: N projetos)
        // Projetos podem ou não estar associados a uma feira
        public List<Projeto> Projetos { get; set; } = new();

        // Uma feira possui dois tipos de instituicao
        // Uma instituicao do responsavel que criou a feira na planilha
        // e uma instituicao onde vai ser realizada
        // Essas duas propriedades guardam isso
        // Relação (1 instituicao: N feiras)
        public int? InstituicaoId { get; set; }
        [ForeignKey("InstituicaoId")]
        public Instituicao? Instituicao { get; set; }
        public int? InstituicaoOrganizadoraId { get; set; }
        [ForeignKey("InstituicaoOrganizadoraId")]
        public Instituicao? InstituicaoOrganizadora { get; set; }

        // Uma feira possui dois tipos de Responsável
        // Um responsável que criou a feira na planilha
        // e um responsável para contato
        // Essas duas propriedades guardam isso
        // Relação (1 responsavel: N feiras)
        public int? ResponsavelId { get; set; }
        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }
        public int? ResponsavelContatoId { get; set; }
        [ForeignKey("ResponsavelContatoId")]
        public Responsavel? ResponsavelContato { get; set; }

        public Feira() { }

        public Feira(string sNome,  string sEndereco, string? sEstadoInt,
            string sDataRealizacao, int? iNumProjeto, int? iNumEscola, string? sAfiliada,
            string? sProcessoSelecao, string? sPeriodoElaboracao, string? sProjetoAvaliado, DateTime dtDataHora)
        {
            Nome = sNome;
            Endereco = sEndereco;
            EstadoInternacional = sEstadoInt;
            DataRealizacao = sDataRealizacao;
            NumProjeto = iNumProjeto;
            NumEscola = iNumEscola;
            Afiliada = sAfiliada;
            ProcessoSelecao = sProcessoSelecao;
            PeriodoElaboracao = sPeriodoElaboracao;
            ProjetoAvaliado = sProjetoAvaliado;
            DataHora = dtDataHora;
        }
    }
}