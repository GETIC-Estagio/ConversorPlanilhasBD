using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    public class Responsavel : Pessoa
    {
        //Campo referente a data de nascimento
        public DateOnly? DataNascimento { get; set; }

        //Se é professor 
        public string? Professor { get; set; }

        public string? NivelEnsino { get; set; }

        //Se já participou de outras edições da Ciência Jovem
        public string? Participante { get; set; }

        // Se o responsavel teve experiencia em outras feiras
        public string? Experiencia { get; set; }

        //Como o responsavel ficou sabendo sobre as filiações de feiras e a pré-inscrição de projetos da 32ª Ciência Jovem
        public string? Recomendacao { get; set; }

        /// <summary>
        /// Guarda uma lista de AuxInstituicaoResponsavel
        /// Por responsavel e instituicao ser uma relacao N:M
        /// Foi criado essa classe auxiliar
        /// </summary>
        public List<AuxInstituicaoResponsavel> AuxInstituicaoResponsavel { get; set; } = new();


        /// <summary>
        /// Uma feira possui dois tipos de Responsável
        /// Um responsável que criou a feira na planilha
        /// e um responsável para contato
        /// Essas duas propriedades guardam isso
        /// Relação (1 responsavel: N feiras)
        /// </summary>
        [InverseProperty("Responsavel")]
        public List<Feira> Feiras { get; set; } = new();
        
        [InverseProperty("ResponsavelContato")]
        public List<Feira> FeirasContato { get; set; } = new();


        /// <summary>
        /// Guarda uma lista de projetos
        /// Relação (1 responsavel: N projetos)
        /// </summary>
        public List<Projeto> Projetos { get; set; } = new();


        //Cria um construtor vazio, para o EF CORE funcionar
        protected Responsavel() { }

        public Responsavel (string? nome, string? idGenero, string? raca) : base(nome, idGenero, raca) { }

        public Responsavel(string? nome, string? idGenero, string? raca,
            DateOnly? dataNascimento, string? professor, string? nivelEnsino, 
            string? participante, string? experiencia, string? recomendacao) : base (nome, idGenero, raca)
        {
            DataNascimento = dataNascimento;
            Professor = professor;
            NivelEnsino = nivelEnsino;
            Participante = participante;
            Experiencia = experiencia;
            Recomendacao = recomendacao;
        }
    }
}