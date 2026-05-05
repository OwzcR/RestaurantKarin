using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
        private readonly double[] _ventas = { 400, 500, 350, 450, 650, 600, 550 };

        // ── Controles de filtro (referencias globales para validación) ──────
        private DateTimePicker _dtpInicio;
        private DateTimePicker _dtpFin;
        private ErrorProvider _errorProvider;

        // ── Controles de reportes (referencias para actualización) ─────────
        private Label _lblTotalVentas;
        private Label _lblCantidadOrdenes;
        private Panel _pnlChart;
        private Panel _cardProd;
        private Panel _cardInv;
        private Panel _cardEmp;
        private Label _lblInventarioConsumido;

        public FormReportes()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _errorProvider = new ErrorProvider();
            BuildUI();
        }

        private void BuildUI()
        {
            // ── BackColor Transparent — hereda del contenedor padre (FormBase) ──
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(16);

            // ── Panel scroll que ocupa todo el UserControl ─────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent  // transparente para ver el fondo
            };
            this.Controls.Add(scroll);

            // ── Panel envolvente (Fill) — sirve para centrar ───────────────
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
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

            var dtpInicio = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 2, 7),
                Location = new Point(168, 10),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                CalendarForeColor = CTexto,
                CalendarMonthBackground = Color.White
            };
            _dtpInicio = dtpInicio;  // Guardar referencia global
            pnlFiltros.Controls.Add(dtpInicio);

            var dtpFin = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 2, 7),
                Location = new Point(293, 10),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _dtpFin = dtpFin;  // Guardar referencia global
            pnlFiltros.Controls.Add(dtpFin);

            var btnVerReporte = MakeButton("VER REPORTE", CAzul, 418, 10, 130, 32);
            btnVerReporte.Click += (s, e) =>
            {
                if (ValidarCampos())
                {
                    CargarReportes();
                }
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

            var btnPDF = MakeButton("+ EXPORTAR PDF", CAzul, 14, 255, 160, 32);
            var btnXLS = MakeButton("+ EXPORTAR EXCEL", CNaranja, 184, 255, 160, 32);
            cardEmp.Controls.Add(btnPDF);
            cardEmp.Controls.Add(btnXLS);
            btnPDF.BringToFront();
            btnXLS.BringToFront();

            btnPDF.Click += (s, e) => MessageBox.Show("Exportar a PDF", "Reportes");
            btnXLS.Click += (s, e) => MessageBox.Show("Exportar a Excel", "Reportes");
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

            double maxVal = 700;
            int steps = 7;

            for (int i = 0; i <= steps; i++)
            {
                float y = oy - (float)(gH * i / steps);
                g.DrawLine(penGrid, ox, y, ox + gW, y);
                string lbl = $"${i * 100}";
                var sz = g.MeasureString(lbl, fntAx);
                g.DrawString(lbl, fntAx, brTxt, ox - sz.Width - 4, y - sz.Height / 2);
            }

            int n = _ventas.Length;
            float slot = gW / (float)n;
            float bw = slot * 0.55f;
            float bo = (slot - bw) / 2f;

            for (int i = 0; i < n; i++)
            {
                float barH = (float)(_ventas[i] / maxVal * gH);
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
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseAllFigures();
            ctrl.Region = new Region(path);
        }

        /// <summary>
        /// Valida todos los campos de filtro del reporte.
        /// Verifica fechas válidas, rangos correctos y sanitiza datos.
        /// </summary>
        /// <returns>true si todos los campos son válidos; false si hay errores</returns>
        public bool ValidarCampos()
        {
            // Limpiar errores previos
            _errorProvider.Clear();

            // Lista de errores encontrados
            var errores = new List<string>();
            Control primerControlConError = null;

            // ─ VALIDACIÓN 1: Verificar que las fechas no estén vacías
            if (_dtpInicio.Value == null)
            {
                errores.Add("La fecha de inicio es requerida.");
                _errorProvider.SetError(_dtpInicio, "Fecha inicio requerida");
                if (primerControlConError == null) primerControlConError = _dtpInicio;
            }

            if (_dtpFin.Value == null)
            {
                errores.Add("La fecha de fin es requerida.");
                _errorProvider.SetError(_dtpFin, "Fecha fin requerida");
                if (primerControlConError == null) primerControlConError = _dtpFin;
            }

            // ─ VALIDACIÓN 2: Comparar rangos de fechas
            if (_dtpInicio.Value != null && _dtpFin.Value != null)
            {
                DateTime fechaInicio = _dtpInicio.Value.Date;
                DateTime fechaFin = _dtpFin.Value.Date;

                if (fechaInicio > fechaFin)
                {
                    errores.Add("La fecha de inicio no puede ser mayor a la fecha de fin.");
                    _errorProvider.SetError(_dtpInicio, "Debe ser ≤ fecha fin");
                    if (primerControlConError == null) primerControlConError = _dtpInicio;
                }
            }

            // ─ VALIDACIÓN 3: Mostrar errores acumulados
            if (errores.Count > 0)
            {
                // Poner el foco en el primer control con error
                if (primerControlConError != null)
                {
                    primerControlConError.Focus();
                }

                // Mostrar MessageBox con todos los errores
                string mensajeError = "❌ Se encontraron los siguientes errores:\n\n" +
                                     string.Join("\n• ", errores);
                MessageBox.Show(mensajeError, "Validación de Reporte", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            return true;
        }

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
                // Validar conexión a BD
                if (!DatabaseHelper.ValidarConexion())
                {
                    MessageBox.Show("❌ No se puede conectar a la base de datos.", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DateTime fechaInicio = _dtpInicio.Value.Date;
                DateTime fechaFin = _dtpFin.Value.Date;

                // Obtener datos de los reportes
                DataTable dtVentas = DatabaseHelper.ObtenerReporteVentas(fechaInicio, fechaFin);
                DataTable dtProductos = DatabaseHelper.ObtenerReporteProductosMasVendidos(fechaInicio, fechaFin);
                DataTable dtEmpleados = DatabaseHelper.ObtenerReporteIngresosPorEmpleado(fechaInicio, fechaFin);
                DataTable dtInventario = DatabaseHelper.ObtenerReporteConsumoInventario(fechaInicio, fechaFin);

                // Mostrar debug info
                string debugInfo = $"Diagnóstico de Datos:\n" +
                    $"📊 Ventas: {dtVentas?.Rows.Count ?? 0} registros\n" +
                    $"📦 Productos: {dtProductos?.Rows.Count ?? 0} registros\n" +
                    $"👤 Empleados: {dtEmpleados?.Rows.Count ?? 0} registros\n" +
                    $"📋 Inventario: {dtInventario?.Rows.Count ?? 0} registros";

                System.Diagnostics.Debug.WriteLine(debugInfo);
                Console.WriteLine(debugInfo);

                // Procesar y mostrar datos
                ProcesarReporteVentas(dtVentas);
                ProcesarReporteProductos(dtProductos);
                ProcesarReporteEmpleados(dtEmpleados);
                ProcesarReporteInventario(dtInventario);

                MessageBox.Show($"✅ Reportes cargados exitosamente.\n\n{debugInfo}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al cargar reportes: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Procesa el reporte de ventas y actualiza la UI.
        /// </summary>
        private void ProcesarReporteVentas(DataTable dtVentas)
        {
            if (dtVentas == null || dtVentas.Rows.Count == 0)
            {
                _lblTotalVentas.Text = "$0.00";
                _lblCantidadOrdenes.Text = "0 Órdenes";
                _pnlChart.Invalidate();
                System.Diagnostics.Debug.WriteLine("✓ REPORTE VENTAS: Actualizado (sin datos)");
                return;
            }

            // Calcular totales
            decimal totalVentas = 0;
            int totalOrdenes = 0;

            foreach (DataRow row in dtVentas.Rows)
            {
                if (row["VentasTotal"] != DBNull.Value)
                    totalVentas += Convert.ToDecimal(row["VentasTotal"]);

                if (row["CantidadOrdenes"] != DBNull.Value)
                    totalOrdenes += Convert.ToInt32(row["CantidadOrdenes"]);
            }

            // Actualizar UI
            _lblTotalVentas.Text = $"${totalVentas:F2}";
            _lblCantidadOrdenes.Text = $"{totalOrdenes} Órdenes";
            _pnlChart.Invalidate();  // Redibujar gráfico

            System.Diagnostics.Debug.WriteLine($"✓ REPORTE VENTAS: Actualizado - ${totalVentas:F2} en {totalOrdenes} órdenes");
        }

        /// <summary>
        /// Procesa el reporte de productos más vendidos y actualiza la UI.
        /// </summary>
        private void ProcesarReporteProductos(DataTable dtProductos)
        {
            if (dtProductos == null || dtProductos.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("✓ REPORTE PRODUCTOS: Actualizado (sin datos)");
                return;
            }

            // Limpiar controles previos
            _cardProd.Controls.Clear();

            // Agregar encabezado
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

            // Mostrar hasta 4 productos
            int maxProductos = Math.Min(4, dtProductos.Rows.Count);
            for (int i = 0; i < maxProductos; i++)
            {
                DataRow row = dtProductos.Rows[i];
                string nombre = row["Producto"].ToString();
                int cantidad = Convert.ToInt32(row["CantidadVendida"]);

                AddProductRow(_cardProd, nombre, cantidad, maxProductos * 15, i);
            }

            // Calcular total de ventas
            int totalProductosVendidos = 0;
            foreach (DataRow row in dtProductos.Rows)
            {
                totalProductosVendidos += Convert.ToInt32(row["CantidadVendida"]);
            }

            // Agregar etiqueta de total
            var lblTotal = new Label
            {
                Text = $"{totalProductosVendidos} Ventas Totales",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, _cardProd.Height - 35)
            };
            _cardProd.Controls.Add(lblTotal);

            System.Diagnostics.Debug.WriteLine($"✓ REPORTE PRODUCTOS: Actualizado - {maxProductos} productos mostrados ({totalProductosVendidos} ventas totales)");
        }

        /// <summary>
        /// Procesa el reporte de ingresos por empleado y actualiza la UI.
        /// </summary>
        private void ProcesarReporteEmpleados(DataTable dtEmpleados)
        {
            if (dtEmpleados == null || dtEmpleados.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("✓ REPORTE EMPLEADOS: Actualizado (sin datos)");
                return;
            }

            // Limpiar controles previos (excepto botones de exportación)
            var botonesExportacion = new List<Button>();
            foreach (Control ctrl in _cardEmp.Controls)
            {
                if (ctrl is Button btn && (btn.Text.Contains("PDF") || btn.Text.Contains("EXCEL")))
                {
                    botonesExportacion.Add(btn);
                }
            }
            _cardEmp.Controls.Clear();

            // Agregar encabezado
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

            // Mostrar hasta 4 empleados
            int maxEmpleados = Math.Min(4, dtEmpleados.Rows.Count);
            for (int i = 0; i < maxEmpleados; i++)
            {
                DataRow row = dtEmpleados.Rows[i];
                string nombre = row["Empleado"].ToString();
                decimal ingreso = Convert.ToDecimal(row["IngresoGenerado"]);

                // Calcular porcentaje (si hay múltiples empleados)
                int porcentaje = maxEmpleados > 0 ? (i + 1) * (100 / maxEmpleados) : 0;
                string ingresoFormato = $"${ingreso:F2}";

                AddEmployeeRow(_cardEmp, nombre, ingresoFormato, porcentaje, i);
            }

            // Restaurar botones de exportación
            foreach (var btn in botonesExportacion)
            {
                _cardEmp.Controls.Add(btn);
                btn.BringToFront();
            }

            System.Diagnostics.Debug.WriteLine($"✓ REPORTE EMPLEADOS: Actualizado - {maxEmpleados} empleados mostrados");
        }

        /// <summary>
        /// Procesa el reporte de consumo de inventario y actualiza la UI.
        /// </summary>
        private void ProcesarReporteInventario(DataTable dtInventario)
        {
            if (dtInventario == null || dtInventario.Rows.Count == 0)
            {
                _lblInventarioConsumido.Text = "0% del Inventario Consumido";
                return;
            }

            // Limpiar controles previos
            _cardInv.Controls.Clear();

            // Agregar encabezado
            var hdr = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(_cardInv.Width, 30),
                BackColor = CHeaderCard
            };
            _cardInv.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = "Consumo de Inventario",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            });

            // Calcular consumo total y costo
            decimal costoTotal = 0;
            decimal stockTotal = 0;
            int maxInsumos = Math.Min(4, dtInventario.Rows.Count);

            foreach (DataRow row in dtInventario.Rows)
            {
                decimal costoUnitario = row["CostoUnitario"] != DBNull.Value ? Convert.ToDecimal(row["CostoUnitario"]) : 0;
                decimal stockActual = row["StockActual"] != DBNull.Value ? Convert.ToDecimal(row["StockActual"]) : 0;
                costoTotal += Convert.ToDecimal(row["CostoTotal"] ?? 0);
                stockTotal += stockActual;
            }

            // Mostrar hasta 4 insumos
            for (int i = 0; i < maxInsumos; i++)
            {
                DataRow row = dtInventario.Rows[i];
                string nombre = row["Insumo"].ToString();  // Corregido: era "Nombre", debe ser "Insumo"
                int stock = Convert.ToInt32(row["StockActual"] ?? 0);
                int maxStock = stock > 0 ? stock : 100;  // Valor por defecto para la barra

                AddProductRow(_cardInv, nombre, stock, maxStock, i);
            }

            // Calcular porcentaje de consumo
            int porcentajeConsumido = stockTotal > 0 ? (int)((costoTotal / (stockTotal * 0.5m)) * 100) : 0;
            porcentajeConsumido = Math.Min(100, porcentajeConsumido);  // Limitar a 100%

            _lblInventarioConsumido.Text = $"{porcentajeConsumido}% del Inventario Consumido";
            _cardInv.Controls.Add(_lblInventarioConsumido);

            System.Diagnostics.Debug.WriteLine($"✓ REPORTE INVENTARIO: Actualizado - {maxInsumos} insumos mostrados ({porcentajeConsumido}% consumido)");
        }
    }
}