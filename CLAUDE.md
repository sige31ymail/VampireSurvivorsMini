# プロジェクト概要
Unity 6 (Universal 2D) のヴァンパイアサバイバー風2Dゲーム。Steam販売を目指す。

# 重要な制約
- 新Input System使用。UnityEngine.Input（旧API）は #if で分岐済み。新コードは Keyboard.current を使う
- 物理エンジン不使用。当たり判定は距離ベースの自前実装
- スプライトはコード生成（VampireSurvivorsMini.CreateSprites）。アセット追加時は相談すること
- 一時停止は Time.timeScale = 0 方式。新しい挙動を追加する際は停止中の動作を考慮する

# 検証方法
- Claude Codeはコンパイル・実行できない。編集後はユーザーがUnityエディタでコンパイル確認とプレイテストを行い、Consoleのエラーを報告する
