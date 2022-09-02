namespace HeadsetControlTray
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeTray();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HeadsetTray.Visible = false;

            Application.Exit();
        }
    }
}