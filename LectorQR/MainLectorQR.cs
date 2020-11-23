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

//Leemos todas las precintas Pi de un taco T (0<=i<=500)
//Leemos todos los tacos Ti (0<=i<=TacosaProducir)
//Comprobamos los códigos leídos una vez terminado cada taco
//Si aparece una precinta perdida del taco 1 y estamos en el taco 10, comprobamos en la lista de perdidas.


namespace LectorQR
{
    //  09/11/2020



    public partial class MainLectorQR : Form
    {


        //Matriz dónde almacenaremos los códigos de las precintas, cada fila de la matriz es un taco distinto
        //public List<SortedSet<int>> matriz_precintas = new List<SortedSet<int>>();
        public List<HashSet<long>> matriz_precintas = new List<HashSet<long>>();
        public List<long> primeros_cods_tacos = new List<long>();
        static Semaphore se_guardado = new Semaphore(1, 1);
        static Semaphore se_escritura = new Semaphore(1, 1);

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

        //Conteo de códigos
        static int Ncodigos = 0;
        static int Nok = 0;
        static int Nerror = 0;

        public string NombreFichero;
        Thread ThreadConexion;
        //Listas dónde almacenamos los códigos leídos y los códigos erroneos
        static List<string> List_Cods = new List<string>();
        static List<string> List_Errs = new List<string>();
        static List<string> List_Perdidos = new List<string>();
        static List<string> Cods_Taco_Actual = new List<string>();
        //Lista con los primeros códigos de cada taco para poder realizar comprobaciones

        Socket s;

        //Ip del controlador ethernet del ordenador 
        static IPAddress localAddr = IPAddress.Parse("192.168.100.10");
        TcpListener myList = new TcpListener(localAddr, 9004);


        private bool comprobacion_tacos = false;
        public bool Inicio = false;
        private bool copiado_cod_error;
        static List<double> max_tiempo= new List<double>();

        //VARIABLES PARA EL TESTEO DE LECTURA
        double maxT = 0, contador_sec = 0;
        int num_bot_per_sec = 0, max_num_bot_per_sec = 0;
        private bool copiado_taco;

