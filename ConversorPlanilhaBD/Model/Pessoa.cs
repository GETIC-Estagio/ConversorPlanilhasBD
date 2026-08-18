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
    public class Pessoa
    {

        //Guarda o id da pessoa e auto-incrementa ao passar para o BD
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        //Guarda o nome da pessoa
        public string? Nome { get; set; }
        
        //Guarda uma lista de Identidades e verifica se são somente numeros
        public List<Identidade> Identidade { get; set; } = new();

        //Guarda uma lista de Emails e verifica se possui @
        public List<Email> Email { get; set; } = new();

        //Guarda uma lista de Telefones e verifica se são somente numeros
        public List<Telefone> Telefone { get; set; } = new();

        //Identidade de Genero
        public string? IdGenero { get; set; }

        //Raça
        public string? Raca { get; set; }

        /// <summary>
        /// Somente quando for aluno
        /// Guarda a chave estrangeira de Projeto
        /// Relação (1 Projeto: N alunos)
        /// </summary>
        public int? ProjetoId { get; set; }
        [ForeignKey("ProjetoId")]
        public Projeto? Projeto { get; set; }


        //Cria um construtor vazio, para o EF CORE funcionar
        protected Pessoa() { }

        //Construtor com as informacoes mais importantes
        public Pessoa(string? nome, string? idGenero, string? raca)
        {
            Nome = nome;
            IdGenero = idGenero;
            Raca = raca;
        }
    }
}