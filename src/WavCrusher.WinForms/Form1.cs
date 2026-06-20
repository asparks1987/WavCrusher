using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WavCrusher.Application.Archiving;
using WavCrusher.Application.Scanning;
using WavCrusher.Domain;
using WavCrusher.Infrastructure.Scanning;
using WavCrusher.WavPack;

namespace WavCrusher.WinForms;

public partial class Form1 : Form
{
    private const string ArchiveCompletedStatus = "Verified";
    private const string ArchiveFailedStatus = "Failed";
    private const string ArchiveCancelledStatus = "Cancelled";
    private const string ArchiveSkippedStatus = "Skipped";
    private const string ArchiveConflictStatus = "Conflict";
    private static readonly string[] ManifestFileNameCandidates =
    [
        TarballPackagePaths.ManifestFileName,
        "archive-manifest.v1.json",
        "wavcompactor-manifest.v1.json"
    ];

    private readonly IWaveFileScanner _scanner;
    private readonly IArchiveCompressor _compressor;
    private readonly string _wvunpackPath;
    private readonly BindingList<ArchiveRow> _rows = [];
    private TarballArchiveManifest? _loadedManifest;
    private string _loadedPackagePath = string.Empty;
    private string _loadedPackageStagingRoot = string.Empty;

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private CancellationTokenSource? _operationCancellation;

    private enum RestoreMode
    {
        RestoreFolder,
        OriginalLocations
    }

    public Form1()
    {
        InitializeComponent();

        var tools = WavPackToolLocator.Locate();
        _compressor = new WavPackCliCompressor(tools.WavPackPath, tools.WvUnpackPath);
        _scanner = new FileSystemWaveFileScanner();
        _wvunpackPath = Path.GetFullPath(tools.WvUnpackPath);

        filesGrid.AutoGenerateColumns = false;
        filesGrid.DataSource = _rows;
        recursiveCheckBox.Checked = true;
        restoreToFolderRadioButton.Checked = true;
        UpdateModeControls();
        SetBusy(false);
        UpdateStatus("Choose source and output folders, scan WAV files, then create a package.");
    }

    private async void ScanButton_Click(object sender, EventArgs e)
    {
        await RunScanAsync().ConfigureAwait(true);
    }

    private async void ArchiveButton_Click(object sender, EventArgs e)
    {
        await RunArchiveAsync().ConfigureAwait(true);
    }

