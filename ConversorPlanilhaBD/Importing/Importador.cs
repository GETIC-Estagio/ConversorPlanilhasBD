using ClosedXML.Excel;
using ConversorPlanilhaBD.Model;
using ConversorPlanilhaBD.Model.AuxModels;
using ConversorPlanilhaBD.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using ConversorPlanilhaBD.Data;
using Microsoft.EntityFrameworkCore;

namespace ConversorPlanilhaBD.Importacao
{
    public class Importador
    {
        private readonly XLWorkbook _workbook;

        private readonly IXLWorksheet _feiras;
        private readonly IXLWorksheet _preProjetos;

        //Cria lista dos modelos necessários para serem inseridos depois no banco de dados
        private readonly List<Responsavel> _responsaveis = new();
        private readonly List<Instituicao> _instituicoes = new();

        private readonly List<Professor> _professores = new();
        private readonly List<Pessoa> _alunos = new();

        private readonly List<Feira> _feirasImportadas = new();
        private readonly List<Projeto> _projetosImportados = new();

        private readonly List<AuxInstituicaoResponsavel>
            _relacionamentosInstituicaoResponsavel = new();

        public event Action<int, int>? Progresso;
        public event Action<int, int>? ContadoresAtualizados;
        public event Action<string>? Erro;

        private readonly string _connectionString;

        public Importador(string caminhoArquivo, string connectionString)
        {
            //Verifica que não esta vazia
            if (string.IsNullOrWhiteSpace(caminhoArquivo))
                throw new ArgumentException(
                    "O caminho da planilha não pode ser vazio.",
                    nameof(caminhoArquivo));

            //Verifica que não esta vazia
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException(
                    "A string de conexão não pode ser vazia.",
                    nameof(connectionString));

            //Pega a string de conexão
            _connectionString = connectionString;

            //Pega o caminho do excel
            _workbook = new XLWorkbook(caminhoArquivo);

            //Pega as duas abas da planilha
            _feiras = _workbook.Worksheet("Feira afiliadas à 32ª edição");
            _preProjetos = _workbook.Worksheet("Pré-projetos da 32ª Edição");
        }

        public async Task<ResultadoImportacao> Importar()
        {
            //Guarda o contador de sucessos e erros
            //E o tipo de erro
            var resultado = new ResultadoImportacao();

            //Le a aba de feiras e salva tudo em listas
            ImportarFeiras(resultado);

            //Le a aba de projetos e salva tudo em listas
            ImportarPreProjetos(resultado);

            //Salva no banco de dados
            await SalvarNoBancoAsync(resultado);

            return resultado;
        }

        #region SalvarBanco
        private async Task SalvarNoBancoAsync(
            ResultadoImportacao resultado)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CienciaJovemDb>();
            optionsBuilder.UseNpgsql(_connectionString);

            await using var db =
                new CienciaJovemDb(optionsBuilder.Options);

            // ============================================================
            // RESPONSÁVEIS
            // ============================================================

            foreach (var responsavel in _responsaveis)
            {
                var telefonesDoResponsavel = responsavel.Telefone.ToList();
                var emailsDoResponsavel = responsavel.Email.ToList();
                var identidadesDoResponsavel = responsavel.Identidade.ToList();

                responsavel.Telefone.Clear();
                responsavel.Email.Clear();
                responsavel.Identidade.Clear();

                try
                {
                    db.Responsaveis.Add(responsavel);
                    await db.SaveChangesAsync();
                    await SalvarContatosPessoaAsync(db, responsavel.Id, telefonesDoResponsavel,
                        emailsDoResponsavel, identidadesDoResponsavel, responsavel.Nome, resultado);
                }
                catch (Exception ex)
                {
                    db.ChangeTracker.Clear(); // Remove o objeto que não conseguiu ser inserado

                    Erro?.Invoke($"Banco de dados - Erro ao inserir Responsável '{responsavel.Nome}': {ex.Message}");
                    resultado.RegistrarErro(0, $"Banco [Responsável: {responsavel.Nome}]: {ex.Message}");
                }
            }

            // ============================================================
            // INSTITUIÇÕES
            // ============================================================

            foreach (var instituicao in _instituicoes)
            {
                var telefonesDaInstituicao = instituicao.Telefone.ToList();
                var emailsDaInstituicao = instituicao.Email.ToList();

                instituicao.Telefone.Clear();
                instituicao.Email.Clear();

                try
                {
                    db.Instituicoes.Add(instituicao);
                    await db.SaveChangesAsync();
                    await SalvarContatosInstituicaoAsync(db, instituicao.Id, telefonesDaInstituicao,
                        emailsDaInstituicao, instituicao.Nome, resultado);
                }
                catch (Exception ex)
                {
                    db.ChangeTracker.Clear(); // Limpa a memória do rastreador

                    Erro?.Invoke($"Banco de dados - Erro ao inserir Instituição '{instituicao.Nome}': {ex.Message}");
                    resultado.RegistrarErro(0, $"Banco [Instituição: {instituicao.Nome}]: {ex.Message}");
                }
            }

            // ============================================================
            // FEIRAS
            // ============================================================

            foreach (var feira in _feirasImportadas)
            {
                try
                {
                    if (feira.Instituicao != null && feira.Instituicao.Id > 0)
                        feira.InstituicaoId = feira.Instituicao.Id;

                    if (feira.InstituicaoOrganizadora != null && feira.InstituicaoOrganizadora.Id > 0)
                        feira.InstituicaoOrganizadoraId = feira.InstituicaoOrganizadora.Id;

                    if (feira.Responsavel != null && feira.Responsavel.Id > 0)
                        feira.ResponsavelId = feira.Responsavel.Id;

                    if (feira.ResponsavelContato != null && feira.ResponsavelContato.Id > 0)
                        feira.ResponsavelContatoId = feira.ResponsavelContato.Id;

                    // Limpa as referências de objeto para focar apenas nas FKs numéricas salvas nos passos anteriores
                    feira.Instituicao = null;
                    feira.InstituicaoOrganizadora = null;
                    feira.Responsavel = null;
                    feira.ResponsavelContato = null;

                    db.Feiras.Add(feira);
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    db.ChangeTracker.Clear(); // Limpa a memória do rastreador

                    Erro?.Invoke($"Banco de dados - Erro ao inserir Feira '{feira.Nome}': {ex.Message}");
                    resultado.RegistrarErro(0, $"Banco [Feira: {feira.Nome}]: {ex.Message}");
                }
            }

            // ============================================================
            // PROFESSORES
            // ============================================================

            foreach (var professor in _professores)
            {
                var telefonesDoProfessor = professor.Telefone.ToList();
                var emailsDoProfessor = professor.Email.ToList();
                var identidadesDoProfessor = professor.Identidade.ToList();

                professor.Telefone.Clear();
                professor.Email.Clear();
                professor.Identidade.Clear();

                try
                {
                    db.Professores.Add(professor);
                    await db.SaveChangesAsync();
                    await SalvarContatosPessoaAsync(db, professor.Id, telefonesDoProfessor,
                        emailsDoProfessor, identidadesDoProfessor, professor.Nome, resultado);
                }
                catch (Exception ex)
                {
                    db.ChangeTracker.Clear(); // Remove o objeto que não conseguiu ser inserado

                    Erro?.Invoke($"Banco de dados - Erro ao inserir Professor '{professor.Nome}': {ex.Message}");
                    resultado.RegistrarErro(0, $"Banco [Professor: {professor.Nome}]: {ex.Message}");
                }
            }

            // ============================================================
            // PROJETOS
            // ============================================================

            foreach (var projeto in _projetosImportados)
            {
                try
                {
                    if (projeto.Responsavel != null && projeto.Responsavel.Id > 0)
                        projeto.ResponsavelId = projeto.Responsavel.Id;

                    if (projeto.Professor != null && projeto.Professor.Id > 0)
                        projeto.ProfessorId = projeto.Professor.Id;

                    projeto.Responsavel = null;
                    projeto.Professor = null;

                    db.Projetos.Add(projeto);
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    db.ChangeTracker.Clear();
                    Erro?.Invoke($"Banco - Erro no Projeto '{projeto.Nome}': {ex.Message}");
                    resultado.RegistrarErro(0, $"Banco [Projeto: {projeto.Nome}]: {ex.Message}");
                }
            }

            // ============================================================
            // ALUNOS (PESSOA)
            // ============================================================

            foreach (var projeto in _projetosImportados)
            {
                if (projeto.Id <= 0)
                {
                    foreach (var aluno in projeto.Alunos)
                    {
                        var telefonesDoAluno = aluno.Telefone.ToList();
                        var emailsDoAluno = aluno.Email.ToList();
                        var identidadesDoAluno = aluno.Identidade.ToList();

                        aluno.Telefone.Clear();
                        aluno.Email.Clear();
                        aluno.Identidade.Clear();

                        try
                        {
                            aluno.ProjetoId = projeto.Id;
                            aluno.Projeto = null;

                            db.Pessoas.Add(aluno);
                            await db.SaveChangesAsync();

                            await SalvarContatosPessoaAsync(db, aluno.Id, telefonesDoAluno, emailsDoAluno,
                                identidadesDoAluno, aluno.Nome, resultado);
                        }
                        catch (Exception ex)
                        {
                            db.ChangeTracker.Clear();
                            Erro?.Invoke($"Banco - Erro no Aluno '{aluno.Nome}': {ex.Message}");
                            resultado.RegistrarErro(0, $"Banco [Aluno: {aluno.Nome}]: {ex.Message}");
                        }
                    }
                }
            }

