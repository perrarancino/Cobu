using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CoBu.Models;

namespace CoBu.Services
{
    public class StorageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        // Texts longer than this are not kept in RAM / history.json, only in their .txt file
        private const int InlineTextLimit = 4096;

        public string BaseStoragePath { get; }
        public string TextsPath { get; }
        public string ImagesPath { get; }
        private string HistoryFilePath => Path.Combine(BaseStoragePath, "history.json");
        private readonly System.Threading.SemaphoreSlim _saveLock = new(1, 1);

        public ObservableCollection<ClipItem> Items { get; } = new();

        public StorageService()
        {
            BaseStoragePath = AppPaths.StorageRoot;
            TextsPath = Path.Combine(BaseStoragePath, "Texts");
            ImagesPath = Path.Combine(BaseStoragePath, "Images");

            EnsureDirectories();
            LoadHistory();
        }

        private void EnsureDirectories()
        {
            try
            {
                Directory.CreateDirectory(BaseStoragePath);
                Directory.CreateDirectory(TextsPath);
                Directory.CreateDirectory(ImagesPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating storage directories: {ex.Message}");
            }
        }

        public void LoadHistory()
        {
            Items.Clear();
            if (!File.Exists(HistoryFilePath))
                return;

            try
            {
                string json = File.ReadAllText(HistoryFilePath);
                var loaded = JsonSerializer.Deserialize<List<ClipItem>>(json, JsonOptions);
                if (loaded != null)
                {
                    foreach (var item in loaded)
                    {
                        // Verify file existence or fallback
                        if (!string.IsNullOrWhiteSpace(item.RelativeFilePath))
                        {
                            string fullPath = Path.Combine(BaseStoragePath, item.RelativeFilePath);
                            if (item.Type == ClipType.Image)
                            {
                                if (!File.Exists(fullPath))
                                    continue;

                                if (string.IsNullOrEmpty(item.ImageHash))
                                {
                                    try
                                    {
                                        byte[] bytes = File.ReadAllBytes(fullPath);
                                        using var sha = System.Security.Cryptography.SHA256.Create();
                                        item.ImageHash = Convert.ToHexString(sha.ComputeHash(bytes));
                                    }
                                    catch { }
                                }
                            }
                            else if (item.Type == ClipType.Text && item.ContentText.Length > InlineTextLimit && File.Exists(fullPath))
                            {
                                // Long text: keep it on disk only, show a short snippet on the card
                                item.Snippet ??= ClipItem.MakeSnippet(item.ContentText);
                                item.ContentText = string.Empty;
                            }
                        }
                        Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading history: {ex.Message}");
            }
        }

        public async Task SaveHistoryAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(Items.ToList(), JsonOptions);
                await File.WriteAllTextAsync(HistoryFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        public async Task<ClipItem?> SaveTextClipAsync(string text, int maxLimit = 100)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            await _saveLock.WaitAsync();
            try
            {
                // If the latest item has the exact same content, just update timestamp and move to top
                var existing = Items.FirstOrDefault(i => i.Type == ClipType.Text && i.TextEquals(text));
                if (existing != null)
                {
                    existing.Timestamp = DateTime.Now;
                    int oldIndex = Items.IndexOf(existing);
                    if (oldIndex > 0)
                    {
                        Items.Move(oldIndex, 0);
                    }
                    await SaveHistoryAsync();
                    return existing;
                }

                string id = Guid.NewGuid().ToString("N");
                string timeStampStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"text_{timeStampStr}_{id[..6]}.txt";
                string relativePath = Path.Combine("Texts", fileName);
                string fullPath = Path.Combine(BaseStoragePath, relativePath);

                await File.WriteAllTextAsync(fullPath, text);

                string preview = text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();
                if (preview.Length > 200)
                    preview = preview[..200] + "...";

                var fileInfo = new FileInfo(fullPath);
                int lineCount = text.Split('\n').Length;

                var item = new ClipItem
                {
                    Id = id,
                    Type = ClipType.Text,
                    Timestamp = DateTime.Now,
                    ContentText = text.Length <= InlineTextLimit ? text : string.Empty,
                    Snippet = ClipItem.MakeSnippet(text),
                    PreviewText = preview,
                    RelativeFilePath = relativePath,
                    CharCount = text.Length,
                    LineCount = lineCount,
                    FileSize = FormatBytes(fileInfo.Length)
                };

                Items.Insert(0, item);

                if (maxLimit > 0)
                {
                    await EnforceLimitInternalAsync(maxLimit);
                }

                await SaveHistoryAsync();
                return item;
            }
            finally
            {
                _saveLock.Release();
            }
        }

        /// <param name="imageBytes">Already PNG-encoded image (encoded once by the clipboard monitor).</param>
        public async Task<ClipItem?> SaveImageClipAsync(byte[] imageBytes, int pixelWidth, int pixelHeight, int maxLimit = 100)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            await _saveLock.WaitAsync();
            try
            {
                string imageHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(imageBytes));

                // Exact duplicate check: if identical image already exists, move to top without creating duplicate file
                var existing = Items.FirstOrDefault(i => i.Type == ClipType.Image && i.ImageHash == imageHash);
                if (existing != null)
                {
                    existing.Timestamp = DateTime.Now;
                    int oldIndex = Items.IndexOf(existing);
                    if (oldIndex > 0)
                    {
                        Items.Move(oldIndex, 0);
                    }
                    await SaveHistoryAsync();
                    return existing;
                }

                string id = Guid.NewGuid().ToString("N");
                string timeStampStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"img_{timeStampStr}_{id[..6]}.png";
                string relativePath = Path.Combine("Images", fileName);
                string fullPath = Path.Combine(BaseStoragePath, relativePath);

                await File.WriteAllBytesAsync(fullPath, imageBytes);

                var fileInfo = new FileInfo(fullPath);

                var item = new ClipItem
                {
                    Id = id,
                    Type = ClipType.Image,
                    Timestamp = DateTime.Now,
                    RelativeFilePath = relativePath,
                    Dimensions = $"{pixelWidth} × {pixelHeight}",
                    FileSize = FormatBytes(fileInfo.Length),
                    PreviewText = $"PNG {pixelWidth}×{pixelHeight}",
                    ImageHash = imageHash
                };

                Items.Insert(0, item);

                if (maxLimit > 0)
                {
                    await EnforceLimitInternalAsync(maxLimit);
                }

                await SaveHistoryAsync();
                return item;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving image clip: {ex.Message}");
                return null;
            }
            finally
            {
                _saveLock.Release();
            }
        }

        public async Task EnforceLimitAsync(int maxLimit)
        {
            if (maxLimit <= 0)
                return;

            await _saveLock.WaitAsync();
            try
            {
                await EnforceLimitInternalAsync(maxLimit);
                await SaveHistoryAsync();
            }
            finally
            {
                _saveLock.Release();
            }
        }

        private async Task EnforceLimitInternalAsync(int maxLimit)
        {
            if (maxLimit <= 0)
                return;

            while (Items.Count > maxLimit)
            {
                int removeIndex = -1;
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (!Items[i].IsPinned)
                    {
                        removeIndex = i;
                        break;
                    }
                }

                if (removeIndex < 0)
                    break; // All remaining items are pinned

                var toRemove = Items[removeIndex];
                Items.RemoveAt(removeIndex);

                try
                {
                    if (!string.IsNullOrWhiteSpace(toRemove.FullFilePath) && File.Exists(toRemove.FullFilePath))
                    {
                        File.Delete(toRemove.FullFilePath);
                    }
                }
                catch { }
            }
        }

