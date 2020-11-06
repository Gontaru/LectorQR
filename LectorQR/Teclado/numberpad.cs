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

    public partial class numberpad : UserControl
    {
        public string valor = "";
        public string TextoTeclado="";
        public bool clicked=false;
        public TextBox text_box_obj;

        public numberpad()
        {
            InitializeComponent();
           
        }

        //Método para establecer en el numberpad el TextBox donde tiene que escribir

        public void setTB(TextBox tb)
        {
            text_box_obj = tb;
            text_box_obj.Focus();
        }
        private void buttonENTER_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                MaquinaLinea.Teclado = TecladoTB.Text;
               // MessageBox.Show(MaquinaLinea.Teclado);
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                MaquinaLinea.Teclado = TextoTeclado;
                TextoTeclado = "";
                if (MaquinaLinea.Teclado == "1877") MaquinaLinea.Password = true;
                if (MaquinaLinea.Teclado != "1877") MaquinaLinea.Password = false;
            }
            TecladoTB.Text = "";
            MaquinaLinea.StatusTeclado = true;
            this.Dispose();
            this.Hide();
        }

        private void buttonBORRAR_Click(object sender, EventArgs e)
        {
            if (TecladoTB.Text != "")
            {

                text_box_obj.Text = text_box_obj.Text.Substring(0, text_box_obj.Text.Length-1);
                TecladoTB.Text = TecladoTB.Text.Substring(0, TecladoTB.Text.Length - 1);


            }//TecladoTB.Text = TecladoTB.Text.Remove(TecladoTB.Text.Length - 1, 1);
        }

        private void buttonpoint_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                TecladoTB.Text += ".";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                TecladoTB.Text += "*";
                TextoTeclado += ".";
            }
        }

        private void button0_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "0";
                TecladoTB.Text += "0";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                TecladoTB.Text += "*";
                TextoTeclado += "0";
                text_box_obj.Text += "0";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "1";

                TecladoTB.Text += "1";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "1";

                TecladoTB.Text += "*";
                TextoTeclado += "1";
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "2";

                TecladoTB.Text += "2";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "2";

                TecladoTB.Text += "*";
                TextoTeclado += "2";
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "3";

                TecladoTB.Text += "3";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "3";

                TecladoTB.Text += "*";
                TextoTeclado += "3";
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "4";

                TecladoTB.Text += "4";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "4";

                TecladoTB.Text += "*";
                TextoTeclado += "4";
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "5";

                TecladoTB.Text += "5";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "5";

                TecladoTB.Text += "*";
                TextoTeclado += "5";
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "6";

                TecladoTB.Text += "6";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "6";

                TecladoTB.Text += "*";
                TextoTeclado += "6";
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "7";

                TecladoTB.Text += "7";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "7";

                TecladoTB.Text += "*";
                TextoTeclado += "7";
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "8";

                TecladoTB.Text += "8";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "8";

                TecladoTB.Text += "*";
                TextoTeclado += "8";
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += "9";

                TecladoTB.Text += "9";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += "9";

                TecladoTB.Text += "*";
                TextoTeclado += "9";
            }
        }
        public void EscribirTeclado()
        {
            TecladoTB.Text = "";
            MaquinaLinea.StatusTeclado = true;
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            //clicked = true;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            //if (clicked)
            //{
            //    Point P = new Point();
            //    P.X = e.X + panel1.Left - (panel1.Width / 2);
            //    P.Y = e.Y + panel1.Top - (panel1.Width / 2);
            //    numberpad.po = P.X;
            //    panel1.Top = P.Y;
            //}
       }

        private void numberpad_Load(object sender, EventArgs e)
        {
            if (MaquinaLinea.TipoTeclado == 0) buttonpoint.Visible = false;
            if (MaquinaLinea.TipoTeclado == 1) buttonpoint.Visible = true;

        }

      
    }
}
