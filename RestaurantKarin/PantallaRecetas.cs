using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class PantallaRecetas : UserControl
    {
        private FlowLayoutPanel _flpLista = null!;
        private TextBox _txtBuscar = null!;
        private DataGridView _dgvDetalle = null!;
        private Label _lblTituloReceta = null!;
        private Label _lblCostoPorcionValor = null!;
        private Panel _pnlDetalleVacio = null!;
        private Panel _pnlCapaEditor = null!;
        private Panel _pnlContenido = null!;
        private Form? _frmDimOverlay;
        private Form? _frmModalEliminar;
        private Panel _pnlDetalle = null!;
        private TableLayoutPanel _tlpMain = null!;
        private PantallaRecetaAgregarEditar? _editor;

        private Receta? _seleccion;
        private const string WatermarkBusqueda = "BUSCAR ID PLATILLO :";

        public PantallaRecetas()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            ConstruirInterfaz();
            RefrescarListaRecetas();
        }

        // ─── Construcción principal ───────────────────────────────────────────

        private void ConstruirInterfaz()
        {
            _pnlContenido = new Panel { BackColor = Color.White };
            _pnlContenido.Resize += (_, _) => AplicarRegionRedondeada(_pnlContenido, 20);

            _tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0f));

            _tlpMain.Controls.Add(CrearColumnaLista(), 0, 0);
            _pnlDetalle = CrearColumnaDetalle();
            _pnlDetalle.Visible = false;
            _tlpMain.Controls.Add(_pnlDetalle, 1, 0);

            _pnlContenido.Controls.Add(_tlpMain);

            _pnlCapaEditor = new Panel { Visible = false, BackColor = Color.White };
            _pnlCapaEditor.Resize += (_, _) => AplicarRegionRedondeada(_pnlCapaEditor, 20);

            Controls.Add(_pnlContenido);
            Controls.Add(_pnlCapaEditor);

            Resize += (_, _) => ColocarPanelContenido();
            HandleCreated += (_, _) => ColocarPanelContenido();

            ActualizarDetalle();
        }

        private void ColocarPanelContenido()
        {
            const int mH = 16, mV = 16;
            float pct = (_pnlDetalle?.Visible == true) ? 0.92f : 0.64f;
            int w = Math.Max(300, (int)(ClientSize.Width * pct));
            _pnlContenido.Bounds = new Rectangle(mH, mV, w, ClientSize.Height - mV * 2);
            int editorW = Math.Max(400, (int)(ClientSize.Width * 0.94f));
            _pnlCapaEditor.Bounds = new Rectangle(mH, mV, editorW, ClientSize.Height - mV * 2);
        }

        private void MostrarDetalle()
        {
            _tlpMain.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 52f);
            _tlpMain.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 48f);
            _pnlDetalle.Visible = true;
            ColocarPanelContenido();
        }

        private void OcultarDetalle()
        {
            _tlpMain.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _tlpMain.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0f);
            _pnlDetalle.Visible = false;
            ColocarPanelContenido();
        }

        private void MostrarOverlayEliminar()
        {
            OcultarOverlayEliminar();
            var mainForm = FindForm();
            if (mainForm == null) return;

            var origin = mainForm.PointToScreen(Point.Empty);

            _frmDimOverlay = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                AllowTransparency = true,
                BackColor = Color.FromArgb(13, 41, 78),
                Opacity = 0.80,
                StartPosition = FormStartPosition.Manual,
                Location = origin,
                Size = mainForm.ClientSize,
                ShowInTaskbar = false,
                Owner = mainForm
            };

            var pnlModal = CrearPanelModal();
            _frmModalEliminar = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.White,
                Size = new Size(560, 370),
                StartPosition = FormStartPosition.Manual,
                Location = new Point(
                    origin.X + mainForm.ClientSize.Width / 2 - 280,
                    origin.Y + mainForm.ClientSize.Height / 2 - 185
                ),
                ShowInTaskbar = false,
                Owner = mainForm
            };
            pnlModal.Dock = DockStyle.Fill;
            _frmModalEliminar.Controls.Add(pnlModal);

            _frmDimOverlay.Show(mainForm);
            _frmModalEliminar.Show(_frmDimOverlay);
        }

        private void OcultarOverlayEliminar()
        {
            _frmModalEliminar?.Close();
            _frmDimOverlay?.Close();
            _frmModalEliminar = null;
            _frmDimOverlay = null;
        }

        // ─── Columna izquierda: lista ─────────────────────────────────────────

        private Panel CrearColumnaLista()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Diseno.PanelGrisMedio,
                Padding = new Padding(14, 14, 14, 14)
            };

            var pnlBusqueda = ConstruirBarraBusqueda();

            _flpLista = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 8)
            };
            _flpLista.Resize += (_, _) => AjustarAnchoFilasLista();

            var pnlPie = new Panel { Height = 66, Dock = DockStyle.Bottom, BackColor = Color.Transparent };
            var btnAgregar = new Button
            {
                Text = "+  AGREGAR NUEVA RECETA",
                Height = 48,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnAgregar);
            btnAgregar.Click += (_, _) => MostrarEditor(nuevo: true);
            pnlPie.Controls.Add(btnAgregar);
            pnlPie.Resize += (_, _) =>
            {
                btnAgregar.Width = pnlPie.ClientSize.Width;
                btnAgregar.Left = 0;
                btnAgregar.Top = 10;
            };

            // Fill primero, luego Bottom, luego Top
            p.Controls.Add(_flpLista);
            p.Controls.Add(pnlPie);
            p.Controls.Add(pnlBusqueda);
            return p;
        }

        // ─── Columna derecha: detalle ─────────────────────────────────────────

        private Panel CrearColumnaDetalle()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 16, 20, 16)
            };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));   // título
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));    // tabla
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));    // costo (2 líneas)
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 62f));    // botones

            // ── Título ──
            _lblTituloReceta = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Diseno.TealPrimario,
                Text = "RECETA :",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            // ── DataGridView ──
            _dgvDetalle = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                GridColor = Color.FromArgb(220, 220, 220),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            _dgvDetalle.RowTemplate.Height = 36;

            // Cabeceras: fondo blanco, texto negro bold
            _dgvDetalle.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            _dgvDetalle.ColumnHeadersDefaultCellStyle.ForeColor = Diseno.TextoNormal;
            _dgvDetalle.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _dgvDetalle.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            _dgvDetalle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Filas normales
            _dgvDetalle.DefaultCellStyle.BackColor = Color.White;
            _dgvDetalle.DefaultCellStyle.ForeColor = Diseno.TextoNormal;
            _dgvDetalle.DefaultCellStyle.SelectionBackColor = Color.FromArgb(204, 229, 240);
            _dgvDetalle.DefaultCellStyle.SelectionForeColor = Diseno.TextoNormal;
            _dgvDetalle.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            // Filas alternas
            _dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            // Columnas con proporciones y alineaciones
            var colInsumo = new DataGridViewTextBoxColumn
            {
                Name = "cInsumo", HeaderText = "Insumo", FillWeight = 50f,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            };
            var colCant = new DataGridViewTextBoxColumn
            {
                Name = "cCant", HeaderText = "Cantidad", FillWeight = 25f,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            };
            var colUnid = new DataGridViewTextBoxColumn
            {
                Name = "cUnid", HeaderText = "Unidad", FillWeight = 25f,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            };
            _dgvDetalle.Columns.AddRange(colInsumo, colCant, colUnid);

            _pnlDetalleVacio = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _pnlDetalleVacio.Controls.Add(new Label
            {
                Text = "Selecciona una receta\npara ver sus detalles.",
                ForeColor = Diseno.TextoSecundario,
                Font = Diseno.FuenteCuerpo(10f),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            });

            var pnlGridHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlGridHost.Controls.Add(_dgvDetalle);
            pnlGridHost.Controls.Add(_pnlDetalleVacio);

            // ── Costo (etiqueta arriba, valor abajo) ──
            var pnlCosto = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var lblCostoTxt = new Label
            {
                Text = "Costo por porción:",
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(0, 8)
            };
            _lblCostoPorcionValor = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Diseno.VerdeCosto,
                Location = new Point(0, 32),
                Text = "$0.00"
            };
            pnlCosto.Controls.Add(lblCostoTxt);
            pnlCosto.Controls.Add(_lblCostoPorcionValor);

            // ── Botones (llenan el ancho completo, separados por gap) ──
            var pnlAcciones = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var btnEditar = new Button
            {
                Text = "+  EDITAR RECETA",
                Height = 48,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(27, 94, 32),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.MouseEnter += (_, _) => btnEditar.BackColor = Color.FromArgb(20, 70, 24);
            btnEditar.MouseLeave += (_, _) => btnEditar.BackColor = Color.FromArgb(27, 94, 32);
            btnEditar.Click += (_, _) =>
            {
                if (_seleccion == null) return;
                _seleccion = RecetasBaseDatos.ObtenerPorId(_seleccion.Id) ?? _seleccion;
                MostrarEditor(nuevo: false);
            };

            var btnBorrar = new Button
            {
                Text = "+  BORRAR RECETA",
                Height = 48,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Diseno.TealPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBorrar.FlatAppearance.BorderSize = 0;
            btnBorrar.MouseEnter += (_, _) => btnBorrar.BackColor = Diseno.TealOscuro;
            btnBorrar.MouseLeave += (_, _) => btnBorrar.BackColor = Diseno.TealPrimario;
            btnBorrar.Click += (_, _) =>
            {
                if (_seleccion == null) return;
                MostrarOverlayEliminar();
            };

            pnlAcciones.Controls.Add(btnBorrar);
            pnlAcciones.Controls.Add(btnEditar);
            pnlAcciones.Resize += (_, _) =>
            {
                const int gap = 10, top = 6;
                int mitad = (pnlAcciones.ClientSize.Width - gap) / 2;
                btnEditar.SetBounds(0, top, mitad, btnEditar.Height);
                btnBorrar.SetBounds(mitad + gap, top, mitad, btnBorrar.Height);
            };

            tlp.Controls.Add(_lblTituloReceta, 0, 0);
            tlp.Controls.Add(pnlGridHost, 0, 1);
            tlp.Controls.Add(pnlCosto, 0, 2);
            tlp.Controls.Add(pnlAcciones, 0, 3);
            p.Controls.Add(tlp);
            return p;
        }

        // ─── Barra de búsqueda ────────────────────────────────────────────────

        private Panel ConstruirBarraBusqueda()
        {
            var pnlBar = new Panel { Height = 54, Dock = DockStyle.Top, BackColor = Color.Transparent };

            var pnlPill = new Panel
            {
                Top = 7,
                Height = 40,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var picLupa = new Label
            {
                Text = "🔍",
                AutoSize = false,
                Size = new Size(28, 28),
                Location = new Point(6, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.Transparent
            };

            _txtBuscar = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Diseno.FuenteCuerpo(10f),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(38, 10),
                BackColor = Color.White,
                Text = WatermarkBusqueda
            };

            pnlPill.Controls.Add(_txtBuscar);
            pnlPill.Controls.Add(picLupa);
            RegistrarWatermarkBusqueda();

            var btnSeleccionar = new Button
            {
                Text = "SELECCIONAR",
                Size = new Size(130, 40),
                Top = 7,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnSeleccionar);
            btnSeleccionar.Click += BtnSeleccionar_Click;

            pnlBar.Controls.Add(btnSeleccionar);
            pnlBar.Controls.Add(pnlPill);
            pnlBar.Layout += (_, _) =>
            {
                const int gap = 10;
                btnSeleccionar.Left = pnlBar.ClientSize.Width - btnSeleccionar.Width;
                pnlPill.Left = 0;
                pnlPill.Width = Math.Max(80, btnSeleccionar.Left - gap);
                _txtBuscar.Width = Math.Max(10, pnlPill.Width - 46);
            };

            return pnlBar;
        }

        // ─── Modal: confirmar eliminación ────────────────────────────────────

        private Panel CrearPanelModal()
        {
            const int mw = 560, mh = 370;
            var modal = new Panel { Size = new Size(mw, mh), BackColor = Color.White };

            // ── Botón X circular ──
            const int xSz = 36;
            var btnCerrar = new Button
            {
                Text = "✕",
                Size = new Size(xSz, xSz),
                Location = new Point(mw - xSz - 14, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Diseno.TealPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            var circlePath = new GraphicsPath();
            circlePath.AddEllipse(0, 0, xSz, xSz);
            btnCerrar.Region = new Region(circlePath);
            circlePath.Dispose();
            btnCerrar.Click += (_, _) => OcultarOverlayEliminar();

            // ── Título ──
            var lblTit = new Label
            {
                Text = "¿ELIMINAR RECETA?",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Diseno.TealPrimario,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(40, 50),
                Size = new Size(mw - 80, 46)
            };

            // ── Texto explicativo ──
            var lblCuerpo = new Label
            {
                Text = "Atención: La receta seleccionada y toda su información asociada " +
                       "(ingredientes, costos, etc.) serán eliminadas permanentemente. " +
                       "Esta acción no se puede deshacer.",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(40, 108),
                Size = new Size(mw - 80, 90),
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };

            // ── ¿Deseas continuar? ──
            var lblPregunta = new Label
            {
                Text = "¿Deseas continuar?",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                ForeColor = Diseno.TextoNormal,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Location = new Point(40, 208),
                Size = new Size(mw - 80, 30)
            };

            // ── Botones ──
            const int btnW = 190, btnH = 44, btnGap = 20;
            int bx = (mw - btnW * 2 - btnGap) / 2;
            int by = mh - 72;

            var btnCancel = new Button { Text = "CANCELAR", Size = new Size(btnW, btnH), Location = new Point(bx, by) };
            Diseno.BotonSecundario(btnCancel);
            btnCancel.Click += (_, _) => OcultarOverlayEliminar();

            var btnSi = new Button { Text = "SÍ, ELIMINAR", Size = new Size(btnW, btnH), Location = new Point(bx + btnW + btnGap, by) };
            Diseno.BotonPrimario(btnSi);
            btnSi.Click += (_, _) =>
            {
                if (_seleccion != null)
                {
                    try { RecetasBaseDatos.Eliminar(_seleccion.Id); }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo eliminar.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _seleccion = null;
                    RefrescarListaRecetas();
                    ActualizarDetalle();
                }
                OcultarOverlayEliminar();
            };

            modal.Controls.AddRange(new Control[] { btnSi, btnCancel, lblPregunta, lblCuerpo, lblTit, btnCerrar });
            return modal;
        }

        // ─── Watermark ────────────────────────────────────────────────────────

        private void RegistrarWatermarkBusqueda()
        {
            _txtBuscar.ForeColor = Diseno.TextoPlaceholder;
            _txtBuscar.Enter += (_, _) =>
            {
                if (_txtBuscar.Text == WatermarkBusqueda)
                {
                    _txtBuscar.Text = "";
                    _txtBuscar.ForeColor = Diseno.TextoNormal;
                }
            };
            _txtBuscar.Leave += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(_txtBuscar.Text))
                {
                    _txtBuscar.Text = WatermarkBusqueda;
                    _txtBuscar.ForeColor = Diseno.TextoPlaceholder;
                }
            };
        }

        // ─── Búsqueda ─────────────────────────────────────────────────────────

        private void BtnSeleccionar_Click(object? sender, EventArgs e)
        {
            var q = _txtBuscar.Text.Trim();
            if (q == WatermarkBusqueda) q = "";
            if (string.IsNullOrEmpty(q))
            {
                MessageBox.Show("Escriba un ID de platillo o parte del nombre para buscar.", "Buscar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Receta? encontrada = null;
            if (int.TryParse(q, out int id))
                encontrada = RecetasBaseDatos.ObtenerPorId(id);
            if (encontrada == null)
            {
                encontrada = RecetasBaseDatos.ListarSoloCabeceras()
                    .FirstOrDefault(r => r.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase));
                if (encontrada != null)
                    encontrada = RecetasBaseDatos.ObtenerPorId(encontrada.Id);
            }
            if (encontrada == null)
            {
                MessageBox.Show("No se encontró ninguna receta con ese criterio.", "Buscar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _seleccion = encontrada;
            ActualizarDetalle();
            RefrescarListaRecetas(resaltarId: encontrada.Id);
            MostrarDetalle();
        }

        // ─── Lista ────────────────────────────────────────────────────────────

        private void RefrescarListaRecetas(int? resaltarId = null)
        {
            try
            {
                _flpLista.SuspendLayout();
                _flpLista.Controls.Clear();
                int i = 0;
                foreach (var r in RecetasBaseDatos.ListarSoloCabeceras())
                {
                    _flpLista.Controls.Add(CrearFilaReceta(r, zebraClaro: i % 2 == 0, resaltada: resaltarId == r.Id));
                    i++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer recetas.\n" + ex.Message, "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _flpLista.ResumeLayout();
                AjustarAnchoFilasLista();
            }
        }

        private Panel CrearFilaReceta(Receta r, bool zebraClaro, bool resaltada)
        {
            const int h = 52;
            var fila = new Panel
            {
                Height = h,
                Margin = new Padding(0, 0, 0, 4),
                BackColor = resaltada ? Color.FromArgb(204, 229, 240) : (zebraClaro ? Color.White : Diseno.ZebraGris)
            };

            var lblNombre = new Label
            {
                Text = r.Nombre,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Diseno.TextoNormal,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(12, 0),
                Height = h
            };

            var btnVer = new Button
            {
                Text = "👁\nVER",
                Size = new Size(62, h - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnVer);
            btnVer.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);

            int idFila = r.Id;
            btnVer.Click += (_, _) =>
            {
                _seleccion = RecetasBaseDatos.ObtenerPorId(idFila);
                ActualizarDetalle();
                RefrescarListaRecetas(resaltarId: idFila);
                MostrarDetalle();
            };

            fila.Controls.Add(btnVer);
            fila.Controls.Add(lblNombre);
            fila.Resize += (_, _) =>
            {
                btnVer.Left = fila.ClientSize.Width - btnVer.Width - 8;
                btnVer.Top = 4;
                lblNombre.Width = fila.ClientSize.Width - btnVer.Width - 28;
            };

            return fila;
        }

        private void AjustarAnchoFilasLista()
        {
            int w = _flpLista.ClientSize.Width - 4;
            if (w < 120) w = 120;
            foreach (Control c in _flpLista.Controls)
                c.Width = w;
        }

        // ─── Actualizar panel de detalle ──────────────────────────────────────

        private void ActualizarDetalle()
        {
            if (_seleccion == null)
            {
                _lblTituloReceta.Text = "RECETA :";
                _dgvDetalle.Rows.Clear();
                _pnlDetalleVacio.Visible = true;
                _dgvDetalle.Visible = false;
                _lblCostoPorcionValor.Text = "$0.00";
                _pnlDetalleVacio.BringToFront();
                OcultarDetalle();
                return;
            }

            _pnlDetalleVacio.Visible = false;
            _dgvDetalle.Visible = true;
            _dgvDetalle.BringToFront();
            _lblTituloReceta.Text = "RECETA : " + _seleccion.Nombre;
            _dgvDetalle.Rows.Clear();
            foreach (var l in _seleccion.Lineas)
                _dgvDetalle.Rows.Add(l.Insumo, l.Cantidad.ToString("0.##"), l.Unidad);
            _lblCostoPorcionValor.Text = "$" + _seleccion.CostoPorPorcion.ToString("0.00");
        }

        // ─── Editor ───────────────────────────────────────────────────────────

        private void MostrarEditor(bool nuevo)
        {
            if (!nuevo && _seleccion == null) return;

            _editor?.Dispose();
            _editor = nuevo
                ? new PantallaRecetaAgregarEditar(null)
                : new PantallaRecetaAgregarEditar(_seleccion);
            _editor.Dock = DockStyle.Fill;
            _pnlCapaEditor.Controls.Clear();
            _pnlCapaEditor.Controls.Add(_editor);
            _editor.Guardado += (_, _) => CerrarEditorYRefrescar();
            _editor.Cancelado += (_, _) => CerrarEditorSolo();
            _pnlCapaEditor.Visible = true;
            _pnlCapaEditor.BringToFront();
        }

        private void CerrarEditorSolo()
        {
            _pnlCapaEditor.Visible = false;
            _pnlCapaEditor.Controls.Clear();
            _editor?.Dispose();
            _editor = null;
        }

        private void CerrarEditorYRefrescar()
        {
            CerrarEditorSolo();
            RefrescarListaRecetas();
            if (_seleccion != null)
                _seleccion = RecetasBaseDatos.ObtenerPorId(_seleccion.Id);
            ActualizarDetalle();
        }

        // ─── Helpers visuales ─────────────────────────────────────────────────

        private static void AplicarRegionRedondeada(Control ctrl, int radio)
        {
            if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
            var path = GetRoundedPath(new Rectangle(0, 0, ctrl.Width, ctrl.Height), radio);
            ctrl.Region = new Region(path);
            path.Dispose();
        }

        private static GraphicsPath GetRoundedPath(Rectangle r, int radio)
        {
            int d = radio * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ─── Diseño ───────────────────────────────────────────────────────────

        private static class Diseno
        {
            public static readonly Color TealPrimario = Color.FromArgb(26, 82, 118);
            public static readonly Color TealOscuro = Color.FromArgb(21, 67, 96);
            public static readonly Color PanelGrisMedio = Color.FromArgb(224, 224, 224);
            public static readonly Color ZebraClaro = Color.FromArgb(250, 250, 250);
            public static readonly Color ZebraGris = Color.FromArgb(236, 236, 236);
            public static readonly Color VerdeCosto = Color.FromArgb(45, 106, 79);
            public static readonly Color TextoNormal = Color.FromArgb(33, 33, 33);
            public static readonly Color TextoSecundario = Color.FromArgb(97, 97, 97);
            public static readonly Color TextoPlaceholder = Color.FromArgb(158, 158, 158);
            public static readonly Color BotonCancelarFondo = Color.FromArgb(204, 204, 204);
            public static readonly Color BotonCancelarTexto = Color.FromArgb(66, 66, 66);

            public static Font FuenteTituloSeccion(float t = 11f) =>
                new Font("Segoe UI", t, FontStyle.Bold, GraphicsUnit.Point);

            public static Font FuenteCuerpo(float t = 10f) =>
                new Font("Segoe UI", t, FontStyle.Regular, GraphicsUnit.Point);

            public static Font FuenteBotonMayus(float t = 9.5f) =>
                new Font("Segoe UI", t, FontStyle.Bold, GraphicsUnit.Point);

            public static void BotonPrimario(Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = TealPrimario;
                b.ForeColor = Color.White;
                b.Font = FuenteBotonMayus();
                b.Cursor = Cursors.Hand;
                b.UseVisualStyleBackColor = false;
                b.MouseEnter += (_, _) => { b.BackColor = TealOscuro; };
                b.MouseLeave += (_, _) => { b.BackColor = TealPrimario; };
            }

            public static void BotonSecundario(Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = BotonCancelarFondo;
                b.ForeColor = BotonCancelarTexto;
                b.Font = FuenteBotonMayus();
                b.Cursor = Cursors.Hand;
                b.UseVisualStyleBackColor = false;
            }
        }
    }
}
