# SETUP-CHANGES.md

リポジトリの外＝**マシン/ユーザ全体に残る変更**の記録（AGENTS.md ポリシー）。
巻き戻し手順は `リセット手順.md` も参照。

| 種別 | 対象 | 値 | 実施日 | 巻き戻し方法 |
| --- | --- | --- | --- | --- |
| 環境変数（User） | `OT_RESOURCE_ROOT` | `D:\git\local\OpenTouryoProject\VerifyCaSkillsForOT\resource` | 2026-08-27（本ラウンドでは**既設値と一致**のため未変更。設定自体は過去ラウンド） | `[System.Environment]::SetEnvironmentVariable('OT_RESOURCE_ROOT', $null, 'User')` |
| リポジトリ外ディレクトリ | `C:\otr\` | OpenTouryo `develop` の ZIP と展開ツリー（基盤ビルドの作業場。MAX_PATH 回避のための短ルート） | 2026-08-27（`develop`＝moving ref のため ZIP を再取得し展開ツリーを作り直した） | `Remove-Item C:\otr -Recurse -Force` |

> **⚠ `C:\otr\OpenTouryo-develop\` の展開ツリーには `base2-overlay/` を適用済み＝素の `develop` ではない。**
> Transform ケース 2・3（親クラス2 のカスタマイズ）で上書きしたため。**素の ref を焼き直すときは展開ツリーを作り直す**こと
> （`scripts\setup-build.ps1 -Fresh -Redownload`）。そのまま再利用すると、overlay の改変が残った DLL が無言でベンダされる。

## 本ラウンドで**変更していない**もの（既に有効だったため。記録は参考）

| 種別 | 対象 | 状態 | 停止方法 |
| --- | --- | --- | --- |
| Windows サービス | ASP.NET State Service (`aspnet_state`) | 実行中（本ラウンド開始時点で既に Running。起動操作は行っていない） | `root\files\bat\aspnet_state-stop.bat`（要管理者） |
| Docker コンテナ | LocalServicesOnDocker（sqlserver / mysql / postgres / oracle / redis / mongo） | 実行中（本ラウンド開始時点で既に Up。起動操作は行っていない） | LocalServicesOnDocker の停止手順 |
| レジストリ | `LongPathsEnabled` | `0` のまま（有効化していない。短ルート `C:\otr` でビルドすることで回避） | － |
