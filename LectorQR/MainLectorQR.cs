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

//15-12-2020

//Leemos todas las precintas Pi de un taco T (0<=i<=500)
//Leemos todos los tacos Ti (0<=i<=TacosaProducir)
//Comprobamos los códigos leídos una vez terminado cada taco
//Si aparece una precinta perdida del taco 1 y estamos en el taco 10, comprobamos en la lista de perdidas.


namespace LectorQR
{

    public partial class MainLectorQR : Form
    {
        public struct primerCod_and_incremento
        {
            public long primer_cod;
            public int tipo_incremento;

            public primerCod_and_incremento(long pc, int ti)
            {
                primer_cod = pc;
                tipo_incremento = ti;
            }
        }

        //public List<SortedSet<int>> matriz_precintas = new List<SortedSet<int>>();
        //Matriz dónde almacenaremos los códigos de las precintas, cada fila de la matriz es un taco distinto
        public List<HashSet<long>> matriz_precintas = new List<HashSet<long>>();
        //Lista con los primeros códigos de cada taco (en cada taco hay 500 códigos) y el incremento que tiene
        public List<primerCod_and_incremento> primeros_cods_tacos = new List<primerCod_and_incremento>();
        //Semáforo para los threads, asegura que un thread no intente guardar si otro lo está haciendo
        static Semaphore se_guardado = new Semaphore(1, 1);
        //Semáforo para el acceso a la cámara
        static Semaphore se_escritura = new Semaphore(1, 1);

        //strings a leer de WHPST
        public string orden;
        public string producto;
        public string cliente;
        public string botellas;
        public string graduacion;
        public string capacidad;
        public string lote;

        //booleano para controlar si estamos guardando
        bool Guardando = false;

        //código obtenido tras la lectura
        static string COD_LEIDO = "";

        //Conteo de códigos
        static int Ncodigos = 0;
        static int Nok = 0;
        static int Nerror = 0;

        //nombre del fichero dónde guardaremos, depende los 
        public string NombreFichero;
        Thread ThreadConexion;
        //Listas dónde almacenamos los códigos leídos y los códigos erroneos
        static List<string> List_Cods = new List<string>();
        static List<string> List_Errs = new List<string>();
        static List<string> List_Perdidos = new List<string>();
        static List<string> Cods_Taco_Actual = new List<string>();
        //Lista con los primeros códigos de cada taco para poder realizar comprobaciones

        static string IP = "192.168.100.10";
        static int port = 9004;
        Socket s;

        //Ip del controlador ethernet del ordenador 
        static IPAddress localAddr = IPAddress.Parse(IP);
        TcpListener myList = new TcpListener(localAddr, port);


        private bool comprobacion_tacos = false;
        public bool Inicio = false;
        private bool copiado_cod_error;
        static List<double> max_tiempo = new List<double>();

        //VARIABLES PARA EL TESTEO DE LECTURA
        double maxT = 0, contador_sec = 0;
        int num_bot_per_sec = 0, max_num_bot_per_sec = 0;
        private bool copiado_taco;

        public MainLectorQR()
        {

            InitializeComponent();
            //CONEXION CON LA APP WHPST
            ThreadConexion = new Thread(() => { new PipeClient(this); });
            ThreadConexion.Start();
            contraseniaTB.PasswordChar ='*';
            /*
            //FUNCION QUE PERMITE REALIZAR UNA PRUEBA
            comprobacion_tacos = true;
            precintas_RTB.View = View.List;
         
            precintas_RTB.Items.Add("20030780005"+Environment.NewLine);
            precintas_RTB.Items.Add("20030888805" + Environment.NewLine);
            matriz_precintas.Add(new HashSet<long>());
            matriz_precintas.Add(new HashSet<long>());
            primeros_cods_tacos.Add( new primerCod_and_incremento(20030780005,1));
            primeros_cods_tacos.Add(new primerCod_and_incremento(20030888805, 1));

            matriz_precintas.Add(new HashSet<long>());
            TesteoLectura();
            Thread.Sleep(100);
            ProcesarFicheros();*/
           
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
                NuevaOrdenB.Enabled = (Inicio) ? false : true;
                OrdenTB.ReadOnly = Inicio;
                LoteTB.ReadOnly = Inicio;
                ProductoTB.ReadOnly = Inicio;
                GradTB.ReadOnly = Inicio;
                ClienteTB.ReadOnly = Inicio;
                CapacidadTB.ReadOnly = Inicio;
                if (!Inicio)
                {

                    if (s != null) s.Close();
                    if (Ncodigos > 0) Guardar();
                    if (List_Errs.Count > 0) GuardarEliminadas();
                    if (List_Errs.Count > 0) ProcesarFicherosErroneos();
                    if (Ncodigos > 0) ProcesarFicheros();
                }
                LeerQR();
            }
        }

