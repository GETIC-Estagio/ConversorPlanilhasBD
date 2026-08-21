using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importing;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importacao
{
    /// <summary>
    /// Classe que organiza os sub-importadores
    /// </summary>
    public class Importador
    {
        private readonly XLWorkbook _workbook;

        //abas da planilha
        private readonly IXLWorksheet _feiras;
        private readonly IXLWorksheet _preProjetos;

        //Eventos de UI
        public event Action<int, int>? Progresso;
        public event Action<int, int>? ContadoresAtualizados;
        public event Action<string>? Erro;

        private readonly string _connectionString;

        public Importador(string caminhoArquivo, string connectionString)
        {
            //Verifica que não esta vazia
            if (string.IsNullOrWhiteSpace(caminhoArquivo))
                throw new ArgumentException(
                    "O caminho da planilha não pode ser vazio.",
                    nameof(caminhoArquivo));

            //Verifica que não esta vazia
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException(
                    "A string de conexão não pode ser vazia.",
                    nameof(connectionString));

            //Pega a string de conexão
            _connectionString = connectionString;

            //Pega o caminho do excel
            _workbook = new XLWorkbook(caminhoArquivo);

            //Pega as duas abas da planilha
            _feiras = _workbook.Worksheet("Feira afiliadas à 32ª edição");
            _preProjetos = _workbook.Worksheet("Pré-projetos da 32ª Edição");
        }

        public async Task<ResultadoImportacao> ImportarAsync()
        {
            //Guarda o contador de sucessos e erros
            //E o tipo de erro
            var resultado = new ResultadoImportacao();

            var optionsBuilder = new DbContextOptionsBuilder<CienciaJovemDb>();
            optionsBuilder.UseNpgsql(_connectionString);

            await using var db = new CienciaJovemDb(optionsBuilder.Options);

            //Processa Feiras
            var importadorFeira = new ImportadorFeira(db, _feiras, resultado);
            importadorFeira.Progresso += (p, t) => Progresso?.Invoke(p, t);
            importadorFeira.ContadoresAtualizados += (s, e) => ContadoresAtualizados?.Invoke(s, e);
            importadorFeira.Erro += msg => Erro?.Invoke(msg);

            await importadorFeira.ImportarAsync();

            //Processa Projetos
            var importadorProjetos = new ImportadorPreProjetos(db, _preProjetos, resultado);
            importadorProjetos.Progresso += (p, t) => Progresso?.Invoke(p, t);
            importadorProjetos.ContadoresAtualizados += (s, e) => ContadoresAtualizados?.Invoke(s, e);
            importadorProjetos.Erro += msg => Erro?.Invoke(msg);

            await importadorProjetos.ImportarAsync();

            return resultado;
        }
    }
}