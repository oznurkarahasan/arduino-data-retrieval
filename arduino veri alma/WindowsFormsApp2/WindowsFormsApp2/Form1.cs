using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private SerialPort seriPort;
        private object veriLock = new object();

        public Form1(object veriLock)
        {
            this.veriLock = veriLock;
        }

        public Form1()
        {
            InitializeComponent();
            LoadCOMPorts();
        
        }
        private string secilenCOMPort;

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void LoadCOMPorts()
        {
           
            string[] comPorts = SerialPort.GetPortNames();

            comboBoxCOMPorts.Items.Clear();
            foreach (var port in comPorts)
            {
                comboBoxCOMPorts.Items.Add(port);
            }

            if (comboBoxCOMPorts.Items.Count > 0)
            {
                comboBoxCOMPorts.SelectedIndex = 0;
                secilenCOMPort = comboBoxCOMPorts.SelectedItem.ToString(); 
            }
        }

        private void comboBoxCOMPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
          
            secilenCOMPort = comboBoxCOMPorts.SelectedItem.ToString();
        }

        private void btnStart1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(secilenCOMPort))
                {
                    MessageBox.Show("Lütfen bir COM portu seçin.");
                    return;
                }
                seriPort = new SerialPort(secilenCOMPort, 9600); 
                seriPort.DataReceived += SeriPort_DataReceived;  
                seriPort.BaudRate = 9600; 
                seriPort.ReadBufferSize = 4096; 
               // seriPort.NewLine = "\n"; 

                seriPort.Open(); 

                MessageBox.Show($"Veri alımına başlandı. Bağlı port: {secilenCOMPort}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Seri port hatası: " + ex.Message);
            }

        }

        private void SeriPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort port = (SerialPort)sender;

                Thread.Sleep(100);
                port.DiscardOutBuffer();              

                string gelenVeri = port.ReadExisting();

                port.DiscardInBuffer();

                string[] sayilar = gelenVeri.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(s => s.Trim())  
                                               .Where(s => !string.IsNullOrWhiteSpace(s)) 
                                               .ToArray();
              
                int[] sayilarInt = sayilar.Select(int.Parse).ToArray();

                lock (veriLock) 
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ListBox[] listBoxes = { listBox1, listBox2, listBox3, listBox4, listBox5,
                                        listBox6, listBox7, listBox8, listBox9, listBox10, listBox11 };
                     
                        int index = 0;
                        foreach (string sayi in sayilar)
                        {
                            if (index < listBoxes.Length) 
                            {
                                listBoxes[index].BeginUpdate();  
                                if (!listBoxes[index].Items.Contains(sayi))  
                                {
                                    listBoxes[index].Items.Add(sayi);
                                }
                                listBoxes[index].EndUpdate(); 
                                index++;
                            }
                         
                        }
                        
                        Label[] labels = { label12, label13, label14, label15, label16,
                                        label17, label18, label19, label20, label21, label22 };
                        int index2 = 0;
                        for (int i = 0; i < labels.Length; i++)
                        {
                            
                            if (index2 < sayilar.Length)
                            {
                                labels[i].Text = sayilar[index2];
                                index2++;
                            }
                        }
                    });
      
                }
                for (int i = 0; i < sayilarInt.Length; i += 11)
                {                   
                    int[] grup = sayilarInt.Skip(i).Take(11).ToArray();
                  
                    if (grup.Length == 11)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            kayitVeritabani(grup);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void kayitVeritabani(int[] sayilarInt)
        {
            string connectionString = "Server=DESKTOP-TT4EBD7\\SQLEXPRESS;Database=arduinoVerileri;Trusted_Connection=True;"; 

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string baglanti = @"INSERT INTO TBL_veriler (Gaz_Pedal_Sensor_Degeri, Direksiyon_Aci_Sensor_Degeri, BatV, BatC, BatA, solPWM, solRPM, solA, sagPWM, sagRPM, sagA) 
                             VALUES (@sayi1, @sayi2, @sayi3, @sayi4, @sayi5, @sayi6, @sayi7, @sayi8, @sayi9, @sayi10, @sayi11)";

                    using (SqlCommand komut = new SqlCommand(baglanti, connection))
                    {
                   
                        komut.Parameters.AddWithValue("@sayi1", sayilarInt[0] );
                        komut.Parameters.AddWithValue("@sayi2", sayilarInt[1]);
                        komut.Parameters.AddWithValue("@sayi3", sayilarInt[2]);
                        komut.Parameters.AddWithValue("@sayi4", sayilarInt[3]);
                        komut.Parameters.AddWithValue("@sayi5", sayilarInt[4]);
                        komut.Parameters.AddWithValue("@sayi6", sayilarInt[5]);
                        komut.Parameters.AddWithValue("@sayi7", sayilarInt[6]);
                        komut.Parameters.AddWithValue("@sayi8", sayilarInt[7]);
                        komut.Parameters.AddWithValue("@sayi9", sayilarInt[8]);
                        komut.Parameters.AddWithValue("@sayi10", sayilarInt[9]);
                        komut.Parameters.AddWithValue("@sayi11", sayilarInt[10]);

                        komut.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (seriPort != null)
            {
                try
                {
                    if (seriPort.IsOpen)
                    {
                        seriPort.DiscardInBuffer();  
                        seriPort.DiscardOutBuffer(); 
                        seriPort.Close();          
                    }
                    seriPort.Dispose();             
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Seri port kapatılamadı: " + ex.Message);
                }
                finally
                {
                    seriPort = null;
                }
            }
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void btnDurdur1_Click(object sender, EventArgs e)
        {
            if (seriPort != null)
            {
                try
                {
                    if (seriPort.IsOpen)
                    {
                        seriPort.DiscardInBuffer();
                        seriPort.DiscardOutBuffer();
                        seriPort.Close();
                    }
                    seriPort.Dispose();
                    MessageBox.Show($"Veri alımı durduruldu. Bağlı port: {secilenCOMPort}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Seri port kapatılamadı: " + ex.Message);
                }
                finally
                {
                    seriPort = null;
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
