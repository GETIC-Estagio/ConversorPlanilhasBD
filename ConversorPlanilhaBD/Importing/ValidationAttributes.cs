using ClosedXML.Excel;
using ConversorPlanilhaBD.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing
{
    public class ValidationAttributes
    {

        private void EnviarErro(int linha, string mensagem)
        {
            _resultado.RegistrarErro(linha, mensagem);
        }


        private string? ExtrairValidarNome(IXLRow row, int coluna)
        {
            try
            {
                string valor = ExcelHelper.ObterValor(row, coluna);
                ValidationHelper.VerificarNome(valor);
                return valor;
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Nome do responsável inválido: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }

        private DateOnly? ExtrairValidarData(IXLRow row, int coluna)
        {
            try
            {
                return ValidationHelper.VerificarDateOnly(ExcelHelper.ObterValor(row, coluna));
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Data de nascimento do responsável inválida: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }


        private string? ExtrairValidarTexto(IXLRow row, int coluna)
        {
            try
            {
                string valor = ExcelHelper.ObterValor(row, coluna);
                ValidationHelper.VerificarTexto(valor);
                return valor;
            }
            catch
            {
                EnviarErro(row.RowNumber(), $"Texto inválido na coluna {coluna}: {ExcelHelper.ObterValor(row, coluna)}");
                return null;
            }
        }
    }
}
