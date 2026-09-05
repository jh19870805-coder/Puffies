using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 项目本地化入口。语言选择独立于游戏存档，并在场景切换与运行时切换后刷新现有文本。
/// </summary>
public static class GameLocalization
{
    public const string DefaultLanguageCode = "en-US";

    private const string LanguagePreferenceKey = "Puffies.Language";
    private const string RuntimeObjectName = "GameLocalizationRuntime";

    private static readonly string[] sLanguageCodes =
    {
        "zh-CN", "en-US", "ru-RU", "es-ES", "es-419", "pt-BR", "pt-PT", "de-DE", "ko-KR",
        "fr-FR", "ja-JP", "tr-TR", "zh-TW", "pl-PL", "it-IT", "uk-UA", "vi-VN", "th-TH"
    };

    private static readonly string[] sNativeLanguageNames =
    {
        "简体中文", "English", "Русский", "Español (España)", "Español (Latinoamérica)",
        "Português (Brasil)", "Português (Portugal)", "Deutsch", "한국어", "Français", "日本語",
        "Türkçe", "繁體中文", "Polski", "Italiano", "Українська", "Tiếng Việt", "ไทย"
    };

    private static readonly Dictionary<string, LocalizedEntry> sEntriesByKey =
        new Dictionary<string, LocalizedEntry>(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> sKeysByDisplayedText =
        new Dictionary<string, string>(StringComparer.Ordinal);
    private static readonly Dictionary<int, string> sRuntimeTextKeys =
        new Dictionary<int, string>();

    private static bool sInitialized;
    private static string sCurrentLanguageCode = DefaultLanguageCode;
    private static int sCurrentLanguageIndex = 1;
    private static GameLocalizationRuntime sRuntime;

    public static event Action LanguageChanged;

    public static IReadOnlyList<string> LanguageCodes => sLanguageCodes;
    public static IReadOnlyList<string> NativeLanguageNames => sNativeLanguageNames;
    public static string CurrentLanguageCode
    {
        get
        {
            EnsureInitialized();
            return sCurrentLanguageCode;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInitialized();
        EnsureRuntime();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        sInitialized = false;
        sCurrentLanguageCode = DefaultLanguageCode;
        sCurrentLanguageIndex = 1;
        sRuntime = null;
        sEntriesByKey.Clear();
        sKeysByDisplayedText.Clear();
        sRuntimeTextKeys.Clear();
        LanguageChanged = null;
    }

    public static bool IsSupported(string languageCode)
    {
        return FindLanguageIndex(languageCode) >= 0;
    }

    public static bool SetLanguage(string languageCode)
    {
        EnsureInitialized();
        var languageIndex = FindLanguageIndex(languageCode);
        if (languageIndex < 0)
        {
            Debug.LogWarning($"GameLocalization: unsupported language code {languageCode}.");
            return false;
        }

        if (languageIndex == sCurrentLanguageIndex)
        {
            RefreshSceneTexts();
            return true;
        }

        sCurrentLanguageIndex = languageIndex;
        sCurrentLanguageCode = sLanguageCodes[languageIndex];
        PlayerPrefs.SetString(LanguagePreferenceKey, sCurrentLanguageCode);
        PlayerPrefs.Save();

        RefreshSceneTexts();
        LanguageChanged?.Invoke();
        return true;
    }

    public static string Get(string key)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(key) || !sEntriesByKey.TryGetValue(key, out var entry))
        {
            return key ?? string.Empty;
        }

        var value = entry.Values[sCurrentLanguageIndex];
        if (string.IsNullOrEmpty(value))
        {
            value = entry.Values[1];
        }

        return string.IsNullOrEmpty(value) ? entry.Key : value;
    }

    public static string Format(string key, params object[] arguments)
    {
        var format = Get(key);
        try
        {
            return string.Format(CultureInfo.CurrentCulture, format, arguments ?? Array.Empty<object>());
        }
        catch (FormatException exception)
        {
            Debug.LogWarning($"GameLocalization: invalid format for {key}. {exception.Message}");
            return format;
        }
    }

    public static void RefreshSceneTexts()
    {
        EnsureInitialized();

        var tmpTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        for (var i = 0; i < tmpTexts.Length; i++)
        {
            var label = tmpTexts[i];
            if (label == null || !IsLoadedSceneObject(label.gameObject))
            {
                continue;
            }

            RefreshLabel(label, label.text, value => label.text = value);
        }

        var legacyTexts = Resources.FindObjectsOfTypeAll<Text>();
        for (var i = 0; i < legacyTexts.Length; i++)
        {
            var label = legacyTexts[i];
            if (label == null || !IsLoadedSceneObject(label.gameObject))
            {
                continue;
            }

            RefreshLabel(label, label.text, value => label.text = value);
        }
    }

    internal static void HandleSceneLoaded()
    {
        sRuntimeTextKeys.Clear();
        RefreshSceneTexts();
        EnsureRuntime()?.RefreshAfterSceneInitialization();
    }

    private static void RefreshLabel(UnityEngine.Object label, string currentText, Action<string> setter)
    {
        var instanceId = label.GetInstanceID();
        if (!sRuntimeTextKeys.TryGetValue(instanceId, out var key))
        {
            var normalizedText = NormalizeDisplayedText(currentText);
            if (!sKeysByDisplayedText.TryGetValue(normalizedText, out key))
            {
                return;
            }

            sRuntimeTextKeys[instanceId] = key;
        }

        setter(Get(key));
    }

    private static bool IsLoadedSceneObject(GameObject gameObject)
    {
        return gameObject != null && gameObject.scene.IsValid() && gameObject.scene.isLoaded;
    }

    private static GameLocalizationRuntime EnsureRuntime()
    {
        if (sRuntime != null)
        {
            return sRuntime;
        }

        var runtimeObject = new GameObject(RuntimeObjectName);
        UnityEngine.Object.DontDestroyOnLoad(runtimeObject);
        sRuntime = runtimeObject.AddComponent<GameLocalizationRuntime>();
        return sRuntime;
    }

