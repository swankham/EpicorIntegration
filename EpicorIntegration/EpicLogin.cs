﻿using Epicor.Mfg.BO;
using Epicor.Mfg.Core;
using System;
using System.Windows.Forms;

namespace Epicor_Integration
{
    public partial class EpicLogin : Form
    {
        public EpicLogin()
        {
            InitializeComponent();
        }

        private void EpicLogin_Load(object sender, EventArgs e)
        {
            try
            {
                Uname.Text = Properties.Settings.Default.uname;

                Passw.Text = Properties.Settings.Default.passw;
            }
            catch { }
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;

	    protected override CreateParams CreateParams
	    {
            get
		    {
			    CreateParams mdiCp = base.CreateParams;
			    mdiCp.ClassStyle = mdiCp.ClassStyle | CP_NOCLOSE_BUTTON;
			    return mdiCp;
		    }
	    }

        private void Cancelbtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Savebtn_Click(object sender, EventArgs e)
        {
            //save settings to resource file then test settings to connect

            Properties.Settings.Default.uname = Uname.Text;

            Properties.Settings.Default.passw = Passw.Text;

            Properties.Settings.Default.Save();

            string server = Properties.Settings.Default .svrname + ":" + Properties.Settings.Default.svrport;

            try
            {

                DataList.EpicConn = new BLConnectionPool(Uname.Text, Passw.Text, "AppServerDC://" + server);

                Part EpicPart = new Part(DataList.EpicConn);
               
                bool ValidLogin = EpicPart.PartExists(null);

                Properties.Settings.Default.validated = true;

                Properties.Settings.Default.Save();

                DataList.EpicClose();

                this.Close();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message,"Try Again");
            }
        }
    }
}
