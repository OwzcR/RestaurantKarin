using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace RestaurantKarin
{
    public partial class FormReportes : UserControl
    {
        // ── Paleta de colores ──────────────────────────────────────────────
        private static readonly Color CAzul = Color.FromArgb(30, 80, 120);
        private static readonly Color CAzulMedio = Color.FromArgb(45, 105, 150);
        private static readonly Color CNaranja = Color.FromArgb(240, 140, 55);
        private static readonly Color CTexto = Color.FromArgb(20, 45, 75);
        private static readonly Color CFondo = Color.FromArgb(200, 225, 240);
        private static readonly Color CBlancoCard = Color.White;
        private static readonly Color CHeaderCard = Color.FromArgb(28, 78, 118);

        // Datos del gráfico
        private readonly string[] _dias = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
        private double[] _ventasDiarias = new double[7];

        // ── Selección de semana ────────────────────────────────────────────
        private DateTime _semanaInicio;
        private DateTime _semanaFin;
        private Label    _lblSemana = null!;

        // ── Controles de reportes (referencias para actualización) ─────────
        private Label _lblTotalVentas;
        private Label _lblCantidadOrdenes;
        private Panel _pnlChart;
        private Panel _cardProd;
        private Panel _cardInv;
        private Panel _cardEmp;
        private Label _lblInventarioConsumido;

        // ── DataTables para exportación ────────────────────────────────────
        private DataTable? _dtVentas;
        private DataTable? _dtProductos;
        private DataTable? _dtEmpleados;
        private DataTable? _dtInventario;

        public FormReportes()
        {
            InitializeComponent();
            DoubleBuffered = true;
            _semanaInicio  = LunesDeEstaSemana(DateTime.Today);
            _semanaFin     = _semanaInicio.AddDays(6);
            BuildUI();
            HandleCreated += (_, _) => CargarReportes();
        }

        private static void SetDoubleBuffered(Control c) =>
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, c, new object[] { true });

        private static DateTime LunesDeEstaSemana(DateTime fecha)
        {
            int diff = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return fecha.AddDays(-diff).Date;
        }

        private void ActualizarLblSemana()
        {
            if (_lblSemana != null)
                _lblSemana.Text = $"{_semanaInicio:dd MMM} — {_semanaFin:dd MMM yyyy}";
        }

        private void BuildUI()
        {
            this.SuspendLayout();

            // ── BackColor Transparent — hereda del contenedor padre (FormBase) ──
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(16);

            // ── Panel scroll que ocupa todo el UserControl ─────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            SetDoubleBuffered(scroll);
            this.Controls.Add(scroll);

            // ── Panel envolvente (Fill) — sirve para centrar ───────────────
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            SetDoubleBuffered(wrapper);
            scroll.Controls.Add(wrapper);

            // ── Panel de contenido con ancho fijo ─────────────────────────
            const int W = 1080;   // ancho total del contenido

            var inner = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Width = W
            };

            // Recentrar cada vez que cambie el tamaño de la ventana
            wrapper.Resize += (s, e) =>
            {
                inner.Location = new Point(Math.Max(0, (wrapper.Width - W) / 2), 12);
            };
            wrapper.Controls.Add(inner);

            // Centrado inicial (antes del primer Resize)
            inner.Location = new Point(Math.Max(0, (wrapper.Width - W) / 2), 12);


            // 1. BARRA DE FILTROS
            var pnlFiltros = new Panel
            {
                Size = new Size(W, 52),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(pnlFiltros);

            var lblPeriodo = new Label
            {
                Text = "PERÍODO DE TIEMPO",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = CAzul,
                AutoSize = false,
                Size = new Size(158, 32),
                Location = new Point(0, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            RoundControl(lblPeriodo, 16);
            pnlFiltros.Controls.Add(lblPeriodo);

            var btnPrev = MakeButton("◄", CAzul, 168, 10, 32, 32);
            btnPrev.Click += (_, _) =>
            {
                _semanaInicio = _semanaInicio.AddDays(-7);
                _semanaFin    = _semanaFin.AddDays(-7);
                ActualizarLblSemana();
                CargarReportes();
            };
            pnlFiltros.Controls.Add(btnPrev);

            _lblSemana = new Label
            {
                Location  = new Point(208, 10),
                Size      = new Size(240, 32),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = CTexto,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            RoundControl(_lblSemana, 6);
            ActualizarLblSemana();
            pnlFiltros.Controls.Add(_lblSemana);

            var btnNext = MakeButton("►", CAzul, 456, 10, 32, 32);
            btnNext.Click += (_, _) =>
            {
                _semanaInicio = _semanaInicio.AddDays(7);
                _semanaFin    = _semanaFin.AddDays(7);
                ActualizarLblSemana();
                CargarReportes();
            };
            pnlFiltros.Controls.Add(btnNext);

            var btnVerReporte = MakeButton("VER REPORTE", CAzul, 498, 10, 130, 32);
            btnVerReporte.Click += (_, _) =>
            {
                _semanaInicio = LunesDeEstaSemana(DateTime.Today);
                _semanaFin    = _semanaInicio.AddDays(6);
                ActualizarLblSemana();
                CargarReportes();
            };
            pnlFiltros.Controls.Add(btnVerReporte);

            // 2. FILA 1 — Ventas | Productos Más Vendidos
            var fila1 = new Panel
            {
                Size = new Size(W, 295),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(fila1);

            // Tarjeta Ventas
            var cardVentas = MakeCard(0, 0, 520, 295, "Ventas");
            fila1.Controls.Add(cardVentas);

            var pnlChart = new Panel
            {
                Location = new Point(10, 38),
                Size = new Size(500, 210),
                BackColor = Color.White
            };
            pnlChart.Paint += (s, e) => DrawBarChart(e.Graphics, pnlChart);
            _pnlChart = pnlChart;  // Guardar referencia
            cardVentas.Controls.Add(pnlChart);

            _lblTotalVentas = new Label
            {
                Text = "$0.00",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 255)
            };
            cardVentas.Controls.Add(_lblTotalVentas);

            _lblCantidadOrdenes = new Label
            {
                Text = "0 Órdenes",
                Font = new Font("Segoe UI", 10),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(100, 262)
            };
            cardVentas.Controls.Add(_lblCantidadOrdenes);

            // Tarjeta Productos Más Vendidos
            var cardProd = MakeCard(534, 0, 526, 295, "Productos Más Vendidos");
            _cardProd = cardProd;  // Guardar referencia
            fila1.Controls.Add(cardProd);

            string[] platillos = { "Platillo 1", "Platillo 2", "Platillo 3", "Platillo 4" };
            int[] cantidades = { 48, 25, 45, 10 };
            for (int i = 0; i < platillos.Length; i++)
                AddProductRow(cardProd, platillos[i], cantidades[i], 50, i);

            cardProd.Controls.Add(new Label
            {
                Text = "0 Ventas Totales",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 258),
                Name = "lblVentasTotales"
            });

            // ═══════════════════════════════════════════════════════════════
            // 3. FILA 2 — Consumo Inventario | Ingresos por Empleado
            // ═══════════════════════════════════════════════════════════════
            var fila2 = new Panel
            {
                Size = new Size(W, 295),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(fila2);

            // Tarjeta Consumo Inventario
            var cardInv = MakeCard(0, 0, 520, 295, "Consumo de Inventario");
            _cardInv = cardInv;  // Guardar referencia
            fila2.Controls.Add(cardInv);

            string[] insumos = { "Insumo 1", "Insumo 2", "Insumo 3", "Insumo 4" };
            int[] consumos = { 24, 18, 16, 10 };
            for (int i = 0; i < insumos.Length; i++)
                AddProductRow(cardInv, insumos[i], consumos[i], 28, i);

            _lblInventarioConsumido = new Label
            {
                Text = "0% del Inventario Consumido",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 258)
            };
            cardInv.Controls.Add(_lblInventarioConsumido);

            // Tarjeta Ingresos por Empleado
            var cardEmp = MakeCard(534, 0, 526, 295, "Ingresos por Empleado");
            _cardEmp = cardEmp;  // Guardar referencia
            fila2.Controls.Add(cardEmp);

            string[] nombres = { "Felipe", "José", "Carla", "Román" };
            string[] ingresos = { "$1,320.00", "$780.00", "$650.00", "$500.00" };
            int[] porcentajes = { 90, 53, 44, 34 };
            for (int i = 0; i < nombres.Length; i++)
                AddEmployeeRow(cardEmp, nombres[i], ingresos[i], porcentajes[i], i);

            var btnXLS = MakeButton("+ EXPORTAR EXCEL", CNaranja, 14, 255, 190, 32);
            cardEmp.Controls.Add(btnXLS);
            btnXLS.BringToFront();

            btnXLS.Click += (s, e) => ExportarExcel();

            this.ResumeLayout(false);
        }

        // ─────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────

        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = CBlancoCard
            };
            RoundControl(card, 10);

            var hdr = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(w, 30),
                BackColor = CHeaderCard
            };
            card.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            });

            return card;
        }

        private void AddProductRow(Panel card, string name, int value, int max, int index)
        {
            int yBase = 36 + index * 52;
            int barW = 300;

            var ico = new Panel { Location = new Point(12, yBase + 2), Size = new Size(22, 22), BackColor = CAzul };
            RoundControl(ico, 11);
            ico.Controls.Add(new Label { Text = "🍽", Font = new Font("Segoe UI Emoji", 8), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            card.Controls.Add(ico);

            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(40, yBase + 4) });

            var barBg = new Panel { Location = new Point(40, yBase + 26), Size = new Size(barW, 12), BackColor = CAzulMedio };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            int fill = Math.Max(1, (int)(barW * value / (double)max));
            var barFg = new Panel { Location = new Point(0, 0), Size = new Size(fill, 12), BackColor = CNaranja };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);

            card.Controls.Add(new Label { Text = value.ToString(), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = CTexto, AutoSize = true, Location = new Point(350, yBase + 24) });
        }

        private void AddInventoryRow(Panel card, string name, double consumido, string unidad, double maxConsumo, int index)
        {
            int yBase = 36 + index * 52;
            int barW  = 300;

            var ico = new Panel { Location = new Point(12, yBase + 2), Size = new Size(22, 22), BackColor = CAzul };
            RoundControl(ico, 11);
            ico.Controls.Add(new Label { Text = "🍽", Font = new Font("Segoe UI Emoji", 8), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            card.Controls.Add(ico);

            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(40, yBase + 4) });

            var barBg = new Panel { Location = new Point(40, yBase + 26), Size = new Size(barW, 12), BackColor = CAzulMedio };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            int fill = maxConsumo > 0 ? Math.Max(1, (int)(barW * consumido / maxConsumo)) : 1;
            var barFg = new Panel { Location = new Point(0, 0), Size = new Size(fill, 12), BackColor = CNaranja };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);

            string lbl = consumido >= 10000 ? $"{consumido / 1000:F2}k {unidad}"
                       : consumido >= 1      ? $"{consumido:F1} {unidad}"
                       :                       $"{consumido:F3} {unidad}";
            card.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = CTexto, AutoSize = true, Location = new Point(350, yBase + 24) });
        }

        private void AddEmployeeRow(Panel card, string name, string income, int pct, int index)
        {
            int yBase = 36 + index * 52;
            int barW = 280;

            var avatar = new Panel { Location = new Point(12, yBase + 2), Size = new Size(22, 22), BackColor = CAzulMedio };
            RoundControl(avatar, 11);
            avatar.Controls.Add(new Label { Text = "👤", Font = new Font("Segoe UI Emoji", 8), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            card.Controls.Add(avatar);

            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(40, yBase + 4) });
            card.Controls.Add(new Label { Text = income, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = CNaranja, AutoSize = true, Location = new Point(360, yBase + 4) });

            var barBg = new Panel { Location = new Point(40, yBase + 26), Size = new Size(barW, 12), BackColor = CAzulMedio };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            int fill = Math.Max(1, (int)(barW * pct / 100.0));
            var barFg = new Panel { Location = new Point(0, 0), Size = new Size(fill, 12), BackColor = CNaranja };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);
        }

        private Button MakeButton(string text, Color bg, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void DrawBarChart(Graphics g, Panel panel)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int padL = 42, padR = 10, padT = 10, padB = 30;
            int gW = panel.Width - padL - padR;
            int gH = panel.Height - padT - padB;
            int ox = padL, oy = panel.Height - padB;

            using var penGrid = new Pen(Color.FromArgb(210, 220, 230), 1f);
            using var fntAx = new Font("Segoe UI", 7);
            using var brTxt = new SolidBrush(CTexto);
            using var brBar = new SolidBrush(CNaranja);

            double maxVal = 100;
            foreach (var v in _ventasDiarias) if (v > maxVal) maxVal = v;
            maxVal = Math.Ceiling(maxVal / 100.0) * 100;
            int steps = 7;
            double stepVal = maxVal / steps;

            for (int i = 0; i <= steps; i++)
            {
                float y = oy - (float)(gH * i / steps);
                g.DrawLine(penGrid, ox, y, ox + gW, y);
                string lbl = $"${i * stepVal:0}";
                var sz = g.MeasureString(lbl, fntAx);
                g.DrawString(lbl, fntAx, brTxt, ox - sz.Width - 4, y - sz.Height / 2);
            }

            int n = _ventasDiarias.Length;
            float slot = gW / (float)n;
            float bw = slot * 0.55f;
            float bo = (slot - bw) / 2f;

            for (int i = 0; i < n; i++)
            {
                float barH = (float)(_ventasDiarias[i] / maxVal * gH);
                float bx = ox + i * slot + bo;
                float by = oy - barH;

                var path = new GraphicsPath();
                float r = 4;
                path.AddArc(bx, by, r * 2, r * 2, 180, 90);
                path.AddArc(bx + bw - r * 2, by, r * 2, r * 2, 270, 90);
                path.AddLine(bx + bw, oy, bx, oy);
                path.CloseFigure();
                g.FillPath(brBar, path);
                path.Dispose();

                var szL = g.MeasureString(_dias[i], fntAx);
                g.DrawString(_dias[i], fntAx, brTxt, bx + bw / 2 - szL.Width / 2, oy + 4);
            }
        }

        private static void RoundControl(Control ctrl, int radius)
        {
            int w = Math.Max(ctrl.Width, 1);
            int h = Math.Max(ctrl.Height, 1);
            int d = radius * 2;
            using var path = new GraphicsPath();
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseAllFigures();
            ctrl.Region = new Region(path);
        }

        public bool ValidarCampos() => _semanaInicio <= _semanaFin;

        /// <summary>
        /// Sanitiza una cadena de texto para prevenir inyección SQL.
        /// Elimina o escapa caracteres peligrosos.
        /// </summary>
        /// <param name="texto">Texto a sanitizar</param>
        /// <returns>Texto sanitizado y seguro para SQL</returns>
        public static string SanitizarSQL(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            // Reemplazar comillas simples con doble comilla (estándar SQL)
            texto = texto.Replace("'", "''");

            // Remover caracteres de control peligrosos
            texto = System.Text.RegularExpressions.Regex.Replace(
                texto,
                @"[\x00-\x1F\x7F]",  // Caracteres de control ASCII
                string.Empty
            );

            return texto.Trim();
        }

        /// <summary>
        /// Sanitiza un valor numérico, extrayendo solo dígitos.
        /// </summary>
        /// <param name="valor">Valor a sanitizar</param>
        /// <returns>Cadena con solo dígitos</returns>
        public static string SanitizarNumerico(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "0";

            return System.Text.RegularExpressions.Regex.Replace(valor, @"[^\d]", string.Empty);
        }

        /// <summary>
        /// Valida si un ID es válido (solo números, no vacío).
        /// </summary>
        /// <param name="id">ID a validar</param>
        /// <returns>true si el ID es válido</returns>
        public static bool EsIDValido(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(id, @"^\d+$");
        }

        /// <summary>
        /// Valida si una cadena es numérica.
        /// </summary>
        /// <param name="valor">Valor a validar</param>
        /// <returns>true si el valor es un número válido</returns>
        public static bool EsNumerico(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            return decimal.TryParse(valor, out _);
        }

        /// <summary>
        /// Carga los reportes desde la base de datos usando las fechas seleccionadas.
        /// </summary>
        private void CargarReportes()
        {
            try
            {
                if (!DatabaseHelper.ValidarConexion())
                {
                    MessageBox.Show("No se puede conectar a la base de datos.", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DateTime fechaInicio = _semanaInicio;
                DateTime fechaFin    = _semanaFin;

                DataTable dtVentas    = DatabaseHelper.ObtenerReporteVentas(fechaInicio, fechaFin);
                DataTable dtProductos = DatabaseHelper.ObtenerReporteProductosMasVendidos(fechaInicio, fechaFin);
                DataTable dtEmpleados = DatabaseHelper.ObtenerReporteIngresosPorEmpleado(fechaInicio, fechaFin);
                DataTable dtInventario = DatabaseHelper.ObtenerReporteConsumoInventario(fechaInicio, fechaFin);

                _dtVentas     = dtVentas;
                _dtProductos  = dtProductos;
                _dtEmpleados  = dtEmpleados;
                _dtInventario = dtInventario;

                this.SuspendLayout();
                ProcesarReporteVentas(dtVentas);
                ProcesarReporteProductos(dtProductos);
                ProcesarReporteEmpleados(dtEmpleados);
                ProcesarReporteInventario(dtInventario);
                this.ResumeLayout(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reportes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Procesa el reporte de ventas y actualiza la UI.
        /// </summary>
        private void ProcesarReporteVentas(DataTable dtVentas)
        {
            _ventasDiarias = new double[7];

            if (dtVentas == null || dtVentas.Rows.Count == 0)
            {
                _lblTotalVentas.Text     = "$0.00";
                _lblCantidadOrdenes.Text = "0 Órdenes";
                _pnlChart.Invalidate();
                return;
            }

            decimal totalVentas  = 0;
            int     totalOrdenes = 0;

            foreach (DataRow row in dtVentas.Rows)
            {
                if (row["VentasTotal"] != DBNull.Value)
                    totalVentas += Convert.ToDecimal(row["VentasTotal"]);

                if (row["CantidadOrdenes"] != DBNull.Value)
                    totalOrdenes += Convert.ToInt32(row["CantidadOrdenes"]);

                // Poblar ventas por día de la semana para el gráfico
                if (row["Fecha"] != DBNull.Value &&
                    DateTime.TryParse(row["Fecha"].ToString(), out DateTime fecha))
                {
                    int idx = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    if (idx >= 0 && idx < 7)
                        _ventasDiarias[idx] = row["VentasTotal"] != DBNull.Value
                            ? Convert.ToDouble(row["VentasTotal"]) : 0;
                }
            }

            _lblTotalVentas.Text     = $"${totalVentas:F2}";
            _lblCantidadOrdenes.Text = $"{totalOrdenes} Órdenes";
            _pnlChart.Invalidate();
        }

        /// <summary>
        /// Procesa el reporte de productos más vendidos y actualiza la UI.
        /// </summary>
        private void ProcesarReporteProductos(DataTable dtProductos)
        {
            if (dtProductos == null || dtProductos.Rows.Count == 0)
            {
                _cardProd.SuspendLayout();
                _cardProd.Controls.Clear();
                var hdrVacio = new Panel { Location = new Point(0, 0), Size = new Size(_cardProd.Width, 30), BackColor = CHeaderCard };
                hdrVacio.Controls.Add(new Label { Text = "Productos Más Vendidos", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), BackColor = Color.Transparent });
                _cardProd.Controls.Add(hdrVacio);
                _cardProd.Controls.Add(new Label { Text = "Sin datos para este período", Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(14, 48) });
                _cardProd.ResumeLayout(false);
                return;
            }

            _cardProd.SuspendLayout();
            _cardProd.Controls.Clear();

            var hdr = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(_cardProd.Width, 30),
                BackColor = CHeaderCard
            };
            _cardProd.Controls.Add(hdr);
            hdr.Controls.Add(new Label
            {
                Text = "Productos Más Vendidos",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            });

            int maxProductos = Math.Min(4, dtProductos.Rows.Count);
            for (int i = 0; i < maxProductos; i++)
            {
                DataRow row = dtProductos.Rows[i];
                string nombre = row["Producto"].ToString();
                int cantidad = Convert.ToInt32(row["CantidadVendida"]);
                AddProductRow(_cardProd, nombre, cantidad, maxProductos * 15, i);
            }

            int totalProductosVendidos = 0;
            foreach (DataRow row in dtProductos.Rows)
                totalProductosVendidos += Convert.ToInt32(row["CantidadVendida"]);

            _cardProd.Controls.Add(new Label
            {
                Text = $"{totalProductosVendidos} Ventas Totales",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, _cardProd.Height - 35)
            });

            _cardProd.ResumeLayout(false);
        }

        /// <summary>
        /// Procesa el reporte de ingresos por empleado y actualiza la UI.
        /// </summary>
        private void ProcesarReporteEmpleados(DataTable dtEmpleados)
        {
            if (dtEmpleados == null || dtEmpleados.Rows.Count == 0)
            {
                _cardEmp.SuspendLayout();
                _cardEmp.Controls.Clear();
                var hdrVacio = new Panel { Location = new Point(0, 0), Size = new Size(_cardEmp.Width, 30), BackColor = CHeaderCard };
                hdrVacio.Controls.Add(new Label { Text = "Ingresos por Empleado", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), BackColor = Color.Transparent });
                _cardEmp.Controls.Add(hdrVacio);
                _cardEmp.Controls.Add(new Label { Text = "Sin datos para este período", Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(14, 48) });
                var btnXlsVacio = MakeButton("+ EXPORTAR EXCEL", CNaranja, 14, 255, 190, 32);
                btnXlsVacio.Click += (s, e) => ExportarExcel();
                _cardEmp.Controls.Add(btnXlsVacio);
                btnXlsVacio.BringToFront();
                _cardEmp.ResumeLayout(false);
                return;
            }

            var botonesExportacion = new List<Button>();
            foreach (Control ctrl in _cardEmp.Controls)
            {
                if (ctrl is Button btn && (btn.Text.Contains("PDF") || btn.Text.Contains("EXCEL")))
                    botonesExportacion.Add(btn);
            }

            _cardEmp.SuspendLayout();
            _cardEmp.Controls.Clear();

            var hdr = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(_cardEmp.Width, 30),
                BackColor = CHeaderCard
            };
            _cardEmp.Controls.Add(hdr);
            hdr.Controls.Add(new Label
            {
                Text = "Ingresos por Empleado",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            });

            int maxEmpleados = Math.Min(4, dtEmpleados.Rows.Count);
            for (int i = 0; i < maxEmpleados; i++)
            {
                DataRow row = dtEmpleados.Rows[i];
                string nombre = row["Empleado"].ToString();
                decimal ingreso = Convert.ToDecimal(row["IngresoGenerado"]);
                int porcentaje = maxEmpleados > 0 ? (i + 1) * (100 / maxEmpleados) : 0;
                AddEmployeeRow(_cardEmp, nombre, $"${ingreso:F2}", porcentaje, i);
            }

            foreach (var btn in botonesExportacion)
            {
                _cardEmp.Controls.Add(btn);
                btn.BringToFront();
            }

            _cardEmp.ResumeLayout(false);
        }

        // ─────────────────────────────────────────────────────────────────
        // EXPORTACIÓN
        // ─────────────────────────────────────────────────────────────────

        private void ExportarExcel()
        {
            if (_dtVentas == null)
            {
                MessageBox.Show("Primero carga los datos antes de exportar.", "Sin datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title    = "Guardar reporte Excel",
                Filter   = "Archivo Excel|*.xlsx",
                FileName = $"Reporte_{_semanaInicio:yyyy-MM-dd}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var wb = new XLWorkbook();
                string periodo = $"Período: {_semanaInicio:dd/MM/yyyy} — {_semanaFin:dd/MM/yyyy}";

                void EstiloEncabezado(IXLCell cell)
                {
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 80, 120);
                    cell.Style.Font.FontColor = XLColor.White;
                }

                // ── Hoja Ventas ────────────────────────────────────────────────
                var wsV = wb.Worksheets.Add("Ventas");
                wsV.Cell(1, 1).Value = "REPORTE SEMANAL — VENTAS";
                wsV.Cell(1, 1).Style.Font.Bold = true; wsV.Cell(1, 1).Style.Font.FontSize = 14;
                wsV.Cell(2, 1).Value = periodo;
                int r = 4;
                foreach (var (h, c) in new[] { ("Fecha", 1), ("Ventas Total", 2), ("Cantidad Órdenes", 3) })
                { wsV.Cell(r, c).Value = h; EstiloEncabezado(wsV.Cell(r, c)); }
                r++;
                if (_dtVentas != null)
                    foreach (DataRow row in _dtVentas.Rows)
                    {
                        wsV.Cell(r, 1).Value = row["Fecha"]?.ToString() ?? "";
                        wsV.Cell(r, 2).Value = row["VentasTotal"] != DBNull.Value ? Convert.ToDouble(row["VentasTotal"]) : 0;
                        wsV.Cell(r, 3).Value = row["CantidadOrdenes"] != DBNull.Value ? Convert.ToInt32(row["CantidadOrdenes"]) : 0;
                        r++;
                    }
                wsV.Columns().AdjustToContents();

                // ── Hoja Productos ─────────────────────────────────────────────
                var wsP = wb.Worksheets.Add("Productos");
                wsP.Cell(1, 1).Value = "PRODUCTOS MÁS VENDIDOS";
                wsP.Cell(1, 1).Style.Font.Bold = true; wsP.Cell(1, 1).Style.Font.FontSize = 14;
                wsP.Cell(2, 1).Value = periodo;
                r = 4;
                foreach (var (h, c) in new[] { ("Producto", 1), ("Cantidad Vendida", 2) })
                { wsP.Cell(r, c).Value = h; EstiloEncabezado(wsP.Cell(r, c)); }
                r++;
                if (_dtProductos != null)
                    foreach (DataRow row in _dtProductos.Rows)
                    {
                        wsP.Cell(r, 1).Value = row["Producto"]?.ToString() ?? "";
                        wsP.Cell(r, 2).Value = row["CantidadVendida"] != DBNull.Value ? Convert.ToInt32(row["CantidadVendida"]) : 0;
                        r++;
                    }
                wsP.Columns().AdjustToContents();

                // ── Hoja Empleados ─────────────────────────────────────────────
                var wsE = wb.Worksheets.Add("Empleados");
                wsE.Cell(1, 1).Value = "INGRESOS POR EMPLEADO";
                wsE.Cell(1, 1).Style.Font.Bold = true; wsE.Cell(1, 1).Style.Font.FontSize = 14;
                wsE.Cell(2, 1).Value = periodo;
                r = 4;
                foreach (var (h, c) in new[] { ("Empleado", 1), ("Ingreso Generado", 2) })
                { wsE.Cell(r, c).Value = h; EstiloEncabezado(wsE.Cell(r, c)); }
                r++;
                if (_dtEmpleados != null)
                    foreach (DataRow row in _dtEmpleados.Rows)
                    {
                        wsE.Cell(r, 1).Value = row["Empleado"]?.ToString() ?? "";
                        wsE.Cell(r, 2).Value = row["IngresoGenerado"] != DBNull.Value ? Convert.ToDouble(row["IngresoGenerado"]) : 0;
                        r++;
                    }
                wsE.Columns().AdjustToContents();

                // ── Hoja Inventario ────────────────────────────────────────────
                var wsI = wb.Worksheets.Add("Inventario");
                wsI.Cell(1, 1).Value = "CONSUMO DE INVENTARIO";
                wsI.Cell(1, 1).Style.Font.Bold = true; wsI.Cell(1, 1).Style.Font.FontSize = 14;
                wsI.Cell(2, 1).Value = periodo;
                r = 4;
                foreach (var (h, c) in new[] { ("Insumo", 1), ("Stock Actual", 2), ("Costo Total", 3) })
                { wsI.Cell(r, c).Value = h; EstiloEncabezado(wsI.Cell(r, c)); }
                r++;
                if (_dtInventario != null)
                    foreach (DataRow row in _dtInventario.Rows)
                    {
                        wsI.Cell(r, 1).Value = row["Insumo"]?.ToString() ?? "";
                        wsI.Cell(r, 2).Value = row["StockActual"] != DBNull.Value ? Convert.ToDouble(row["StockActual"]) : 0;
                        wsI.Cell(r, 3).Value = row["CostoTotal"] != DBNull.Value ? Convert.ToDouble(row["CostoTotal"]) : 0;
                        r++;
                    }
                wsI.Columns().AdjustToContents();

                wb.SaveAs(sfd.FileName);

                MessageBox.Show("Excel exportado correctamente.", "Exportar Excel",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar Excel:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Procesa el reporte de consumo de inventario y actualiza la UI.
        /// </summary>
        private void ProcesarReporteInventario(DataTable dtInventario)
        {
            _cardInv.SuspendLayout();
            _cardInv.Controls.Clear();

            var hdr = new Panel { Location = new Point(0, 0), Size = new Size(_cardInv.Width, 30), BackColor = CHeaderCard };
            hdr.Controls.Add(new Label { Text = "Consumo de Inventario", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), BackColor = Color.Transparent });
            _cardInv.Controls.Add(hdr);

            if (dtInventario == null || dtInventario.Rows.Count == 0)
            {
                _cardInv.Controls.Add(new Label { Text = "Sin consumo registrado en este período", Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(14, 48) });
                _lblInventarioConsumido.Text = "0 insumos consumidos en el período";
                _cardInv.Controls.Add(_lblInventarioConsumido);
                _cardInv.ResumeLayout(false);
                return;
            }

            // Calcular máximo de consumo para proporcionar las barras
            double maxConsumo = 1.0;
            decimal costoTotal = 0;
            foreach (DataRow row in dtInventario.Rows)
            {
                double c = row["Consumido"] != DBNull.Value ? Convert.ToDouble(row["Consumido"]) : 0;
                if (c > maxConsumo) maxConsumo = c;
                costoTotal += row["CostoTotal"] != DBNull.Value ? Convert.ToDecimal(row["CostoTotal"]) : 0;
            }

            int maxInsumos = Math.Min(4, dtInventario.Rows.Count);
            for (int i = 0; i < maxInsumos; i++)
            {
                DataRow row    = dtInventario.Rows[i];
                string  nombre = row["Insumo"].ToString() ?? "";
                double  cons   = row["Consumido"] != DBNull.Value ? Convert.ToDouble(row["Consumido"]) : 0;
                string  unidad = row["Unidad"].ToString() ?? "";
                AddInventoryRow(_cardInv, nombre, cons, unidad, maxConsumo, i);
            }

            _lblInventarioConsumido.Text = $"{dtInventario.Rows.Count} insumos consumidos — costo estimado: ${costoTotal:F2}";
            _cardInv.Controls.Add(_lblInventarioConsumido);

            _cardInv.ResumeLayout(false);
        }
    }
}