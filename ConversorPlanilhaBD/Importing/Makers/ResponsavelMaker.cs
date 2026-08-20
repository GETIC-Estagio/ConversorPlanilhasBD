using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class ResponsavelMaker
    {
        private readonly CienciaJovemDb _db;

        public ResponsavelMaker(CienciaJovemDb db)
        {
            _db = db;
        }

        public async Task<Responsavel> ObterOuCriarAsync(
            string? nome, string? idGenero, string? raca,
            DateOnly? dataNascimento, string? professor, string? nivelEnsino,
            string? participante, string? experiencia, string? recomendacao, string cpf)
        {
            var responsavel = await _db.Responsaveis
                .FirstOrDefault(r => r.Identidade.CPF == cpf)
        }
    }
}
