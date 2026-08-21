using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ConversorPlanilhaBD.Model.ValueObjects;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class ResponsavelMaker : ValidationMaker
    {
        private readonly CienciaJovemDb _db;

        public ResponsavelMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        public async Task<Responsavel?> ObterOuCriarAsync(IXLRow row, int numeroLinha, bool isContato)
        {
            // ============================================================
            //  OBTENÇÃO CPF E BUSCA NO BANCO DE DADOS
            // ============================================================

            string? cpf = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.CPF_Responsavel);

            if (!string.IsNullOrWhiteSpace(cpf))
            {
                try
                {
                    cpf = ValidationHelper.VerificarCPF(cpf);

                    var responsavelExistente = await _db.Responsaveis
                   .Include(r => r.Identidade)
                   .FirstOrDefaultAsync(r => r.Identidade != null && r.Identidade.CPF == cpf);

                    if (responsavelExistente != null)
                    {
                        return responsavelExistente;
                    }

                }
                catch
                {
                    EnviarErro(numeroLinha, $"CPF do responsável inválido: {cpf}");
                    cpf = null;
                }
            }
            else
            {
                cpf = null;
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
                novoResponsavel.Identidade = new Identidade(cpf);


            // ============================================================
            //  INSERÇÂO DE CONTATOS
            // ============================================================

            AdicionarContatos(row, novoResponsavel, isContato);

            // ============================================================
            //  SALVAR NO BANCO DE DADOS
            // ============================================================

            _db.Responsaveis.Add(novoResponsavel);

            try
            {
                await _db.SaveChangesAsync();
                return novoResponsavel;

            }
            catch (Exception ex)
            {
                //Erro Banco de Dados
                string erroMsg = ex.InnerException?.Message ?? ex.Message;
                EnviarErro(numeroLinha, $"Erro ao salvar a Responsavel no Banco de Dados: {erroMsg}");

                // REMOVE A ENTIDADE COM ERRO DO CONTEXTO PARA NÃO QUEBRAR A PRÓXIMA LINHA
                _db.Entry(novoResponsavel).State = EntityState.Detached;

                return null; //Inserção falhou
            }
        }

        private void AdicionarContatos(IXLRow row, Responsavel responsavel, bool isContato)
        {
            int colEmail = isContato ? ExcelHelper.ColunasFeiras.EmailContatoFeira : ExcelHelper.ColunasFeiras.EmailResponsavel;
            int colTelefone = isContato ? ExcelHelper.ColunasFeiras.TelefoneContatoFeira : ExcelHelper.ColunasFeiras.TelefoneResponsavel;

            string? email = ExtrairValidarEmail(row, colEmail);
            string? telefone = ExtrairValidarTelefone(row, colTelefone);


            if (email != null)
            {
                responsavel.Email.Add(new Email(email));
            }

            if (telefone != null)
            {
                responsavel.Telefone.Add(new Telefone(telefone));
            }

        }
    }
}
