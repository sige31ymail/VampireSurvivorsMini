using UnityEngine;

/// <summary>
/// アップグレードショップUI
/// タイトル画面からアクセスして永続アップグレードを購入
/// </summary>
public class UpgradeShopUI : MonoBehaviour
{
    public bool IsVisible { get; private set; }

    GUIStyle titleStyle;
    GUIStyle goldStyle;
    GUIStyle upgradeNameStyle;
    GUIStyle upgradeDescStyle;
    GUIStyle upgradeLevelStyle;
    GUIStyle buttonStyle;
    GUIStyle disabledButtonStyle;
    GUIStyle backButtonStyle;
    GUIStyle sectionStyle;

    Vector2 scrollPosition;

    int selectedTab; // 0: Upgrades, 1: Unlocks

    public void Show()
    {
        IsVisible = true;
        scrollPosition = Vector2.zero;
        selectedTab = 0;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    void OnGUI()
    {
        if (!IsVisible) return;

        InitStyles();

        // 背景
        GUI.color = new Color(0, 0, 0, 0.95f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float startY = 20f;

        // タイトルとゴールド表示
        GUI.Label(new Rect(0, startY, Screen.width, 50), "UPGRADE SHOP", titleStyle);

        int gold = MetaProgressionManager.Instance?.Gold ?? 0;
        GUI.Label(new Rect(0, startY + 50, Screen.width, 35), $"Gold: {gold}", goldStyle);

        // タブ
        float tabY = startY + 95;
        float tabW = 140f;
        float tabH = 35f;
        float tabX = cx - tabW;

        GUI.color = selectedTab == 0 ? new Color(0.3f, 0.3f, 0.4f) : new Color(0.15f, 0.15f, 0.2f);
        if (GUI.Button(new Rect(tabX, tabY, tabW, tabH), "Upgrades"))
            selectedTab = 0;

        GUI.color = selectedTab == 1 ? new Color(0.3f, 0.3f, 0.4f) : new Color(0.15f, 0.15f, 0.2f);
        if (GUI.Button(new Rect(tabX + tabW, tabY, tabW, tabH), "Unlocks"))
            selectedTab = 1;

        GUI.color = Color.white;

        // コンテンツエリア
        float contentY = tabY + tabH + 15;
        float contentH = Screen.height - contentY - 70;
        float panelW = Mathf.Min(500f, Screen.width - 40f);
        float panelX = cx - panelW / 2f;

        if (selectedTab == 0)
            DrawUpgradesTab(panelX, contentY, panelW, contentH);
        else
            DrawUnlocksTab(panelX, contentY, panelW, contentH);

        // 戻るボタン
        float btnW = 120f, btnH = 45f;
        if (GUI.Button(new Rect(cx - btnW / 2f, Screen.height - 55, btnW, btnH), "Back", backButtonStyle))
        {
            Hide();
        }
    }

    void DrawUpgradesTab(float x, float y, float width, float height)
    {
        var upgrades = MetaProgressionManager.Upgrades;
        float rowH = 75f;
        float totalH = upgrades.Length * rowH;

        // スクロールビュー
        scrollPosition = GUI.BeginScrollView(
            new Rect(x, y, width, height),
            scrollPosition,
            new Rect(0, 0, width - 20, totalH)
        );

        float cy = 0;
        foreach (var upgrade in upgrades)
        {
            DrawUpgradeRow(0, cy, width - 20, rowH - 5, upgrade);
            cy += rowH;
        }

        GUI.EndScrollView();
    }

    void DrawUpgradeRow(float x, float y, float width, float height, UpgradeDefinition upgrade)
    {
        var manager = MetaProgressionManager.Instance;
        if (manager == null) return;

        int level = manager.GetUpgradeLevel(upgrade.Type);
        int cost = manager.GetUpgradeCost(upgrade.Type);
        bool isMax = level >= upgrade.MaxLevel;
        bool canBuy = manager.CanPurchase(upgrade.Type);

        // 背景
        GUI.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float padding = 10f;
        float btnW = 90f;

        // 名前と説明
        GUI.Label(new Rect(x + padding, y + 5, width - btnW - padding * 2, 25), upgrade.Name, upgradeNameStyle);
        GUI.Label(new Rect(x + padding, y + 28, width - btnW - padding * 2, 20), upgrade.Description, upgradeDescStyle);

        // レベル表示
        string levelText = isMax ? "MAX" : $"Lv.{level}/{upgrade.MaxLevel}";
        GUI.Label(new Rect(x + padding, y + 48, 100, 20), levelText, upgradeLevelStyle);

        // 効果表示
        if (level > 0)
        {
            string effectText = upgrade.GetEffectText(level);
            var effectStyle = new GUIStyle(upgradeLevelStyle);
            effectStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            GUI.Label(new Rect(x + padding + 80, y + 48, 120, 20), effectText, effectStyle);
        }

        // 購入ボタン
        float btnX = x + width - btnW - padding;
        float btnY = y + (height - 40) / 2f;

        if (isMax)
        {
            GUI.enabled = false;
            GUI.Button(new Rect(btnX, btnY, btnW, 40), "MAX", disabledButtonStyle);
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = canBuy;
            if (GUI.Button(new Rect(btnX, btnY, btnW, 40), $"{cost}G", canBuy ? buttonStyle : disabledButtonStyle))
            {
                if (manager.PurchaseUpgrade(upgrade.Type))
                {
                    AudioManager.PlayLevelUp();
                }
            }
            GUI.enabled = true;
        }
    }

    void DrawUnlocksTab(float x, float y, float width, float height)
    {
        float rowH = 50f;
        float sectionH = 30f;

        // カテゴリごとの高さを計算
        float totalH = sectionH + UnlockManager.Weapons.Length * rowH +
                       sectionH + UnlockManager.Characters.Length * rowH +
                       sectionH + UnlockManager.Stages.Length * rowH + 30;

        scrollPosition = GUI.BeginScrollView(
            new Rect(x, y, width, height),
            scrollPosition,
            new Rect(0, 0, width - 20, totalH)
        );

        float cy = 0;

        // 武器セクション
        GUI.Label(new Rect(0, cy, width, sectionH), "Weapons", sectionStyle);
        cy += sectionH;
        foreach (var item in UnlockManager.Weapons)
        {
            bool unlocked = UnlockManager.Instance?.IsWeaponUnlocked(item.Id) ?? item.Id == 0;
            DrawUnlockRow(0, cy, width - 20, rowH - 5, item, unlocked);
            cy += rowH;
        }

        cy += 10;

        // キャラクターセクション
        GUI.Label(new Rect(0, cy, width, sectionH), "Characters", sectionStyle);
        cy += sectionH;
        foreach (var item in UnlockManager.Characters)
        {
            bool unlocked = UnlockManager.Instance?.IsCharacterUnlocked(item.Id) ?? item.Id == 0;
            DrawUnlockRow(0, cy, width - 20, rowH - 5, item, unlocked);
            cy += rowH;
        }

        cy += 10;

        // ステージセクション
        GUI.Label(new Rect(0, cy, width, sectionH), "Stages", sectionStyle);
        cy += sectionH;
        foreach (var item in UnlockManager.Stages)
        {
            bool unlocked = UnlockManager.Instance?.IsStageUnlocked(item.Id) ?? item.Id == 0;
            DrawUnlockRow(0, cy, width - 20, rowH - 5, item, unlocked);
            cy += rowH;
        }

        GUI.EndScrollView();
    }

    void DrawUnlockRow(float x, float y, float width, float height, UnlockableItem item, bool unlocked)
    {
        // 背景
        GUI.color = unlocked ? new Color(0.15f, 0.25f, 0.15f, 0.8f) : new Color(0.2f, 0.15f, 0.15f, 0.6f);
        GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float padding = 10f;

        // 名前
        var nameStyle = new GUIStyle(upgradeNameStyle);
        nameStyle.normal.textColor = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(x + padding, y + 5, width * 0.4f, 22), item.Name, nameStyle);

        // 状態/条件
        var descStyle = new GUIStyle(upgradeDescStyle);
        if (unlocked)
        {
            descStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            GUI.Label(new Rect(x + padding, y + 25, width - padding * 2, 18), "UNLOCKED", descStyle);
        }
        else
        {
            descStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(x + padding, y + 25, width - padding * 2, 18), item.Description, descStyle);

            // 進捗バー
            float progress = UnlockManager.Instance?.GetUnlockProgress(item) ?? 0f;
            float barX = x + width - 110;
            float barY = y + 8;
            float barW = 100;
            float barH = 12;

            GUI.color = new Color(0.2f, 0.2f, 0.2f);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);
            GUI.color = new Color(0.8f, 0.6f, 0.2f);
            GUI.DrawTexture(new Rect(barX, barY, barW * progress, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // パーセント表示
            var pctStyle = new GUIStyle(upgradeLevelStyle);
            pctStyle.fontSize = 10;
            pctStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(barX, barY - 1, barW, barH + 2), $"{Mathf.RoundToInt(progress * 100)}%", pctStyle);
        }
    }

    void InitStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

        goldStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        goldStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);

        upgradeNameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        upgradeNameStyle.normal.textColor = Color.white;

        upgradeDescStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13
        };
        upgradeDescStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

        upgradeLevelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14
        };
        upgradeLevelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };

        disabledButtonStyle = new GUIStyle(buttonStyle);
        disabledButtonStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        backButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18
        };

        sectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        sectionStyle.normal.textColor = new Color(0.9f, 0.75f, 0.3f);
    }
}
