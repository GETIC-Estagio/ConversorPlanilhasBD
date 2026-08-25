using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Enums
{
    /// <summary>
    /// Guarda as areas de Conhecimento,
    /// utiliza flags por poder ter mais de uma area ao mesmo tempo
    /// </summary>
    public enum EnumAreasConhecimento
    {
        // Potências de 2 geradas automaticamente via deslocamento de bits (Shift)
        CienciasExatasETerra = 1 << 0, // 1
        CienciasBiologicas = 1 << 1, // 2
        Engenharias = 1 << 2, // 4
        CienciasDaSaude = 1 << 3, // 8
        CienciasAgrarias = 1 << 4, // 16 etc...
        CienciasSociaisAplicadas = 1 << 5,
        CienciasHumanas = 1 << 6,
        LinguisticaLetrasEArtes = 1 << 7,
        Multidisciplinar = 1 << 8
    }
}
