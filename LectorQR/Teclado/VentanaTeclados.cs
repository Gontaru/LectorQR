using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LectorQR
{
    public partial class VentanaTeclados : Form
    {
        public string valor = "";
        public string TextoTeclado = "";
        public bool clicked = false;
        public TextBoxBase text_box_obj;
        private KeyBoard kb = null;
        private int w, h;
        private List<TextBoxBase> tb_visitados = new List<TextBoxBase>();
        private int num_tb = 0;
        private Form padre;

        public VentanaTeclados(Form p)
        {
            InitializeComponent();
            MaquinaLinea.TecladoAbierto = true;
            this.TopMost = true;
            w = this.Width;
            h = this.Height;
            this.padre = p;

            List<TextBox> aux = new List<TextBox>();
            TextBox tb = new TextBox();
            foreach (Control c in this.padre.Controls)
            {
                if (c.Controls.Count > 0)
                {
                    foreach (Control c2 in c.Controls)
                    {
                        if (c2.GetType().ToString() == "System.Windows.Forms.TextBox")
                        {
                            Console.WriteLine("CONTROL: " + c2.Name.ToString() + " " + c2.TabIndex);
                            tb = c2 as TextBox;
                            if (tb.ReadOnly == false && !aux.Contains(tb))
                            {
                                aux.Add(tb);
                                num_tb++;
                            }
                        }
                        if (c2.Controls.Count > 0)
                        {
                            foreach (Control c3 in c2.Controls)
                            {
                                if (c3.GetType().ToString() == "System.Windows.Forms.TextBox")
                                {
                                    tb = c3 as TextBox;
                                    if (tb.ReadOnly == false && !aux.Contains(tb))
                                    {

                                        aux.Add(tb);
                                        num_tb++;
                                    }
                                }
                                if (c3.Controls.Count > 0)
                                {
                                    foreach (Control c4 in c3.Controls)
                                    {
                                        if (c4.GetType().ToString() == "System.Windows.Forms.TextBox")
                                        {
                                            tb = c4 as TextBox;
                                            if (tb.ReadOnly == false && !aux.Contains(tb))
                                            {

                                                aux.Add(tb);
                                                num_tb++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AbrirCalculadora(Form p, TextBoxBase tb)
        {
            if (!MaquinaLinea.TecladoAbierto) {MaquinaLinea.numberpad2 = new VentanaTeclados(p);}
            else{MaquinaLinea.numberpad2.setPadre(p);}

            MaquinaLinea.numberpad2.setTB(tb);
            MaquinaLinea.numberpad2.activarTimer();
            if (p.Name.Contains("Comentariors")) MaquinaLinea.numberpad2.changekeyboard_Click(p, new EventArgs());
        }
        private void activarTimer()
        {
            this.timer1.Enabled = true;
        }

        private void setPadre(Form p)
        {
            this.padre = p;
        }

        //Método para establecer en el numberpad el TextBox donde tiene que escribir

       /* public void setTB(TextBox tb)
        {
            TecladoTB.UseSystemPasswordChar = (tb.UseSystemPasswordChar) ? true : false;
            this.TecladoTB.Text = "";
            this.TecladoTB.Text = tb.Text;
            text_box_obj = tb;
            text_box_obj.Focus();
            this.Visible = true;
            if(!tb_visitados.Contains(tb)) this.tb_visitados.Add(tb);

        }*/
        public void setTB(TextBoxBase tb)
        {
            this.TecladoTB.Text = "";
            this.TecladoTB.Text = tb.Text;
            text_box_obj = tb;
            text_box_obj.Focus();
            this.Visible = true;
            if (!tb_visitados.Contains(tb)) this.tb_visitados.Add(tb);

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
            MaquinaLinea.TecladoAbierto = false;

            this.Dispose();
            this.Hide();
        }

        private void buttonBORRAR_Click(object sender, EventArgs e)
        {
            if (TecladoTB.Text != "")
            {
                text_box_obj.Text=text_box_obj.Text.Remove(text_box_obj.Text.Length-1);
                TecladoTB.Text=TecladoTB.Text.Remove(TecladoTB.Text.Length - 1);

            }
        }

        private void buttonpoint_Click(object sender, EventArgs e)
        {
            if (MaquinaLinea.ModoTeclado == false)
            {
                text_box_obj.Text += ".";
                TecladoTB.Text += ".";
            }
            if (MaquinaLinea.ModoTeclado == true)
            {
                text_box_obj.Text += ".";
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

        private void Numberpad2_FormClosed(object sender, FormClosedEventArgs e)
        {
            MaquinaLinea.TecladoAbierto = false;
        }

        private void changekeyboard_Click(object sender, EventArgs e)
        {
            //830,484
      //      this.Width = Convert.ToInt32((SystemInformation.VirtualScreen.Width/ SystemInformation.VirtualScreen.Height) *830);
     //       this.Height = Convert.ToInt32((SystemInformation.VirtualScreen.Width / SystemInformation.VirtualScreen.Height) * 520);

            this.groupBox1.Hide();
            this.kb = new KeyBoard(this);
            this.Width = Convert.ToInt32((SystemInformation.VirtualScreen.Width / SystemInformation.VirtualScreen.Height) * (this.kb.Width+10));
            this.Height = Convert.ToInt32((SystemInformation.VirtualScreen.Width / SystemInformation.VirtualScreen.Height) * (this.kb.Height+40));
            kb.setTB(text_box_obj);
            this.Controls.Add(kb);
            this.kb.Show();
        }
        
        private void button10_Click(object sender, EventArgs e)
        {

            if (tb_visitados.Count == num_tb)
            {
                tb_visitados.Clear();
            }

            TextBox tb = new TextBox();
            bool asignado = false;
            foreach (Control c in this.padre.Controls)
            {

                if (c.Controls.Count > 0 && !asignado)
                {
                    foreach (Control c2 in c.Controls)
                    {
                        if (c2.GetType().ToString() == "System.Windows.Forms.TextBox" && !asignado)
                        {
                            tb = c2 as TextBox;
                            if (tb.ReadOnly == false && tb.Visible == true && !tb_visitados.Contains(tb) && tb.Name != text_box_obj.Name)
                            {
                                setTB(tb);
                                asignado = true;
                                break;
                            }
                        }

                        if (c2.Controls.Count > 0 && !asignado)
                        {
                            foreach (Control c3 in c2.Controls)
                            {
                                if (c3.GetType().ToString() == "System.Windows.Forms.TextBox" && !asignado)
                                {
                                    tb = c3 as TextBox;
                                    if (tb.ReadOnly == false && tb.Visible == true && !tb_visitados.Contains(tb) && tb.Name != text_box_obj.Name)
                                    {
                                        setTB(tb);
                                        asignado = true;
                                        break;
                                    }
                                }
                                if (c3.Controls.Count > 0 && !asignado)
                                {
                                    foreach (Control c4 in c3.Controls)
                                    {
                                        if (c4.GetType().ToString() == "System.Windows.Forms.TextBox" && !asignado)
                                        {
                                            tb = c4 as TextBox;
                                            if (tb.ReadOnly == false && tb.Visible == true && !tb_visitados.Contains(tb) && tb.Name != text_box_obj.Name)
                                            {
                                                setTB(tb);
                                                asignado = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }

                }

            }




        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.padre.Visible==false) {
                this.Hide();
                this.timer1.Enabled = false;
            }
        }

        internal void ClosingKeyBoard()
        {
            this.setTB(this.kb.text_box_obj);
            this.kb = null;
            
            this.Width = Convert.ToInt32((SystemInformation.VirtualScreen.Width / SystemInformation.VirtualScreen.Height) * w);
            this.Height = Convert.ToInt32((SystemInformation.VirtualScreen.Width / SystemInformation.VirtualScreen.Height) * h);
            this.groupBox1.Show();
        }
    }
}
