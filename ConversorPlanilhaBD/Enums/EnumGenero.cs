using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Enums
{
    /// <summary>
    /// Guarda as identidades de gênero de uma pessoa
    /// </summary>
    public enum EnumGenero
    {
        Masculino = 1,
        Feminino,
        HomemCisgenero,
        MulherCisgenero,
        HomemTransgenero,
        MulherTransgenero,
        NaoBinario,
        NaoInformado,
        Outro
    }
}
