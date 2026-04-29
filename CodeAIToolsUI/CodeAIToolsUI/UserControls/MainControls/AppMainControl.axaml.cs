using System;
using Avalonia;
using System.IO;
using System.Text;
using System.Linq;
using Avalonia.Media;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Avalonia.Controls;
using System.Diagnostics;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CodeAIToolsUI.APIs;
using CodeAIToolsUI.APIs.DTOs;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace CodeAIToolsUI.UserControls.MainControls
{
    public partial class AppMainControl : UserControl
    {
        private static readonly string LibsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Configs.PROJECT_NAME, Configs.LIBS_DIR);

        private CancellationTokenSource? _cts;

        // RowDefinition / ColumnDefinition'lara index üzerinden erişim
        // mainGrid: rowCards=0, rowTerminalSplitter=1, rowTerminal=2
        private RowDefinition RowCards => mainGrid.RowDefinitions[0];
        private RowDefinition RowTerminalSplitter => mainGrid.RowDefinitions[1];
        private RowDefinition RowTerminal => mainGrid.RowDefinitions[2];

        // innerGrid: colFlow=0, colCode=2 | rowCommentSplitter=1, rowComment=2
        private ColumnDefinition ColFlow => innerGrid.ColumnDefinitions[0];
        private ColumnDefinition ColCode => innerGrid.ColumnDefinitions[2];
        private RowDefinition RowCommentSplitter => innerGrid.RowDefinitions[1];
        private RowDefinition RowComment => innerGrid.RowDefinitions[2];

        private GridLength _savedFlowWidth;
        private GridLength _savedCodeWidth;
        private GridLength _savedCardsHeight;
        private GridLength _savedCommentHeight;
        private GridLength _savedTerminalHeight;
        private GridLength _savedCommentSplitterHeight;
        private GridLength _savedTerminalSplitterHeight;

        private bool _flowMaximized = false;
        private bool _codeMaximized = false;
        private bool _workModeActive = false;
        private bool _explanationNeeded = true;
        private bool _terminalMaximized = false;

        public AppMainControl()
        {
            InitializeComponent();
        }

        #region Execute Methods

        private async void ExecuteButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                _cts = new CancellationTokenSource();
                SetExecutionState(true);
                try
                {
                    await ExecuteProcess();
                }
                catch (OperationCanceledException)
                {
                    ShowInformationLine("Execution cancelled");
                }
                catch (Exception ex)
                {
                    ShowErrorLine($"Error: {ex.Message}");
                }
                finally
                {
                    SetExecutionState(false);
                }
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException($"An unexcepted error occured {ex.Message}");
            }
        }

        private async Task ExecuteProcess()
        {
            var codeText = codePage.editor.Document.Text;
            var output = await RunJavaCodeAsync(codeText, _cts!.Token);

            if (!string.IsNullOrEmpty(output))
            {
                string userContent = $"""
                                      <java_code>
                                      {codePage.editor.Document.Text}
                                      </java_code>

                                      <error_output>
                                      {output}
                                      </error_output>
                                      """;
                await ExplainTheError(new AIRequestDto(userContent), _explanationNeeded);
            }

            _explanationNeeded = true;
        }

        private async Task<string> RunJavaCodeAsync(string javaCode, CancellationToken token)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "JavaRunner_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            string className = "Main";
            var match = Regex.Match(javaCode, @"public\s+class\s+(\w+)");
            if (match.Success) className = match.Groups[1].Value;

            string javaFile = Path.Combine(tempDir, className + ".java");

            if (!Directory.Exists(LibsDir))
            {
                Directory.CreateDirectory(LibsDir);
                File.SetAttributes(LibsDir, File.GetAttributes(LibsDir) | FileAttributes.Hidden);
            }

            var jars = Directory.GetFiles(LibsDir, "*.jar");
            string classpath = string.Join(Path.PathSeparator.ToString(), jars);
            string cpFlag = string.IsNullOrEmpty(classpath) ? "" : $"-cp \"{classpath}\"";
            string runCp = string.IsNullOrEmpty(classpath)
                ? $"\"{tempDir}\""
                : $"\"{tempDir}\"{Path.PathSeparator}{classpath}";

            try
            {
                await File.WriteAllTextAsync(javaFile, javaCode, token);

                string compileOutput = await RunProcessAsync("javac", $"{cpFlag} \"{javaFile}\"", tempDir, token);
                if (!string.IsNullOrEmpty(compileOutput))
                {
                    ShowDownloadLink(compileOutput);
                    return $"[Derleme Hatası]\n{compileOutput}";
                }

                return await RunProcessAsync("java", $"-cp {runCp} {className}", tempDir, token);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private async Task<string> RunProcessAsync(string fileName, string arguments,
            string workingDir, CancellationToken token)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            await using var registration = token.Register(() =>
            {
                try
                {
                    if (!process.HasExited) process.Kill(entireProcessTree: true);
                }
                catch
                {
                }
            });

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(token);
            token.ThrowIfCancellationRequested();

            return string.IsNullOrEmpty(error) ? output : error;
        }

        #endregion

        #region Spinner Methods

        private void StartLoading()
        {
            loadingOverlay.IsVisible = true;
        }

        private void StopLoading()
        {
            loadingOverlay.IsVisible = false;
        }

        private void DisableTransformButton()
        {
            transformButton.IsEnabled = false;
            transformButton.Background = new SolidColorBrush(Color.Parse("#4d4d4f"));
        }

        private void EnableTransformButton()
        {
            transformButton.IsEnabled = true;
            transformButton.Background = Brushes.Transparent;
        }

        #endregion

        #region Maximize Methods

        private void BtnMaxFlow_Click(object sender, RoutedEventArgs e)
        {
            if (!_flowMaximized)
            {
                SaveAllSizes();
                ColFlow.Width = new GridLength(1, GridUnitType.Star);
                ColCode.Width = new GridLength(0);
                RowComment.Height = new GridLength(0);
                RowCommentSplitter.Height = new GridLength(0);
                RowTerminal.Height = new GridLength(0);
                RowTerminalSplitter.Height = new GridLength(0);
                RowCards.Height = new GridLength(1, GridUnitType.Star);

                verticalSplitter.IsEnabled = false;
                horizontalSplitter.IsEnabled = false;
                terminalSplitter.IsEnabled = false;

                iconMaxFlow.Text = "❐";
                btnMaxCode.IsEnabled = false;
                btnMaxTerminal.IsEnabled = false;
                btnWorkMode.IsEnabled = false;
                _flowMaximized = true;
            }
            else
            {
                RestoreAllSizes();
                iconMaxFlow.Text = "⛶";
                _flowMaximized = false;
                btnMaxCode.IsEnabled = true;
                btnMaxTerminal.IsEnabled = true;
                btnWorkMode.IsEnabled = true;
            }
        }

        private void BtnMaxCode_Click(object sender, RoutedEventArgs e)
        {
            if (!_codeMaximized)
            {
                SaveAllSizes();
                ColCode.Width = new GridLength(1, GridUnitType.Star);
                ColFlow.Width = new GridLength(0);
                RowComment.Height = new GridLength(0);
                RowCommentSplitter.Height = new GridLength(0);
                RowTerminal.Height = new GridLength(0);
                RowTerminalSplitter.Height = new GridLength(0);
                RowCards.Height = new GridLength(1, GridUnitType.Star);

                verticalSplitter.IsEnabled = false;
                horizontalSplitter.IsEnabled = false;
                terminalSplitter.IsEnabled = false;

                iconMaxCode.Text = "❐";
                btnMaxFlow.IsEnabled = false;
                btnMaxTerminal.IsEnabled = false;
                btnWorkMode.IsEnabled = false;
                _codeMaximized = true;
            }
            else
            {
                RestoreAllSizes();
                iconMaxCode.Text = "⛶";
                _codeMaximized = false;
                btnMaxFlow.IsEnabled = true;
                btnMaxTerminal.IsEnabled = true;
                btnWorkMode.IsEnabled = true;
            }
        }

        private void BtnMaxTerminal_Click(object sender, RoutedEventArgs e)
        {
            if (!_terminalMaximized)
            {
                SaveAllSizes();
                RowCards.Height = new GridLength(0);
                RowTerminalSplitter.Height = new GridLength(0);
                RowTerminal.Height = new GridLength(1, GridUnitType.Star);
                terminalSplitter.IsEnabled = false;

                iconMaxTerminal.Text = "❐";
                btnMaxFlow.IsEnabled = false;
                btnMaxCode.IsEnabled = false;
                btnWorkMode.IsEnabled = false;
                _terminalMaximized = true;
            }
            else
            {
                RestoreAllSizes();
                iconMaxTerminal.Text = "⛶";
                _terminalMaximized = false;
                btnMaxFlow.IsEnabled = true;
                btnMaxCode.IsEnabled = true;
                btnWorkMode.IsEnabled = true;
            }
        }

        private void BtnWorkMode_Click(object sender, RoutedEventArgs e)
        {
            if (!_workModeActive)
            {
                SaveAllSizes();
                ColFlow.Width = new GridLength(1, GridUnitType.Star);
                ColCode.Width = new GridLength(1, GridUnitType.Star);
                RowComment.Height = new GridLength(0);
                RowCommentSplitter.Height = new GridLength(0);
                RowCards.Height = new GridLength(1, GridUnitType.Star);
                horizontalSplitter.IsEnabled = false;

                terminalContentBorder.IsVisible = false;
                TerminalHead.IsVisible = false;
                RowTerminal.Height = GridLength.Auto;
                RowTerminalSplitter.Height = new GridLength(0);
                terminalSplitter.IsEnabled = false;

                iconWorkMode.Text = "❐";
                btnMaxTerminal.IsEnabled = false;
                _workModeActive = true;
            }
            else
            {
                terminalContentBorder.IsVisible = true;
                TerminalHead.IsVisible = true;
                RestoreAllSizes();

                iconWorkMode.Text = "⊞";
                _workModeActive = false;
                btnMaxTerminal.IsEnabled = true;
            }
        }

        private void SaveAllSizes()
        {
            _savedFlowWidth = ColFlow.Width;
            _savedCodeWidth = ColCode.Width;
            _savedCommentHeight = RowComment.Height;
            _savedCommentSplitterHeight = RowCommentSplitter.Height;
            _savedCardsHeight = RowCards.Height;
            _savedTerminalHeight = RowTerminal.Height;
            _savedTerminalSplitterHeight = RowTerminalSplitter.Height;
        }

        private void RestoreAllSizes()
        {
            ColFlow.Width = _savedFlowWidth;
            ColCode.Width = _savedCodeWidth;
            RowComment.Height = _savedCommentHeight;
            RowCommentSplitter.Height = _savedCommentSplitterHeight;
            RowCards.Height = _savedCardsHeight;
            RowTerminal.Height = _savedTerminalHeight;
            RowTerminalSplitter.Height = _savedTerminalSplitterHeight;

            verticalSplitter.IsEnabled = true;
            horizontalSplitter.IsEnabled = true;
            terminalSplitter.IsEnabled = true;
        }

        #endregion

        #region Terminal Methods

        private void ShowErrorLine(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                TerminalLines.Children.Add(new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.Red,
                    TextWrapping = TextWrapping.Wrap
                });
            });
        }

        private void ShowInformationLine(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                TerminalLines.Children.Add(new TextBlock
                {
                    Text = message,
                    Foreground = new SolidColorBrush(Color.Parse("#94a3b8")),
                    TextWrapping = TextWrapping.Wrap
                });
            });
        }

        private void ShowLinkLine(string text, string url)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var linkBtn = new Button
                {
                    Content = text,
                    Foreground = Brushes.DodgerBlue,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                linkBtn.Click += async (_, _) =>
                {
                    try
                    {
                        Directory.CreateDirectory(LibsDir);
                        string jarName = Path.GetFileName(url);
                        string jarPath = Path.Combine(LibsDir, jarName);

                        if (!File.Exists(jarPath))
                        {
                            ShowInformationLine("Downloading...");
                            using var client = new HttpClient();
                            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                            var bytes = await client.GetByteArrayAsync(url);
                            await File.WriteAllBytesAsync(jarPath, bytes);
                            ShowInformationLine("Downloaded. Please run the program again.");
                        }
                        else
                        {
                            ShowInformationLine("Already exists. Please run the program again.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowInformationLine($"Download error: {ex.Message}");
                    }
                };
                TerminalLines.Children.Add(linkBtn);
            });
        }

        private void ClearTerminal_Clicked(object sender, RoutedEventArgs e)
        {
            TerminalLines.Children.Clear();
        }

        #endregion

        #region AI Helper Methods

        private static string TransformManuelly(string assistantContent)
        {
            assistantContent = Regex.Replace(assistantContent, @"^```[a-zA-Z]*\n?", "", RegexOptions.Multiline);
            assistantContent = assistantContent.Replace("```", "");
            assistantContent =
                Regex.Replace(assistantContent, @"DEF:\s+(\w+)\s*\((.*?)\)", "public static void $1($2)");
            assistantContent = Regex.Replace(assistantContent, @"RETURN:\s+(.+)", "return $1;");
            assistantContent = Regex.Replace(assistantContent, @"RETURN:\s*$", "}", RegexOptions.Multiline);
            assistantContent = Regex.Replace(assistantContent, @"FOR:\s+(\w+)\s+from\s+(\d+)\s+to\s+(\d+)",
                "for (int $1 = $2; $1 <= $3; $1++)");
            assistantContent = Regex.Replace(assistantContent, @"^\s*NEXT\s*$", "}", RegexOptions.Multiline);
            assistantContent = Regex.Replace(assistantContent, @"WHILE:\s+(.+)", "while ($1)");
            assistantContent = Regex.Replace(assistantContent, @"^\s*STOP\s*$", "}", RegexOptions.Multiline);
            assistantContent = Regex.Replace(assistantContent, @"UNTIL:\s+(.+)", "while (!($1))");
            assistantContent = Regex.Replace(assistantContent, @"^\s*DONE\s*$", "}", RegexOptions.Multiline);
            assistantContent = Regex.Replace(assistantContent, @"IF:\s+(.+)", "if ($1)");
            assistantContent = Regex.Replace(assistantContent, @"ELIF:\s+(.+)", "else if ($1)");
            assistantContent = Regex.Replace(assistantContent, @"^\s*ELSE\s*$", "else", RegexOptions.Multiline);
            assistantContent = Regex.Replace(assistantContent, @"^\s*END\s*$", "}", RegexOptions.Multiline);
            assistantContent =
                Regex.Replace(assistantContent, @"^\s*EXIT\s*$", "System.exit(0);", RegexOptions.Multiline);
            assistantContent = Regex.Replace(assistantContent, @"^\s*MAIN\s*$",
                "public static void main(String[] args) {", RegexOptions.Multiline);
            return assistantContent.Trim();
        }

        private bool IsUserSubscribed()
        {
            var user = RequestManager.ActiveUserDto;
            return user?.u_is_subscribed == true && 
                   user.u_subscription_end > DateTime.Now &&
                   (user.u_subscription_plan?.ToLower() == "premium" || 
                    user.u_subscription_plan?.ToLower() == "pro");
        }

        private async Task HandleAiResponse(AIRequestDto aiRequestDto, String language)
        {
            // Check if user is subscribed for Python/Java
            if ((language == "Python" || language == "Java") && !IsUserSubscribed())
            {
                ShowErrorLine($"{language} requires a premium subscription. Click 'Abonelik' in the menu to upgrade.");
                return;
            }

            aiRequestDto.languageToParse = language;
            string json = JsonConvert.SerializeObject(aiRequestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(ApiEndpoints.AI_TRANS_API, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonConvert.DeserializeObject<AIResponseDto>(responseBody);
                string? assistantContent = aiResponse?.GetAssistantContent();

                if (string.IsNullOrEmpty(assistantContent))
                {
                    codePage.editor.Document.Text = "AI response is empty.";
                    codePage.editor.Foreground = Brushes.Red;
                }
                else
                {
                    codePage.editor.Document.Text = TransformManuelly(assistantContent);
                }
            }
            else
            {
                codePage.editor.Document.Text = $"AI request failed: {response.StatusCode}";
            }
        }

        private async Task ExplainTheError(AIRequestDto aiRequestDto, bool needed)
        {
            if (!needed) return;

            ShowErrorLine("The code encountered an error; AI is now examining...");

            string json = JsonConvert.SerializeObject(aiRequestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(ApiEndpoints.AI_EXPL_API, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonConvert.DeserializeObject<AIResponseDto>(responseBody);
                string? assistantContent = aiResponse?.GetAssistantContent();
                ShowErrorLine(string.IsNullOrEmpty(assistantContent) ? "AI response is empty." : assistantContent);
            }
        }

        public async void TransformButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var flowText = flowPage.editor.Document.Text;
                var language = "Java";
                if (string.IsNullOrEmpty(flowText)) return;

                try
                {
                    StartLoading();
                    DisableTransformButton();
                    await HandleAiResponse(new AIRequestDto(flowText), language);
                }
                catch (Exception ex)
                {
                    codePage.editor.Document.Text = $"Error: {ex.Message}";
                    codePage.editor.Foreground = Brushes.Red;
                }
                finally
                {
                    StopLoading();
                    EnableTransformButton();
                }
            }
            catch (Exception ex)
            {
                await GeneralRoutines.ShowException(
                    $"Transform error occured. AI could not transform your flow. {ex.Message}");
            }
        }

        #endregion

        #region Unclassified Methods

        private void AppRunningStyle(bool mode)
        {
            if (mode)
            {
                executeButton.IsEnabled = false;
                executeButton.Background = new SolidColorBrush(Color.Parse("#75d990"));
                executeIcon.Foreground = Brushes.Black;
            }
            else
            {
                executeButton.IsEnabled = true;
                executeButton.Background = Brushes.Transparent;
                executeIcon.Foreground = new SolidColorBrush(Color.Parse("#94a3b8"));
            }
        }

        private void StopButton_Clicked(object sender, RoutedEventArgs e) => _cts?.Cancel();

        private void SetExecutionState(bool isRunning)
        {
            AppRunningStyle(isRunning);
            executeButton.IsEnabled = !isRunning;
            stopButton.IsEnabled = isRunning;

            if (isRunning) DisableTransformButton();
            else EnableTransformButton();

            stopIcon.Foreground = isRunning
                ? new SolidColorBrush(Color.Parse("#e05555"))
                : new SolidColorBrush(Color.Parse("#94a3b8"));
        }

        #endregion

        #region Maven Libs Download Methods

        private void ShowDownloadLink(string compileError)
        {
            var matches = Regex.Matches(compileError, @"package ([\w\.]+) does not exist");
            foreach (Match m in matches)
            {
                string package = m.Groups[1].Value;
                ShowInformationLine($"Missing package detected: {package}. Searching Maven Central...");
                _ = ResolveAndShowLink(package);
                _explanationNeeded = false;
            }
        }

        private async Task ResolveAndShowLink(string package)
        {
            try
            {
                string jarUrl = await FindJarUrlAsync(package);
                ShowInformationLine($"Required package: {package}");
                ShowInformationLine($"After downloading, place the JAR file into: {LibsDir}");
                ShowLinkLine("Download →", jarUrl);
                _explanationNeeded = false;
            }
            catch (Exception ex)
            {
                ShowInformationLine($"Could not resolve package {package}: {ex.Message}");
                ShowInformationLine($"Search manually: https://central.sonatype.com/search?q={package}");
                _explanationNeeded = false;
            }
        }

        private async Task<string> FindJarUrlAsync(string package)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            var groupIdCandidates = new List<string>
            {
                string.Join(".", package.Split('.').Take(3)),
                string.Join(".", package.Split('.').Take(2)),
                package
            };

            foreach (var groupId in groupIdCandidates.Distinct())
            {
                string url = $"https://search.maven.org/solrsearch/select?q=g:\"{groupId}\"&rows=1&wt=json";
                var result = await TrySearchMavenAsync(client, url);
                if (result != null) return result;
            }

            string keywordUrl = $"https://search.maven.org/solrsearch/select?q={package}&rows=1&wt=json";
            var keywordResult = await TrySearchMavenAsync(client, keywordUrl);
            if (keywordResult != null) return keywordResult;

            throw new Exception("Package not found on Maven Central.");
        }

        private async Task<string?> TrySearchMavenAsync(HttpClient client, string searchUrl)
        {
            try
            {
                var response = await client.GetStringAsync(searchUrl);
                var json = JsonDocument.Parse(response);
                var docs = json.RootElement.GetProperty("response").GetProperty("docs");

                if (docs.GetArrayLength() == 0) return null;

                var doc = docs[0];
                string g = doc.GetProperty("g").GetString()!;
                string a = doc.GetProperty("a").GetString()!;
                string v = doc.GetProperty("latestVersion").GetString()!;
                string gPath = g.Replace('.', '/');
                string jarUrl = $"https://repo1.maven.org/maven2/{gPath}/{a}/{v}/{a}-{v}.jar";

                var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, jarUrl));
                return head.IsSuccessStatusCode ? jarUrl : null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}