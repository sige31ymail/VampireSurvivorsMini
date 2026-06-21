# マップ高品質化 実装計画 / 作業ハンドオフ

> 作成日: 2026-06-21 / 次回作業: 2026-06-22 以降
> このドキュメントを最初に読めば、文脈ゼロからでも作業を再開できます。

---

## 0. ゴールと方向性（決定済み）

ヴァンサバ系のマップ品質に近づける。調査の結論「**差はコードでなくアート方向**」を踏まえ、以下に決定済み：

| 項目 | 決定 |
|---|---|
| アート方向 | **平面トップダウン＋ペイント/カートゥーン調**（参考: 本家VS / カートゥーン系モバイル） |
| マップ構造 | **無限チャンク・ストリーミング**（物理なし・座標演算） |
| 衝突 | **プレイヤーのみ**（敵はすり抜け）。距離ベース・物理エンジン不使用 |
| 地面の変化の付け方 | **タイルを混ぜない**。単一のシームレス地面 ＋ **Decor（透過小物）の散布**で変化を出す |

> 重要な学び: 明るさの違うタイルをグリッドで混ぜると**市松模様**になる（実証済み・却下）。変化は必ず Decor レイヤーで。

---

## 1. 現状（実装済み・コミット済み）

直近コミット: `3bc5619`（origin/master に push 済み）

| 機能 | 状態 | 主担当ファイル |
|---|---|---|
| 無限チャンク・ストリーミング | ✅ | `Assets/Scripts/Stage/ChunkManager.cs` |
| プレイヤー専用の距離衝突（壁ずり対応） | ✅ | `Assets/Scripts/Stage/Arena.cs`（チャンク単位で動的登録） |
| 障害物アート（透過・バイオーム別） | ✅ | `Resources/Props/<biome>/` |
| 接地影（プレイヤー・敵・障害物） | ✅ | `Assets/Scripts/Effects/GroundShadow.cs` |
| バイオーム別の空気感（暗所スポットライト＋ヴィネット） | ✅ | `Assets/Scripts/Stage/StageAtmosphere.cs` |
| 地面タイル（単一基本タイル） | ✅ | `ChunkManager.LayGround` / `Resources/Ground/<biome>/` |
| チープな初期装飾（道・家・木）の除去 | ✅ | `AmbientFx` を `VampireSurvivorsMini.cs` で無効化 |

### 主要ファイルの役割
- `Stage/ChunkManager.cs` … ストリーミング本体。地面タイル・障害物の生成/プール/Arena登録。
- `Stage/Arena.cs` … `ClampMovement(from,to,radius)` で移動可否。障害物はチャンク単位で `SetChunk/RemoveChunk`。
- `Stage/StageManager.cs` … `ChunkManager` と `StageAtmosphere` を起動。地面下地(Background)・カメラ設定。
- `Stage/StageAtmosphere.cs` … バイオーム別 `Config()`（暗さ/色/ヴィネット/背景）。
- `Effects/ScreenVignette.cs` … 四隅ビネット。`intensity` を毎フレーム反映（実行時調整可）。
- `Effects/GroundShadow.cs` … `GroundShadow.Attach(parent,w,h,yOffset,alpha)` で接地影。
- `Player/Player.cs` … `Move()` 内で `Arena.ClampMovement` を通す（当たり半径 `CollisionRadius=0.4`）。
- `Core/VampireSurvivorsMini.cs` … 起動ブートストラップ。`AmbientFx` 生成行はコメントアウト済み。
- `Core/Background.cs` … 無限タイリングの地面“下地”（`Resources/Tiles/<biome>`）。Ground タイルの下に残置。

---

## 2. 次にやる作業（優先順）

### ★4 Decor 散布 ← 次の本命
**目的**: 単一地面の単調さを、散らした透過小物（花・草の房・小石・ひび）で解消。

