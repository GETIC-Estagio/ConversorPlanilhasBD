using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Enums
{
    /// <summary>
    /// Guarda o nivel de ensino de forma combinável (Flags)
    /// </summary>
    [Flags]
    public enum EnumNivelEnsino
    {
        EducacaoInfantil = 1 << 0,

        EnsinoFundamentalI = 1 << 1,

        EnsinoFundamentalII = 1 << 2,

        EJAFundamental = 1 << 3,

        EnsinoMedio = 1 << 4,

        EnsinoMedioTecnico = 1 << 5,

        EJAMedio = 1 << 6,

        EnsinoTecnico = 1 << 7,

        Tecnologo = 1 << 8,

        Bacharelado = 1 << 9,

        Licenciatura = 1 << 10,

        Especializacao = 1 << 11,

        PosGraduacao = 1 << 12,

        Mestrado = 1 << 13,

        Doutorado = 1 << 14
    }
}
