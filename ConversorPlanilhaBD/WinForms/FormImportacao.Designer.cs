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
            this.btnImportar = new System.Windows.Forms.Button();
            this.progressBarImportacao = new System.Windows.Forms.ProgressBar();
            this.lblProgresso = new System.Windows.Forms.Label();
            this.lblSucessos = new System.Windows.Forms.Label();
            this.lblErros = new System.Windows.Forms.Label();
            this.lblTituloErros = new System.Windows.Forms.Label();
            this.lstErros = new System.Windows.Forms.ListBox();

            this.SuspendLayout();

            // 
            // btnImportar
            // 
            this.btnImportar.Location = new System.Drawing.Point(30, 30);
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.Size = new System.Drawing.Size(150, 40);
            this.btnImportar.TabIndex = 0;
            this.btnImportar.Text = "Importar";
            this.btnImportar.UseVisualStyleBackColor = true;
            this.btnImportar.Click += new System.EventHandler(this.btnImportar_Click);

            // 
            // progressBarImportacao
            // 
            this.progressBarImportacao.Location = new System.Drawing.Point(30, 95);
            this.progressBarImportacao.Name = "progressBarImportacao";
            this.progressBarImportacao.Size = new System.Drawing.Size(740, 25);
            this.progressBarImportacao.TabIndex = 1;

            // 
            // lblProgresso
            // 
            this.lblProgresso.AutoSize = true;
            this.lblProgresso.Location = new System.Drawing.Point(30, 135);
            this.lblProgresso.Name = "lblProgresso";
            this.lblProgresso.Size = new System.Drawing.Size(78, 15);
            this.lblProgresso.TabIndex = 2;
            this.lblProgresso.Text = "Iniciando...";

            // 
            // lblSucessos
            // 
            this.lblSucessos.AutoSize = true;
            this.lblSucessos.Location = new System.Drawing.Point(30, 170);
            this.lblSucessos.Name = "lblSucessos";
            this.lblSucessos.Size = new System.Drawing.Size(76, 15);
            this.lblSucessos.TabIndex = 3;
            this.lblSucessos.Text = "Sucessos: 0";

            // 
            // lblErros
            // 
            this.lblErros.AutoSize = true;
            this.lblErros.Location = new System.Drawing.Point(180, 170);
            this.lblErros.Name = "lblErros";
            this.lblErros.Size = new System.Drawing.Size(51, 15);
            this.lblErros.TabIndex = 4;
            this.lblErros.Text = "Erros: 0";

            // 
            // lblTituloErros
            // 
            this.lblTituloErros.AutoSize = true;
            this.lblTituloErros.Font = new System.Drawing.Font(
                "Segoe UI",
                9F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point
            );
            this.lblTituloErros.Location = new System.Drawing.Point(30, 215);
            this.lblTituloErros.Name = "lblTituloErros";
            this.lblTituloErros.Size = new System.Drawing.Size(104, 15);
            this.lblTituloErros.TabIndex = 5;
            this.lblTituloErros.Text = "Erros encontrados:";

            // 
            // lstErros
            // 
            this.lstErros.HorizontalScrollbar = true;
            this.lstErros.FormattingEnabled = true;
            this.lstErros.ItemHeight = 15;
            this.lstErros.Location = new System.Drawing.Point(30, 245);
            this.lstErros.Name = "lstErros";
            this.lstErros.Size = new System.Drawing.Size(740, 274);
            this.lstErros.TabIndex = 6;

            // 
            // FormImportacao
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 550);

            this.Controls.Add(this.lstErros);
            this.Controls.Add(this.lblTituloErros);
            this.Controls.Add(this.lblErros);
            this.Controls.Add(this.lblSucessos);
            this.Controls.Add(this.lblProgresso);
            this.Controls.Add(this.progressBarImportacao);
            this.Controls.Add(this.btnImportar);

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormImportacao";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Importação de Dados";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