        public async Task DeleteItemAsync(ClipItem item)
        {
            if (Items.Remove(item))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(item.FullFilePath) && File.Exists(item.FullFilePath))
                    {
                        File.Delete(item.FullFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting file: {ex.Message}");
                }

                await SaveHistoryAsync();
            }
        }

        public async Task ClearAllAsync(bool keepPinned = true)
        {
            var toRemove = keepPinned 
                ? Items.Where(i => !i.IsPinned).ToList() 
                : Items.ToList();

            foreach (var item in toRemove)
            {
                Items.Remove(item);
                try
                {
                    if (!string.IsNullOrWhiteSpace(item.FullFilePath) && File.Exists(item.FullFilePath))
                    {
                        File.Delete(item.FullFilePath);
                    }
                }
                catch { }
            }

            await SaveHistoryAsync();
        }

        public void OpenFolderInExplorer()
        {
            try
            {
                EnsureDirectories();
                Process.Start(new ProcessStartInfo
                {
                    FileName = BaseStoragePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening storage folder: {ex.Message}");
            }
        }

        public void OpenFileInDefaultApp(ClipItem item)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(item.FullFilePath) && File.Exists(item.FullFilePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = item.FullFilePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening file: {ex.Message}");
            }
        }

        private static string FormatBytes(long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
                _ => $"{bytes / (1024.0 * 1024.0):F2} MB"
            };
        }
    }
}