            // ============================================================
            // RELACIONAMENTOS RESPONSÁVEL <-> INSTITUIÇÃO
            // ============================================================

            foreach (var responsavel in _responsaveis)
            {
                foreach (var aux in responsavel.AuxInstituicaoResponsavel)
                {
                    try
                    {
                        // Se Responsavel ou Instituição falahou anteriormente Id deles = 0. 
                        // Se for não é inserido.
                        if (aux.Responsavel != null && aux.Responsavel.Id > 0)
                        {
                            aux.ResponsavelId = aux.Responsavel.Id;
                        }
                        if (aux.Instituicao != null && aux.Instituicao.Id > 0)
                        {
                            aux.InstituicaoId = aux.Instituicao.Id;
                        }

                        // Remove as referências de objeto para o EF persistir puramente pelas chaves mapeadas
                        aux.Responsavel = null;
                        aux.Instituicao = null;

                        // Só insere se as chaves forem válidas
                        if (aux.ResponsavelId > 0 && aux.InstituicaoId > 0)
                        {
                            db.AuxInstituicoesResponsaveis.Add(aux);
                            await db.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        db.ChangeTracker.Clear(); // Limpa a memória do rastreador

                        Erro?.Invoke($"Banco de dados - Relacionamento Responsável/Instituição falhou: {ex.Message}");
                        resultado.RegistrarErro(0, $"Banco [Vínculo Responsável/Instituição]: {ex.Message}");
                    }
                }
            }
        }

        private async Task SalvarContatosPessoaAsync(CienciaJovemDb db, int Id, List<Telefone> telefones, List<Email> emails,
            List<Identidade> identidades, string? nome, ResultadoImportacao resultado)
        {
            if (telefones.Count != 0)
            {
                foreach (var t in telefones)
                {
                    try
                    {
                        t.PessoaId = Id;
                        t.Pessoa = null;
                        db.Telefones.Add(t);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Destaca apenas o telefone problemático sem resetar o contexto inteiro
                        db.Entry(t).State = EntityState.Detached;
                        resultado.RegistrarErro(0, $"Banco [Telefone da Pessoa {nome}]: {ex.Message}");
                    }
                }
            }

            if (emails.Count != 0)
            {
                foreach (var e in emails)
                {
                    try
                    {
                        e.PessoaId = Id;
                        e.Pessoa = null;
                        db.Emails.Add(e);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Destaca apenas o email problemático
                        db.Entry(e).State = EntityState.Detached;
                        resultado.RegistrarErro(0, $"Banco [Email da Pessoa {nome}]: {ex.Message}");
                    }
                }
            }

            if (identidades.Count != 0)
            {
                foreach (var i in identidades)
                {
                    try
                    {
                        i.PessoaId = Id;
                        i.Pessoa = null;
                        db.Identidades.Add(i);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        //Destaca apenas a identidade problemática
                        db.Entry(i).State = EntityState.Detached;
                        resultado.RegistrarErro(0, $"Banco [Identidade da Pessoa {nome}]: {ex.Message}");
                    }
                }
            }
        }

        private async Task SalvarContatosInstituicaoAsync(
            CienciaJovemDb db, int instituicaoId, List<Telefone> telefones,
            List<Email> emails, string? nomeInst, ResultadoImportacao resultado)
        {
            if (telefones.Count != 0)
            {
                foreach (var t in telefones)
                {
                    try
                    {
                        t.InstituicaoId = instituicaoId;
                        t.Instituicao = null;
                        db.Telefones.Add(t);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Remove apenas o telefone problemático da memória
                        db.Entry(t).State = EntityState.Detached;
                        resultado.RegistrarErro(0, $"Banco [Telefone da Instituição {nomeInst}]: {ex.Message}");
                    }
                }
            }

            if (emails.Count != 0)
            {
                foreach (var e in emails)
                {
                    try
                    {
                        e.InstituicaoId = instituicaoId;
                        e.Instituicao = null;
                        db.Emails.Add(e);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Remove apenas o email problemático da memória
                        db.Entry(e).State = EntityState.Detached;
                        resultado.RegistrarErro(0, $"Banco [Email da Instituição {nomeInst}]: {ex.Message}");
                    }
                }
            }
        }
        #endregion

        //Serve para ligar com a UI do WinForm
        //mostrando o contador de erro e acerto
        private void RegistrarErro(
            ResultadoImportacao resultado,
            int linha,
            string contexto,
            Exception ex)
        {
            string mensagem = $"{contexto}: {ex.Message}";

            resultado.RegistrarErro(linha, mensagem);

            Erro?.Invoke($"Linha {linha}: {mensagem}");
        }

