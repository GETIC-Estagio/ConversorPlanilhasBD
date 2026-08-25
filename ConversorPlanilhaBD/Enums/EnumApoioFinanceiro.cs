using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Enums
{
    /// <summary>
    /// Guarda se a instituicao recebeu apoio financeiro e de quem
    /// </summary>
    [Flags]
    public enum EnumApoioFinanceiro
    {
        Nao,
        SecretariaMunicipalEducacao = 1 >> 0,
        SecretariaEstadualEducacao = 1 >> 1,
        CienciaJovemCNPq = 1 >> 2,
        SEDUCTO = 1 >> 3,
        Comunidade = 1 >> 4,
        MinisterioDefesa = 1 >> 5,
        OrgaoFederalEducacao = 1 >> 6,
        Edital = 1 >> 7
    }
}
