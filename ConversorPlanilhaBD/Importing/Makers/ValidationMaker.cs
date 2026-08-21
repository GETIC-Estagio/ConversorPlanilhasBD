using ClosedXML.Excel;
using ConversorPlanilhaBD.Importacao;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing.Makers
{
    /// <summary>
    /// Essa classe é responsável por fornecer métodos de validação e extração de dados de uma planilha Excel,
    /// registrando erros no resultado da importação quando necessário.
    /// </summary>
    public abstract class ValidationMaker
    {

        protected readonly ResultadoImportacao _resultado;

        protected ValidationMaker(ResultadoImportacao resultado)
        {
            _resultado = resultado;
        }

        protected void EnviarErro(int linha, string mensagem)
        {
            _resultado.RegistrarErro(linha, mensagem);
        }


        protected string? ExtrairValidarNome(IXLRow row, int coluna)
        {
            try
            {
                string? valor = ExcelHelper.ObterValor(row, coluna);

                if (string.IsNullOrWhiteSpace(valor))
                {
                    throw new ArgumentException("A célula está vazia.");
                }

                ValidationHelper.VerificarNome(valor);

                return valor;
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Nome inválido: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }

        protected DateOnly? ExtrairValidarData(IXLRow row, int coluna)
        {
            try
            {
                string? valor = ExcelHelper.ObterValor(row, coluna);

                if (string.IsNullOrWhiteSpace(valor))
                {
                    throw new ArgumentException("A célula está vazia.");
                }

                return ValidationHelper.VerificarDateOnly(valor);
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Data de nascimento inválida: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }


        protected string? ExtrairValidarTexto(IXLRow row, int coluna)
        {
            try
            {
                string? valor = ExcelHelper.ObterValor(row, coluna);

                if (string.IsNullOrWhiteSpace(valor))
                {
                    throw new ArgumentException("A célula está vazia.");
                }

                ValidationHelper.VerificarTexto(valor);
                return valor;
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Texto inválido na : {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }

        protected string? ExtrairValidarEmail(IXLRow row, int coluna)
        {
            try
            {
                string? valor = ExcelHelper.ObterValor(row, coluna);
                if (string.IsNullOrWhiteSpace(valor))
                {
                    throw new ArgumentException("A célula está vazia.");
                }
                ValidationHelper.VerificarEmail(valor);
                return valor;
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Email inválido: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }

        protected string? ExtrairValidarTelefone(IXLRow row, int coluna)
        {
            try
            {
                string? valor = ExcelHelper.ObterValor(row, coluna);
                if (string.IsNullOrWhiteSpace(valor))
                {
                    throw new ArgumentException("A célula está vazia.");
                }
                return ValidationHelper.VerificarTelefone(valor);
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Telefone inválido: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }
    }
}
