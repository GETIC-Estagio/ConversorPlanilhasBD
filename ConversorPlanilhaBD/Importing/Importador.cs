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
    public class Importador
    {
        private readonly XLWorkbook _workbook;

        private readonly IXLWorksheet _feiras;
        private readonly IXLWorksheet _preProjetos;

        //Cria lista dos modelos necessários para serem inseridos depois no banco de dados
        private readonly List<Responsavel> _responsaveis = new();
        private readonly List<Instituicao> _instituicoes = new();

        private readonly List<Professor> _professores = new();
        private readonly List<Pessoa> _alunos = new();

        private readonly List<Feira> _feirasImportadas = new();
        private readonly List<Projeto> _projetosImportados = new();

        private readonly List<AuxInstituicaoResponsavel>
            _relacionamentosInstituicaoResponsavel = new();

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


            return resultado;
        }
    }
}