    private static void EnsureInitialized()
    {
        if (sInitialized)
        {
            return;
        }

        BuildCatalog();
        var savedLanguage = PlayerPrefs.GetString(LanguagePreferenceKey, DefaultLanguageCode);
        var savedIndex = FindLanguageIndex(savedLanguage);
        sCurrentLanguageIndex = savedIndex >= 0 ? savedIndex : 1;
        sCurrentLanguageCode = sLanguageCodes[sCurrentLanguageIndex];
        sInitialized = true;
    }

    private static int FindLanguageIndex(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return -1;
        }

        for (var i = 0; i < sLanguageCodes.Length; i++)
        {
            if (string.Equals(sLanguageCodes[i], languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string NormalizeDisplayedText(string text)
    {
        var lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var normalized = new List<string>(lines.Length);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (!string.IsNullOrEmpty(line))
            {
                normalized.Add(line);
            }
        }

        return string.Join("\n", normalized);
    }

    private static void BuildCatalog()
    {
        sEntriesByKey.Clear();
        sKeysByDisplayedText.Clear();
        var entries = GameLocalizationCatalog.CreateEntries();
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.Key) || entry.Values == null
                || entry.Values.Length != sLanguageCodes.Length)
            {
                Debug.LogError($"GameLocalization: invalid catalog entry at index {i}.");
                continue;
            }

            sEntriesByKey[entry.Key] = entry;
            for (var valueIndex = 0; valueIndex < entry.Values.Length; valueIndex++)
            {
                var displayedText = NormalizeDisplayedText(entry.Values[valueIndex]);
                if (!string.IsNullOrEmpty(displayedText)
                    && !sKeysByDisplayedText.ContainsKey(displayedText))
                {
                    sKeysByDisplayedText.Add(displayedText, entry.Key);
                }
            }
        }
    }

    internal sealed class LocalizedEntry
    {
        public readonly string Key;
        public readonly string[] Values;

        public LocalizedEntry(string key, params string[] values)
        {
            Key = key;
            Values = values;
        }
    }
}

internal sealed class GameLocalizationRuntime : MonoBehaviour
{
    private Coroutine mRefreshCoroutine;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameLocalization.HandleSceneLoaded();
    }

    public void RefreshAfterSceneInitialization()
    {
        if (mRefreshCoroutine != null)
        {
            StopCoroutine(mRefreshCoroutine);
        }

        mRefreshCoroutine = StartCoroutine(RefreshNextFrame());
    }

    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        mRefreshCoroutine = null;
        GameLocalization.RefreshSceneTexts();
    }
}

internal static class GameLocalizationCatalog
{
    private static GameLocalization.LocalizedEntry E(string key, params string[] values)
    {
        return new GameLocalization.LocalizedEntry(key, values);
    }

