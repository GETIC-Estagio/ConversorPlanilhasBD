using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Importing.Makers;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.AuxModels;
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

                try
                {
                    //Cria os Responsaveis
                    var responsavelSubmissao = await _responsavelMaker.ObterOuCriarAsync(row, numeroLinha, isContato: false);
                    var responsavelContato = await _responsavelMaker.ObterOuCriarAsync(row, numeroLinha, isContato: true);

                    //Cria as Instituicoes
                    var instSede = await _instituicaoMaker.ObterOuCriarAsync(row, numeroLinha, isOrganizadora: false);
                    var instOrganizadora = await _instituicaoMaker.ObterOuCriarAsync(row, numeroLinha, isOrganizadora: true);

                    // Vínculo Auxiliar (Responsável <-> Instituição Sede)
                    if (responsavelSubmissao != null && instSede != null)
                    {
                        await VincularAuxiliarAsync(row, numeroLinha, responsavelSubmissao, instSede);
                    }

                    // Cria a Feira
                    var feira = await _feiraMaker.ObterOuCriarAsync(row, numeroLinha, responsavelSubmissao, instSede, instOrganizadora, responsavelContato);

                    // Se a feira foi criada, deu sucesso na linha
                    if (feira != null) _resultado.RegistrarSucesso();
                }
                catch (Exception ex)
                {
                    _resultado.RegistrarErro(numeroLinha, $"Erro inesperado: {ex.Message}");
                    Erro?.Invoke($"Linha {numeroLinha}: {ex.Message}");
                }
                processadas++;
                Progresso?.Invoke(processadas, total);
                ContadoresAtualizados?.Invoke(_resultado.Sucessos, _resultado.Erros);
            }
        }

        private async Task VincularAuxiliarAsync(IXLRow row, int linha, Responsavel responsavel, Instituicao instituicao)
        {
            bool jaExiste = await _db.AuxInstituicoesResponsaveis
                .AnyAsync(a => a.ResponsavelId == responsavel.Id && a.InstituicaoId == instituicao.Id);

            if (!jaExiste)
            {
                string funcao = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.FuncaoResponsavelInstituicao) ?? "";

                var aux = new AuxInstituicaoResponsavel
                {
                    ResponsavelId = responsavel.Id,
                    InstituicaoId = instituicao.Id,
                    FuncaoInstituicao = funcao
                };

                _db.AuxInstituicoesResponsaveis.Add(aux);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    string erroMsg = ex.InnerException?.Message ?? ex.Message;
                    _resultado.RegistrarErro(linha, $"Erro ao criar vínculo Responsável-Instituição: {erroMsg}");

                    _db.Entry(aux).State = EntityState.Detached;
                }
            }
        }
    }
}
