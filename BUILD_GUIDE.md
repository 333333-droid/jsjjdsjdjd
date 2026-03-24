# OTRIO ビルドガイド

## Unity Editor

- Unity 6 で `/Users/hiromac/otrioProject` を開く
- [SampleScene.unity](/Users/hiromac/otrioProject/Assets/Scenes/SampleScene.unity) を開く
- `GameManager` オブジェクトについて、以下が設定されていることを確認する
- `boardCells` に 9 個のセルが割り当てられている
- `smallPiecePrefab`、`midPiecePrefab`、`bigPiecePrefab` が割り当てられている

## ビルド前の確認

- Play を押して、タイトル画面が表示されることを確認する
- 2 人対戦または 4 人対戦を開始し、いくつか駒を置いてみる
- 駒の色が正しいことを確認する
- `1P` は赤
- `2P` は青
- `3P` は黄
- `4P` は緑
- 勝利判定が行われ、`○Pの勝利！` と表示されることを確認する
- `タイトルへ` と `はじめから` の両方が動作することを確認する

## Windows EXE

- `File > Build Profiles` または `File > Build Settings` を開く
- `Windows, Mac, Linux` を選ぶ
- `Windows` を選択する
- 必要であれば [SampleScene.unity](/Users/hiromac/otrioProject/Assets/Scenes/SampleScene.unity) を追加する
- `Build` をクリックする
- `Builds/Windows` などの出力先フォルダを選ぶ

Unity の出力内容:

- `otrioProject.exe`
- 対応する `_Data` フォルダ

配布時にはこの両方が必要です。

## 仕上げのおすすめ

- Player Settings で製品名を `otrioProject` から `OTRIO` に変更する
- Player Settings でゲームアイコンを設定する
- クリックですぐ起動できる形で配布したい場合は、Windows ビルドフォルダ全体を zip にまとめて共有する
