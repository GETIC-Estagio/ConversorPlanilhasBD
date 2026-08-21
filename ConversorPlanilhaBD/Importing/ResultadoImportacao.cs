using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importacao
{
    /// <summary>
    /// Representa aonsolidado da importação, 
    /// armazenando o total de sucessos, a contagem de erros 
    /// e uma lista detalhada com as mensagens de falha por linha.
    /// </summary>
    public class ResultadoImportacao
    {
        public int Sucessos { get; private set; }
        public int Erros { get; private set; }

        public List<string> MensagensErro { get; private set; } = new();

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
