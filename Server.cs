using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTPServer
{
    public partial class Server : Form
    {
        delegate void SetTextCallback(string text);
        public Server()
        {
            InitializeComponent();
        }
        private void SetText(string text)
        {
            this.Cbb_clients.Items.Add(text);
        }
        private string IP = "127.0.0.1";
        TcpListener listener;
        Socket client;
        List<Socket> clientList = new List<Socket>();
        Socket socketForClient;
        private Thread serverThread;
        private Thread findPC;
        private Thread notification;
        int flag = 0;
        string fileName = "";
        private bool serverRunning = false;
        private bool isConnected = false;
        int x = 9;
        int y = 308;
        int fileReceived = 0;
        string savePath;
        string senderIP;
        string senderMachineName;
        string targetIP;
        string targetName;
        NotificationForm f2;
        private RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        private string pathKeysXML = "";
        private bool isEncryptFile = true;
        IPEndPoint IP1;
        Socket server;
        long Lenght;
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();
        Dictionary<string, string> key = new Dictionary<string, string>();
        private void changeSaveLocButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browse = new FolderBrowserDialog();
            if (browse.ShowDialog() == DialogResult.OK)
            {
                string savePath = browse.SelectedPath;
                savePathLabel.Text = savePath;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files|*.*";
            openFileDialog1.Title = "Select a File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileNameLabel.Text = openFileDialog1.FileName;  //file path
                fileNameLabel.Tag = openFileDialog1.SafeFileName; //file name only.
            }
            timer1.Start();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            fileNameLabel.Text = ".";
            timer1.Stop();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            x = x - 5;
            fileNameLabel.Location = new Point(x, y);
            if (x < (fileNameLabel.Text.Length * (-1)))
                x = 545;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            notificationLabel.ForeColor = Color.Red;
            notificationLabel.Text = "Application is offline";
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void startServer()
        {
            try
            {
                IP1 = new IPEndPoint(IPAddress.Any, 8903);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(IP1);
                server.Listen(10);
                serverRunning = true;
                //listener = new TcpListener(IPAddress.Parse("192.168.2.1"),11000);
                //listener.Start();
                //serverThread = new Thread(new ThreadStart(serverTasks));
               // serverThread.Start();
                //while (!serverThread.IsAlive);
                Thread serverThread = new Thread(AcceptMgs);
                serverThread.IsBackground = true;
                serverThread.Start(server);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void AcceptMgs(object o)
        {
            try
            {
                Socket socketWatc = (Socket)o;
                while (true)
                {
                   
                    Socket socketSend = socketWatc.Accept();
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    clientList.Add(socketSend);
                    NetworkStream stream = new NetworkStream(socketSend);
                    Byte[] bytes = new Byte[1024];
                    String data = null;
                    int i;
                    // Loop to receive all the data sent by the client.
                    if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        //MessageBox.Show(data);
                    }
                    if (data != null)
                    {
                        //MessageBox.Show(data);
                    }
                    else
                    {
                        data = "";
                    }
                    key.Add(socketSend.RemoteEndPoint.ToString(), data);
                    stream.Close();
                    if (this.Cbb_clients.InvokeRequired)
                    {
                        SetTextCallback d = new SetTextCallback(SetText);
                        this.Invoke(d, new object[] { socketSend.RemoteEndPoint.ToString() });
                    }
                    else
                    {
                        // It's on the same thread, no need for Invoke
                        //this.cboUsers.Text = text + " (No Invoke)";
                        this.Cbb_clients.Items.Add(socketSend.RemoteEndPoint.ToString());
                    }
                    Thread td = new Thread(serverTasks);
                    td.IsBackground = true;
                    td.Start(socketSend);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        void serverTasks(object o)
        {
            Socket client = (Socket)o;
                try
               {
                    //Socket socketWatc = (Socket)o;
                   
                    //ListViewItem item = new ListViewItem();
                    //item.Text = client.RemoteEndPoint.ToString();
                    //item.SubItems.Add("127.0.0.1");
                    //onlinePCList.Items.Add(item);
               
                    while (true)
                    {
                        if (fileReceived == 1)
                        {
                            if (MessageBox.Show("Save File?", "File received", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            {
                                File.Delete(savePath);
                                fileReceived = 0;
                                //flag = 0;
                            }
                            else
                            {

                                fileReceived = 0;
                                //flag = 0;
                            }
                        }

                        // MessageBox.Show(socketSend.RemoteEndPoint.ToString());
                        //clientList.Add(client);
                        NetworkStream stream = new NetworkStream(client);
                        // IPEndPoint remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        //MessageBox.Show(remoteIpEndPoint.ToString());
                        //ListViewItem item = new ListViewItem();
                        //String ClientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        //MessageBox.Show(ClientIp);

                        isConnected = true;
                        //MessageBox.Show(flag.ToString());
                        //MessageBox.Show(isConnected.ToString());
                        //NetworkStream stream = myNetworkStream.get

                        if (flag == 1)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                notificationPanel.Visible = true;
                                notificationTempLabel.Text = "File coming..." + "\n" + fileName + "\n" + "From: " + senderIP + " " + senderMachineName;
                                fileNotificationLabel.Text = "File Coming from " + senderIP + " " + senderMachineName;
                            });
                            //MessageBox.Show("doc file hoan tat!");
                            flag = 0;
                            int count = 0;
                            fileReceived = 1;
                            savePath = savePathLabel.Text + "\\" + fileName;
                            using (var output = File.Create(savePath))
                            {
                                //MessageBox.Show("doc file hoan tat!");
                                // read the file divided by 1KB

                                var buffer = new byte[1024];
                                int bytesRead = 0;
                                int t = 0;
                                //MessageBox.Show(Lenght.ToString());
                                while (t < Lenght)
                                {
                                    //MessageBox.Show(t.ToString());
                                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                                    t += bytesRead;
                                    output.Write(buffer, 0, bytesRead);
                                };

                            }

                            //MessageBox.Show(count.ToString());


                            isConnected = false;
                            fileName = "";
                            fileReceived = 1;
                            senderIP = "";
                            senderMachineName = "";
                            Invoke((MethodInvoker)delegate
                            {
                                notificationTempLabel.Text = "";
                                notificationPanel.Visible = false;
                                fileNotificationLabel.Text = "";
                            });


                            //MessageBox.Show("doc file hoan tat!");
                        }
                        else if (flag == 0)
                        {
                            Byte[] bytes = new Byte[256];
                            String data = null;
                            int i;
                            // Loop to receive all the data sent by the client.
                            if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                                //MessageBox.Show(data.ToString());
                                flag = 1;
                            }
                            if (data != null)
                            {
                                string[] msg = data.Split('@');
                                fileName = msg[0];
                                senderIP = msg[1];
                                senderMachineName = msg[2];
                                Lenght = long.Parse(msg[3].ToString());
                            }
                            // MessageBox.Show(fileName, senderIP);
                            //client.Close();
                            // isConnected = false;

                            //MessageBox.Show(Lenght.ToString());
                        }
                        stream.Close();
                        stream.Dispose();

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    //lag = 0;
                    //isConnected = false;

                }
            

        }
        void searchPC()
        {
            bool isNetworkUp = NetworkInterface.GetIsNetworkAvailable();
            if (isNetworkUp)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        this.IP = ip.ToString();
                    }
                }
                Invoke((MethodInvoker)delegate
                {
                    infoLabel.Text = "This Computer: " + this.IP;
                });
               
                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Green;
                    notificationLabel.Text = "Application is Online";
                });
                
                 if (!serverRunning)
                    startServer();
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Red;
                    notificationLabel.Text = "Application is Offline";
                });
                MessageBox.Show("Not connected to LAN");
            }
        }
        void pingCompletedEvent(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply.Status == IPStatus.Success)
            {
                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                }
                catch (SocketException ex)
                {
                    name = ex.Message;
                }
                Invoke((MethodInvoker)delegate
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = ip;
                    item.SubItems.Add(name);
                    onlinePCList.Items.Add(item);
                });
            }
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            ipBox.Text = "";
            onlinePCList.Items.Clear();
            notificationLabel.ForeColor = Color.Green;
            notificationLabel.Text = "Finding...";
            searchPC();
            //startServer();
           
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (serverRunning)
            {
                serverRunning = false;
                onlinePCList.Items.Clear();
                if (listener != null)
                    listener.Stop();
                if (serverThread != null)
                {
                    serverThread.Abort();
                    serverThread.Join();

                }
                server.Close();
                server.Dispose();
                notificationLabel.ForeColor = Color.Red;
                notificationLabel.Text = "Application is Offline";
                infoLabel.Text = "";
                fileNameLabel.Text = ".";
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (serverRunning)
            {
                if (listener != null)
                    listener.Stop();
                if (serverThread != null)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }

            }
            Application.Exit();
        }
        private string GetPublicKey()
        {
            string KeyfromClient = txtPublickey.Text;
            return KeyfromClient;
        }
        private string UsingPrivateKey()
        {
            string ServerPrivateKey = txtPrivatekey.Text;
            return ServerPrivateKey;
        }
        void AddMesseagePublickey(string s)
        {
            //txtPublickey.AppendText(s + Environment.NewLine);
            txtPublickey.Text = s;
            //textBox1.Clear();
        }
        void AddMesseagePrivatekey(string s)
        {
            //txtPrivatekey.AppendText(s + Environment.NewLine);
            txtPrivatekey.Text = s;
            //textBox1.Clear();
        }
        private void CreateNewKeys()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);
            //how to get the private key
            RSAParameters privKey = csp.ExportParameters(true);
            //and the public key ...
            RSAParameters pubKey = csp.ExportParameters(false);
            //converting the public key into a string representation
            string pubKeyString;
            {
                //we need some buffer
                var sw = new StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();                   //right
                AddMesseagePublickey(pubKeyString);             //right
                //return pubKeyString;
            }
            string privKeyString;
            {
                //we need some buffer
                var sw = new StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, privKey);
                //get the string from the stream
                privKeyString = sw.ToString();
                AddMesseagePrivatekey(privKeyString);
            }
        }
       

        private void txtPublickey_TextChanged(object sender, EventArgs e)
        {

        }
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }
        object Derserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
            return stream.ToArray();
        }
        void SendKey(Socket client)
        {
            string Key = txtPublickey.Text;
            byte[] request =Serialize(Key);
            if (request != null)
            {
                client.Send(Serialize(Key));
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            CreateNewKeys();
            Socket item;
            if (Cbb_clients.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng Chọn Máy Khách Để Gửi!");
            }
            else
            {
                item = dicSocket[Cbb_clients.SelectedItem.ToString()];
                SendKey(item);
            }
        }
        /*private void RSA_Algorithm(string inputFile, string outputFile, RSAParameters RSAKeyInfo, bool isEncrypt)
        {
            try
            {
                FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read); //Đọc file input
                FileStream fsCiperText = new FileStream(outputFile, FileMode.Create, FileAccess.Write); //Tạo file output
                fsCiperText.SetLength(0);
                byte[] bin, encryptedData;
                long rdlen = 0;
                long totlen = fsInput.Length;
                int len;
                this.progressBar1.Minimum = 0;
                this.progressBar1.Maximum = 100;

                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.ImportParameters(RSAKeyInfo); //Nhập thông tin khoá RSA (bao gồm khoá riêng)

                int maxBytesCanEncrypted;
                //RSA chỉ có thể mã hóa các khối dữ liệu ngắn hơn độ dài khóa, chia dữ liệu cho một số khối và sau đó mã hóa từng khối và sau đó hợp nhất chúng
                if (isEncrypt)
                    maxBytesCanEncrypted = ((RSA.KeySize - 384) / 8) + 37;// + 7: OAEP - Đệm mã hóa bất đối xứng tối ưu

                else
                    maxBytesCanEncrypted = (RSA.KeySize / 8);
                //Read from the input file, then encrypt and write to the output file.
                while (rdlen < totlen)
                {
                    if (totlen - rdlen < maxBytesCanEncrypted) maxBytesCanEncrypted = (int)(totlen - rdlen);
                    bin = new byte[maxBytesCanEncrypted];
                    len = fsInput.Read(bin, 0, maxBytesCanEncrypted);

                    if (isEncrypt) encryptedData = RSA.Encrypt(bin, false); //Mã Hoá
                    else encryptedData = RSA.Decrypt(bin, false); //Giải mã

                    fsCiperText.Write(encryptedData, 0, encryptedData.Length);
                    rdlen = rdlen + len;
                    //this.progressBar1.Value = (int)((rdlen * 100) / totlen);//thanh tiến trình
                    //this.label1f.Text
                }

                fsCiperText.Close(); //save file
                fsInput.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed: " + ex.Message);
            }
        }*/
        private void sendFileButton_Click(object sender, EventArgs e)
        {
            Socket socketSend;
            string publickeyclient = "";
            if (Cbb_clients.SelectedItem==null)
            {
                MessageBox.Show("Vui lòng Chọn Máy Khách Để Gửi!");
                return;
            }
            else
            {
                socketSend = dicSocket[Cbb_clients.SelectedItem.ToString()];
                publickeyclient= key[Cbb_clients.SelectedItem.ToString()];
                //MessageBox.Show(publickeyclient);
                //MessageBox.Show(socketSend.RemoteEndPoint.ToString());
                Thread td = new Thread(SendbigFile);
                td.IsBackground = true;
                td.Start();
            }   
       /*     long length;
       
        try
        {
                Stopwatch sw = Stopwatch.StartNew();
                sw.Start();
                if (fileNameLabel.Text != ".")
                {
                    //notificationPanel.Visible = true;
                    //notificationTempLabel.Text = "File sending to " + targetIP + " " + targetName + "...";
                    fileNotificationLabel.Text = "Please don't do other tasks. File sending to " + targetIP + " " + targetName + "...";
                    //closing the server

                    //now making this program a client
                    //socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                    string fileName = fileNameLabel.Tag.ToString();
                    //long fileSize = new FileInfo(fileNameLabel.Text).Length;
                    using (FileStream fsRead = new FileStream(fileNameLabel.Text, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        length = fsRead.Length;
                       // MessageBox.Show(length.ToString());
                    }
                    length = ((int)(length / 53) + 1)*64;
                    MessageBox.Show(length.ToString());
                    byte[] fileNameData = Encoding.Default.GetBytes(fileName + "@" + this.IP + "@" + Environment.MachineName + "@" + length.ToString());
                    socketSend.Send(fileNameData);
                    //socketForClient.Shutdown(SocketShutdown.Both);
                    //socketForClient.Close();
                    //socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                    //socketForClient.SendFile(fileNameLabel.Text);                          
                    //socketSend.SendFile(fileNameLabel.Text);

                        FileStream fsInput = new FileStream(fileNameLabel.Text, FileMode.Open, FileAccess.Read); //Đọc file input
                        //FileStream fsCiperText = new FileStream(outputFile, FileMode.Create, FileAccess.Write); //Tạo file output
                        //fsCiperText.SetLength(0);
                        byte[] bin=null, encryptedData=null;
                        long rdlen = 0;
                        long totlen = fsInput.Length;
                        MessageBox.Show(totlen.ToString());
                    //MessageBox.Show(totlen.ToString());
                    int len;
                        this.progressBar2.Minimum = 0;
                        this.progressBar2.Maximum = 100;

                        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                        RSA.FromXmlString(publickeyclient);
                        int maxBytesCanEncrypted=0;
                        //RSA chỉ có thể mã hóa các khối dữ liệu ngắn hơn độ dài khóa, chia dữ liệu cho một số khối và sau đó mã hóa từng khối và sau đó hợp nhất chúng
                        maxBytesCanEncrypted = ((RSA.KeySize - 384) / 8) + 37;// + 7: OAEP - Đệm mã hóa bất đối xứng tối ưu
                        long dem = 0;
                        //Read from the input file, then encrypt and write to the output file.
                        while (rdlen < totlen)
                        {
                            if (totlen - rdlen < maxBytesCanEncrypted) maxBytesCanEncrypted = (int)(totlen - rdlen);
                            bin = new byte[maxBytesCanEncrypted];
                            len = fsInput.Read(bin, 0, maxBytesCanEncrypted);
                            //MessageBox.Show(len.ToString());
                            encryptedData = RSA.Encrypt(bin, false); //Mã Hoá
                            //MessageBox.Show(encryptedData.Length.ToString());
                            //dem += encryptedData.Length;
                            //MessageBox.Show(encryptedData.Length.ToString());
                            //else encryptedData = RSA.Decrypt(bin, false); //Giải mã
                            //socketSend.Send(encryptedData);
                            //fsCiperText.Write(encryptedData, 0, encryptedData.Length);
                            this.progressBar2.Value = (int)((rdlen * 100) / totlen);//thanh tiến trình
                            
                            rdlen = rdlen + len;
                            this.label1f.Text = "Tên tệp xử lý : " + Path.GetFileName(fileNameLabel.Text) + "\t Thành công: " + ((long)(rdlen * 100) / totlen).ToString() + " %";
                            this.label1f.Update();
                            this.label1f.Refresh();
                            socketSend.Send(encryptedData);
                    }
                        //MessageBox.Show(dem.ToString());
                        // MessageBox.Show(rdlen.ToString());
                        //fsCiperText.Close(); //save file
                        fsInput.Close();
                        this.label1f.Text = "";
                        this.progressBar2.Value = 0;
                    
                    // MessageBox.Show("Hoàn Thành!", send.ToString());
                    MessageBox.Show("Hoàn Thành!,File Đã Gửi Đến : "+ socketSend.RemoteEndPoint.ToString());
                    fileNameLabel.Text = "";
                }
                sw.Stop();
            }
            catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "Lỗi Truyền File!");
                }*/
        }
        private void SendbigFile()
        {
            Socket socketSend;
            string publickeyclient = "";
            socketSend = dicSocket[Cbb_clients.SelectedItem.ToString()];
            publickeyclient = key[Cbb_clients.SelectedItem.ToString()];
            
            long length;

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                sw.Start();
                if (fileNameLabel.Text != ".")
                {
                    //notificationPanel.Visible = true;
                    //notificationTempLabel.Text = "File sending to " + targetIP + " " + targetName + "...";
                    fileNotificationLabel.Text = "Please don't do other tasks. File sending to " + targetIP + " " + targetName + "...";

 
                    //socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                    string fileName = fileNameLabel.Tag.ToString();
                    //long fileSize = new FileInfo(fileNameLabel.Text).Length;
                    using (FileStream fsRead = new FileStream(fileNameLabel.Text, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        length = fsRead.Length;
                        // MessageBox.Show(length.ToString());
                    }
                    length = ((int)(length / 53) + 1) * 64;
                    //MessageBox.Show(length.ToString());
                    byte[] fileNameData = Encoding.Default.GetBytes(fileName + "@" + this.IP + "@" + Environment.MachineName + "@" + length.ToString());
                    socketSend.Send(fileNameData);
                    //socketForClient.Shutdown(SocketShutdown.Both);
                    //socketForClient.Close();
                    //socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                    //socketForClient.SendFile(fileNameLabel.Text);                          
                    //socketSend.SendFile(fileNameLabel.Text);

                    FileStream fsInput = new FileStream(fileNameLabel.Text, FileMode.Open, FileAccess.Read); 
                    byte[] bin = null, encryptedData = null;
                    long rdlen = 0;
                    long totlen = fsInput.Length;
                    //MessageBox.Show(totlen.ToString());
                    //MessageBox.Show(totlen.ToString());
                    int len;
                    this.progressBar2.Minimum = 0;
                    this.progressBar2.Maximum = 100;

                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSA.FromXmlString(publickeyclient);
                    int maxBytesCanEncrypted = 0;
                    //RSA chỉ có thể mã hóa các khối dữ liệu ngắn hơn độ dài khóa, chia dữ liệu cho một số khối và sau đó mã hóa từng khối và sau đó hợp nhất chúng
                    maxBytesCanEncrypted = ((RSA.KeySize - 384) / 8) + 37;// + 7: OAEP - Đệm mã hóa bất đối xứng tối ưu
                    long dem = 0;
                    //Read from the input file, then encrypt and write to the output file.
                    while (rdlen < totlen)
                    {
                        if (totlen - rdlen < maxBytesCanEncrypted) maxBytesCanEncrypted = (int)(totlen - rdlen);
                        bin = new byte[maxBytesCanEncrypted];
                        len = fsInput.Read(bin, 0, maxBytesCanEncrypted);
                        //MessageBox.Show(len.ToString());
                        encryptedData = RSA.Encrypt(bin, false);//Mã Hoá
                        //MessageBox.Show(encryptedData.Length.ToString());
                        //dem += encryptedData.Length;
                        //MessageBox.Show(encryptedData.Length.ToString());
                        //else encryptedData = RSA.Decrypt(bin, false); //Giải mã
                        socketSend.Send(encryptedData);
                        //fsCiperText.Write(encryptedData, 0, encryptedData.Length);
                        rdlen = rdlen + len;
                        this.progressBar2.Value = (int)((rdlen * 100) / totlen);//thanh tiến trình
                        this.label1f.Text = "Tên tệp xử lý : " + Path.GetFileName(fileNameLabel.Text) + "\t Thành công: " + ((long)(rdlen * 100) / totlen).ToString() + " %";
                        this.label1f.Update();
                        this.label1f.Refresh();
                        //socketSend.Send(encryptedData);
                    }
                    //MessageBox.Show(dem.ToString());
                    // MessageBox.Show(rdlen.ToString());
                    //fsCiperText.Close(); //save file
                    fsInput.Close();
                    this.label1f.Text = "";
                    this.progressBar2.Value = 0;

                    // MessageBox.Show("Hoàn Thành!", send.ToString());
                    //MessageBox.Show("Hoàn Thành!,File Đã Gửi Đến : " + socketSend.RemoteEndPoint.ToString());
                    fileNameLabel.Text = "";
                }
                sw.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Lỗi Truyền File!");
            }
        }

    }
}
