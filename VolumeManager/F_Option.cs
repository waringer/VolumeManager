using System;
using System.Windows.Forms;
using VolumeManager.Properties;

namespace VolumeManager
{
    public partial class F_Option : Form
    {
        public F_Option()
        {
            InitializeComponent();
        }

        private void F_Option_Shown(object sender, EventArgs e)
        {
            CB_Send2UDP.Checked = Settings.Default.Option_SendUDP;
        }

        private void B_OK_Click(object sender, EventArgs e)
        {
            Settings.Default.Option_SendUDP = CB_Send2UDP.Checked;
            Settings.Default.Save();
        }
    }
}

