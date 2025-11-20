/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using Microsoft.Extensions.Logging;

namespace dcArca.TestApp;

public sealed class LogViewerForm : Form
{
    private readonly ListBox _lstLog;

    public LogViewerForm()
    {
        Text = "dcARCA - Logs";
        StartPosition = FormStartPosition.Manual;
        Size = new Size(600, 400);
        MinimumSize = new Size(400, 250);

        _lstLog = new ListBox
        {
            Dock = DockStyle.Fill,
            HorizontalScrollbar = true,
            IntegralHeight = false,
            Font = new Font("Consolas", 9f),
        };

        Controls.Add(_lstLog);
    }

    public void AppendLog(LogLevel level, string message, Exception? exception)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => AppendLog(level, message, exception)));
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var entry = $"{timestamp} [{level}] {message}";
        if (exception != null)
        {
            entry += $" | {exception.Message}";
        }

        _lstLog.Items.Insert(0, entry);
        const int maxEntries = 1000;
        if (_lstLog.Items.Count > maxEntries)
        {
            _lstLog.Items.RemoveAt(_lstLog.Items.Count - 1);
        }
    }
}
