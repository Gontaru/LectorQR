using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace LectorQR
{
    
    public partial class MainLectorQR : Form
    {

        //strings a leer de WHPST
        public string orden;
        public string producto;
        public string cliente;
        public string botellas;
        public string graduacion;
        public string capacidad;
        public string lote;

        bool Guardando = false;
        static string COD_LEIDO="";
        static int Ncodigos = 0;
        static int Nok = 0;
        static int Nerror = 0;
        public string NombreFichero;
        static List<string> List_Cods = new List<string>();
        Socket s;

        IPAddress localAddr = IPAddress.Parse("192.168.100.10");
        //TcpListener myList = new TcpListener(IPAddress.Any, 8010);
        public bool Inicio = false;


        public MainLectorQR()
        {
            InitializeComponent();

            Thread T1 = new Thread(() =>
            {
                new PipeClient(this);
            });T1.Start();
        }
        //Función que asigna el texto leido por el cliente a los TB
        internal void AsignarTB()
        {
            OrdenTB.Text = orden;
            LoteTB.Text = lote;
            ProductoTB.Text = producto;
            ClienteTB.Text = cliente;
            GradTB.Text = graduacion;
            CapacidadTB.Text = capacidad;
            NBotTB.Text = botellas;

            OrdenTB.ReadOnly = true;
            LoteTB.ReadOnly = true;
            ProductoTB.ReadOnly = true;
            GradTB.ReadOnly = true;
            ClienteTB.ReadOnly = true;
            CapacidadTB.ReadOnly = true;
            NBotTB.ReadOnly = true;
        }

        //Función que cierra el programa. Si hemos leído algún código, los guarda en un fichero.
        internal void Cerrar()
        {
            MessageBox.Show("CERRANDO");
            Inicio = false;
            if(Ncodigos>0)Guardar();
            this.Dispose();
            Environment.Exit(0);
        }
        //Función para leer los códigos QR
        public void LeerQR()
        {
            if (Inicio)
            {
                Thread T1 = new Thread(() =>
                {
                    //Si el botón de start ha sido pulsado, no paramos de leer códigos
                    while (Inicio)
                    {
                        panelImagen.BackColor = Color.White;
                        CodigoLeidoTB.BackColor = Color.White;
                        TcpListener myList = new TcpListener(localAddr, 9004);
                        

                        try
                        {
                            myList.Start();

                            s = myList.AcceptSocket();

                            //Guardamos en b los bytes recibidos
                            byte[] b = new byte[128];
                            int k = s.Receive(b);
                            for (int i = 0; i < k; i++)Console.Write(Convert.ToChar(b[i]));
                            if (b[0] == 2 && b[k-1] == 3)
                            {
                                ASCIIEncoding asen = new ASCIIEncoding();
                                s.Send(asen.GetBytes("The string was recieved by the server."));
                                COD_LEIDO = "";
                                //convertimos los bytes a string
                                COD_LEIDO = getString(b);
                                //escribimos en los TB correspondientes el código leído
                                EscribirTB();
                                //si no estamos guardando, realizamos un guardado para no perder los datos
                                if (!Guardando)
                                {
                                    Thread T2 = new Thread(() =>
                                    {
                                      Guardar();
                                    }); T2.Start();
                                }
                            }
                            Console.Read();
                            s.Close();
                            myList.Stop();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Fallo en la conexión con la camara\n ERROR EXCEPCION: "+e.ToString());
                            Guardar();
                            break;
                        }
                    }
                      
                    });
                    T1.Start();

            }
        }

        public String getString(byte[] text)
        {
            System.Text.ASCIIEncoding codificador = new System.Text.ASCIIEncoding();
            return codificador.GetString(text);
        }


        private void EscribirTB() {
            //Incrementamos el nº de códigos leidos
            Ncodigos += 1;

            FillNcodigosTB(Convert.ToString(Ncodigos));

            if (COD_LEIDO.Contains("ERROR")) COD_LEIDO = "ERROR";
            FillCodLeidoTB(COD_LEIDO);
            RichTCD_Leido.Text += COD_LEIDO + Environment.NewLine;

            switch (COD_LEIDO)
            {
                case "ERROR":
                    List_Cods.Add(COD_LEIDO);
                    Nerror = Ncodigos - Nok;
                    FillErrorTB(Convert.ToString(Nerror));
                    break;

                case "":
                    break;

                default:
                    if (!List_Cods.Contains(COD_LEIDO))
                    {
                        AjustarCodPrecinta(COD_LEIDO);
                        List_Cods.Add(ExtraerCodigo(COD_LEIDO));
                    }
                    Nok += 1;
                    FillOkTB(Convert.ToString(Nok));
                    break;
            }

            
        }
        public void FillTB(string value, TextBox TextBox)
        {
            if (InvokeRequired)
            {
            //    this.Invoke(new Action<string>(FillTB), new object[] { value });
                return;
            }
            TextBox.Text = value;

        }
        public void FillNcodigosTB(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillNcodigosTB), new object[] { value });
                return;
            }
            NCodigosTB.Text =value;
            NCodigosTB.Update();
        
        }
        public void FillCodLeidoTB(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillCodLeidoTB), new object[] { value });
                return;
            }
            CodigoLeidoTB.Text = value;
            CodigoLeidoTB.Update();
        }
        public void FillOkTB(string value)
        {
            
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillOkTB), new string[] { value });
                return;
            }
            panelImagen.BackColor = Color.DarkSeaGreen;
            CodigoLeidoTB.BackColor = Color.DarkSeaGreen;
            OkTB.Text = value;
            OkTB.Update();
        }
        public void FillErrorTB(string value)
        {
            
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillErrorTB), new object[] { value });

                return;
            }
            panelImagen.BackColor = Color.IndianRed;
            CodigoLeidoTB.BackColor = Color.IndianRed;
            ErrorTB.Text = value;
            ErrorTB.Update();
        }
        private void ExitB_Click(object sender, EventArgs e)
        {
            if (!Guardando)
            {
                this.Close();
                Application.Exit();
            }
        }

        private void StartB_Click(object sender, EventArgs e)
        {
            if (OrdenTB.Text == "" || LoteTB.Text == "" || ProductoTB.Text == "" || ClienteTB.Text == "" || GradTB.Text == "" || CapacidadTB.Text == "" || NBotTB.Text == "")
            {
                VentanaTeclados.AbrirCalculadora(this, OrdenTB);
            }
            else
           {
                Inicio = (Inicio) ? false : true;
                StartB.Text = (Inicio) ? "Start" : "Pause";
                StartB.BackColor = (Inicio) ? Color.DarkSeaGreen : Color.IndianRed;
                OrdenTB.ReadOnly = true;
                LoteTB.ReadOnly = true;
                ProductoTB.ReadOnly = true;
                GradTB.ReadOnly = true;
                ClienteTB.ReadOnly = true;
                CapacidadTB.ReadOnly = true;
                NBotTB.ReadOnly = true;
                LeerQR();
            }
        }

        private string AjustarCodPrecinta(string s)
        {
            if (Convert.ToByte(s[0])==2) {
                s = s.Substring(1, s.Length-1); }
            for(int i=0; i<s.Length; i++)
            {
                if (i + 1 < s.Length)
                    if (s[i] == '=' && s[i+1] == '=')
                        if(i+2<s.Length) s = s.Substring(0, i+2);
            }
            return s;
        }
        private void MainLectorQR_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(s!=null)s.Close();
        }
        private void Guardar() {
            Guardando = true;
            
            string aux = "";
            for (int i = 0; i < List_Cods.Count; i++)
            {
                aux += List_Cods[i] + Environment.NewLine;
            }

            if(Directory.Exists(@"C:/RegistroPrecintas")==false)Directory.CreateDirectory(@"C:/RegistroPrecintas/");
            
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile = "C:/RegistroPrecintas/PrecintasFiscales" + date + OrdenTB.Text + ".csv";
            if (File.Exists("@"+ namefile))
            {
                if (File.Exists("@" + namefile))
                {
                    File.Delete(namefile);
                }
                File.Copy(namefile, "C:/RegistroPrecintas/COPYPrecintasFiscales" + date + ".csv");
                using (StreamWriter sw = File.AppendText(namefile))
                {
                    sw.Write(aux);
                }
            }
            else
            {
                File.WriteAllText(namefile, aux);
            }
            Guardando = false;
        }

        private string ExtraerCodigo(string s)
        {
            string r="";

            if (s.Contains("http"))
            {
                for (int i=0; s[i] != '='; i++)
                {
                    s = s.Remove(0, i);
                }
                for (int i = 0; s[i]!='&'; i++)
                {
                    r += s[i];
                }
            }
            return r;
        }

        private void OrdenTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, OrdenTB);
        }

        private void LoteTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, LoteTB);
        }

        private void ProductoTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, ProductoTB);

        }

        private void ClienteTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, ClienteTB);

        }

        private void NBotTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, NBotTB);


        }

        private void GradTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, GradTB);


        }

        private void CapacidadTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, CapacidadTB);


        }
    }
}