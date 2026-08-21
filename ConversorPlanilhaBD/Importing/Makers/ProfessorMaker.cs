using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.ValueObjects;
using ConversorPlanilhaBD.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class ProfessorMaker : MakerHelper
    {
        private readonly CienciaJovemDb _db;

        public ProfessorMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        public async Task<Professor?> ObterOuCriarAsync(IXLRow row, int numeroLinha)
        {

            string? cpf = ExcelHelper.ObterValor(row, ExcelHelper.ColunasProjetos.CPF_Professor);

            // Verifica se já existe pelo CPF
            if (!string.IsNullOrWhiteSpace(cpf))
            {
                try
                {
                    cpf = ValidationHelper.VerificarCPF(cpf);

                    var profExistente = await _db.Professores
                        .Include(p => p.Identidade)
                        .FirstOrDefaultAsync(p => p.Identidade != null && p.Identidade.CPF == cpf);

                    if (profExistente != null) 
                        return profExistente;
                }
                catch
                {
                    EnviarErro(numeroLinha, $"CPF do Professor inválido: {cpf}");
                    cpf = null;
                }
            }

            var novoProfessor = new Professor(
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.NomeProfessor),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.GeneroProfessor),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.RacaProfessor),
                ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.MatriculaProfessor)
            );

            novoProfessor.Identidade = new Identidade(cpf)
            {
                RG = ExtrairValidarRG(row, ExcelHelper.ColunasProjetos.RGProfessor),
                OrgaoExpedidor = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.OrgaoExpedidorProfessor)
            };

            string? email = ExtrairValidarEmail(row, ExcelHelper.ColunasProjetos.EmailProfessor);
            string? telefone = ExtrairValidarTelefone(row, ExcelHelper.ColunasProjetos.TelefoneProfessor);

            if (email != null) novoProfessor.Email.Add(new Email(email));
            if (telefone != null) novoProfessor.Telefone.Add(new Telefone(telefone));

            _db.Professores.Add(novoProfessor);

            try
            {
                await _db.SaveChangesAsync();
                return novoProfessor;
            }
            catch (Exception ex)
            {
                EnviarErro(numeroLinha, $"Erro ao salvar Professor: {ex.InnerException?.Message ?? ex.Message}");
                _db.Entry(novoProfessor).State = EntityState.Detached;
                return null;
            }
        }
    }
}
