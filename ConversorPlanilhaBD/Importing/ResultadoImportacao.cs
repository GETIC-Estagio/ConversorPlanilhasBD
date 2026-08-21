using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importacao
{
    public class ResultadoImportacao
    {
        public int Sucessos { get; set; }
        public int Erros { get; set; }

        public List<string> MensagensErro { get; set; } = new();

        //Se houver um sucesso aumentar o contador
        public void RegistrarSucesso()
        {
            Sucessos++;
        }

        //Se houver um erro aumentar o contador
        //E salvar qual foi erro e a linha
        public void RegistrarErro(int linha, string mensagem)
        {
            Erros++;

            MensagensErro.Add(
                $"Linha {linha}: {mensagem}"
            );
        }
    }
}
