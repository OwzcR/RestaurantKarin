namespace RestaurantKarin
{
    partial class FormReporte
    {
        private System.ComponentModel.IContainer components = null;

        // ── Filtros ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlFiltros;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpFechaInicio;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpFechaFin;
        private Guna.UI2.WinForms.Guna2Button btnVerReporte;

        // ── Contenedor principal ───────────────────────────────────────────
        private System.Windows.Forms.Panel pnlPrincipal;

        // ── Fila 1 (Ventas | Productos) ────────────────────────────────────
        private System.Windows.Forms.Panel pnlFila1;
        private System.Windows.Forms.Panel pnlVentas;
        private System.Windows.Forms.Label lblVentas;
        private System.Windows.Forms.Panel chartVentas;   // Panel para dibujo GDI+
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Panel pnlProductos;
        private System.Windows.Forms.Label lblProductosMasVendidos;
        private System.Windows.Forms.FlowLayoutPanel flpProductos;

        // ── Fila 2 (Inventario | Empleados) ───────────────────────────────
        private System.Windows.Forms.Panel pnlFila2;
        private System.Windows.Forms.Panel pnlInventario;
        private System.Windows.Forms.Label lblConsumoInventario;
        private System.Windows.Forms.FlowLayoutPanel flpInventario;
        private System.Windows.Forms.Label lblPorcentajeInventario;
        private System.Windows.Forms.Panel pnlIngresos;
        private System.Windows.Forms.Label lblIngresosPorEmpleado;
        private System.Windows.Forms.FlowLayoutPanel flpEmpleados;

        // ── Botones exportar ──────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBotones;
        private Guna.UI2.WinForms.Guna2Button btnExportarPDF;
        private Guna.UI2.WinForms.Guna2Button btnExportarExcel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Guna2Button guna2Button1;
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pnlFiltros = new Panel();
            dtpFechaInicio = new Guna.UI2.WinForms.Guna2DateTimePicker();
            dtpFechaFin = new Guna.UI2.WinForms.Guna2DateTimePicker();
            btnVerReporte = new Guna.UI2.WinForms.Guna2Button();
            pnlPrincipal = new Panel();
            pnlFila2 = new Panel();
            pnlIngresos = new Panel();
            pnlBotones = new Panel();
            btnExportarPDF = new Guna.UI2.WinForms.Guna2Button();
            btnExportarExcel = new Guna.UI2.WinForms.Guna2Button();
            flpEmpleados = new FlowLayoutPanel();
            lblIngresosPorEmpleado = new Label();
            pnlInventario = new Panel();
            flpInventario = new FlowLayoutPanel();
            lblPorcentajeInventario = new Label();
            lblConsumoInventario = new Label();
            pnlFila1 = new Panel();
            pnlProductos = new Panel();
            flpProductos = new FlowLayoutPanel();
            lblProductosMasVendidos = new Label();
            pnlVentas = new Panel();
            chartVentas = new Panel();
            lblTotal = new Label();
            lblVentas = new Label();
            guna2Button1 = new Guna.UI2.WinForms.Guna2Button();
            pnlFiltros.SuspendLayout();
            pnlPrincipal.SuspendLayout();
            pnlFila2.SuspendLayout();
            pnlIngresos.SuspendLayout();
            pnlBotones.SuspendLayout();
            pnlInventario.SuspendLayout();
            pnlFila1.SuspendLayout();
            pnlProductos.SuspendLayout();
            pnlVentas.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Button1
            // 
            guna2Button1.BackColor = Color.Transparent;
            guna2Button1.BorderRadius = 10;
            guna2Button1.CustomizableEdges = customizableEdges1;
            guna2Button1.FillColor = Color.FromArgb(23, 91, 122);
            guna2Button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            guna2Button1.ForeColor = Color.White;
            guna2Button1.Location = new Point(53, 13);
            guna2Button1.Name = "guna2Button1";
            guna2Button1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Button1.Size = new Size(231, 61);
            guna2Button1.TabIndex = 4;
            guna2Button1.Text = "PERIODO DE TIEMPO";
            guna2Button1.Click += guna2Button1_Click;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Anchor = AnchorStyles.None;
            pnlFiltros.BackColor = Color.Transparent;
            pnlFiltros.Controls.Add(guna2Button1);
            pnlFiltros.Controls.Add(dtpFechaInicio);
            pnlFiltros.Controls.Add(dtpFechaFin);
            pnlFiltros.Controls.Add(btnVerReporte);
            pnlFiltros.Location = new Point(13, 44);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Padding = new Padding(12, 10, 12, 10);
            pnlFiltros.Size = new Size(1217, 95);
            pnlFiltros.TabIndex = 0;
            pnlFiltros.Paint += pnlFiltros_Paint;
            // 
            // dtpFechaInicio
            // 
            dtpFechaInicio.BorderRadius = 10;
            dtpFechaInicio.Checked = true;
            dtpFechaInicio.CustomizableEdges = customizableEdges3;
            dtpFechaInicio.FillColor = Color.FromArgb(232, 232, 232);
            dtpFechaInicio.Font = new Font("Segoe UI", 9F);
            dtpFechaInicio.Format = DateTimePickerFormat.Short;
            dtpFechaInicio.Location = new Point(290, 13);
            dtpFechaInicio.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            dtpFechaInicio.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            dtpFechaInicio.Name = "dtpFechaInicio";
            dtpFechaInicio.ShadowDecoration.CustomizableEdges = customizableEdges4;
            dtpFechaInicio.Size = new Size(152, 61);
            dtpFechaInicio.TabIndex = 1;
            dtpFechaInicio.Value = new DateTime(2026, 4, 21, 14, 47, 1, 832);
            // 
            // dtpFechaFin
            // 
            dtpFechaFin.BorderRadius = 10;
            dtpFechaFin.Checked = true;
            dtpFechaFin.CustomizableEdges = customizableEdges5;
            dtpFechaFin.FillColor = Color.FromArgb(232, 232, 232);
            dtpFechaFin.Font = new Font("Segoe UI", 9F);
            dtpFechaFin.Format = DateTimePickerFormat.Short;
            dtpFechaFin.Location = new Point(446, 13);
            dtpFechaFin.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            dtpFechaFin.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            dtpFechaFin.Name = "dtpFechaFin";
            dtpFechaFin.ShadowDecoration.CustomizableEdges = customizableEdges6;
            dtpFechaFin.Size = new Size(158, 61);
            dtpFechaFin.TabIndex = 2;
            dtpFechaFin.Value = new DateTime(2026, 4, 21, 14, 47, 1, 870);
            dtpFechaFin.ValueChanged += dtpFechaFin_ValueChanged;
            // 
            // btnVerReporte
            // 
            btnVerReporte.BackColor = Color.Transparent;
            btnVerReporte.BorderRadius = 10;
            btnVerReporte.CustomizableEdges = customizableEdges7;
            btnVerReporte.FillColor = Color.FromArgb(23, 91, 122);
            btnVerReporte.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnVerReporte.ForeColor = Color.White;
            btnVerReporte.Location = new Point(610, 13);
            btnVerReporte.Name = "btnVerReporte";
            btnVerReporte.ShadowDecoration.CustomizableEdges = customizableEdges8;
            btnVerReporte.Size = new Size(160, 61);
            btnVerReporte.TabIndex = 3;
            btnVerReporte.Text = "VER REPORTE";
            btnVerReporte.Click += BtnVerReporte_Click;
            // 
            // pnlPrincipal
            // 
            pnlPrincipal.Anchor = AnchorStyles.None;
            pnlPrincipal.BackColor = Color.Transparent;
            pnlPrincipal.Controls.Add(pnlFila2);
            pnlPrincipal.Controls.Add(pnlFila1);
            pnlPrincipal.Location = new Point(13, 145);
            pnlPrincipal.Name = "pnlPrincipal";
            pnlPrincipal.Padding = new Padding(14);
            pnlPrincipal.Size = new Size(1231, 820);
            pnlPrincipal.TabIndex = 1;
            pnlPrincipal.Paint += pnlPrincipal_Paint;
            // 
            // pnlFila2
            // 
            pnlFila2.Anchor = AnchorStyles.None;
            pnlFila2.BackColor = Color.Transparent;
            pnlFila2.Controls.Add(pnlIngresos);
            pnlFila2.Controls.Add(pnlInventario);
            pnlFila2.Location = new Point(14, 450);
            pnlFila2.Name = "pnlFila2";
            pnlFila2.Padding = new Padding(0, 10, 0, 0);
            pnlFila2.Size = new Size(1203, 370);
            pnlFila2.TabIndex = 1;
            pnlFila2.Paint += pnlFila2_Paint;
            // 
            // pnlIngresos
            // 
            pnlIngresos.Anchor = AnchorStyles.None;
            pnlIngresos.BackColor = Color.FromArgb(232, 232, 232);
            pnlIngresos.Controls.Add(pnlBotones);
            pnlIngresos.Controls.Add(flpEmpleados);
            pnlIngresos.Controls.Add(lblIngresosPorEmpleado);
            pnlIngresos.Location = new Point(602, 33);
            pnlIngresos.Name = "pnlIngresos";
            pnlIngresos.Padding = new Padding(10);
            pnlIngresos.Size = new Size(590, 320);
            pnlIngresos.TabIndex = 1;
            pnlIngresos.Paint += pnlIngresos_Paint;
            // 
            // pnlBotones
            // 
            pnlBotones.Anchor = AnchorStyles.None;
            pnlBotones.BackColor = Color.Transparent;
            pnlBotones.Controls.Add(btnExportarPDF);
            pnlBotones.Controls.Add(btnExportarExcel);
            pnlBotones.Location = new Point(13, 247);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(574, 63);
            pnlBotones.TabIndex = 2;
            pnlBotones.Paint += pnlBotones_Paint;
            // 
            // btnExportarPDF
            // 
            btnExportarPDF.Anchor = AnchorStyles.None;
            btnExportarPDF.BackColor = Color.Transparent;
            btnExportarPDF.BorderRadius = 10;
            btnExportarPDF.CustomizableEdges = customizableEdges9;
            btnExportarPDF.FillColor = Color.FromArgb(23, 91, 122);
            btnExportarPDF.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportarPDF.ForeColor = Color.White;
            btnExportarPDF.Location = new Point(3, 3);
            btnExportarPDF.Name = "btnExportarPDF";
            btnExportarPDF.ShadowDecoration.CustomizableEdges = customizableEdges10;
            btnExportarPDF.Size = new Size(214, 47);
            btnExportarPDF.TabIndex = 0;
            btnExportarPDF.Text = "+ EXPORTAR PDF";
            btnExportarPDF.TextFormatNoPrefix = true;
            btnExportarPDF.Click += BtnExportarPDF_Click;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.Anchor = AnchorStyles.None;
            btnExportarExcel.BackColor = Color.Transparent;
            btnExportarExcel.BorderRadius = 10;
            btnExportarExcel.CustomizableEdges = customizableEdges11;
            btnExportarExcel.FillColor = Color.FromArgb(23, 91, 122);
            btnExportarExcel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Location = new Point(235, 3);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.ShadowDecoration.CustomizableEdges = customizableEdges12;
            btnExportarExcel.Size = new Size(214, 47);
            btnExportarExcel.TabIndex = 1;
            btnExportarExcel.Text = "+ EXPORTAR EXCEL";
            btnExportarExcel.TextFormatNoPrefix = true;
            btnExportarExcel.Click += BtnExportarExcel_Click;
            // 
            // flpEmpleados
            // 
            flpEmpleados.Anchor = AnchorStyles.None;
            flpEmpleados.BackColor = Color.Transparent;
            flpEmpleados.FlowDirection = FlowDirection.TopDown;
            flpEmpleados.Location = new Point(10, 64);
            flpEmpleados.Name = "flpEmpleados";
            flpEmpleados.Size = new Size(566, 177);
            flpEmpleados.TabIndex = 1;
            flpEmpleados.WrapContents = false;
            // 
            // lblIngresosPorEmpleado
            // 
            lblIngresosPorEmpleado.Anchor = AnchorStyles.None;
            lblIngresosPorEmpleado.BackColor = Color.FromArgb(30, 80, 120);
            lblIngresosPorEmpleado.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblIngresosPorEmpleado.ForeColor = Color.White;
            lblIngresosPorEmpleado.Location = new Point(0, 0);
            lblIngresosPorEmpleado.Name = "lblIngresosPorEmpleado";
            lblIngresosPorEmpleado.Size = new Size(590, 29);
            lblIngresosPorEmpleado.TabIndex = 0;
            lblIngresosPorEmpleado.Text = "Ingresos por Empleado";
            // 
            // pnlInventario
            // 
            pnlInventario.Anchor = AnchorStyles.None;
            pnlInventario.BackColor = Color.FromArgb(232, 232, 232);
            pnlInventario.Controls.Add(flpInventario);
            pnlInventario.Controls.Add(lblPorcentajeInventario);
            pnlInventario.Controls.Add(lblConsumoInventario);
            pnlInventario.Location = new Point(0, 33);
            pnlInventario.Name = "pnlInventario";
            pnlInventario.Padding = new Padding(10);
            pnlInventario.Size = new Size(590, 320);
            pnlInventario.TabIndex = 0;
            pnlInventario.Paint += pnlInventario_Paint;
            // 
            // flpInventario
            // 
            flpInventario.Anchor = AnchorStyles.None;
            flpInventario.BackColor = Color.Transparent;
            flpInventario.FlowDirection = FlowDirection.TopDown;
            flpInventario.Location = new Point(11, 75);
            flpInventario.Name = "flpInventario";
            flpInventario.Size = new Size(566, 179);
            flpInventario.TabIndex = 1;
            flpInventario.WrapContents = false;
            // 
            // lblPorcentajeInventario
            // 
            lblPorcentajeInventario.Anchor = AnchorStyles.None;
            lblPorcentajeInventario.BackColor = Color.Transparent;
            lblPorcentajeInventario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPorcentajeInventario.ForeColor = Color.Black;
            lblPorcentajeInventario.Location = new Point(13, 257);
            lblPorcentajeInventario.Name = "lblPorcentajeInventario";
            lblPorcentajeInventario.Size = new Size(260, 31);
            lblPorcentajeInventario.TabIndex = 2;
            lblPorcentajeInventario.Text = "67% del Inventario Consumido";
            // 
            // lblConsumoInventario
            // 
            lblConsumoInventario.Anchor = AnchorStyles.None;
            lblConsumoInventario.BackColor = Color.FromArgb(30, 80, 120);
            lblConsumoInventario.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblConsumoInventario.ForeColor = Color.White;
            lblConsumoInventario.Location = new Point(0, 0);
            lblConsumoInventario.Name = "lblConsumoInventario";
            lblConsumoInventario.Size = new Size(590, 28);
            lblConsumoInventario.TabIndex = 0;
            lblConsumoInventario.Text = "Consumo de Inventario";
            lblConsumoInventario.Click += lblConsumoInventario_Click;
            // 
            // pnlFila1
            // 
            pnlFila1.Anchor = AnchorStyles.None;
            pnlFila1.BackColor = Color.Transparent;
            pnlFila1.Controls.Add(pnlProductos);
            pnlFila1.Controls.Add(pnlVentas);
            pnlFila1.Location = new Point(17, 17);
            pnlFila1.Name = "pnlFila1";
            pnlFila1.Size = new Size(1203, 427);
            pnlFila1.TabIndex = 0;
            pnlFila1.Paint += pnlFila1_Paint;
            // 
            // pnlProductos
            // 
            pnlProductos.Anchor = AnchorStyles.None;
            pnlProductos.BackColor = Color.FromArgb(232, 232, 232);
            pnlProductos.Controls.Add(flpProductos);
            pnlProductos.Controls.Add(lblProductosMasVendidos);
            pnlProductos.Location = new Point(602, 20);
            pnlProductos.Name = "pnlProductos";
            pnlProductos.Padding = new Padding(10);
            pnlProductos.Size = new Size(590, 363);
            pnlProductos.TabIndex = 1;
            // 
            // flpProductos
            // 
            flpProductos.Anchor = AnchorStyles.None;
            flpProductos.BackColor = Color.Transparent;
            flpProductos.FlowDirection = FlowDirection.TopDown;
            flpProductos.Location = new Point(13, 76);
            flpProductos.Name = "flpProductos";
            flpProductos.Size = new Size(546, 217);
            flpProductos.TabIndex = 1;
            flpProductos.WrapContents = false;
            flpProductos.Paint += flpProductos_Paint;
            // 
            // lblProductosMasVendidos
            // 
            lblProductosMasVendidos.Anchor = AnchorStyles.None;
            lblProductosMasVendidos.BackColor = Color.FromArgb(30, 80, 120);
            lblProductosMasVendidos.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblProductosMasVendidos.ForeColor = Color.White;
            lblProductosMasVendidos.Location = new Point(0, 0);
            lblProductosMasVendidos.Name = "lblProductosMasVendidos";
            lblProductosMasVendidos.Size = new Size(590, 37);
            lblProductosMasVendidos.TabIndex = 0;
            lblProductosMasVendidos.Text = "Productos Más Vendidos";
            // 
            // pnlVentas
            // 
            pnlVentas.Anchor = AnchorStyles.None;
            pnlVentas.BackColor = Color.FromArgb(232, 232, 232);
            pnlVentas.Controls.Add(chartVentas);
            pnlVentas.Controls.Add(lblTotal);
            pnlVentas.Controls.Add(lblVentas);
            pnlVentas.Location = new Point(0, 20);
            pnlVentas.Name = "pnlVentas";
            pnlVentas.Padding = new Padding(10);
            pnlVentas.Size = new Size(590, 363);
            pnlVentas.TabIndex = 0;
            // 
            // chartVentas
            // 
            chartVentas.Anchor = AnchorStyles.None;
            chartVentas.BackColor = Color.Transparent;
            chartVentas.Location = new Point(13, 60);
            chartVentas.Name = "chartVentas";
            chartVentas.Size = new Size(546, 233);
            chartVentas.TabIndex = 1;
            // 
            // lblTotal
            // 
            lblTotal.Anchor = AnchorStyles.None;
            lblTotal.BackColor = Color.Transparent;
            lblTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotal.ForeColor = Color.Black;
            lblTotal.Location = new Point(13, 308);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(160, 25);
            lblTotal.TabIndex = 2;
            lblTotal.Text = "$3,250   Total";
            // 
            // lblVentas
            // 
            lblVentas.Anchor = AnchorStyles.None;
            lblVentas.BackColor = Color.FromArgb(30, 80, 120);
            lblVentas.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblVentas.ForeColor = Color.White;
            lblVentas.Location = new Point(0, 0);
            lblVentas.Name = "lblVentas";
            lblVentas.Size = new Size(590, 37);
            lblVentas.TabIndex = 0;
            lblVentas.Text = "Ventas";
            lblVentas.Click += lblVentas_Click;
            // 
            // FormReporte
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Transparent;
            Controls.Add(pnlPrincipal);
            Controls.Add(pnlFiltros);
            Name = "FormReporte";
            Size = new Size(1250, 1000);
            Load += FormReporte_Load_1;
            pnlFiltros.ResumeLayout(false);
            pnlPrincipal.ResumeLayout(false);
            pnlFila2.ResumeLayout(false);
            pnlIngresos.ResumeLayout(false);
            pnlBotones.ResumeLayout(false);
            pnlInventario.ResumeLayout(false);
            pnlFila1.ResumeLayout(false);
            pnlProductos.ResumeLayout(false);
            pnlVentas.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion

        private Guna.UI2.WinForms.Guna2Button guna2Button1;
    }
}