- **実装方針**（`ChunkManager` に「装飾レイヤー」を追加）:
  - `Resources/Decor/<biome>/` のテクスチャを `Init` で読み込む（無ければ何も出さない＝チープな代替は出さない）。
  - 各チャンク生成時に、**非衝突**の装飾を決定論シードで疎らに散布（密度: 1チャンクあたり 6〜12 程度、要調整）。
  - **Arena には登録しない**（衝突しない）。`sortingOrder = -3`（地面 -50/影 -5 より上、障害物 0 より下、もしくは小物は -3〜-1）。
  - プール再利用（`groundPool`/`pool` と同様に `decorPool`）。チャンク解放時に返却。
  - サイズは小さめ（0.4〜1.0）でランダム。接地影は付けない（小物なので不要）。
- **受け入れ条件**: 草原で花/草が疎らに散り、市松にならず自然。遠近で湧き/消えのカクつき無し。
- **必要アート**: `Resources/Decor/grassland/` に `flower_01.png` `tuft_01.png` `pebble_01.png` `crack_01.png`（透過・小物単体）。

### ★5 VFX / 攻撃テレグラフ強化
- `Assets/Scripts/VFX/VfxManager.cs` を起点に、範囲攻撃の予兆表示などを強化（参考画像の赤い剣閃・ルーン円）。

### その他（細かい改善・必要に応じて）
- **地面の継ぎ目**: `grassland.png` は左右差23とほぼシームレスだが、4マスごとに薄い線が出るなら「シームレス補正（端ぼかし）」を適用。
- **下地 Background の二重描画**: Ground タイルがある biome では Background を止めても良い（軽微な最適化）。
- **カメラのクランプ**: 無限マップなので不要（現状でOK）。
- **`Sprite Tiling ... Full Rect` 警告**: 下地 Background の仕様。無害。消すなら対象タイルの Import 設定で Mesh Type = Full Rect。

---

## 3. アート制作キット（ChatGPT）

### フォルダ構成（ローダー対応済み）
```
Assets/Resources/
├─ Ground/<biome>/   ← シームレス地面タイル“1種だけ”（不透明・正方形・4辺がつながる）
├─ Decor/<biome>/    ← 非衝突の小物（透過・1個ずつ・中央配置）★4で使用
├─ Props/<biome>/    ← 衝突する障害物（透過・岩/木/墓石/柱）※実装済み
└─ Tiles/            ← 旧・単一タイル（Background 下地）
```
`<biome>` = `grassland` / `forest` / `graveyard` / `castle`

