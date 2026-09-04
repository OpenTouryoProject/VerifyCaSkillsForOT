# 概要
Open棟梁向けに開発されたコーディング・エージェントのスキルを実験・検証するリポジトリ

## 前提
- CodingAgentは、ClaudeCode（モデルはClaude Opus 5）
- .NETは、net48、net10.0 が前提
- [Open棟梁](https://github.com/OpenTouryoProject/OpenTouryo)はdevelopを使用(.dosc/prompt/検証プロンプト.md）
- [OpenTouryoCodingAgentAssets](https://github.com/OpenTouryoProject/OpenTouryoCodingAgentAssets)から最新スキル（main）をインストール。
- [LocalServicesOnDocker](https://github.com/NetDevInfraWGinOSSConsortium/LocalServicesOnDocker)でDB（ローカル・サービス）を動かしておく。

## プロンプト
docのplan、specを参照するので以下の非常に短いプロンプトのみで検証可能。

- .\docs\prompt\検証プロンプト.md の セットアップ > 基本 の Core、Transformを実行して下さい。

- .\docs\prompt\検証プロンプト.md の アプリ実装 の マスタ の 1-5 を実行して下さい。

- .\docs\prompt\検証プロンプト.md の アプリ実装 の トランザクション1 の 1-5 を実行して下さい。

- .\docs\prompt\検証プロンプト.md の アプリ実装 の トランザクション2 の 1-5 を実行して下さい。  
モジュール名等に使用するProgramIDには受注管理のprefix:Ordを使用して下さい（例:OrdListSearch、OrdBusiness）。

- .\docs\prompt\検証プロンプト.md の アプリ実装 の トランザクション3 の 1-5 を実行して下さい。  
トランザクション3 は トランザクション2 に明細を追加実装するもので、同じProgramIDを使用します（例:OrdDetailedView）。