using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConversorPlanilhaBD.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instituicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    CNPJ = table.Column<string>(type: "text", nullable: true),
                    Pais = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: true),
                    Municipio = table.Column<string>(type: "text", nullable: true),
                    Endereco = table.Column<string>(type: "text", nullable: true),
                    TipoRede = table.Column<string>(type: "text", nullable: true),
                    GRE = table.Column<string>(type: "text", nullable: true),
                    IDEB = table.Column<double>(type: "double precision", nullable: true),
                    IDHM = table.Column<double>(type: "double precision", nullable: true),
                    Participante = table.Column<string>(type: "text", nullable: true),
                    OfertaEnsino = table.Column<string>(type: "text", nullable: true),
                    Adere = table.Column<string>(type: "text", nullable: true),
                    TipologiaMunicipio = table.Column<string>(type: "text", nullable: true),
                    ApoioFinanceiro = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instituicoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuxInstituicoesResponsaveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    InstituicaoId = table.Column<int>(type: "integer", nullable: true),
                    FuncaoInstituicao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuxInstituicoesResponsaveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuxInstituicoesResponsaveis_Instituicoes_InstituicaoId",
                        column: x => x.InstituicaoId,
                        principalTable: "Instituicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Endereco = table.Column<string>(type: "text", nullable: true),
                    PessoaId = table.Column<int>(type: "integer", nullable: true),
                    InstituicaoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emails_Instituicoes_InstituicaoId",
                        column: x => x.InstituicaoId,
                        principalTable: "Instituicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    Alcance = table.Column<string>(type: "text", nullable: true),
                    Endereco = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: true),
                    PeriodoRealizacao = table.Column<string>(type: "text", nullable: true),
                    DataRealizacao = table.Column<string>(type: "text", nullable: true),
                    ModalidadeParticipacao = table.Column<string>(type: "text", nullable: true),
                    NumProjetos = table.Column<int>(type: "integer", nullable: true),
                    AreasConhecimento = table.Column<string>(type: "text", nullable: true),
                    NivelEnsino = table.Column<string>(type: "text", nullable: true),
                    NumEscolas = table.Column<int>(type: "integer", nullable: true),
                    Afiliada = table.Column<string>(type: "text", nullable: true),
                    ProcessoSelecao = table.Column<string>(type: "text", nullable: true),
                    PeriodoElaboracao = table.Column<string>(type: "text", nullable: true),
                    ProjetosAvaliados = table.Column<string>(type: "text", nullable: true),
                    QuantosProjetos = table.Column<int>(type: "integer", nullable: true),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InstituicaoId = table.Column<int>(type: "integer", nullable: true),
                    InstituicaoOrganizadoraId = table.Column<int>(type: "integer", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    ResponsavelContatoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feiras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feiras_Instituicoes_InstituicaoId",
                        column: x => x.InstituicaoId,
                        principalTable: "Instituicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Feiras_Instituicoes_InstituicaoOrganizadoraId",
                        column: x => x.InstituicaoOrganizadoraId,
                        principalTable: "Instituicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Identidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "text", nullable: true),
                    OrgaoExpedidor = table.Column<string>(type: "text", nullable: true),
                    PessoaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    IdGenero = table.Column<string>(type: "text", nullable: true),
                    Raca = table.Column<string>(type: "text", nullable: true),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Professores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    NumMatricula = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Professores_Pessoas_Id",
                        column: x => x.Id,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Responsaveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Professor = table.Column<string>(type: "text", nullable: true),
                    NivelEnsino = table.Column<string>(type: "text", nullable: true),
                    Participante = table.Column<string>(type: "text", nullable: true),
                    Experiencia = table.Column<string>(type: "text", nullable: true),
                    Recomendacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsaveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responsaveis_Pessoas_Id",
                        column: x => x.Id,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Telefones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "text", nullable: true),
                    PessoaId = table.Column<int>(type: "integer", nullable: true),
                    InstituicaoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telefones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Telefones_Instituicoes_InstituicaoId",
                        column: x => x.InstituicaoId,
                        principalTable: "Instituicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Telefones_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    Deficiencia = table.Column<string>(type: "text", nullable: true),
                    Participacao = table.Column<string>(type: "text", nullable: true),
                    CategoriaInscricao = table.Column<string>(type: "text", nullable: true),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PalavrasChave = table.Column<string>(type: "text", nullable: true),
                    ODS = table.Column<string>(type: "text", nullable: true),
                    Tema = table.Column<string>(type: "text", nullable: true),
                    Area = table.Column<string>(type: "text", nullable: true),
                    Objetivo = table.Column<string>(type: "text", nullable: true),
                    Resumo = table.Column<string>(type: "text", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    ProfessorId = table.Column<int>(type: "integer", nullable: true),
                    FeiraId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projetos_Feiras_FeiraId",
                        column: x => x.FeiraId,
                        principalTable: "Feiras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projetos_Professores_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Professores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Projetos_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuxInstituicoesResponsaveis_InstituicaoId",
                table: "AuxInstituicoesResponsaveis",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AuxInstituicoesResponsaveis_ResponsavelId",
                table: "AuxInstituicoesResponsaveis",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_InstituicaoId",
                table: "Emails",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_PessoaId",
                table: "Emails",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Feiras_InstituicaoId",
                table: "Feiras",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Feiras_InstituicaoOrganizadoraId",
                table: "Feiras",
                column: "InstituicaoOrganizadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Feiras_ResponsavelContatoId",
                table: "Feiras",
                column: "ResponsavelContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Feiras_ResponsavelId",
                table: "Feiras",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Identidades_PessoaId",
                table: "Identidades",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_ProjetoId",
                table: "Pessoas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_FeiraId",
                table: "Projetos",
                column: "FeiraId");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_ProfessorId",
                table: "Projetos",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_ResponsavelId",
                table: "Projetos",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Telefones_InstituicaoId",
                table: "Telefones",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Telefones_PessoaId",
                table: "Telefones",
                column: "PessoaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuxInstituicoesResponsaveis_Responsaveis_ResponsavelId",
                table: "AuxInstituicoesResponsaveis",
                column: "ResponsavelId",
                principalTable: "Responsaveis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Pessoas_PessoaId",
                table: "Emails",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feiras_Responsaveis_ResponsavelContatoId",
                table: "Feiras",
                column: "ResponsavelContatoId",
                principalTable: "Responsaveis",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Feiras_Responsaveis_ResponsavelId",
                table: "Feiras",
                column: "ResponsavelId",
                principalTable: "Responsaveis",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Identidades_Pessoas_PessoaId",
                table: "Identidades",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoas_Projetos_ProjetoId",
                table: "Pessoas",
                column: "ProjetoId",
                principalTable: "Projetos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feiras_Instituicoes_InstituicaoId",
                table: "Feiras");

            migrationBuilder.DropForeignKey(
                name: "FK_Feiras_Instituicoes_InstituicaoOrganizadoraId",
                table: "Feiras");

            migrationBuilder.DropForeignKey(
                name: "FK_Feiras_Responsaveis_ResponsavelContatoId",
                table: "Feiras");

            migrationBuilder.DropForeignKey(
                name: "FK_Feiras_Responsaveis_ResponsavelId",
                table: "Feiras");

            migrationBuilder.DropForeignKey(
                name: "FK_Projetos_Responsaveis_ResponsavelId",
                table: "Projetos");

            migrationBuilder.DropForeignKey(
                name: "FK_Professores_Pessoas_Id",
                table: "Professores");

            migrationBuilder.DropTable(
                name: "AuxInstituicoesResponsaveis");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "Identidades");

            migrationBuilder.DropTable(
                name: "Telefones");

            migrationBuilder.DropTable(
                name: "Instituicoes");

            migrationBuilder.DropTable(
                name: "Responsaveis");

            migrationBuilder.DropTable(
                name: "Pessoas");

            migrationBuilder.DropTable(
                name: "Projetos");

            migrationBuilder.DropTable(
                name: "Feiras");

            migrationBuilder.DropTable(
                name: "Professores");
        }
    }
}
