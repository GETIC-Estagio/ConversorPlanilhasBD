using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Enums
{
    /// <summary>
    /// Guarda a oferta de ensino da instituicao
    /// </summary>
    [Flags]
    public enum EnumOfertaEnsino
    {
        Regular = 1 << 0,
        
        Integral = 1 << 1,

        SemiIntegral = 1 << 2,

        EJA = 1 << 3
    }
}
