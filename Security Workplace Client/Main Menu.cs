using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;

namespace Security_Workplace_Client
{
    public partial class main_menu : Form
    {
        //terminal command to check Raspberry temperature: vcgencmd measure_temp
        //169.254.227.106
        public string rasp16_ip = "169.254.26.25", rasp32w_ip = "169.254.227.106", user = "pi",
                      rasp16_pass = "raspberrylol", rasp32w_pass = "pi3";

        Connection rasp16, rasp32w;//connection to 16GB Raspberry
        Process local_stream; //process to stream on this host
        Thread stream_thread;//new thread for streaming to prevent freezing

        //criminal photo donwload paths
        String dwnld_path = "/home/pi/recognition/detected/";
        String[] file_names_perm = { "Yeldar", "Alisher", "Demo1", "Demo2", "Demo3", "Demo4" },
            file_names = { "Yeldar", "Alisher", "Demo1", "Demo2", "Demo3", "Demo4" };
        String[] detected_names = new string[6]; //array for preventing repetitious detection (photo display) of a single criminal
        int detection_count = 0;
        int imgx = 364, imgy = 80, y_shift = 510; //photo coordinates
        int textx = 600, texty = 50, texty_shift = 480; // alert text coordinates

        private delegate void SafeCallDelegate(); //delegate for safe control invocation

        //data structure for sftp/ssh connection to Raspberry
        public struct Connection
        {
            public SftpClient sftp_client;
            public SshClient ssh_client;

            public Connection(string ip, string user, string pass)
            {
                ConnectionInfo con_info = new ConnectionInfo(ip, user, new PasswordAuthenticationMethod(user, pass));
                sftp_client = new SftpClient(con_info);
                ssh_client = new SshClient(con_info);
            }
        }

        //stream initialization method
        public void videostream_init()
        {
            get_photo();
            //command for launching the python script (videostream initializiation) on the remote Raspberry
            var remote_stream_init = rasp16.ssh_client.CreateCommand("cd recognition && python3 recognizing.py");

            //setting data to launch the videostream with gstreamer on the local host
            local_stream = new Process();
            local_stream.StartInfo.FileName = @"C:\gstreamer\1.0\x86_64\bin\gst-launch-1.0.exe";
            local_stream.StartInfo.Arguments = "udpsrc port=5800 caps=\"application/x-rtp\" ! rtph264depay ! avdec_h264 ! autovideosink";

            //prevent the console window from appearing
            local_stream.StartInfo.CreateNoWindow = true;
            local_stream.StartInfo.UseShellExecute = false;

            //videostream start on both hosts
            local_stream.Start();
            remote_stream_init.Execute();

        }

        //stream termination method
        public void videostream_stop()
        {
            //command to stop the remote python script
            var remote_stream_stop = rasp16.ssh_client.CreateCommand("pkill -f \"recognizing.py\"");
            var delete_photos = rasp16.ssh_client.CreateCommand("cd recognition/detected && rm *");

            local_stream.Kill(); //stopping stream on this host
            remote_stream_stop.Execute();
            delete_photos.Execute();
            stream_thread.Abort();
        }

        //method for getting photos of detected "criminals" from the remote Raspberry
        public async void get_photo()
        {
            while (true)
            {
                for (int i = 0; i < 6; i++)
                {
                    using (Stream current_stream = File.Create(file_names[i]))
                    {
                        try
                        {
                            rasp16.sftp_client.DownloadFile(dwnld_path + file_names[i] + ".jpg", current_stream);
                            detection_count++;
                            detected_names[detection_count - 1] = file_names[i];
                            file_names[i] = "lol";
                            //Random random = new Random();
                            //int rand_int = random.Next(100);


                            //rasp16.sftp_client.RenameFile(dwnld_path + file_names[i], dwnld_path + file_names[i] + rand_int.ToString());
                            rasp16.sftp_client.DeleteFile(dwnld_path + file_names[i] + ".jpg");
                        }
                        catch { }
                    }
                }

                display_photo();

                await Task.Delay(50);
            }
        }
        
