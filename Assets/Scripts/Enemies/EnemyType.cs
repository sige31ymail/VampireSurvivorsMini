/// <summary>敵の種類</summary>
public enum EnemyType
{
    Chaser, // 標準：まっすぐ追いかける
    Runner, // 速いが脆い
    Tank,   // 遅いが硬い、XP多め
    Dasher, // 接近すると狙いを定めて突進
    Boss    // 60秒ごとに出現する大型
}
