using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin.UserControls
{
    public class MesaCompact : UserControl
    {
        private Label lblTitle;
        private Label lblDetail;
        private Button btnAbrir;
        private int mesaId;

        public event EventHandler<int>? AbrirCuentaClicked;

        public MesaCompact()
        {
            this.Height = 48;
            this.Width = 700;
            this.Margin = new Padding(4);
            this.BackColor = Color.Transparent;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            lblTitle = new Label();
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(8, 12);
            lblTitle.Size = new Size(200, 20);
            lblTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(20, 20, 20);
            this.Controls.Add(lblTitle);

            lblDetail = new Label();
            lblDetail.AutoSize = false;
            lblDetail.Location = new Point(220, 12);
            lblDetail.Size = new Size(340, 20);
            lblDetail.Font = new Font("Segoe UI", 9);
            lblDetail.ForeColor = Color.FromArgb(60, 60, 60);
            this.Controls.Add(lblDetail);

            btnAbrir = new Button();
            btnAbrir.Text = "Abrir Cuenta";
            btnAbrir.Size = new Size(120, 28);
            btnAbrir.Location = new Point(580, 8);
            btnAbrir.BackColor = Color.FromArgb(13, 93, 128);
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
            lblTitle.Text = mesa.Nombre;
            lblDetail.Text = mesa.Activa ? $"Personas : {mesa.Personas}" : $"Capacidad : {mesa.Capacidad} Personas";
            btnAbrir.Visible = !mesa.Activa;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = this.ClientRectangle;
            rect.Inflate(-2, -2);
            using (var brush = new SolidBrush(Color.FromArgb(230, 245, 245, 245)))
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            using (var path = RoundedRect(rect, 8))
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
