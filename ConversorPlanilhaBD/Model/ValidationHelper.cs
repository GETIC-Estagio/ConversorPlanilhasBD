using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ConversorPlanilhaBD.Model
{
    //Cria uma classe estatica para ajudar com as validacoes
    public static class ValidationHelper
    {
        //Serve para verificar que um país existe
        private static readonly List<RegionInfo> PaisesValidos = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(c =>
        {
            try { return new RegionInfo(c.Name); }
            catch { return null; }// Ignora culturas que não possuem região
        })
        .Where(r => r != null)
        .GroupBy(r => r!.GeoId) 
        .Select(g => g.First()!) // Remove países duplicados (ex: en-US e es-US apontam para o mesmo país)
        .ToList();

        //Lista de Estados Brasileiros
        private static readonly Dictionary<string, string> EstadosBrasileiros = new(StringComparer.OrdinalIgnoreCase)
        {
            { "AC", "Acre" }, { "AL", "Alagoas" }, { "AP", "Amapa" }, { "AM", "Amazonas" },
            { "BA", "Bahia" }, { "CE", "Ceara" }, { "DF", "Distrito Federal" }, { "ES", "Espirito Santo" },
            { "GO", "Goias" }, { "MA", "Maranhao" }, { "MT", "Mato Grosso" }, { "MS", "Mato Grosso do Sul" },
            { "MG", "Minas Gerais" }, { "PA", "Para" }, { "PB", "Paraiba" }, { "PR", "Parana" },
            { "PE", "Pernambuco" }, { "PI", "Piaui" }, { "RJ", "Rio de Janeiro" }, { "RN", "Rio Grande do Norte" },
            { "RS", "Rio Grande do Sul" }, { "RO", "Rondonia" }, { "RR", "Roraima" }, { "SC", "Santa Catarina" },
            { "SP", "Sao Paulo" }, { "SE", "Sergipe" }, { "TO", "Tocantins" }
        };

        // Verifica se o nome é vazio/null/ou tem digitos
        public static void VerificarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome não pode ser vazio");

            foreach (char c in nome)
            {
                if (!char.IsLetter(c) && c != ' ') throw new ArgumentException("Nomes só podem conter letras");
            }
        }

        //Tenta fazer a conversão de uma string para o formato "dd/MM/yyyy"
        public static DateOnly VerificarData(string data)
        {
            try
            {
                return DateOnly.ParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException($"Data não pode ser vazia. {e}");
            }
            catch (FormatException e)
            {
                throw new FormatException($"Data precisa seguir o padrão dd/MM/aaaa. {e}");
            }
            catch (Exception e)
            {
                throw new Exception($"Ocorreu um erro inesperado. {e}");
            }
        }

        //Tenta fazer a conversão de uma string para o formato "dd/MM/yyyy HH:mm:ss"
        public static DateTime VerificarDataHora(string data)
        {
            try
            {
                return DateTime.ParseExact(data, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException($"Data não pode ser vazia. {e}");
            }
            catch (FormatException e)
            {
                throw new FormatException($"Data precisa seguir o padrão dia/mes/ano hora:minuto:segundo. {e}");
            }
            catch (Exception e)
            {
                throw new Exception($"Ocorreu um erro inesperado. {e}");
            }
        }


        //So valida se contem algo
        //E se tem arroba
        //Validacao adicional é necessaria mais tarde
        public static void VerificarEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email não pode ser vazio");

            if (!email.Contains('@')) throw new ArgumentException("Email precisa conter um @");
        }

        //Verifica se numero não é vazio
        //Retira possiveis erros do usuario
        //Verifica que é só número
        public static string VerificarTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone)) throw new ArgumentException("Telefone não pode ser vazio");

            telefone = telefone.Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(" ", "");

            foreach (char c in telefone)
            {
                if (!char.IsDigit(c)) throw new ArgumentException("Telefone só pode conter números");
            }
            return telefone;
        }

        //RG não é padronizado não tem como saber se é CPF ou RG ou se é um erro
        //Então so verifica que não é vazio e só numero
        public static string VerificarIdentidade(string identidade)
        {
            if (string.IsNullOrEmpty(identidade)) throw new ArgumentException("A identidade não pode ser vazia");

            identidade = identidade.Replace(".", "").Replace("-", "").Replace(" ", "");

            foreach (char c in identidade)
            {
                if (!char.IsDigit(c)) throw new ArgumentException("Identidade só pode conter números");
            }
            return identidade;
        }

        //Verifica que é o texto não é vazio
        public static void VerificarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) throw new ArgumentException("O campo não pode ser vazio");
        }

        //Verifica se o cnpj é valido 
        //e retorna ele somente com numeros
        public static string VerificarCNPJ(string cnpj)
        {
            int digito;
            int soma = 0;
            List<int> numeros_cnpj = new List<int>();
            List<int> pesos = new List<int> { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            if (string.IsNullOrEmpty(cnpj))
            {
                throw new ArgumentException("CNPJ não pode ser vazio");
            }

            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

            if (cnpj.Length != 14)
            {
                throw new ArgumentException("CNPJ deve ter 14 dígitos");
            }


            foreach (char c in cnpj)
            {
                if (!char.IsDigit(c))
                {
                    throw new ArgumentException("CNPJ só pode conter dígitos");
                }
                numeros_cnpj.Add(c - '0');
            }


            for (int i = 0; i < 12; i++)
            {
                soma += numeros_cnpj[i] * pesos[i + 1];
            }

            if (soma % 11 < 2)
            {
                digito = 0;
            }
            else
            {
                digito = 11 - soma % 11;
            }

            if (digito != numeros_cnpj[12])
            {
                throw new ArgumentException("Dígito Validador Incorreto");
            }

            soma = 0;

            for (int i = 0; i < 13; i++)
            {
                soma += numeros_cnpj[i] * pesos[i];
            }

            if (soma % 11 < 2)
            {
                digito = 0;
            }
            else
            {
                digito = 11 - soma % 11;
            }

            if (digito != numeros_cnpj[13])
            {
                throw new ArgumentException("Dígito Validador Incorreto");
            }

            return cnpj;
        }

        
        //Classe para remover acentos das palavras
        public static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string textoNormal = texto.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();

            foreach (char c in textoNormal)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }


        //verificar se o pais de fato existe
        public static void VerificarPais(string pais)
        {
            if (string.IsNullOrEmpty(pais)) throw new ArgumentException("O campo não pode ser vazio");

            pais = RemoverAcentos(pais.Trim());

            bool paisExiste = PaisesValidos.Any(r =>
                RemoverAcentos(r.DisplayName).Equals(pais, StringComparison.OrdinalIgnoreCase) ||
                RemoverAcentos(r.EnglishName).Equals(pais, StringComparison.OrdinalIgnoreCase) ||
                r.TwoLetterISORegionName.Equals(pais, StringComparison.OrdinalIgnoreCase)); //Aceita siglas US, BR

            if (!paisExiste) throw new ArgumentException("País não existe");
        }


        //Verifica apenas estados do brasil
        public static void VerificarEstadoBR(string estado)
        {
            if (string.IsNullOrEmpty(estado)) throw new ArgumentException("O campo não pode ser vazio");

            estado = RemoverAcentos(estado.Trim());

            bool estadoExiste = EstadosBrasileiros.Any(e =>
                e.Key.Equals(estado, StringComparison.OrdinalIgnoreCase) ||
                e.Value.Equals(estado, StringComparison.OrdinalIgnoreCase)
            );

            if (!estadoExiste) throw new ArgumentException("Estado não existe");
        }

        //Verifica numeros double
        public static double VerificarNumeroDouble(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("O campo não pode ser vazio");

            try
            {
                return Double.Parse(text, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new FormatException("Esse campo só vale números", e);
            }
            catch (OverflowException e)
            {
                throw new OverflowException($"Digite um número entre {double.MaxValue} e {double.MinValue}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"Ocorreu um erro inesperado", e);
            }
        }

        //Verifica numeros int
        public static int VerificarNumeroInt(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("O campo não pode ser vazio");

            try
            {
                return int.Parse(text, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new FormatException("Esse campo só vale números", e);
            }
            catch (OverflowException e)
            {
                throw new OverflowException($"Digite um número entre {int.MaxValue} e {int.MinValue}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"Ocorreu um erro inesperado", e);
            }
        }

        //Verifica se dois objetos tem o mesmo nome
        public static bool VerificarMesmoNome(string? nome1, string? nome2)
        {
            if (string.IsNullOrWhiteSpace(nome1) ||
                string.IsNullOrWhiteSpace(nome2))
                return false;

            return string.Equals(
                nome1.Trim(),
                nome2.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}
