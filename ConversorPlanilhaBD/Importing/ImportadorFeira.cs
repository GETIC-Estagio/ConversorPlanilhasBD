using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Importing.Makers;
using ConversorPlanilhaBD.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing
{
    /// <summary>
    /// Essa classe é responsável por importar os dados de uma feira a partir de uma planilha Excel para o banco de dados.
    /// </summary>
    public class ImportadorFeira
    {
        private readonly CienciaJovemDb _db;
        private readonly IXLWorksheet _planilha;
        private readonly ResultadoImportacao _resultado;

        //Makers
        private readonly ResponsavelMaker _responsavelMaker;
        private readonly InstituicaoMaker _instituicaoMaker;
        private readonly FeiraMaker _feiraMaker;

        //Comunicacao com UI
        public event Action<int, int>? Progresso;
        public event Action<int, int>? ContadoresAtualizados;
        public event Action<string>? Erro;

        public ImportadorFeira(CienciaJovemDb db, IXLWorksheet planilha, ResultadoImportacao resultado)
        {
            _db = db;
            _planilha = planilha;
            _resultado = resultado;

            //Inicialização Makers
            _responsavelMaker = new ResponsavelMaker(db, resultado);
            _instituicaoMaker = new InstituicaoMaker(db, resultado);
            _feiraMaker = new FeiraMaker(db, resultado);
        }

        public async Task ImportarAsync()
        {
            //Pula cabeçalho
            var linhas = _planilha.RowsUsed().Skip(1).ToList();
            int total = linhas.Count;
            int processadas = 0;

            foreach (var row in linhas)
            {
                int numeroLinha = row.RowNumber();
                bool erroNaLinha = false;
            }
        }
    }
}
