using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Importing
{
    /// <summary>
    /// Essa classe contém métodos auxiliares para trabalhar com planilhas Excel,
    /// como obter valores de células e definir constantes para colunas específicas.
    /// </summary>
    public static class ExcelHelper
    {
        // Serve para obter o valor de uma célula de uma linha específica, considerando a coluna desejada.
        public static string? ObterValor(IXLRow row, int coluna)
        {
            var celula = row.Cell(coluna);
            //Se a celula for do tipo data, força formatação BR
            if (celula.DataType == XLDataType.DateTime)
            {
                try
                {
                    DateTime dataHoraNativa = celula.GetDateTime();

                    //Caso 1: Transforma em dd/MM/yyyy HH:mm:ss
                    if (coluna == ColunasFeiras.CarimboDataHoraFeira || coluna == ColunasProjetos.CarimboDataHoraProjeto)
                    {
                        return dataHoraNativa.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }

                    //Caso2: Transforma somente em dd/MM/yyyy
                    return dataHoraNativa.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    //Se houver erro deixa do jeito que estava
                    return celula.GetString().Trim();
                }
            }

            return celula.GetString().Trim();
        }

        // ============================================================
        // COLUNAS DA ABA DE FEIRAS
        // ============================================================
        public static class ColunasFeiras
        {

            //=================================
            // RESPONSAVEL
            //=================================

            public const int EmailResponsavel = 2;
            public const int NomeResponsavel = 3;
            public const int SobrenomeResponsavel = 4;
            public const int DataNascimentoResponsavel = 5;
            public const int CPF_Responsavel = 6;
            public const int OutroEmailResponsavel = 7;
            public const int TelefoneResponsavel = 8;
            public const int IdentidadeGeneroResponsavel = 9;
            public const int RacaResponsavel = 10;
            public const int EhProfessorResponsavel = 11;
            public const int NivelEnsinoResponsavel = 12;
            public const int ParticipouCienciaJovemResponsavel = 13;
            public const int ExperienciaFeirasResponsavel = 14;
            public const int RecomendacaoResponsavel = 91;

            //=================================
            // INSTITUIÇÃO
            //=================================

            public const int CNPJ_Instituicao = 15;
            public const int NomeInstituicao = 16;
            public const int PaisInstituicao = 18;
            public const int EstadoInstituicao = 19;
            public const int MunicipioInstituicao = 20;
            public const int EnderecoInstituicao = 21;
            public const int TipoRedeInstituicao = 22;
            public const int TelefoneInstituicao = 23;
            public const int EmailInstituicao = 24;
            public const int GreInstituicao = 25;
            public const int IdebInstituicao = 26;
            public const int IdhmInstituicao = 27;
            public const int OfertaEnsinoInstituicao = 29;
            public const int AdereTempoIntegralInstituicao = 30;
            public const int TipologiaMunicipioInstituicao = 31;
            public const int ParticipouCienciaJovemInstituicao = 86;
            public const int AnosParticipacaoInstituicao = 87;
            public const int QuantosProjetosApresentadosInstituicao = 88;
            public const int ApoioFinanceiroInstituicao = 89;

            //=================================
            // AUXILIAR RESPONSAVEL-INSTITUIÇÃO
            //=================================

            public const int FuncaoResponsavelInstituicao = 17;


            //=================================
            // FEIRA
            //=================================
            public const int CarimboDataHoraFeira = 1;
            public const int NomeFeira = 66;
            public const int AlcanceFeira = 68;
            public const int EnderecoFeira = 69;
            public const int EstadoFeira = 70;
            public const int PeriodoRealizacaoFeira = 74;
            public const int DataRealizacaoFeira = 75;
            public const int ModalidadeParticipacaoFeira = 76;
            public const int NumeroProjetosParticipantesFeira = 77;
            public const int AreasConhecimentoFeira = 78;
            public const int NivelEnsinoAlunosFeira = 79;
            public const int NumeroEscolasParticipantesFeira = 80;
            public const int FeiraAfiliada = 81;
            public const int ProcessoSelecaoFeira = 82;
            public const int PeriodoElaboracaoFeira = 83;
            public const int ProjetosAvaliadosFeira = 84;
            public const int FormaAvaliacaoFeira = 85;

            //=================================
            // INSTITUICAO ORGANIZADORA
            //=================================

            public const int InstituicaoOrganizadora = 67;

            //=================================
            // RESPONSAVEL DE CONTATO DA FEIRA
            //=================================

            public const int NomeResponsavelContatoFeira = 71;
            public const int TelefoneContatoFeira = 72;
            public const int EmailContatoFeira = 73;

        }

        // ============================================================
        // COLUNAS DA ABA DE Pré-Projetos
        // ============================================================
        public static class ColunasProjetos
        {
            //=================================
            // RESPONSAVEL
            //=================================

            public const int EmailResponsavel = 1;
            public const int NomeCompletoResponsavel = 2;
            public const int DataNascimentoResponsavel = 3;
            public const int CPF_Responsavel = 4;
            public const int OutroEmailResponsavel = 5;
            public const int TelefoneResponsavel = 6;
            public const int IdentidadeGeneroResponsavel = 7;
            public const int RacaResponsavel = 8;
            public const int ehProfessorResponsavel = 9;
            public const int NivelEnsinoResponsavel = 10;
            public const int ParticipanteResponsavel = 11;
            public const int ExperienciaResponsavel = 12;
            public const int Recomendacao = 90;

            //=================================
            // Instituicao
            //=================================

            public const int CNPJ_Instituicao = 13;
            public const int NomeInstituicao = 14;
            public const int PaisInstituicao = 16;
            public const int EstadoInstituicao = 17;
            public const int MunicipioInstituicao = 18;
            public const int EnderecoInstituicao = 19;
            public const int TipoRedeInstituicao = 20;
            public const int TelefoneInstituicao = 21;
            public const int EmailInstituicao = 22;
            public const int GREInstituicao = 23;
            public const int IDEBInstituicao = 24;
            public const int IDHMInstituicao = 25;
            public const int ParticipouCienciaJovemInstituicao = 85;
            public const int OfertaEnsinoInstituicao = 27;
            public const int AdereInstituicao = 28;

            //=================================
            // AUXILIAR RESPONSAVEL-INSTITUIÇÃO
            //=================================

            public const int FuncaoResponsavelInstituicao = 15;

            //=================================
            // PROFESSOR
            //=================================

            public const int NomeProfessor = 31;
            public const int MatriculaProfessor = 32;
            public const int RGProfessor = 33;
            public const int OrgaoExpedidorProfessor = 34;
            public const int CPF_Professor = 35;
            public const int RacaProfessor = 36;
            public const int GeneroProfessor = 37;
            public const int TelefoneProfessor = 38;
            public const int EmailProfessor = 39;

            //=================================
            // ALUNO1
            //=================================

            public const int NomeAluno1 = 40;
            public const int RGAluno1 = 41;
            public const int OrgaoExpedidorAluno1 = 42;
            public const int CPFAluno1 = 43;
            public const int RacaAluno1 = 44;
            public const int GeneroAluno1 = 45;
            public const int EmailAluno1 = 46;

            //=================================
            // ALUNO2
            //=================================

            public const int NomeAluno2 = 47;
            public const int RGAluno2 = 48;
            public const int OrgaoExpedidorAluno2 = 49;
            public const int CPFAluno2 = 50;
            public const int RacaAluno2 = 51;
            public const int GeneroAluno2 = 52;
            public const int EmailAluno2 = 53;

            //=================================
            // PROJETO
            //=================================

            public const int DeficienciaProjeto = 54;
            public const int ParticipacaoProjeto = 55;
            public const int CategoriaInscricaoProjeto = 56;
            public const int CarimboDataHoraProjeto = 57;
            public const int NomeProjeto = 58;
            public const int PalavrasChaveProjeto = 59;
            public const int ODSProjeto = 60;
            public const int TemaProjeto = 61;
            public const int AreaProjeto = 62;
            public const int ObjetivoProjeto = 63;
            public const int ResumoProjeto = 64;
        }
    }
}
