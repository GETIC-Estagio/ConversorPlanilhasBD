using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    public class Instituicao
    {
        //Guarda o id e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Guarda o nome da instituicao
        public string? Nome { get; set; }

        //Guarda o cnpj da instituicao
        public string? CNPJ { get; set; }

        //guarda o país
        public string? Pais { get; set; }
        
        //Guarda o estado
        public string? Estado { get; set; }

        //Guarda o municipio
        public string? Municipio { get; set; }

        //Guarda o endereco
        public string? Endereco { get; set; }

        //rede Pública ou Rede Privada etc
        public string? TipoRede { get; set; }

        //GRE (Gerência Regional de Educação) - Estado de Pernambuco
        public string? GRE { get; set; }

        //Indice de Desenvolvimento da Educação Básica (IDEB)
        public double? IDEB { get; set; }

        //Índice de Desenvolvimento Humano Municipal (IDHM)
        public double? IDHM { get; set; }

        // Se a instituicao ja foi participante do ciencia jovem
        //e detalhes relacionados
        public string? Participante { get; set; }

        //Integral, regular etc
        public string? OfertaEnsino { get; set; }

        //  A escola adere ao Programa Escola em Tempo Integral, iniciativa do Ministério da Educação(MEC)
        //  e coordenado pela Secretaria de Educação Básica(SEB)?
        public string? Adere { get; set; }

        //Rural, urbano etc
        public string? TipologiaMunicipio { get; set; }

        //Se a instituição recebeu algum apoio financeiro para realizar a feira
        public string? ApoioFinanceiro { get; set; }

        //Se a instituição ja participou do Ciencia Jovem e 
        //se sim, em que anos e quantos projetos foram apresentados
        public string? ParticipacaoCienciaJovem { get; set; }

        /// <summary>
        /// Guarda uma lista de AuxInstituicaoResponsavel
        /// Por responsavel e instituicao ser uma relacao N:M
        /// Foi criado essa classe auxiliar
        /// </summary>
        public List<AuxInstituicaoResponsavel> AuxInstituicaoResponsavel { get;   set; } = new();


        /// <summary>
        /// Uma feira possui dois tipos de instituicao
        /// Uma instituicao do responsavel que criou a feira na planilha
        /// e uma instituicao onde vai ser realizada
        /// Essas duas propriedades guardam isso
        /// Relação (1 instituicao: N feiras)
        /// </summary>
        [InverseProperty("Instituicao")]
        public List<Feira> Feiras { get; set; } = new();

        [InverseProperty("InstituicaoOrganizadora")]
        public List<Feira> FeirasOrganizadas { get; set; } = new();

        //Guarda uma lista de Telefones e verifica se são somente numeros
        public List<Telefone> Telefone { get; set; } = new();

        //Guarda uma lista de Emails e verifica se possui @
        public List<Email> Email { get; set; } = new();

        //Cria um construtor vazio, para o EF CORE funcionar
        protected Instituicao() { }

        public Instituicao (string? nome)
        {
            Nome = nome;
        }

        public Instituicao(string? nome, string? cnpj, string? pais, string? estado,
            string? municipio, string? endereco, string? tipoRede, string? gre, double? ideb, double? idhm, 
            string? participante, string? ofertaEnsino, string? adere,
            string? tipologiaMunicipio, string? apoioFinanceiro, string? participacaoCJ) : this(nome)
        {
            CNPJ = cnpj;
            Pais = pais;
            Estado = estado;
            Municipio = municipio;
            Endereco = endereco;
            TipoRede = tipoRede;
            GRE = gre;
            IDEB = ideb;
            IDHM = idhm;
            Participante = participante;
            OfertaEnsino = ofertaEnsino;
            Adere = adere;
            TipologiaMunicipio = tipologiaMunicipio;
            ApoioFinanceiro = apoioFinanceiro;
            ParticipacaoCienciaJovem = participacaoCJ;
        }
    }
}