        public async void display_photo()
        {
            try
            {
                if (picbox1.InvokeRequired && label1.InvokeRequired)
                {
                    var d = new SafeCallDelegate(display_photo);
                    Invoke(d);
                }
                else if (picbox1.Image == null && detected_names[0] != null)
                {
                    picbox1.ImageLocation = detected_names[0];
                    picbox1.Location = new Point(imgx, imgy);
                    label1.Text = detected_names[0] + " was found!";
                    label1.Location = new Point(textx, texty);
                }

                if (picbox2.InvokeRequired && label2.InvokeRequired)
                {
                    var d = new SafeCallDelegate(display_photo);
                    Invoke(d);
                }
                else if (picbox2.Image == null && detected_names[1] != null)
                {
                    picbox2.ImageLocation = detected_names[1];
                    picbox2.Location = new Point(imgx, imgy + y_shift);
                    label2.Text = detected_names[1] + " was found!";
                    label2.Location = new Point(textx, texty + y_shift);
                }

                if (picbox3.InvokeRequired && label3.InvokeRequired)
                {
                    var d = new SafeCallDelegate(display_photo);
                    Invoke(d);
                }
                else if (picbox3.Image == null && detected_names[2] != null)
                {
                    picbox3.ImageLocation = detected_names[2];
                    picbox3.Location = new Point(imgx, imgy + y_shift * 2);
                    label3.Text = detected_names[2] + " was found!";
                    label3.Location = new Point(textx, texty + y_shift * 2);
                }

                if (picbox4.InvokeRequired && label4.InvokeRequired)
                {
                    var d = new SafeCallDelegate(display_photo);
                    Invoke(d);
                }
                else if (picbox4.Image == null && detected_names[3] != null)
                {
                    picbox4.ImageLocation = detected_names[3];
                    picbox4.Location = new Point(imgx, imgy + y_shift * 3);
                    label4.Text = detected_names[3] + " was found!";
                    label4.Location = new Point(textx, texty + y_shift * 3);
                }

                if (picbox5.InvokeRequired && label5.InvokeRequired)
                {
                    var d = new SafeCallDelegate(display_photo);
                    Invoke(d);
                }
                else if (picbox5.Image == null && detected_names[4] != null)
                {
                    picbox5.ImageLocation = detected_names[4];
                    picbox5.Location = new Point(imgx, imgy + texty_shift * 4);
                    label5.Text = detected_names[4] + " was found!";
                    label5.Location = new Point(textx, texty + y_shift * 4);
                }

                if (picbox6.InvokeRequired && label6.InvokeRequired)
                {
                    var d = new SafeCallDelegate(display_photo);
                    Invoke(d);
                }
                else if (picbox6.Image == null && detected_names[5] != null)
                {
                    picbox6.ImageLocation = detected_names[5];
                    picbox6.Location = new Point(imgx, imgy + texty_shift * 5);
                    label6.Text = detected_names[5] + " was found!";
                    label6.Location = new Point(textx, texty + y_shift * 5);
                }
                await Task.Delay(50);
            }
            catch { }
        }

        public void peep_count_init()
        {
            var count_init_cmd = rasp32w.ssh_client.CreateCommand("cd peoplecounter && python3 counter.py");
            count_init_cmd.Execute();
        }

        public async void peep_count()
        { 
            while (true)
            {
                if (count_label.InvokeRequired)
                {
                    var d = new SafeCallDelegate(peep_count);
                    Invoke(d);
                }
                else
                {
                    count_label.Text = rasp32w.sftp_client.ReadAllText("peoplecounter/peoplenum.txt");
                    await Task.Delay(50);
                }
            }
        }

        public void peep_count_stop()
        {
            var count_stop_cmd = rasp32w.ssh_client.CreateCommand("pkill -f \"counter.py\"");
            count_stop_cmd.Execute();
        }


        public main_menu()
        {
            InitializeComponent();
            label1.Text = null;
            label2.Text = null;
            label3.Text = null;
            label4.Text = null;
            label5.Text = null;
            label6.Text = null;
        }

        private void Main_menu_Load(object sender, EventArgs e)
        {
            rasp16 = new Connection(rasp16_ip, user, rasp16_pass);
            rasp32w = new Connection(rasp32w_ip, user, rasp32w_pass);
            try
            {
                //rasp16.ssh_client.Connect(); //establishing ssh connection to Raspberry(16GB) with the declared struct
                //rasp16.sftp_client.Connect();//sftp
                rasp32w.ssh_client.Connect();//ssh to Raspberry(32GB white)
                rasp32w.sftp_client.Connect();//sftp
            }
            catch { }
        }

        private void stream_starter_Click(object sender, EventArgs e)
        {
            stream_thread = new Thread(videostream_init);
            stream_thread.Start();
        }

        private void stream_stopper_Click(object sender, EventArgs e)
        {
            videostream_stop();
        }

        private void Feed_cleaner_Click(object sender, EventArgs e)
        {
            var delete_photos = rasp16.ssh_client.CreateCommand("cd recognition/detected && rm *");
            delete_photos.Execute();
            file_names = file_names_perm;
            detected_names = new string[6];
            detection_count = 0;
            picbox1.Image = null;
            picbox2.Image = null;
            picbox3.Image = null;
            picbox4.Image = null;
            picbox5.Image = null;
            picbox6.Image = null;
            label1.Text = null;
            label2.Text = null;
            label3.Text = null;
            label4.Text = null;
            label5.Text = null;
            label6.Text = null;
        }

        private void Start_count_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                peep_count_init();
                await Task.Delay(50);
            });
            peep_count();
        }

        private void Stop_count_Click(object sender, EventArgs e)
        {
            peep_count_stop();
        }

        private void Main_menu_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                videostream_stop();
                peep_count_stop();
            }
            catch { }
        }
    }

}
