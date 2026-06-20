# UI（uGUI + TextMeshPro）セットアップ手順

新UIは TextMeshPro を使います。**別PCで初回だけ**以下を行ってください（コードでは完結しない部分）。

## 1. TMP Essentials の取込
Unityメニュー → `Window > TextMeshPro > Import TMP Essential Resources` を実行。
（`Assets/TextMesh Pro/` が生成されればOK）

## 2. 日本語フォントアセットの作成
TMP標準フォント（LiberationSans）は**日本語グリフを含まない**ため、日本語フォントが必要です。

1. 日本語TTF/OTFをプロジェクトに追加
   - 例: [Noto Sans JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) の `NotoSansJP-Regular.ttf` を `Assets/` 配下に置く
2. `Window > TextMeshPro > Font Asset Creator` を開く
   - **Source Font**: 追加したTTF
   - **Atlas Population Mode**: `Dynamic`（推奨。未収録文字も実行時に動的生成されるので文字欠けが起きにくい）
     - ※ Dynamicが使えない/重い場合は `Static` で、Character Set を「常用漢字＋かな＋英数記号」に。
   - `Generate Font Atlas` → `Save` で `.asset` を保存
3. 生成された **フォントアセット(`.asset`)** を **`Assets/Resources/Fonts/`** に置く
   - フォルダが無ければ作成。ファイル名は任意（コードは `Resources/Fonts` 内の最初のTMPフォントを自動採用）

## 3. 確認
- Play して、HUDやメニューの**日本語が文字化けせず表示**されればOK。
- もし豆腐（□）や空白になる場合：フォントアセットが `Resources/Fonts/` に無い、もしくは Static で対象文字が未収録。
  Console に `[UITheme] Resources/Fonts に日本語TMPフォントが見つかりません…` の警告が出ていないか確認。

## 補足
- uGUIのクリック判定には EventSystem が必要ですが、コード側(`UIKit.EnsureEventSystem`)が
  **新Input System用の `InputSystemUIInputModule` 付きで自動生成**します。手動配置は不要です。
- 解像度は `CanvasScaler`（参照1920×1080）で自動追従します。
