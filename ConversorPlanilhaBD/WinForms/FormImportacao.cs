using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConversorPlanilhaBD.Importacao;

namespace ConversorPlanilhaBD
{
    public partial class FormImportacao : Form
    {
        private readonly string _connectionString;
        public FormImportacao(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
        }

        private async void btnImportar_Click(
            object sender,
            EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                Title = "Selecione a planilha"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            //evitar cliques duplos
            btnImportar.Enabled = false;

            progressBarImportacao.Minimum = 0;
            progressBarImportacao.Maximum = 100;
            progressBarImportacao.Value = 0;

            lblProgresso.Text = "Iniciando";
            lblSucessos.Text = "Sucessos: 0";
            lblErros.Text = "Erros: 0";

            lstErros.Items.Clear();

            try
            {
                //Faz o peso da importação para uma thread separada
                var resultado = await Task.Run(() =>
                {
                    var importador =
                        new Importador(dialog.FileName, _connectionString);

                    // --------------------------------------------
                    // PROGRESSO
                    // --------------------------------------------

                    importador.Progresso +=
                        (processadas, total) =>
                        {
                            if (IsDisposed)
                                return;

                            BeginInvoke(() =>
                            {
                                if (total <= 0)
                                    return;

                                int porcentagem =
                                    processadas * 100 / total;

                                progressBarImportacao.Value =
                                    Math.Min(
                                        porcentagem,
                                        100
                                    );

                                lblProgresso.Text =
                                    $"{processadas}/{total} linhas";
                            });
                        };

                    // --------------------------------------------
                    // CONTADORES
                    // --------------------------------------------

                    importador.ContadoresAtualizados +=
                        (sucessos, erros) =>
                        {
                            if (IsDisposed)
                                return;

                            BeginInvoke(() =>
                            {
                                lblSucessos.Text =
                                    $"Sucessos: {sucessos}";

                                lblErros.Text =
                                    $"Erros: {erros}";
                            });
                        };

                    // --------------------------------------------
                    // ERROS
                    // --------------------------------------------

                    importador.Erro +=
                        mensagem =>
                        {
                            if (IsDisposed)
                                return;

                            BeginInvoke(() =>
                            {
                                lstErros.Items.Add(mensagem);

                                // Mostra sempre o erro mais recente
                                if (lstErros.Items.Count > 0)
                                {
                                    lstErros.TopIndex =
                                        lstErros.Items.Count - 1;
                                }
                            });
                        };

                    // --------------------------------------------
                    // IMPORTAÇÃO
                    // --------------------------------------------

                    return importador.Importar();
                });

                // --------------------------------------------
                // FINALIZAÇÃO
                // --------------------------------------------

                progressBarImportacao.Value = 100;

                lblProgresso.Text =
                    "Importação concluída";

                lblSucessos.Text =
                    $"Sucessos: {resultado.Sucessos}";

                lblErros.Text =
                    $"Erros: {resultado.Erros}";

                // Caso algum erro tenha sido registrado no
                // ResultadoImportacao mas não tenha chegado
                // pelo evento, garante que ele apareça na lista.
                if (lstErros.Items.Count == 0 &&
                    resultado.MensagensErro.Count > 0)
                {
                    foreach (var mensagem in resultado.MensagensErro)
                    {
                        lstErros.Items.Add(mensagem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro na importação",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                lblProgresso.Text =
                    "Importação interrompida";
            }
            finally
            {
                btnImportar.Enabled = true;
            }
        }
    }
}