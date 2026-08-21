using ClosedXML.Excel;
using ConversorPlanilhaBD.Data;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    public class InstituicaoMaker : ValidationMaker
    {
        private readonly CienciaJovemDb _db;

        public InstituicaoMaker(CienciaJovemDb db, ResultadoImportacao resultado) : base(resultado)
        {
            _db = db;
        }

        public async Task<Instituicao?> ObterOuCriarAsync(IXLRow row, int numeroLinha, bool isOrganizadora)
        {
            // ============================================================
            //  OBTENÇÃO CNPJ E BUSCA NO BANCO DE DADOS
            // ============================================================

            int colunaNome = isOrganizadora ? ExcelHelper.ColunasFeiras.InstituicaoOrganizadora : ExcelHelper.ColunasFeiras.NomeInstituicao;
            string? nomeInstituicao = ExtrairValidarTexto(row, colunaNome);

            // Se for uma instituição sem nome, não podemos criar
            if (string.IsNullOrWhiteSpace(nomeInstituicao)) return null;

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

            // 2. BUSCA NO BANCO DE DADOS (Por CNPJ primeiro, ou por Nome)
            Instituicao? instituicaoExistente = null;

            if (cnpj != null)
            {
                instituicaoExistente = await _db.Instituicoes.FirstOrDefaultAsync(i => i.CNPJ == cnpj);
            }
            else
            {
                // Busca pelo nome caso não tenha CNPJ (ignorando maiúsculas/minúsculas)
                instituicaoExistente = await _db.Instituicoes.FirstOrDefaultAsync(i => i.Nome != null && i.Nome.ToLower() == nomeInstituicao.ToLower());
            }

            if (instituicaoExistente != null) return instituicaoExistente;

            // 3. CRIAÇÃO DO NOVO REGISTRO
            Instituicao novaInstituicao = new Instituicao { Nome = nomeInstituicao };

            if (!isOrganizadora)
            {
                novaInstituicao.CNPJ = cnpj;
                novaInstituicao.Pais = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.PaisInstituicao);
                novaInstituicao.Estado = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EstadoInstituicao);
                novaInstituicao.Municipio = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.MunicipioInstituicao);
                novaInstituicao.Endereco = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.EnderecoInstituicao);
                novaInstituicao.TipoRede = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.TipoRedeInstituicao);
                novaInstituicao.Gre = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.GreInstituicao);

                // IDEB e IDHM normalmente são números (double). Se na sua classe forem string, use ExtrairValidarTexto.
                // Se forem double, ideal é ter um ExtrairValidarDouble no ValidationMaker. Deixarei como texto genérico por segurança:
                novaInstituicao.OfertaEnsino = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.OfertaEnsinoInstituicao);
                novaInstituicao.AdereTempoIntegral = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.AdereTempoIntegralInstituicao);
                novaInstituicao.TipologiaMunicipio = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.TipologiaMunicipioInstituicao);
                novaInstituicao.ApoioFinanceiro = ExtrairValidarTexto(row, ExcelHelper.ColunasFeiras.ApoioFinanceiroInstituicao);

                // 4. INSERÇÃO DE CONTATOS
                AdicionarContatos(row, novaInstituicao);
            }

            // 5. SALVAR NO BANCO DE DADOS
            _db.Instituicoes.Add(novaInstituicao);
            await _db.SaveChangesAsync();

            return novaInstituicao;
        }

        private void AdicionarContatos(IXLRow row, Instituicao instituicao)
        {
            string? email = ExtrairValidarEmail(row, ExcelHelper.ColunasFeiras.EmailInstituicao);
            string? telefone = ExtrairValidarTelefone(row, ExcelHelper.ColunasFeiras.TelefoneInstituicao);

            if (email != null) instituicao.Email.Add(new Email(email));
            if (telefone != null) instituicao.Telefone.Add(new Telefone(telefone));
        }
    }
}