    private async void BrowsePackageButton_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open WavCrusher package",
            Filter = "WavCrusher tarball (*.tar.gz)|*.tar.gz|All files|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            packagePathTextBox.Text = dialog.FileName;
            await LoadPackageAsync(dialog.FileName).ConfigureAwait(true);
        }
    }

    private async void RestoreButton_Click(object sender, EventArgs e)
    {
        await RunRestoreAsync().ConfigureAwait(true);
    }

    private void ClearButton_Click(object sender, EventArgs e)
    {
        ClearCurrentJobState();
    }

    private void AboutButton_Click(object sender, EventArgs e)
    {
        using var dialog = new AboutDialog();
        dialog.ShowDialog(this);
    }

    private void BrowseSourceButton_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose the folder containing WAV files to archive",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            sourcePathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void BrowseOutputButton_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose where the .tar.gz package will be written",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            outputPathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void BrowseRestoreRootButton_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose folder where WAV files should be restored",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            restoreRootTextBox.Text = dialog.SelectedPath;
        }
    }

    private void RestoreModeChanged(object sender, EventArgs e)
    {
        UpdateModeControls();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        _operationCancellation?.Cancel();
    }

    private async Task RunScanAsync()
    {
        if (!TryGetExistingDirectory(sourcePathTextBox.Text, "source folder", out var sourceRoot))
        {
            return;
        }

        _rows.Clear();
        _loadedManifest = null;
        _loadedPackagePath = string.Empty;
        packageSummaryLabel.Text = "No package loaded.";
        clearButton.Enabled = false;

        _operationCancellation = new CancellationTokenSource();
        SetBusy(true);

        try
        {
            InitializeProgressUi(1, "Scanning for WAV files");
            UpdateStatus("Scanning for WAV files...");
            var candidates = await _scanner
                .ScanAsync(sourceRoot, recursiveCheckBox.Checked, _operationCancellation.Token)
                .ConfigureAwait(true);

            foreach (var candidate in candidates)
            {
                _rows.Add(ArchiveRow.FromCandidate(candidate));
            }

            UpdateStatus(candidates.Count == 0
                ? "No .wav files found in the selected source root."
                : $"Found {candidates.Count:N0} .wav file(s).");
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Scan cancelled.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Scan failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Scan failed: {ex.Message}");
        }
        finally
        {
            _operationCancellation?.Dispose();
            _operationCancellation = null;
            SetBusy(false);
            archiveButton.Enabled = _rows.Count > 0;
        }
    }

    private async Task RunArchiveAsync()
    {
        if (_rows.Count == 0)
        {
            MessageBox.Show(this, "Scan for WAV files first.", "Nothing to archive", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryGetExistingDirectory(sourcePathTextBox.Text, "source folder", out var sourceRootInput) ||
            !TryGetOrCreateDirectory(outputPathTextBox.Text, "output folder", out var outputRoot))
        {
            return;
        }

        ArchiveRoots roots;
        try
        {
            roots = ArchiveRoots.Create(
                NormalizedAbsolutePath.Create(sourceRootInput),
                NormalizedAbsolutePath.Create(outputRoot));
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, "Invalid roots", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var packagePath = TarballPackagePaths.BuildPackagePath(roots.SourceRoot.Value, roots.DestinationRoot.Value, DateTime.UtcNow);
        var confirm = MessageBox.Show(
            this,
            $"Create package in:{Environment.NewLine}{packagePath}{Environment.NewLine}{Environment.NewLine}Every item will be verified before publication.",
            "Create package",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var packageId = Guid.NewGuid().ToString("N");
        var packageManifestRoot = Path.Combine(Path.GetTempPath(), "WavCrusher", "TarballBuild", packageId);
        var archivesRoot = Path.Combine(packageManifestRoot, "archives");
        var stagedManifestPath = Path.Combine(packageManifestRoot, TarballPackagePaths.ManifestFileName);
        var tarPath = TarballPackagePaths.BuildIntermediateTarPath(packageManifestRoot, packageId);
        var packageTempPath = packagePath + ".partial";

        _operationCancellation = new CancellationTokenSource();
        SetBusy(true);

        try
        {
            Directory.CreateDirectory(archivesRoot);
            var workerCount = GetArchiveWorkerCount(_rows.Count);
            InitializeProgressUi(_rows.Count, $"Archive progress ({workerCount} workers)");
            UpdateStatus($"Starting archive with {workerCount} worker(s)...");

            var rowSnapshot = _rows.ToList();
            var manifestItems = new TarballManifestItem?[rowSnapshot.Count];
            var manifestItemTasks = new List<Task<ArchiveWorkOutcome>>(rowSnapshot.Count);
            var processed = 0;
            using var concurrencyGate = new SemaphoreSlim(workerCount);

            for (var rowIndex = 0; rowIndex < rowSnapshot.Count; rowIndex++)
            {
                var currentIndex = rowIndex;
                var row = rowSnapshot[currentIndex];
                _operationCancellation.Token.ThrowIfCancellationRequested();

                row.Status = ArchiveSkippedStatus;
                row.Message = "Preparing";
                row.ArchiveLengthBytes = null;
                filesGrid.Refresh();

                ArchiveItemProgress? lastProgress = null;
                var progress = new Progress<ArchiveItemProgress>(p =>
                {
                    lastProgress = p;
                    row.Status = p.Stage;
                    row.Message = p.Message;
                    filesGrid.Refresh();
                    UpdateCurrentItemProgressUi(row.RelativePath, p.Stage, true);
                });

                manifestItemTasks.Add(Task.Run(async () =>
                {
                    await concurrencyGate.WaitAsync(_operationCancellation.Token).ConfigureAwait(false);
                    try
                    {
                        return await ArchiveRowAsync(currentIndex, row, archivesRoot, progress, _operationCancellation.Token).ConfigureAwait(false);
                    }
                    finally
                    {
                        concurrencyGate.Release();
                    }
                }, _operationCancellation.Token));
            }

            while (manifestItemTasks.Count > 0)
            {
                _operationCancellation.Token.ThrowIfCancellationRequested();

                var completedTask = await Task.WhenAny(manifestItemTasks).ConfigureAwait(true);
                manifestItemTasks.Remove(completedTask);

                var outcome = await completedTask.ConfigureAwait(true);
                manifestItems[outcome.RowIndex] = outcome.ManifestItem;

                outcome.Row.ArchiveRelativePath = outcome.ManifestItem.ArchiveRelativePath;
                outcome.Row.ArchiveLengthBytes = outcome.ArchiveLengthBytes;
                outcome.Row.OriginalLocation = outcome.Row.FullPath;
                outcome.Row.Status = outcome.Status;
                outcome.Row.Message = outcome.Message;
                filesGrid.Refresh();

                processed++;
                UpdateOverallProgressUi(processed, rowSnapshot.Count, $"Archived {processed:N0}/{rowSnapshot.Count:N0} item(s).");
                UpdateCurrentItemProgressUi(outcome.Row.RelativePath, outcome.Status, false);
                UpdateStatus($"{outcome.Status}: {outcome.Row.RelativePath}");
            }

            var manifest = BuildManifest(manifestItems.Where(item => item is not null).Select(item => item!).ToList(), packageId, roots.SourceRoot.Value);
            var manifestJson = JsonSerializer.Serialize(manifest, ManifestJsonOptions);
            await File.WriteAllTextAsync(stagedManifestPath, manifestJson, Encoding.UTF8, _operationCancellation.Token).ConfigureAwait(true);

            if (File.Exists(packageTempPath))
            {
                File.Delete(packageTempPath);
            }

            TarFile.CreateFromDirectory(packageManifestRoot, tarPath, includeBaseDirectory: false);
            using (var sourceStream = File.OpenRead(tarPath))
            using (var destinationStream = File.Create(packageTempPath))
            using (var gzip = new GZipStream(destinationStream, CompressionLevel.Optimal))
            {
                await sourceStream.CopyToAsync(gzip, _operationCancellation.Token).ConfigureAwait(true);
            }

            if (File.Exists(packagePath))
            {
                throw new IOException("Destination package already exists. Please rerun to generate a unique timestamp.");
            }

            File.Move(packageTempPath, packagePath, overwrite: false);
            UpdateStatus($"Created package: {packagePath}");
            MessageBox.Show(
                this,
                $"Package created.{Environment.NewLine}{packagePath}",
                "Archive complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Archive cancelled.");
        }
        catch (Exception ex)
        {
            _operationCancellation?.Cancel();
            MessageBox.Show(this, ex.Message, "Archive failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Archive failed: {ex.Message}");
        }
        finally
        {
            _operationCancellation?.Dispose();
            _operationCancellation = null;
            SetBusy(false);

            if (File.Exists(packageTempPath))
            {
                File.Delete(packageTempPath);
            }

            if (Directory.Exists(packageManifestRoot))
            {
                Directory.Delete(packageManifestRoot, recursive: true);
            }

            if (File.Exists(tarPath))
            {
                File.Delete(tarPath);
            }

            ResetProgressUi();
            archiveButton.Enabled = _rows.Count > 0;
        }
    }

    private async Task LoadPackageAsync(string packagePath)
    {
        if (!File.Exists(packagePath))
        {
            MessageBox.Show(this, "Selected package file does not exist.", "Open archive", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _operationCancellation = new CancellationTokenSource();
        SetBusy(true);

        try
        {
            InitializeProgressUi(1, "Loading package");
            _rows.Clear();
            if (!string.IsNullOrWhiteSpace(_loadedPackageStagingRoot) && Directory.Exists(_loadedPackageStagingRoot))
            {
                Directory.Delete(_loadedPackageStagingRoot, recursive: true);
            }

            _loadedPackageStagingRoot = Path.Combine(
                Path.GetTempPath(),
                "WavCrusher",
                "OpenPackage",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_loadedPackageStagingRoot);

            var tarPath = Path.Combine(_loadedPackageStagingRoot, "package.tar");
            using (var packageStream = File.OpenRead(packagePath))
            using (var gzip = new GZipStream(packageStream, CompressionMode.Decompress))
            using (var tarOutput = File.Create(tarPath))
            {
                await gzip.CopyToAsync(tarOutput, _operationCancellation.Token).ConfigureAwait(true);
            }

            TarFile.ExtractToDirectory(tarPath, _loadedPackageStagingRoot, overwriteFiles: true);

            var manifestPath = ResolveManifestPath(_loadedPackageStagingRoot);
            if (!File.Exists(manifestPath))
            {
                throw new InvalidOperationException("Manifest file is missing from package.");
            }

            var manifestText = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8, _operationCancellation.Token).ConfigureAwait(true);
            var manifest = JsonSerializer.Deserialize<TarballArchiveManifest>(manifestText, ManifestJsonOptions);
            if (manifest is null)
            {
                throw new InvalidOperationException("Could not parse package manifest.");
            }

            ValidateManifest(manifest);
            _loadedManifest = manifest;
            _loadedPackagePath = packagePath;

            var verified = manifest.Items.Count(item => string.Equals(item.Status, ArchiveCompletedStatus, StringComparison.OrdinalIgnoreCase));
            packageSummaryLabel.Text =
                $"Format {manifest.Format} | Created {manifest.CreatedUtc} | Items {manifest.Items.Count}, Verified {verified}";

            foreach (var item in manifest.Items)
            {
                _rows.Add(new ArchiveRow(
                    Guid.NewGuid(),
                    item.SourceRelativePath,
                    item.SourceAbsolutePathHint,
                    item.SourceLengthBytes,
                    item.Status,
                    item.Message,
                    item.SourceAbsolutePathHint,
                    item.ArchiveRelativePath,
                    item.ArchiveLengthBytes));
            }

            ResetProgressUi();
            restoreButton.Enabled = _rows.Count > 0;
            UpdateStatus($"Loaded package with {manifest.Items.Count:N0} item(s).");
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Open package cancelled.");
        }
        catch (Exception ex)
        {
            _rows.Clear();
            _loadedManifest = null;
            _loadedPackagePath = string.Empty;
            packageSummaryLabel.Text = "No package loaded.";
            MessageBox.Show(this, ex.Message, "Open archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Open package failed: {ex.Message}");
        }
        finally
        {
            _operationCancellation?.Dispose();
            _operationCancellation = null;
            SetBusy(false);
            restoreButton.Enabled = _loadedManifest is not null && _rows.Count > 0;
        }
    }

    private async Task RunRestoreAsync()
    {
        if (_loadedManifest is null)
        {
            MessageBox.Show(this, "Open a valid package first.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!string.Equals(_loadedPackagePath, packagePathTextBox.Text, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "Package path changed. Re-open the package before restoring.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var mode = restoreToOriginalRadioButton.Checked ? RestoreMode.OriginalLocations : RestoreMode.RestoreFolder;
        if (mode == RestoreMode.OriginalLocations)
        {
            var confirm = MessageBox.Show(
                this,
                "Original-location restore can place files anywhere, including non-selected folders. Continue?",
                "Restore to original paths",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes)
            {
                return;
            }
        }

        string restoreRoot = string.Empty;
        if (mode == RestoreMode.RestoreFolder &&
            !TryGetOrCreateDirectory(restoreRootTextBox.Text, "restore root", out restoreRoot))
        {
            return;
        }

        if (mode == RestoreMode.RestoreFolder && !Directory.Exists(restoreRoot))
        {
            return;
        }

        var confirmed = MessageBox.Show(
            this,
            "Only items marked 'Verified' in the manifest will be restored. Continue?",
            "Restore",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (confirmed != DialogResult.Yes)
        {
            return;
        }

        var restoreStaging = Path.Combine(_loadedPackageStagingRoot, "restore", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(restoreStaging);

        _operationCancellation = new CancellationTokenSource();
        SetBusy(true);
        try
        {
            InitializeProgressUi(_loadedManifest.Items.Count, mode == RestoreMode.OriginalLocations ? "Restore progress (original locations)" : "Restore progress");
            var restoredCount = 0;
            var verifiedCount = 0;
            var conflictCount = 0;
            var failedCount = 0;
            var completedItems = 0;

            foreach (var item in _loadedManifest.Items)
            {
                _operationCancellation.Token.ThrowIfCancellationRequested();

                var row = _rows.FirstOrDefault(x => string.Equals(x.RelativePath, item.SourceRelativePath, StringComparison.OrdinalIgnoreCase));
                if (row is null)
                {
                    continue;
                }

                row.Status = ArchiveSkippedStatus;
                row.Message = "Preparing restore";
                filesGrid.Refresh();
                UpdateCurrentItemProgressUi(row.RelativePath, "Preparing restore", true);

                if (!IsVerifiedManifestItem(item))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Item is not verified in manifest.";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    UpdateStatus($"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                if (!TarballPackagePaths.LooksLikeSafeArchiveRelativePath(item.ArchiveRelativePath))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Unsafe manifest archive path.";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                if (!string.Equals(item.SourceSha256.Algorithm, "sha256", StringComparison.OrdinalIgnoreCase))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Unsupported source hash algorithm.";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                var archiveEntryPath = ResolveArchiveEntryPath(_loadedPackageStagingRoot, item.ArchiveRelativePath);

                if (!File.Exists(archiveEntryPath))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = $"Compressed file missing from package: {item.ArchiveRelativePath}";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                var restoreTarget = mode == RestoreMode.OriginalLocations
                    ? item.SourceAbsolutePathHint
                    : CombineRestoreTarget(restoreRoot, item.SourceRelativePath);
                if (!TryValidateRestoreTarget(mode, restoreRoot, item.SourceRelativePath, restoreTarget, row, out var normalizedTarget))
                {
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                if (File.Exists(normalizedTarget))
                {
                    row.Status = ArchiveConflictStatus;
                    row.Message = "Target file already exists.";
                    conflictCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                var tempRestorePath = Path.Combine(restoreStaging, $"{Path.GetFileName(archiveEntryPath)}.{Guid.NewGuid():N}.wav");
                UpdateCurrentItemProgressUi(row.RelativePath, "Decoding restore", true);
                var decodeResult = await RunWvUnpackAsync(archiveEntryPath, tempRestorePath, _operationCancellation.Token).ConfigureAwait(true);
                if (!decodeResult.Succeeded || decodeResult.ExitCode != 0)
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Restore decode failed.";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                if (!File.Exists(tempRestorePath))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Restore file was not created.";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                UpdateCurrentItemProgressUi(row.RelativePath, "Verifying restored file", true);
                var restoredHash = await ComputeSha256Async(tempRestorePath, _operationCancellation.Token).ConfigureAwait(true);
                var restoredLength = new FileInfo(tempRestorePath).Length;
                if (restoredLength != item.SourceLengthBytes || !string.Equals(restoredHash, item.SourceSha256.Hex, StringComparison.OrdinalIgnoreCase))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Restored content does not match source hash.";
                    failedCount++;
                    completedItems++;
                    UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restore {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                    continue;
                }

                UpdateCurrentItemProgressUi(row.RelativePath, "Publishing restored file", true);
                Directory.CreateDirectory(Path.GetDirectoryName(normalizedTarget)!);
                File.Copy(tempRestorePath, normalizedTarget, overwrite: false);

                row.Status = ArchiveCompletedStatus;
                row.Message = $"Restored to {normalizedTarget}";
                restoredCount++;
                verifiedCount++;
                completedItems++;
                UpdateOverallProgressUi(completedItems, _loadedManifest.Items.Count, $"Restored {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                UpdateStatus($"Restored {completedItems:N0}/{_loadedManifest.Items.Count:N0}...");
                filesGrid.Refresh();
            }

            var hasFailures = failedCount + conflictCount > 0;
            var successMessage =
                $"Restore complete.{Environment.NewLine}" +
                $"Verified restored: {verifiedCount:N0}{Environment.NewLine}" +
                $"Conflicts: {conflictCount:N0}{Environment.NewLine}" +
                $"Failed: {failedCount:N0}{Environment.NewLine}" +
                $"Created: {restoredCount:N0}";
            MessageBox.Show(
                this,
                hasFailures ? $"{successMessage}{Environment.NewLine}{Environment.NewLine}Completed with issues." : successMessage,
                "Restore",
                MessageBoxButtons.OK,
                hasFailures ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            UpdateStatus(hasFailures ? "Restore completed with issues." : "Restore completed.");
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Restore cancelled.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Restore failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Restore failed: {ex.Message}");
        }
        finally
        {
            if (Directory.Exists(restoreStaging))
            {
                Directory.Delete(restoreStaging, recursive: true);
            }

            ResetProgressUi();
            _operationCancellation?.Dispose();
            _operationCancellation = null;
            SetBusy(false);
            restoreButton.Enabled = _loadedManifest is not null;
        }
    }

    private async Task<ProcessOutcome> RunWvUnpackAsync(
        string archivePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        var start = DateTime.UtcNow;
        var workingDirectory = Path.GetDirectoryName(archivePath) ?? Path.GetTempPath();
        var startInfo = new ProcessStartInfo
        {
            FileName = _wvunpackPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };

        if (!File.Exists(startInfo.FileName))
        {
            return new ProcessOutcome(-1, false, "wvunpack.exe is missing.", TimeSpan.Zero);
        }

        startInfo.ArgumentList.Add(archivePath);
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(destinationPath);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdOut = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErr = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKillProcessTree(process);
            throw;
        }

        var diagnostics = string.Concat(await stdOut.ConfigureAwait(false), await stdErr.ConfigureAwait(false));
        var succeeded = process.ExitCode == 0 && File.Exists(destinationPath);
        return new ProcessOutcome(process.ExitCode, succeeded, diagnostics, DateTime.UtcNow - start);
    }

    private static bool IsVerifiedManifestItem(TarballManifestItem item) =>
        string.Equals(item.Status, ArchiveCompletedStatus, StringComparison.OrdinalIgnoreCase)
        && item.ArchiveLengthBytes > 0
        && item.ArchiveSha256 is not null;

    private static string CombineRestoreTarget(string restoreRoot, string sourceRelativePath)
    {
        var validatedRelative = ValidatedRelativePath.Create(sourceRelativePath.Replace('\\', '/'));
        return Path.Combine(restoreRoot, validatedRelative.Value.Replace('/', Path.DirectorySeparatorChar));
    }

    private static bool TryValidateRestoreTarget(
        RestoreMode mode,
        string restoreRoot,
        string sourceRelativePath,
        string target,
        ArchiveRow row,
        out string normalizedTarget)
    {
        normalizedTarget = string.Empty;

        try
        {
            if (mode == RestoreMode.RestoreFolder)
            {
                var validated = ValidatedRelativePath.Create(sourceRelativePath.Replace('\\', '/'));
                if (!Path.IsPathFullyQualified(restoreRoot))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Restore root is not absolute.";
                    return false;
                }

                normalizedTarget = Path.GetFullPath(Path.Combine(restoreRoot, validated.Value.Replace('/', Path.DirectorySeparatorChar)));
                var normalizedRoot = Path.GetFullPath(restoreRoot);
                var relativeToRoot = Path.GetRelativePath(normalizedRoot, normalizedTarget);
                if (relativeToRoot.StartsWith("..", StringComparison.Ordinal) ||
                    Path.IsPathFullyQualified(relativeToRoot))
                {
                    row.Status = ArchiveFailedStatus;
                    row.Message = "Restore target escapes restore root.";
                    return false;
                }

                return true;
            }

            if (!Path.IsPathFullyQualified(target))
            {
                row.Status = ArchiveFailedStatus;
                row.Message = "Manifest absolute target is invalid.";
                return false;
            }

            normalizedTarget = Path.GetFullPath(target);
            return true;
        }
        catch (ArgumentException)
        {
            row.Status = ArchiveFailedStatus;
            row.Message = "Manifest path is invalid.";
            return false;
        }
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            128 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static TarballArchiveManifest BuildManifest(IReadOnlyList<TarballManifestItem> items, string packageId, string sourceRootHint)
    {
        var verifiedCount = items.Count(item => string.Equals(item.Status, ArchiveCompletedStatus, StringComparison.OrdinalIgnoreCase));
        var failedCount = items.Count - verifiedCount;

        var summary = new TarballManifestSummary(
            ItemCount: items.Count,
            VerifiedCount: verifiedCount,
            FailedCount: failedCount,
            ConflictCount: 0);

        return new TarballArchiveManifest(
            TarballPackagePaths.ManifestFormat,
            "1.0",
            DateTime.UtcNow.ToString("O"),
            packageId,
            sourceRootHint,
            summary,
            items);
    }

    private static bool TryGetExistingDirectory(string path, string label, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show($"Please choose a {label}.", "Missing directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        normalizedPath = Path.GetFullPath(path);
        if (!Directory.Exists(normalizedPath))
        {
            MessageBox.Show($"The {label} does not exist: {normalizedPath}", "Missing directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private static bool TryGetOrCreateDirectory(string path, string label, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show($"Please choose a {label}.", "Missing directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        normalizedPath = Path.GetFullPath(path);
        Directory.CreateDirectory(normalizedPath);
        return true;
    }

    private static string ResolveManifestPath(string packageStagingRoot)
    {
        foreach (var candidate in ManifestFileNameCandidates)
        {
            var exactPath = Path.Combine(packageStagingRoot, candidate);
            if (File.Exists(exactPath))
            {
                return exactPath;
            }
        }

        foreach (var candidate in Directory.EnumerateFiles(packageStagingRoot, "*.json", SearchOption.AllDirectories))
        {
            if (IsLikelyTarballManifest(candidate))
            {
                return candidate;
            }
        }

        return Path.Combine(packageStagingRoot, TarballPackagePaths.ManifestFileName);
    }

    private static string ResolveArchiveEntryPath(string packageStagingRoot, string archiveRelativePath)
    {
        var normalizedRelativePath = archiveRelativePath.Replace('/', Path.DirectorySeparatorChar);
        var exactPath = Path.Combine(packageStagingRoot, normalizedRelativePath);
        if (File.Exists(exactPath))
        {
            return exactPath;
        }

        var expectedTail = Path.DirectorySeparatorChar + normalizedRelativePath;
        foreach (var candidate in Directory.EnumerateFiles(packageStagingRoot, Path.GetFileName(normalizedRelativePath), SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(candidate);
            if (fullPath.EndsWith(expectedTail, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }
        }

        return exactPath;
    }

    private static bool IsLikelyTarballManifest(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var document = JsonDocument.Parse(stream);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!document.RootElement.TryGetProperty("format", out var formatElement))
            {
                return false;
            }

            if (!string.Equals(formatElement.GetString(), TarballPackagePaths.ManifestFormat, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!document.RootElement.TryGetProperty("version", out _))
            {
                return false;
            }

            if (!document.RootElement.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void ValidateManifest(TarballArchiveManifest manifest)
    {
        if (!string.Equals(manifest.Format, TarballPackagePaths.ManifestFormat, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsupported manifest format.");
        }

        if (!manifest.Version.StartsWith("1.", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsupported manifest version.");
        }

        if (manifest.Items.Count == 0)
        {
            throw new InvalidOperationException("Package manifest contains no items.");
        }

        foreach (var item in manifest.Items)
        {
            if (string.IsNullOrWhiteSpace(item.SourceRelativePath)
                || string.IsNullOrWhiteSpace(item.ArchiveRelativePath)
                || !TarballPackagePaths.LooksLikeSafeArchiveRelativePath(item.ArchiveRelativePath))
            {
                throw new InvalidOperationException("Manifest item has invalid paths.");
            }

            _ = ValidatedRelativePath.Create(item.SourceRelativePath.Replace('\\', '/'));

            if (item.SourceSha256 is null || string.IsNullOrWhiteSpace(item.SourceSha256.Algorithm))
            {
                throw new InvalidOperationException("Manifest item source hash is missing.");
            }
        }
    }

    private static void TryKillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void UpdateModeControls()
    {
        browseRestoreRootButton.Enabled = restoreToFolderRadioButton.Checked;
        restoreRootTextBox.Enabled = restoreToFolderRadioButton.Checked;
        restoreRootTextBox.ReadOnly = !restoreToFolderRadioButton.Checked;
    }

    private void SetBusy(bool busy)
    {
        scanButton.Enabled = !busy;
        archiveButton.Enabled = !busy && _rows.Count > 0;
        restoreButton.Enabled = !busy && _loadedManifest is not null;
        clearButton.Enabled = !busy && HasCurrentJobState();
        browseSourceButton.Enabled = !busy;
        browseOutputButton.Enabled = !busy;
        browsePackageButton.Enabled = !busy;
        browseRestoreRootButton.Enabled = !busy;
        recursiveCheckBox.Enabled = !busy;
        restoreToFolderRadioButton.Enabled = !busy;
        restoreToOriginalRadioButton.Enabled = !busy;
        sourcePathTextBox.Enabled = !busy;
        outputPathTextBox.Enabled = !busy;
        packagePathTextBox.Enabled = !busy;
        restoreRootTextBox.Enabled = !busy && restoreToFolderRadioButton.Checked;
        cancelButton.Enabled = busy;
        filesGrid.Enabled = !busy;
        if (!busy)
        {
            ResetProgressUi();
        }
    }

    private void ClearCurrentJobState()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_loadedPackageStagingRoot) && Directory.Exists(_loadedPackageStagingRoot))
            {
                Directory.Delete(_loadedPackageStagingRoot, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        _loadedPackageStagingRoot = string.Empty;
        _loadedManifest = null;
        _loadedPackagePath = string.Empty;
        _rows.Clear();
        restoreToFolderRadioButton.Checked = true;
        UpdateModeControls();

        sourcePathTextBox.Clear();
        outputPathTextBox.Clear();
        packagePathTextBox.Clear();
        restoreRootTextBox.Clear();
        recursiveCheckBox.Checked = true;
        packageSummaryLabel.Text = "No package loaded.";
        ResetProgressUi();
        UpdateStatus("Job state cleared. Ready for another run.");

        archiveButton.Enabled = false;
        restoreButton.Enabled = false;
        clearButton.Enabled = false;
    }

    private bool HasCurrentJobState() =>
        _loadedManifest is not null
        || _rows.Count > 0
        || !string.IsNullOrWhiteSpace(packagePathTextBox.Text)
        || !string.IsNullOrWhiteSpace(restoreRootTextBox.Text)
        || !string.IsNullOrWhiteSpace(_loadedPackageStagingRoot);

    private void InitializeProgressUi(int totalItems, string summaryText)
    {
        progressSummaryLabel.Text = summaryText;
        itemProgressLabel.Text = "Current item";
        progressBar.Minimum = 0;
        progressBar.Maximum = Math.Max(totalItems, 1);
        progressBar.Value = 0;
        progressBar.Style = ProgressBarStyle.Continuous;
        itemProgressBar.Style = ProgressBarStyle.Marquee;
        itemProgressBar.MarqueeAnimationSpeed = 30;
        itemProgressBar.Value = 0;
        statusLabel.Text = summaryText;
        statusLabel.AccessibleDescription = summaryText;
    }

    private void UpdateOverallProgressUi(int completedItems, int totalItems, string summaryText)
    {
        progressSummaryLabel.Text = summaryText;
        progressBar.Maximum = Math.Max(totalItems, 1);
        progressBar.Value = Math.Min(Math.Max(completedItems, 0), progressBar.Maximum);
    }

    private void UpdateCurrentItemProgressUi(string itemText, string stageText, bool active)
    {
        itemProgressLabel.Text = $"{stageText}: {itemText}";
        itemProgressBar.Style = active ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        itemProgressBar.MarqueeAnimationSpeed = active ? 30 : 0;
    }

    private void ResetProgressUi()
    {
        progressSummaryLabel.Text = "Overall progress";
        itemProgressLabel.Text = "Current item";
        progressBar.Minimum = 0;
        progressBar.Maximum = 1;
        progressBar.Value = 0;
        progressBar.Style = ProgressBarStyle.Blocks;
        itemProgressBar.Minimum = 0;
        itemProgressBar.Maximum = 1;
        itemProgressBar.Value = 0;
        itemProgressBar.Style = ProgressBarStyle.Blocks;
        itemProgressBar.MarqueeAnimationSpeed = 0;
    }

    private void UpdateStatus(string message)
    {
        statusLabel.Text = message;
        statusLabel.AccessibleDescription = message;
    }

    private async Task<ArchiveWorkOutcome> ArchiveRowAsync(
        int rowIndex,
        ArchiveRow row,
        string archivesRoot,
        IProgress<ArchiveItemProgress>? progress,
        CancellationToken cancellationToken)
    {
        var startedUtc = DateTime.UtcNow;
        var rowPackagePath = Path.Combine(
            archivesRoot,
            TarballPackagePaths.GetArchiveEntryPath(row.RelativePath).Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(rowPackagePath)!);

        ArchiveResult result;
        try
        {
            result = await _compressor
                .CompressAsync(new ArchiveRequest(row.Id, row.FullPath, rowPackagePath), progress, cancellationToken)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            row.Status = ArchiveCancelledStatus;
            row.Message = "Canceled by user.";
            throw;
        }
        catch (Exception ex)
        {
            result = new ArchiveResult(
                row.Id,
                row.FullPath,
                rowPackagePath,
                Succeeded: false,
                Status: ArchiveFailedStatus,
                Message: ex.Message,
                SourceSha256: null,
                RestoredSha256: null,
                ArchiveSha256: null);
        }

        if (result.SourceSha256 is null)
        {
            result = result with
            {
                SourceSha256 = await ComputeSha256Async(row.FullPath, cancellationToken).ConfigureAwait(true)
            };
        }

        var sourceSha256 = result.SourceSha256!;
        var archiveLength = File.Exists(rowPackagePath) ? new FileInfo(rowPackagePath).Length : 0;
        var status = result.Status;
        var completedUtc = DateTime.UtcNow;
        var manifestItem = new TarballManifestItem(
            SourceRelativePath: row.RelativePath,
            SourceAbsolutePathHint: row.FullPath,
            SourceLengthBytes: row.LengthBytes,
            SourceSha256: new HashReference("sha256", sourceSha256),
            ArchiveRelativePath: TarballPackagePaths.GetArchiveEntryPath(row.RelativePath),
            ArchiveLengthBytes: archiveLength,
            ArchiveSha256: result.ArchiveSha256 is null ? null : new HashReference("sha256", result.ArchiveSha256),
            RestoredSha256: result.RestoredSha256 is null ? null : new HashReference("sha256", result.RestoredSha256),
            ProfileId: "wavpack-pure-lossless-hh-x6-v1",
            Status: status,
            FailureCode: string.Equals(status, ArchiveCompletedStatus, StringComparison.OrdinalIgnoreCase) ? null : status,
            Message: result.Message,
            StartedUtc: startedUtc.ToString("O"),
            CompletedUtc: completedUtc.ToString("O"));

        return new ArchiveWorkOutcome(
            rowIndex,
            row,
            rowPackagePath,
            archiveLength,
            manifestItem,
            status,
            result.Message);
    }

    private static int GetArchiveWorkerCount(int itemCount) =>
        Math.Max(1, Math.Min(Environment.ProcessorCount, itemCount));

    private sealed class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "About WavCrusher";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(640, 420);

            var titleLabel = new Label
            {
                Text = "WavCrusher",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var authorLabel = new Label
            {
                Text = "Developer: Aryn Mikel Sparks",
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var emailLabel = new Label
            {
                Text = "Email: Aryn.sparks1987@gmail.com",
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var licenseLabel = new Label
            {
                Text = "License: MIT",
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var descriptionLabel = new Label
            {
                Text = "WavCrusher archives WAV files as verified pure-lossless WavPack .wv files and keeps source WAVs intact.",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 44
            };

            var licenseTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Text =
                    "MIT License\r\n\r\n" +
                    "Copyright (c) 2026 WavCrusher contributors\r\n\r\n" +
                    "Permission is hereby granted, free of charge, to any person obtaining a copy\r\n" +
                    "of this software and associated documentation files (the \"Software\"), to deal\r\n" +
                    "in the Software without restriction, including without limitation the rights\r\n" +
                    "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell\r\n" +
                    "copies of the Software, and to permit persons to whom the Software is\r\n" +
                    "furnished to do so, subject to the following conditions:\r\n\r\n" +
                    "The above copyright notice and this permission notice shall be included in all\r\n" +
                    "copies or substantial portions of the Software.\r\n\r\n" +
                    "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\r\n" +
                    "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\r\n" +
                    "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\r\n" +
                    "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\r\n" +
                    "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\r\n" +
                    "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\r\n" +
                    "SOFTWARE."
            };

            var closeButton = new Button
            {
                Text = "Close",
                AutoSize = true,
                Dock = DockStyle.Right,
                DialogResult = DialogResult.OK
            };

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 12, 0, 0),
                Height = 44,
                WrapContents = false
            };
            buttonPanel.Controls.Add(closeButton);

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 12),
                Height = 120
            };
            headerPanel.Controls.Add(licenseLabel);
            headerPanel.Controls.Add(emailLabel);
            headerPanel.Controls.Add(authorLabel);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(descriptionLabel);

            AcceptButton = closeButton;
            CancelButton = closeButton;

            Controls.Add(licenseTextBox);
            Controls.Add(buttonPanel);
            Controls.Add(headerPanel);
        }
    }

    private sealed class ArchiveRow
    {
        public ArchiveRow(
            Guid id,
            string relativePath,
            string fullPath,
            long lengthBytes,
            string status,
            string message,
            string sourceAbsoluteHint,
            string? archiveRelativePath,
            long? archiveLengthBytes)
        {
            Id = id;
            RelativePath = relativePath;
            FullPath = fullPath;
            LengthBytes = lengthBytes;
            Status = status;
            Message = message;
            SourceAbsoluteHint = sourceAbsoluteHint;
            ArchiveRelativePath = archiveRelativePath;
            OriginalLocation = sourceAbsoluteHint;
            ArchiveLengthBytes = archiveLengthBytes;
        }

        public Guid Id { get; private init; }

        public string RelativePath { get; private init; }

        public string FullPath { get; private init; }

        public long LengthBytes { get; private init; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string SourceAbsoluteHint { get; private init; }

        public string OriginalLocation { get; set; }

        public string? ArchiveRelativePath { get; set; }

        public long? ArchiveLengthBytes { get; set; }

        public string CompressionRatioText => ArchiveLengthBytes is long archiveLength
            ? FormatCompressionRatio(archiveLength, LengthBytes)
            : "n/a";

        public static ArchiveRow FromCandidate(WaveFileCandidate candidate) => new(
            candidate.Id,
            candidate.RelativePath,
            candidate.FullPath,
            candidate.LengthBytes,
            "Ready",
            string.Empty,
            candidate.FullPath,
            null,
            null);

        private static string FormatCompressionRatio(long archiveLengthBytes, long sourceLengthBytes)
        {
            if (sourceLengthBytes <= 0)
            {
                return "n/a";
            }

            var ratio = (double)archiveLengthBytes / sourceLengthBytes;
            return $"{ratio:P1}";
        }
    }

    private sealed record ArchiveWorkOutcome(
        int RowIndex,
        ArchiveRow Row,
        string RowPackagePath,
        long ArchiveLengthBytes,
        TarballManifestItem ManifestItem,
        string Status,
        string Message);

    private sealed record ProcessOutcome(int ExitCode, bool Succeeded, string Diagnostics, TimeSpan Duration);
}
