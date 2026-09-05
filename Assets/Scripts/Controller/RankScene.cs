using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const string BootstrapObjectName = "RankSceneBootstrap";
    private const string ContentObjectName = "Content";
    private const string RankItemPrefabEditorPath = "Assets/Prefabs/RankItem.prefab";
    private const string RankItemPrefabResourcesPath = "RankItem";
    private const string RankBackgroundObjectName = "RankBg";
    private const string RankNumObjectName = "RankNum";
    private const string RankNumTextObjectName = "RankNumText";
    private const string RankNameObjectName = "RankName";
    private const string RankScoreObjectName = "RankScore";
    private const string RankBagNumObjectName = "RankBagNum";
    private const string TotalPlayerTextObjectName = "TextPlayerTotal";
    private const string RankBackgroundSpritePathPrefix = "Assets/UI/RankScene/RankCellBg_";
    private const string RankNumSpritePathPrefix = "Assets/UI/RankScene/RankNum_";
    private const int MockRankCount = 10;
    private const int MockTotalPlayerCount = 2543368;
    private static bool sHookedSceneLoaded;
    private RectTransform mContentRoot;
    private GameObject mRankItemPrefab;
    private Camera mSceneCamera;
    private Canvas mSceneCanvas;
    private int mAppliedScreenWidth;
    private int mAppliedScreenHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<RankScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneRank,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneRank))
        {
            Destroy(gameObject);
            return;
        }

        RefreshForWindowSizeChange();

        ConfigureReturnButton();
        ConfigureMockRankItems();
        RefreshLocalizedSummary();
    }

    private void Update()
    {
        RefreshForWindowSizeChange();
    }

    private void RefreshForWindowSizeChange()
    {
        GameCommonUtility.RefreshFixedAspectSceneCanvas(
            ref mSceneCamera,
            ref mSceneCanvas,
            ref mAppliedScreenWidth,
            ref mAppliedScreenHeight,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit);
    }

    private void ConfigureReturnButton()
    {
        var returnButtonObject = GameObject.Find(GameDefine.ReturnButtonObjectName);
        if (returnButtonObject == null)
        {
            Debug.LogWarning($"RankScene: return button not found. Expected object named {GameDefine.ReturnButtonObjectName}.");
            return;
        }

        var button = returnButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"RankScene: {GameDefine.ReturnButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnReturnButtonClicked);
        button.onClick.AddListener(OnReturnButtonClicked);
    }

    private void OnReturnButtonClicked()
    {
        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        GameManager.EnterMainScene();
    }

    private void ConfigureMockRankItems()
    {
        if (!TryResolveRankUi())
        {
            return;
        }

        ClearContent();
        var rankItems = CreateMockRankItems();
        for (var i = 0; i < rankItems.Count; i++)
        {
            CreateRankItem(rankItems[i]);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mContentRoot);

        var scrollRect = mContentRoot.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        Debug.Log($"RankScene: mock rank items created. total={rankItems.Count}");
    }

    private static void RefreshLocalizedSummary()
    {
        var totalPlayerObject = GameCommonUtility.FindSceneObject(TotalPlayerTextObjectName);
        var totalPlayerText = totalPlayerObject != null
            ? totalPlayerObject.GetComponent<TMP_Text>()
            : null;
        if (totalPlayerText != null)
        {
            totalPlayerText.text = GameLocalization.Format(
                "rank.total_players",
                MockTotalPlayerCount);
        }

        GameLocalization.RefreshSceneTexts();
    }

    private bool TryResolveRankUi()
    {
        var contentObject = GameCommonUtility.FindSceneObject(ContentObjectName);
        if (contentObject == null || !contentObject.TryGetComponent(out mContentRoot))
        {
            Debug.LogWarning($"RankScene: content root not found. Expected object named {ContentObjectName}.");
            return false;
        }

        mRankItemPrefab = LoadRankItemPrefab();
        if (mRankItemPrefab == null)
        {
            Debug.LogWarning($"RankScene: prefab not found. Expected {RankItemPrefabEditorPath}.");
            return false;
        }

        return true;
    }

    private static GameObject LoadRankItemPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RankItemPrefabEditorPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>(RankItemPrefabResourcesPath);
    }

    private void ClearContent()
    {
        if (mContentRoot == null)
        {
            return;
        }

        for (var i = mContentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(mContentRoot.GetChild(i).gameObject);
        }
    }

    private void CreateRankItem(MockRankData data)
    {
        var item = Instantiate(mRankItemPrefab, mContentRoot, false);
        item.name = $"RankItem{data.Rank:D2}";

        SetRankBackground(item.transform, data.Rank);
        SetRankNumber(item.transform, data.Rank);
        SetText(item.transform, RankNameObjectName, data.PlayerName);
        SetText(item.transform, RankScoreObjectName, data.Score.ToString());
        SetText(item.transform, RankBagNumObjectName, data.CardBagCount.ToString());
    }

    private static void SetRankBackground(Transform itemRoot, int rank)
    {
        if (rank < 1 || rank > 3)
        {
            return;
        }

        var backgroundImage = FindChild(itemRoot, RankBackgroundObjectName)?.GetComponent<Image>();
        if (backgroundImage == null)
        {
            return;
        }

        var sprite = LoadRankSprite(RankBackgroundSpritePathPrefix, rank);
        if (sprite != null)
        {
            backgroundImage.sprite = sprite;
            backgroundImage.SetNativeSize();
        }
    }

    private static void SetRankNumber(Transform itemRoot, int rank)
    {
        var rankNum = FindChild(itemRoot, RankNumObjectName);
        if (rankNum == null)
        {
            return;
        }

        var rankImage = rankNum.GetComponent<Image>();
        if (rankImage != null)
        {
            rankImage.enabled = rank <= 3;
            if (rank <= 3)
            {
                var sprite = LoadRankSprite(RankNumSpritePathPrefix, rank);
                if (sprite != null)
                {
                    rankImage.sprite = sprite;
                }
            }
        }

        if (rank <= 3)
        {
            return;
        }

        var rankText = FindChild(rankNum, RankNumTextObjectName);
        if (rankText == null)
        {
            var textObject = new GameObject(RankNumTextObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            rankText = textObject.transform;
            rankText.SetParent(rankNum, false);
        }

        var rankRect = rankText as RectTransform;
        if (rankRect != null)
        {
            rankRect.anchorMin = Vector2.zero;
            rankRect.anchorMax = Vector2.one;
            rankRect.pivot = new Vector2(0.5f, 0.5f);
            rankRect.offsetMin = Vector2.zero;
            rankRect.offsetMax = Vector2.zero;
            rankRect.localScale = Vector3.one;
        }

        var label = rankText.GetComponent<TMP_Text>();
        var nameLabel = FindChild(itemRoot, RankNameObjectName)?.GetComponent<TMP_Text>();
        if (label != null)
        {
            if (nameLabel != null)
            {
                label.font = nameLabel.font;
                label.fontSharedMaterial = nameLabel.fontSharedMaterial;
            }

            label.text = rank.ToString();
            label.fontSize = 48f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.black;
            label.raycastTarget = false;
        }
    }

    private static Sprite LoadRankSprite(string pathPrefix, int rank)
    {
        var spritePath = $"{pathPrefix}{rank}.png";
#if UNITY_EDITOR
        var editorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (editorSprite != null)
        {
            return editorSprite;
        }
#endif
        return GameCommonUtility.LoadSpriteByPath(spritePath, PixelsPerUnit);
    }

    private static List<MockRankData> CreateMockRankItems()
    {
        var result = new List<MockRankData>(MockRankCount);
        for (var i = 1; i <= MockRankCount; i++)
        {
            result.Add(new MockRankData
            {
                Rank = i,
                PlayerName = $"Player {i:D2}",
                Score = 10000 - (i - 1) * 735,
                CardBagCount = 42 - (i - 1) * 3
            });
        }

        return result;
    }

    private static void SetText(Transform root, string objectName, string text)
    {
        var child = FindChild(root, objectName);
        if (child == null)
        {
            return;
        }

        var label = child.GetComponent<TMP_Text>();
        if (label != null)
        {
            label.text = text;
        }
    }

    private static Transform FindChild(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var result = FindChild(root.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private struct MockRankData
    {
        public int Rank;
        public string PlayerName;
        public int Score;
        public int CardBagCount;
    }
}
