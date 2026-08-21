using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ConversorPlanilhaBD.Helpers;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class InstituicaoMaker : MakerHelper
    {
        private readonly CienciaJovemDb _db;

        public InstituicaoMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        // ============================================================
        // RESPONSÁVEL DA ABA DE FEIRAS
        // ============================================================

        public async Task<Instituicao?> ObterOuCriarFeiraAsync(IXLRow row, int numeroLinha, bool isOrganizadora)
        {

            // ============================================================
            // VALIDAÇÃO DO NOME
            // ============================================================

            int colunaNome = isOrganizadora ? ExcelHelper.ColunasFeiras.InstituicaoOrganizadoraNome
                : ExcelHelper.ColunasFeiras.NomeInstituicao;

            string? nomeInstituicao = ExtrairValidarTexto(row, colunaNome);


            if (string.IsNullOrWhiteSpace(nomeInstituicao))
            {
                EnviarErro(
                    numeroLinha,
                    "Nome da instituição não informado."
                );

                return null;
            }

            // ============================================================
            //  VALIDACAO CNPJ
            // ============================================================

            string? cnpj = null;

            if (!isOrganizadora)
            {
                cnpj = ExcelHelper.ObterValor(row, ExcelHelper.ColunasFeiras.CNPJ_Instituicao);
                if (!string.IsNullOrWhiteSpace(cnpj))
                {
                    try
                    {
                        cnpj = ValidationHelper.VerificarCNPJ(cnpj);
                    }
                    catch
                    {
                        EnviarErro(numeroLinha, $"CNPJ da instituição inválido: {cnpj}");
                        cnpj = null;
                    }
                }
                else
                {
                    cnpj = null;
                }
            }

            // ============================================================
            // BUSCA INSTITUIÇÃO EXISTENTE
            // ============================================================

            Instituicao? instituicaoExistente = null;

            if (cnpj != null)
            {
                instituicaoExistente = await _db.Instituicoes.FirstOrDefaultAsync(i => i.CNPJ == cnpj);
            }

            if (instituicaoExistente != null) return instituicaoExistente;

            // ============================================================
            //  CRIACAO INSTITUICAO
            // ============================================================

            Instituicao novaInstituicao = new Instituicao(nomeInstituicao);

            if (!isOrganizadora)
            {
                novaInstituicao.CNPJ = cnpj;
                novaInstituicao.Pais = ExtrairValidarPais(row, ExcelHelper.ColunasFeiras.PaisInstituicao);

                if (novaInstituicao.Pais != null &&
                    (novaInstituicao.Pais.ToLower() == "brazil" ||
                     novaInstituicao.Pais.ToLower() == "brasil" ||
                     novaInstituicao.Pais.ToLower() == "br"))
                {
                    novaInstituicao.Estado = ExtrairValidarEstado(row, ExcelHelper.ColunasFeiras.EstadoInstituicao);
                }
                else
                {
                    novaInstituicao.Estado = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EstadoInstituicao);
                }

                novaInstituicao.Municipio = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.MunicipioInstituicao);
                novaInstituicao.Endereco = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EnderecoInstituicao);
                novaInstituicao.TipoRede = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.TipoRedeInstituicao);
                novaInstituicao.GRE = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.GreInstituicao);

                novaInstituicao.IDEB = ExtrairValidarDouble(row, ExcelHelper.ColunasFeiras.IdebInstituicao);
                novaInstituicao.IDHM = ExtrairValidarDouble(row, ExcelHelper.ColunasFeiras.IdhmInstituicao);


                novaInstituicao.ParticipacaoCienciaJovem = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ParticipouCienciaJovemInstituicao);
                novaInstituicao.OfertaEnsino = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.OfertaEnsinoInstituicao);
                novaInstituicao.Adere = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.AdereTempoIntegralInstituicao);
                novaInstituicao.TipologiaMunicipio = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.TipologiaMunicipioInstituicao);
                novaInstituicao.ApoioFinanceiro = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ApoioFinanceiroInstituicao);

                // ============================================================
                //  INSERINDO CONTATOS
                // ============================================================

                AdicionarContatosFeira(row, novaInstituicao);
            }

            // ============================================================
            //  SALVAR INSTITUICAO NO BANCO
            // ============================================================

            _db.Instituicoes.Add(novaInstituicao);

            try
            {
                await _db.SaveChangesAsync();
                return novaInstituicao;

            }
            catch (Exception ex)
            {
                //Erro Banco de Dados
                string erroMsg = ex.InnerException?.Message ?? ex.Message;
                EnviarErro(numeroLinha, $"Erro ao salvar a Instituicao no Banco de Dados: {erroMsg}");

                // REMOVE A ENTIDADE COM ERRO DO CONTEXTO PARA NÃO QUEBRAR A PRÓXIMA LINHA
                _db.Entry(novaInstituicao).State = EntityState.Detached;

                return null; //Inserção falhou
            }
        }

        // ============================================================
        // RESPONSÁVEL DA ABA DE PROJETOS
        // ============================================================

        public async Task<Instituicao?> ObterOuCriarProjetoAsync(IXLRow row, int numeroLinha)
        {

            // ============================================================
            // VALIDAÇÃO DO NOME
            // ============================================================

            string? nomeInstituicao = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.NomeInstituicao);


            if (string.IsNullOrWhiteSpace(nomeInstituicao))
            {
                EnviarErro(
                    numeroLinha,
                    "Nome da instituição não informado."
                );

                return null;
            }

            // ============================================================
            //  VALIDACAO CNPJ
            // ============================================================

            string? cnpj = ExcelHelper.ObterValor(row, ExcelHelper.ColunasProjetos.CNPJ_Instituicao);


            if (!string.IsNullOrWhiteSpace(cnpj))
            {
                try
                {
                    cnpj = ValidationHelper.VerificarCNPJ(cnpj);
                }
                catch
                {
                    EnviarErro(numeroLinha, $"CNPJ da instituição inválido: {cnpj}");
                    cnpj = null;
                }
            }
            else
            {
                cnpj = null;
            }


            // ============================================================
            // BUSCA INSTITUIÇÃO EXISTENTE
            // ============================================================

            Instituicao? instituicaoExistente = null;

            if (cnpj != null)
            {
                instituicaoExistente = await _db.Instituicoes.FirstOrDefaultAsync(i => i.CNPJ == cnpj);
            }

            if (instituicaoExistente != null) return instituicaoExistente;

            // ============================================================
            //  CRIACAO INSTITUICAO
            // ============================================================

            Instituicao novaInstituicao = new Instituicao(nomeInstituicao);

            novaInstituicao.CNPJ = cnpj;
            novaInstituicao.Pais = ExtrairValidarPais(row, ExcelHelper.ColunasProjetos.PaisInstituicao);

            if (novaInstituicao.Pais != null &&
                (novaInstituicao.Pais.ToLower() == "brazil" ||
                 novaInstituicao.Pais.ToLower() == "brasil" ||
                 novaInstituicao.Pais.ToLower() == "br"))
            {
                novaInstituicao.Estado = ExtrairValidarEstado(row, ExcelHelper.ColunasProjetos.EstadoInstituicao);
            }
            else
            {
                novaInstituicao.Estado = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.EstadoInstituicao);
            }

            novaInstituicao.Municipio = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.MunicipioInstituicao);
            novaInstituicao.Endereco = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.EnderecoInstituicao);
            novaInstituicao.TipoRede = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.TipoRedeInstituicao);
            novaInstituicao.GRE = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.GREInstituicao);

            novaInstituicao.IDEB = ExtrairValidarDouble(row, ExcelHelper.ColunasProjetos.IDEBInstituicao);
            novaInstituicao.IDHM = ExtrairValidarDouble(row, ExcelHelper.ColunasProjetos.IDHMInstituicao);


            novaInstituicao.ParticipacaoCienciaJovem = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.ParticipouCienciaJovemInstituicao);
            novaInstituicao.OfertaEnsino = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.OfertaEnsinoInstituicao);
            novaInstituicao.Adere = ExtrairValidarTexto(row, ExcelHelper.ColunasProjetos.AdereInstituicao);
            
            //Não vem da planilha
            novaInstituicao.TipologiaMunicipio = null;
            novaInstituicao.ApoioFinanceiro = null;

            // ============================================================
            //  INSERINDO CONTATOS
            // ============================================================

            AdicionarContatosProjeto(row, novaInstituicao);


            // ============================================================
            //  SALVAR INSTITUICAO NO BANCO
            // ============================================================

            _db.Instituicoes.Add(novaInstituicao);

            try
            {
                await _db.SaveChangesAsync();
                return novaInstituicao;

            }
            catch (Exception ex)
            {
                //Erro Banco de Dados
                string erroMsg = ex.InnerException?.Message ?? ex.Message;
                EnviarErro(numeroLinha, $"Erro ao salvar a Instituicao no Banco de Dados: {erroMsg}");

                // REMOVE A ENTIDADE COM ERRO DO CONTEXTO PARA NÃO QUEBRAR A PRÓXIMA LINHA
                _db.Entry(novaInstituicao).State = EntityState.Detached;

                return null; //Inserção falhou
            }
        }
        private void AdicionarContatosFeira(IXLRow row, Instituicao instituicao)
        {
            string? email = ExtrairValidarEmail(row, ExcelHelper.ColunasFeiras.EmailInstituicao);
            string? telefone = ExtrairValidarTelefone(row, ExcelHelper.ColunasFeiras.TelefoneInstituicao);

            if (email != null) instituicao.Email.Add(new Email(email));
            if (telefone != null) instituicao.Telefone.Add(new Telefone(telefone));
        }

        private void AdicionarContatosProjeto(IXLRow row, Instituicao instituicao)
        {
            string? email = ExtrairValidarEmail(row, ExcelHelper.ColunasProjetos.EmailInstituicao);
            string? telefone = ExtrairValidarTelefone(row, ExcelHelper.ColunasProjetos.TelefoneInstituicao);

            if (email != null) instituicao.Email.Add(new Email(email));
            if (telefone != null) instituicao.Telefone.Add(new Telefone(telefone));
        }
    }
}
