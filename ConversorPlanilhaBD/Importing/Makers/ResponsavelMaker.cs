using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class ResponsavelMaker
    {
        private readonly CienciaJovemDb _db;
        private readonly ResultadoImportacao _resultado;

        public ResponsavelMaker(CienciaJovemDb db, ResultadoImportacao resultado)
        {
            _db = db;
            _resultado = resultado;
        }

        public async Task<Responsavel?> ObterOuCriarAsync(IXLRow row, int numeroLinha, bool isContato)
        {
            // ============================================================
            //  OBTENÇÃO CPF E BUSCA NO BANCO DE DADOS
            // ============================================================

            string? cpf = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.CPF_Responsavel);

            try { 
                ValidationHelper.VerificarCPF(cpf); 
            }
            catch
            {
                EnviarErro(numeroLinha, $"CPF do responsável inválido: {cpf}");
                cpf = null;
            }


            if (cpf != null)
            {
                var responsavelExistente = await _db.Responsaveis.FirstOrDefault(
                    r => r.Identidade != null && r.Identidade.CPF == cpf);

                if (responsavelExistente != null)
                {
                    return responsavelExistente;
                }
            }

            // ============================================================
            //  CRIACAO NOVO REGISTRO DE RESPONSAVEL
            // ============================================================

            Responsavel novoResponsavel;

            if (isContato)
            {
                //Raca e Genero não são enviados para o responsável de contato, então são nulos
                novoResponsavel = new Responsavel(
                    ExtrairValidarNome(row, ExcelHelper.ColunasFeiras.NomeResponsavelContatoFeira),
                    null,
                    null
                );
            }
            else
            {
                novoResponsavel = new Responsavel(
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.NomeResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.IdentidadeGeneroResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.RacaResponsavel),
                    ExtrairValidarData(row, ExcelHelper.ColunasFeiras.DataNascimentoResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EhProfessorResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.NivelEnsinoResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ParticipouCienciaJovemResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ExperienciaFeirasResponsavel),
                    ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.RecomendacaoResponsavel)
                    );
            }

            //Insere o CPF dele
            if (cpf != null)
                novoResponsavel.Identidade.CPF = cpf;


            // ============================================================
            //  INSERÇÂO DE CONTATOS
            // ============================================================

            AdicionarContatos(row, novoResponsavel, isContato);

            // ============================================================
            //  SALVAR NO BANCO DE DADOS
            // ============================================================

            _db.Responsaveis.Add(novoResponsavel);
            await _db.SaveChangesAsync();

            return novoResponsavel;


        }
 

        private void AdicionarContatos(IXLRow row, Responsavel responsavel, bool isContato)
        {
            // ============================================================
            //  OBTENÇÃO EMAILS
            // ============================================================
            string? email1 = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.EmailResponsavel);
            string? email2 = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.OutroEmailResponsavel);
            if (!string.IsNullOrWhiteSpace(email1))
            {
                responsavel.Email.Add(new Email(email1));
            }
            if (!string.IsNullOrWhiteSpace(email2))
            {
                responsavel.Email.Add(new Email(email2));
            }
            // ============================================================
            //  OBTENÇÃO TELEFONES
            // ============================================================
            string? telefone1 = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.Telefone1Responsavel);
            string? telefone2 = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.Telefone2Responsavel);
            if (!string.IsNullOrWhiteSpace(telefone1))
            {
                responsavel.Telefone.Add(new Telefone(telefone1));
            }
            if (!string.IsNullOrWhiteSpace(telefone2))
            {
                responsavel.Telefone.Add(new Telefone(telefone2));
            }
        }

    }
}