### 透過の注意 & 後処理
ChatGPT は透過PNGが出ないことがある。出ない場合の後処理（白背景除去）を実施済みの手順:
- 原本を `C:\app\claude_temp\_props_raw_backup\` 等にバックアップ → System.Drawing で端からフラッドフィルして白背景を透明化 → 512pxへリサイズ。
- ※このスクリプトは未ファイル化。再利用したいので、必要なら `tools/remove_bg.ps1` として保存する（TODO）。

### プロンプト（コピペ用）

**地面タイル（Ground、不透明・シームレス）**
```
A seamless tileable top-down ground texture of [SURFACE], painterly cartoon game
art style. 512x512, repeats perfectly on all four edges with no visible seams.
Even flat top-down lighting, no directional shadows, no vignette, no central focal
object, uniform coverage. Cohesive [PALETTE] palette, subtle natural variation.
No props, no characters, no text. Make sure it is perfectly tileable and seamless.
```
- grassland: `SURFACE=lush green grass`, `PALETTE=muted green`
- forest: `SURFACE=dark mossy forest floor with leaves`, `PALETTE=deep forest green-brown`
- graveyard: `SURFACE=cold packed dirt with sparse dead grass`, `PALETTE=desaturated grey-brown`
- castle: `SURFACE=grey stone flagstone floor`, `PALETTE=cold grey stone`
- ※ Ground は **1 biome 1枚**だけでよい（変種を混ぜない方針）。

**Decor 小物（透過・★4用）**
```
A single small [grass tuft / flower / cluster of pebbles / crack in the ground],
top-down, painterly cartoon game style, transparent background PNG with alpha,
one item centered with small padding, soft tiny contact shadow, [biome] palette,
256x256. No scene, no other objects, no text.
```

**障害物（Props・透過）**※既存運用
```
A single [mossy boulder / gnarled dark tree / weathered gravestone / broken stone
pillar], top-down three-quarter view, 2D game asset sprite, transparent background
PNG with alpha, one object centered, soft contact shadow under the base, [biome]
palette, 512x512. No ground, no other objects, no text.
```

---

## 4. コーディング規約・落とし穴

- **物理エンジン禁止**: 当たり判定は距離ベース自前（`Arena`）。Physics2D / Collider / Rigidbody は使わない。
- **スプライトはコード生成 or Resourcesアート**: 実行時 `Sprite.Create`。手動スプライト分割は不要（透過さえあればよい）。
- **UI は IMGUI(OnGUI)**: TMP＋日本語フォントは文字化けで却下済み。既存 `UISkin` を使う。
- **決定論シード**: チャンク内容は `cx*73856093 ^ cy*19349663 ^ stageSeed` 系で生成（再訪で同じ）。`System.Random` を使い、`UnityEngine.Random` のグローバル状態を汚さない。
- **プール必須**: 生成物は `Stack<GameObject>` で再利用。チャンク解放時に返却。
- **チャンク境界の通路**: 障害物は `EdgeInset=3` で境界から離す → グリッド線沿いに必ず通路ができる（詰み防止）。
- **変種タイルを混ぜない**（市松の原因）。変化は Decor で。
- **透過必須**: 障害物・Decor は透明背景でないと「白い四角」になる。

### sortingOrder 早見表
```
Background(下地) -100 / 地面タイル -50 / 接地影 -5 / Decor -3〜-1(予定) /
障害物 0 / 敵・プレイヤー等 1〜13 / 暗所スポットライト 900 / 画面ヴィネット 1000 / UI(IMGUI)最上
```

### 主要パラメータ（調整ポイント）
- `ChunkManager`: `ChunkSize=20` `ViewRadius=1`(3x3) `GroundTileSize=4` `EdgeInset=3` `MinGap=1.6` `CenterClear=4.5` `MinPerChunk=4` `MaxPerChunk=8`
- `Player.CollisionRadius=0.4`
- 障害物の当たり半径係数: 丸 `0.46` / 四角 `0.42`
- `StageAtmosphere.Config(biome)`: (darkness, tint, vignette, bgDarken)
  - grassland 0.00 / forest 0.30 / graveyard 0.45 / castle 0.55

---

## 5. 明日の再開手順

1. `git pull`（最新 `3bc5619` 以降を取得）
2. Unity Hub から `6000.4.10f1` で起動 → `Assets/Scenes/TitleScene` を開く
3. ▶ Play で現状確認（草原: 一様な草地＋障害物＋接地影／墓地・城: 暗所スポットライト）
4. **★4 Decor 散布**から着手（このドキュメントの「2.★4」を参照）
   - 先に `Resources/Decor/grassland/` に小物を数枚用意できると、実装直後に見栄え確認できる
5. 動作OKならコミット＆プッシュ（メッセージ末尾に `Co-Authored-By` を付与）

---

## 6. バックログ / アイデア（優先度低）
- **破壊可能プロップ**（壊すとXP/ゴールド）: `Resources/Destructibles/<biome>/`。genre本命のリッチさ。
- バイオーム遷移（無限マップ内で地形が変わる帯）。
- ボス用アリーナ（一時的に区切る）。
- 地面シームレス自動補正ツール（`tools/`）。
- 障害物/Decor のアート追加（`_02`,`_03`…）でバリエーション増（コード変更不要）。

---

## 付記: これまでのコミット履歴（マップ関連）
- タイトル画面の修復（空シーン問題）
- 武器のフリーズ修正（`GameState.Enemies` の foreach 中変更）
- `29d9ef0` 無限チャンク・ストリーミング＋障害物アート
- `98972cd` バイオーム別の空気感＋障害物の接地影
- `3bc5619` 地面タイル（単一基本）＋チープ装飾の除去 ← 現在地
