using System;
using System.Windows.Forms;

namespace keybon
{
    public partial class About : Form
    {
        MainWindow ownerForm = null;

        public About()
        {
        }

        public About(MainWindow ownerForm)
        {
            InitializeComponent();
            this.ownerForm = ownerForm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CoretechR/Keybon");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hackaday.io/project/176239");
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.thingiverse.com/wingman94/designs");
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCcG7wBJZVuM3g7axxkQt1Ig");
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/@MaxK94");
        }
    }
}