        //Función para leer los códigos QR
        public void LeerQR()
        {
            if (Inicio)
            {

                //Si el botón de start ha sido pulsado, no paramos de leer códigos
                Thread main = new Thread(() =>
                {
                    //for(int i = 0; i<Dns.GetHostEntry(Dns.GetHostName()).AddressList.Length; i++)
                    //{ Console.WriteLine(Dns.GetHostEntry(Dns.GetHostName()).AddressList[i]); }
                 
                    while (Inicio)
                    {
                      
                            myList.Start();

                            try
                            {
                                se_escritura.WaitOne();
                                s = myList.AcceptSocket();

                                //Guardamos en b los bytes recibidos
                                byte[] b = new byte[128];
                                int k = s.Receive(b);
                                for (int i = 0; i < k; i++) Console.Write(Convert.ToChar(b[i]));
                                if ((b[0] == 2 && b[k - 1] == 3) || (b[0] == 33 && b[k - 1] == 33))
                                {
                                    //ASCIIEncoding asen = new ASCIIEncoding();
                                    COD_LEIDO = "";
                                    //convertimos los bytes a string
                                    COD_LEIDO = getString(b);
                                    COD_LEIDO = COD_LEIDO.Remove(k - 1, COD_LEIDO.Length - k + 1);

                                    //Escribimos en el RichTB 
                                    EscribirTB();

                                    //Si está activada la comprobación de tacos entramos aqui
                                    if (comprobacion_tacos)

                                    {
                                        long precinta_leida = Convert.ToInt64(AjustarCodPrecinta(COD_LEIDO));
                                        for (int i = 0; i < primeros_cods_tacos.Count; i++)
                                        {
                                            //Comprobamos a qué taco pertenece el código leído
                                            if ((primeros_cods_tacos[i].primer_cod <= precinta_leida) &&
                                            precinta_leida <= (primeros_cods_tacos[i].primer_cod + (primeros_cods_tacos[i].tipo_incremento * 500)))
                                            {
                                                //Si el código no pertenece al set correspondiente, lo añadimos                        
                                                if (!matriz_precintas[i].Contains(precinta_leida))
                                                {
                                                    //Console.WriteLine("Fila : " + i + " cod:" + COD_LEIDO);
                                                    matriz_precintas[i].Add(precinta_leida);
                                                    if (matriz_precintas[i].Count == 500) ;
                                                    break;
                                                }

                                            }
                                        }

                                    }

                                }
                                se_escritura.Release();

                                //si no estamos guardando, realizamos un guardado para no perder los datos                              
                                if (!Guardando)
                                {
                                    se_guardado.WaitOne();
                                    Guardar();
                                    se_guardado.Release();
                                }
                                Console.Read();
                                s.Close();
                                myList.Stop();



                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("Fallo en la conexión con la camara");
                                Guardar();
                            }
                        }
                    

                }); main.Start();
            }
        }



