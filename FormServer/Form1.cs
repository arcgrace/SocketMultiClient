using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketTest
{
    public partial class FormServer : Form
    {
        Socket socketListen;//用於監聽的socket
        Socket socketConnect;//用於通訊的socket
        string RemoteEndPoint;     //客戶端的網路節點  
        Dictionary<string, Socket> dicClient = new Dictionary<string, Socket>();//連線的客戶端集合
        public FormServer()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void FormServer_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartSocket();
        }
        public void StartSocket()
        {
            IPHostEntry ihe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ihe.AddressList[5];
            //建立套接字
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(textBox_ip.Text), int.Parse(textBox_port.Text));
            socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //繫結埠和IP
            socketListen.Bind(ipe);
            //設定監聽
            socketListen.Listen(10);
            //連線客戶端
            AsyncConnect(socketListen);
        }
        /// <summary>
        /// 連線到客戶端
        /// </summary>
        /// <param name="socket"></param>
        private void AsyncConnect(Socket socket)
        {
            try
            {
                socket.BeginAccept(asyncResult =>
                {
                    //獲取客戶端套接字
                    socketConnect = socket.EndAccept(asyncResult);
                    RemoteEndPoint = socketConnect.RemoteEndPoint.ToString();
                    dicClient.Add(RemoteEndPoint, socketConnect);//新增至客戶端集合
                    comboBox1.Items.Add(RemoteEndPoint);//新增客戶端埠號
                    AsyncSend(socketConnect, string.Format("歡迎你{0}", socketConnect.RemoteEndPoint));
                    AsyncReceive(socketConnect);
                    AsyncConnect(socketListen);
                }, null);


            }
            catch (Exception ex)
            {

            }

        }
        /// <summary>
        /// 接收訊息
        /// </summary>
        /// <param name="client"></param>
        private void AsyncReceive(Socket socket)
        {
            byte[] data = new byte[1024];
            try
            {
                //開始接收訊息
                socket.BeginReceive(data, 0, data.Length, SocketFlags.None,
                asyncResult =>
                {
                    try
                    {
                        int length = socket.EndReceive(asyncResult);
                        setText(Encoding.UTF8.GetString(data));
                    }
                    catch (Exception)
                    {
                        AsyncReceive(socket);
                    }

                    AsyncReceive(socket);
                }, null);

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 傳送訊息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="p"></param>
        private void AsyncSend(Socket client, string message)
        {
            if (client == null || message == string.Empty) return;
            //資料轉碼
            byte[] data = Encoding.UTF8.GetBytes(message);
            try
            {
                //開始傳送訊息
                client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    //完成訊息傳送
                    int length = client.EndSend(asyncResult);
                }, null);
            }
            catch (Exception ex)
            {
                //傳送失敗，將該客戶端資訊刪除
                string deleteClient = client.RemoteEndPoint.ToString();
                dicClient.Remove(deleteClient);
                comboBox1.Items.Remove(deleteClient);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                AsyncSend(socketConnect, textBox2.Text);
            }
            else
            {
                AsyncSend(dicClient[comboBox1.SelectedItem.ToString()], textBox2.Text);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void setText(string str)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => setText(str)));
            }
            else
            {
                textBox1.Text += "\r\n" + str;
            }
        }
    }
}
