namespace WavCrusher.WinForms;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private TextBox sourcePathTextBox;
    private TextBox outputPathTextBox;
    private TextBox packagePathTextBox;
    private TextBox restoreRootTextBox;
    private Button browseSourceButton;
    private Button browseOutputButton;
    private Button browsePackageButton;
    private Button browseRestoreRootButton;
    private CheckBox recursiveCheckBox;
    private FlowLayoutPanel commandPanel;
    private Button scanButton;
    private Button archiveButton;
    private Button restoreButton;
    private Button cancelButton;
    private RadioButton restoreToFolderRadioButton;
    private RadioButton restoreToOriginalRadioButton;
    private DataGridView filesGrid;
    private ProgressBar progressBar;
    private Label statusLabel;
    private Label packageSummaryLabel;
    private Label sourceLabel;
    private Label outputLabel;
    private Label packageLabel;
    private Label restoreRootLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        sourcePathTextBox = new TextBox();
        outputPathTextBox = new TextBox();
        packagePathTextBox = new TextBox();
        restoreRootTextBox = new TextBox();
        browseSourceButton = new Button();
        browseOutputButton = new Button();
        browsePackageButton = new Button();
        browseRestoreRootButton = new Button();
        recursiveCheckBox = new CheckBox();
        commandPanel = new FlowLayoutPanel();
        scanButton = new Button();
        archiveButton = new Button();
        restoreButton = new Button();
        cancelButton = new Button();
        restoreToFolderRadioButton = new RadioButton();
        restoreToOriginalRadioButton = new RadioButton();
        filesGrid = new DataGridView();
        progressBar = new ProgressBar();
        statusLabel = new Label();
        packageSummaryLabel = new Label();
        sourceLabel = new Label();
        outputLabel = new Label();
        packageLabel = new Label();
        restoreRootLabel = new Label();
        var rootLayout = new TableLayoutPanel();
        var restoreModePanel = new FlowLayoutPanel();
        var statusPanel = new TableLayoutPanel();

        ((System.ComponentModel.ISupportInitialize)filesGrid).BeginInit();
        SuspendLayout();

        rootLayout.ColumnCount = 3;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Padding = new Padding(12);
        rootLayout.RowCount = 9;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

        sourceLabel.Text = "Source folder";
        sourceLabel.TextAlign = ContentAlignment.MiddleLeft;
        sourceLabel.Dock = DockStyle.Fill;

        outputLabel.Text = "Output folder";
        outputLabel.TextAlign = ContentAlignment.MiddleLeft;
        outputLabel.Dock = DockStyle.Fill;

        packageLabel.Text = "Package (.tar.gz)";
        packageLabel.TextAlign = ContentAlignment.MiddleLeft;
        packageLabel.Dock = DockStyle.Fill;

        restoreRootLabel.Text = "Restore folder";
        restoreRootLabel.TextAlign = ContentAlignment.MiddleLeft;
        restoreRootLabel.Dock = DockStyle.Fill;

        sourcePathTextBox.Dock = DockStyle.Fill;
        sourcePathTextBox.AccessibleName = "Source folder path";

        outputPathTextBox.Dock = DockStyle.Fill;
        outputPathTextBox.AccessibleName = "Output folder path";

        packagePathTextBox.Dock = DockStyle.Fill;
        packagePathTextBox.AccessibleName = "Package path";

        restoreRootTextBox.Dock = DockStyle.Fill;
        restoreRootTextBox.AccessibleName = "Restore root path";

        browseSourceButton.Text = "Browse...";
        browseSourceButton.AutoSize = true;
        browseSourceButton.Dock = DockStyle.Fill;
        browseSourceButton.AccessibleName = "Browse for source folder";
        browseSourceButton.Click += BrowseSourceButton_Click;

        browseOutputButton.Text = "Browse...";
        browseOutputButton.AutoSize = true;
        browseOutputButton.Dock = DockStyle.Fill;
        browseOutputButton.AccessibleName = "Browse for output folder";
        browseOutputButton.Click += BrowseOutputButton_Click;

        browsePackageButton.Text = "Open...";
        browsePackageButton.AutoSize = true;
        browsePackageButton.Dock = DockStyle.Fill;
        browsePackageButton.AccessibleName = "Browse for package file";
        browsePackageButton.Click += BrowsePackageButton_Click;

        browseRestoreRootButton.Text = "Browse...";
        browseRestoreRootButton.AutoSize = true;
        browseRestoreRootButton.Dock = DockStyle.Fill;
        browseRestoreRootButton.AccessibleName = "Browse restore folder";
        browseRestoreRootButton.Click += BrowseRestoreRootButton_Click;

        recursiveCheckBox.Text = "Include subfolders";
        recursiveCheckBox.AutoSize = true;
        recursiveCheckBox.AccessibleName = "Include subfolders";
        recursiveCheckBox.Margin = new Padding(0, 6, 16, 3);

        scanButton.Text = "Scan";
        scanButton.AutoSize = true;
        scanButton.AccessibleName = "Scan for WAV files";
        scanButton.Click += ScanButton_Click;

        archiveButton.Text = "Create package";
        archiveButton.AutoSize = true;
        archiveButton.Enabled = false;
        archiveButton.AccessibleName = "Create archive package";
        archiveButton.Click += ArchiveButton_Click;

        restoreButton.Text = "Restore";
        restoreButton.AutoSize = true;
        restoreButton.Enabled = false;
        restoreButton.AccessibleName = "Restore files from package";
        restoreButton.Click += RestoreButton_Click;

        cancelButton.Text = "Cancel";
        cancelButton.AutoSize = true;
        cancelButton.Enabled = false;
        cancelButton.AccessibleName = "Cancel current operation";
        cancelButton.Click += CancelButton_Click;

        restoreToFolderRadioButton.Text = "Restore to folder";
        restoreToFolderRadioButton.Checked = true;
        restoreToFolderRadioButton.AutoSize = true;
        restoreToFolderRadioButton.AccessibleName = "Restore to selected folder";
        restoreToFolderRadioButton.CheckedChanged += RestoreModeChanged;

        restoreToOriginalRadioButton.Text = "Original locations";
        restoreToOriginalRadioButton.AutoSize = true;
        restoreToOriginalRadioButton.AccessibleName = "Restore to original file locations";
        restoreToOriginalRadioButton.CheckedChanged += RestoreModeChanged;

        restoreModePanel.BorderStyle = BorderStyle.FixedSingle;
        restoreModePanel.FlowDirection = FlowDirection.LeftToRight;
        restoreModePanel.Dock = DockStyle.Fill;
        restoreModePanel.Padding = new Padding(2);
        restoreModePanel.Controls.Add(restoreToFolderRadioButton);
        restoreModePanel.Controls.Add(restoreToOriginalRadioButton);

        filesGrid.AllowUserToAddRows = false;
        filesGrid.AllowUserToDeleteRows = false;
        filesGrid.AllowUserToResizeRows = false;
        filesGrid.BackgroundColor = SystemColors.Window;
        filesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        filesGrid.Dock = DockStyle.Fill;
        filesGrid.ReadOnly = true;
        filesGrid.RowHeadersVisible = false;
        filesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        filesGrid.AccessibleName = "Archive and restore item list";
        filesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "RelativePath",
            HeaderText = "Source file",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 220
        });
        filesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "LengthBytes",
            HeaderText = "Bytes",
            Width = 110
        });
        filesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Status",
            HeaderText = "Status",
            Width = 125
        });
        filesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Message",
            HeaderText = "Message",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 240
        });

        progressBar.Dock = DockStyle.Fill;
        progressBar.AccessibleName = "Operation progress";
        progressBar.Style = ProgressBarStyle.Blocks;

        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusLabel.AutoEllipsis = true;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.AccessibleName = "Status";

        packageSummaryLabel.Text = "No package loaded.";
        packageSummaryLabel.TextAlign = ContentAlignment.MiddleLeft;
        packageSummaryLabel.Dock = DockStyle.Fill;
        packageSummaryLabel.ForeColor = Color.FromArgb(64, 64, 64);

        commandPanel.BorderStyle = BorderStyle.None;
        commandPanel.Dock = DockStyle.Fill;
        commandPanel.FlowDirection = FlowDirection.LeftToRight;
        commandPanel.WrapContents = false;
        commandPanel.Controls.Add(recursiveCheckBox);
        commandPanel.Controls.Add(scanButton);
        commandPanel.Controls.Add(archiveButton);
        commandPanel.Controls.Add(restoreButton);
        commandPanel.Controls.Add(cancelButton);

        statusPanel.ColumnCount = 2;
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusPanel.Dock = DockStyle.Fill;
        statusPanel.Controls.Add(progressBar, 0, 0);
        statusPanel.Controls.Add(statusLabel, 1, 0);

        rootLayout.Controls.Add(sourceLabel, 0, 0);
        rootLayout.Controls.Add(sourcePathTextBox, 1, 0);
        rootLayout.Controls.Add(browseSourceButton, 2, 0);

        rootLayout.Controls.Add(outputLabel, 0, 1);
        rootLayout.Controls.Add(outputPathTextBox, 1, 1);
        rootLayout.Controls.Add(browseOutputButton, 2, 1);

        rootLayout.Controls.Add(packageLabel, 0, 2);
        rootLayout.Controls.Add(packagePathTextBox, 1, 2);
        rootLayout.Controls.Add(browsePackageButton, 2, 2);

        rootLayout.Controls.Add(restoreRootLabel, 0, 3);
        rootLayout.Controls.Add(restoreRootTextBox, 1, 3);
        rootLayout.Controls.Add(browseRestoreRootButton, 2, 3);

        rootLayout.Controls.Add(commandPanel, 1, 4);
        rootLayout.SetColumnSpan(commandPanel, 2);

        rootLayout.Controls.Add(restoreModePanel, 1, 5);
        rootLayout.SetColumnSpan(restoreModePanel, 2);

        rootLayout.Controls.Add(filesGrid, 0, 6);
        rootLayout.SetColumnSpan(filesGrid, 3);

        rootLayout.Controls.Add(packageSummaryLabel, 0, 7);
        rootLayout.SetColumnSpan(packageSummaryLabel, 3);

        // status row
        rootLayout.Controls.Add(statusPanel, 0, 8);
        rootLayout.SetColumnSpan(statusPanel, 3);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1040, 680);
        Controls.Add(rootLayout);
        MinimumSize = new Size(860, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WavCrusher";

        ((System.ComponentModel.ISupportInitialize)filesGrid).EndInit();
        ResumeLayout(false);
    }
}
