using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LectorQR
{
    public partial class KeyBoard : UserControl
    {
        public TextBoxBase text_box_obj;
        private VentanaTeclados parent;
        public KeyBoard()
        {
            InitializeComponent();
        }

        public KeyBoard(VentanaTeclados p)
        {
            this.parent = p;
            InitializeComponent();
        }
        public void setTB(TextBox tb)
        {
            this.TecladoTB.Text = "";
            this.TecladoTB.Text = tb.Text;
            text_box_obj = tb;
            text_box_obj.Focus();
            this.Visible = true;

        }
        public void setTB(TextBoxBase tb)
        {
            this.TecladoTB.Text = "";
            this.TecladoTB.Text = tb.Text;
            text_box_obj = tb;
            text_box_obj.Focus();
            this.Visible = true;

        }
        private void Q_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.Q.Text;
            TecladoTB.Text += this.Q.Text;


        }

        private void O_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.O.Text;
            TecladoTB.Text += this.O.Text;

        }

        private void P_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.P.Text;
            TecladoTB.Text += this.P.Text;

        }

        private void I_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.I.Text;
            TecladoTB.Text += this.I.Text;

        }

        private void U_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.U.Text;
            TecladoTB.Text += this.U.Text;

        }

        private void Y_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.Y.Text;
            TecladoTB.Text += this.Y.Text;

        }

        private void T_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.T.Text;
            TecladoTB.Text += this.T.Text;

        }

        private void R_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.R.Text;
            TecladoTB.Text += this.R.Text;

        }

        private void E_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.E.Text;
            TecladoTB.Text += this.E.Text;

        }

        private void W_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.W.Text;
            TecladoTB.Text += this.W.Text;

        }

        private void A_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.A.Text;
            TecladoTB.Text += this.A.Text;

        }

        private void S_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.S.Text;
            TecladoTB.Text += this.S.Text;

        }

        private void D_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.D.Text;
            TecladoTB.Text += this.D.Text;

        }

        private void F_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.F.Text;
            TecladoTB.Text += this.F.Text;

        }

        private void G_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.G.Text;
            TecladoTB.Text += this.G.Text;

        }

        private void H_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.H.Text;
            TecladoTB.Text += this.H.Text;

        }

        private void J_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.J.Text;
            TecladoTB.Text += this.J.Text;

        }

        private void K_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.K.Text;
            TecladoTB.Text += this.K.Text;

        }

        private void L_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.L.Text;
            TecladoTB.Text += this.L.Text;

        }

        private void Ñ_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.Ñ.Text;
            TecladoTB.Text += this.Ñ.Text;

        }

        private void Z_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.Z.Text;
            TecladoTB.Text += this.Z.Text;

        }

        private void X_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.X.Text;
            TecladoTB.Text += this.X.Text;

        }

        private void C_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.C.Text;
            TecladoTB.Text += this.C.Text;

        }

        private void V_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.V.Text;
            TecladoTB.Text += this.V.Text;

        }

        private void Bm_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.Bm.Text;
            TecladoTB.Text += this.Bm.Text;

        }

        private void N_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.N.Text;
            TecladoTB.Text += this.N.Text;

        }

        private void M_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += this.M.Text;
            TecladoTB.Text += this.M.Text;

        }

        private void comma_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += ",";
            TecladoTB.Text += ",";

        }

        private void point_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += ".";
            TecladoTB.Text += ".";

        }

        private void rod_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += "/";
            TecladoTB.Text += "/";

        }

        private void hyphen_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += "-";
            TecladoTB.Text += "-";

        }

        private void delete_Click(object sender, EventArgs e)
        {
            if (text_box_obj.Text != "") text_box_obj.Text = text_box_obj.Text.Substring(0, text_box_obj.Text.Length - 1);
            if (TecladoTB.Text != "") TecladoTB.Text=TecladoTB.Text.Substring(0, TecladoTB.Text.Length - 1);

        }

        private void space_Click(object sender, EventArgs e)
        {
            text_box_obj.Text += " ";
            TecladoTB.Text += " ";

        }

        private void shift_Click(object sender, EventArgs e)
        {
            if (this.A.Text == "A")
            {
                this.A.Text = "a";
                this.Bm.Text = "b";
                this.C.Text = "c";
                this.E.Text = "e";
                this.D.Text = "d";
                this.F.Text = "f";
                this.G.Text = "g";
                this.H.Text = "h";
                this.I.Text = "i";
                this.J.Text = "j";
                this.K.Text = "k";
                this.L.Text = "l";
                this.M.Text = "m";
                this.N.Text = "n";
                this.Ñ.Text = "ñ";
                this.O.Text = "o";
                this.P.Text = "p";
                this.Q.Text = "q";
                this.R.Text = "r";
                this.S.Text = "s";
                this.T.Text = "t";
                this.U.Text = "u";
                this.V.Text = "v";
                this.W.Text = "w";
                this.X.Text = "x";
                this.Y.Text = "y";
                this.Z.Text = "z";
            }
            else {
                this.A.Text = "A";
                this.Bm.Text = "B";
                this.C.Text = "C";
                this.E.Text = "D";
                this.D.Text = "E";
                this.F.Text = "F";
                this.G.Text = "G";
                this.H.Text = "H";
                this.I.Text = "I";
                this.J.Text = "J";
                this.K.Text = "K";
                this.L.Text = "L";
                this.M.Text = "M";
                this.N.Text = "N";
                this.Ñ.Text = "Ñ";
                this.O.Text = "O";
                this.P.Text = "P";
                this.Q.Text = "Q";
                this.R.Text = "R";
                this.S.Text = "S";
                this.T.Text = "T";
                this.U.Text = "U";
                this.V.Text = "V";
                this.W.Text = "W";
                this.X.Text = "X";
                this.Y.Text = "Y";
                this.Z.Text = "Z";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Hide();
            this.parent.ClosingKeyBoard();
        }
    }
}
