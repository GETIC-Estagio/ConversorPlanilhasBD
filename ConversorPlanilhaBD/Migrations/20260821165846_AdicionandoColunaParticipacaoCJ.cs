using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConversorPlanilhaBD.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoColunaParticipacaoCJ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Identidades_PessoaId",
                table: "Identidades");

            migrationBuilder.DropColumn(
                name: "QuantosProjetos",
                table: "Feiras");

            migrationBuilder.RenameColumn(
                name: "IdGenero",
                table: "Pessoas",
                newName: "Genero");

            migrationBuilder.RenameColumn(
                name: "Participante",
                table: "Instituicoes",
                newName: "ParticipacaoCienciaJovem");

            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "Identidades",
                newName: "RG");

            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Identidades",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Identidades_PessoaId",
                table: "Identidades",
                column: "PessoaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Identidades_PessoaId",
                table: "Identidades");

            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Identidades");

            migrationBuilder.RenameColumn(
                name: "Genero",
                table: "Pessoas",
                newName: "IdGenero");

            migrationBuilder.RenameColumn(
                name: "ParticipacaoCienciaJovem",
                table: "Instituicoes",
                newName: "Participante");

            migrationBuilder.RenameColumn(
                name: "RG",
                table: "Identidades",
                newName: "Numero");

            migrationBuilder.AddColumn<int>(
                name: "QuantosProjetos",
                table: "Feiras",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Identidades_PessoaId",
                table: "Identidades",
                column: "PessoaId");
        }
    }
}
