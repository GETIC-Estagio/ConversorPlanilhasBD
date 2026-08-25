using ConversorPlanilhaBD.Enums;
using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model.RegisterModels
{
    /// <summary>
    /// Guarda os dados de uma instituição
    /// </summary>
    public class Instituicao : ModelCadastro
    {
        public string Nome { get; set; } = null!;

        public string CNPJ { get; set; } = null!;

        public string Pais { get; set; } = null!;

        //Caso seja Nacional
        public EnumEstadosBrasileiros EstadoBr { get; set; }

        //Caso seja Internacional
        public string? EstadoInternacional { get; set; }

        public string? Municipio { get; set; }

        public string Endereco { get; set; } = null!;

        public EnumTipoRede TipoRede { get; set; }

        //GRE (Gerência Regional de Educação) - Estado de Pernambuco
        public string? GRE { get; set; }

        //Indice de Desenvolvimento da Educação Básica (IDEB)
        public double? IDEB { get; set; }

        //Índice de Desenvolvimento Humano Municipal (IDHM)
        public double? IDHM { get; set; }

        // Se a instituicao ja foi participante do ciencia jovem e
        // se sim, em que anos e quantos projetos foram apresentados
        public string? ParticipacaoCienciaJovem { get; set; }

        public EnumOfertaEnsino OfertaEnsino { get; set; }

        //  A escola adere ao Programa Escola em Tempo Integral, iniciativa do Ministério da Educação(MEC)
        //  e coordenado pela Secretaria de Educação Básica(SEB)?
        public string? Adere { get; set; }

        public EnumTipologiaMunicipio TipologiaMunicipio { get; set; }

        //Se a instituição recebeu algum apoio financeiro para realizar a feira
        public EnumApoioFinanceiro ApoioFinanceiro { get; set; }

 
        // Guarda uma lista de AuxInstituicaoResponsavel
        // Por responsavel e instituicao ser uma relacao N:M
        // Foi criado essa classe auxiliar
        public List<AuxInstituicaoResponsavel> AuxInstituicaoResponsavel { get;   set; } = new();

        // Uma feira possui dois tipos de instituicao
        // Uma instituicao do responsavel que criou a feira na planilha
        // e uma instituicao onde vai ser realizada
        // Essas duas propriedades guardam isso
        // Relação (1 instituicao: N feiras)
        [InverseProperty("Instituicao")]
        public List<Feira> Feiras { get; set; } = new();

        [InverseProperty("InstituicaoOrganizadora")]
        public List<Feira> FeirasOrganizadas { get; set; } = new();

        //Guarda uma lista de Telefones
        public List<Telefone> Telefones { get; set; } = new();

        //Guarda uma lista de Emails
        public List<Email> Emails { get; set; } = new();

        //Cria um construtor vazio, para o EF CORE funcionar
        public Instituicao() { }

        public Instituicao (string sNome)
        {
            Nome = sNome;
        }

        public Instituicao(string sNome, string sCnpj, string sPais, string? sEstadoInt,
            string? sMunicipio, string sEndereco, string? sGre, double? dIdeb, double? dIdhm, 
            string? sParticipacaoCJ, string? sAdere) : this(sNome)
        {
            CNPJ = sCnpj;
            Pais = sPais;
            EstadoInternacional = sEstadoInt;
            Municipio = sMunicipio;
            Endereco = sEndereco;
            GRE = sGre;
            IDEB = dIdeb;
            IDHM = dIdhm;
            ParticipacaoCienciaJovem = sParticipacaoCJ;
            Adere = sAdere;
        }
    }
}
