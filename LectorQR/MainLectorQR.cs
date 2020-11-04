using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        Thread ThreadConexion;
        static List<string> List_Cods = new List<string>();
        static List<string> List_Errs = new List<string>();
        Socket s;

        IPAddress localAddr = IPAddress.Parse("192.168.100.10");
        //TcpListener myList = new TcpListener(IPAddress.Any, 8010);
        public bool Inicio = false;
        private bool copiado_cod_error;
        static List<double> max_tiempo= new List<double>();
        double maxT = 0;
        double contador_sec = 0;
        int num_bot_per_sec = 0, max_num_bot_per_sec=0;

        public MainLectorQR()
        {
            Stopwatch timeMeasure = new Stopwatch();

            InitializeComponent();
            ThreadConexion = new Thread(() =>
            {
                 new PipeClient(this);
            }); ThreadConexion.Start();
           /* for (int i = 0; i < 100; i++)
            {
                timeMeasure = new Stopwatch();
                timeMeasure.Start();
                COD_LEIDO ="https://www2.agenciatributaria.gob.es/wlpl/ADMF-JDIT/V?C=" + ((20030780005) + i) + "&T=+XHyxjyFj3mxx3Tldf6l6A==";
                EscribirTB();
                if(i%2!=0)List_Errs.Add(ExtraerCodigo(AjustarCodPrecinta("https://www2.agenciatributaria.gob.es/wlpl/ADMF-JDIT/V?C=" + ((20030780005) + i) + "&T=+XHyxjyFj3mxx3Tldf6l6A==")));
                COD_LEIDO = "https://www2.agenciatributaria.gob.es/wlpl/ADMF-JDIT/V?C=" +((20030780005)+i+1)+"&T=+XHyxjyFj3mxx3Tldf6l6A==";
                EscribirTB();
                if (!Guardando)
                {
                    Guardando = true;
                    Thread T2 = new Thread(() =>
                    {
                        Guardar();
                    }); T2.Start();
                }
                timeMeasure.Stop();


                if (contador_sec >= 1000)
                {
                    if (max_num_bot_per_sec < num_bot_per_sec)
                    {
                        max_num_bot_per_sec = num_bot_per_sec;
                    }
                    num_bot_per_sec = 0;
                    contador_sec = 0;
                }
                else
                {
                    contador_sec += timeMeasure.Elapsed.TotalMilliseconds;
                    num_bot_per_sec++;

                }
                if (timeMeasure.Elapsed.TotalMilliseconds > maxT)
                {
                    maxT = timeMeasure.Elapsed.TotalMilliseconds;
                }
                if (timeMeasure.Elapsed.TotalMilliseconds > 200)
                {
                    Console.WriteLine("NOS HEMOS PASADO");
                    max_tiempo.Add(timeMeasure.Elapsed.TotalMilliseconds);
                }
                Console.WriteLine("Tiempo: " + timeMeasure.Elapsed.TotalMilliseconds + " ms");
                
            }*/
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

            OrdenTB.ReadOnly = OrdenTB.Text==""? false : true;
            LoteTB.ReadOnly = LoteTB.Text==""? false : true;
            ProductoTB.ReadOnly = ProductoTB.Text==""? false : true;
            GradTB.ReadOnly = GradTB.Text==""? false : true;
            ClienteTB.ReadOnly = ClienteTB.Text==""? false : true;
            CapacidadTB.ReadOnly = CapacidadTB.Text==""? false : true;
        }

        //Función que cierra el programa. Si hemos leído algún código, los guarda en un fichero.
      
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

            switch (COD_LEIDO)
            {
                case "ERROR":
                    List_Cods.Add(COD_LEIDO);
                    Nerror = Ncodigos - Nok;
                    FillRichTB("ERROR");
                    FillErrorTB(Convert.ToString(Nerror));
                    break;

                case "":
                    break;

                default:
                    //Quitamos los caracteres que introduce la cámara al leer (bandera inicio y fin de texto)
                    AjustarCodPrecinta(COD_LEIDO);
                    //Extraemos el código de la precinta en la URL
                    COD_LEIDO = ExtraerCodigo(COD_LEIDO);
                    //Añadimos el código a la lista
                    List_Cods.Add(COD_LEIDO);
                    //Rellenamos TB
                    FillRichTB(COD_LEIDO);
                    //Incrementamos contador
                    Nok += 1;

                    FillOkTB(Convert.ToString(Nok));
                    break;
            }

            
        }
        public void FillRichTB(string value) {

            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillRichTB), new object[] { value });
                return;
            }
            RichTCD_Leido.Text += value + Environment.NewLine;
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
            if (OrdenTB.Text == "" || LoteTB.Text == "" || ProductoTB.Text == "" || ClienteTB.Text == "" || GradTB.Text == "" || CapacidadTB.Text == "")
            {
                MessageBox.Show("Faltan campos por introducir");
                VentanaTeclados.AbrirCalculadora(this, OrdenTB);
            }
            else
           {
                Inicio = (Inicio) ? false : true;
                StartB.Text = (Inicio) ? "Start" : "Pause";
                StartB.BackColor = (Inicio) ? Color.DarkSeaGreen : Color.IndianRed;
                OrdenTB.ReadOnly = Inicio;
                LoteTB.ReadOnly = Inicio;
                ProductoTB.ReadOnly = Inicio;
                GradTB.ReadOnly = Inicio;
                ClienteTB.ReadOnly = Inicio;
                CapacidadTB.ReadOnly = Inicio;
                LeerQR();
            }
        }

        private string AjustarCodPrecinta(string s)
        {
            if (Convert.ToByte(s[0])==2) {
                s = s.Substring(1, s.Length-1); }
            for (int i = 0; i < s.Length; i++)
            {
                if (i + 1 < s.Length)
                    if (s[i] == '=' && s[i + 1] == '=')
                        if (i + 2 < s.Length)
                        {
                            s = s.Substring(0, i + 2);
                        }
            }
            return s;
        }
        public void SalirPrograma()
        {

            if (InvokeRequired)
            {
                this.SalirPrograma();
                return;
            }
            Environment.Exit(0);
        }

        public void MainLectorQR_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThreadConexion.Abort();
            Inicio = false;
            if (Ncodigos > 0) Guardar();
            if(List_Errs.Count>0) GuardarErrores();


            //           System.Windows.Forms.Application.Exit();
            ThreadConexion.Abort();
            if (s != null) s.Close();


            while (Guardando) { }
            ProcesarFicherosErroneos();
            ProcesarFicheros();
            Environment.Exit(0);

        }
        private void ProcesarFicherosErroneos()
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile = "C:/RegistroPrecintas/PrecintasFiscalesErroneas." + OrdenTB.Text + "." + date + ".csv";
            if (File.Exists(@namefile))
            {

                List<string> aux = new List<string>();
                string[] lineas = File.ReadAllLines(namefile);
                List<string> result = new List<string>();

                foreach (string s in List_Errs)
                {
                    if (!result.Contains(s))
                    {
                        result.Add(s);
                    }
                }
                for (int i = 1; i < File.ReadAllLines(namefile).Length; i++)
                {
                    if (!result.Contains(lineas[i]))
                        result.Add(lineas[i]);
                }
                List_Errs.Clear();
                List_Errs = result;
                GuardarErrores();
            }
        }

        private void ProcesarFicheros()
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile = "C:/RegistroPrecintas/PrecintasFiscales." + OrdenTB.Text + "." + date + ".csv";
            if (File.Exists(@namefile))
            {

                List<string> aux = new List<string>();
                string[] lineas = File.ReadAllLines(namefile);               
                List<string> result = new List<string>();

                foreach (string s in List_Cods)
                {
                    if (!result.Contains(s))
                    {
                        result.Add(s);
                    }
                }
                for (int i = 1; i < File.ReadAllLines(namefile).Length; i++)
                {
                    if(!result.Contains(lineas[i]))
                        result.Add(lineas[i]);
                }

                List_Cods.Clear();
                List_Cods = result;
                foreach(string s in List_Errs)
                {
                    if (List_Cods.Contains(s)) List_Cods.Remove(s);
                }
                Guardar();            
            }
        }

        private void Guardar() {
            Guardando = true;
            
            string aux = "";
            string time = DateTime.Now.ToString("hh:mm:ss");
            aux += time + Environment.NewLine;
            for (int i = 0; i < List_Cods.Count; i++)
            {
                if (i == List_Cods.Count - 1) aux += List_Cods[i];
                else aux += List_Cods[i] + Environment.NewLine;
            }

            if(Directory.Exists(@"C:/RegistroPrecintas")==false)Directory.CreateDirectory(@"C:/RegistroPrecintas/");
            
            string date = DateTime.Now.ToString("dd-MM-yyyy");

            string namefile = "C:/RegistroPrecintas/PrecintasFiscales." + OrdenTB.Text + "." + date + ".csv";

            if (File.Exists(@namefile))
            {
                if (Directory.Exists(@"C:/RegistroPrecintas/CopiasRegistrosFiscales") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/CopiasRegistrosFiscales");

                if (File.Exists(@"C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscales." + OrdenTB.Text + "."+ date + ".csv"))
                {
                    File.Delete("C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscales." + OrdenTB.Text + "." + date + ".csv");
                }
                File.Copy(namefile, "C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscales." + OrdenTB.Text + "." + date + ".csv");

                /*string s = File.ReadAllText(namefile);
                s=s.Remove(0,s.IndexOf('\r'));
                File.WriteAllText("temp", aux + s);
                File.Delete(namefile);
                File.Copy("temp", namefile);
                File.Delete("temp");*/

            }
            else
            {
                //File.WriteAllText(namefile, aux);
            }
            File.WriteAllText(namefile, aux);

            Guardando = false;
        }
        private void GuardarErrores()
        {
            Guardando = true;

            string aux = "";
            string time = DateTime.Now.ToString("hh:mm:ss");

            aux += "Ultima escritura: " + time + Environment.NewLine;
            for (int i = 0; i < List_Errs.Count; i++)
            {
                if (i == List_Errs.Count - 1) aux += List_Errs[i];
                else aux += List_Errs[i] + Environment.NewLine;
            }

            if (Directory.Exists(@"C:/RegistroPrecintas") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/");

            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile;
      
            namefile = "C:/RegistroPrecintas/PrecintasFiscalesErroneas" + OrdenTB.Text + date + ".csv";
         
            if (File.Exists(@namefile))
            {
                if (Directory.Exists(@"C:/RegistroPrecintas/CopiasRegistrosFiscales") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/CopiasRegistrosFiscales");

                if (File.Exists(@"C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscalesErroneas" + OrdenTB.Text + date + ".csv"))
                {
                    File.Delete("C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscalesErroneas" + OrdenTB.Text + date + ".csv");
                }
                File.Copy(namefile, "C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscalesErroneas" + OrdenTB.Text + date + ".csv");
               /* string s = File.ReadAllText(namefile);
                s = s.Remove(0, s.IndexOf('\r'));
                File.WriteAllText("temp", aux + s);
                File.Delete(namefile);
                File.Copy("temp", namefile);
                File.Delete("temp");*/
            }
            else
            {//GIT
               // File.WriteAllText(namefile, aux);

            }
            File.WriteAllText(namefile, aux);
            Guardando = false;
        }
        private string ExtraerCodigo(string s)
        {
            string r="";

            if (s.Contains("http"))
            {

                s = s.Remove(0, s.IndexOf('=')+1);

                for (int i = 0; s[i]!='&'; i++)
                {
                    r += s[i];
                }
            }
            return r;
        }

        private void OrdenTB_Click(object sender, EventArgs e)
        {
            if(!OrdenTB.ReadOnly)VentanaTeclados.AbrirCalculadora(this, OrdenTB);
        }

        private void LoteTB_Click(object sender, EventArgs e)
        {

            if (!LoteTB.ReadOnly) VentanaTeclados.AbrirCalculadora(this, LoteTB);
        }

        private void ProductoTB_Click(object sender, EventArgs e)
        {
            if (!ProductoTB.ReadOnly) VentanaTeclados.AbrirCalculadora(this, ProductoTB);

        }

        private void ClienteTB_Click(object sender, EventArgs e)
        {
            if (!ClienteTB.ReadOnly) VentanaTeclados.AbrirCalculadora(this, ClienteTB);

        }


        private void GradTB_Click(object sender, EventArgs e)
        {
            if (!GradTB.ReadOnly) VentanaTeclados.AbrirCalculadora(this, GradTB);


        }

        private void CapacidadTB_Click(object sender, EventArgs e)
        {
            VentanaTeclados.AbrirCalculadora(this, CapacidadTB);


        }
        private String ExtraerCodErronea(string s) {
            string r = "";
            
            s = s.Remove(0, s.IndexOf('¡') + 1);

            for (int i = 0; s[i] != '/'; i++)
            {
                r += s[i];
            }
            return r;
        }
        private void ActualizarCodLeidosRTB() {
            RichTCD_Leido.Text = "";
            foreach(string s in List_Cods)
            {
                RichTCD_Leido.Text += s+Environment.NewLine;


            }
            RichTCD_Leido.Update();
        }

        private void CodigoErroneoTB_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (copiado_cod_error && CodigoErroneoTB.Text != "") CodigoErroneoTB.Text = ""; copiado_cod_error = false;
            if (e.KeyCode == Keys.Enter)
            {
                AjustarCodPrecinta(CodigoErroneoTB.Text);
                string codE = ExtraerCodErronea(CodigoErroneoTB.Text);
                List_Errs.Add(codE);

                RichTCD_Erroneo.Text += codE + Environment.NewLine;

                /*if (List_Cods.Contains(codE))
                {
                    List_Cods.Remove(codE);
                    ActualizarCodLeidosRTB();
                }*/
                   GuardarErrores();
                copiado_cod_error = true;
            }
        }
    }
}