        public MainLectorQR()
        {

            InitializeComponent();

            //CONEXION CON LA APP WHPST
            ThreadConexion = new Thread(() => { new PipeClient(this);});
            ThreadConexion.Start();
            
            /* Stopwatch timeMeasure = new Stopwatch();
            timeMeasure.Start();
            //PROBANDO THREADPOOL
            
            TesteoLecturaTacos();

            timeMeasure.Stop();
            Console.WriteLine("TIEMPO DE LA PRUEBA : " + timeMeasure.Elapsed.TotalMilliseconds + " ms");
            foreach(double d in max_tiempo)
            {
                Console.WriteLine("TIEMPOS COMPROMETIDOS: " + d);
            }
            foreach (HashSet<long> t in matriz_precintas)
            {
                Console.WriteLine("Tamaño del taco: " + matriz_precintas.IndexOf(t) + " " + t.Count);
            }*/

            //FUNCION QUE PERMITE REALIZAR UNA PRUEBA
            //TesteoLectura();
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
                if (!Inicio)
                {
                   
                   if(s!=null)s.Close();
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
//                TcpListener myList = new TcpListener(localAddr, 9004);
                int workerThreads;
                int portThreads;
                ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            //    Console.WriteLine("MAXIMO NUMERO DE THREADS: "+workerThreads);
                int n_threads = 0;

                Thread main = new Thread(() =>
                {
                    while (Inicio)
                    {
                        // if (n_threads < workerThreads)
                        // {
                        // Thread T1 = new Thread(() =>
                        // {
                        //     n_threads++;
                       /* if(myList.Pending()) */myList.Start();

                        try
                        {
                                    //if(!se_escritura.WaitOne(500))
                                    se_escritura.WaitOne();

                                    s = myList.AcceptSocket();

                                    //Guardamos en b los bytes recibidos
                                    byte[] b = new byte[128];
                                    int k = s.Receive(b);
                                    for (int i = 0; i < k; i++) Console.Write(Convert.ToChar(b[i]));
                                    if (b[0] == 2 && b[k - 1] == 3)
                                    {
                                        //ASCIIEncoding asen = new ASCIIEncoding();
                                        //s.Send(asen.GetBytes("The string was recieved by the server."));
                                        COD_LEIDO = "";
                                        //convertimos los bytes a string
                                        COD_LEIDO = getString(b);
                                        COD_LEIDO = COD_LEIDO.Remove(k - 1, COD_LEIDO.Length - k + 1);

                                        //Si está activada la comprobación de tacos entramos aqui
                                        if (comprobacion_tacos)
                                        {
                                            for (int i = 0; i < primeros_cods_tacos.Count; i++)
                                            {
                                                //Comprobamos a qué taco pertenece el código leído
                                                if (primeros_cods_tacos[i] <= Convert.ToInt64(COD_LEIDO) && Convert.ToInt64(COD_LEIDO) <= (primeros_cods_tacos[i] + 500))
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

                                        }
                                        //escribimos en los TB correspondientes el código leído
                                        EscribirTB();
                                  
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
                              //  n_threads--;
                            //});
                            //T1.Start();
                        }
                   // }
                    
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

            for (int i = 0; i < s.Length-1 && s[i] != '/' ; i++)
            {
                r += s[i];
            }
            return r;
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


        //Rellena los TB con el COD_LEIDO
        private void EscribirTB() {
            //Incrementamos el nº de códigos leidos
            
            Ncodigos += 1;

            FillNcodigosTB(Convert.ToString(Ncodigos));

          //  if (COD_LEIDO.Contains("ERROR")) COD_LEIDO = "ERROR";
            FillCodLeidoTB(COD_LEIDO);
            COD_LEIDO = (COD_LEIDO.Contains("ERROR")) ? "ERROR" : COD_LEIDO; 
            switch (COD_LEIDO)
            {
                case "ERROR":
                    Nerror = Ncodigos - Nok;
                    List_Cods.Add(COD_LEIDO+" "+Nerror);

                    FillRichTB(COD_LEIDO+" "+ Nerror);
                    FillErrorTB(Convert.ToString(Nerror));
                    break;

                case "":
                    break;

                default:
                    //Quitamos los caracteres que introduce la cámara al leer (bandera inicio y fin de texto)
                    //AjustarCodPrecinta(COD_LEIDO);
                    //Extraemos el código de la precinta en la URL
                    //COD_LEIDO = ExtraerCodigo(COD_LEIDO);
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
            aux += time + Environment.NewLine;
            for (int i = 0; i < List_Cods.Count; i++)
            {
                if (i == List_Cods.Count - 1) aux += List_Cods[i];
                else aux += List_Cods[i] + Environment.NewLine;
            }

            if (Directory.Exists(@"C:/RegistroPrecintas") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/");

            string date = DateTime.Now.ToString("dd-MM-yyyy");

            string namefile = "C:/RegistroPrecintas/PrecintasFiscales." + OrdenTB.Text + "." + date + ".csv";
            string copyfile = "C:/RegistroPrecintas/CopiasRegistrosFiscales/COPYPrecintasFiscales." + OrdenTB.Text + "." + date + ".csv";

            if (File.Exists(@namefile))
            {
                if (Directory.Exists(@"C:/RegistroPrecintas/CopiasRegistrosFiscales") == false) Directory.CreateDirectory(@"C:/RegistroPrecintas/CopiasRegistrosFiscales");

                if (File.Exists(@copyfile))
                {
                    File.Delete(copyfile);
                }
                File.Copy(namefile, copyfile);

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
        //Guardamos en un fichero los códigos de las precintas eliminadas con la pistola
        private void GuardarEliminadas()
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

        //Eliminamos del fichero los códigos duplicados
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
                GuardarEliminadas();
            }
        }
        private void ProcesarFicheros()
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string namefile = "C:/RegistroPrecintas/PrecintasFiscales." + OrdenTB.Text + "." + date + ".csv";
    
            //Si el fichero existe lo abrimos
            if (File.Exists(@namefile))
            {
                //Creamos lista de string auxiliar
                List<string> aux = new List<string>();
                //Array de string que almacena los datos leidos del fichero
                string[] lineas = File.ReadAllLines(namefile);
                //Lista de string con el resultado de unir los strings de List_Cods y el fichero
                List<string> result = new List<string>();

                //Añadimos los códigos leídos a result, comprobando de no añadir duplicados.
                //Si es un error 
                foreach (string s in List_Cods)
                {

                    if (!result.Contains(s))
                    {
                        result.Add(s);
                    }
                }

                //Comprobamos que en la lista result no falte algún código que esté en el fichero
                for (int i = 1; i < File.ReadAllLines(namefile).Length; i++)
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
            comprobacion_tacos_button.BackColor = (comprobacion_tacos_button.BackColor == Color.White) ? Color.DarkSeaGreen: Color.White;
            comprobacion_tacos = (comprobacion_tacos)? false : true ;
            taco_introducidoTB.Focus();

        }
        private void registrar_taco_Click(object sender, EventArgs e)
        {
            registrar_taco.BackColor = (registrar_taco.BackColor==Color.White)? Color.SeaGreen: Color.White;

            if (registrar_taco.BackColor == Color.SeaGreen)
            {

                taco_introducidoTB.Select();
            }
        }

        private void taco_introducidoTB_KeyDown(object sender, KeyEventArgs e)
            {
            if (copiado_taco && CodigoErroneoTB.Text != "") CodigoErroneoTB.Text = ""; copiado_taco = false;
            if (e.KeyCode == Keys.Enter)
            {
                AjustarCodPrecinta(taco_introducidoTB.Text);
                string aux_taco = ExtraerCodErronea(taco_introducidoTB.Text);
                
                primeros_cods_tacos.Add(Convert.ToInt64(aux_taco));

                precintas_RTB.Text += aux_taco + Environment.NewLine;
                aux_taco = "";
                taco_introducidoTB.Text = "";
                copiado_taco = true;

            }
      
        }

        private void ExitB_Click(object sender, EventArgs e)
        {
            if (!Guardando)
            {
                MainLectorQR_FormClosing(this, new FormClosingEventArgs(CloseReason.UserClosing, false));
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            LoteTB.Text = "Prueba";
            GradTB.Text = "Prueba";
            ClienteTB.Text = "Prueba";
            OrdenTB.Text = "Prueba";
            CapacidadTB.Text = "Prueba";
            ProductoTB.Text = "Prueba";
        }




        //----------------------- PRUEBA ------------------
        private void TesteoLectura()
        {
            //------------------------------------  TESTEO DE LECTURA -----------------------
            Stopwatch timeMeasure = new Stopwatch();
            for (int i = 0; i < 100; i++)
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
                
            }
        }

        private void TesteoLecturaTacos()
        {
            int fila = 0;
            for (int i = 0; i < 4000; i += 501)
            {
                //primer taco 20030780005
                primeros_cods_tacos.Add(20030780005 + i);
                //ultimo taco 20030789505
                fila++;
                Console.WriteLine("Creado el taco nº: " + primeros_cods_tacos.IndexOf((20030780005 + i)) + " con cod: " + (20030780005 + i));
            }
            matriz_precintas = new List<HashSet<long>>();
            for (int i = 0; i < fila; i++)
            {
                matriz_precintas.Add(new HashSet<long>());
                matriz_precintas[i].Add(primeros_cods_tacos[i]);
                Console.WriteLine("Primera posición de la fila nº: " + i + " con cod: " + matriz_precintas[i].First());

            }

            int n = 0;
            long ultimo = primeros_cods_tacos[fila - 1] + 500;
            while (20030780005+n < ultimo) {
                Thread proc = new Thread(() => {
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
                        if (primeros_cods_tacos[i] <= Convert.ToInt64(COD_LEIDO) && Convert.ToInt64(COD_LEIDO) <= (primeros_cods_tacos[i] + 500))
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
            }
           // });

        }

    }
}