using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin.UserControls
{
    public class MesaCard : UserControl
    {
        private Label lblNombre;
        private Label lblDetalle;
        private Button btnAgregar;
        private Button btnDetalles;
        private Button btnCerrar;
        private Button btnAbrir;
        private PictureBox picIcon;
        private int mesaId;

        public event EventHandler<int>? AgregarPedidoClicked;
        public event EventHandler<int>? DetallesClicked;
        public event EventHandler<int>? CerrarCuentaClicked;
        public event EventHandler<int>? AbrirCuentaClicked;

        public MesaCard()
        {
            // Reduce flicker and enable custom painting
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            this.Height = 120;
            this.Width = 760;
            this.Margin = new Padding(8);
            this.BackColor = Color.Transparent;

            picIcon = new PictureBox();
            picIcon.Size = new Size(64, 64);
            picIcon.Location = new Point(16, 26);
            picIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            try { picIcon.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "Imgs", "table.png")); } catch { }
            this.Controls.Add(picIcon);

            lblNombre = new Label();
            lblNombre.AutoSize = false;
            lblNombre.Location = new Point(96, 12);
            lblNombre.Size = new Size(520, 26);
            lblNombre.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblNombre.ForeColor = Color.FromArgb(30, 30, 30);
            this.Controls.Add(lblNombre);

            lblDetalle = new Label();
            lblDetalle.AutoSize = false;
            lblDetalle.Location = new Point(96, 40);
            lblDetalle.Size = new Size(560, 20);
            lblDetalle.Font = new Font("Segoe UI", 9);
            lblDetalle.ForeColor = Color.FromArgb(64, 64, 64);
            this.Controls.Add(lblDetalle);

            // Buttons styled as bars
            btnAgregar = new Button();
            btnAgregar.Text = "Agregar Pedido";
            btnAgregar.Size = new Size(360, 28);
            btnAgregar.Location = new Point(96, 68);
            btnAgregar.BackColor = Color.FromArgb(0, 150, 136);
            btnAgregar.ForeColor = Color.White;
            btnAgregar.FlatStyle = FlatStyle.Flat;
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Cursor = Cursors.Hand;
            btnAgregar.Click += (s, e) => AgregarPedidoClicked?.Invoke(this, mesaId);
            this.Controls.Add(btnAgregar);

            btnDetalles = new Button();
            btnDetalles.Text = "Detalles";
            btnDetalles.Size = new Size(120, 28);
            btnDetalles.Location = new Point(462, 68);
            btnDetalles.BackColor = Color.FromArgb(0, 121, 107);
            btnDetalles.ForeColor = Color.White;
            btnDetalles.FlatStyle = FlatStyle.Flat;
            btnDetalles.FlatAppearance.BorderSize = 0;
            btnDetalles.Cursor = Cursors.Hand;
            btnDetalles.Click += (s, e) => DetallesClicked?.Invoke(this, mesaId);
            this.Controls.Add(btnDetalles);

            btnCerrar = new Button();
            btnCerrar.Text = "Cerrar Cuenta";
            btnCerrar.Size = new Size(120, 28);
            btnCerrar.Location = new Point(592, 68);
            btnCerrar.BackColor = Color.FromArgb(20, 50, 60);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Click += (s, e) => CerrarCuentaClicked?.Invoke(this, mesaId);
            this.Controls.Add(btnCerrar);

            btnAbrir = new Button();
            btnAbrir.Text = "Abrir Cuenta";
            btnAbrir.Size = new Size(120, 28);
            btnAbrir.Location = new Point(592, 68);
            btnAbrir.BackColor = Color.FromArgb(0, 121, 107);
            btnAbrir.ForeColor = Color.White;
            btnAbrir.FlatStyle = FlatStyle.Flat;
            btnAbrir.FlatAppearance.BorderSize = 0;
            btnAbrir.Cursor = Cursors.Hand;
            btnAbrir.Click += (s, e) => AbrirCuentaClicked?.Invoke(this, mesaId);
            this.Controls.Add(btnAbrir);
        }

        public void SetData(RestaurantKarin.Mesa mesa)
        {
            mesaId = mesa.Id;
            lblNombre.Text = mesa.Nombre;

            if (mesa.Activa)
            {
                lblDetalle.Text = $"Personas : {mesa.Personas}    Hora Llegada : {mesa.HoraLlegada.ToShortTimeString()}    Cuenta : {mesa.Cuenta}$    Propina : {mesa.PropinaPercent}%";
                btnAgregar.Visible = true;
                btnDetalles.Visible = true;
                btnCerrar.Visible = true;
                btnAbrir.Visible = false;
            }
            else
            {
                lblDetalle.Text = $"Capacidad : {mesa.Capacidad} Personas";
                btnAgregar.Visible = false;
                btnDetalles.Visible = false;
                btnCerrar.Visible = false;
                btnAbrir.Visible = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // draw rounded background
            var rect = this.ClientRectangle;
            rect.Inflate(-2, -2);
            using (var brush = new SolidBrush(Color.FromArgb(230, 245, 245, 245)))
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            using (var path = RoundedRect(rect, 10))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
