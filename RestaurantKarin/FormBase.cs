namespace RestaurantKarin
{
    public partial class Base : Form
    {
        private bool isMenuExpanded = true;
        private const int ExpandedWidth = 153;
        private const int CollapsedWidth = 50;

        public Base()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void Configuración_Click(object sender, EventArgs e)
        {

        }

        private void Inventario_Click(object sender, EventArgs e)
        {

        }

        private void Reportes_Click(object sender, EventArgs e)
        {

        }

        private void LogOut_Click(object sender, EventArgs e)
        {

        }

        private void PanelMenu_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BtnToggleMenu_Click(object sender, EventArgs e)
        {
            if (isMenuExpanded)
            {
                CollapseMenu();
            }
            else
            {
                ExpandMenu();
            }
        }

        private void CollapseMenu()
        {
            isMenuExpanded = false;
            PanelMenu.Width = CollapsedWidth;
            PanelContenedor.Location = new Point(CollapsedWidth, 0);
            PanelContenedor.Size = new Size(ClientSize.Width - CollapsedWidth, ClientSize.Height);
            HideAllMenuItems();
            BtnToggleMenu.Text = "►";
            BtnToggleMenu.Location = new Point(10, 8);
        }

        private void ExpandMenu()
        {
            isMenuExpanded = true;
            PanelMenu.Width = ExpandedWidth;
            PanelContenedor.Location = new Point(ExpandedWidth, 0);
            PanelContenedor.Size = new Size(ClientSize.Width - ExpandedWidth, ClientSize.Height);
            ShowAllMenuItems();
            BtnToggleMenu.Text = "◄";
            BtnToggleMenu.Location = new Point(120, 8);
        }

        private void ShowAllMenuItems()
        {
            Pedidos.Visible = true;
            Cuentas.Visible = true;
            Inventario.Visible = true;
            Recetas.Visible = true;
            Reportes.Visible = true;
            Configuración.Visible = true;
            LogOut.Visible = true;
        }

        private void HideAllMenuItems()
        {
            Pedidos.Visible = false;
            Cuentas.Visible = false;
            Inventario.Visible = false;
            Recetas.Visible = false;
            Reportes.Visible = false;
            Configuración.Visible = false;
            LogOut.Visible = false;
        }

        private void PanelContenedor_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
