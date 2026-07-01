using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace TranslatorApp
{
    public class MainForm : Form
    {
        private TextBox txtInputText;
        private TextBox txtTranslatedText;
        private ComboBox cmbFromLanguage;
        private ComboBox cmbToLanguage;
        private Button btnTranslate;
        private Button btnSwapLanguages;
        private Button btnClear;
        private Label lblStatus;
        private Label lblCharCount;
        private readonly HttpClient httpClient;
        private readonly Dictionary<string, string> languages;

        public MainForm()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.mymemory.translated.net/");

            // Словарь языков (4 языка)
            languages = new Dictionary<string, string>
            {
                { "ru", "🇷🇺 Русский" },
                { "en", "🇬🇧 Английский" },
                { "de", "🇩🇪 Немецкий" },
                { "fr", "🇫🇷 Французский" }
            };

            InitializeUI();
            LoadLanguages();
        }

        private void InitializeUI()
        {
            this.Text = "Переводчик иностранных слов";
            this.Size = new System.Drawing.Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = System.Drawing.Color.White;

            // Заголовок
            Label lblTitle = new Label()
            {
                Text = "Переводчик",
                Location = new System.Drawing.Point(20, 10),
                Size = new System.Drawing.Size(650, 35),
                Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            // Панель выбора языков
            Panel panelLanguages = new Panel()
            {
                Location = new System.Drawing.Point(20, 55),
                Size = new System.Drawing.Size(650, 70),
                BackColor = System.Drawing.Color.WhiteSmoke
            };

            // Метка "С какого"
            Label lblFrom = new Label()
            {
                Text = "С:",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(30, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            cmbFromLanguage = new ComboBox()
            {
                Location = new System.Drawing.Point(45, 8),
                Size = new System.Drawing.Size(180, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Кнопка смены языков
            btnSwapLanguages = new Button()
            {
                Text = "⇄",
                Location = new System.Drawing.Point(235, 5),
                Size = new System.Drawing.Size(40, 30),
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                ForeColor = System.Drawing.Color.DarkBlue
            };
            btnSwapLanguages.Click += (s, e) => SwapLanguages();

            // Метка "На какой"
            Label lblTo = new Label()
            {
                Text = "На:",
                Location = new System.Drawing.Point(285, 10),
                Size = new System.Drawing.Size(30, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            cmbToLanguage = new ComboBox()
            {
                Location = new System.Drawing.Point(320, 8),
                Size = new System.Drawing.Size(180, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            panelLanguages.Controls.AddRange(new Control[] {
                lblFrom,
                cmbFromLanguage,
                btnSwapLanguages,
                lblTo,
                cmbToLanguage
            });

            // Метка для ввода
            Label lblInput = new Label()
            {
                Text = "Введите текст для перевода:",
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(200, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            // Поле для ввода текста
            txtInputText = new TextBox()
            {
                Location = new System.Drawing.Point(20, 170),
                Size = new System.Drawing.Size(650, 100),
                Font = new System.Drawing.Font("Segoe UI", 11F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Hello, how are you?"
            };
            txtInputText.TextChanged += (s, e) => UpdateCharCount();

            // Счетчик символов
            lblCharCount = new Label()
            {
                Text = "Символов: 0",
                Location = new System.Drawing.Point(20, 275),
                Size = new System.Drawing.Size(200, 20),
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = System.Drawing.Color.Gray
            };

            // Кнопка "Перевести"
            btnTranslate = new Button()
            {
                Text = "Перевести",
                Location = new System.Drawing.Point(230, 300),
                Size = new System.Drawing.Size(230, 45),
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                ForeColor = System.Drawing.Color.DarkBlue
            };
            btnTranslate.Click += async (s, e) => await TranslateTextAsync();

            // Кнопка "Очистить"
            btnClear = new Button()
            {
                Text = "Очистить",
                Location = new System.Drawing.Point(470, 300),
                Size = new System.Drawing.Size(200, 45),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                BackColor = System.Drawing.Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                ForeColor = System.Drawing.Color.White
            };
            btnClear.Click += (s, e) => ClearAll();

            // Метка для результата
            Label lblOutput = new Label()
            {
                Text = "Результат перевода:",
                Location = new System.Drawing.Point(20, 360),
                Size = new System.Drawing.Size(200, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            // Поле для вывода перевода
            txtTranslatedText = new TextBox()
            {
                Location = new System.Drawing.Point(20, 390),
                Size = new System.Drawing.Size(650, 100),
                Font = new System.Drawing.Font("Segoe UI", 11F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = System.Drawing.Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Здесь появится перевод..."
            };

            // Статусная строка
            lblStatus = new Label()
            {
                Text = "Готов к переводу",
                Location = new System.Drawing.Point(20, 500),
                Size = new System.Drawing.Size(650, 25),
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = System.Drawing.Color.Green,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            // Добавляем все элементы на форму
            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                panelLanguages,
                lblInput,
                txtInputText,
                lblCharCount,
                btnTranslate,
                btnClear,
                lblOutput,
                txtTranslatedText,
                lblStatus
            });
        }

        private void LoadLanguages()
        {
            foreach (var lang in languages)
            {
                cmbFromLanguage.Items.Add(lang.Value);
                cmbToLanguage.Items.Add(lang.Value);
            }

            // Устанавливаем значения по умолчанию (Английский → Русский)
            cmbFromLanguage.SelectedIndex = GetLanguageIndex("en");
            cmbToLanguage.SelectedIndex = GetLanguageIndex("ru");
        }

        private int GetLanguageIndex(string code)
        {
            int index = 0;
            foreach (var lang in languages)
            {
                if (lang.Key == code)
                    return index;
                index++;
            }
            return 0;
        }

        private string GetLanguageCode(string displayName)
        {
            foreach (var lang in languages)
            {
                if (lang.Value == displayName)
                    return lang.Key;
            }
            return "en";
        }

        private void SwapLanguages()
        {
            int fromIndex = cmbFromLanguage.SelectedIndex;
            int toIndex = cmbToLanguage.SelectedIndex;

            cmbFromLanguage.SelectedIndex = toIndex;
            cmbToLanguage.SelectedIndex = fromIndex;

            // Если есть текст, автоматически перевести заново
            if (!string.IsNullOrWhiteSpace(txtInputText.Text))
            {
                _ = TranslateTextAsync();
            }
        }

        private void UpdateCharCount()
        {
            int count = txtInputText.Text.Length;
            lblCharCount.Text = $"Символов: {count}";
            lblCharCount.ForeColor = count > 0 ? System.Drawing.Color.DarkBlue : System.Drawing.Color.Gray;
        }

        private void ClearAll()
        {
            txtInputText.Clear();
            txtTranslatedText.Text = "Здесь появится перевод...";
            txtTranslatedText.ForeColor = System.Drawing.Color.Gray;
            lblStatus.Text = "Поля очищены";
            lblStatus.ForeColor = System.Drawing.Color.Green;
            UpdateCharCount();
        }

        //  Основной метод перевода 
        private async System.Threading.Tasks.Task TranslateTextAsync()
        {
            try
            {
                // Проверка: введен ли текст
                string inputText = txtInputText.Text.Trim();
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    lblStatus.Text = "Введите текст для перевода!";
                    lblStatus.ForeColor = System.Drawing.Color.Orange;
                    txtTranslatedText.Text = "Введите текст для перевода";
                    txtTranslatedText.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Проверка: выбраны ли языки
                if (cmbFromLanguage.SelectedItem == null || cmbToLanguage.SelectedItem == null)
                {
                    lblStatus.Text = "Выберите языки!";
                    lblStatus.ForeColor = System.Drawing.Color.Orange;
                    return;
                }

                // Отключаем кнопки во время перевода
                btnTranslate.Enabled = false;
                btnTranslate.Text = "Перевод...";
                txtTranslatedText.Text = "Перевод выполняется...";
                txtTranslatedText.ForeColor = System.Drawing.Color.Orange;
                lblStatus.Text = "Идет перевод...";
                lblStatus.ForeColor = System.Drawing.Color.Orange;

                // Получаем коды языков
                string fromLang = GetLanguageCode(cmbFromLanguage.SelectedItem.ToString());
                string toLang = GetLanguageCode(cmbToLanguage.SelectedItem.ToString());

                // URL для API MyMemory
                string url = $"get?q={Uri.EscapeDataString(inputText)}&langpair={fromLang}|{toLang}";

                // Выполняем запрос
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                // Настройки для гибкого парсинга JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var translationResult = JsonSerializer.Deserialize<TranslationResponse>(json, options);

                // Проверяем ответ
                if (translationResult != null && translationResult.ResponseData != null)
                {
                    // 200 = успешный ответ
                    if (translationResult.ResponseStatus == 200)
                    {
                        string translatedText = translationResult.ResponseData.TranslatedText;

                        if (string.IsNullOrEmpty(translatedText))
                        {
                            txtTranslatedText.Text = "Пустой ответ от сервера";
                            txtTranslatedText.ForeColor = System.Drawing.Color.Orange;
                            lblStatus.Text = "Сервер вернул пустой ответ";
                            lblStatus.ForeColor = System.Drawing.Color.Orange;
                        }
                        else
                        {
                            txtTranslatedText.Text = translatedText;
                            txtTranslatedText.ForeColor = System.Drawing.Color.DarkGreen;
                            lblStatus.Text = $"✅Перевод выполнен ({fromLang} → {toLang})";
                            lblStatus.ForeColor = System.Drawing.Color.Green;
                        }
                    }
                    else
                    {
                        // Ошибка от API
                        string errorMsg = translationResult.ResponseDetails ?? "Неизвестная ошибка";
                        txtTranslatedText.Text = $"Ошибка: {errorMsg}";
                        txtTranslatedText.ForeColor = System.Drawing.Color.Red;
                        lblStatus.Text = $"Ошибка API (код {translationResult.ResponseStatus}): {errorMsg}";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                    }
                }
                else
                {
                    throw new Exception("Не удалось получить перевод");
                }
            }
            catch (HttpRequestException ex)
            {
                txtTranslatedText.Text = "❌ Ошибка интернет-соединения";
                txtTranslatedText.ForeColor = System.Drawing.Color.Red;
                lblStatus.Text = $"❌ Ошибка сети: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show("Проверьте интернет-соединение!", "Ошибка сети",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (JsonException ex)
            {
                txtTranslatedText.Text = "❌ Ошибка обработки ответа сервера";
                txtTranslatedText.ForeColor = System.Drawing.Color.Red;
                lblStatus.Text = "❌ Ошибка: сервер вернул некорректный ответ";
                lblStatus.ForeColor = System.Drawing.Color.Red;

                // Показываем подробности для отладки
                MessageBox.Show($"Ошибка парсинга JSON:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                txtTranslatedText.Text = $"Ошибка: {ex.Message}";
                txtTranslatedText.ForeColor = System.Drawing.Color.Red;
                lblStatus.Text = $"Ошибка: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                btnTranslate.Enabled = true;
                btnTranslate.Text = "Перевести";
            }
        }

        //  МОДЕЛИ ДАННЫХ 
        private class TranslationResponse
        {
            public ResponseData ResponseData { get; set; }
            public int ResponseStatus { get; set; }       
            public string ResponseDetails { get; set; }
        }

        private class ResponseData
        {
            public string TranslatedText { get; set; }
            public object Match { get; set; }             
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}