        //-------------------------- FUNCIONES LEERQR ----------------------
        //CONVERTIDOR DEL ARRAY APORTADO POR LA CAMARA A STRING
        public String getString(byte[] text)
        {
            System.Text.ASCIIEncoding codificador = new System.Text.ASCIIEncoding();
            return codificador.GetString(text);
        }
        private string ExtraerCodigo(string s)
        {
            string r = "";

            if (s.Contains("http"))
            {

                s = s.Remove(0, s.IndexOf('=') + 1);

                for (int i = 0; s[i] != '&'; i++)
                {
                    r += s[i];
                }
            }
            return r;
        }
        private String ExtraerCodErronea(string s)
        {
            string r = "";

            s = s.Remove(0, s.IndexOf('¡') + 1);

            for (int i = 0; i < s.Length - 1 && s[i] != '/'; i++)
            {
                r += s[i];
            }
            return r;
        }
        private string AjustarCodPrecinta(string s)
        {
            if (Convert.ToByte(s[0]) == 2)
            {
                s = s.Substring(1, s.Length - 1);
            }
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


        //Rellena los TB con el COD_LEIDO
        private void EscribirTB()
        {
            //Incrementamos el nº de códigos leidos

            Ncodigos += 1;

            FillNcodigosTB(Convert.ToString(Ncodigos));

            FillCodLeidoTB(COD_LEIDO);
            COD_LEIDO = (COD_LEIDO.Contains("ERROR")) ? "ERROR" : COD_LEIDO;
            switch (COD_LEIDO)
            {
                case "ERROR":
                    Nerror = Ncodigos - Nok;
                    List_Cods.Add(COD_LEIDO + " " + Nerror);

                    FillRichTB(COD_LEIDO + " " + Nerror);
                    FillErrorTB(Convert.ToString(Nerror));
                    break;

                case "":
                    break;

                default:

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

        //Guardamos en un fichero los códigos leídos y el número de errores obtenidos
        private void Guardar()
        {
            Guardando = true;
            string aux = "";
            string time = DateTime.Now.ToString("hh:mm:ss");
            aux += "Datos del pedido; Orden: " + OrdenTB.Text + " Lote: " + LoteTB.Text + " Producto: " + ProductoTB.Text + " Cliente: " + ClienteTB.Text + " Graduacion: " + GradTB.Text + " Capacidad: " + CapacidadTB.Text + Environment.NewLine;
            aux += "Precintas buenas leidas: " + (List_Cods.Count - Nerror)+ " Numero de errores: " + Nerror + " " + time + Environment.NewLine;
            for (int i = 0; i < List_Cods.Count; i++)
            {
                if (i == List_Cods.Count - 1) aux += List_Cods[i];
                else
                {
                    aux += List_Cods[i] + Environment.NewLine;
                }
            }


            //Si no hay directorio raiz dónde guardamos los registros de las precintas(es decir, no se encuentra ningún registro), creamos directorio
            if (Directory.Exists(@"C:/RegistroPrecintas") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/");
            if (Directory.Exists(@"//10.10.10.11/")) if (!Directory.Exists(@"//10.10.10.11/compartidas/WHPST/RegistroPrecintas")) Directory.CreateDirectory(@"//10.10.10.11/compartidas/WHPST/RegistroPrecintas/");
            string date = DateTime.Now.ToString("dd-MM-yyyy");

            //creamos un subdirectorio para el registro actual
            string subdirectorio = "C:/RegistroPrecintas/" + ProductoTB.Text + "." + LoteTB.Text + "." + OrdenTB.Text + date + "/";
            string subdirectorioRED = "//10.10.10.11/compartidas/WHPST/RegistroPrecintas/" + ProductoTB.Text + "." + LoteTB.Text + "." + OrdenTB.Text + date + "/";
            if (!Directory.Exists(subdirectorio)) Directory.CreateDirectory (subdirectorio);
            if (Directory.Exists(@"//10.10.10.11/")) if (!Directory.Exists(subdirectorioRED)) Directory.CreateDirectory (subdirectorioRED);

            //nombre de los ficheros
            string namefile = "PrecintasFiscales." + ProductoTB.Text + "." + LoteTB.Text + "." + OrdenTB.Text + "." + date + ".csv";
            string copyfile = "COPYPrecintasFiscales." + ProductoTB.Text + "." + LoteTB.Text + "." + OrdenTB.Text + "." + date + ".csv";

            if (File.Exists(@subdirectorio+namefile))
            {

                if (File.Exists(@subdirectorio+copyfile))
                {
                    File.Delete(subdirectorio+copyfile);
                }
                File.Copy(subdirectorio+namefile, subdirectorio+copyfile);
                File.Delete(subdirectorio + namefile);

            }
            if (File.Exists(subdirectorioRED + namefile))
            {
                if (File.Exists(@subdirectorioRED + copyfile))
                {
                    File.Delete(subdirectorioRED + copyfile);
                }
                File.Copy(subdirectorioRED + namefile, subdirectorioRED + copyfile);
                File.Delete(subdirectorioRED + namefile);


            }
            File.WriteAllText(subdirectorio+namefile, aux);
            if (Directory.Exists(@"//10.10.10.11/")) File.WriteAllText(subdirectorioRED+namefile, aux);

            Guardando = false;
        }
        //Guardamos en un fichero los códigos de las precintas eliminadas con la pistola
        private void GuardarEliminadas()
        {
            Guardando = true;

            string aux = "";
            string time = DateTime.Now.ToString("hh:mm:ss");
            aux += "Datos del pedido; Orden: " + OrdenTB.Text + " Lote: " + LoteTB.Text + " Producto: " + ProductoTB.Text + " Cliente: " + ClienteTB.Text + " Graduacion: " + GradTB.Text + " Capacidad: " + CapacidadTB.Text + Environment.NewLine;

            aux += "Ultima escritura: " + time + Environment.NewLine;
            for (int i = 0; i < List_Errs.Count; i++)
            {
                if (i == List_Errs.Count - 1) aux += List_Errs[i];
                else aux += List_Errs[i] + Environment.NewLine;
            }
            //Si no hay directorio raiz dónde guardamos los registros de las precintas(es decir, no se encuentra ningún registro), creamos directorio
            if (Directory.Exists(@"C:/RegistroPrecintas") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/");
            if (Directory.Exists(@"//10.10.10.11/compartidas/WHPST/RegistroPrecintas") == false) Directory.CreateDirectory(@"//10.10.10.11/compartidas/WHPST/RegistroPrecintas/");


            string date = DateTime.Now.ToString("dd-MM-yyyy");
            //creamos un subdirectorio para el registro actual
            string subdirectorio = "C:/RegistroPrecintas/"+ ProductoTB.Text + "." + OrdenTB.Text + date + "/";
            string subdirectorioRED = "//10.10.10.11/compartidas/WHPST/RegistroPrecintas/" + ProductoTB.Text + "." + OrdenTB.Text + date + "/";

            if (!Directory.Exists(subdirectorio)) Directory.CreateDirectory(subdirectorio);
            if (!Directory.Exists(subdirectorioRED)) Directory.CreateDirectory(subdirectorioRED);

            //nombre de los ficheros
            string namefile = "PrecintasFiscalesErroneas"+ ProductoTB.Text + "." + OrdenTB.Text + date + ".csv";
            string copyfile = "COPYPrecintasFiscalesErroneas" + ProductoTB.Text + "." + OrdenTB.Text + date + ".csv";

            if (File.Exists(@subdirectorio+namefile))
            {

                if (File.Exists(@subdirectorio + copyfile))
                {
                    File.Delete(subdirectorio + copyfile);
                }
                File.Copy(subdirectorio + namefile, subdirectorio + copyfile);
                File.Delete(subdirectorio + namefile);

            }

            if (File.Exists(@subdirectorioRED + namefile))
            {

                if (File.Exists(@subdirectorioRED + copyfile))
                {
                    File.Delete(subdirectorioRED + copyfile);
                }
                File.Copy(subdirectorioRED + namefile, subdirectorioRED + copyfile);
                File.Delete(subdirectorioRED + namefile);

            }
            File.WriteAllText(subdirectorio+namefile, aux);
            File.WriteAllText(subdirectorioRED + namefile, aux);

            Guardando = false;
        }

        //Eliminamos del fichero los códigos duplicados
        private void ProcesarFicherosErroneos()
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile = "PrecintasFiscales." + ProductoTB.Text + "." + LoteTB.Text + "." + OrdenTB.Text + "." + date + ".csv";
            string subdirectorio = "C:/RegistroPrecintas/" + ProductoTB.Text + "." + LoteTB.Text + "." + OrdenTB.Text + date + "/";

            if (File.Exists(@subdirectorio+namefile))
            {

                List<string> aux = new List<string>();
                string[] lineas = File.ReadAllLines(subdirectorio + namefile);
                List<string> result = new List<string>();

                foreach (string s in List_Errs)
                {
                    if (!result.Contains(s))
                    {
                        result.Add(s);
                    }
                }
                for (int i = 1; i < File.ReadAllLines(subdirectorio + namefile).Length; i++)
                {
                    if (!result.Contains(lineas[i]))
                        result.Add(lineas[i]);
                }
                List_Errs.Clear();
                List_Errs = result;
                GuardarEliminadas();
            }
        }
        private void ProcesarFicheros()
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile = "PrecintasFiscales." + ProductoTB.Text + "." + LoteTB.Text +"." + OrdenTB.Text + "." + date + ".csv";
            string subdirectorio = "C:/RegistroPrecintas/" + ProductoTB.Text + "." + LoteTB.Text+ "." + OrdenTB.Text + date + "/";

            //Si el fichero existe lo abrimos
            if (File.Exists(@subdirectorio+namefile))
            {
                //Creamos lista de string auxiliar
                List<string> aux = new List<string>();
                //Array de string que almacena los datos leidos del fichero
                string[] lineas = File.ReadAllLines(subdirectorio + namefile);
                //Lista de string con el resultado de unir los strings de List_Cods y el fichero
                List<string> result = new List<string>();

                //Añadimos los códigos leídos a result, comprobando de no añadir duplicados.
                foreach (string s in List_Cods)
                {
                    if (!result.Contains(s))
                    {
                        result.Add(s);
                    }
                }

                //Comprobamos que en la lista result no falte algún código que esté en el fichero
                for (int i = 1; i < File.ReadAllLines(subdirectorio + namefile).Length; i++)
                {
                    if (!result.Contains(lineas[i]))
                        result.Add(lineas[i]);
                }
                
                List_Cods.Clear();
                List_Cods = result;

                foreach (string s in List_Errs)
                {
                    if (List_Cods.Contains(s)) List_Cods.Remove(s);
                }
                Guardar();
            }
        }


        //--------------- ESCRITURA EN LOS TEXTBOX DESDE THREAD ----------------
        public void FillRichTB(string value)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillRichTB), new object[] { value });
                return;
            }
            RichTCD_Leido.Text += value + Environment.NewLine;
            RichTCD_Leido.SelectionStart = RichTCD_Leido.Text.Length;
            RichTCD_Leido.ScrollToCaret();
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
            NCodigosTB.Text = value;
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
            CodigoLeidoTB.BackColor = Color.Maroon;
            ErrorTB.Text = value;
            ErrorTB.Update();
        }
        public void FillPipeClientLB(string value)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(FillPipeClientLB), new object[] { value });

                return;
            }

            PipeClientLB.Text = value;
            PipeClientLB.Update();
        }
        private void ActualizarCodLeidosRTB()
        {
            RichTCD_Leido.Text = "";
            foreach (string s in List_Cods)
            {
                RichTCD_Leido.Text += s + Environment.NewLine;
            }
            RichTCD_Leido.Update();
        }

        //---------------------- LECTURA DE LA PISTOLA ---------------------
        private void CodigoErroneoTB_KeyDown(object sender, KeyEventArgs e)
        {


            if (copiado_cod_error && CodigoErroneoTB.Text != "") CodigoErroneoTB.Text = ""; copiado_cod_error = false;
            if (e.KeyCode == Keys.Enter)
            {
                AjustarCodPrecinta(CodigoErroneoTB.Text);
                string codE = ExtraerCodErronea(CodigoErroneoTB.Text);

                List_Errs.Add(codE);

                RichTCD_Erroneo.Text += codE + Environment.NewLine;
                RichTCD_Erroneo.ScrollToCaret();
                CodigoErroneoTB.Text = codE + Environment.NewLine;
                GuardarEliminadas();
                copiado_cod_error = true;

            }
        }

        //----------------------- REGISTRO DE CAMPOS DE INFORMACION ------------------
        //Función que asigna el texto leido por el cliente a los TB
        internal void AsignarTB()
        {
            OrdenTB.Text = orden;
            LoteTB.Text = lote;
            ProductoTB.Text = producto;
            ClienteTB.Text = cliente;
            GradTB.Text = graduacion;
            CapacidadTB.Text = capacidad;

            OrdenTB.ReadOnly = OrdenTB.Text == "" ? false : true;
            LoteTB.ReadOnly = LoteTB.Text == "" ? false : true;
            ProductoTB.ReadOnly = ProductoTB.Text == "" ? false : true;
            GradTB.ReadOnly = GradTB.Text == "" ? false : true;
            ClienteTB.ReadOnly = ClienteTB.Text == "" ? false : true;
            CapacidadTB.ReadOnly = CapacidadTB.Text == "" ? false : true;
        }
        //------------------- TextBox ----------------
        private void OrdenTB_Click(object sender, EventArgs e)
        {
            if (!OrdenTB.ReadOnly) VentanaTeclados.AbrirCalculadora(this, OrdenTB);
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

        //----------------------- CERRAR ------------------
        public void MainLectorQR_FormClosing(object sender, FormClosingEventArgs e)
        {


            Inicio = false;
            if (Ncodigos > 0) Guardar();
            if (List_Errs.Count > 0) GuardarEliminadas();


            //           System.Windows.Forms.Application.Exit();
            if (s != null) s.Close();
            if (List_Errs.Count > 0) ProcesarFicherosErroneos();
            if (Ncodigos > 0) ProcesarFicheros();
            Environment.Exit(0);


        }
        // Activamos la comprobación por tacos
        private void comprobacion_tacos_button_Click(object sender, EventArgs e)
        {
            comprobacion_tacos_button.BackColor = (comprobacion_tacos_button.BackColor == Color.White) ? Color.DarkSeaGreen : Color.White;
            comprobacion_tacos = (comprobacion_tacos) ? false : true;

        }
        private void registrar_taco_Click(object sender, EventArgs e)
        {
            //habilitamos el panel para registrar el tipo de incremento de las precintas(+10,-10,+1,-1,...)
            incremento_precintas_panel.Visible = (incremento_precintas_panel.Visible) ? false : true;
            registrar_taco.BackColor = (registrar_taco.BackColor == Color.White) ? Color.SeaGreen : Color.White;

        }

        private void taco_introducidoTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (copiado_taco && taco_introducidoTB.Text != "") taco_introducidoTB.Text = ""; copiado_taco = false;
            if (e.KeyCode == Keys.Enter)
            {

                AjustarCodPrecinta(taco_introducidoTB.Text);
                string aux_taco = ExtraerCodErronea(taco_introducidoTB.Text);

                //añadimos el código introducido a la lista de los primeros códigos de cada taco

                // checkedListBox1.Items.Add(aux_taco);
                precintas_RTB.Text += aux_taco + Environment.NewLine;

                if (incremento_uno.BackColor == Color.DarkSeaGreen)
                {
                    primeros_cods_tacos.Add(new primerCod_and_incremento(Convert.ToInt64(aux_taco), 1));
                    matriz_precintas.Add(new HashSet<long>());
                }
                else if (incremento_diez.BackColor == Color.DarkSeaGreen)
                {
                    primeros_cods_tacos.Add(new primerCod_and_incremento(Convert.ToInt64(aux_taco), 10));
                    matriz_precintas.Add(new HashSet<long>());
                }
                else
                {
                    MessageBox.Show("Debe introducir el tipo de incremento");
                    // checkedListBox1.Items.Remove(aux_taco);
                    precintas_RTB.Text.Remove(precintas_RTB.Text.Last());
                }

                copiado_taco = true;
                aux_taco = "";

            }


        }

        private void ExitB_Click(object sender, EventArgs e)
        {
            if (!Guardando)
            {
                MainLectorQR_FormClosing(this, new FormClosingEventArgs(CloseReason.UserClosing, false));
            }
        }

        private void TituloLB_Click(object sender, EventArgs e)
        {
            LoteTB.Text = "Prueba";
            GradTB.Text = "Prueba";
            ClienteTB.Text = "Prueba";
            OrdenTB.Text = "Prueba";
            CapacidadTB.Text = "Prueba";
            ProductoTB.Text = "Prueba";
        }

        private void incremento_diez_Click(object sender, EventArgs e)
        {
            incremento_uno.BackColor = Color.White;
            incremento_diez.BackColor = (incremento_diez.BackColor == Color.White) ? Color.DarkSeaGreen : Color.White;
            taco_introducidoTB.Select();

        }

        private void incremento_uno_Click(object sender, EventArgs e)
        {

            incremento_diez.BackColor = Color.White;
            incremento_uno.BackColor = (incremento_uno.BackColor == Color.White) ? Color.DarkSeaGreen : Color.White;
            taco_introducidoTB.Select();

        }

        private void configuracionB_Click(object sender, EventArgs e)
        {
            
                ipTB.Text = IP;
                portTB.Text = Convert.ToString(port);
                panel_red.Visible = (panel_red.Visible) ? false : true;
                contraseniaTB.Text = "";


        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel_red.Visible = false;
            bool haycambios = false;

            if (contraseniaTB.Text=="1PP0RT") {
                if (ipTB.Text != IP)
                {
                    IP = ipTB.Text;
                    localAddr = IPAddress.Parse(IP);
                    haycambios = true;
                }
                if (Convert.ToInt32(portTB.Text) != port)
                {
                    port = Convert.ToInt32(portTB.Text);
                    haycambios = true;
                }

                if (haycambios) myList = new TcpListener(localAddr, port);
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta");
            }
        }

        private void precintas_RTB_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < precintas_RTB.SelectedItems.Count; i++)
            {
                if (precintas_RTB.SelectedItems[i].ForeColor == Color.DarkSeaGreen)
                {
                    MessageBox.Show("Taco Completado");

                }
                else
                {
                    int faltan = 0;
                    string cuerpo = "";

                    for (int j = 0; j < primeros_cods_tacos.Count; j++)
                    {
                        if ((precintas_RTB.SelectedItems[0].Text.Contains(Convert.ToString(primeros_cods_tacos[j].primer_cod))))
                        {

                            for (long codigo = primeros_cods_tacos[j].primer_cod;
                                codigo < primeros_cods_tacos[j].primer_cod + (500 * primeros_cods_tacos[j].tipo_incremento); codigo += primeros_cods_tacos[j].tipo_incremento)
                            {
                                if (!matriz_precintas[j].Contains(codigo))
                                {
                                    faltan++;
                                    cuerpo += codigo + Environment.NewLine;
                                }
                            }
                            Font fuente = new Font(FontFamily.GenericSansSerif, 12.0F);
                            Label resumen = new Label();
                            resumen.Font = fuente;
                            resumen.Text = "Faltan " + faltan + " códigos: " + Environment.NewLine;


                            RichTextBox ts = new RichTextBox();
                            Form form = new Form();
                            ts.Font = fuente;
                            ts.Text = cuerpo;

                            form.Controls.Add(resumen);
                            form.Controls.Add(ts);

                            resumen.Dock = DockStyle.Top;
                            ts.Dock = DockStyle.Fill;
                            ts.Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold);
                            form.Show();

                            //MessageBox.Show(ts.Text);
                            break;
                        }
                    }

                }

            }
        }

        private void NuevaOrdenB_Click(object sender, EventArgs e)
        {
            if (!Inicio)
            {
                DialogResult dialogResult = MessageBox.Show("Se van a borrar todos los datos del registro actual ¿Desea continuar?", "Borrado registro", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    CodigoErroneoTB.Text = "";
                    CodigoLeidoTB.Text = "";
                    OrdenTB.Text = "";
                    LoteTB.Text = "";
                    ProductoTB.Text = "";
                    ClienteTB.Text = "";
                    GradTB.Text = "";
                    CapacidadTB.Text = "";
                    NCodigosTB.Text = "0";
                    OkTB.Text = "0";
                    ErrorTB.Text = "0";
                    List_Cods.Clear();
                    List_Errs.Clear();
                    List_Perdidos.Clear();
                    RichTCD_Erroneo.Clear();
                    RichTCD_Leido.Clear();
                    precintas_RTB.Clear();
                }
               
                
            }
        }



        //----------------------- PRUEBA ------------------
        private void TesteoLectura()
        {
            //------------------------------------  TESTEO DE LECTURA -----------------------
            Stopwatch timeMeasure = new Stopwatch();
            for (int i = 0; i < 600; i++)
            {
                timeMeasure = new Stopwatch();
                timeMeasure.Start();
                COD_LEIDO = Convert.ToString((20030780005) + i);
                EscribirTB();
                if (i % 2 != 0) List_Errs.Add(ExtraerCodigo(AjustarCodPrecinta(Convert.ToString((20030780005) + i))));

                if (comprobacion_tacos)

                {
                    long precinta_leida = Convert.ToInt64(AjustarCodPrecinta(COD_LEIDO));
                    for (int index = 0; index < primeros_cods_tacos.Count; index++)
                    {
                        //Comprobamos a qué taco pertenece el código leído
                        if ((primeros_cods_tacos[index].primer_cod <= precinta_leida) &&
                        precinta_leida <= (primeros_cods_tacos[index].primer_cod + (primeros_cods_tacos[index].tipo_incremento * 500)))
                        {
                            //Si el código no pertenece al set correspondiente, lo añadimos                        
                            if (!matriz_precintas[index].Contains(precinta_leida))
                            {
                                //Console.WriteLine("Fila : " + i + " cod:" + COD_LEIDO);
                                matriz_precintas[index].Add(precinta_leida);
                                if (matriz_precintas[index].Count == 500)
                                {
                                    precintas_RTB.Items[index].ForeColor = Color.DarkSeaGreen;

                                    Console.WriteLine("TACO COMPLETADO");
                                };
                                break;
                            }

                        }
                    }
                }


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

            }
        }

        private void TesteoLecturaTacos()
        {
            int fila = 0;
            for (int i = 0; i < 4000; i += 501)
            {
                //primer taco 20030780005
                // primeros_cods_tacos.Add(20030780005 + i);
                //ultimo taco 20030789505
                fila++;
                //  Console.WriteLine("Creado el taco nº: " + primeros_cods_tacos.IndexOf((20030780005 + i)) + " con cod: " + (20030780005 + i));
            }
            matriz_precintas = new List<HashSet<long>>();
            for (int i = 0; i < fila; i++)
            {
                matriz_precintas.Add(new HashSet<long>());
                //      matriz_precintas[i].Add(primeros_cods_tacos[i]);
                Console.WriteLine("Primera posición de la fila nº: " + i + " con cod: " + matriz_precintas[i].First());

            }

            int n = 0;
            // long ultimo = primeros_cods_tacos[fila - 1] + 500;
            //    while (20030780005+n < ultimo) {
            Thread proc = new Thread(() =>
            {
                //ParallelOptions po = new ParallelOptions();
                // po.MaxDegreeOfParallelism = 10000;
                // Parallel.For(0, 4000, cnt =>
                // {
                //------------------------------------  TESTEO DE LECTURA -----------------------
                se_escritura.WaitOne();
                COD_LEIDO = Convert.ToString(20030780005 + /*cnt*/n);

                n++;
                //Recorremos los códigos iniciales de cada taco
                for (int i = 0; i < fila; i++)
                {
                    //Comprobamos a qué taco pertenece el código leído
                    // if (primeros_cods_tacos[i] <= Convert.ToInt64(COD_LEIDO) && Convert.ToInt64(COD_LEIDO) <= (primeros_cods_tacos[i] + 500))
                    {

                        //Si el código no pertenece al set correspondiente, lo añadimos                        
                        if (!matriz_precintas[i].Contains(Convert.ToInt64(COD_LEIDO)))
                        {
                            //Console.WriteLine("Fila : " + i + " cod:" + COD_LEIDO);
                            matriz_precintas[i].Add(Convert.ToInt64(COD_LEIDO));
                            EscribirTB();
                            break;
                        }

                    }
                }
                se_escritura.Release();

                if (!Guardando)
                {
                    se_guardado.WaitOne();
                    //Console.WriteLine("GUARDANDOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
                    Guardando = true;
                    Guardar();
                    se_guardado.Release();
                }

            });
            proc.Start();
            //}
            // });

        }

    }
}