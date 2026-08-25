namespace ConversorPlanilhaBD
{
    partial class FormImportacao
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.ProgressBar progressBarImportacao;
        private System.Windows.Forms.Label lblProgresso;
        private System.Windows.Forms.Label lblSucessos;
        private System.Windows.Forms.Label lblErros;
        private System.Windows.Forms.Label lblTituloErros;
        private System.Windows.Forms.ListBox lstErros;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnImportar = new Button();
            progressBarImportacao = new ProgressBar();
            lblProgresso = new Label();
            lblSucessos = new Label();
            lblErros = new Label();
            lblTituloErros = new Label();
            lstErros = new ListBox();
            SuspendLayout();
            // 
            // btnImportar
            // 
            btnImportar.Location = new Point(30, 30);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new Size(150, 40);
            btnImportar.TabIndex = 0;
            btnImportar.Text = "Importar";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += btnImportar_Click;
            // 
            // progressBarImportacao
            // 
            progressBarImportacao.Location = new Point(30, 95);
            progressBarImportacao.Name = "progressBarImportacao";
            progressBarImportacao.Size = new Size(740, 25);
            progressBarImportacao.TabIndex = 1;
            // 
            // lblProgresso
            // 
            lblProgresso.AutoSize = true;
            lblProgresso.Location = new Point(30, 135);
            lblProgresso.Name = "lblProgresso";
            lblProgresso.Size = new Size(56, 15);
            lblProgresso.TabIndex = 2;
            lblProgresso.Text = "Iniciando";
            // 
            // lblSucessos
            // 
            lblSucessos.AutoSize = true;
            lblSucessos.Location = new Point(30, 170);
            lblSucessos.Name = "lblSucessos";
            lblSucessos.Size = new Size(66, 15);
            lblSucessos.TabIndex = 3;
            lblSucessos.Text = "Sucessos: 0";
            // 
            // lblErros
            // 
            lblErros.AutoSize = true;
            lblErros.Location = new Point(180, 170);
            lblErros.Name = "lblErros";
            lblErros.Size = new Size(45, 15);
            lblErros.TabIndex = 4;
            lblErros.Text = "Erros: 0";
            // 
            // lblTituloErros
            // 
            lblTituloErros.AutoSize = true;
            lblTituloErros.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTituloErros.Location = new Point(30, 215);
            lblTituloErros.Name = "lblTituloErros";
            lblTituloErros.Size = new Size(110, 15);
            lblTituloErros.TabIndex = 5;
            lblTituloErros.Text = "Erros encontrados:";
            // 
            // lstErros
            // 
            lstErros.FormattingEnabled = true;
            lstErros.HorizontalScrollbar = true;
            lstErros.Location = new Point(30, 245);
            lstErros.Name = "lstErros";
            lstErros.Size = new Size(740, 274);
            lstErros.TabIndex = 6;
            // 
            // FormImportacao
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 550);
            Controls.Add(lstErros);
            Controls.Add(lblTituloErros);
            Controls.Add(lblErros);
            Controls.Add(lblSucessos);
            Controls.Add(lblProgresso);
            Controls.Add(progressBarImportacao);
            Controls.Add(btnImportar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormImportacao";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Importação de Dados";
            Load += FormImportacao_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
