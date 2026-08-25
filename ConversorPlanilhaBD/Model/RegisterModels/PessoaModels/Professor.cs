using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Model.RegisterModels.PessoaModels
{
    /// <summary>
    /// Guarda os dados de um professor, que é uma pessoa responsavel por um projeto
    /// </summary>
    public class Professor : Pessoa
    {
        //Guarda o número da matricula do professor
        public string NumMatricula { get; set; } = null!;

        //Guarda uma lista de Projetos (relação 1 Professor:N projetos)
        public List<Projeto> Projetos { get; set; } = new();

        protected Professor() { }
        
        public Professor(string sNome, string sNumMatricula) : base(sNome)
        {
            NumMatricula = sNumMatricula;
        }
    }
}
