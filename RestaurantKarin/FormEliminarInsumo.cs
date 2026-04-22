using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormEliminarInsumo : Form
    {
        private string idInsumo;
        private Color colorAzulBoton = Color.FromArgb(26, 90, 122);
        private Color colorGrisBoton = Color.FromArgb(200, 200, 200);

        public FormEliminarInsumo()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Size = new Size(500, 350);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            
            this.Padding = new Padding(2);

           
            Label lblIconoX = new Label
            {
                Text = "X",
                Font = new Font("Segoe UI", 50, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 90, 122),
                Size = new Size(80, 80),
                Location = new Point(210, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblIconoX.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawEllipse(new Pen(colorAzulBoton, 4), 5, 5, 70, 70);
            };

            Label lblTitulo = new Label
            {
                Text = "¿ELIMINAR INSUMO?",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = colorAzulBoton,
                Location = new Point(0, 110),
                Size = new Size(500, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Mensaje de Advertencia
            Label lblMensaje = new Label
            {
                Text = "Atención: El insumo seleccionado y toda su información asociada (unidad, costos, etc.) serán eliminados permanentemente.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(50, 160),
                Size = new Size(400, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblPregunta = new Label
            {
                Text = "¿Deseas continuar?",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(0, 230),
                Size = new Size(500, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Size = new Size(160, 45),
                Location = new Point(80, 280),
                BackColor = colorGrisBoton,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.Close();

            Button btnEliminar = new Button
            {
                Text = "SÍ, ELIMINAR",
                Size = new Size(160, 45),
                Location = new Point(260, 280),
                BackColor = colorAzulBoton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += (s, e) => {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.AddRange(new Control[] { lblIconoX, lblTitulo, lblMensaje, lblPregunta, btnCancelar, btnEliminar });
        }
    }
}