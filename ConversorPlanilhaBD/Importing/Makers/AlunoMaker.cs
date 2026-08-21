using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.ValueObjects;
using ConversorPlanilhaBD.Validators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class AlunoMaker : ValidationMaker
    {
        private readonly CienciaJovemDb _db;

        public AlunoMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        // Recebe as colunas dinamicamente para servir para Aluno 1 e Aluno 2
        public async Task<Aluno?> ObterOuCriarAsync(IXLRow row, int numeroLinha,
            int colNome, int colRG, int colOrgao, int colCPF, int colRaca, int colGenero, int colEmail)
        {
            string? nome = ExtrairValidarNome(row, colNome);
            if (string.IsNullOrWhiteSpace(nome)) return null; // Se não tem nome, ignora este aluno

            string? cpf = ExcelHelper.ObterValor(row, colCPF);

            if (!string.IsNullOrWhiteSpace(cpf))
            {
                try
                {
                    cpf = ValidationHelper.VerificarCPF(cpf);
                    var alunoExistente = await _db.Pessoas
                        .Include(p => p.Identidade)
                        .FirstOrDefaultAsync(p => p.Identidade != null && p.Identidade.CPF == cpf);
                    if (alunoExistente != null) return (Aluno)alunoExistente;
                }
                catch
                {
                    EnviarErro(numeroLinha, $"CPF do Aluno inválido: {cpf}");
                    cpf = null;
                }
            }

            var novoAluno = new Aluno(
                nome,
                ExtrairValidarTexto(row, colGenero),
                ExtrairValidarTexto(row, colRaca)
            );

            novoAluno.Identidade = new Identidade(cpf)
            {
                RG = ExtrairValidarTexto(row, colRG),
                OrgaoExpedidor = ExtrairValidarTexto(row, colOrgao)
            };

            string? email = ExtrairValidarEmail(row, colEmail);
            if (email != null) novoAluno.Email.Add(new Email(email));

            _db.Pessoas.Add(novoAluno);

            try
            {
                await _db.SaveChangesAsync();
                return novoAluno;
            }
            catch (Exception ex)
            {
                EnviarErro(numeroLinha, $"Erro ao salvar Aluno: {ex.InnerException?.Message ?? ex.Message}");
                _db.Entry(novoAluno).State = EntityState.Detached;
                return null;
            }
        }
    }
}
