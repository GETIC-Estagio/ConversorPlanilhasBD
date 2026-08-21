using ClosedXML.Excel;
using ConversorPlanilhaBD.Importing;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ConversorPlanilhaBD.Validators
{
    /// <summary>
    /// Serve para validar dados de entrada, como nomes, datas, emails, telefones, identidades, CNPJs, países e estados brasileiros.
    /// </summary>
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

        //Verifica se não é vazio e se contem um @
        public static void VerificarEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email não pode ser vazio");

            if (!email.Contains('@')) throw new ArgumentException("Email precisa conter um @");
        }

        //Verifica não é vazio
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

        //Verificar RG é difícil, então apenas verificado se não é vazio e se contem só números
        public static string VerificarRG(string rg, string orgaoExpedidor)
        {
            if (string.IsNullOrEmpty(rg)) throw new ArgumentException("A identidade não pode ser vazia");

            rg = rg.Replace(".", "").Replace("-", "").Replace(" ", "");

            foreach (char c in rg)
            {
                if (!char.IsDigit(c)) throw new ArgumentException("Identidade só pode conter números");
            }

            VerificarTexto(orgaoExpedidor);

            return rg;
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

        //Verifica se o cpf é valido 
        //e retorna ele somente com numeros
        public static string VerificarCPF(string cpf)
        {
            int digito;
            int soma = 0;
            List<int> numeros_cpf = new List<int>();
            List<int> pesos = new List<int> { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            if (string.IsNullOrEmpty(cpf))
            {
                throw new ArgumentException("CPF não pode ser vazio");
            }

            cpf = cpf.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

            if (cpf.Length != 11)
            {
                throw new ArgumentException("CPF deve ter 11 dígitos");
            }


            foreach (char c in cpf)
            {
                if (!char.IsDigit(c))
                {
                    throw new ArgumentException("CPF só pode conter dígitos");
                }
                numeros_cpf.Add(c - '0');
            }


            for (int i = 0; i < 9; i++)
            {
                soma += numeros_cpf[i] * pesos[i + 1];
            }

            if (soma % 11 == 0 || soma % 11 == 1)
            {
                digito = 0;
            }
            else
            {
                digito = 11 - soma % 11;
            }

            if (digito != numeros_cpf[9])
            {
                throw new ArgumentException("Dígito Validador Incorreto");
            }

            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += numeros_cpf[i] * pesos[i];
            }

            if (soma % 11 == 0 || soma % 11 == 1)
            {
                digito = 0;
            }
            else
            {
                digito = 11 - soma % 11;
            }

            if (digito != numeros_cpf[10])
            {
                throw new ArgumentException("Dígito Validador Incorreto");
            }

            return cpf;
        }


        //Deixa somente as letras normalizadas, sem acentos, cedilha, etc
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

        //Verifica se dois objetos tem o mesmo cpf
        public static bool VerificarCPFIgual(string? cpf1, string? cpf2)
        {
            if (string.IsNullOrWhiteSpace(cpf1) ||
                string.IsNullOrWhiteSpace(cpf2))
                return false;

            //Cpf1 vem da planilha, portanto precisa ser validado
            //Cpf2 vem do banco de dados, portanto já foi validado
            cpf1 = VerificarCPF(cpf1);
            
            return string.Equals(
                cpf1,
                cpf2,
                StringComparison.OrdinalIgnoreCase
            );
        }

        // Verifica se é um DateTime válido (Data e Hora)
        public static DateTime VerificarDateTime(string dataHora)
        {
            if (string.IsNullOrWhiteSpace(dataHora))
                throw new ArgumentException("A data e hora não podem ser vazias");

            dataHora = dataHora.Trim();

            // Tenta os formatos específicos que o seu sistema gera
            string[] formatosEsperados = { "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy" };

            if (DateTime.TryParseExact(dataHora, formatosEsperados, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultado))
            {
                return resultado;
            }

            // Tenta um fallback para qualquer data válida no padrão brasileiro (pt-BR)
            if (DateTime.TryParse(dataHora, new CultureInfo("pt-BR"), DateTimeStyles.None, out resultado))
            {
                return resultado;
            }

            throw new FormatException("Data e hora em formato inválido. Use o padrão dd/MM/yyyy HH:mm:ss");
        }

        // Verifica se é um DateOnly válido (Apenas Data)
        public static DateOnly VerificarDateOnly(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("A data não pode ser vazia");

            data = data.Trim();

            // Tenta os formatos específicos
            string[] formatosEsperados = { "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };

            // Tenta o TryParseExact primeiro
            if (DateOnly.TryParseExact(data, formatosEsperados, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly resultado))
            {
                return resultado;
            }

            // Tenta um fallback para o padrão brasileiro (pt-BR)
            if (DateOnly.TryParse(data, new CultureInfo("pt-BR"), DateTimeStyles.None, out resultado))
            {
                return resultado;
            }

            throw new FormatException("Data em formato inválido. Use o padrão dd/MM/yyyy");
        }
    }
}
