using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormReporte : UserControl
    {
        // ── Paleta ─────────────────────────────────────────────────────────
        private static readonly Color CAzul = Color.FromArgb(30, 80, 120);
        private static readonly Color CNaranja = Color.FromArgb(245, 140, 60);
        private static readonly Color CTexto = Color.FromArgb(30, 50, 80);
        private static readonly Color CFondo = Color.FromArgb(220, 235, 245);

        // Datos del gráfico de ventas
        private readonly string[] _diasVentas = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
        private readonly double[] _valoresVentas = { 400, 300, 350, 500, 450, 600, 550 };

        public FormReporte()
        {
            InitializeComponent();
            AplicarEsquinas();
            CargarBarrasProductos();
            CargarBarrasInventario();
            CargarFilasEmpleados();
            ConfigurarFondo();

            // El gráfico de ventas se dibuja en el evento Paint del panel
            pnlVentas.Paint += PnlVentas_Paint;
        }

        // ══════════════════════════════════════════════════════════════════
        //  GRÁFICO DE VENTAS  (dibujado a mano con GDI+)
        // ══════════════════════════════════════════════════════════════════
        private void PnlVentas_Paint(object sender, PaintEventArgs e)
        {
            // Área reservada para el gráfico (debajo del título, encima del total)
            int padL = 48, padR = 16, padT = 36, padB = 32;
            int gW = pnlVentas.Width - padL - padR;
            int gH = chartVentas.Height;   // usamos la altura del placeholder

            int originX = padL;
            int originY = padT + gH;        // esquina inferior izquierda del gráfico

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double maxVal = 700;
            int steps = 7;             // 0,100,200,...,700
            float stepPx = gH / (float)steps;

            // ── Líneas de cuadrícula y etiquetas Y ──────────────────────
            using var penGrid = new Pen(Color.FromArgb(30, 0, 0, 0), 1f);
            using var fntAxis = new Font("Segoe UI", 7.5f);
            using var brText = new SolidBrush(CTexto);

            for (int i = 0; i <= steps; i++)
            {
                float y = originY - i * stepPx;
                g.DrawLine(penGrid, originX, y, originX + gW, y);

                string lbl = $"${i * 100}";
                var sz = g.MeasureString(lbl, fntAxis);
                g.DrawString(lbl, fntAxis, brText, originX - sz.Width - 2, y - sz.Height / 2);
            }

            // ── Barras ───────────────────────────────────────────────────
            int n = _valoresVentas.Length;
            float slotW = gW / (float)n;
            float barW = slotW * 0.55f;
            float barOff = (slotW - barW) / 2f;

            using var brBar = new SolidBrush(CNaranja);

            for (int i = 0; i < n; i++)
            {
                float barH = (float)(_valoresVentas[i] / maxVal * gH);
                float x = originX + i * slotW + barOff;
                float y = originY - barH;

                // Barra con esquinas superiores redondeadas
                int rx = 4;
                var rect = new RectangleF(x, y, barW, barH);
                var path = new GraphicsPath();
                path.AddArc(x, y, rx * 2, rx * 2, 180, 90);
                path.AddArc(x + barW - rx * 2, y, rx * 2, rx * 2, 270, 90);
                path.AddLine(x + barW, y + rx, x + barW, y + barH);
                path.AddLine(x + barW, y + barH, x, y + barH);
                path.AddLine(x, y + barH, x, y + rx);
                path.CloseAllFigures();
                g.FillPath(brBar, path);

                // Etiqueta X
                var szL = g.MeasureString(_diasVentas[i], fntAxis);
                g.DrawString(_diasVentas[i], fntAxis, brText,
                             x + barW / 2 - szL.Width / 2,
                             originY + 3);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  ESQUINAS REDONDEADAS en las 4 tarjetas blancas
        // ══════════════════════════════════════════════════════════════════
        private void AplicarEsquinas()
        {
            foreach (var pnl in new Panel[] { pnlVentas, pnlProductos, pnlInventario, pnlIngresos })
            {
                pnl.Resize += (s, e) => Redondear((Control)s, 12);
                Redondear(pnl, 12);
            }
        }

        private static void Redondear(Control c, int r)
        {
            int w = Math.Max(c.Width, 1);
            int h = Math.Max(c.Height, 1);
            var path = new GraphicsPath();
            path.AddArc(0, 0, r * 2, r * 2, 180, 90);
            path.AddArc(w - r * 2, 0, r * 2, r * 2, 270, 90);
            path.AddArc(w - r * 2, h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(0, h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseAllFigures();
            c.Region = new Region(path);
        }

        // ══════════════════════════════════════════════════════════════════
        //  BARRAS HORIZONTALES – Productos más vendidos
        // ══════════════════════════════════════════════════════════════════
        private void CargarBarrasProductos()
        {
            flpProductos.Controls.Clear();
            var datos = new (string nombre, int valor, int max)[]
            {
                ("Platillo 1", 48, 60),
                ("Platillo 2", 25, 60),
                ("Platillo 3", 45, 60),
                ("Platillo 4", 10, 60),
            };
            foreach (var d in datos)
                flpProductos.Controls.Add(CrearFilaBarra(d.nombre, d.valor, d.max));

            flpProductos.Controls.Add(new Label
            {
                Text = "128 Ventas Totales",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = CTexto,
                Width = flpProductos.Width > 0 ? flpProductos.Width - 10 : 540,
                Height = 22,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 4, 0, 0)
            });
        }

        // ══════════════════════════════════════════════════════════════════
        //  BARRAS HORIZONTALES – Consumo de inventario
        // ══════════════════════════════════════════════════════════════════
        private void CargarBarrasInventario()
        {
            flpInventario.Controls.Clear();
            var datos = new (string nombre, int valor, int max)[]
            {
                ("Insumo 1", 24, 30),
                ("Insumo 2", 18, 30),
                ("Insumo 3", 16, 30),
                ("Insumo 4", 10, 30),
            };
            foreach (var d in datos)
                flpInventario.Controls.Add(CrearFilaBarra(d.nombre, d.valor, d.max));
        }

        // ══════════════════════════════════════════════════════════════════
        //  FILAS DE EMPLEADOS
        // ══════════════════════════════════════════════════════════════════
        private void CargarFilasEmpleados()
        {
            flpEmpleados.Controls.Clear();
            var datos = new (string nombre, string ingreso, int pct)[]
            {
                ("Felipe", "$1,320.00", 90),
                ("José",   "$780.00",   53),
                ("Carla",  "$650.00",   44),
                ("Roman",  "$500.00",   34),
            };
            foreach (var d in datos)
                flpEmpleados.Controls.Add(CrearFilaEmpleado(d.nombre, d.ingreso, d.pct));
        }

        // ══════════════════════════════════════════════════════════════════
        //  HELPER: fila de barra horizontal genérica
        // ══════════════════════════════════════════════════════════════════
        private Control CrearFilaBarra(string nombre, int valor, int maximo)
        {
            int ancho = 260;
            int filaAncho = flpProductos.Width > 0 ? flpProductos.Width - 10 : 540;

            var fila = new Panel
            {
                Width = filaAncho,
                Height = 42,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 2)
            };

            var icono = new Panel { Width = 24, Height = 24, Left = 4, Top = 9, BackColor = CNaranja };
            Redondear(icono, 12);

            var lblN = new Label
            {
                Text = nombre,
                Font = new Font("Segoe UI", 9f),
                ForeColor = CTexto,
                Left = 36,
                Top = 4,
                Width = 90,
                Height = 18,
                BackColor = Color.Transparent
            };

            var barFondo = new Panel { Left = 36, Top = 26, Width = ancho, Height = 10, BackColor = CAzul };
            Redondear(barFondo, 5);

            int ac = (int)(ancho * (double)valor / maximo);
            var barAc = new Panel { Left = 0, Top = 0, Width = Math.Max(ac, 4), Height = 10, BackColor = CNaranja };
            Redondear(barAc, 5);
            barFondo.Controls.Add(barAc);

            var lblV = new Label
            {
                Text = valor.ToString(),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = CTexto,
                Left = 36 + ancho + 8,
                Top = 22,
                Width = 40,
                Height = 18,
                BackColor = Color.Transparent
            };

            fila.Controls.AddRange(new Control[] { icono, lblN, barFondo, lblV });
            return fila;
        }

        private Control CrearFilaEmpleado(string nombre, string ingreso, int porcentaje)
        {
            int ancho = 220;
            int filaAncho = flpEmpleados.Width > 0 ? flpEmpleados.Width - 10 : 540;

            var fila = new Panel
            {
                Width = filaAncho,
                Height = 42,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 2)
            };

            var avatar = new Panel { Width = 24, Height = 24, Left = 4, Top = 9, BackColor = CAzul };
            Redondear(avatar, 12);

            var lblN = new Label
            {
                Text = nombre,
                Font = new Font("Segoe UI", 9f),
                ForeColor = CTexto,
                Left = 36,
                Top = 4,
                Width = 80,
                Height = 18,
                BackColor = Color.Transparent
            };

            var barFondo = new Panel { Left = 36, Top = 26, Width = ancho, Height = 10, BackColor = CAzul };
            Redondear(barFondo, 5);

            int ac = (int)(ancho * porcentaje / 100.0);
            var barAc = new Panel { Left = 0, Top = 0, Width = Math.Max(ac, 4), Height = 10, BackColor = CNaranja };
            Redondear(barAc, 5);
            barFondo.Controls.Add(barAc);

            var lblI = new Label
            {
                Text = ingreso,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = CTexto,
                Left = 36 + ancho + 8,
                Top = 10,
                Width = 100,
                Height = 18,
                BackColor = Color.Transparent
            };

            fila.Controls.AddRange(new Control[] { avatar, lblN, barFondo, lblI });
            return fila;
        }

        // ══════════════════════════════════════════════════════════════════
        //  FONDO OPCIONAL
        // ══════════════════════════════════════════════════════════════════
        private void ConfigurarFondo()
        {
            string ruta = Path.Combine(Application.StartupPath, "Imgs", "fondo.png");
            if (!File.Exists(ruta)) return;
            try
            {
                this.BackgroundImage = Image.FromFile(ruta);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════
        //  EVENTOS
        // ══════════════════════════════════════════════════════════════════
        private void BtnVerReporte_Click(object sender, EventArgs e)
            => MessageBox.Show(
                $"Reporte del {dtpFechaInicio.Value:yyyy-MM-dd} al {dtpFechaFin.Value:yyyy-MM-dd}",
                "Ver Reporte");

        private void BtnExportarPDF_Click(object sender, EventArgs e)
            => MessageBox.Show("Exportar a PDF – Por implementar");

        private void BtnExportarExcel_Click(object sender, EventArgs e)
            => MessageBox.Show("Exportar a Excel – Por implementar");

        private void pnlFila2_Paint(object sender, PaintEventArgs e) { }
        private void pnlPrincipal_Paint(object sender, PaintEventArgs e) { }
        private void pnlIngresos_Paint(object sender, PaintEventArgs e) { }
        private void pnlInventario_Paint(object sender, PaintEventArgs e) { }
        private void lblConsumoInventario_Click(object sender, EventArgs e) { }
        private void dtpFechaFin_ValueChanged(object sender, EventArgs e) { }

        private void lblPeriodo_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void pnlBotones_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flpProductos_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FormReporte_Load(object sender, EventArgs e)
        {

        }

        private void pnlFiltros_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FormReporte_Load_1(object sender, EventArgs e)
        {

        }

        private void pnlFila1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblVentas_Click(object sender, EventArgs e)
        {

        }
    }
}