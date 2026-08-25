using ConversorPlanilhaBD.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Model
{
    public abstract class ModelCadastro : ModelBase
    {
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public EnumStatus Status { get; set; }
        public EnumImportacao Importacao { get; set; }
    }
}