    public static GameLocalization.LocalizedEntry[] CreateEntries()
    {
        return new[]
        {
            E("common.back", "返回", "Back", "Назад", "Volver", "Volver", "Voltar", "Voltar", "Zurück", "뒤로", "Retour", "戻る", "Geri", "返回", "Wstecz", "Indietro", "Назад", "Quay lại", "กลับ"),
            E("common.yes", "是的", "Yes", "Да", "Sí", "Sí", "Sim", "Sim", "Ja", "예", "Oui", "はい", "Evet", "是", "Tak", "Sì", "Так", "Có", "ใช่"),
            E("common.ok", "确定", "OK", "ОК", "Aceptar", "Aceptar", "OK", "OK", "OK", "확인", "OK", "OK", "Tamam", "確定", "OK", "OK", "Гаразд", "OK", "ตกลง"),
            E("common.pack_count", "卡包数", "Packs", "Наборы", "Paquetes", "Paquetes", "Pacotes", "Pacotes", "Packs", "팩 수", "Packs", "パック数", "Paketler", "卡包數", "Paczki", "Pacchetti", "Набори", "Gói", "แพ็ก"),
            E("common.score", "得分", "Score", "Счёт", "Puntuación", "Puntuación", "Pontuação", "Pontuação", "Punktzahl", "점수", "Score", "スコア", "Puan", "得分", "Wynik", "Punteggio", "Рахунок", "Điểm", "คะแนน"),

            E("main.windowed", "窗口化", "Windowed", "Оконный режим", "Modo ventana", "Modo ventana", "Modo janela", "Modo janela", "Fenstermodus", "창 모드", "Mode fenêtré", "ウィンドウ", "Pencere modu", "視窗模式", "Tryb okienkowy", "Modalità finestra", "Віконний режим", "Chế độ cửa sổ", "โหมดหน้าต่าง"),
            E("main.language", "语言", "Language", "Язык", "Idioma", "Idioma", "Idioma", "Idioma", "Sprache", "언어", "Langue", "言語", "Dil", "語言", "Język", "Lingua", "Мова", "Ngôn ngữ", "ภาษา"),
            E("main.settings", "设置", "Settings", "Настройки", "Ajustes", "Ajustes", "Configurações", "Definições", "Einstellungen", "설정", "Paramètres", "設定", "Ayarlar", "設定", "Ustawienia", "Impostazioni", "Налаштування", "Cài đặt", "การตั้งค่า"),
            E("main.start", "开始", "Start", "Начать", "Empezar", "Empezar", "Iniciar", "Iniciar", "Start", "시작", "Commencer", "スタート", "Başla", "開始", "Start", "Inizia", "Почати", "Bắt đầu", "เริ่ม"),
            E("main.replay_pack", "重玩拼图包", "Replay Puzzle Pack", "Переиграть набор", "Repetir paquete", "Repetir paquete", "Jogar pacote de novo", "Repetir pacote", "Pack erneut spielen", "퍼즐 팩 다시 하기", "Rejouer le pack", "パックをリプレイ", "Paketi yeniden oyna", "重玩拼圖包", "Zagraj paczkę ponownie", "Rigioca pacchetto", "Переграти набір", "Chơi lại gói", "เล่นแพ็กอีกครั้ง"),
            E("main.language_select", "语言选择", "Select Language", "Выбор языка", "Seleccionar idioma", "Seleccionar idioma", "Selecionar idioma", "Selecionar idioma", "Sprache wählen", "언어 선택", "Choisir la langue", "言語を選択", "Dil seç", "選擇語言", "Wybierz język", "Seleziona lingua", "Вибір мови", "Chọn ngôn ngữ", "เลือกภาษา"),
            E("main.usable", "可使用", "Accessibility", "Доступность", "Accesibilidad", "Accesibilidad", "Acessibilidade", "Acessibilidade", "Barrierefreiheit", "접근성", "Accessibilité", "アクセシビリティ", "Erişilebilirlik", "輔助功能", "Ułatwienia dostępu", "Accessibilità", "Доступність", "Trợ năng", "การช่วยการเข้าถึง"),
            E("main.increase_contrast", "提高贴纸的对比度", "Increase sticker contrast", "Повысить контраст наклеек", "Aumentar contraste de pegatinas", "Aumentar contraste de stickers", "Aumentar contraste dos adesivos", "Aumentar contraste dos autocolantes", "Sticker-Kontrast erhöhen", "스티커 대비 높이기", "Augmenter le contraste des autocollants", "ステッカーのコントラストを上げる", "Çıkartma kontrastını artır", "提高貼紙對比度", "Zwiększ kontrast naklejek", "Aumenta contrasto adesivi", "Підвищити контраст наліпок", "Tăng độ tương phản nhãn dán", "เพิ่มความต่างสีของสติกเกอร์"),
            E("main.exit_game", "退出游戏", "Exit Game", "Выйти из игры", "Salir del juego", "Salir del juego", "Sair do jogo", "Sair do jogo", "Spiel beenden", "게임 종료", "Quitter le jeu", "ゲームを終了", "Oyundan çık", "退出遊戲", "Wyjdź z gry", "Esci dal gioco", "Вийти з гри", "Thoát trò chơi", "ออกจากเกม"),
            E("main.menu", "菜单", "Menu", "Меню", "Menú", "Menú", "Menu", "Menu", "Menü", "메뉴", "Menu", "メニュー", "Menü", "選單", "Menu", "Menu", "Меню", "Menu", "เมนู"),
            E("main.music", "音乐", "Music", "Музыка", "Música", "Música", "Música", "Música", "Musik", "음악", "Musique", "音楽", "Müzik", "音樂", "Muzyka", "Musica", "Музика", "Nhạc", "เพลง"),
            E("main.sfx", "音效", "Sound Effects", "Звуковые эффекты", "Efectos de sonido", "Efectos de sonido", "Efeitos sonoros", "Efeitos sonoros", "Soundeffekte", "효과음", "Effets sonores", "効果音", "Ses efektleri", "音效", "Efekty dźwiękowe", "Effetti sonori", "Звукові ефекти", "Hiệu ứng âm thanh", "เสียงเอฟเฟกต์"),
            E("main.high_contrast", "高对比度", "High Contrast", "Высокий контраст", "Alto contraste", "Alto contraste", "Alto contraste", "Alto contraste", "Hoher Kontrast", "고대비", "Contraste élevé", "ハイコントラスト", "Yüksek kontrast", "高對比度", "Wysoki kontrast", "Contrasto elevato", "Високий контраст", "Độ tương phản cao", "ความต่างสีสูง"),
            E("main.continue", "继续", "Continue", "Продолжить", "Continuar", "Continuar", "Continuar", "Continuar", "Weiter", "계속", "Continuer", "続ける", "Devam", "繼續", "Kontynuuj", "Continua", "Продовжити", "Tiếp tục", "ดำเนินการต่อ"),
            E("main.level_outline", "关卡描边", "Board Outline", "Контур поля", "Contorno del tablero", "Contorno del tablero", "Contorno do tabuleiro", "Contorno do tabuleiro", "Brettumriss", "보드 윤곽선", "Contour du plateau", "ボードの輪郭", "Tahta ana hattı", "關卡描邊", "Obrys planszy", "Contorno tavola", "Контур поля", "Viền bảng", "เส้นขอบกระดาน"),
            E("main.sticker_outline", "贴纸描边", "Sticker Outlines", "Контуры наклеек", "Contornos de pegatinas", "Contornos de stickers", "Contornos dos adesivos", "Contornos dos autocolantes", "Sticker-Umrisse", "스티커 윤곽선", "Contours des autocollants", "ステッカーの輪郭", "Çıkartma ana hatları", "貼紙描邊", "Obrysy naklejek", "Contorni adesivi", "Контури наліпок", "Viền nhãn dán", "เส้นขอบสติกเกอร์"),
            E("main.my_saves", "我的保存", "My Saves", "Мои сохранения", "Mis partidas", "Mis partidas", "Meus jogos salvos", "Os meus jogos guardados", "Meine Spielstände", "내 저장", "Mes sauvegardes", "セーブデータ", "Kayıtlarım", "我的存檔", "Moje zapisy", "I miei salvataggi", "Мої збереження", "Bản lưu của tôi", "บันทึกของฉัน"),
            E("main.delete_save", "删除保存位置", "Delete Save Slot", "Удалить сохранение", "Borrar espacio guardado", "Borrar espacio guardado", "Excluir espaço salvo", "Eliminar posição guardada", "Spielstand löschen", "저장 슬롯 삭제", "Supprimer la sauvegarde", "セーブ枠を削除", "Kayıt yuvasını sil", "刪除存檔位置", "Usuń zapis", "Elimina salvataggio", "Видалити збереження", "Xóa ô lưu", "ลบช่องบันทึก"),
            E("main.hint", "提示", "Hint", "Подсказка", "Pista", "Pista", "Dica", "Dica", "Hinweis", "힌트", "Indice", "ヒント", "İpucu", "提示", "Podpowiedź", "Suggerimento", "Підказка", "Gợi ý", "คำใบ้"),
            E("main.wishlist", "添加到\n愿望单", "ADD A\nWISH LIST", "В СПИСОК\nЖЕЛАЕМОГО", "AÑADIR A\nDESEADOS", "AÑADIR A\nDESEADOS", "ADICIONAR À\nLISTA DE DESEJOS", "ADICIONAR À\nLISTA DE DESEJOS", "AUF DIE\nWUNSCHLISTE", "위시리스트에\n추가", "AJOUTER AUX\nSOUHAITS", "ウィッシュリストに\n追加", "İSTEK LİSTESİNE\nEKLE", "加入\n願望清單", "DODAJ DO\nLISTY ŻYCZEŃ", "AGGIUNGI ALLA\nLISTA DESIDERI", "ДОДАТИ ДО\nБАЖАНОГО", "THÊM VÀO\nDANH SÁCH ƯỚC", "เพิ่มใน\nสิ่งที่อยากได้"),
            E("main.replay_warning", "重玩此拼图包将重置其进度，是否继续？", "Replaying this puzzle pack will reset its progress. Continue?", "Повторная игра сбросит прогресс набора. Продолжить?", "Repetir este paquete reiniciará su progreso. ¿Continuar?", "Repetir este paquete reiniciará su progreso. ¿Continuar?", "Jogar este pacote novamente redefinirá o progresso. Continuar?", "Repetir este pacote irá repor o progresso. Continuar?", "Beim erneuten Spielen wird der Fortschritt zurückgesetzt. Fortfahren?", "이 퍼즐 팩을 다시 하면 진행도가 초기화됩니다. 계속할까요?", "Rejouer ce pack réinitialisera sa progression. Continuer ?", "このパックをリプレイすると進行状況がリセットされます。続けますか？", "Bu paketi yeniden oynamak ilerlemeyi sıfırlar. Devam edilsin mi?", "重玩此拼圖包將重設進度，是否繼續？", "Ponowna gra zresetuje postęp paczki. Kontynuować?", "Rigiocare il pacchetto ne azzererà i progressi. Continuare?", "Повторна гра скине прогрес набору. Продовжити?", "Chơi lại gói này sẽ đặt lại tiến trình. Tiếp tục?", "การเล่นแพ็กนี้อีกครั้งจะรีเซ็ตความคืบหน้า ดำเนินการต่อไหม"),
            E("main.confirm_exit", "确认退出游戏？", "Exit the game?", "Выйти из игры?", "¿Salir del juego?", "¿Salir del juego?", "Sair do jogo?", "Sair do jogo?", "Spiel beenden?", "게임을 종료할까요?", "Quitter le jeu ?", "ゲームを終了しますか？", "Oyundan çıkılsın mı?", "確定退出遊戲？", "Wyjść z gry?", "Uscire dal gioco?", "Вийти з гри?", "Thoát trò chơi?", "ออกจากเกมหรือไม่"),
            E("main.confirm_delete_save", "确认删除进度存储？", "Delete this save?", "Удалить это сохранение?", "¿Borrar esta partida?", "¿Borrar esta partida?", "Excluir este jogo salvo?", "Eliminar este jogo guardado?", "Diesen Spielstand löschen?", "이 저장을 삭제할까요?", "Supprimer cette sauvegarde ?", "このセーブデータを削除しますか？", "Bu kayıt silinsin mi?", "確定刪除此存檔？", "Usunąć ten zapis?", "Eliminare questo salvataggio?", "Видалити це збереження?", "Xóa bản lưu này?", "ลบบันทึกนี้หรือไม่"),
            E("main.yes_replay", "是的,重玩", "Yes, Replay", "Да, переиграть", "Sí, repetir", "Sí, repetir", "Sim, jogar de novo", "Sim, repetir", "Ja, erneut spielen", "예, 다시 하기", "Oui, rejouer", "はい、リプレイ", "Evet, yeniden oyna", "是，重玩", "Tak, zagraj ponownie", "Sì, rigioca", "Так, переграти", "Có, chơi lại", "ใช่ เล่นอีกครั้ง"),
            E("main.play", "玩", "Play", "Играть", "Jugar", "Jugar", "Jogar", "Jogar", "Spielen", "플레이", "Jouer", "遊ぶ", "Oyna", "玩", "Graj", "Gioca", "Грати", "Chơi", "เล่น"),
            E("main.replay", "重玩", "Replay", "Переиграть", "Repetir", "Repetir", "Jogar de novo", "Repetir", "Erneut spielen", "다시 하기", "Rejouer", "リプレイ", "Yeniden oyna", "重玩", "Zagraj ponownie", "Rigioca", "Переграти", "Chơi lại", "เล่นอีกครั้ง"),
            E("main.new_game", "新游戏", "New Game", "Новая игра", "Nueva partida", "Nueva partida", "Novo jogo", "Novo jogo", "Neues Spiel", "새 게임", "Nouvelle partie", "ニューゲーム", "Yeni oyun", "新遊戲", "Nowa gra", "Nuova partita", "Нова гра", "Trò chơi mới", "เกมใหม่"),
            E("main.save_summary", "已解锁的拼图包：{0}\n{1}", "Unlocked puzzle packs: {0}\n{1}", "Открыто наборов: {0}\n{1}", "Paquetes desbloqueados: {0}\n{1}", "Paquetes desbloqueados: {0}\n{1}", "Pacotes desbloqueados: {0}\n{1}", "Pacotes desbloqueados: {0}\n{1}", "Freigeschaltete Packs: {0}\n{1}", "잠금 해제한 퍼즐 팩: {0}\n{1}", "Packs débloqués : {0}\n{1}", "解放済みパック：{0}\n{1}", "Açılan bulmaca paketleri: {0}\n{1}", "已解鎖的拼圖包：{0}\n{1}", "Odblokowane paczki: {0}\n{1}", "Pacchetti sbloccati: {0}\n{1}", "Відкрито наборів: {0}\n{1}", "Gói đã mở khóa: {0}\n{1}", "แพ็กที่ปลดล็อก: {0}\n{1}"),
            E("main.level_outline_description", "勾勒出整个关卡的轮廓(关闭时分数增加5%)", "Outline the entire board (score +5% when off)", "Контур всего поля (при отключении +5% к счёту)", "Contornea todo el tablero (puntuación +5% al desactivar)", "Contornea todo el tablero (puntuación +5% al desactivar)", "Contorna todo o tabuleiro (pontuação +5% quando desativado)", "Contorna todo o tabuleiro (pontuação +5% quando desligado)", "Umrandet das ganze Brett (aus: +5% Punkte)", "전체 보드 윤곽 표시 (끄면 점수 +5%)", "Trace le contour du plateau (désactivé : score +5 %)", "ボード全体を縁取り（オフでスコア+5%）", "Tüm tahtayı çizer (kapalıyken puan +%5)", "勾勒整個關卡輪廓（關閉時分數增加5%）", "Obrys całej planszy (wyłączone: +5% wyniku)", "Contorna tutta la tavola (disattivato: punteggio +5%)", "Контур усього поля (вимкнено: +5% до рахунку)", "Viền toàn bộ bảng (tắt: điểm +5%)", "ตีเส้นขอบทั้งกระดาน (ปิด: คะแนน +5%)"),
            E("main.sticker_outline_description", "勾勒出整个关卡的外框(关闭时分数增加2%)", "Outline every sticker slot (score +2% when off)", "Контуры всех ячеек (при отключении +2% к счёту)", "Contornea cada hueco (puntuación +2% al desactivar)", "Contornea cada espacio (puntuación +2% al desactivar)", "Contorna cada encaixe (pontuação +2% quando desativado)", "Contorna cada espaço (pontuação +2% quando desligado)", "Umrandet alle Stickerplätze (aus: +2% Punkte)", "모든 스티커 홈 윤곽 표시 (끄면 점수 +2%)", "Trace chaque emplacement (désactivé : score +2 %)", "各ステッカー枠を縁取り（オフでスコア+2%）", "Her çıkartma yuvasını çizer (kapalıyken puan +%2)", "勾勒每個貼紙凹槽（關閉時分數增加2%）", "Obrys każdego miejsca (wyłączone: +2% wyniku)", "Contorna ogni spazio (disattivato: punteggio +2%)", "Контур кожного місця (вимкнено: +2% до рахунку)", "Viền từng ô nhãn dán (tắt: điểm +2%)", "ตีเส้นขอบทุกช่องสติกเกอร์ (ปิด: คะแนน +2%)"),

            E("game.complete", "完成!", "Complete!", "Готово!", "¡Completado!", "¡Completado!", "Concluído!", "Concluído!", "Geschafft!", "완료!", "Terminé !", "完成！", "Tamamlandı!", "完成！", "Ukończono!", "Completato!", "Завершено!", "Hoàn thành!", "สำเร็จ!"),
            E("game.total_score", "总得分：", "Total Score:", "Общий счёт:", "Puntuación total:", "Puntuación total:", "Pontuação total:", "Pontuação total:", "Gesamtpunktzahl:", "총점:", "Score total :", "合計スコア：", "Toplam puan:", "總得分：", "Łączny wynik:", "Punteggio totale:", "Загальний рахунок:", "Tổng điểm:", "คะแนนรวม:"),
            E("game.test_complete", "一键完成", "Complete All", "Завершить всё", "Completar todo", "Completar todo", "Concluir tudo", "Concluir tudo", "Alles abschließen", "모두 완료", "Tout terminer", "すべて完成", "Tümünü tamamla", "一鍵完成", "Ukończ wszystko", "Completa tutto", "Завершити все", "Hoàn thành tất cả", "ทำให้เสร็จทั้งหมด"),
            E("game.tutorial.place", "从托盘中选出匹配的贴纸，贴在板子的正确位置上。", "Choose the matching sticker from the tray and place it in the correct spot on the board.", "Выберите подходящую наклейку с подноса и поместите её на нужное место.", "Elige la pegatina correcta de la bandeja y colócala en su lugar del tablero.", "Elige el sticker correcto de la bandeja y colócalo en su lugar del tablero.", "Escolha o adesivo certo na bandeja e coloque-o no local correto do tabuleiro.", "Escolhe o autocolante certo do tabuleiro e coloca-o no local correto.", "Wähle den passenden Sticker aus der Ablage und setze ihn an die richtige Stelle.", "트레이에서 알맞은 스티커를 골라 보드의 올바른 위치에 놓으세요.", "Choisissez l'autocollant correspondant et placez-le au bon endroit sur le plateau.", "トレイから合うステッカーを選び、ボードの正しい位置に置きましょう。", "Tepsiden eşleşen çıkartmayı seçip tahtadaki doğru yere yerleştirin.", "從托盤選出相符貼紙，放到板子的正確位置。", "Wybierz pasującą naklejkę z tacki i umieść ją we właściwym miejscu.", "Scegli l'adesivo giusto dal vassoio e mettilo nel punto corretto.", "Виберіть відповідну наліпку з лотка й помістіть її на правильне місце.", "Chọn nhãn dán phù hợp từ khay và đặt vào đúng vị trí trên bảng.", "เลือกสติกเกอร์ที่ตรงกันจากถาด แล้ววางในตำแหน่งที่ถูกต้องบนกระดาน"),
            E("game.tutorial.two", "将两个贴纸贴在板子的合适位置上，完成关卡。", "Place both stickers in the correct spots to complete the level.", "Поместите две наклейки на нужные места, чтобы завершить уровень.", "Coloca las dos pegatinas en su lugar para completar el nivel.", "Coloca los dos stickers en su lugar para completar el nivel.", "Coloque os dois adesivos nos locais certos para concluir a fase.", "Coloca os dois autocolantes nos locais certos para concluir o nível.", "Setze beide Sticker an die richtigen Stellen, um das Level abzuschließen.", "두 스티커를 올바른 위치에 놓아 레벨을 완료하세요.", "Placez les deux autocollants au bon endroit pour terminer le niveau.", "2枚のステッカーを正しい位置に置いてステージを完成させましょう。", "Bölümü tamamlamak için iki çıkartmayı doğru yerlere koyun.", "將兩張貼紙放在適當位置，完成關卡。", "Umieść obie naklejki we właściwych miejscach, aby ukończyć poziom.", "Metti entrambi gli adesivi nei punti corretti per completare il livello.", "Розмістіть дві наліпки правильно, щоб завершити рівень.", "Đặt hai nhãn dán vào đúng vị trí để hoàn thành màn chơi.", "วางสติกเกอร์ทั้งสองในตำแหน่งที่ถูกต้องเพื่อผ่านด่าน"),
            E("game.tutorial.hint", "攻克这一关！如果遇到困难，请使用“提示”按钮。", "Finish this level! Use the Hint button if you get stuck.", "Завершите уровень! Если возникнут трудности, нажмите «Подсказка».", "¡Supera el nivel! Usa el botón Pista si necesitas ayuda.", "¡Supera el nivel! Usa el botón Pista si necesitas ayuda.", "Conclua a fase! Use o botão Dica se precisar de ajuda.", "Conclui o nível! Usa o botão Dica se precisares de ajuda.", "Schließe das Level ab! Nutze bei Bedarf den Hinweis-Button.", "레벨을 완료하세요! 어려우면 힌트 버튼을 사용하세요.", "Terminez le niveau ! Utilisez le bouton Indice en cas de difficulté.", "ステージをクリアしましょう！困ったらヒントボタンを使ってください。", "Bölümü bitirin! Zorlanırsanız İpucu düğmesini kullanın.", "攻克這一關！遇到困難時請使用「提示」按鈕。", "Ukończ poziom! W razie trudności użyj przycisku Podpowiedź.", "Completa il livello! Se sei in difficoltà, usa il pulsante Suggerimento.", "Завершіть рівень! Якщо складно, скористайтеся кнопкою «Підказка».", "Chinh phục màn này! Nếu gặp khó khăn, hãy dùng nút Gợi ý.", "ผ่านด่านนี้ให้ได้! หากติดขัดให้ใช้ปุ่มคำใบ้"),
            E("game.bonus.no_hint", "未使用提示", "No Hints Used", "Без подсказок", "Sin usar pistas", "Sin usar pistas", "Sem usar dicas", "Sem usar dicas", "Keine Hinweise", "힌트 미사용", "Sans indice", "ヒント未使用", "İpucu kullanılmadı", "未使用提示", "Bez podpowiedzi", "Nessun suggerimento", "Без підказок", "Không dùng gợi ý", "ไม่ใช้คำใบ้"),
            E("game.bonus.no_level_outline", "关闭关卡描边", "Board Outline Off", "Контур поля отключён", "Contorno del tablero desactivado", "Contorno del tablero desactivado", "Contorno do tabuleiro desativado", "Contorno do tabuleiro desligado", "Brettumriss aus", "보드 윤곽선 끔", "Contour du plateau désactivé", "ボード輪郭オフ", "Tahta ana hattı kapalı", "關閉關卡描邊", "Obrys planszy wyłączony", "Contorno tavola disattivato", "Контур поля вимкнено", "Tắt viền bảng", "ปิดเส้นขอบกระดาน"),
            E("game.bonus.no_sticker_outline", "关闭贴纸描边", "Sticker Outlines Off", "Контуры наклеек отключены", "Contornos de pegatinas desactivados", "Contornos de stickers desactivados", "Contornos dos adesivos desativados", "Contornos dos autocolantes desligados", "Sticker-Umrisse aus", "스티커 윤곽선 끔", "Contours des autocollants désactivés", "ステッカー輪郭オフ", "Çıkartma ana hatları kapalı", "關閉貼紙描邊", "Obrysy naklejek wyłączone", "Contorni adesivi disattivati", "Контури наліпок вимкнено", "Tắt viền nhãn dán", "ปิดเส้นขอบสติกเกอร์"),
            E("game.bonus.fast", "快速完成", "Quick Completion", "Быстрое завершение", "Finalización rápida", "Finalización rápida", "Conclusão rápida", "Conclusão rápida", "Schnell abgeschlossen", "빠른 완료", "Terminé rapidement", "スピードクリア", "Hızlı tamamlama", "快速完成", "Szybkie ukończenie", "Completamento rapido", "Швидке завершення", "Hoàn thành nhanh", "ผ่านอย่างรวดเร็ว"),
            E("game.bonus.points", "+{0}分", "+{0} points", "+{0} очков", "+{0} puntos", "+{0} puntos", "+{0} pontos", "+{0} pontos", "+{0} Punkte", "+{0}점", "+{0} points", "+{0}ポイント", "+{0} puan", "+{0}分", "+{0} pkt", "+{0} punti", "+{0} очок", "+{0} điểm", "+{0} คะแนน"),

            E("task.score.any", "完成任意拼图包，收集 {0} 分", "Complete any puzzle pack and collect {0} points", "Завершите любой набор и наберите {0} очков", "Completa cualquier paquete y consigue {0} puntos", "Completa cualquier paquete y consigue {0} puntos", "Conclua qualquer pacote e ganhe {0} pontos", "Conclui qualquer pacote e obtém {0} pontos", "Schließe ein beliebiges Pack ab und sammle {0} Punkte", "아무 퍼즐 팩이나 완료하고 {0}점을 모으세요", "Terminez un pack et gagnez {0} points", "任意のパックを完成させて{0}ポイント獲得", "Herhangi bir paketi tamamlayıp {0} puan topla", "完成任意拼圖包，收集 {0} 分", "Ukończ dowolną paczkę i zdobądź {0} pkt", "Completa un pacchetto e ottieni {0} punti", "Завершіть будь-який набір і наберіть {0} очок", "Hoàn thành gói bất kỳ và thu thập {0} điểm", "ทำแพ็กใดก็ได้ให้เสร็จและเก็บ {0} คะแนน"),
            E("task.score.size", "完成 {0} 尺寸拼图包，收集 {1} 分", "Complete a size {0} puzzle pack and collect {1} points", "Завершите набор размера {0} и наберите {1} очков", "Completa un paquete de tamaño {0} y consigue {1} puntos", "Completa un paquete de tamaño {0} y consigue {1} puntos", "Conclua um pacote tamanho {0} e ganhe {1} pontos", "Conclui um pacote de tamanho {0} e obtém {1} pontos", "Schließe ein Pack der Größe {0} ab und sammle {1} Punkte", "{0} 크기 퍼즐 팩을 완료하고 {1}점을 모으세요", "Terminez un pack de taille {0} et gagnez {1} points", "サイズ{0}のパックを完成させて{1}ポイント獲得", "{0} boyutunda bir paketi tamamlayıp {1} puan topla", "完成 {0} 尺寸拼圖包，收集 {1} 分", "Ukończ paczkę w rozmiarze {0} i zdobądź {1} pkt", "Completa un pacchetto di dimensione {0} e ottieni {1} punti", "Завершіть набір розміру {0} і наберіть {1} очок", "Hoàn thành gói cỡ {0} và thu thập {1} điểm", "ทำแพ็กขนาด {0} ให้เสร็จและเก็บ {1} คะแนน"),
            E("task.stickers.any", "从任意拼图包中收集 {0} 个贴纸", "Collect {0} stickers from any puzzle pack", "Соберите {0} наклеек из любых наборов", "Consigue {0} pegatinas de cualquier paquete", "Consigue {0} stickers de cualquier paquete", "Colete {0} adesivos de qualquer pacote", "Recolhe {0} autocolantes de qualquer pacote", "Sammle {0} Sticker aus beliebigen Packs", "아무 퍼즐 팩에서 스티커 {0}개를 모으세요", "Collectez {0} autocollants dans n'importe quel pack", "任意のパックからステッカーを{0}枚集める", "Herhangi bir paketten {0} çıkartma topla", "從任意拼圖包收集 {0} 張貼紙", "Zbierz {0} naklejek z dowolnych paczek", "Raccogli {0} adesivi da qualsiasi pacchetto", "Зберіть {0} наліпок із будь-яких наборів", "Thu thập {0} nhãn dán từ gói bất kỳ", "เก็บสติกเกอร์ {0} ชิ้นจากแพ็กใดก็ได้"),
            E("task.stickers.size", "从 {0} 尺寸拼图包中收集 {1} 个贴纸", "Collect {1} stickers from size {0} puzzle packs", "Соберите {1} наклеек из наборов размера {0}", "Consigue {1} pegatinas de paquetes de tamaño {0}", "Consigue {1} stickers de paquetes de tamaño {0}", "Colete {1} adesivos de pacotes tamanho {0}", "Recolhe {1} autocolantes de pacotes de tamanho {0}", "Sammle {1} Sticker aus Packs der Größe {0}", "{0} 크기 퍼즐 팩에서 스티커 {1}개를 모으세요", "Collectez {1} autocollants dans des packs de taille {0}", "サイズ{0}のパックからステッカーを{1}枚集める", "{0} boyutundaki paketlerden {1} çıkartma topla", "從 {0} 尺寸拼圖包收集 {1} 張貼紙", "Zbierz {1} naklejek z paczek w rozmiarze {0}", "Raccogli {1} adesivi dai pacchetti di dimensione {0}", "Зберіть {1} наліпок із наборів розміру {0}", "Thu thập {1} nhãn dán từ gói cỡ {0}", "เก็บสติกเกอร์ {1} ชิ้นจากแพ็กขนาด {0}"),
            E("task.packs.any", "完成 {0} 个任意尺寸的拼图包", "Complete {0} puzzle packs of any size", "Завершите {0} наборов любого размера", "Completa {0} paquetes de cualquier tamaño", "Completa {0} paquetes de cualquier tamaño", "Conclua {0} pacotes de qualquer tamanho", "Conclui {0} pacotes de qualquer tamanho", "Schließe {0} Packs beliebiger Größe ab", "아무 크기의 퍼즐 팩 {0}개를 완료하세요", "Terminez {0} packs de n'importe quelle taille", "任意サイズのパックを{0}個完成", "Herhangi bir boyutta {0} paket tamamla", "完成 {0} 個任意尺寸的拼圖包", "Ukończ {0} paczek dowolnego rozmiaru", "Completa {0} pacchetti di qualsiasi dimensione", "Завершіть {0} наборів будь-якого розміру", "Hoàn thành {0} gói với cỡ bất kỳ", "ทำแพ็กขนาดใดก็ได้ให้เสร็จ {0} แพ็ก"),
            E("task.packs.size", "完成 {0} 个 {1} 尺寸的拼图包", "Complete {0} size {1} puzzle packs", "Завершите {0} наборов размера {1}", "Completa {0} paquetes de tamaño {1}", "Completa {0} paquetes de tamaño {1}", "Conclua {0} pacotes tamanho {1}", "Conclui {0} pacotes de tamanho {1}", "Schließe {0} Packs der Größe {1} ab", "{1} 크기 퍼즐 팩 {0}개를 완료하세요", "Terminez {0} packs de taille {1}", "サイズ{1}のパックを{0}個完成", "{1} boyutunda {0} paket tamamla", "完成 {0} 個 {1} 尺寸的拼圖包", "Ukończ {0} paczek w rozmiarze {1}", "Completa {0} pacchetti di dimensione {1}", "Завершіть {0} наборів розміру {1}", "Hoàn thành {0} gói cỡ {1}", "ทำแพ็กขนาด {1} ให้เสร็จ {0} แพ็ก"),
            E("task.reward", "{0}，获得卡包奖励！", "{0}. Puzzle pack reward earned!", "{0}. Получена награда: набор!", "{0}. ¡Has ganado un paquete!", "{0}. ¡Ganaste un paquete!", "{0}. Você ganhou um pacote!", "{0}. Ganhaste um pacote!", "{0}. Pack-Belohnung erhalten!", "{0}. 퍼즐 팩 보상을 획득했습니다!", "{0}. Pack bonus obtenu !", "{0}。パック報酬を獲得！", "{0}. Paket ödülü kazanıldı!", "{0}，獲得卡包獎勵！", "{0}. Zdobyto paczkę w nagrodę!", "{0}. Pacchetto premio ottenuto!", "{0}. Отримано набір-нагороду!", "{0}. Đã nhận thưởng một gói!", "{0} ได้รับรางวัลแพ็ก!"),

            E("rank.rank", "排名", "Rank", "Место", "Posición", "Posición", "Posição", "Posição", "Rang", "순위", "Rang", "順位", "Sıra", "排名", "Miejsce", "Posizione", "Місце", "Hạng", "อันดับ"),
            E("rank.nickname", "昵称", "Nickname", "Имя", "Apodo", "Apodo", "Apelido", "Alcunha", "Name", "닉네임", "Pseudo", "ニックネーム", "Takma ad", "暱稱", "Nazwa", "Soprannome", "Ім'я", "Biệt danh", "ชื่อเล่น"),
            E("rank.my_rank", "我的排名", "My Rank", "Моё место", "Mi posición", "Mi posición", "Minha posição", "A minha posição", "Mein Rang", "내 순위", "Mon rang", "自分の順位", "Sıram", "我的排名", "Moje miejsce", "La mia posizione", "Моє місце", "Hạng của tôi", "อันดับของฉัน"),
            E("rank.total_players", "玩家总数:{0}", "Total players: {0}", "Всего игроков: {0}", "Total de jugadores: {0}", "Total de jugadores: {0}", "Total de jogadores: {0}", "Total de jogadores: {0}", "Spieler insgesamt: {0}", "전체 플레이어: {0}", "Total de joueurs : {0}", "プレイヤー総数：{0}", "Toplam oyuncu: {0}", "玩家總數：{0}", "Łącznie graczy: {0}", "Giocatori totali: {0}", "Усього гравців: {0}", "Tổng số người chơi: {0}", "ผู้เล่นทั้งหมด: {0}"),
            E("rank.top_ten", "排名前10%", "Top 10%", "Лучшие 10%", "10% mejores", "10% mejores", "10% melhores", "10% melhores", "Top 10 %", "상위 10%", "Top 10 %", "上位10%", "İlk %10", "排名前10%", "Najlepsze 10%", "Primi 10%", "Найкращі 10%", "Top 10%", "10% อันดับสูงสุด"),

            E("achievement.count", "成就数量", "Achievements", "Достижения", "Logros", "Logros", "Conquistas", "Conquistas", "Erfolge", "도전 과제", "Succès", "実績", "Başarımlar", "成就數量", "Osiągnięcia", "Obiettivi", "Досягнення", "Thành tựu", "ความสำเร็จ"),
            E("achievement.mock.title", "成就{0}", "Achievement {0}", "Достижение {0}", "Logro {0}", "Logro {0}", "Conquista {0}", "Conquista {0}", "Erfolg {0}", "도전 과제 {0}", "Succès {0}", "実績{0}", "Başarım {0}", "成就{0}", "Osiągnięcie {0}", "Obiettivo {0}", "Досягнення {0}", "Thành tựu {0}", "ความสำเร็จ {0}"),
            E("achievement.mock.description", "成就描述{0}", "Achievement description {0}", "Описание достижения {0}", "Descripción del logro {0}", "Descripción del logro {0}", "Descrição da conquista {0}", "Descrição da conquista {0}", "Erfolgsbeschreibung {0}", "도전 과제 설명 {0}", "Description du succès {0}", "実績の説明{0}", "Başarım açıklaması {0}", "成就描述{0}", "Opis osiągnięcia {0}", "Descrizione obiettivo {0}", "Опис досягнення {0}", "Mô tả thành tựu {0}", "คำอธิบายความสำเร็จ {0}"),
            E("photo.saved", "图像已保存到电脑桌面。", "Image saved to your desktop.", "Изображение сохранено на рабочем столе.", "Imagen guardada en el escritorio.", "Imagen guardada en el escritorio.", "Imagem salva na área de trabalho.", "Imagem guardada no ambiente de trabalho.", "Bild auf dem Desktop gespeichert.", "이미지를 바탕 화면에 저장했습니다.", "Image enregistrée sur le bureau.", "画像をデスクトップに保存しました。", "Görsel masaüstüne kaydedildi.", "圖片已儲存到電腦桌面。", "Obraz zapisano na pulpicie.", "Immagine salvata sul desktop.", "Зображення збережено на робочому столі.", "Đã lưu ảnh vào màn hình nền.", "บันทึกรูปภาพไว้บนเดสก์ท็อปแล้ว"),
            E("loading.progress", "加载中... {0}%", "Loading... {0}%", "Загрузка... {0}%", "Cargando... {0}%", "Cargando... {0}%", "Carregando... {0}%", "A carregar... {0}%", "Laden... {0}%", "불러오는 중... {0}%", "Chargement... {0}%", "読み込み中... {0}%", "Yükleniyor... %{0}", "載入中... {0}%", "Wczytywanie... {0}%", "Caricamento... {0}%", "Завантаження... {0}%", "Đang tải... {0}%", "กำลังโหลด... {0}%")
        };
    }
}
