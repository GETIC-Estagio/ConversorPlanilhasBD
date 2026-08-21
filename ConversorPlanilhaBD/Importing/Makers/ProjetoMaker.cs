using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class ProjetoMaker : MakerHelper
    {
        private readonly CienciaJovemDb _db;

        public ProjetoMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        public async Task<Projeto?> ObterOuCriarAsync(
            IXLRow row, int numeroLinha,
            Responsavel? responsavel, Professor? professor, List<Aluno> alunos)
        {
            string? nomeProjeto = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.NomeProjeto);
            if (string.IsNullOrWhiteSpace(nomeProjeto)) return null;

            var projetoExistente = await _db.Projetos.FirstOrDefaultAsync(p => p.Nome != null && p.Nome.ToLower() == nomeProjeto.ToLower());
            if (projetoExistente != null) return projetoExistente;

            // Supondo construtor da classe Projeto
            var novoProjeto = new Projeto
            (
                nomeProjeto,
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.DeficienciaProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.ParticipacaoProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.CategoriaInscricaoProjeto),
                ExtrairValidarDataHora(row, ExcelHelper.ColunasProjetos.CarimboDataHoraProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.PalavrasChaveProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.ODSProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.TemaProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.AreaProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.ObjetivoProjeto),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.ResumoProjeto)
            );

            if (responsavel != null) novoProjeto.ResponsavelId = responsavel.Id;
            if (professor != null) novoProjeto.ProfessorId = professor.Id;

            // Associa os alunos criados ao projeto
            foreach (var aluno in alunos)
            {
                novoProjeto.Alunos.Add(aluno);
            }

            _db.Projetos.Add(novoProjeto);

            try
            {
                await _db.SaveChangesAsync();
                return novoProjeto;
            }
            catch (Exception ex)
            {
                EnviarErro(numeroLinha, $"Erro ao salvar Projeto: {ex.InnerException?.Message ?? ex.Message}");
                _db.Entry(novoProjeto).State = EntityState.Detached;
                return null;
            }
        }
    }
}
