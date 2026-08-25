using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Enums
{
    /// <summary>
    /// Guarda se o modelo os status de um modelo que pode ser cadastrado
    /// </summary>
    public enum EnumStatus
    {
        Ativo = 1, //Quando é criado ou pode ser usado é ativo 
        Inativo //Quando é deletado ou não pode ser usado é inativo 
    }
}
