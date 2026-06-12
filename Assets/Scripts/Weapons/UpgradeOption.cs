/// <summary>レベルアップ時の選択肢（タイトル・説明・適用処理）</summary>
public class UpgradeOption
{
    public readonly string title;
    public readonly string desc;
    public readonly System.Action<Player> apply;

    public UpgradeOption(string title, string desc, System.Action<Player> apply)
    {
        this.title = title;
        this.desc = desc;
        this.apply = apply;
    }
}
