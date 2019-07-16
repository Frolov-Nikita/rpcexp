using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NModbus;
using NModbus.Data;
using NModbus.Utility;
using Newtonsoft.Json;

namespace Sim
{
    public partial class Form1 : Form
    {
        Task NetworkListener;

        int port = 11502;
        byte slaveId = 1;

        IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

        IModbusSlave slave1;
        TcpListener slaveTcpListener;
        CancellationTokenSource cts;

        SlaveStorage store = new SlaveStorage();

        public List<Tag> Tags { get; set; }
        public int Port { get => port;
            set
            {
                port = value;
                Properties.Settings.Default.Port = port;
                Properties.Settings.Default.Save();
            }
        }
        public byte SlaveId { get => slaveId;
            set
            {
                slaveId = value;
                Properties.Settings.Default.SlaveId = slaveId;
                Properties.Settings.Default.Save();
            }
        }

        public Form1()
        {
            InitializeComponent();
            Properties.Settings.Default.Reload();
            var cfgFile = Properties.Settings.Default.cfgFile;

            Port = Properties.Settings.Default.Port;
            toolStripTextBoxPort.Text = Port.ToString();

            SlaveId = Properties.Settings.Default.SlaveId;
            toolStripTextBoxSlaveId.Text = SlaveId.ToString();

            Tags = new List<Tag>();
            LoadTagsFromFile(cfgFile);
            
            toolStripStatusLabel1.Text = "stop";

            listViewTags.ItemSelectionChanged += ListViewTags_ItemSelectionChanged;
            listViewTags.DoubleClick += ModifyValueEventHandler;
        }


        void LoadTagsFromFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                if (Properties.Settings.Default.cfgFile != fileName)
                {
                    Properties.Settings.Default.cfgFile = fileName;
                    Properties.Settings.Default.Save();
                }

                Text = $"Modbus Sim [{fileName}]";
                var json = System.IO.File.ReadAllText(fileName);
                var tagInfos = JsonConvert.DeserializeObject<List<TagInfo>>(json);
                Tags = new List<Tag>(tagInfos.Count);
                foreach (var ti in tagInfos)
                    Tags.Add(new Sim.Tag(ti, store));
                ReDrawList();
            }
        }

        void SaveTagsToFile(string fileName)
        {
            Properties.Settings.Default.cfgFile = fileName;
            Properties.Settings.Default.Save();
            Text = $"Modbus Sim [{fileName}]";
            System.IO.File.WriteAllText(
                fileName,
                JsonConvert.SerializeObject(Tags, Formatting.Indented));
        }

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "json|*.json";
            openFileDialog1.FileName = Properties.Settings.Default.cfgFile;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                LoadTagsFromFile(openFileDialog1.FileName);
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "json|*.json";
            saveFileDialog1.FileName = Properties.Settings.Default.cfgFile;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                SaveTagsToFile(saveFileDialog1.FileName);

        }

        private void ListViewTags_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var i = e.ItemIndex;

            UpdateList();

            if (e.Item?.Tag == null)
                return;

            propertyGrid1.SelectedObject = e.Item?.Tag;
        }

        private void UpdateListItem(ListViewItem lvi)
        {
            var t = (Tag)lvi.Tag;
            lvi.SubItems[0].Text = t.Name;
            lvi.SubItems[1].Text = t.Description;
            lvi.SubItems[2].Text = (t.ValueType.ToString());
            lvi.SubItems[3].Text = (t.Region.ToString());
            lvi.SubItems[4].Text = (t.Address.ToString());
            lvi.SubItems[5].Text = (t.Value.ToString());
        }

        private void UpdateList()
        {
            foreach (ListViewItem lvi in listViewTags.Items)
                UpdateListItem(lvi);
        }

        private void ReDrawList()
        {
            listViewTags.Items.Clear();
            foreach(var t in Tags)
            {
                var lvi = new ListViewItem(t.Name);
                lvi.Tag = t;
                lvi.SubItems.Add(t.Description);
                lvi.SubItems.Add(t.ValueType.ToString());
                lvi.SubItems.Add(t.Region.ToString());
                lvi.SubItems.Add(t.Address.ToString());
                lvi.SubItems.Add(t.Value.ToString());
                listViewTags.Items.Add(lvi);

                t.PropertyChanged += (s, e) =>  UpdateListItem(lvi);
            }                
        }
        
        private void ToolStripButtonRun_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();

            if (int.TryParse(toolStripTextBoxPort.Text, out int p))
                Port = p;
            
            toolStripTextBoxPort.Text = Port.ToString();
            toolStripTextBoxPort.Enabled = false;

            if (byte.TryParse(toolStripTextBoxSlaveId.Text, out byte sid))
                SlaveId = sid;

            toolStripTextBoxSlaveId.Text = SlaveId.ToString();
            toolStripTextBoxSlaveId.Enabled = false;

            slaveTcpListener = new TcpListener(address, Port);
            slaveTcpListener.Start();
            IModbusFactory factory = new ModbusFactory();
            
            IModbusSlaveNetwork network = factory.CreateSlaveNetwork(slaveTcpListener);
            
            slave1 = factory.CreateSlave(SlaveId, store);

            network.AddSlave(slave1);

            NetworkListener = network.ListenAsync(cts.Token);
            
            toolStripStatusLabel1.Text = "run";
        }

        private void ToolStripButtonStop_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            NetworkListener.Wait(1000);
            toolStripStatusLabel1.Text = "stop";
            toolStripTextBoxPort.Enabled = true;
            toolStripTextBoxSlaveId.Enabled = true;
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Sim.Tag(store);
            t.Name = "NewTag";
            t.Region = ModbusRegion.HoldingRegisters;
            t.ValueType = ModbusValueType.Int16;
            t.Address = 1;
            Tags.Add(new Sim.Tag(store));

            ReDrawList();
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewTags.SelectedItems.Count == 0)
                return;

            foreach (ListViewItem i in listViewTags.SelectedItems)
                Tags.Remove((Tag)i.Tag);

            ReDrawList();
        }

        private void ToolStripButtonUpdateList_Click(object sender, EventArgs e)
        {
            ReDrawList();
        }

        private void ModifyValueEventHandler(object sender, EventArgs e)
        {
            if (listViewTags.SelectedItems.Count == 0)
                return;
            Tag t = (Tag)listViewTags.SelectedItems[0].Tag;

            Form f;
            
            switch (t.ValueType)
            {
                case ModbusValueType.Bool:
                    f = new FormBoolValueSetter(t);                    
                    f.Left = Left;
                    f.Top = MousePosition.Y;
                    f.Show();
                    break;
                /*case ModbusValueType.Float:
                case ModbusValueType.Int16:
                case ModbusValueType.Int32:
                    break;*/
                default:
                    f = new FormValueSetter(t);
                    
                    f.Left = Left;
                    f.Top = MousePosition.Y;
                    f.Show();
                    break;
            }
        }

        private void ToolStripTextBoxPort_TextChanged(object sender, EventArgs e)
        {
            if (toolStripTextBoxPort.Text == Port.ToString())
                return;

            if (int.TryParse(toolStripTextBoxPort.Text, out int p))
                Port = p;

            toolStripTextBoxPort.Text = Port.ToString();
        }

        private void ToolStripTextBoxSlaveId_TextChanged(object sender, EventArgs e)
        {
            if (toolStripTextBoxSlaveId.Text == SlaveId.ToString())
                return;

            if (byte.TryParse(toolStripTextBoxSlaveId.Text, out byte sid))
                SlaveId = sid;

            toolStripTextBoxSlaveId.Text = SlaveId.ToString();
        }
    }
}
