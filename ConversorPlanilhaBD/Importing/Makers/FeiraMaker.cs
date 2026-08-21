using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class FeiraMaker : MakerHelper
    {
        private readonly CienciaJovemDb _db;

        public FeiraMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        public async Task<Feira?> ObterOuCriarAsync(
            IXLRow row, int numeroLinha,
            Responsavel? responsavelSubmissao,
            Instituicao? instituicaoSede,
            Instituicao? instituicaoOrganizadora,
            Responsavel? pessoaContato)
        {
            // ============================================================
            //  OBTENÇÃO NOME E BUSCA NO BANCO DE DADOS
            // ============================================================

            string? nomeFeira = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.NomeFeira);
            if (string.IsNullOrWhiteSpace(nomeFeira))
            {
                EnviarErro(numeroLinha, "Nome da feira não informado.");
                return null; // Não dá pra salvar sem nome
            }
            nomeFeira = nomeFeira.Trim();

            var feiraExistente = await _db.Feiras.FirstOrDefaultAsync(f => f.Nome != null && f.Nome.ToLower() == nomeFeira.ToLower());
            if (feiraExistente != null) return feiraExistente;

            // ============================================================
            //  NOVA FEIRA
            // ============================================================

            Feira novaFeira = new Feira
            (
                nomeFeira,
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.AlcanceFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EnderecoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EstadoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.PeriodoRealizacaoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.DataRealizacaoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ModalidadeParticipacaoFeira),
                ExtrairValidarInt(row, ExcelHelper.ColunasFeiras.NumeroProjetosParticipantesFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.AreasConhecimentoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.NivelEnsinoAlunosFeira),
                ExtrairValidarInt(row, ExcelHelper.ColunasFeiras.NumeroEscolasParticipantesFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.FeiraAfiliada),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ProcessoSelecaoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.PeriodoElaboracaoFeira),
                ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ProjetosAvaliadosFeira),
                ExtrairValidarDataHora(row, ExcelHelper.ColunasFeiras.CarimboDataHoraFeira)
            );

            // ============================================================
            //  VINCULACAO CHAVES ESTRANGEIRAS
            // ============================================================

            // Apenas vinculamos se o objeto chegou não nulo do Maker anterior

            if (responsavelSubmissao != null) novaFeira.ResponsavelId = responsavelSubmissao.Id;
            if (instituicaoSede != null) novaFeira.InstituicaoId = instituicaoSede.Id;
            if (instituicaoOrganizadora != null) novaFeira.InstituicaoOrganizadoraId = instituicaoOrganizadora.Id;
            if (pessoaContato != null) novaFeira.ResponsavelContatoId = pessoaContato.Id;

            // ============================================================
            //  SALVAR BANCO DE DADOS
            // ============================================================

            _db.Feiras.Add(novaFeira);

            try
            {
                await _db.SaveChangesAsync();
                return novaFeira;

            }
            catch (Exception ex)
            {
                //Erro Banco de Dados
                string erroMsg = ex.InnerException?.Message ?? ex.Message;
                EnviarErro(numeroLinha, $"Erro ao salvar a Feira no Banco de Dados: {erroMsg}");

                // REMOVE A ENTIDADE COM ERRO DO CONTEXTO PARA NÃO QUEBRAR A PRÓXIMA LINHA
                _db.Entry(novaFeira).State = EntityState.Detached;

                return null; //Inserção falhou
            }
        }
    }
}