        //Metodo para Importar as Feiras
        private void ImportarFeiras(ResultadoImportacao resultado)
        {
            //Pula cabeçalho
            var linhas = _feiras.RowsUsed().Skip(1).ToList();
            int total = linhas.Count;
            int processadas = 0;

            foreach (var row in linhas)
            {
                int numeroLinha = row.RowNumber();
                bool erroNaLinha = false;

                //Cria Modelos vazios para inserir
                //Mesmo que de problema em tudo
                Responsavel? responsavelSubmissao = null;
                Responsavel? pessoaContato = null;
                Instituicao? instituicaoSede = null;
                Instituicao? instituicaoOrganizadora = null;
                AuxInstituicaoResponsavel? aux = null;
                Feira? feira = null;

                #region Responsavel
                // ============================================================
                // 1. RESPONSÁVEL DA SUBMISSÃO
                // ============================================================

                //Pega o nome fazendo concatenação
                string nome = ObterValor(row, ColunasFeira.NomeCompleto);
                string sobrenome = ObterValor(row, ColunasFeira.Sobrenome);
                string? nomeCompleto = $"{nome} {sobrenome}".Trim();

                //Testa para ver se o nome existe
                //se não continua
                try
                {
                    ValidationHelper.VerificarNome(nomeCompleto);
                }
                catch (ArgumentException ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Responsável", ex);
                    erroNaLinha = true;
                    nomeCompleto = null;
                }

                //Se o nome existe, testa para ver se aquele responsavel ja existe
                //Pq não foi inserido no banco ainda, não pode procurar por id
                if (nomeCompleto != null)
                {
                    // Evita duplicados na memória buscando pelo nome completo
                    responsavelSubmissao = _responsaveis.FirstOrDefault(r =>
                        ValidationHelper.VerificarMesmoNome(r.Nome, nomeCompleto));
                }

                //Se não existe um responsavel, um novo é criado
                if (responsavelSubmissao == null)
                {
                    //idGenero
                    string? idGenero = ObterValor(row, ColunasFeira.IdentidadeGenero);
                    try
                    {
                        ValidationHelper.VerificarTexto(idGenero);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Identidade Gênero Responsável", ex);
                        erroNaLinha = true;
                        idGenero = null;
                        
                    }

                    //raça
                    string? raca = ObterValor(row, ColunasFeira.RacaPessoa);
                    try
                    {
                        ValidationHelper.VerificarTexto(raca);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Raça Responsável", ex);
                        erroNaLinha = true;
                        raca = null;
                        
                    }

                    //dataNascimento
                    DateOnly? dataNascimento = null;
                    try
                    {
                        dataNascimento = ValidationHelper.VerificarData(ObterValor(row, ColunasFeira.DataNascimento)); ;
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Data Nascimento Responsável", ex);
                        erroNaLinha = true;
                        
                    }

                    //Professor
                    string? professor = ObterValor(row, ColunasFeira.EhProfessor);
                    try
                    {
                        ValidationHelper.VerificarTexto(professor); ;
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Professor Responsável", ex);
                        erroNaLinha = true;
                        professor = null;
                        
                    }

                    //Nível Ensino
                    string? nivelEnsino = ObterValor(row, ColunasFeira.NivelEnsinoResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(nivelEnsino);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Nível Ensino Responsável", ex);
                        erroNaLinha = true;
                        nivelEnsino = null;
                        
                    }

                    //Participou Ciência Jovem
                    string? participouCienciaJovem = ObterValor(row, ColunasFeira.ParticipouCienciaJovem);
                    try
                    {
                        ValidationHelper.VerificarTexto(participouCienciaJovem);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participou Ciência Jovem Responsável", ex);
                        erroNaLinha = true;
                        participouCienciaJovem = null;
                        
                    }

                    //Experiência Feiras
                    string? experienciaFeiras = ObterValor(row, ColunasFeira.ExperienciaFeiras);
                    try
                    {
                        ValidationHelper.VerificarTexto(experienciaFeiras);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Experiência Feiras Responsável", ex);
                        erroNaLinha = true;
                        experienciaFeiras = null;
                        
                    }

                    //Recomendação
                    string? recomendacao = ObterValor(row, ColunasFeira.Recomendacao);
                    try
                    {
                        ValidationHelper.VerificarTexto(recomendacao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Recomendação Responsável", ex);
                        erroNaLinha = true;
                        recomendacao = null;
                        
                    }

                    responsavelSubmissao = new Responsavel(
                        nomeCompleto,
                        idGenero,
                        raca,
                        dataNascimento,
                        professor,
                        nivelEnsino,
                        participouCienciaJovem,
                        experienciaFeiras,
                        recomendacao
                    );
                    _responsaveis.Add(responsavelSubmissao);
                }

                // Alimenta os contatos na RAM (as listas são limpas no SalvarNoBancoAsync)

                //email
                string emailResp = ObterValor(row, ColunasFeira.EnderecoEmailResponsavel);
                try
                {
                    ValidationHelper.VerificarEmail(emailResp);

                    if (!responsavelSubmissao.Email.Any(e => e.Endereco == emailResp))
                        responsavelSubmissao.Email.Add(new Email(emailResp));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Responsável", ex);
                    erroNaLinha = true;
                    
                }

                //telefone
                string telResp = ObterValor(row, ColunasFeira.TelefoneCelular);
                try
                {
                    telResp = ValidationHelper.VerificarTelefone(telResp);

                    if (!responsavelSubmissao.Telefone.Any(e => e.Numero == telResp))
                        responsavelSubmissao.Telefone.Add(new Telefone(telResp));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Telefone Responsável", ex);
                    erroNaLinha = true;
                    
                }

                //Identidade
                string docResp = ObterValor(row, ColunasFeira.DocumentoIdentificacao);
                try
                {
                    docResp = ValidationHelper.VerificarIdentidade(docResp);

                    if (!responsavelSubmissao.Identidade.Any(i => i.Numero == docResp))
                        responsavelSubmissao.Identidade.Add(new Identidade(docResp));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Identidade Responsável", ex);
                    erroNaLinha = true;
                    
                }

                #endregion

                #region Instituicao
                // ============================================================
                // 2. INSTITUIÇÃO SEDE (DA SUBMISSÃO)
                // ============================================================

                string? nomeInst = ObterValor(row, ColunasFeira.NomeInstituicao);

                try
                {
                    ValidationHelper.VerificarNome(nomeInst);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Instituição", ex);
                    erroNaLinha = true;
                    nomeInst = null;
                    
                }

                //Procura para saber se ja existe uma instituicao de mesmo nome
                //Pelo mesmo motivo de responsavel
                if (nomeInst != null)
                {
                    instituicaoSede = _instituicoes.FirstOrDefault(i =>
                        ValidationHelper.VerificarMesmoNome(i.Nome, nomeInst));
                }

                //Criacao de instituicao
                if (instituicaoSede == null)
                {
                    // CNPJ
                    string? cnpj = ObterValor(row, ColunasFeira.Cnpj);
                    try
                    {
                        cnpj = ValidationHelper.VerificarCNPJ(cnpj);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "CNPJ Instituição", ex);
                        erroNaLinha = true;
                        cnpj = null;
                        
                    }

                    // País
                    string? pais = ObterValor(row, ColunasFeira.Pais);
                    try
                    {
                        ValidationHelper.VerificarPais(pais);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "País Instituição", ex);
                        erroNaLinha = true;
                        pais = null;
                        
                    }

                    // Estado
                    string? estado = ObterValor(row, ColunasFeira.EstadoInstituicao);
                    try
                    {
                        if (pais != null && (pais.ToUpper() == "BRAZIL" || pais.ToUpper() == "BRASIL" || pais.ToUpper() == "BR"))
                        {
                            ValidationHelper.VerificarEstadoBR(estado);
                        }
                        else
                        {
                            ValidationHelper.VerificarTexto(estado);
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Estado Instituição", ex);
                        erroNaLinha = true;
                        estado = null;
                        
                    }

                    // Município
                    string? municipio = ObterValor(row, ColunasFeira.Municipio);
                    try
                    {
                        ValidationHelper.VerificarTexto(municipio);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Município Instituição", ex);
                        erroNaLinha = true;
                        municipio = null;
                        
                    }

                    // Endereço
                    string? endereco = ObterValor(row, ColunasFeira.EnderecoInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(endereco);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Endereço Instituição", ex);
                        erroNaLinha = true;
                        endereco = null;
                        
                    }

                    // Tipo de Rede
                    string? tipoRede = ObterValor(row, ColunasFeira.TipoRede);
                    try
                    {
                        ValidationHelper.VerificarTexto(tipoRede);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Tipo Rede Instituição", ex);
                        erroNaLinha = true;
                        tipoRede = null;
                        
                    }

                    // GRE
                    string? gre = ObterValor(row, ColunasFeira.Gre);
                    try
                    {
                        ValidationHelper.VerificarTexto(gre);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "GRE Instituição", ex);
                        erroNaLinha = true;
                        gre = null;
                        
                    }

                    // IDEB
                    double ideb = 0;
                    try
                    {
                        ideb = ValidationHelper.VerificarNumeroDouble(ObterValor(row, ColunasFeira.Ideb));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "IDEB Instituição", ex);
                        erroNaLinha = true;
                        
                    }

                    // IDHM
                    double idhm = 0;
                    try
                    {
                        idhm = ValidationHelper.VerificarNumeroDouble(ObterValor(row, ColunasFeira.Idhm));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "IDHM Instituição", ex);
                        erroNaLinha = true;
                        
                    }

                    // Participante
                    string? participante = ObterValor(row, ColunasFeira.ParticipacaoInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(participante);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participação Instituição", ex);
                        erroNaLinha = true;
                        participante = null;
                        
                    }

                    // Oferta Ensino
                    string? ofertaEnsino = ObterValor(row, ColunasFeira.OfertaEnsino);
                    try
                    {
                        ValidationHelper.VerificarTexto(ofertaEnsino);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Oferta Ensino Instituição", ex);
                        erroNaLinha = true;
                        ofertaEnsino = null;
                        
                    }

                    // Adere Tempo Integral
                    string? adere = ObterValor(row, ColunasFeira.AdereTempoIntegral);
                    try
                    {
                        ValidationHelper.VerificarTexto(adere);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Adesão Tempo Integral Instituição", ex);
                        erroNaLinha = true;
                        adere = null;
                        
                    }

                    // Tipologia Município
                    string? tipologiaMunicipio = ObterValor(row, ColunasFeira.TipologiaMunicipio);
                    try
                    {
                        ValidationHelper.VerificarTexto(tipologiaMunicipio);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Tipologia Município Instituição", ex);
                        erroNaLinha = true;
                        tipologiaMunicipio = null;
                        
                    }

                    // Apoio Financeiro
                    string? apoioFinanceiro = ObterValor(row, ColunasFeira.ApoioFinanceiro);
                    try
                    {
                        ValidationHelper.VerificarTexto(apoioFinanceiro);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Apoio Financeiro Instituição", ex);
                        erroNaLinha = true;
                        apoioFinanceiro = null;
                        
                    }

                    //Participaco CienciaJovem Instituicao
                    string? participacaoCJ = ObterValor(row, ColunasFeira.ParticipouCienciaJovemFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(participacaoCJ);
                        if (participacaoCJ.ToUpper() == "SIM")
                        {
                            string? detalhes = ObterValor(row, ColunasFeira.AnosParticipacaoInstituicao);
                            try
                            {
                                ValidationHelper.VerificarTexto(detalhes);
                            }
                            catch (Exception ex)
                            {
                                RegistrarErro(resultado, numeroLinha, "Participacao Ciencia Jovem Instituicao", ex);
                                erroNaLinha = true;
                                detalhes = null;
                                
                            }

                            int quantosProjetos = 0;
                            try
                            {
                                quantosProjetos = ValidationHelper.VerificarNumeroInt(ObterValor(row, ColunasFeira.QuantosProjetos));
                            }
                            catch (Exception ex)
                            {
                                RegistrarErro(resultado, numeroLinha, "Participacao Ciencia Jovem Instituicao", ex);
                                erroNaLinha = true;
                                
                            }

                            participacaoCJ = $"{participacaoCJ}. {detalhes}. Quantidade: {quantosProjetos}";
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participacao Ciencia Jovem Instituicao", ex);
                        erroNaLinha = true;
                        participacaoCJ = null;
                        
                    }

                    instituicaoSede = new Instituicao(
                        nomeInst,
                        cnpj,
                        pais,
                        estado,
                        municipio,
                        endereco,
                        tipoRede,
                        gre,
                        ideb,
                        idhm,
                        participante,
                        ofertaEnsino,
                        adere,
                        tipologiaMunicipio,
                        apoioFinanceiro,
                        participacaoCJ
                    );
                    _instituicoes.Add(instituicaoSede);
                }

                // Telefone Instituição
                string telInst = ObterValor(row, ColunasFeira.TelefoneInstituicao);
                try
                {
                    telInst = ValidationHelper.VerificarTelefone(telInst);

                    if (!instituicaoSede.Telefone.Any(e => e.Numero == telInst))
                        instituicaoSede.Telefone.Add(new Telefone(telInst));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Telefone Instituição", ex);
                    erroNaLinha = true;
                    
                }

