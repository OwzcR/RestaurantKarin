using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantKarin
{
    /// <summary>
    /// Pantalla principal de Recetas: lista, detalle, agregar (abre editor), editar, eliminar (modal).
    /// Los colores y tamaños del diseño están en la clase interna <see cref="Diseno"/> al final del archivo.
    /// Los datos vienen de <see cref="RecetasBaseDatos"/>.
    /// </summary>
    public class PantallaRecetas : UserControl
    {
        private FlowLayoutPanel _flpLista = null!;
        private TextBox _txtBuscar = null!;
        private DataGridView _dgvDetalle = null!;
        private Label _lblTituloReceta = null!;
        private Label _lblCostoPorcionValor = null!;
        private Panel _pnlDetalleVacio = null!;
        private Panel _pnlCapaEditor = null!;
        private Panel _pnlOverlayEliminar = null!;
        private PantallaRecetaAgregarEditar? _editor;

        private Receta? _seleccion;

        private const string WatermarkBusqueda = "BUSCAR ID PLATILLO :";

        public PantallaRecetas()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Padding = new Padding(20, 18, 20, 18);
            ConstruirInterfaz();
            RefrescarListaRecetas();
        }

        private void ConstruirInterfaz()
        {
            var pnlRaiz = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8)
            };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));

            tlp.Controls.Add(CrearColumnaIzquierda(), 0, 0);
            tlp.Controls.Add(CrearColumnaDerecha(), 1, 0);
            pnlRaiz.Controls.Add(tlp);

            _pnlCapaEditor = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.White
            };

            _pnlOverlayEliminar = CrearOverlayEliminar();
            _pnlOverlayEliminar.Dock = DockStyle.Fill;
            _pnlOverlayEliminar.Visible = false;

            Controls.Add(_pnlOverlayEliminar);
            Controls.Add(_pnlCapaEditor);
            Controls.Add(pnlRaiz);
        }

        private Panel CrearColumnaIzquierda()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Diseno.PanelGrisMedio,
                Padding = new Padding(16, 18, 16, 18),
                Margin = new Padding(0, 0, 10, 0)
            };

            var pnlBusqueda = new Panel { Height = 48, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var btnAtras = new Button
            {
                Size = new Size(40, 40),
                Location = new Point(0, 4),
                Text = "<",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Diseno.TextoTitulo,
                BackColor = Diseno.CremaFlechaAtras,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAtras.FlatAppearance.BorderSize = 0;
            btnAtras.Click += (_, _) =>
            {
                _seleccion = null;
                ActualizarDetalle();
                _txtBuscar.Text = WatermarkBusqueda;
                _txtBuscar.ForeColor = Diseno.TextoPlaceholder;
                RefrescarListaRecetas();
            };

            var pnlBuscarPill = new Panel
            {
                Location = new Point(48, 4),
                Height = 40,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtBuscar = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Diseno.FuenteCuerpo(10f),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(36, 10),
                Text = ""
            };
            var picLupa = new Label
            {
                Text = "🔍",
                AutoSize = false,
                Size = new Size(28, 28),
                Location = new Point(6, 6),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f)
            };
            pnlBuscarPill.Controls.Add(_txtBuscar);
            pnlBuscarPill.Controls.Add(picLupa);

            var btnSeleccionar = new Button
            {
                Text = "SELECCIONAR",
                Size = new Size(120, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnSeleccionar);
            pnlBusqueda.Controls.Add(btnSeleccionar);
            pnlBusqueda.Controls.Add(pnlBuscarPill);
            pnlBusqueda.Controls.Add(btnAtras);
            RegistrarWatermarkBusqueda();
            pnlBusqueda.Layout += (_, _) =>
            {
                btnSeleccionar.Left = pnlBusqueda.ClientSize.Width - btnSeleccionar.Width;
                btnSeleccionar.Top = 4;
                pnlBuscarPill.Width = Math.Max(100, pnlBusqueda.ClientSize.Width - 48 - btnSeleccionar.Width - 8);
                _txtBuscar.Width = pnlBuscarPill.Width - 44;
            };
            btnSeleccionar.Click += BtnSeleccionar_Click;

            _flpLista = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 12)
            };
            _flpLista.Resize += (_, _) => AjustarAnchoFilasLista();

            var pnlPieIzq = new Panel { Height = 56, Dock = DockStyle.Bottom, BackColor = Color.Transparent };
            var btnAgregar = new Button
            {
                Text = "+  AGREGAR NUEVA RECETA",
                Height = 46,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnAgregar);
            btnAgregar.Click += (_, _) => MostrarEditor(nuevo: true);
            pnlPieIzq.Controls.Add(btnAgregar);
            pnlPieIzq.Resize += (_, _) =>
            {
                int pad = 8;
                btnAgregar.Width = pnlPieIzq.ClientSize.Width - pad * 2;
                btnAgregar.Left = pad;
                btnAgregar.Top = 5;
            };

            p.Controls.Add(_flpLista);
            p.Controls.Add(pnlPieIzq);
            p.Controls.Add(pnlBusqueda);
            return p;
        }

        private Panel CrearColumnaDerecha()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Diseno.BlancoTarjeta,
                Padding = new Padding(18, 16, 18, 16),
                Margin = new Padding(10, 0, 0, 0)
            };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Diseno.BlancoTarjeta
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));

            _lblTituloReceta = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Diseno.TealPrimario,
                Text = "RECETA :",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

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
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 34,
                Font = Diseno.FuenteCuerpo()
            };
            _dgvDetalle.ColumnHeadersDefaultCellStyle.BackColor = Diseno.PanelGrisMedio;
            _dgvDetalle.ColumnHeadersDefaultCellStyle.Font = Diseno.FuenteTituloSeccion();
            _dgvDetalle.ColumnHeadersDefaultCellStyle.ForeColor = Diseno.TextoNormal;
            _dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = Diseno.ZebraClaro;
            _dgvDetalle.Columns.Add("cInsumo", "Insumo");
            _dgvDetalle.Columns.Add("cCant", "Cantidad");
            _dgvDetalle.Columns.Add("cUnid", "Unidad");

            _pnlDetalleVacio = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _pnlDetalleVacio.Controls.Add(new Label
            {
                Text = "Selecciona una receta de la lista o usa SELECCIONAR.",
                ForeColor = Diseno.TextoSecundario,
                Font = Diseno.FuenteCuerpo(10f),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            });

            var pnlGridHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlGridHost.Controls.Add(_dgvDetalle);
            pnlGridHost.Controls.Add(_pnlDetalleVacio);

            var pnlCosto = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlCosto.Controls.Add(new Label
            {
                Text = "Costo por porción:",
                AutoSize = true,
                Font = Diseno.FuenteTituloSeccion(10f),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(0, 10)
            });
            _lblCostoPorcionValor = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Diseno.VerdeCosto,
                Location = new Point(170, 8),
                Text = "$ 0.00"
            };
            pnlCosto.Controls.Add(_lblCostoPorcionValor);

            var pnlAcciones = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var btnEditar = new Button
            {
                Text = "+  EDITAR RECETA",
                Size = new Size(200, 42),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(27, 94, 32),
                ForeColor = Color.White,
                Font = Diseno.FuenteBotonMayus(),
                Cursor = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += (_, _) =>
            {
                if (_seleccion == null) return;
                _seleccion = RecetasBaseDatos.ObtenerPorId(_seleccion.Id) ?? _seleccion;
                MostrarEditor(nuevo: false);
            };

            var btnBorrar = new Button
            {
                Text = "+  BORRAR RECETA",
                Size = new Size(200, 42),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnBorrar);
            btnBorrar.Click += (_, _) =>
            {
                if (_seleccion == null) return;
                _pnlOverlayEliminar.Visible = true;
                _pnlOverlayEliminar.BringToFront();
            };

            pnlAcciones.Controls.Add(btnBorrar);
            pnlAcciones.Controls.Add(btnEditar);
            pnlAcciones.Resize += (_, _) =>
            {
                int gap = 12;
                btnBorrar.Left = pnlAcciones.ClientSize.Width - btnBorrar.Width;
                btnEditar.Left = btnBorrar.Left - gap - btnEditar.Width;
                btnBorrar.Top = 6;
                btnEditar.Top = 6;
            };

            tlp.Controls.Add(_lblTituloReceta, 0, 0);
            tlp.Controls.Add(pnlGridHost, 0, 1);
            tlp.Controls.Add(pnlCosto, 0, 2);
            tlp.Controls.Add(pnlAcciones, 0, 3);
            p.Controls.Add(tlp);

            ActualizarDetalle();
            return p;
        }

        private void RegistrarWatermarkBusqueda()
        {
            _txtBuscar.Text = WatermarkBusqueda;
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

        private Panel CrearOverlayEliminar()
        {
            var capa = new Panel { BackColor = Color.Black };
            capa.Paint += (_, e) =>
            {
                using var b = new SolidBrush(Color.FromArgb(170, 13, 41, 78));
                e.Graphics.FillRectangle(b, capa.ClientRectangle);
            };

            var modal = new Panel
            {
                Size = new Size(520, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            capa.Resize += (_, _) =>
            {
                modal.Left = (capa.ClientSize.Width - modal.Width) / 2;
                modal.Top = (capa.ClientSize.Height - modal.Height) / 2;
            };

            var btnCerrar = new Button
            {
                Text = "X",
                Size = new Size(32, 32),
                Location = new Point(modal.Width - 40, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Diseno.TealPrimario,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (_, _) => { capa.Visible = false; };

            var lblTit = new Label
            {
                Text = "¿ELIMINAR RECETA?",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Diseno.TealPrimario,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 44),
                Width = 480,
                Height = 32
            };

            var lblCuerpo = new Label
            {
                Text = "Atención: La receta seleccionada y toda su información asociada (ingredientes, costos, etc.) " +
                       "serán eliminadas permanentemente. Esta acción no se puede deshacer.\n\n¿Deseas continuar?",
                Font = Diseno.FuenteCuerpo(9.5f),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(28, 84),
                Size = new Size(460, 120),
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = false
            };

            var btnCancel = new Button { Text = "CANCELAR", Size = new Size(180, 40) };
            Diseno.BotonSecundario(btnCancel);
            btnCancel.Location = new Point(40, 240);
            btnCancel.Click += (_, _) => { capa.Visible = false; };

            var btnSi = new Button { Text = "SÍ, ELIMINAR", Size = new Size(180, 40) };
            Diseno.BotonPrimario(btnSi);
            btnSi.Location = new Point(300, 240);
            btnSi.Click += (_, _) =>
            {
                if (_seleccion != null)
                {
                    try
                    {
                        RecetasBaseDatos.Eliminar(_seleccion.Id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo eliminar.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    _seleccion = null;
                    RefrescarListaRecetas();
                    ActualizarDetalle();
                }
                capa.Visible = false;
            };

            modal.Controls.Add(btnSi);
            modal.Controls.Add(btnCancel);
            modal.Controls.Add(lblCuerpo);
            modal.Controls.Add(lblTit);
            modal.Controls.Add(btnCerrar);
            capa.Controls.Add(modal);

            return capa;
        }

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
        }

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
            int h = 52;
            var fila = new Panel
            {
                Height = h,
                Margin = new Padding(0, 0, 0, 6),
                BackColor = resaltada ? Color.FromArgb(220, 237, 244) : (zebraClaro ? Diseno.ZebraGris : Color.White)
            };

            var lblNombre = new Label
            {
                Text = r.Nombre,
                AutoEllipsis = true,
                Font = Diseno.FuenteCuerpo(10.5f),
                ForeColor = Diseno.TextoNormal,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(12, 0),
                Height = h - 2
            };

            var btnVer = new Button
            {
                Text = "👁\nVER",
                Size = new Size(64, h - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Diseno.BotonPrimario(btnVer);
            btnVer.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            int idFila = r.Id;
            btnVer.Click += (_, _) =>
            {
                _seleccion = RecetasBaseDatos.ObtenerPorId(idFila);
                ActualizarDetalle();
            };

            fila.Controls.Add(btnVer);
            fila.Controls.Add(lblNombre);
            fila.Resize += (_, _) =>
            {
                btnVer.Left = fila.ClientSize.Width - btnVer.Width - 8;
                btnVer.Top = 4;
                lblNombre.Width = fila.ClientSize.Width - btnVer.Width - 28;
                lblNombre.Top = (fila.ClientSize.Height - lblNombre.Height) / 2;
            };

            return fila;
        }

        private void AjustarAnchoFilasLista()
        {
            int w = _flpLista.ClientSize.Width - 24;
            if (w < 120) w = 120;
            foreach (Control c in _flpLista.Controls)
                c.Width = w;
        }

        private void ActualizarDetalle()
        {
            if (_seleccion == null)
            {
                _lblTituloReceta.Text = "RECETA :";
                _dgvDetalle.Rows.Clear();
                _pnlDetalleVacio.Visible = true;
                _dgvDetalle.Visible = false;
                _lblCostoPorcionValor.Text = "$ 0.00";
                _pnlDetalleVacio.BringToFront();
                return;
            }

            _pnlDetalleVacio.Visible = false;
            _dgvDetalle.Visible = true;
            _dgvDetalle.BringToFront();
            _lblTituloReceta.Text = "RECETA : " + _seleccion.Nombre;
            _dgvDetalle.Rows.Clear();
            foreach (var l in _seleccion.Lineas)
                _dgvDetalle.Rows.Add(l.Insumo, l.Cantidad.ToString("0.##"), l.Unidad);
            _lblCostoPorcionValor.Text = "$ " + _seleccion.CostoPorPorcion.ToString("0.00");
        }

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

        /// <summary>Colores y fuentes del diseño (todo en un solo sitio dentro de esta pantalla).</summary>
        private static class Diseno
        {
            public static readonly Color TealPrimario = Color.FromArgb(26, 82, 118);
            public static readonly Color TealOscuro = Color.FromArgb(21, 67, 96);
            public static readonly Color PanelGrisMedio = Color.FromArgb(224, 224, 224);
            public static readonly Color ZebraClaro = Color.FromArgb(250, 250, 250);
            public static readonly Color ZebraGris = Color.FromArgb(236, 236, 236);
            public static readonly Color BlancoTarjeta = Color.White;
            public static readonly Color CremaFlechaAtras = Color.FromArgb(249, 231, 159);
            public static readonly Color VerdeCosto = Color.FromArgb(45, 106, 79);
            public static readonly Color TextoTitulo = Color.FromArgb(13, 41, 78);
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
