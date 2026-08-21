using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    /// <summary>
    /// Guarda os dados de um professor, que é uma pessoa responsavel por um projeto
    /// </summary>
    public class Professor : Pessoa
    {
        //Guarda o número da matricula do professor
        public string? NumMatricula { get; set; }

        //Guarda uma lista de Projetos (relação 1 Professor:N projetos)
        public List<Projeto> Projetos { get; set; } = new();

        protected Professor() { }
        
        //Chama o construtor pai Pessoa
        public Professor(string? nome, string? idGenero, string? raca, string? numMatricula)
            : base(nome, idGenero, raca)
        {
            NumMatricula = numMatricula;
        }
    }
}
