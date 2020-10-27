using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace LectorQR
{
    public class PipeClient
    {
        public PipeClient(MainLectorQR parent)
        {
            //Realizamos conexión para recibir los datos de la aplicación WHPS
            using (NamedPipeClientStream pipeClient =
                new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut))
            {
                // Connect to the pipe or wait until the pipe is available.
                Console.Write("Attempting to connect to pipe...");

                Thread T1 = new Thread(() =>
                {
                        pipeClient.Connect();

                });T1.Start();
                System.Threading.Thread.Sleep(3000);
                if (!pipeClient.IsConnected) {

                    MessageBox.Show("NO NOS HEMOS CONECTADO");

                }

                else
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(pipeClient);
                        StreamReader sr = new StreamReader(pipeClient);
                        sw.AutoFlush = true;
                        parent.orden = sr.ReadLine();
                        parent.producto = sr.ReadLine();
                        parent.cliente = sr.ReadLine();
                        parent.botellas = sr.ReadLine();
                        parent.graduacion = sr.ReadLine();
                        parent.capacidad = sr.ReadLine();
                        parent.lote = sr.ReadLine();
                        parent.AsignarTB();

                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }


            }

            //Abrimos una nueva conexión que estará esperando a WHPS para comunicar el cierre de la aplicación LectorQR
            using (NamedPipeClientStream pipeClient =
               new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut))
            {
                pipeClient.Connect();

                StreamReader sr = new StreamReader(pipeClient);

                while ((sr.ReadLine()) != "true") { }
                parent.Cerrar();
            }
        }
    }
}