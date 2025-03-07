using SensorHealthMonitoring.Shared.Enums;
using SensorHealthMonitoring.Shared.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SensorHealthMonitoring.Server.Controls
{
    public class SensorPanel : Panel
    {
        private readonly int _sensorId;
        private readonly Label _statusLabel;
        private readonly Label _timestampLabel;
        private readonly System.Windows.Forms.Timer _timeoutTimer;
        private readonly IAppLogger _logger;

        public SensorPanel(int sensorId, IAppLogger logger)
        {
            _sensorId = sensorId;
            _logger = logger;

            Size = new Size(150, 150);
            BorderStyle = BorderStyle.Fixed3D;
            BackColor = Color.White;

            var titleLabel = new Label
            {
                Text = $"Sensor {_sensorId}",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _statusLabel = new Label
            {
                Text = "No Signal",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };

            _timestampLabel = new Label
            {
                Text = "",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            Controls.Add(titleLabel);
            Controls.Add(_statusLabel);
            Controls.Add(_timestampLabel);

            _timeoutTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _timeoutTimer.Tick += TimeoutTimer_Tick;

            SetNoSignal();
        }

        public void UpdateStatus(SensorHealth status, DateTime timestamp)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(status, timestamp)));
                return;
            }

            _statusLabel.Text = status.ToString();
            _timestampLabel.Text = timestamp.ToString("HH:mm:ss");
            BackColor = GetColorForStatus(status);

            _logger.LogSensorUpdate(_sensorId, status, timestamp);

            _timeoutTimer.Stop();
            _timeoutTimer.Start();
        }

        private void TimeoutTimer_Tick(object sender, EventArgs e)
        {
            SetNoSignal();
        }

        private void SetNoSignal()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SetNoSignal));
                return;
            }

            BackColor = Color.LightGray;
            _statusLabel.Text = "No Signal";
            _timestampLabel.Text = "";

            _logger.LogSensorUpdate(_sensorId, SensorHealth.NoSignal, DateTime.UtcNow);
            _logger.LogInformation($"Sensor {_sensorId} signal lost");
        }

        private Color GetColorForStatus(SensorHealth status)
        {
            return status switch
            {
                SensorHealth.Good => Color.PaleGreen,
                SensorHealth.Unknown => Color.Khaki,
                SensorHealth.Bad => Color.Salmon,
                _ => Color.LightGray
            };
        }
    }
}