                // Email Instituição
                string emailInst = ObterValor(row, ColunasFeira.EmailInstituicao);
                try
                {
                    ValidationHelper.VerificarEmail(emailInst);

                    if (!instituicaoSede.Email.Any(e => e.Endereco == emailInst))
                        instituicaoSede.Email.Add(new Email(emailInst));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Instituição", ex);
                    erroNaLinha = true;
                    
                }

                #endregion

                #region Auxiliar Responsavel Instituicao

                // ============================================================
                // 3. VÍNCULO AUXILIAR (RESPONSÁVEL <-> INSTITUIÇÃO)
                // ============================================================

                if (responsavelSubmissao != null && instituicaoSede != null)
                {
                    string funcao = ObterValor(row, ColunasFeira.FuncaoResponsavelInstituicao);

                    try
                    {
                        ValidationHelper.VerificarTexto(funcao);

                        if (!responsavelSubmissao.AuxInstituicaoResponsavel.Any(a => a.Instituicao == instituicaoSede))
                        {
                            aux = new AuxInstituicaoResponsavel(responsavelSubmissao, instituicaoSede, funcao);
                            responsavelSubmissao.AuxInstituicaoResponsavel.Add(aux);
                            instituicaoSede.AuxInstituicaoResponsavel.Add(aux);

                            _relacionamentosInstituicaoResponsavel.Add(aux);
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Vínculo Responsável/Instituição", ex);
                        erroNaLinha = true;
                        
                    }
                }

                #endregion

                #region Instituicao Organizadora
                // ============================================================
                // 4. INSTITUIÇÃO ORGANIZADORA DA FEIRA
                // ============================================================

                string? nomeOrganizadora = ObterValor(row, ColunasFeira.InstituicaoOrganizadora);

                try
                {
                    ValidationHelper.VerificarNome(nomeOrganizadora);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Instituição Organizadora", ex);
                    erroNaLinha = true;
                    nomeOrganizadora = null;
                    
                }

                if (nomeOrganizadora != null)
                {
                    instituicaoOrganizadora = _instituicoes.FirstOrDefault(i =>
                        ValidationHelper.VerificarMesmoNome(i.Nome, nomeOrganizadora));
                }


                if (instituicaoOrganizadora == null)
                {
                    instituicaoOrganizadora = new Instituicao(nomeOrganizadora);
                    _instituicoes.Add(instituicaoOrganizadora);
                }

                #endregion

                #region Pessoa Contato
                // ============================================================
                // 5. PESSOA DE CONTATO DA FEIRA
                // ============================================================

                string? nomeContato = ObterValor(row, ColunasFeira.PessoaContatoFeira);

                try
                {
                    ValidationHelper.VerificarNome(nomeContato);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Pessoa Contato Feira", ex);
                    erroNaLinha = true;
                    nomeContato = null;
                    
                }

                if (nomeContato != null)
                {
                    pessoaContato = _responsaveis.FirstOrDefault(r =>
                        ValidationHelper.VerificarMesmoNome(r.Nome, nomeContato));
                }

                if (pessoaContato == null)
                {
                    //Não é enviado raça, nem idGenero no responsavel de contato
                    //portanto é iniciado com null
                    pessoaContato = new Responsavel(nomeContato, null, null);
                    _responsaveis.Add(pessoaContato);
                }

                // Telefone Contato
                string telContato = ObterValor(row, ColunasFeira.TelefoneContatoFeira);
                try
                {
                    telContato = ValidationHelper.VerificarTelefone(telContato);

                    if (!pessoaContato.Telefone.Any(e => e.Numero == telContato))
                        pessoaContato.Telefone.Add(new Telefone(telContato));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Telefone Pessoa Contato Feira", ex);
                    erroNaLinha = true;
                    
                }

                // Email Contato
                string emailContato = ObterValor(row, ColunasFeira.EmailContatoFeira);
                try
                {
                    ValidationHelper.VerificarEmail(emailContato);

                    if (!pessoaContato.Email.Any(e => e.Endereco == emailContato))
                        pessoaContato.Email.Add(new Email(emailContato));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Pessoa Contato Feira", ex);
                    erroNaLinha = true;
                    
                }

                #endregion

                #region Feira
                // ============================================================
                // 6. INSTANCIAÇÃO DA FEIRA E VÍNCULOS DE NAVEGAÇÃO
                // ============================================================

                string? nomeFeira = ObterValor(row, ColunasFeira.NomeFeira);

                try
                {
                    ValidationHelper.VerificarTexto(nomeFeira);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome da Feira", ex);
                    erroNaLinha = true;
                    nomeFeira = null;
                    
                }

                if (nomeFeira != null)
                {
                    // Evita duplicados na memória buscando pelo nome completo
                    feira = _feirasImportadas.FirstOrDefault(r =>
                        ValidationHelper.VerificarMesmoNome(r.Nome, nomeFeira));
                }

                if (feira == null)
                {
                    //Alcance
                    string? alcance = ObterValor(row, ColunasFeira.AlcanceFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(alcance);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Alcance da Feira", ex);
                        erroNaLinha = true;
                        alcance = null;
                        
                    }

                    //Endereco
                    string? enderecoFeira = ObterValor(row, ColunasFeira.EnderecoFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(enderecoFeira);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Endereço da Feira", ex);
                        erroNaLinha = true;
                        enderecoFeira = null;
                        
                    }

                    //Estado
                    string? estadoFeira = ObterValor(row, ColunasFeira.EstadoFeira);
                    try
                    {
                        // Valida o estado apenas se a instituição sede for do Brasil
                        string? paisSede = instituicaoSede?.Pais;
                        if (paisSede != null && (paisSede.ToUpper() == "BRAZIL" || paisSede.ToUpper() == "BRASIL" || paisSede.ToUpper() == "BR"))
                        {
                            ValidationHelper.VerificarEstadoBR(estadoFeira);
                        }
                        else
                        {
                            ValidationHelper.VerificarTexto(estadoFeira);
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Estado da Feira", ex);
                        erroNaLinha = true;
                        estadoFeira = null;
                        
                    }

                    //Periodo Realizacao
                    string? periodoRealizacao = ObterValor(row, ColunasFeira.PeriodoRealizacaoFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(periodoRealizacao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Período de Realização da Feira", ex);
                        erroNaLinha = true;
                        periodoRealizacao = null;
                        
                    }

                    //Data realizacao
                    string? dataRealizacao = ObterValor(row, ColunasFeira.DataRealizacaoFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(dataRealizacao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Data de Realização da Feira", ex);
                        erroNaLinha = true;
                        dataRealizacao = null;
                        
                    }

                    //Modalidade
                    string? modalidade = ObterValor(row, ColunasFeira.ModalidadeParticipacaoFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(modalidade);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Modalidade da Feira", ex);
                        erroNaLinha = true;
                        modalidade = null;
                        
                    }

                    //Num projeto
                    int numProjetos = 0;
                    try
                    {
                        numProjetos = ValidationHelper.VerificarNumeroInt(ObterValor(row, ColunasFeira.NumeroProjetosParticipantes));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Número de Projetos Participantes", ex);
                        erroNaLinha = true;
                        
                    }

                    //areasConhecimento
                    string? areasConhecimento = ObterValor(row, ColunasFeira.AreasConhecimento);
                    try
                    {
                        ValidationHelper.VerificarTexto(areasConhecimento);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Áreas de Conhecimento da Feira", ex);
                        erroNaLinha = true;
                        areasConhecimento = null;
                        
                    }

                    //nivel ensino
                    string? nivelEnsinoAlunos = ObterValor(row, ColunasFeira.NivelEnsinoAlunos);
                    try
                    {
                        ValidationHelper.VerificarTexto(nivelEnsinoAlunos);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Nível de Ensino da Feira", ex);
                        erroNaLinha = true;
                        nivelEnsinoAlunos = null;
                        
                    }

                    //numero escolas
                    int numEscolas = 0;
                    try
                    {
                        numEscolas = ValidationHelper.VerificarNumeroInt(ObterValor(row, ColunasFeira.NumeroEscolasParticipantes));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Número de Escolas Participantes", ex);
                        erroNaLinha = true;
                        
                    }

                    //afiliacoes
                    string? afiliada = ObterValor(row, ColunasFeira.FeiraAfiliada);
                    try
                    {
                        ValidationHelper.VerificarTexto(afiliada);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Feira Afiliada", ex);
                        erroNaLinha = true;
                        afiliada = null;
                        
                    }

                    //processo de selecao
                    string? processoSelecao = ObterValor(row, ColunasFeira.ProcessoSelecao);
                    try
                    {
                        ValidationHelper.VerificarTexto(processoSelecao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Processo de Seleção da Feira", ex);
                        erroNaLinha = true;
                        processoSelecao = null;
                        
                    }

                    //periodo de elaboração
                    string? periodoElaboracao = ObterValor(row, ColunasFeira.PeriodoElaboracao);
                    try
                    {
                        ValidationHelper.VerificarTexto(periodoElaboracao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Período de Elaboração da Feira", ex);
                        erroNaLinha = true;
                        periodoElaboracao = null;
                        
                    }

                    //projetos avaliados
                    string? projetosAvaliados = ObterValor(row, ColunasFeira.ProjetosAvaliados);
                    try
                    {
                        ValidationHelper.VerificarTexto(projetosAvaliados);
                        if (projetosAvaliados.ToUpper() == "SIM")
                        {
                            string? detalhes = ObterValor(row, ColunasFeira.FormaAvaliacao);
                            try
                            {
                                ValidationHelper.VerificarTexto(detalhes);
                            }
                            catch (Exception ex)
                            {
                                RegistrarErro(resultado, numeroLinha, "Projetos Avaliados da Feira", ex);
                                erroNaLinha = true;
                                detalhes = null;
                                
                            }

                            projetosAvaliados = $"{projetosAvaliados}. {detalhes}";
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Projetos Avaliados da Feira", ex);
                        erroNaLinha = true;
                        projetosAvaliados = null;
                        
                    }

                    //quantos projetos
                    int quantosProjetos = 0;
                    try
                    {
                        quantosProjetos = ValidationHelper.VerificarNumeroInt(ObterValor(row, ColunasFeira.QuantosProjetos));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Quantidade Total de Projetos Apresentados", ex);
                        erroNaLinha = true;
                        
                    }

                    //Carimbo Data Hora
                    DateTime? carimboDataHora = null;
                    try
                    {
                        carimboDataHora = ValidationHelper.VerificarDataHora(ObterValor(row, ColunasFeira.CarimboDataHora));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Carimbo de Data/Hora da Feira", ex);
                        erroNaLinha = true;
                        
                    }

                    feira = new Feira(
                        nomeFeira,
                        alcance,
                        enderecoFeira,
                        estadoFeira,
                        periodoRealizacao,
                        dataRealizacao,
                        modalidade,
                        numProjetos,
                        areasConhecimento,
                        nivelEnsinoAlunos,
                        numEscolas,
                        afiliada,
                        processoSelecao,
                        periodoElaboracao,
                        projetosAvaliados,
                        carimboDataHora
                    );
                    _feirasImportadas.Add(feira);
                }

                // Vincula o Responsável da Submissão (Principal)
                if (responsavelSubmissao != null)
                {
                    feira.Responsavel = responsavelSubmissao;
                    responsavelSubmissao.Feiras.Add(feira); // Sincroniza a lista inversa
                }

                // Vincula a Instituição Sede
                if (instituicaoSede != null)
                {
                    feira.Instituicao = instituicaoSede;
                    instituicaoSede.Feiras.Add(feira); // Sincroniza a lista inversa
                }

                // Vincula a Instituição Organizadora
                if (instituicaoOrganizadora != null)
                {
                    feira.InstituicaoOrganizadora = instituicaoOrganizadora;
                    instituicaoOrganizadora.FeirasOrganizadas.Add(feira); // Sincroniza a lista inversa do InverseProperty
                }

                // Vincula a Pessoa de Contato da Feira
                if (pessoaContato != null)
                {
                    feira.ResponsavelContato = pessoaContato;
                    pessoaContato.FeirasContato.Add(feira); // Sincroniza a lista inversa do InverseProperty
                }
                #endregion

                if (!erroNaLinha)
                {
                    resultado.RegistrarSucesso();
                }

                processadas++;
                Progresso?.Invoke(processadas, total);
                ContadoresAtualizados?.Invoke(resultado.Sucessos, resultado.Erros);
            }
        }

        //Metodo para Importar os Pre-projetos
        private void ImportarPreProjetos(ResultadoImportacao resultado)
        {
            var linhas = _preProjetos.RowsUsed().Skip(1).ToList();
            int total = linhas.Count;
            int processadas = 0;

            foreach (var row in linhas)
            {
                int numeroLinha = row.RowNumber();
                bool erroNaLinha = false;

                Responsavel? responsavel = null;
                Instituicao? instituicao = null;
                AuxInstituicaoResponsavel? aux = null;
                Professor? professor = null;
                Pessoa? aluno = null;
                Projeto? projeto = null;

                #region Responsavel
                // ============================================================
                // 1. RESPONSÁVEL DO PROJETO
                // ============================================================
                string? nomeCompleto = ObterValor(row, ColunasProjetos.NomeCompletoResponsavel);

                try
                {
                    ValidationHelper.VerificarNome(nomeCompleto);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Responsável", ex);
                    erroNaLinha = true;
                    nomeCompleto = null;
                    
                }

                // Busca na RAM para evitar duplicados
                if (nomeCompleto != null)
                {
                    responsavel = _responsaveis.FirstOrDefault(r =>
                        ValidationHelper.VerificarMesmoNome(r.Nome, nomeCompleto));
                }

                if (responsavel == null)
                {
                    //idGenero
                    string? idGenero = ObterValor(row, ColunasProjetos.IdentidadeGeneroResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(idGenero);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Identidade Gênero Responsável", ex);
                        erroNaLinha = true;
                        idGenero = null;
                        
                    }

                    //raça
                    string? raca = ObterValor(row, ColunasProjetos.RacaResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(raca);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Raça Responsável", ex);
                        erroNaLinha = true;
                        raca = null;
                        
                    }

                    //dataNascimento
                    DateOnly? dataNascimento = null;
                    try
                    {
                        dataNascimento = ValidationHelper.VerificarData(ObterValor(row, ColunasProjetos.DataNascimentoResponsavel));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Data Nascimento Responsável", ex);
                        erroNaLinha = true;
                        
                    }

                    //Professor
                    string? professor2 = ObterValor(row, ColunasProjetos.ProfessorResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(professor2);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Professor Responsável", ex);
                        erroNaLinha = true;
                        professor2 = null;
                        
                    }

                    //Nível Ensino
                    string? nivelEnsino = ObterValor(row, ColunasProjetos.NivelEnsinoResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(nivelEnsino);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Nível Ensino Responsável", ex);
                        erroNaLinha = true;
                        nivelEnsino = null;
                        
                    }

                    //Participante
                    string? participante = ObterValor(row, ColunasProjetos.ParticipanteResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(participante);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participante Responsável", ex);
                        erroNaLinha = true;
                        participante = null;
                        
                    }

                    //Experiência Feiras
                    string? experienciaFeiras = ObterValor(row, ColunasProjetos.ExperienciaResponsavel);
                    try
                    {
                        ValidationHelper.VerificarTexto(experienciaFeiras);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Experiência Feiras Responsável", ex);
                        erroNaLinha = true;
                        experienciaFeiras = null;
                        
                    }

                    //Recomendação
                    string? recomendacao = ObterValor(row, ColunasProjetos.Recomendacao);
                    try
                    {
                        ValidationHelper.VerificarTexto(recomendacao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Recomendação Responsável", ex);
                        erroNaLinha = true;
                        recomendacao = null;
                        
                    }

                    responsavel = new Responsavel(
                        nomeCompleto,
                        idGenero,
                        raca,
                        dataNascimento,
                        professor2,
                        nivelEnsino,
                        participante,
                        experienciaFeiras,
                        recomendacao
                    );
                    _responsaveis.Add(responsavel);
                }

                //email
                string emailResp = ObterValor(row, ColunasProjetos.EmailResponsavel);
                try
                {
                    ValidationHelper.VerificarEmail(emailResp);

                    if (!responsavel.Email.Any(e => e.Endereco == emailResp))
                        responsavel.Email.Add(new Email(emailResp));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Responsável", ex);
                    erroNaLinha = true;
                    
                }

                //telefone
                string telResp = ObterValor(row, ColunasProjetos.TelefoneResponsavel);
                try
                {
                    telResp = ValidationHelper.VerificarTelefone(telResp);

                    if (!responsavel.Telefone.Any(t => t.Numero == telResp))
                        responsavel.Telefone.Add(new Telefone(telResp));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Telefone Responsável", ex);
                    erroNaLinha = true;
                    
                }

                //Identidade
                string docResp = ObterValor(row, ColunasProjetos.DocumentoIdentificacaoResponsavel);
                try
                {
                    docResp = ValidationHelper.VerificarIdentidade(docResp);

                    if (!responsavel.Identidade.Any(i => i.Numero == docResp))
                        responsavel.Identidade.Add(new Identidade(docResp));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Identidade Responsável", ex);
                    erroNaLinha = true;
                    
                }
                #endregion

                #region Instituicao
                // ============================================================
                // 2. INSTITUIÇÃO DO PROJETO
                // ============================================================
                string? nomeInst = ObterValor(row, ColunasProjetos.NomeInstituicao);

                try
                {
                    ValidationHelper.VerificarNome(nomeInst);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Instituição", ex);
                    erroNaLinha = true;
                    nomeInst = null;
                    
                }

                if (nomeInst != null)
                {
                    instituicao = _instituicoes.FirstOrDefault(i =>
                        ValidationHelper.VerificarMesmoNome(i.Nome, nomeInst));
                }

                if (instituicao == null)
                {
                    //CNPJ
                    string? cnpj = ObterValor(row, ColunasProjetos.CNPJInstituicao);
                    try
                    {
                        cnpj = ValidationHelper.VerificarCNPJ(cnpj);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "CNPJ Instituição", ex);
                        erroNaLinha = true;
                        cnpj = null;
                        
                    }

                    //País
                    string? pais = ObterValor(row, ColunasProjetos.PaisInstituicao);
                    try
                    {
                        ValidationHelper.VerificarPais(pais);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "País Instituição", ex);
                        erroNaLinha = true;
                        pais = null;
                        
                    }

                    //Estado
                    string? estado = ObterValor(row, ColunasProjetos.EstadoInstituicao);
                    try
                    {
                        if (pais != null && (pais.ToUpper() == "BRAZIL" || pais.ToUpper() == "BRASIL" || pais.ToUpper() == "BR"))
                        {
                            ValidationHelper.VerificarEstadoBR(estado);
                        }
                        else
                        {
                            ValidationHelper.VerificarTexto(estado);
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Estado Instituição", ex);
                        erroNaLinha = true;
                        estado = null;
                        
                    }

                    //Município
                    string? municipio = ObterValor(row, ColunasProjetos.MunicipioInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(municipio);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Município Instituição", ex);
                        erroNaLinha = true;
                        municipio = null;
                        
                    }

                    //Endereço
                    string? endereco = ObterValor(row, ColunasProjetos.EnderecoInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(endereco);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Endereço Instituição", ex);
                        erroNaLinha = true;
                        endereco = null;
                        
                    }

                    //Tipo de Rede
                    string? tipoRede = ObterValor(row, ColunasProjetos.TipoRedeInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(tipoRede);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Tipo Rede Instituição", ex);
                        erroNaLinha = true;
                        tipoRede = null;
                        
                    }

                    //GRE
                    string? gre = ObterValor(row, ColunasProjetos.GREInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(gre);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "GRE Instituição", ex);
                        erroNaLinha = true;
                        gre = null;
                        
                    }

                    //IDEB
                    double ideb = 0;
                    try
                    {
                        ideb = ValidationHelper.VerificarNumeroDouble(ObterValor(row, ColunasProjetos.IDEBInstituicao));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "IDEB Instituição", ex);
                        erroNaLinha = true;
                        
                    }

                    //IDHM
                    double idhm = 0;
                    try
                    {
                        idhm = ValidationHelper.VerificarNumeroDouble(ObterValor(row, ColunasProjetos.IDHMInstituicao));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "IDHM Instituição", ex);
                        erroNaLinha = true;
                        
                    }

                    //Participante
                    string? participante = ObterValor(row, ColunasProjetos.ParticipanteInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(participante);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participação Instituição", ex);
                        erroNaLinha = true;
                        participante = null;
                        
                    }

                    //Oferta Ensino
                    string? ofertaEnsino = ObterValor(row, ColunasProjetos.OfertaEnsinoInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(ofertaEnsino);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Oferta Ensino Instituição", ex);
                        erroNaLinha = true;
                        ofertaEnsino = null;
                        
                    }

                    //Adere Tempo Integral
                    string? adere = ObterValor(row, ColunasProjetos.AdereInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(adere);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Adesão Tempo Integral Instituição", ex);
                        erroNaLinha = true;
                        adere = null;
                        
                    }

                    //Tipologia Município
                    string? tipologiaMunicipio = ObterValor(row, ColunasProjetos.TipologiaMunicipioInstituicao);
                    try
                    {
                        ValidationHelper.VerificarTexto(tipologiaMunicipio);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Tipologia Município Instituição", ex);
                        erroNaLinha = true;
                        tipologiaMunicipio = null;
                        
                    }

                    // Apoio Financeiro
                    string? apoioFinanceiro = ObterValor(row, ColunasProjetos.ApoioFinanceiro);
                    try
                    {
                        ValidationHelper.VerificarTexto(apoioFinanceiro);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Apoio Financeiro Instituição", ex);
                        erroNaLinha = true;
                        apoioFinanceiro = null;
                        
                    }

                    //Participaco CienciaJovem Instituicao
                    string? participacaoCJ = ObterValor(row, ColunasProjetos.ParticipouCienciaJovemFeira);
                    try
                    {
                        ValidationHelper.VerificarTexto(participacaoCJ);
                        if (participacaoCJ.ToUpper() == "SIM")
                        {
                            string? detalhes = ObterValor(row, ColunasProjetos.AnosParticipacaoInstituicao);
                            try
                            {
                                ValidationHelper.VerificarTexto(detalhes);
                            }
                            catch (Exception ex)
                            {
                                RegistrarErro(resultado, numeroLinha, "Participacao Ciencia Jovem Instituicao", ex);
                                erroNaLinha = true;
                                detalhes = null;
                                
                            }

                            int quantosProjetos = 0;
                            try
                            {
                                quantosProjetos = ValidationHelper.VerificarNumeroInt(ObterValor(row, ColunasProjetos.QuantosProjetos));
                            }
                            catch (Exception ex)
                            {
                                RegistrarErro(resultado, numeroLinha, "Participacao Ciencia Jovem Instituicao", ex);
                                erroNaLinha = true;
                                
                            }

                            participacaoCJ = $"{participacaoCJ}. {detalhes}. Quantidade: {quantosProjetos}";
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participacao Ciencia Jovem Instituicao", ex);
                        erroNaLinha = true;
                        participacaoCJ = null;
                    }


                    instituicao = new Instituicao(
                        nomeInst,
                        cnpj,
                        pais,
                        estado,
                        municipio,
                        endereco,
                        tipoRede,
                        gre,
                        ideb,
                        idhm,
                        participante,
                        ofertaEnsino,
                        adere,
                        tipologiaMunicipio,
                        apoioFinanceiro,
                        participacaoCJ
                    );
                    _instituicoes.Add(instituicao);
                }

                //telefone
                string telInst = ObterValor(row, ColunasProjetos.TelefoneInstituicao);
                try
                {
                    telInst = ValidationHelper.VerificarTelefone(telInst);

                    if (!instituicao.Telefone.Any(t => t.Numero == telInst))
                        instituicao.Telefone.Add(new Telefone(telInst));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Telefone Instituição", ex);
                    erroNaLinha = true;
                }

                //email
                string emailInst = ObterValor(row, ColunasProjetos.EmailInstituicao);
                try
                {
                    ValidationHelper.VerificarEmail(emailInst);

                    if (!instituicao.Email.Any(e => e.Endereco == emailInst))
                        instituicao.Email.Add(new Email(emailInst));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Instituição", ex);
                    erroNaLinha = true;
                }

                #endregion

                #region Auxiliar Responsavel Instituicao
                // ============================================================
                // 3. VÍNCULO AUXILIAR (RESPONSÁVEL <-> INSTITUIÇÃO)
                // ============================================================
                if (responsavel != null && instituicao != null)
                {
                    try
                    {
                        string funcao = ObterValor(row, ColunasProjetos.FuncaoResponsavelInstituicao);

                        //Função
                        ValidationHelper.VerificarTexto(funcao);

                        // Evita duplicar o mesmo vínculo se o responsável já estiver associado a essa escola
                        if (!responsavel.AuxInstituicaoResponsavel.Any(a => a.Instituicao == instituicao))
                        {
                            aux = new AuxInstituicaoResponsavel(responsavel, instituicao, funcao);
                            responsavel.AuxInstituicaoResponsavel.Add(aux);
                            instituicao.AuxInstituicaoResponsavel.Add(aux);
                        }
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Vínculo Responsável/Instituição", ex);
                        erroNaLinha = true;
                    }
                }

                #endregion

                #region Professor
                // ============================================================
                // 4. PROFESSOR ORIENTADOR
                // ============================================================

                string? nomeProf = ObterValor(row, ColunasProjetos.NomeProfessor);

                try
                {
                    ValidationHelper.VerificarNome(nomeProf);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Professor", ex);
                    erroNaLinha = true;
                    nomeProf = null;
                    
                }

                // Busca na RAM para evitar duplicados na hierarquia TPT
                if (nomeProf != null)
                {
                    professor = _professores.FirstOrDefault(p =>
                        ValidationHelper.VerificarMesmoNome(p.Nome, nomeProf));
                }

                if (professor == null)
                {
                    //gênero
                    string? generoProf = ObterValor(row, ColunasProjetos.GeneroProfessor);
                    try
                    {
                        ValidationHelper.VerificarTexto(generoProf);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Gênero Professor", ex);
                        erroNaLinha = true;
                        generoProf = null;
                        
                    }

                    //raça
                    string? racaProf = ObterValor(row, ColunasProjetos.RacaProfessor);
                    try
                    {
                        ValidationHelper.VerificarTexto(racaProf);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Raça Professor", ex);
                        erroNaLinha = true;
                        racaProf = null;
                        
                    }

                    //matrícula
                    string? matriculaProf = ObterValor(row, ColunasProjetos.MatriculaProfessor);
                    try
                    {
                        ValidationHelper.VerificarTexto(matriculaProf);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Matrícula Professor", ex);
                        erroNaLinha = true;
                        matriculaProf = null;
                        
                    }

                    professor = new Professor(
                        nomeProf,
                        generoProf,
                        racaProf,
                        matriculaProf
                    );
                    _professores.Add(professor);
                }

                // Documentos e Contatos do Professor

                //RG / Identidade
                string rgProf = ObterValor(row, ColunasProjetos.IdentidadeProfessor);
                string orgaoProf = ObterValor(row, ColunasProjetos.OrgaoExpedidorProfessor);
                try
                {
                    rgProf = ValidationHelper.VerificarIdentidade(rgProf);
                    ValidationHelper.VerificarTexto(orgaoProf);

                    if (!professor.Identidade.Any(i => i.Numero == rgProf))
                        professor.Identidade.Add(new Identidade(rgProf, orgaoProf));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Identidade Professor", ex);
                    erroNaLinha = true;
                    
                }

                //CPF
                string cpfProf = ObterValor(row, ColunasProjetos.CPFProfessor);
                try
                {
                    cpfProf = ValidationHelper.VerificarIdentidade(cpfProf);

                    if (!professor.Identidade.Any(i => i.Numero == cpfProf))
                        professor.Identidade.Add(new Identidade(cpfProf));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "CPF Professor", ex);
                    erroNaLinha = true;
                    
                }

                //telefone
                string telProf = ObterValor(row, ColunasProjetos.TelefoneProfessor);
                try
                {
                    telProf = ValidationHelper.VerificarTelefone(telProf);

                    if (!professor.Telefone.Any(i => i.Numero == telProf))
                        professor.Telefone.Add(new Telefone(telProf));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Telefone Professor", ex);
                    erroNaLinha = true;
                    
                }

                //email
                string emailProf = ObterValor(row, ColunasProjetos.EmailProfessor);
                try
                {
                    ValidationHelper.VerificarEmail(emailProf);

                    if (!professor.Email.Any(i => i.Endereco == emailProf))
                        professor.Email.Add(new Email(emailProf));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Professor", ex);
                    erroNaLinha = true;
                    
                }
                #endregion

                #region Projetos
                // ============================================================
                // 5. PROJETOS
                // ============================================================

                string? nomeProjeto = ObterValor(row, ColunasProjetos.NomeProjeto);

                try
                {
                    ValidationHelper.VerificarNome(nomeProjeto);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Projeto", ex);
                    erroNaLinha = true;
                    nomeProjeto = null;
                    
                }

                if (nomeProjeto != null)
                {
                    projeto = _projetosImportados.FirstOrDefault(p =>
                        ValidationHelper.VerificarMesmoNome(p.Nome, nomeProjeto));
                }

                if (projeto == null)
                {
                    string? deficiencia = ObterValor(row, ColunasProjetos.Deficiencia);
                    try
                    {
                        ValidationHelper.VerificarTexto(deficiencia);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Deficiência Projeto", ex);
                        erroNaLinha = true;
                        deficiencia = null;
                        
                    }

                    string? participacao = ObterValor(row, ColunasProjetos.Participacao);
                    try
                    {
                        ValidationHelper.VerificarTexto(participacao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Participação Projeto", ex);
                        erroNaLinha = true;
                        participacao = null;
                        
                    }

                    string? categoriaInscricao = ObterValor(row, ColunasProjetos.CategoriaInscricao);
                    try
                    {
                        ValidationHelper.VerificarTexto(categoriaInscricao);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Categoria de Inscrição Projeto", ex);
                        erroNaLinha = true;
                        categoriaInscricao = null;
                        
                    }

                    DateTime? carimboDataHora = null;
                    try
                    {
                        carimboDataHora = ValidationHelper.VerificarDataHora(ObterValor(row, ColunasProjetos.CarimboDataHora));
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Carimbo de Data/Hora Projeto", ex);
                        erroNaLinha = true;
                        
                    }

                    string? palavrasChave = ObterValor(row, ColunasProjetos.PalavrasChave);
                    try
                    {
                        ValidationHelper.VerificarTexto(palavrasChave);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Palavras-Chave Projeto", ex);
                        erroNaLinha = true;
                        palavrasChave = null;
                        
                    }

                    string? ods = ObterValor(row, ColunasProjetos.ODS);
                    try
                    {
                        ValidationHelper.VerificarTexto(ods);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "ODS Projeto", ex);
                        erroNaLinha = true;
                        ods = null;
                        
                    }

                    string? tema = ObterValor(row, ColunasProjetos.Tema);
                    try
                    {
                        ValidationHelper.VerificarTexto(tema);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Tema Projeto", ex);
                        erroNaLinha = true;
                        tema = null;
                        
                    }

                    string? area = ObterValor(row, ColunasProjetos.Area);
                    try
                    {
                        ValidationHelper.VerificarTexto(area);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Área Projeto", ex);
                        erroNaLinha = true;
                        area = null;
                        
                    }

                    string? objetivo = ObterValor(row, ColunasProjetos.Objetivo);
                    try
                    {
                        ValidationHelper.VerificarTexto(objetivo);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Objetivo Projeto", ex);
                        erroNaLinha = true;
                        objetivo = null;
                        
                    }

                    string? resumo = ObterValor(row, ColunasProjetos.Resumo);
                    try
                    {
                        ValidationHelper.VerificarTexto(resumo);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Resumo Projeto", ex);
                        erroNaLinha = true;
                        resumo = null;
                        
                    }

                    projeto = new Projeto
                    (
                        nomeProjeto,
                        deficiencia,
                        participacao,
                        categoriaInscricao,
                        carimboDataHora,
                        palavrasChave,
                        ods,
                        tema,
                        area,
                        objetivo,
                        resumo
                    );
                    _projetosImportados.Add(projeto);
                }

                // Associa o Responsável (Se criado/recuperado com sucesso)
                if (responsavel != null)
                {
                    projeto.Responsavel = responsavel;
                    responsavel.Projetos.Add(projeto);
                }

                // Associa o Professor Orientador
                if (professor != null)
                {
                    projeto.Professor = professor;
                    professor.Projetos.Add(projeto);
                }
                #endregion

                #region Alunos
                // ============================================================
                // 6. ALUNOS
                // ============================================================

                // ALUNO 1
                string? nomeAluno = ObterValor(row, ColunasProjetos.NomeAluno1);

                try
                {
                    ValidationHelper.VerificarNome(nomeAluno);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Aluno 1", ex);
                    erroNaLinha = true;
                    nomeAluno = null;
                    
                }

                if (nomeAluno != null)
                {
                    aluno = _alunos.FirstOrDefault(p =>
                        ValidationHelper.VerificarMesmoNome(p.Nome, nomeAluno));
                }

                if (aluno == null)
                {
                    //idGenero
                    string? generoAluno = ObterValor(row, ColunasProjetos.GeneroAluno1);
                    try
                    {
                        ValidationHelper.VerificarTexto(generoAluno);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Gênero Aluno 1", ex);
                        erroNaLinha = true;
                        generoAluno = null;
                        
                    }

                    //raça
                    string? racaAluno = ObterValor(row, ColunasProjetos.RacaAluno1);
                    try
                    {
                        ValidationHelper.VerificarTexto(racaAluno);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Raça Aluno 1", ex);
                        erroNaLinha = true;
                        racaAluno = null;
                        
                    }
                    aluno = new Pessoa(nomeAluno, generoAluno, racaAluno);
                    _alunos.Add(aluno);
                }

                //RG / Identidade
                string rgAluno = ObterValor(row, ColunasProjetos.IdentidadeAluno1);
                string orgaoAluno = ObterValor(row, ColunasProjetos.OrgaoExpedidorAluno1);
                try
                {
                    rgAluno = ValidationHelper.VerificarIdentidade(rgAluno);
                    ValidationHelper.VerificarTexto(orgaoAluno);

                    if (!aluno.Identidade.Any(i => i.Numero == rgAluno))
                        aluno.Identidade.Add(new Identidade(rgAluno, orgaoAluno));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Identidade Aluno", ex);
                    erroNaLinha = true;
                    
                }

                //CPF
                string cpfAluno = ObterValor(row, ColunasProjetos.CPFAluno1);
                try
                {
                    cpfAluno = ValidationHelper.VerificarIdentidade(cpfAluno);

                    if (!aluno.Identidade.Any(i => i.Numero == cpfAluno))
                        aluno.Identidade.Add(new Identidade(cpfAluno));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "CPF Aluno 1", ex);
                    erroNaLinha = true;
                    
                }

                //email
                string emailAluno = ObterValor(row, ColunasProjetos.EmailAluno1);
                try
                {
                    ValidationHelper.VerificarEmail(emailAluno);

                    if (!aluno.Email.Any(i => i.Endereco == emailAluno))
                        aluno.Email.Add(new Email(emailAluno));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Aluno 1", ex);
                    erroNaLinha = true;
                    
                }

                //Associa a projeto
                if (projeto != null)
                    projeto.Alunos.Add(aluno);

                // ALUNO 2
                nomeAluno = ObterValor(row, ColunasProjetos.NomeAluno2);

                try
                {
                    ValidationHelper.VerificarNome(nomeAluno);
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Nome Aluno 2", ex);
                    erroNaLinha = true;
                    nomeAluno = null;
                    
                }

                if (nomeAluno != null)
                {
                    aluno = _alunos.FirstOrDefault(p =>
                        ValidationHelper.VerificarMesmoNome(p.Nome, nomeAluno));
                }

                if (aluno == null)
                {
                    //idGenero
                    string? generoAluno = ObterValor(row, ColunasProjetos.GeneroAluno2);
                    try
                    {
                        ValidationHelper.VerificarTexto(generoAluno);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Gênero Aluno 2", ex);
                        erroNaLinha = true;
                        generoAluno = null;
                        
                    }

                    //raça
                    string? racaAluno = ObterValor(row, ColunasProjetos.RacaAluno2);
                    try
                    {
                        ValidationHelper.VerificarTexto(racaAluno);
                    }
                    catch (Exception ex)
                    {
                        RegistrarErro(resultado, numeroLinha, "Raça Aluno 2", ex);
                        erroNaLinha = true;
                        racaAluno = null;
                        
                    }
                    aluno = new Pessoa(nomeAluno, generoAluno, racaAluno);
                    _alunos.Add(aluno);
                }
                //RG / Identidade
                rgAluno = ObterValor(row, ColunasProjetos.IdentidadeAluno2);
                orgaoAluno = ObterValor(row, ColunasProjetos.OrgaoExpedidorAluno2);
                try
                {
                    rgAluno = ValidationHelper.VerificarIdentidade(rgAluno);
                    ValidationHelper.VerificarTexto(orgaoAluno);

                    if (!aluno.Identidade.Any(i => i.Numero == rgAluno))
                        aluno.Identidade.Add(new Identidade(rgAluno, orgaoAluno));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Identidade Aluno 2", ex);
                    erroNaLinha = true;
                    
                }

                //CPF
                cpfAluno = ObterValor(row, ColunasProjetos.CPFAluno2);
                try
                {
                    cpfAluno = ValidationHelper.VerificarIdentidade(cpfAluno);

                    if (!aluno.Identidade.Any(i => i.Numero == cpfAluno))
                        aluno.Identidade.Add(new Identidade(cpfAluno));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "CPF Aluno 2", ex);
                    erroNaLinha = true;
                    
                }

                //email
                emailAluno = ObterValor(row, ColunasProjetos.EmailAluno2);
                try
                {
                    ValidationHelper.VerificarEmail(emailAluno);

                    if (!aluno.Email.Any(i => i.Endereco == emailAluno))
                        aluno.Email.Add(new Email(emailAluno));
                }
                catch (Exception ex)
                {
                    RegistrarErro(resultado, numeroLinha, "Email Aluno 2", ex);
                    erroNaLinha = true;
                    
                }

                //Associa a projeto
                if (projeto != null)
                    projeto.Alunos.Add(aluno);

                #endregion

                if (!erroNaLinha)
                {
                    resultado.RegistrarSucesso();
                }

                processadas++;
                Progresso?.Invoke(processadas, total);
                ContadoresAtualizados?.Invoke(resultado.Sucessos, resultado.Erros);
            }
        }

