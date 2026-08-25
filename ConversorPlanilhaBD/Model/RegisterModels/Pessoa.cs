using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ConversorPlanilhaBD.Enums;

namespace ConversorPlanilhaBD.Model.RegisterModels
{
    /// <summary>
    /// Classe abstrata que Alunos, Responsaveis e Professores herdam.
    /// </summary>
    public abstract class Pessoa : ModelCadastro
    {
        public string Nome { get; set; } = null!;

        //Guarda a Identidade separada por CPF e RG
        //Se vier identidade sem órgão expedidor, é CPF
        public Identidade Identidade{ get; set; } = null!;

        public List<Email> Emails { get; set; } = new();
        public List<Telefone> Telefones { get; set; } = new();

        public EnumGenero Genero { get; set; }
        public EnumRaca Raca { get; set; }

        public Pessoa() { }

        public Pessoa(string sNome) : base()
        {
            Nome = sNome;
        }
    }
}