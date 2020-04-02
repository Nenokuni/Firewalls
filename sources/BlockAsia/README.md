# 概要

* [Google Cloud Platform](https://console.cloud.google.com/?hl=ja)のファイアウォールにIPアドレスブロックルールを追加するコマンドラインアプリケーション。
* [国/地域別IPアドレス割当数一覧](https://ipv4.fetus.jp)のプレーンテキストIPアドレスリストに対応。
* 表示・まとめて追加・まとめて削除が可能。
* 一部のみ追加・更新は不可。
* ゲームサーバーをGCPで建てる際に活用することを想定している。
    * GCPがファイアウォールを行ってくれるため、稼働サーバーに負荷をかけずに済む。
    * ゲームーサーバーに国ブロック機能がない場合に有効。

# スタートガイド

必要要件
| アプリケーション | バージョン                |
| :--------------- | :------------------------ |
| ASP.NET Core     | `3.1.201`                 |

* 前提条件
    * GCPのファイアウォールを操作できる権限を持つサービスアカウントを作成すること。
        * IAMで「Computeセキュリティ管理者」のロールを割り当てるとルール作成可能になる。
    * サービスアカウントから認証キーを発行すること。
        * 発行したキーは外部に漏れないよう厳重に保管すること！

リポジトリをクローンする。
```
git clone https://github.com/sevenspice/Firewalls.git
```

ディレクトリを移動する。
```
cd Firewalls/sources/BlockAsia
```

リストアとビルドを行う。
```
dotnet restore
dotnet build
```

ブロックするIPアドレスの一覧を取得する
```
wget https://ipv4.fetus.jp/krfilter.2.txt
```
* [krfilter](https://ipv4.fetus.jp/krfilter)より取得。
* ブロックする範囲は要件に合わせて適宜決めること。

Debugモードで実行する。
```
dotnet run -c Debug -- create -k <認証キーファイルパス> -p <GCPのプロジェクトID> -f <IPアドレス一覧ファイルパス> -r <ルールの接頭辞名>
```

or

Releaseモードで実行する。
```
dotnet run -c Release -- create -k <認証キーファイルパス> -p <GCPのプロジェクトID> -f <IPアドレス一覧ファイルパス> -r <ルールの接頭辞名>
```

追加されたかどうかを確認する。
```
dotnet run -- list -k <認証キーファイルパス> -p <GCPのプロジェクトID>
```

# 追加したルールを削除する

IPブロックを廃止する場合、あるいはIPアドレスリストを間違ってしまった時などは一括して削除する。

```
dotnet run -- list -k <認証キーファイルパス> -p <GCPのプロジェクトID> -r <ルールの接頭辞名> -c <追加されたルール数>
```

# 単一実行ファイルの生成

ASP.NET Core をインストールできない環境下で使用したい場合は以下のコマンドで実行ファイルを生成することができる。

Windows10 64bit Debug
```
# Debug
dotnet publish -c Debug -r win10-x64 -p:PublishSingleFile=true

# Release
dotnet publish -c Release -r win10-x64 -p:PublishSingleFile=true
```

Linux 64bit Debug
```
# Debug
dotnet publish -c Debug -r linux-x64 -p:PublishSingleFile=true

# Release
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true
```

# 免責事項

このアプリケーションを使用した時点で、発生したいかなる損害等には開発者は一切の責任を追わないことに同意したものとみなします。
