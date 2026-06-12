/// <summary>武器の基底クラス。Playerが毎フレームTickを呼ぶ。</summary>
public abstract class Weapon
{
    public int level = 1;
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual int MaxLevel => 5;
    public bool IsMaxLevel => level >= MaxLevel;

    /// <summary>毎フレームの処理（攻撃タイミング管理など）</summary>
    public abstract void Tick(Player player, float dt);

    /// <summary>レベルアップ時の処理</summary>
    public virtual void LevelUp(Player player) { level++; }
}