        #region DefinicaoColunas

        private static string ObterValor(IXLRow row, int coluna)
        {
            var celula = row.Cell(coluna);

            //Se a celula for do tipo data, força formatação BR
            if (celula.DataType == XLDataType.DateTime)
            {
                try
                {
                    DateTime dataHoraNativa = celula.GetDateTime();

                    //Caso 1: Transforma em dd/MM/yyyy HH:mm:ss
                    if (coluna == ColunasFeira.CarimboDataHora || coluna == ColunasProjetos.CarimboDataHora)
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

        private static class ColunasFeira
        {
            public const int CarimboDataHora = 1;
            public const int EnderecoEmailResponsavel = 2;

            // Responsável
            public const int NomeCompleto = 3;
            public const int Sobrenome = 4;
            public const int DataNascimento = 5;
            public const int DocumentoIdentificacao = 6;
            public const int EmailPessoa = 7;
            public const int TelefoneCelular = 8;
            public const int IdentidadeGenero = 9;
            public const int RacaPessoa = 10;

            public const int EhProfessor = 11;
            public const int NivelEnsinoResponsavel = 12;
            public const int ParticipouCienciaJovem = 13;
            public const int ExperienciaFeiras = 14;
            public const int Recomendacao = 91;

            // Instituição
            public const int Cnpj = 15;
            public const int NomeInstituicao = 16;
            public const int FuncaoResponsavelInstituicao = 17;
            public const int Pais = 18;
            public const int EstadoInstituicao = 19;
            public const int Municipio = 20;
            public const int EnderecoInstituicao = 21;
            public const int TipoRede = 22;
            public const int TelefoneInstituicao = 23;
            public const int EmailInstituicao = 24;
            public const int Gre = 25;
            public const int Ideb = 26;
            public const int Idhm = 27;
            public const int ParticipacaoInstituicao = 28;
            public const int OfertaEnsino = 29;
            public const int AdereTempoIntegral = 30;
            public const int TipologiaMunicipio = 31;
            public const int ApoioFinanceiro = 89;
            public const int ParticipouCienciaJovemFeira = 86;
            public const int AnosParticipacaoInstituicao = 87;
            public const int QuantosProjetos = 88;

            // Feira
            public const int NomeFeira = 66;
            public const int InstituicaoOrganizadora = 67;
            public const int AlcanceFeira = 68;
            public const int EnderecoFeira = 69;
            public const int EstadoFeira = 70;
            public const int PessoaContatoFeira = 71;
            public const int TelefoneContatoFeira = 72;
            public const int EmailContatoFeira = 73;
            public const int PeriodoRealizacaoFeira = 74;
            public const int DataRealizacaoFeira = 75;
            public const int ModalidadeParticipacaoFeira = 76;
            public const int NumeroProjetosParticipantes = 77;
            public const int AreasConhecimento = 78;
            public const int NivelEnsinoAlunos = 79;
            public const int NumeroEscolasParticipantes = 80;
            public const int FeiraAfiliada = 81;
            public const int ProcessoSelecao = 82;
            public const int PeriodoElaboracao = 83;
            public const int ProjetosAvaliados = 84;
            public const int FormaAvaliacao = 85;
        }

        // ============================================================
        // COLUNAS DA ABA DE Pré-Projetos
        // ============================================================
        private static class ColunasProjetos
        {
            // Responsável
            public const int NomeCompletoResponsavel = 2;
            public const int DataNascimentoResponsavel = 3;
            public const int DocumentoIdentificacaoResponsavel = 4;
            public const int EmailResponsavel = 5;
            public const int TelefoneResponsavel = 6;
            public const int IdentidadeGeneroResponsavel = 7;
            public const int RacaResponsavel = 8;
            public const int ProfessorResponsavel = 9;
            public const int NivelEnsinoResponsavel = 10;
            public const int ParticipanteResponsavel = 11;
            public const int ExperienciaResponsavel = 12;
            public const int Recomendacao = 90;

            // Instituicao
            public const int CNPJInstituicao = 13;
            public const int NomeInstituicao = 14;
            public const int FuncaoResponsavelInstituicao = 15;
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
            public const int ParticipanteInstituicao = 26;
            public const int OfertaEnsinoInstituicao = 27;
            public const int AdereInstituicao = 28;
            public const int TipologiaMunicipioInstituicao = 29;
            public const int ApoioFinanceiro = 88;
            public const int ParticipouCienciaJovemFeira = 85;
            public const int AnosParticipacaoInstituicao = 86;
            public const int QuantosProjetos = 87;

            // Professor
            public const int NomeProfessor = 31;
            public const int MatriculaProfessor = 32;
            public const int IdentidadeProfessor = 33;
            public const int OrgaoExpedidorProfessor = 34;
            public const int CPFProfessor = 35;
            public const int RacaProfessor = 36;
            public const int GeneroProfessor = 37;
            public const int TelefoneProfessor = 38;
            public const int EmailProfessor = 39;

            // Aluno1
            public const int NomeAluno1 = 40;
            public const int IdentidadeAluno1 = 41;
            public const int OrgaoExpedidorAluno1 = 42;
            public const int CPFAluno1 = 43;
            public const int RacaAluno1 = 44;
            public const int GeneroAluno1 = 45;
            public const int EmailAluno1 = 46;

            // Aluno2
            public const int NomeAluno2 = 47;
            public const int IdentidadeAluno2 = 48;
            public const int OrgaoExpedidorAluno2 = 49;
            public const int CPFAluno2 = 50;
            public const int RacaAluno2 = 51;
            public const int GeneroAluno2 = 52;
            public const int EmailAluno2 = 53;

            // Projeto
            public const int Deficiencia = 54;
            public const int Participacao = 55;
            public const int CategoriaInscricao = 56;
            public const int CarimboDataHora = 57;
            public const int NomeProjeto = 58;
            public const int PalavrasChave = 59;
            public const int ODS = 60;
            public const int Tema = 61;
            public const int Area = 62;
            public const int Objetivo = 63;
            public const int Resumo = 64;
        }
        #endregion
    }
}