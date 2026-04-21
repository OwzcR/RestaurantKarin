using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormInventario : Form
    {
        private Color colorVerdeBorde = Color.FromArgb(88, 160, 166);
        private Color colorTablaFondo = Color.FromArgb(220, 230, 235);
        private Color colorAzulBtn = Color.FromArgb(26, 90, 122);

        public FormInventario()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(29, 53, 87);
            this.Padding = new Padding(40);

            // 1. TARJETA BLANCA PRINCIPAL
            Panel cardPrincipal = new Panel();
            cardPrincipal.Dock = DockStyle.Fill;
            cardPrincipal.BackColor = Color.White;
            cardPrincipal.Padding = new Padding(20);
            this.Controls.Add(cardPrincipal);
            RedondearControl(cardPrincipal, 30);

            // 2. PANEL SUPERIOR (Buscador)
            Panel pnlSuperior = new Panel();
            pnlSuperior.Dock = DockStyle.Top;
            pnlSuperior.Height = 70;
            cardPrincipal.Controls.Add(pnlSuperior);

            Panel pnlBusqueda = new Panel();
            pnlBusqueda.Size = new Size(500, 40);
            pnlBusqueda.Location = new Point(10, 15);
            pnlBusqueda.BackColor = Color.FromArgb(240, 244, 248);
            pnlSuperior.Controls.Add(pnlBusqueda);
            RedondearControl(pnlBusqueda, 20);

            TextBox txtBusqueda = new TextBox();
            txtBusqueda.Text = " BUSCAR ID PRODUCTO :";
            txtBusqueda.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            txtBusqueda.ForeColor = Color.Gray;
            txtBusqueda.BorderStyle = BorderStyle.None;
            txtBusqueda.BackColor = pnlBusqueda.BackColor;
            txtBusqueda.Location = new Point(15, 10);
            txtBusqueda.Width = 320;
            pnlBusqueda.Controls.Add(txtBusqueda);

            Button btnSel = new Button();
            btnSel.Text = "SELECCIONAR";
            btnSel.Size = new Size(120, 30);
            btnSel.Location = new Point(360, 5);
            btnSel.BackColor = colorVerdeBorde;
            btnSel.ForeColor = Color.White;
            btnSel.FlatStyle = FlatStyle.Flat;
            btnSel.FlatAppearance.BorderSize = 0;
            btnSel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            pnlBusqueda.Controls.Add(btnSel);
            RedondearControl(btnSel, 12);

            // 3. PANEL INFERIOR (Contenedor de Botones)
            TableLayoutPanel pnlBotones = new TableLayoutPanel();
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Height = 100;
            pnlBotones.ColumnCount = 4;
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardPrincipal.Controls.Add(pnlBotones);

            pnlBotones.Controls.Add(CrearBotonAccion("➕ AGREGAR\nINSUMO", colorVerdeBorde), 0, 0);
            pnlBotones.Controls.Add(CrearBotonAccion("📝 EDITAR\nINSUMO", colorAzulBtn), 1, 0);
            pnlBotones.Controls.Add(CrearBotonAccion("🗃 ENTRADA\nINSUMOS", colorAzulBtn), 2, 0);
            pnlBotones.Controls.Add(CrearBotonAccion("🗑 ELIMINAR\nINSUMO", Color.FromArgb(239, 83, 80)), 3, 0);

            // 4. TABLA (ListView)
            ListView lista = new ListView();
            lista.Dock = DockStyle.Fill;
            lista.View = View.Details;
            lista.FullRowSelect = true;
            lista.BorderStyle = BorderStyle.None;
            lista.BackColor = colorTablaFondo;
            lista.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lista.OwnerDraw = true;

            lista.Columns.Add("Insumo", 250);
            lista.Columns.Add("Stock Actual", 120);
            lista.Columns.Add("Unidad", 100);
            lista.Columns.Add("Stock Mínimo", 120);
            lista.Columns.Add("Última Entrada", 170);
            lista.Columns.Add("Costo Unitario", 180);

            lista.DrawColumnHeader += (s, e) => {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                e.Graphics.DrawRectangle(new Pen(colorVerdeBorde, 3), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, lista.Font, e.Bounds, Color.FromArgb(26, 75, 80), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };
            lista.DrawSubItem += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(colorVerdeBorde, 1), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, lista.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };

            cardPrincipal.Controls.Add(lista);
            lista.BringToFront(); // Para que quede en medio de los paneles Dock
        }

        private Button CrearBotonAccion(string texto, Color color)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(10, 5, 10, 5); // Espacio entre botones
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.White;
            btn.FlatAppearance.BorderSize = 2;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return btn;
        }

        private void RedondearControl(Control c, int radio)
        {
            c.Region = new Region(new Rectangle(0, 0, c.Width, c.Height)); // Reset para que el Dock funcione bien
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, radio, radio, 180, 90);
            gp.AddArc(c.Width - radio, 0, radio, radio, 270, 90);
            gp.AddArc(c.Width - radio, c.Height - radio, radio, radio, 0, 90);
            gp.AddArc(0, c.Height - radio, radio, radio, 90, 90);
            c.Region = new Region(gp);
            c.Resize += (s, e) => RedondearControl(c, radio); // Recalcular al cambiar tamaño
        }
    }
}