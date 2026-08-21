using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ConversorPlanilhaBD.Model
{
    /// <summary>
    /// Guarda os dados de uma pessoa, como nome, identidade, emails, telefones, gênero e raça.
    /// </summary>
    public class Pessoa
    {

        //Guarda o id da pessoa e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Nome { get; set; }

        //Guarda a Identidade separada por CPF e RG
        //Se vier identidade sem órgão expedidor, é CPF
        public Identidade? Identidade{ get; set; }

        //Guarda uma lista de Emails
        public List<Email> Email { get; set; } = new();

        //Guarda uma lista de Telefones
        public List<Telefone> Telefone { get; set; } = new();

        public string? Genero { get; set; }

        //Raça
        public string? Raca { get; set; }


        //Somente quando for aluno
        //Guarda a chave estrangeira de Projeto
        //Relação (1 Projeto: N alunos)
        public int? ProjetoId { get; set; }
        [ForeignKey("ProjetoId")]
        public Projeto? Projeto { get; set; }


        //Cria um construtor vazio, para o EF CORE funcionar
        protected Pessoa() { }

        public Pessoa(string? nome, string? idGenero, string? raca)
        {
            Nome = nome;
            Genero = idGenero;
            Raca = raca;
        }
    }
}