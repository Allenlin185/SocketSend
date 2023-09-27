using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HotaProgramMethod;
using XMLMethod;
using System.Xml;
using System.Diagnostics;

namespace SocketSend
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket Socket_sender;
        private byte[] Data = new byte[1024];
        public delegate void MyInvoke(string str1);
        SynchronizationContext _syncContext = null;
        private ProgramMethod PGMethod = new ProgramMethod();
        private GenerateXML hotaXMLmethod = new GenerateXML();
        private bool IsSerrverWork = false;
        private bool CanSend = false;
        private Process EZ;
        public MainWindow()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
            //Timer CheckServertimer = new Timer(new TimerCallback(timerCall), null, 5000, 5000);
            tb_msg.GotFocus += SelectAllText;
            //System.Windows.Forms.SendKeys.SendWait("^(R)");
            //EZ = Process.Start(@"C:\Program Files (x86)\EZ-Writer Pro\MM27A.exe");
            //EZ.StartInfo.Arguments("ALT + R");
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                if (tb_ip.Text == "")
                {
                    tb_ip.Focus();
                    lb_status.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    lb_status.Content = "請輸入伺服器 IP address";
                    return;
                }
                if (tb_port.Text == "")
                {
                    tb_port.Focus();
                    lb_status.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    lb_status.Content = "請輸入Port";
                    return;
                }
                IPAddress ipAddress = IPAddress.Parse(tb_ip.Text.Trim());
                int port = int.Parse(tb_port.Text.Trim());
                Socket_sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    Socket_sender.Connect(ipAddress, port);
                    lb_status.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                    lb_status.Content = "連線到伺服器成功!";
                    IsSerrverWork = true;
                    ReseviceMsg(Socket_sender);
                }
                catch (ObjectDisposedException oe)
                {
                    IsSerrverWork = false;
                }
            }
            catch (Exception)
            {
                lb_status.Content = "無法連線到伺服器: " + tb_ip.Text + ":" + tb_port.Text;
            }
        }
        private void timerCall(object obj)
        {
            if (!IsSerrverWork)
            {
                _syncContext.Post(SetStatus, "尚未連線伺服器，請連線登錄。");
            }
        }
        private void ReseviceMsg(Socket clientSocket)
        {
            bool doit = true;
            Thread thread = new Thread(() =>
            {
                while (doit)
                {
                    try
                    {
                        int getlength = clientSocket.Receive(Data);
                        if (getlength <= 0)
                        {
                            _syncContext.Post(GetMsgFomServer, "\n沒有回覆.");
                            IsSerrverWork = false;
                            break;
                        }
                        else
                        {
                            //int res = 0;
                            string getmsg = Encoding.Default.GetString(Data, 0, getlength).Replace("/r/n", "");
                            PGMethod.WriteLog(getmsg);
                            _syncContext.Post(GetMsgFomServer, getmsg + "\n已接收到訊息。");
                            /*
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(getmsg.Trim());
                            XmlNode rootNode = doc.DocumentElement;
                            string corr_id = rootNode.Attributes["corr_Id"].Value;
                            XmlNode FileTypeNode = rootNode.SelectSingleNode("/TrxSet/TITA/trx_id");
                            if (FileTypeNode == null)
                            {
                                FileTypeNode = rootNode.SelectSingleNode("/TrxSet/TOTA/trx_id");
                                if (FileTypeNode == null) return;
                            }
                            if (string.IsNullOrEmpty(FileTypeNode.InnerText.Trim())) continue;
                            string fileType = FileTypeNode.InnerText.Trim();
                            string returnmsg;
                            switch (fileType)
                            {
                                case "TMESYC01":
                                    Thread.Sleep(200);
                                    returnmsg = hotaXMLmethod.CreateTimeSyc02(corr_id);
                                    Socket_sender.Send(Encoding.UTF8.GetBytes(returnmsg));
                                    break;
                                case "CURINF01":
                                    Thread.Sleep(200);
                                    returnmsg = hotaXMLmethod.CreateCURInfo02(corr_id);
                                    Socket_sender.Send(Encoding.UTF8.GetBytes(returnmsg));
                                    break;
                                case "ONLINE02":
                                case "RMTCMD01":
                                    break;
                                default:
                                    PGMethod.WriteLog("無法判斷檔案類型");
                                    break;
                            }
                            */
                        }
                    }
                    catch (Exception ex)
                    {
                        PGMethod.WriteLog(ex.Message);
                        IsSerrverWork = false;
                        doit = false;
                    }
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
        }
        private void GetMsgFomServer(object msg)
        {
            tbl_log.Text = msg.ToString() + tbl_log.Text;
        }
        private void SetStatus(object msg)
        {
            lb_status.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            lb_status.Content = msg.ToString();
        }
        #region 舊程式
        /*
        public void EndRead(IAsyncResult I)
        {
            MyInvoke mi = new MyInvoke(AddMessage);
            int len = Socket_sender.EndReceive(I);
            Console.WriteLine("網路訊息>>" + Encoding.UTF8.GetString(Data, 0, len));
            Socket_sender.BeginReceive(Data, 0, 1024, SocketFlags.None, EndRead, null);
        }
        //開始傳送
        public void Send(string msg)
        {
            byte[] Buffer = Encoding.UTF8.GetBytes(msg);
            Socket_sender.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, EndSend, Socket_sender);
        }
        //結束傳送
        private void EndSend(IAsyncResult Result)
        {
            Socket_sender.EndSend(Result);
        }
        private void AddMessage(string sMessage)
        {
            this.tbl_log.Text += sMessage + Environment.NewLine;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tb_msg.Text == "")
            {
                tbl_log.Text += "請輸入Message\n";
                return;
            }
            Send(tb_msg.Text);
        }
        */
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbl_log.Text = "";
            #region 發送文字
            /*
            var msg = tb_msg.Text.Trim() + "\r\n";
            try
            {
                if (Socket_sender.Connected)
                {
                    Encoding ei = Encoding.GetEncoding(950);
                    int sendMsgLength = Socket_sender.Send(ei.GetBytes(msg));
                }
                else
                {
                    IsSerrverWork = false;
                }

            }
            catch (ObjectDisposedException)
            {
                IsSerrverWork = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsSerrverWork = false;
            }
            */
            #endregion
            #region 發送CSV檔
            string DataFilePath = "SendDATA";
            string[] LDFiles = Directory.GetFiles(DataFilePath, "*.csv");
            foreach (var LDfileName in LDFiles)
            {
                //string FileName = LDfileName.Substring(DataFilePath.Length + 1);
                var reader = new StreamReader(File.OpenRead(LDfileName));
                string msg = "";
                while (!reader.EndOfStream)
                {
                    msg += reader.ReadLine() + Environment.NewLine;
                }
                try
                {
                    if (Socket_sender.Connected)
                    {
                        int sendMsgLength = Socket_sender.Send(Encoding.UTF8.GetBytes(msg));
                    }
                    else
                    {
                        IsSerrverWork = false;
                    }

                }
                catch (ObjectDisposedException oe)
                {
                    IsSerrverWork = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    IsSerrverWork = false;
                }
                finally
                {
                    reader.Close();
                }
                Thread.Sleep(500);
            }
            #endregion
            #region 發送XML檔
            /*
            var msg = hotaXMLmethod.CreateOnline01("0000001");
            PGMethod.WriteLog(msg);
            try
            {
                if (Socket_sender.Connected)
                {
                    int sendMsgLength = Socket_sender.Send(Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    IsSerrverWork = false;
                }
                
            }
            catch (ObjectDisposedException oe)
            {
                IsSerrverWork = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsSerrverWork = false;
            }
            */
            #endregion
        }
        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                textBox.Text = "";
            }
        }
        private bool processData(string remoteData)
        {
            leonardoProcess(remoteData);
            return true;
        }
        private void leonardoProcess(string xmlData)
        {
            PGMethod.WriteLog(xmlData);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlData.Trim());
            XmlElement root = doc.DocumentElement;
            string corr_id = root.GetAttribute("corr_Id");
            string fileType = doc.GetElementsByTagName("trx_id")[0]?.InnerText;
            string returnmsg;
            int sendMsgLength;
            switch (fileType)
            {
                case "TMESYC01":
                    returnmsg = hotaXMLmethod.CreateTimeSyc02(corr_id);
                    sendMsgLength = Socket_sender.Send(Encoding.UTF8.GetBytes(returnmsg));
                    break;
                case "CURINF01":
                    returnmsg = hotaXMLmethod.CreateCURInfo02(corr_id);
                    sendMsgLength = Socket_sender.Send(Encoding.UTF8.GetBytes(returnmsg));
                    returnmsg = hotaXMLmethod.CreateWorkEND01(corr_id);
                    sendMsgLength = Socket_sender.Send(Encoding.UTF8.GetBytes(returnmsg));
                    break;
                case "WRKEND02":
                    //SendXml();
                    break;
                default:
                    PGMethod.WriteLog("無法判斷檔案類型:" + fileType);
                    break;
            }
        }
        private void SendXml()
        {
            string DataFilePath = "SendDATA";
            string[] LDFiles = Directory.GetFiles(DataFilePath, "*.csv");
            if (LDFiles.Length == 0) return;
            var reader = new StreamReader(File.OpenRead(LDFiles[0]));
            string msg = "";
            while (!reader.EndOfStream)
            {
                msg += reader.ReadLine() + "\n";
            }
            try
            {
                if (Socket_sender.Connected)
                {
                    int sendMsgLength = Socket_sender.Send(Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    IsSerrverWork = false;
                }

            }
            catch (ObjectDisposedException)
            {
                IsSerrverWork = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsSerrverWork = false;
            }
            finally
            {
                reader.Close();
            }
            File.Move(LDFiles[0], Path.ChangeExtension(LDFiles[0], ".bak"));
        }
        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
