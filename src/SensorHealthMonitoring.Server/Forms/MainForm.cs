using SensorHealthMonitoring.Server.Controls;
using SensorHealthMonitoring.Shared.Interfaces;
using SensorHealthMonitoring.Shared.Models;

namespace SensorHealthMonitoring.Server.Forms;
public partial class MainForm : Form, IMessageHandler
{
    private readonly Dictionary<int, SensorPanel> _sensorPanels;
    private readonly ITcpServerWrapper _server;
    private readonly IAppLogger _logger;

    private MenuStrip _menuStrip;
    private ToolStripMenuItem _fileMenu;
    private ToolStripMenuItem _startServerItem;
    private ToolStripMenuItem _stopServerItem;
    private ToolStripMenuItem _exitItem;
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _serverStatusLabel;

    private Button _startButton;
    private Button _stopButton;

    private TextBox _logTextBox;
    private Label _logLabel;
    private Panel _logPanel;

    public MainForm(ITcpServerWrapper server, IAppLogger logger)
    {
        InitializeComponent();
        _logger = logger;
        _server = server;
        _sensorPanels = new Dictionary<int, SensorPanel>();

        InitializeMenu();
        InitializeStatusBar();
        InitializeUI();
        UpdateServerStatus(false);
    }

    private void InitializeMenu()
    {
        _menuStrip = new MenuStrip();

        _fileMenu = new ToolStripMenuItem("File");

        _startServerItem = new ToolStripMenuItem("Start Server");
        _startServerItem.Click += (s, e) => StartServer();

        _stopServerItem = new ToolStripMenuItem("Stop Server");
        _stopServerItem.Click += (s, e) => StopServer();

        _exitItem = new ToolStripMenuItem("Exit");
        _exitItem.Click += (s, e) => Close();

        _fileMenu.DropDownItems.Add(_startServerItem);
        _fileMenu.DropDownItems.Add(_stopServerItem);
        _fileMenu.DropDownItems.Add(new ToolStripSeparator());
        _fileMenu.DropDownItems.Add(_exitItem);

        _menuStrip.Items.Add(_fileMenu);

        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;
    }

    private void InitializeStatusBar()
    {
        _statusStrip = new StatusStrip();

        _serverStatusLabel = new ToolStripStatusLabel
        {
            Text = "Server Status: Stopped",
            ForeColor = Color.Red
        };

        _statusStrip.Items.Add(_serverStatusLabel);

        Controls.Add(_statusStrip);
    }

    private void InitializeUI()
    {
        Text = "Sensor Health Monitoring";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(875, 600);

        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(10),
        };

        // Configure row heights
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _menuStrip.Dock = DockStyle.Top;

        Panel buttonPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            Margin = new Padding(12, 25, 0, 0)
        };


        _startButton = new Button
        {
            Text = "Start Server",
            Width = 80,
            Height = 40,
            Location = new Point(0, 5)
        };
        _startButton.Click += (s, e) => StartServer();

        _stopButton = new Button
        {
            Text = "Stop Server",
            Width = 80,
            Height = 40,
            Location = new Point(90, 5)
        };
        _stopButton.Click += (s, e) => ConfirmAndStopServer();

        buttonPanel.Controls.Add(_startButton);
        buttonPanel.Controls.Add(_stopButton);

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(5),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        for (int i = 1; i <= 5; i++)
        {
            var panel = new SensorPanel(i, _logger)
            {
                Width = 150,
                Height = 150,
                Margin = new Padding(5)
            };
            _sensorPanels.Add(i, panel);
            flowPanel.Controls.Add(panel);
        }

        _logLabel = new Label
        {
            Text = "Log Messages:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        _logTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.White,
            Font = new Font("Consolas", 9),
            WordWrap = false
        };

        mainLayout.Controls.Add(buttonPanel, 0, 0);
        mainLayout.Controls.Add(flowPanel, 0, 1);
        mainLayout.Controls.Add(_logLabel, 0, 2);
        mainLayout.Controls.Add(_logTextBox, 0, 3);

        Controls.Add(mainLayout);

        _statusStrip.Dock = DockStyle.Bottom;
    }


    private void StartServer()
    {
        try
        {
            _server.Start(this);
            _logger.LogInformation("Server started successfully");
            UpdateServerStatus(true);
            LogMessage("Server started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to start server: {ex.Message}");
            LogMessage($"Error: Failed to start server: {ex.Message}");
            MessageBox.Show("Failed to start server: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfirmAndStopServer()
    {
        DialogResult result = MessageBox.Show(
            "Are you sure you want to stop the server?",
            "Confirm Server Stop",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (result == DialogResult.Yes)
        {
            StopServer();
        }
        else
        {
            LogMessage("Server stop operation canceled by user");
        }
    }

    private void StopServer()
    {
        _server.Stop();
        _logger.LogInformation("Server stopped");
        UpdateServerStatus(false);
        LogMessage("Server stopped");
    }

    private void UpdateServerStatus(bool isRunning)
    {
        if (isRunning)
        {
            _serverStatusLabel.Text = "Server Status: Running";
            _serverStatusLabel.ForeColor = Color.Green;
            _startServerItem.Enabled = false;
            _stopServerItem.Enabled = true;
        }
        else
        {
            _serverStatusLabel.Text = "Server Status: Stopped";
            _serverStatusLabel.ForeColor = Color.Red;
            _startServerItem.Enabled = true;
            _stopServerItem.Enabled = false;
        }
    }

    public void LogMessage(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => LogMessage(message)));
            return;
        }

        string timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        _logTextBox.AppendText(timestampedMessage + Environment.NewLine);

        _logTextBox.SelectionStart = _logTextBox.Text.Length;
        _logTextBox.ScrollToCaret();
    }

    public void HandleMessage(MessagePacket<Sensor> message)
    {
        if (message.Data == null) return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandleMessage(message)));
            return;
        }

        LogMessage($"Received sensor data: {message.Data}");

        if (_sensorPanels.TryGetValue(message.Data.SensorId, out var panel))
        {
            panel.UpdateStatus(message.Data.HealthStatus, message.Timestamp);
            LogMessage($"Updated Sensor #{message.Data.SensorId} Status: {message.Data.HealthStatus}");
        }
        else
        {
            LogMessage($"Warning: Received data for unknown Sensor ID: {message.Data.SensorId}");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        LogMessage("Application shutting down...");
        _server.Stop();
        base.OnFormClosing(e);
    }
}