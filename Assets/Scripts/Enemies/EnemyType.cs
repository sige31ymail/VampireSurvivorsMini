/// <summary>敵の種類</summary>
public enum EnemyType
{
    // 基本タイプ（草原ステージ）
    Chaser,     // 標準：まっすぐ追いかける
    Runner,     // 速いが脆い
    Tank,       // 遅いが硬い、XP多め
    Dasher,     // 接近すると狙いを定めて突進

    // 追加タイプ（森ステージ）
    Bat,        // 高速で波状に移動
    Slime,      // 死亡時に分裂
    Archer,     // 遠距離から弾を撃つ

    // 追加タイプ（墓地ステージ）
    Ghost,      // 半透明、一定確率で攻撃を回避
    Skeleton,   // 死亡後に復活（1回のみ）
    Mage,       // 周囲に魔法陣を展開

    // 追加タイプ（城ステージ）
    Golem,      // 非常に硬い、移動が遅い
    Vampire,    // HP吸収攻撃

    // ボス
    Boss,           // 草原ボス
    ForestBoss,     // 森ボス：分裂
    GraveyardBoss,  // 墓地ボス：召喚
    CastleBoss      // 城ボス：多段攻撃
}
