using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Importing.Makers;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing
{
    /// <summary>
    /// Essa classe é responsável por importar os dados de um pré-projeto a partir de uma planilha Excel para o banco de dados.
    /// </summary>
    public class ImportadorPreProjetos
    {
        private readonly CienciaJovemDb _db;
        private readonly IXLWorksheet _planilha;
        private readonly ResultadoImportacao _resultado;

        // Makers
        private readonly ProfessorMaker _professorMaker;
        private readonly AlunoMaker _alunoMaker;
        private readonly ProjetoMaker _projetoMaker;

        // NOTA: Você precisará adaptar o seu ResponsavelMaker e InstituicaoMaker atuais 
        // para aceitarem quais constantes de coluna usar (ColunasFeiras ou ColunasProjetos)
        // ou criar versões específicas para Projetos.
        private readonly ResponsavelMaker _responsavelMaker;
        private readonly InstituicaoMaker _instituicaoMaker;

        public event Action<int, int>? Progresso;
        public event Action<int, int>? ContadoresAtualizados;
        public event Action<string>? Erro;

        public ImportadorPreProjetos(CienciaJovemDb db, IXLWorksheet planilha, ResultadoImportacao resultado)
        {
            _db = db;
            _planilha = planilha;
            _resultado = resultado;

            _professorMaker = new ProfessorMaker(db, resultado);
            _alunoMaker = new AlunoMaker(db, resultado);
            _projetoMaker = new ProjetoMaker(db, resultado);

            _responsavelMaker = new ResponsavelMaker(db, resultado);
            _instituicaoMaker = new InstituicaoMaker(db, resultado);
        }

        public async Task ImportarAsync()
        {
            var linhas = _planilha.RowsUsed().Skip(1).ToList();
            int total = linhas.Count;
            int processadas = 0;

            foreach (var row in linhas)
            {
                int numeroLinha = row.RowNumber();

                try
                {
                    // 1. Cria Responsável e Instituição (Adaptar o maker para ler de ColunasProjetos)
                    var responsavel = await _responsavelMaker.ObterOuCriarAsync(row, numeroLinha, isContato: false);
                    var instituicao = await _instituicaoMaker.ObterOuCriarAsync(row, numeroLinha, isOrganizadora: false);

                    // 2. Vínculo Auxiliar (Opcional, se existir a lógica)
                    if (responsavel != null && instituicao != null)
                    {
                        await VincularAuxiliarAsync(row, numeroLinha, responsavel, instituicao);
                    }

                    // 3. Cria Professor
                    var professor = await _professorMaker.ObterOuCriarAsync(row, numeroLinha);

                    // 4. Cria Alunos
                    var alunos = new List<Aluno>();

                    var aluno1 = await _alunoMaker.ObterOuCriarAsync(row, numeroLinha,
                        ExcelHelper.ColunasProjetos.NomeAluno1, ExcelHelper.ColunasProjetos.RGAluno1,
                        ExcelHelper.ColunasProjetos.OrgaoExpedidorAluno1, ExcelHelper.ColunasProjetos.CPFAluno1,
                        ExcelHelper.ColunasProjetos.RacaAluno1, ExcelHelper.ColunasProjetos.GeneroAluno1,
                        ExcelHelper.ColunasProjetos.EmailAluno1);

                    if (aluno1 != null) alunos.Add(aluno1);

                    var aluno2 = await _alunoMaker.ObterOuCriarAsync(row, numeroLinha,
                        ExcelHelper.ColunasProjetos.NomeAluno2, ExcelHelper.ColunasProjetos.RGAluno2,
                        ExcelHelper.ColunasProjetos.OrgaoExpedidorAluno2, ExcelHelper.ColunasProjetos.CPFAluno2,
                        ExcelHelper.ColunasProjetos.RacaAluno2, ExcelHelper.ColunasProjetos.GeneroAluno2,
                        ExcelHelper.ColunasProjetos.EmailAluno2);

                    if (aluno2 != null) alunos.Add(aluno2);

                    // 5. Cria Projeto
                    var projeto = await _projetoMaker.ObterOuCriarAsync(row, numeroLinha, responsavel, professor, alunos);

                    if (projeto != null) _resultado.RegistrarSucesso();
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
